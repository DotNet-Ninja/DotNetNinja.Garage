using DotNetNinja.Garage.Models;
using DotNetNinja.Garage.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotNetNinja.Garage.Controllers;

[Authorize]
public class PagesController : MvcController<PagesController>
{
    private readonly IPageManager _pageManager;

    public PagesController(IMvcContext<PagesController> mvcContext, IPageManager pageManager) : base(mvcContext)
    {
        _pageManager = pageManager;
    }

    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Index(string? slug, string? edit = null)
    {
        if (!string.IsNullOrWhiteSpace(slug))
        {
            var page = await _pageManager.GetUserPageBySlugAsync(User, slug);
            if (page is not null)
            {
                var navigation = await _pageManager.GetUserNavigationLinks(User);
                return View(PageViewModel.FromPage(page, edit is "1" or "true") with
                {
                    CanDelete = navigation.Count > 1
                });
            }
        }

        var defaultPage = await _pageManager.GetUserDefaultPage(User)
            ?? await _pageManager.CreateUserDefaultPage(User);

        return RedirectToRoute("page", new { slug = defaultPage.Slug });
    }
}
