namespace DotNetNinja.Garage.Data;

public class LinkPage
{
    public int Id { get; set; } = 0;

    public string UserId { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public bool IsDefault { get; set; } = false;

    public int SortIndex { get; set; } = 0;

    public int Revision { get; set; }

    public ICollection<LinkGroup> Groups { get; set; } = new List<LinkGroup>();
}
