using DotNetNinja.Garage.Models;
using DotNetNinja.Garage.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotNetNinja.Garage.Controllers;

[Authorize]
public sealed class PageEditsController : MvcController<PageEditsController>
{
    private readonly IPageManager _pageManager;
    private readonly IPageEditService _editService;

    public PageEditsController(
        IMvcContext<PageEditsController> mvcContext,
        IPageManager pageManager,
        IPageEditService editService) : base(mvcContext)
    {
        _pageManager = pageManager;
        _editService = editService;
    }

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> CreatePage(PageCreateInput input, string returnSlug, CancellationToken cancellationToken) =>
        ExecuteAsync(returnSlug, "page-create", async () =>
        {
            EnsureModelState();
            var page = await _editService.CreatePageAsync(UserId, input, cancellationToken);
            return EditRedirect(page.Slug);
        });

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> UpdatePage(PageUpdateInput input, string returnSlug, CancellationToken cancellationToken) =>
        ExecuteAsync(returnSlug, "page-update", async () =>
        {
            EnsureModelState();
            var page = await _editService.UpdatePageAsync(UserId, input, cancellationToken);
            return EditRedirect(page.Slug);
        });

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> SetDefaultPage(int id, int revision, string returnSlug, CancellationToken cancellationToken) =>
        ExecuteAsync(returnSlug, "page-settings", async () =>
        {
            await _editService.SetDefaultPageAsync(UserId, id, revision, cancellationToken);
            return EditRedirect(returnSlug);
        });

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> MovePage(int id, int revision, int direction, string returnSlug, CancellationToken cancellationToken) =>
        ExecuteAsync(returnSlug, "page-settings", async () =>
        {
            await _editService.MovePageAsync(UserId, id, revision, direction, cancellationToken);
            return EditRedirect(returnSlug);
        });

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> DeletePage(int id, int revision, string returnSlug, CancellationToken cancellationToken) =>
        ExecuteAsync(returnSlug, "page-delete", async () =>
        {
            var nextSlug = await _editService.DeletePageAsync(UserId, id, revision, cancellationToken);
            return EditRedirect(nextSlug);
        });

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> CreateGroup(GroupCreateInput input, string returnSlug, CancellationToken cancellationToken) =>
        ExecuteAsync(returnSlug, "group-create", async () =>
        {
            EnsureModelState();
            var group = await _editService.CreateGroupAsync(UserId, input, cancellationToken);
            return EditRedirect(group.LinkPage.Slug);
        });

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> UpdateGroup(GroupUpdateInput input, string returnSlug, CancellationToken cancellationToken) =>
        ExecuteAsync(returnSlug, $"group-update-{input.Id}", async () =>
        {
            EnsureModelState();
            var group = await _editService.UpdateGroupAsync(UserId, input, cancellationToken);
            return EditRedirect(group.LinkPage.Slug);
        });

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> MoveGroup(int id, int revision, int direction, string returnSlug, CancellationToken cancellationToken) =>
        ExecuteAsync(returnSlug, $"group-update-{id}", async () =>
        {
            await _editService.MoveGroupAsync(UserId, id, revision, direction, cancellationToken);
            return EditRedirect(returnSlug);
        });

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> DeleteGroup(int id, int revision, string returnSlug, CancellationToken cancellationToken) =>
        ExecuteAsync(returnSlug, $"group-delete-{id}", async () =>
        {
            await _editService.DeleteGroupAsync(UserId, id, revision, cancellationToken);
            return EditRedirect(returnSlug);
        });

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> CreateLink(LinkCreateInput input, string returnSlug, CancellationToken cancellationToken) =>
        ExecuteAsync(returnSlug, $"link-create-{input.GroupId}", async () =>
        {
            EnsureModelState();
            var link = await _editService.CreateLinkAsync(UserId, input, cancellationToken);
            return EditRedirect(link.LinkGroup.LinkPage.Slug);
        });

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> UpdateLink(LinkUpdateInput input, string returnSlug, CancellationToken cancellationToken) =>
        ExecuteAsync(returnSlug, $"link-update-{input.Id}", async () =>
        {
            EnsureModelState();
            var link = await _editService.UpdateLinkAsync(UserId, input, cancellationToken);
            return EditRedirect(link.LinkGroup.LinkPage.Slug);
        });

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> DeleteLink(int id, int revision, string returnSlug, CancellationToken cancellationToken) =>
        ExecuteAsync(returnSlug, $"link-delete-{id}", async () =>
        {
            await _editService.DeleteLinkAsync(UserId, id, revision, cancellationToken);
            return EditRedirect(returnSlug);
        });

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> MoveLink(LinkMoveInput input, string returnSlug, CancellationToken cancellationToken) =>
        ExecuteAsync(returnSlug, $"link-move-{input.LinkId}", async () =>
        {
            await _editService.MoveLinkAsync(UserId, input, cancellationToken);
            return EditRedirect(returnSlug);
        });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveLinkJson(LinkMoveInput input, CancellationToken cancellationToken)
    {
        try
        {
            await _editService.MoveLinkAsync(UserId, input, cancellationToken);
            return NoContent();
        }
        catch (EditValidationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (EditConflictException exception)
        {
            return Conflict(new { message = exception.Message });
        }
        catch (EditNotFoundException)
        {
            return NotFound();
        }
    }

    private string UserId => _pageManager.GetUserId(User);

    private RedirectToRouteResult EditRedirect(string slug)
    {
        TempData["EditSuccess"] = "Saved.";
        return RedirectToRoute("page", new { slug, edit = 1 });
    }

    private async Task<IActionResult> ExecuteAsync(string returnSlug, string formKey, Func<Task<IActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (EditValidationException exception)
        {
            return await RenderFailureAsync(returnSlug, formKey, exception.Field, exception.Message);
        }
        catch (EditConflictException exception)
        {
            return await RenderFailureAsync(returnSlug, formKey, string.Empty, exception.Message);
        }
        catch (EditNotFoundException)
        {
            return NotFound();
        }
    }

    private async Task<IActionResult> RenderFailureAsync(string slug, string formKey, string field, string message)
    {
        var page = await _pageManager.GetUserPageBySlugAsync(User, slug);
        if (page is null) return NotFound();
        ViewData["FailedForm"] = formKey;
        ViewData["ErrorField"] = field;
        ViewData["EditError"] = message;
        Response.StatusCode = field.Length == 0 ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest;
        var navigation = await _pageManager.GetUserNavigationLinks(User);
        return View("~/Views/Pages/Index.cshtml", PageViewModel.FromPage(page, true) with
        {
            CanDelete = navigation.Count > 1
        });
    }

    private void EnsureModelState()
    {
        if (!ModelState.IsValid)
        {
            var field = ModelState.First(entry => entry.Value?.Errors.Count > 0).Key;
            throw new EditValidationException(field, "Check the highlighted field and try again.");
        }
    }
}
