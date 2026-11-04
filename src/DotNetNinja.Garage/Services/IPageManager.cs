using System.Security.Principal;
using DotNetNinja.Garage.Data;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DotNetNinja.Garage.Services;

public interface IPageManager
{
    Task<LinkPage?> GetUserPageBySlugAsync(IPrincipal user, string slug);
    Task<LinkPage?> GetUserDefaultPage(IPrincipal user);
    Task<LinkPage> CreateUserDefaultPage(IPrincipal user);
    Task<List<NavigationLink>> GetUserNavigationLinks(IPrincipal user);
    string GetUserId(IPrincipal user);
}