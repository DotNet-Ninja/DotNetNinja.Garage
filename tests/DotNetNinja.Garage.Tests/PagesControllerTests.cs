using System.Security.Claims;
using DotNetNinja.Garage.Controllers;
using DotNetNinja.Garage.Data;
using DotNetNinja.Garage.Models;
using DotNetNinja.Garage.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace DotNetNinja.Garage.Tests;

public class PagesControllerTests
{
    [Fact]
    public async Task Existing_slug_renders_the_users_page()
    {
        var page = new LinkPage { Slug = "work", Text = "Work" };
        var manager = new Mock<IPageManager>();
        manager.Setup(service => service.GetUserPageBySlugAsync(It.IsAny<ClaimsPrincipal>(), "work"))
            .ReturnsAsync(page);

        var result = await CreateController(manager.Object).Index("work");

        var model = Assert.IsType<PageViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal("Work", model.Title);
        manager.Verify(service => service.GetUserDefaultPage(It.IsAny<ClaimsPrincipal>()), Times.Never);
    }

    [Fact]
    public async Task Edit_query_opens_edit_mode_on_the_current_page()
    {
        var manager = new Mock<IPageManager>();
        manager.Setup(service => service.GetUserPageBySlugAsync(It.IsAny<ClaimsPrincipal>(), "start"))
            .ReturnsAsync(new LinkPage { Slug = "start", Text = "Start" });

        var result = await CreateController(manager.Object).Index("start", "1");

        Assert.True(Assert.IsType<PageViewModel>(Assert.IsType<ViewResult>(result).Model).IsEditing);
    }

    [Fact]
    public async Task Unknown_slug_redirects_to_existing_default_page()
    {
        var manager = new Mock<IPageManager>();
        manager.Setup(service => service.GetUserDefaultPage(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(new LinkPage { Slug = "start" });

        var result = await CreateController(manager.Object).Index("unknown");

        var redirect = Assert.IsType<RedirectToRouteResult>(result);
        Assert.Equal("page", redirect.RouteName);
        Assert.Equal("start", redirect.RouteValues!["slug"]);
        manager.Verify(service => service.CreateUserDefaultPage(It.IsAny<ClaimsPrincipal>()), Times.Never);
    }

    [Fact]
    public async Task First_visit_creates_default_page_and_redirects_to_it()
    {
        var manager = new Mock<IPageManager>();
        manager.Setup(service => service.CreateUserDefaultPage(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(new LinkPage { Slug = "start" });

        var result = await CreateController(manager.Object).Index(null);

        var redirect = Assert.IsType<RedirectToRouteResult>(result);
        Assert.Equal("start", redirect.RouteValues!["slug"]);
        manager.Verify(service => service.GetUserPageBySlugAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>()), Times.Never);
        manager.Verify(service => service.CreateUserDefaultPage(It.IsAny<ClaimsPrincipal>()), Times.Once);
    }

    private static PagesController CreateController(IPageManager pageManager)
    {
        var mvcContext = new Mock<IMvcContext<PagesController>>();
        mvcContext.Setup(context => context.Logger).Returns(NullLogger<PagesController>.Instance);
        Mock.Get(pageManager).Setup(service => service.GetUserNavigationLinks(It.IsAny<ClaimsPrincipal>())).ReturnsAsync([]);
        return new PagesController(mvcContext.Object, pageManager)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        [new Claim(ClaimTypes.NameIdentifier, "user-1")], "test"))
                }
            }
        };
    }
}
