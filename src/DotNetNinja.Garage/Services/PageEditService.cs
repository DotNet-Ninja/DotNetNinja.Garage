using System.Data;
using System.Text.RegularExpressions;
using DotNetNinja.Garage.Constants;
using DotNetNinja.Garage.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DotNetNinja.Garage.Services;

public sealed partial class PageEditService(GarageDbContext db) : IPageEditService
{
    private static readonly HashSet<string> ReservedSlugs = new(StringComparer.OrdinalIgnoreCase)
    {
        "account", "callback", "css", "error", "garage", "home", "images", "js", "lib", "pagedits", "pages"
    };

    public async Task<LinkPage> CreatePageAsync(string userId, PageCreateInput input, CancellationToken cancellationToken)
    {
        var text = RequireText(input.Text, 64, nameof(input.Text));
        var baseSlug = SlugCharacters().Replace(text.ToLowerInvariant(), "-").Trim('-');
        if (baseSlug.Length == 0) baseSlug = "page";
        if (ReservedSlugs.Contains(baseSlug)) baseSlug += "-page";
        baseSlug = baseSlug[..Math.Min(baseSlug.Length, 120)].TrimEnd('-');

        var existingSlugs = await db.Pages.Where(page => page.UserId == userId)
            .Select(page => page.Slug).ToListAsync(cancellationToken);
        var slug = baseSlug;
        for (var suffix = 2; existingSlugs.Contains(slug, StringComparer.OrdinalIgnoreCase); suffix++)
        {
            slug = $"{baseSlug}-{suffix}";
        }

        var nextIndex = await db.Pages.Where(page => page.UserId == userId)
            .Select(page => (int?)page.SortIndex).MaxAsync(cancellationToken) ?? 0;
        var page = new LinkPage { UserId = userId, Text = text, Slug = slug, SortIndex = nextIndex + 1 };
        db.Pages.Add(page);
        await SaveAsync(cancellationToken);
        return page;
    }

    public async Task<LinkPage> UpdatePageAsync(string userId, PageUpdateInput input, CancellationToken cancellationToken)
    {
        var page = await db.Pages.SingleOrDefaultAsync(page => page.Id == input.Id && page.UserId == userId, cancellationToken)
            ?? throw new EditNotFoundException();
        CheckRevision(page.Revision, input.Revision);
        var text = RequireText(input.Text, 64, nameof(input.Text));
        var slug = RequireSlug(input.Slug);
        if (await db.Pages.AnyAsync(other => other.UserId == userId && other.Slug == slug && other.Id != page.Id, cancellationToken))
        {
            throw new EditValidationException(nameof(input.Slug), "This URL slug is already in use.");
        }

        page.Text = text;
        page.Slug = slug;
        page.Revision++;
        await SaveAsync(cancellationToken);
        return page;
    }

    public async Task SetDefaultPageAsync(string userId, int pageId, int revision, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var pages = await db.Pages.Where(page => page.UserId == userId).ToListAsync(cancellationToken);
        var target = pages.SingleOrDefault(page => page.Id == pageId) ?? throw new EditNotFoundException();
        CheckRevision(target.Revision, revision);
        foreach (var page in pages.Where(page => page.IsDefault != (page.Id == pageId)))
        {
            page.IsDefault = page.Id == pageId;
            page.Revision++;
        }
        await SaveAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task MovePageAsync(string userId, int pageId, int revision, int direction, CancellationToken cancellationToken)
    {
        ValidateDirection(direction);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var pages = await db.Pages.Where(page => page.UserId == userId)
            .OrderBy(page => page.SortIndex).ThenBy(page => page.Id).ToListAsync(cancellationToken);
        var index = pages.FindIndex(page => page.Id == pageId);
        if (index < 0) throw new EditNotFoundException();
        CheckRevision(pages[index].Revision, revision);
        if (index + direction >= 0 && index + direction < pages.Count)
        {
            (pages[index], pages[index + direction]) = (pages[index + direction], pages[index]);
            NormalizePages(pages);
            await SaveAsync(cancellationToken);
        }
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<string> DeletePageAsync(string userId, int pageId, int revision, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var pages = await db.Pages.Where(page => page.UserId == userId)
            .OrderBy(page => page.SortIndex).ThenBy(page => page.Id).ToListAsync(cancellationToken);
        var target = pages.SingleOrDefault(page => page.Id == pageId) ?? throw new EditNotFoundException();
        CheckRevision(target.Revision, revision);
        if (pages.Count == 1) throw new EditValidationException("Page", "The last page cannot be deleted.");
        pages.Remove(target);
        if (target.IsDefault || pages.All(page => !page.IsDefault))
        {
            foreach (var page in pages)
            {
                if (page.IsDefault == (page == pages[0])) continue;
                page.IsDefault = page == pages[0];
                page.Revision++;
            }
        }
        NormalizePages(pages);
        db.Pages.Remove(target);
        await SaveAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return pages.First(page => page.IsDefault).Slug;
    }

    public async Task<LinkGroup> CreateGroupAsync(string userId, GroupCreateInput input, CancellationToken cancellationToken)
    {
        var text = RequireText(input.Text, 64, nameof(input.Text));
        var page = await db.Pages.Include(page => page.Groups)
            .SingleOrDefaultAsync(page => page.Id == input.PageId && page.UserId == userId, cancellationToken)
            ?? throw new EditNotFoundException();
        CheckRevision(page.Revision, input.PageRevision);
        var group = new LinkGroup
        {
            LinkPage = page,
            Text = text,
            SortIndex = page.Groups.Select(item => item.SortIndex).DefaultIfEmpty().Max() + 1
        };
        page.Revision++;
        db.Groups.Add(group);
        await SaveAsync(cancellationToken);
        return group;
    }

    public async Task<LinkGroup> UpdateGroupAsync(string userId, GroupUpdateInput input, CancellationToken cancellationToken)
    {
        var group = await OwnedGroup(userId).SingleOrDefaultAsync(group => group.Id == input.Id, cancellationToken)
            ?? throw new EditNotFoundException();
        CheckRevision(group.Revision, input.Revision);
        group.Text = RequireText(input.Text, 64, nameof(input.Text));
        group.Revision++;
        await SaveAsync(cancellationToken);
        return group;
    }

    public async Task MoveGroupAsync(string userId, int groupId, int revision, int direction, CancellationToken cancellationToken)
    {
        ValidateDirection(direction);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var page = await db.Pages.Include(page => page.Groups)
            .SingleOrDefaultAsync(page => page.UserId == userId && page.Groups.Any(group => group.Id == groupId), cancellationToken)
            ?? throw new EditNotFoundException();
        var groups = page.Groups.OrderBy(group => group.SortIndex).ThenBy(group => group.Id).ToList();
        var index = groups.FindIndex(group => group.Id == groupId);
        CheckRevision(groups[index].Revision, revision);
        if (index + direction >= 0 && index + direction < groups.Count)
        {
            (groups[index], groups[index + direction]) = (groups[index + direction], groups[index]);
            NormalizeGroups(groups);
            page.Revision++;
            await SaveAsync(cancellationToken);
        }
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task DeleteGroupAsync(string userId, int groupId, int revision, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var page = await db.Pages.Include(page => page.Groups)
            .SingleOrDefaultAsync(page => page.UserId == userId && page.Groups.Any(group => group.Id == groupId), cancellationToken)
            ?? throw new EditNotFoundException();
        var group = page.Groups.Single(group => group.Id == groupId);
        CheckRevision(group.Revision, revision);
        db.Groups.Remove(group);
        NormalizeGroups(page.Groups.Where(item => item.Id != groupId).OrderBy(item => item.SortIndex).ThenBy(item => item.Id).ToList());
        page.Revision++;
        await SaveAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<LinkItem> CreateLinkAsync(string userId, LinkCreateInput input, CancellationToken cancellationToken)
    {
        var group = await OwnedGroup(userId).Include(group => group.Links)
            .SingleOrDefaultAsync(group => group.Id == input.GroupId, cancellationToken)
            ?? throw new EditNotFoundException();
        CheckRevision(group.Revision, input.GroupRevision);
        var link = new LinkItem
        {
            LinkGroup = group,
            Text = RequireText(input.Text, 64, nameof(input.Text)),
            Href = RequireUrl(input.Href),
            Icon = RequireIcon(input.Icon),
            IconColor = RequireColor(input.IconColor),
            OpenInNewTab = input.OpenInNewTab,
            SortIndex = group.Links.Select(item => item.SortIndex).DefaultIfEmpty().Max() + 1
        };
        group.Revision++;
        db.Items.Add(link);
        await SaveAsync(cancellationToken);
        return link;
    }

    public async Task<LinkItem> UpdateLinkAsync(string userId, LinkUpdateInput input, CancellationToken cancellationToken)
    {
        var link = await OwnedLink(userId).SingleOrDefaultAsync(link => link.Id == input.Id, cancellationToken)
            ?? throw new EditNotFoundException();
        CheckRevision(link.Revision, input.Revision);
        link.Text = RequireText(input.Text, 64, nameof(input.Text));
        link.Href = RequireUrl(input.Href);
        link.Icon = RequireIcon(input.Icon);
        link.IconColor = RequireColor(input.IconColor);
        link.OpenInNewTab = input.OpenInNewTab;
        link.Revision++;
        await SaveAsync(cancellationToken);
        return link;
    }

    public async Task DeleteLinkAsync(string userId, int linkId, int revision, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var link = await OwnedLink(userId).SingleOrDefaultAsync(link => link.Id == linkId, cancellationToken)
            ?? throw new EditNotFoundException();
        CheckRevision(link.Revision, revision);
        var group = await OwnedGroup(userId).Include(group => group.Links)
            .SingleAsync(group => group.Id == link.LinkGroupId, cancellationToken);
        db.Items.Remove(link);
        NormalizeLinks(group.Links.Where(item => item.Id != linkId).OrderBy(item => item.SortIndex).ThenBy(item => item.Id).ToList());
        group.Revision++;
        await SaveAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task MoveLinkAsync(string userId, LinkMoveInput input, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var groups = await OwnedGroup(userId).Include(group => group.Links)
            .Where(group => group.Id == input.SourceGroupId || group.Id == input.DestinationGroupId)
            .ToListAsync(cancellationToken);
        var source = groups.SingleOrDefault(group => group.Id == input.SourceGroupId) ?? throw new EditNotFoundException();
        var destination = groups.SingleOrDefault(group => group.Id == input.DestinationGroupId) ?? throw new EditNotFoundException();
        if (source.LinkPageId != destination.LinkPageId) throw new EditNotFoundException();
        CheckRevision(source.Revision, input.SourceRevision);
        CheckRevision(destination.Revision, input.DestinationRevision);
        var link = source.Links.SingleOrDefault(link => link.Id == input.LinkId) ?? throw new EditNotFoundException();
        CheckRevision(link.Revision, input.LinkRevision);

        var sourceLinks = source.Links.OrderBy(item => item.SortIndex).ThenBy(item => item.Id).ToList();
        sourceLinks.Remove(link);
        var destinationLinks = source == destination
            ? sourceLinks
            : destination.Links.OrderBy(item => item.SortIndex).ThenBy(item => item.Id).ToList();
        var position = input.Position == -1 ? destinationLinks.Count : input.Position;
        if (position < 0 || position > destinationLinks.Count)
        {
            throw new EditValidationException(nameof(input.Position), "Choose a valid link position.");
        }
        destinationLinks.Insert(position, link);
        if (source == destination && destinationLinks.SequenceEqual(source.Links.OrderBy(item => item.SortIndex).ThenBy(item => item.Id)))
        {
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        if (source != destination)
        {
            link.LinkGroup = destination;
            link.LinkGroupId = destination.Id;
            link.Revision++;
        }
        NormalizeLinks(sourceLinks);
        if (source != destination) NormalizeLinks(destinationLinks);
        source.Revision++;
        if (source != destination) destination.Revision++;
        await SaveAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private IQueryable<LinkGroup> OwnedGroup(string userId) =>
        db.Groups.Include(group => group.LinkPage).Where(group => group.LinkPage.UserId == userId);

    private IQueryable<LinkItem> OwnedLink(string userId) =>
        db.Items.Include(link => link.LinkGroup).ThenInclude(group => group.LinkPage)
            .Where(link => link.LinkGroup.LinkPage.UserId == userId);

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new EditConflictException();
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new EditValidationException("Slug", "This URL slug is already in use.");
        }
    }

    private static string RequireText(string? value, int maxLength, string field)
    {
        var text = value?.Trim() ?? string.Empty;
        if (text.Length == 0 || text.Length > maxLength)
            throw new EditValidationException(field, $"Enter a name between 1 and {maxLength} characters.");
        return text;
    }

    private static string RequireSlug(string? value)
    {
        var slug = value?.Trim().ToLowerInvariant() ?? string.Empty;
        if (slug.Length is < 1 or > 128 || !SlugFormat().IsMatch(slug) || ReservedSlugs.Contains(slug))
            throw new EditValidationException("Slug", "Use up to 128 lowercase letters, numbers, and hyphens. This slug may be reserved.");
        return slug;
    }

    private static string RequireUrl(string? value)
    {
        var url = value?.Trim() ?? string.Empty;
        if (url.Length is < 1 or > 256 || !Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) || uri.Host.Length == 0)
            throw new EditValidationException("Href", "Enter an absolute HTTP or HTTPS URL of at most 256 characters.");
        return url;
    }

    private static Icons RequireIcon(Icons icon)
    {
        if (!Enum.IsDefined(icon)) throw new EditValidationException("Icon", "Choose a valid icon.");
        return icon;
    }

    private static string RequireColor(string? value)
    {
        var color = string.IsNullOrWhiteSpace(value) ? Colors.White : value.Trim();
        if (!HexColor().IsMatch(color))
            throw new EditValidationException("IconColor", "Choose a six-digit hex color such as #127CD6.");
        return color;
    }

    private static void CheckRevision(int actual, int expected)
    {
        if (actual != expected) throw new EditConflictException();
    }

    private static void ValidateDirection(int direction)
    {
        if (direction is not (-1 or 1)) throw new EditValidationException("Direction", "Choose a valid move direction.");
    }

    private static void NormalizePages(IReadOnlyList<LinkPage> pages)
    {
        for (var index = 0; index < pages.Count; index++)
        {
            if (pages[index].SortIndex == index + 1) continue;
            pages[index].SortIndex = index + 1;
            pages[index].Revision++;
        }
    }

    private static void NormalizeGroups(IReadOnlyList<LinkGroup> groups)
    {
        for (var index = 0; index < groups.Count; index++)
        {
            if (groups[index].SortIndex == index + 1) continue;
            groups[index].SortIndex = index + 1;
            groups[index].Revision++;
        }
    }

    private static void NormalizeLinks(IReadOnlyList<LinkItem> links)
    {
        for (var index = 0; index < links.Count; index++)
        {
            if (links[index].SortIndex == index + 1) continue;
            links[index].SortIndex = index + 1;
            links[index].Revision++;
        }
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex SlugCharacters();

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugFormat();

    [GeneratedRegex("^#[0-9A-Fa-f]{6}$")]
    private static partial Regex HexColor();
}
