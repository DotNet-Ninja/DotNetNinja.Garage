using System.Text.RegularExpressions;
using DotNetNinja.Garage.Constants;
using DotNetNinja.Garage.Data;

namespace DotNetNinja.Garage.Models;

public sealed partial record PageViewModel(string Slug, string Title, IReadOnlyList<PageGroupViewModel> Groups)
{
    public static PageViewModel FromPage(LinkPage page)
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
                        link.OpenInNewTab))
                    .ToList()))
            .ToList();

        return new PageViewModel(page.Slug, page.Text, groups);
    }

    [GeneratedRegex("^#[0-9A-Fa-f]{6}$")]
    private static partial Regex HexColor();
}

public sealed record PageGroupViewModel(string Title, IReadOnlyList<PageLinkViewModel> Links);

public sealed record PageLinkViewModel(string Title, string Href, Icons Icon, string IconColor, bool OpenInNewTab);
