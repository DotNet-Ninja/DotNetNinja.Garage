using DotNetNinja.Garage.Constants;
using DotNetNinja.Garage.Controllers;
using DotNetNinja.Garage.Data;
using DotNetNinja.Garage.Data.Migrations;
using DotNetNinja.Garage.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace DotNetNinja.Garage.Tests;

public class AccountDatabaseTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Database_displays_pending_status(bool hasPendingMigrations)
    {
        var pending = hasPendingMigrations ? new List<string> { "20261101220020_Initial" } : [];
        var migrations = new Mock<IMigrationsManager<GarageDbContext>>();
        migrations.Setup(manager => manager.GetPendingMigrationsAsync()).ReturnsAsync(pending);

        var result = await CreateController(migrations.Object).Database();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<DatabaseViewModel>(view.Model);
        Assert.Equal(hasPendingMigrations, model.HasPendingMigrations);
        Assert.Equal(pending, model.PendingMigrations);
    }

    [Fact]
    public async Task ApplyMigrations_runs_manager_and_redirects_to_status()
    {
        var migrations = new Mock<IMigrationsManager<GarageDbContext>>();

        var result = await CreateController(migrations.Object).ApplyMigrations();

        migrations.Verify(manager => manager.ApplyMigrationsAsync(), Times.Once);
        Assert.Equal(nameof(AccountController.Database), Assert.IsType<RedirectToActionResult>(result).ActionName);
    }

    [Theory]
    [InlineData(nameof(AccountController.Database))]
    [InlineData(nameof(AccountController.ApplyMigrations))]
    public void Database_actions_require_database_manager_role(string actionName)
    {
        var action = typeof(AccountController).GetMethod(actionName)!;
        var authorization = Assert.Single(action.GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>());
        Assert.Equal(Roles.DatabaseManager, authorization.Roles);
    }

    [Fact]
    public void ApplyMigrations_requires_antiforgery_token()
    {
        var action = typeof(AccountController).GetMethod(nameof(AccountController.ApplyMigrations))!;
        Assert.Single(action.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), false));
    }

    private static AccountController CreateController(IMigrationsManager<GarageDbContext> migrationsManager)
    {
        var mvcContext = new Mock<IMvcContext<AccountController>>();
        mvcContext.Setup(context => context.Logger).Returns(NullLogger<AccountController>.Instance);
        return new AccountController(mvcContext.Object, migrationsManager);
    }
}
