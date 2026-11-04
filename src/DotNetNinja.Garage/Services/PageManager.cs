using System.Security.Authentication;
using System.Security.Principal;
using DotNetNinja.Garage.Configuration;
using DotNetNinja.Garage.Constants;
using DotNetNinja.Garage.Data;
using Microsoft.EntityFrameworkCore;

namespace DotNetNinja.Garage.Services;

public class PageManager : IPageManager
{
    private readonly GarageDbContext _db;

    public PageManager(GarageDbContext db)
    {
        _db = db;
    }

    public async Task<LinkPage?> GetUserPageBySlugAsync(IPrincipal user, string slug)
    {
        var userId = GetUserId(user);
        return await _db.Pages
            .Where(p => p.UserId == userId && p.Slug == slug)
            .Include(p => p.Groups)
            .ThenInclude(g => g.Links)
            .FirstOrDefaultAsync();
    }

    public async Task<LinkPage?> GetUserDefaultPage(IPrincipal user)
    {
        var userId = GetUserId(user);
        return await _db.Pages
            .Where(p => p.UserId == userId && p.IsDefault)
            .Include(p => p.Groups)
            .ThenInclude(g => g.Links)
            .FirstOrDefaultAsync();
    }

    public async Task<LinkPage> CreateUserDefaultPage(IPrincipal user)
    {
        var page = Defaults.CreateDefaultPage();
        page.UserId = GetUserId(user);
        _db.Pages.Add(page);
        await _db.SaveChangesAsync();
        return page;
    }

    public async Task<List<NavigationLink>> GetUserNavigationLinks(IPrincipal user)
    {
        var userId = GetUserId(user);
        return await _db.Pages
            .Where(p => p.UserId == userId)
            .Select(x=> new NavigationLink
                        {
                            Slug = x.Slug,
                            Text = x.Text,
                            SortIndex = x.SortIndex
                        })
            .OrderBy(x => x.SortIndex)
            .ToListAsync();
    }

    public string GetUserId(IPrincipal user)
    {
        if (user.Identity is null || !user.Identity.IsAuthenticated)
        {
            throw new AuthenticationException("User must be authenticated to get user ID.");
        }
        var userId = user.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new AuthenticationException("Authenticated user does not have a user ID.");
        }

        return userId;
    }
}
