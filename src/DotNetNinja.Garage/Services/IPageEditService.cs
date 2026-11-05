using DotNetNinja.Garage.Data;

namespace DotNetNinja.Garage.Services;

public interface IPageEditService
{
    Task<LinkPage> CreatePageAsync(string userId, PageCreateInput input, CancellationToken cancellationToken);
    Task<LinkPage> UpdatePageAsync(string userId, PageUpdateInput input, CancellationToken cancellationToken);
    Task SetDefaultPageAsync(string userId, int pageId, int revision, CancellationToken cancellationToken);
    Task MovePageAsync(string userId, int pageId, int revision, int direction, CancellationToken cancellationToken);
    Task<string> DeletePageAsync(string userId, int pageId, int revision, CancellationToken cancellationToken);
    Task<LinkGroup> CreateGroupAsync(string userId, GroupCreateInput input, CancellationToken cancellationToken);
    Task<LinkGroup> UpdateGroupAsync(string userId, GroupUpdateInput input, CancellationToken cancellationToken);
    Task MoveGroupAsync(string userId, int groupId, int revision, int direction, CancellationToken cancellationToken);
    Task ReorderGroupAsync(string userId, int groupId, int revision, int position, CancellationToken cancellationToken);
    Task DeleteGroupAsync(string userId, int groupId, int revision, CancellationToken cancellationToken);
    Task<LinkItem> CreateLinkAsync(string userId, LinkCreateInput input, CancellationToken cancellationToken);
    Task<LinkItem> UpdateLinkAsync(string userId, LinkUpdateInput input, CancellationToken cancellationToken);
    Task DeleteLinkAsync(string userId, int linkId, int revision, CancellationToken cancellationToken);
    Task MoveLinkAsync(string userId, LinkMoveInput input, CancellationToken cancellationToken);
}
