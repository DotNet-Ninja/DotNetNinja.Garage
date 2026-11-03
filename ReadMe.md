# DotNetNinja.Garage
Home Page Application

The planned signed-in functionality is specified in [docs/product-spec.md](docs/product-spec.md), with a staged [implementation plan](docs/implementation-plan.md).

## Auth0 local setup

Garage uses Auth0 as a **Regular Web Application** with a server-side ASP.NET Core session cookie. Create a development application in your Auth0 tenant and configure:

| Auth0 application setting | Local value |
| --- | --- |
| Allowed Callback URLs | `https://localhost:7044/callback` |
| Allowed Logout URLs | `https://localhost:7044/` |
| Allowed Web Origins | `https://localhost:7044` |

Enable the login connection you intend to use for this application. Run the application's **https** launch profile. Keep the Auth0 client secret out of `appsettings.json` and source control. Set the three values with .NET user secrets from the repository root:

```powershell
dotnet user-secrets set "Auth0:Domain" "<your-tenant>.us.auth0.com" --project src/DotNetNinja.Garage
dotnet user-secrets set "Auth0:ClientId" "<development-regular-web-app-client-id>" --project src/DotNetNinja.Garage
dotnet user-secrets set "Auth0:ClientSecret" "<development-regular-web-app-client-secret>" --project src/DotNetNinja.Garage
```

Use the exact Domain shown in Auth0 without `https://` or a trailing slash. The application fails at startup with a clear message when these values are missing. For deployment, supply the same keys through a secret store or environment variables (`Auth0__Domain`, `Auth0__ClientId`, and `Auth0__ClientSecret`) and register the deployed HTTPS callback and logout URLs in a separate production Auth0 application.

The home login button starts Auth0 sign-in. After login, `/Garage` is protected and currently shows a placeholder for the planned personal start page. The navigation includes a POST-based logout action.

## Entity Framework Core migrations

Run these commands from the solution root (`D:\Code\DotNetNinja.Garage`). The web project contains `GarageDbContext` and is both the migration project and startup project. New migration files go in `src/DotNetNinja.Garage/Data/Migrations`. Configure the local Auth0 and database settings before running EF commands, since the startup project loads them at design time.

```powershell
# Install the EF CLI if it is not installed yet.
dotnet tool install --global dotnet-ef --version 10.0.14

# Update an existing global EF CLI installation to match the project's EF Core packages.
dotnet tool update --global dotnet-ef --version 10.0.14

# Create a migration; replace <MigrationName> with a descriptive name.
dotnet ef migrations add <MigrationName> --project src/DotNetNinja.Garage --startup-project src/DotNetNinja.Garage --context GarageDbContext --output-dir Data/Migrations

# Remove the most recent migration, provided it has not been applied to the database.
dotnet ef migrations remove --project src/DotNetNinja.Garage --startup-project src/DotNetNinja.Garage --context GarageDbContext

# Apply all pending migrations to the configured database.
dotnet ef database update --project src/DotNetNinja.Garage --startup-project src/DotNetNinja.Garage --context GarageDbContext

# Roll back to a named migration before removing a migration that was already applied.
dotnet ef database update <PreviousMigrationName> --project src/DotNetNinja.Garage --startup-project src/DotNetNinja.Garage --context GarageDbContext
```

Review generated migrations before applying them. `database update` changes the database selected by the current configuration; verify its connection string and back up important data before rolling back a migration.
