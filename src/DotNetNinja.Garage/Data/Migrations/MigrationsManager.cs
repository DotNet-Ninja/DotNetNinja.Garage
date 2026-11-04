using Microsoft.EntityFrameworkCore;

namespace DotNetNinja.Garage.Data.Migrations;

public class MigrationsManager<TContext> : IMigrationsManager<TContext> where TContext : DbContext
{
    public MigrationsManager(TContext context)
    {
        Context = context;
    }

    public TContext Context { get; }

    public async Task ApplyMigrationsAsync()
    {
        await Context.Database.MigrateAsync();
    }

    public async Task<List<string>> GetPendingMigrationsAsync()
    {
        var pendingMigrations = await Context.Database.GetPendingMigrationsAsync();
        return pendingMigrations.ToList();
    }

    public async Task<bool> HasPendingMigrationsAsync()
    {
        var pendingMigrations = await GetPendingMigrationsAsync();
        return pendingMigrations.Any();
    }

}
