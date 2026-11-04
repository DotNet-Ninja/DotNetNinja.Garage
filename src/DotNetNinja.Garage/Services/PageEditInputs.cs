using DotNetNinja.Garage.Constants;

namespace DotNetNinja.Garage.Services;

public sealed class PageCreateInput
{
    public string? Text { get; set; }
}

public sealed class PageUpdateInput
{
    public int Id { get; set; }
    public int Revision { get; set; }
    public string? Text { get; set; }
    public string? Slug { get; set; }
}

public sealed class GroupCreateInput
{
    public int PageId { get; set; }
    public int PageRevision { get; set; }
    public string? Text { get; set; }
}

public sealed class GroupUpdateInput
{
    public int Id { get; set; }
    public int Revision { get; set; }
    public string? Text { get; set; }
}

public sealed class LinkCreateInput
{
    public int GroupId { get; set; }
    public int GroupRevision { get; set; }
    public string? Text { get; set; }
    public string? Href { get; set; }
    public Icons Icon { get; set; }
    public string? IconColor { get; set; }
    public bool OpenInNewTab { get; set; }
}

public sealed class LinkUpdateInput
{
    public int Id { get; set; }
    public int Revision { get; set; }
    public string? Text { get; set; }
    public string? Href { get; set; }
    public Icons Icon { get; set; }
    public string? IconColor { get; set; }
    public bool OpenInNewTab { get; set; }
}

public sealed class LinkMoveInput
{
    public int LinkId { get; set; }
    public int LinkRevision { get; set; }
    public int SourceGroupId { get; set; }
    public int SourceRevision { get; set; }
    public int DestinationGroupId { get; set; }
    public int DestinationRevision { get; set; }
    public int Position { get; set; }
}

public sealed class EditValidationException(string field, string message) : Exception(message)
{
    public string Field { get; } = field;
}

public sealed class EditConflictException : Exception
{
    public EditConflictException() : base("This page changed since you opened it. Reload and try again.") { }
}

public sealed class EditNotFoundException : Exception
{
    public EditNotFoundException() : base("The requested item was not found.") { }
}
