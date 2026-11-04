using DotNetNinja.AutoBoundConfiguration;
using DotNetNinja.Garage.Constants;
using DotNetNinja.Garage.Controllers;
using Auth0.AspNetCore.Authentication;
using DotNetNinja.Garage.Data;
using DotNetNinja.Garage.Data.Migrations;
using DotNetNinja.Garage.Services;
using Microsoft.EntityFrameworkCore;

namespace DotNetNinja.Garage.Configuration;

public static class StartUpExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, StartupConfiguration configuration)
    {
        services.AddAuthenticationConfiguration(configuration.Authentication);
        services.AddDatabaseContext<GarageDbContext>(configuration.Databases);
        services.AddScoped<IMigrationsManager<GarageDbContext>, MigrationsManager<GarageDbContext>>();
        services.AddScoped<IPageManager, PageManager>();
        services.AddScoped<IPageEditService, PageEditService>();

        return services;
    }

    public static StartupConfiguration AddAutoConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration
            .AddJsonFile(AppSettings.FileName, optional: false, reloadOnChange: true)
            .AddJsonFile(string.Format(AppSettings.EnvironmentFileName, builder.Environment.EnvironmentName), optional: true, reloadOnChange: true);

        if (builder.Environment.IsDevelopment())
        {
            builder.Configuration.AddUserSecrets<Settings>();
        }

        builder.Configuration.AddEnvironmentVariables();

        var provider = builder.Services.AddAutoBoundConfigurations(builder.Configuration).FromAssemblyOf<Settings>().Provider;

        var auth = provider.Get<AuthSettings>();
        var dbs = provider.Get<DatabaseSettings>();
        var start = new StartupConfiguration(auth, dbs);

        builder.Services.AddSingleton(start);

        return start;
    }

    public static IMvcBuilder WithContext(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.Services.AddScoped(typeof(IMvcContext<>), typeof(MvcContext<>));
        return mvcBuilder;
    }

    public static WebApplication UseErrorHandler(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler(AppSettings.ErrorRoute);
            app.UseHsts();
        }
        return app;
    }

    public static WebApplication UseDefaultMiddleware(this WebApplication app, bool withAuthentication)
    {
        app.UseHttpsRedirection();
        app.UseRouting();

        if (withAuthentication)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }

        return app;
    }

    public static WebApplication UseApplicationRouting(this WebApplication app)
    {
        app.MapStaticAssets();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.MapControllerRoute(
            name: "page",
            pattern: "{**slug}",
            defaults: new { controller = "Pages", action = "Index" });

        return app;
    }

    private static IServiceCollection AddAuthenticationConfiguration(this IServiceCollection services, AuthSettings auth)
    {
        auth.ThrowIfInvalid();
        services.AddAuth0WebAppAuthentication(options =>
        {
            options.Domain = auth.Domain;
            options.ClientId = auth.ClientId;
            options.ClientSecret = auth.ClientSecret;
            options.Scope = "openid profile email";
        });
        return services;
    }

    private static IServiceCollection AddDatabaseContext<TContext>(this IServiceCollection services, DatabaseSettings dbSettings) where TContext : DbContext
    {
        var contextName = typeof(TContext).Name;
        var dbConfig = dbSettings.SingleOrDefault(database => database.ContextName == contextName)
            ?? throw new InvalidOperationException($"Database configuration for context '{contextName}' is not provided.");
        if (string.IsNullOrWhiteSpace(dbConfig.ConnectionString))
        {
            throw new ArgumentException($"Connection string for context '{dbConfig.ContextName}' is not provided.");
        }
        services.AddDbContext<TContext>(options =>
            options.UseSqlServer(dbConfig.ConnectionString));
        return services;
    }
}
