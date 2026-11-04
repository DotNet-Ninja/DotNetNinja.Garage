namespace DotNetNinja.Garage.Models;

public sealed record DatabaseViewModel(IReadOnlyList<string> PendingMigrations)
{
    public bool HasPendingMigrations => PendingMigrations.Count > 0;
}
