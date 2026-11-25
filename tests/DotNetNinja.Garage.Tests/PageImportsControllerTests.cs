using System.Text;
using DotNetNinja.Garage.Constants;
using DotNetNinja.Garage.Controllers;
using DotNetNinja.Garage.Data;
using DotNetNinja.Garage.Models;
using DotNetNinja.Garage.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace DotNetNinja.Garage.Tests;

public class PageImportsControllerTests
{
    private static readonly string LongUrl = "https://example.com/" + new string('a', 500);
    private static readonly string Export = $$"""
        {
          "defaultPage": "legacy-id",
          "pages": [
            {
              "id": "legacy-id",
              "text": "Later",
              "slug": "Later",
              "sortIndex": 2,
              "groups": [
                {
                  "text": "Resources",
                  "bookmarks": [
                    { "text": "Example", "href": "{{LongUrl}}", "icon": 2, "iconColor": "#ABC123", "openInNewTab": true }
                  ]
                }
              ]
            },
            { "text": "Earlier", "sortIndex": 1, "groups": [] }
          ]
        }
        """;

    [Fact]
    public async Task Preview_shows_pages_in_order_and_import_submits_the_chosen_index()
    {
        var edits = new Mock<IPageEditService>();
        edits.Setup(service => service.ImportPagesAsync("user-1", It.IsAny<IReadOnlyList<LinkPage>>(), It.IsAny<int[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        var controller = CreateController("user-1", edits.Object, new EphemeralDataProtectionProvider());
        using var file = new MemoryStream(Encoding.UTF8.GetBytes(Export));

        var preview = GetModel(await controller.Preview(CreateFile(file), CancellationToken.None));

        Assert.Collection(preview.Pages,
            option => Assert.Equal(new PageImportOption(0, "Earlier", 0, 0, true), option),
            option => Assert.Equal(new PageImportOption(1, "Later", 1, 1, true), option));
        Assert.IsType<RedirectToActionResult>(await controller.Import(preview.Token, [1], CancellationToken.None));
        edits.Verify(service => service.ImportPagesAsync("user-1",
            It.Is<IReadOnlyList<LinkPage>>(pages => pages[1].Slug == "later"),
            It.Is<int[]>(selection => selection.Length == 1 && selection[0] == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Another_account_cannot_submit_the_preview()
    {
        var provider = new EphemeralDataProtectionProvider();
        var edits = new Mock<IPageEditService>(MockBehavior.Strict);
        using var file = new MemoryStream(Encoding.UTF8.GetBytes(Export));
        var preview = GetModel(await CreateController("user-1", edits.Object, provider).Preview(CreateFile(file), CancellationToken.None));

        var result = GetModel(await CreateController("user-2", edits.Object, provider).Import(preview.Token, [0], CancellationToken.None));

        Assert.Null(result.Token);
        Assert.NotNull(result.Error);
        edits.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("{")]
    [InlineData("{\"pages\":[]}")]
    [InlineData("{\"pages\":[{\"text\":\"Missing groups\"}]}")]
    public async Task Invalid_export_returns_an_upload_error(string json)
    {
        var controller = CreateController("user-1", Mock.Of<IPageEditService>(), new EphemeralDataProtectionProvider());
        using var file = new MemoryStream(Encoding.UTF8.GetBytes(json));

        var preview = GetModel(await controller.Preview(CreateFile(file), CancellationToken.None));

        Assert.NotNull(preview.Error);
        Assert.Null(preview.Token);
        Assert.Empty(preview.Pages);
    }

    [Fact]
    public void Parser_preserves_bookmark_fields_and_rejects_unsafe_urls()
    {
        var pages = PageEditService.ReadImport(Export);
        var link = Assert.Single(Assert.Single(pages[1].Groups).Links);

        Assert.Equal(LongUrl, link.Href);
        Assert.Equal(Icons.AlarmFill, link.Icon);
        Assert.Equal("#ABC123", link.IconColor);
        Assert.True(link.OpenInNewTab);
        Assert.Equal(0, pages[1].Id);
        Assert.False(pages[1].IsDefault);
        Assert.Throws<EditValidationException>(() => PageEditService.ReadImport(
            Export.Replace(LongUrl, "javascript:alert(1)", StringComparison.Ordinal)));
    }

    private static FormFile CreateFile(MemoryStream stream) =>
        new(stream, 0, stream.Length, "file", "garage.json");

    private static PageImportViewModel GetModel(IActionResult result) =>
        Assert.IsType<PageImportViewModel>(Assert.IsType<ViewResult>(result).Model);

    private static PageImportsController CreateController(string userId, IPageEditService edits, IDataProtectionProvider provider)
    {
        var pageManager = new Mock<IPageManager>();
        pageManager.Setup(manager => manager.GetUserId(It.IsAny<System.Security.Principal.IPrincipal>())).Returns(userId);
        return new PageImportsController(pageManager.Object, edits, provider)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
            TempData = Mock.Of<ITempDataDictionary>()
        };
    }
}
