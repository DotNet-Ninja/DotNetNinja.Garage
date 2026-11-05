using System.Security.Claims;
using DotNetNinja.Garage.Controllers;
using DotNetNinja.Garage.Data;
using DotNetNinja.Garage.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace DotNetNinja.Garage.Tests;

public class PageEditsControllerTests
{
    [Fact]
    public async Task Updating_a_page_uses_the_authenticated_owner_and_redirects_to_new_slug()
    {
        var edits = new Mock<IPageEditService>();
        edits.Setup(service => service.UpdatePageAsync("auth0|user-1", It.IsAny<PageUpdateInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LinkPage { Slug = "new-slug" });
        var controller = CreateController(edits.Object);

        var result = await controller.UpdatePage(new PageUpdateInput { Id = 7, Revision = 1, Text = "New", Slug = "new-slug" }, "old-slug", CancellationToken.None);

        Assert.Equal("new-slug", Assert.IsType<RedirectToRouteResult>(result).RouteValues!["slug"]);
        edits.Verify(service => service.UpdatePageAsync("auth0|user-1", It.Is<PageUpdateInput>(input => input.Id == 7), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Stale_drag_returns_conflict_without_claiming_success()
    {
        var edits = new Mock<IPageEditService>();
        edits.Setup(service => service.MoveLinkAsync("auth0|user-1", It.IsAny<LinkMoveInput>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EditConflictException());

        var result = await CreateController(edits.Object).MoveLinkJson(new LinkMoveInput(), CancellationToken.None);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Group_drag_uses_the_authenticated_owner_and_requested_position()
    {
        var edits = new Mock<IPageEditService>();
        var result = await CreateController(edits.Object).ReorderGroupJson(7, 2, 0, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        edits.Verify(service => service.ReorderGroupAsync("auth0|user-1", 7, 2, 0, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Stale_group_drag_returns_conflict()
    {
        var edits = new Mock<IPageEditService>();
        edits.Setup(service => service.ReorderGroupAsync("auth0|user-1", 7, 2, 0, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EditConflictException());

        var result = await CreateController(edits.Object).ReorderGroupJson(7, 2, 0, CancellationToken.None);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public void Every_edit_action_requires_post_and_antiforgery()
    {
        Assert.Single(typeof(PageEditsController).GetCustomAttributes(typeof(AuthorizeAttribute), false));
        foreach (var action in typeof(PageEditsController).GetMethods()
                     .Where(method => method.DeclaringType == typeof(PageEditsController) && method.IsPublic))
        {
            Assert.Single(action.GetCustomAttributes(typeof(HttpPostAttribute), false));
            Assert.Single(action.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), false));
        }
    }

    private static PageEditsController CreateController(IPageEditService editService)
    {
        var mvcContext = new Mock<IMvcContext<PageEditsController>>();
        mvcContext.Setup(context => context.Logger).Returns(NullLogger<PageEditsController>.Instance);
        var pages = new Mock<IPageManager>();
        pages.Setup(service => service.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("auth0|user-1");
        var tempData = new Mock<ITempDataDictionary>();
        return new PageEditsController(mvcContext.Object, pages.Object, editService)
        {
            TempData = tempData.Object,
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        [new Claim(ClaimTypes.NameIdentifier, "auth0|user-1")], "test"))
                }
            }
        };
    }
}
