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
    public async Task<IActionResult> Index(string? slug)
    {
        if (!string.IsNullOrWhiteSpace(slug))
        {
            var page = await _pageManager.GetUserPageBySlugAsync(User, slug);
            if (page is not null)
            {
                return View(PageViewModel.FromPage(page));
            }
        }

        var defaultPage = await _pageManager.GetUserDefaultPage(User)
            ?? await _pageManager.CreateUserDefaultPage(User);

        return RedirectToRoute("page", new { slug = defaultPage.Slug });
    }
}
