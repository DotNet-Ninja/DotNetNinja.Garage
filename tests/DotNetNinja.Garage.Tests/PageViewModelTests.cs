using DotNetNinja.Garage.Constants;
using DotNetNinja.Garage.Data;
using DotNetNinja.Garage.Models;

namespace DotNetNinja.Garage.Tests;

public class PageViewModelTests
{
    [Fact]
    public void Groups_and_links_are_sorted_and_unsafe_links_are_omitted()
    {
        var page = new LinkPage
        {
            Slug = "start",
            Text = "Start",
            Groups =
            [
                new LinkGroup { Id = 2, Text = "Second", SortIndex = 2 },
                new LinkGroup
                {
                    Id = 1,
                    Text = "First",
                    SortIndex = 1,
                    Links =
                    [
                        new LinkItem { Text = "Later", Href = "https://example.com/later", SortIndex = 2, IconColor = "#123ABC" },
                        new LinkItem { Text = "Unsafe", Href = "javascript:alert(1)", SortIndex = 0 },
                        new LinkItem { Text = "Earlier", Href = "http://example.com/earlier", SortIndex = 1, IconColor = "red" }
                    ]
                }
            ]
        };

        var model = PageViewModel.FromPage(page);

        Assert.Equal(["First", "Second"], model.Groups.Select(group => group.Title));
        Assert.Equal(["Earlier", "Later"], model.Groups[0].Links.Select(link => link.Title));
        Assert.Equal(Colors.White, model.Groups[0].Links[0].IconColor);
        Assert.Equal("#123ABC", model.Groups[0].Links[1].IconColor);
    }
}
