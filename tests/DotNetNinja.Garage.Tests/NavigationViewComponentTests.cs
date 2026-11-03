using System.Security.Claims;
using DotNetNinja.Garage.Data;
using DotNetNinja.Garage.Services;
using DotNetNinja.Garage.ViewComponents;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace DotNetNinja.Garage.Tests;

public class NavigationViewComponentTests
{
    [Fact]
    public async Task Signed_out_visitor_does_not_query_private_navigation()
    {
        var pageManager = new Mock<IPageManager>();
        var component = CreateComponent(pageManager.Object, authenticated: false);

        var result = await component.InvokeAsync();

        Assert.IsType<ContentViewComponentResult>(result);
        pageManager.Verify(manager => manager.GetUserNavigationLinks(It.IsAny<ClaimsPrincipal>()), Times.Never);
    }

    [Fact]
    public async Task Signed_in_user_receives_their_navigation_links()
    {
        var links = new List<NavigationLink> { new() { Slug = "start", Text = "Start", SortIndex = 1 } };
        var pageManager = new Mock<IPageManager>();
        pageManager.Setup(manager => manager.GetUserNavigationLinks(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(links);
        var component = CreateComponent(pageManager.Object, authenticated: true);

        var result = await component.InvokeAsync();

        Assert.Same(links, Assert.IsType<ViewViewComponentResult>(result).ViewData!.Model);
        pageManager.Verify(manager => manager.GetUserNavigationLinks(
            It.Is<ClaimsPrincipal>(user => user.FindFirst(ClaimTypes.NameIdentifier)!.Value == "user-1")), Times.Once);
    }

    [Fact]
    public async Task No_pages_produces_no_navigation_menu()
    {
        var pageManager = new Mock<IPageManager>();
        pageManager.Setup(manager => manager.GetUserNavigationLinks(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync([]);

        var result = await CreateComponent(pageManager.Object, authenticated: true).InvokeAsync();

        Assert.IsType<ContentViewComponentResult>(result);
    }

    private static NavigationViewComponent CreateComponent(IPageManager pageManager, bool authenticated)
    {
        var identity = authenticated
            ? new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-1")], "test")
            : new ClaimsIdentity();
        return new NavigationViewComponent(pageManager, NullLogger<NavigationViewComponent>.Instance)
        {
            ViewComponentContext = new ViewComponentContext
            {
                ViewContext = new ViewContext
                {
                    HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
                }
            }
        };
    }
}
