using DotNetNinja.Garage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace DotNetNinja.Garage.ViewComponents;

public class NavigationViewComponent : ViewComponent
{
    private readonly IPageManager _pageManager;
    private readonly ILogger<NavigationViewComponent> _logger;

    public NavigationViewComponent(IPageManager pageManager, ILogger<NavigationViewComponent> logger)
    {
        _pageManager = pageManager;
        _logger = logger;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (UserClaimsPrincipal.Identity?.IsAuthenticated != true)
        {
            return Content(string.Empty);
        }

        try
        {
            var links = await _pageManager.GetUserNavigationLinks(UserClaimsPrincipal);
            return links.Count == 0 ? Content(string.Empty) : View(links);
        }
        catch (SqlException exception) when (exception.Number == 208)
        {
            _logger.LogWarning("Page navigation is unavailable because the Pages table has not been created yet.");
            return Content(string.Empty);
        }
    }
}
