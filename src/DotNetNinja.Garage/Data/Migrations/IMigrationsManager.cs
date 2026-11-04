using Microsoft.EntityFrameworkCore;

namespace DotNetNinja.Garage.Data.Migrations;

public interface IMigrationsManager<TContext> where TContext : DbContext
{
    TContext Context { get; }
    Task ApplyMigrationsAsync();
    Task<List<string>> GetPendingMigrationsAsync();
    Task<bool> HasPendingMigrationsAsync();
}