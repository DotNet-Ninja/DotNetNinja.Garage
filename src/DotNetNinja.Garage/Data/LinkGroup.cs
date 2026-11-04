namespace DotNetNinja.Garage.Data;

public class LinkGroup
{
    public int Id { get; set; }

    public int LinkPageId { get; set; }

    public LinkPage LinkPage { get; set; } = null!;

    public string Text { get; set; } = string.Empty;

    public int SortIndex { get; set; }

    public int Revision { get; set; }

    public ICollection<LinkItem> Links { get; set; } = new List<LinkItem>();
}
