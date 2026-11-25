namespace DotNetNinja.Garage.Models;

public sealed class PageImportViewModel
{
    public string? Token { get; init; }
    public string? Error { get; init; }
    public IReadOnlyList<PageImportOption> Pages { get; init; } = [];
}

public sealed record PageImportOption(int Index, string Name, int GroupCount, int BookmarkCount, bool Selected);
