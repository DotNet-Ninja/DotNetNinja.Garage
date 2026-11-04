using System.Text.RegularExpressions;
using DotNetNinja.Garage.Constants;
using DotNetNinja.Garage.Data;

namespace DotNetNinja.Garage.Models;

public sealed partial record PageViewModel(string Slug, string Title, IReadOnlyList<PageGroupViewModel> Groups)
{
    public int Id { get; init; }
    public int Revision { get; init; }
    public bool IsDefault { get; init; }
    public bool IsEditing { get; init; }
    public bool CanDelete { get; init; }

    public static PageViewModel FromPage(LinkPage page, bool isEditing = false)
    {
        var groups = page.Groups
            .OrderBy(group => group.SortIndex)
            .ThenBy(group => group.Id)
            .Select(group => new PageGroupViewModel(
                group.Text,
                group.Links
                    .OrderBy(link => link.SortIndex)
                    .ThenBy(link => link.Id)
                    .Where(link => Uri.TryCreate(link.Href, UriKind.Absolute, out var uri)
                        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                    .Select(link => new PageLinkViewModel(
                        link.Text,
                        link.Href,
                        link.Icon,
                        HexColor().IsMatch(link.IconColor) ? link.IconColor : Colors.White,
                        link.OpenInNewTab)
                    {
                        Id = link.Id,
                        Revision = link.Revision,
                        SortIndex = link.SortIndex
                    })
                    .ToList())
            {
                Id = group.Id,
                Revision = group.Revision,
                SortIndex = group.SortIndex,
                TotalLinkCount = group.Links.Count
            })
            .ToList();

        return new PageViewModel(page.Slug, page.Text, groups)
        {
            Id = page.Id,
            Revision = page.Revision,
            IsDefault = page.IsDefault,
            IsEditing = isEditing
        };
    }

    [GeneratedRegex("^#[0-9A-Fa-f]{6}$")]
    private static partial Regex HexColor();
}

public sealed record PageGroupViewModel(string Title, IReadOnlyList<PageLinkViewModel> Links)
{
    public int Id { get; init; }
    public int Revision { get; init; }
    public int SortIndex { get; init; }
    public int TotalLinkCount { get; init; }
}

public sealed record PageLinkViewModel(string Title, string Href, Icons Icon, string IconColor, bool OpenInNewTab)
{
    public int Id { get; init; }
    public int Revision { get; init; }
    public int SortIndex { get; init; }
}
