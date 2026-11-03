using System.Security.Claims;
using Auth0.AspNetCore.Authentication;
using DotNetNinja.Garage.Configuration;
using DotNetNinja.Garage.Constants;
using DotNetNinja.Garage.Data;
using DotNetNinja.Garage.Data.Migrations;
using DotNetNinja.Garage.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotNetNinja.Garage.Controllers;

public class AccountController : MvcController<AccountController>
{
    private readonly IMigrationsManager<GarageDbContext> _migrationsManager;

    public AccountController(IMvcContext<AccountController> mvcContext, IMigrationsManager<GarageDbContext> migrationsManager) : base(mvcContext)
    {
        _migrationsManager = migrationsManager;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        var destination = Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Action("Index", "Garage")!;
        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(destination);
        }

        var properties = new LoginAuthenticationPropertiesBuilder()
            .WithRedirectUri(destination)
            .Build();
        return Challenge(properties, Auth0Constants.AuthenticationScheme);
    }

    [Authorize]
    [HttpGet]
    public IActionResult MyAccount()
    {
        var name = User.Identity?.Name ?? User.FindFirstValue("name") ?? "Not provided";
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email") ?? "Not provided";
        return View(new MyAccountViewModel(name, email));
    }

    [Authorize]
    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Claims()
    {
        var claims = User.Claims
            .Select(claim => new UserClaimViewModel(claim.Type, claim.Value, claim.Issuer))
            .OrderBy(claim => claim.Type, StringComparer.Ordinal)
            .ToList();

        return View(new AccountClaimsViewModel(claims));
    }

    [Authorize(Roles = Roles.DatabaseManager)]
    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Database()
    {
        var pendingMigrations = await _migrationsManager.GetPendingMigrationsAsync();
        return View(new DatabaseViewModel(pendingMigrations));
    }

    [Authorize(Roles = Roles.DatabaseManager)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyMigrations()
    {
        await _migrationsManager.ApplyMigrationsAsync();
        return RedirectToAction(nameof(Database));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task Logout()
    {
        var properties = new LogoutAuthenticationPropertiesBuilder()
            .WithRedirectUri(Url.Action("Index", "Home")!)
            .Build();

        await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, properties);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
