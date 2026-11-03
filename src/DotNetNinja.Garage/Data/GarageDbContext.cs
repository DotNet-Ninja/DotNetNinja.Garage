using Microsoft.EntityFrameworkCore;

namespace DotNetNinja.Garage.Data;

public class GarageDbContext: DbContext
{
    public GarageDbContext(DbContextOptions<GarageDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyTypeConfigurationsFromAssemblyOf<GarageDbContext>();
    }

    public DbSet<LinkPage> Pages => Set<LinkPage>();

    public DbSet<LinkGroup> Groups => Set<LinkGroup>();

    public DbSet<LinkItem> Items => Set<LinkItem>();
}