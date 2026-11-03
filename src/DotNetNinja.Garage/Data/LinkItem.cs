using DotNetNinja.Garage.Constants;

namespace DotNetNinja.Garage.Data;

public class LinkItem
{
    public int Id { get; set; }

    public int LinkGroupId { get; set; }

    public LinkGroup LinkGroup { get; set; } = null!;

    public string Href { get; set; } = string.Empty;

    public Icons Icon { get; set; }

    public string IconColor { get; set; } = string.Empty;

    public bool OpenInNewTab { get; set; } = true;

    public string Text { get; set; } = string.Empty;

    public int SortIndex { get; set; }
}
