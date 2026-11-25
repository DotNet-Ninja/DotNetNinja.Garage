using System.Data;
using System.Text.Json;
using DotNetNinja.Garage.Constants;
using DotNetNinja.Garage.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DotNetNinja.Garage.Services;

public sealed partial class PageEditService
{
    private const int MaxImportPages = 100;
    private static readonly JsonSerializerOptions ImportJsonOptions = new(JsonSerializerDefaults.Web)
    {
        MaxDepth = 16
    };

    public static IReadOnlyList<LinkPage> ReadImport(string json)
    {
        var export = JsonSerializer.Deserialize<GarageExport>(json, ImportJsonOptions);
        if (export?.Pages is not { Count: > 0 and <= MaxImportPages } pages)
            throw new EditValidationException("File", $"The export must contain between 1 and {MaxImportPages} pages.");

        return pages.Select(ReadPage).OrderBy(page => page.SortIndex).ToList();
    }

    public async Task<int> ImportPagesAsync(string userId, IReadOnlyList<LinkPage> pages, int[] selectedPages, CancellationToken cancellationToken)
    {
        var selected = selectedPages.ToHashSet();
        if (selected.Count == 0 || selected.Any(index => index < 0 || index >= pages.Count))
            throw new EditValidationException("SelectedPages", "Choose at least one page from the preview.");

        try
        {
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            var existing = await db.Pages.Where(page => page.UserId == userId)
                .Select(page => new { page.Text, page.Slug, page.SortIndex }).ToListAsync(cancellationToken);
            var usedNames = existing.Select(page => page.Text).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var usedSlugs = existing.Select(page => page.Slug).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var sortIndex = existing.Select(page => page.SortIndex).DefaultIfEmpty().Max();

            foreach (var index in selected.Order())
            {
                var page = pages[index];
                page.UserId = userId;
                page.Text = AllocateImportName(page.Text, usedNames);
                page.Slug = AllocateSlug(page.Slug, usedSlugs);
                page.IsDefault = existing.Count == 0 && sortIndex == 0;
                page.SortIndex = ++sortIndex;
                db.Pages.Add(page);
            }

            await SaveAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return selected.Count;
        }
        catch (Exception exception) when (exception is SqlException { Number: 1205 }
            or DbUpdateException { InnerException: SqlException { Number: 1205 } })
        {
            throw new EditConflictException();
        }
    }

    private static string AllocateImportName(string original, HashSet<string> usedNames)
    {
        var name = original;
        for (var number = 1; !usedNames.Add(name); number++)
        {
            var suffix = number == 1 ? "-Import" : $"-{number}-Import";
            var prefixLength = Math.Min(original.Length, 64 - suffix.Length);
            if (char.IsHighSurrogate(original[prefixLength - 1])) prefixLength--;
            name = original[..prefixLength].TrimEnd() + suffix;
        }
        return name;
    }

    private static LinkPage ReadPage(ExportPage? source, int index)
    {
        try
        {
            if (source?.Groups is null)
                throw new EditValidationException("Groups", "The page must have a groups array.");

            var name = RequireText(source.Text, 64, "Text");
            var slug = source.Slug?.Trim().ToLowerInvariant() ?? string.Empty;
            return new LinkPage
            {
                Text = name,
                Slug = IsAvailableSlugFormat(slug) ? slug : CreateBaseSlug(name),
                SortIndex = source.SortIndex,
                Groups = source.Groups.Select(ReadGroup).OrderBy(group => group.SortIndex).ToList()
            };
        }
        catch (EditValidationException exception)
        {
            throw new EditValidationException("File", $"Page {index + 1} ({exception.Field}): {exception.Message}");
        }
    }

    private static LinkGroup ReadGroup(ExportGroup? source)
    {
        if (source?.Bookmarks is null)
            throw new EditValidationException("Bookmarks", "Each group must have a bookmarks array.");

        return new LinkGroup
        {
            Text = RequireText(source.Text, 64, "Group name"),
            SortIndex = source.SortIndex,
            Links = source.Bookmarks.Select(ReadBookmark).OrderBy(link => link.SortIndex).ToList()
        };
    }

    private static LinkItem ReadBookmark(ExportBookmark? source)
    {
        if (source is null)
            throw new EditValidationException("Bookmarks", "Each bookmark must be an object.");

        return new LinkItem
        {
            Text = RequireText(source.Text, 64, "Bookmark name"),
            Href = RequireUrl(source.Href),
            Icon = RequireIcon(source.Icon),
            IconColor = RequireColor(source.IconColor),
            OpenInNewTab = source.OpenInNewTab,
            SortIndex = source.SortIndex
        };
    }

    private sealed record GarageExport(List<ExportPage?>? Pages);
    private sealed record ExportPage(string? Text, string? Slug, int SortIndex, List<ExportGroup?>? Groups);
    private sealed record ExportGroup(string? Text, int SortIndex, List<ExportBookmark?>? Bookmarks);
    private sealed record ExportBookmark(string? Text, string? Href, Icons Icon, string? IconColor, bool OpenInNewTab, int SortIndex);
}
