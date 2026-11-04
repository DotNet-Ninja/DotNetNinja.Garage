namespace DotNetNinja.Garage.Data;

public class NavigationLink
{
    public string Slug { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int SortIndex { get; set; } = 0;
}