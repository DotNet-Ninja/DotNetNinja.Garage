using System.Security.Claims;
using System.Security.Principal;

namespace DotNetNinja.Garage.Configuration;

public static class IUserExtensions
{
    public static string? GetUserName(this IPrincipal user)
    {
        return user.Identity?.Name;
    }

    public static Claim? GetClaim(this IPrincipal user, string claimType)
    {
        if (user.Identity is ClaimsIdentity claimsIdentity)
        {
            return claimsIdentity.FindFirst(claimType);
        }
        return null;
    }

    public static string GetUserId(this IPrincipal user)
    {
        return user.GetClaim(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }
}