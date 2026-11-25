using System.Security.Cryptography;
using System.Text.Json;
using DotNetNinja.Garage.Data;
using DotNetNinja.Garage.Models;
using DotNetNinja.Garage.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace DotNetNinja.Garage.Controllers;

[Authorize]
[Route("Account/ImportPages")]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class PageImportsController(
    IPageManager pageManager,
    IPageEditService pageEdits,
    IDataProtectionProvider dataProtection) : Controller
{
    private const int MaxFileSize = 512 * 1024;
    private const int MaxRequestSize = 2 * 1024 * 1024;

    [HttpGet]
    public IActionResult Index() => View(new PageImportViewModel());

    [HttpPost("Preview"), ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxRequestSize)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestSize)]
    public async Task<IActionResult> Preview(IFormFile? file, CancellationToken cancellationToken)
    {
        var userId = pageManager.GetUserId(User);
        if (file is null || file.Length is <= 0 or > MaxFileSize)
            return UploadError("Choose a JSON export file up to 512 KB.");

        using var reader = new StreamReader(file.OpenReadStream());
        var json = await reader.ReadToEndAsync(cancellationToken);
        try
        {
            var pages = PageEditService.ReadImport(json);
            var token = CreateProtector(userId).Protect(json, TimeSpan.FromMinutes(30));
            return PreviewView(pages, token, Enumerable.Range(0, pages.Count));
        }
        catch (JsonException)
        {
            return UploadError("The file could not be read as a Garage JSON export.");
        }
        catch (EditValidationException exception)
        {
            return UploadError(exception.Message);
        }
    }

    [HttpPost("Import"), ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxRequestSize)]
    public async Task<IActionResult> Import(string? token, int[] selectedPages, CancellationToken cancellationToken)
    {
        var userId = pageManager.GetUserId(User);
        if (string.IsNullOrEmpty(token))
            return UploadError("Upload an export to preview its pages.");

        IReadOnlyList<LinkPage> pages;
        try
        {
            pages = PageEditService.ReadImport(CreateProtector(userId).Unprotect(token));
        }
        catch (Exception exception) when (exception is CryptographicException or FormatException or JsonException or EditValidationException)
        {
            return UploadError("This preview is invalid or has expired. Upload the file again.");
        }

        if (!ModelState.IsValid)
            return PreviewView(pages, token, selectedPages, "Choose valid pages from the preview.");

        try
        {
            var count = await pageEdits.ImportPagesAsync(userId, pages, selectedPages, cancellationToken);
            TempData["PageImportSuccess"] = count == 1 ? "Imported 1 page." : $"Imported {count} pages.";
            return RedirectToAction(nameof(Index));
        }
        catch (EditValidationException exception)
        {
            return PreviewView(pages, token, selectedPages, exception.Message);
        }
        catch (EditConflictException)
        {
            return PreviewView(pages, token, selectedPages, "Your pages changed during the import. Try importing again.");
        }
    }

    private ITimeLimitedDataProtector CreateProtector(string userId) =>
        dataProtection.CreateProtector("Garage.PageImports.v1", userId).ToTimeLimitedDataProtector();

    private ViewResult UploadError(string error) => View(nameof(Index), new PageImportViewModel { Error = error });

    private ViewResult PreviewView(IReadOnlyList<LinkPage> pages, string token, IEnumerable<int> selectedPages, string? error = null)
    {
        var selected = selectedPages.ToHashSet();
        return View(nameof(Index), new PageImportViewModel
        {
            Token = token,
            Error = error,
            Pages = pages.Select((page, index) => new PageImportOption(
                index, page.Text, page.Groups.Count, page.Groups.Sum(group => group.Links.Count), selected.Contains(index))).ToList()
        });
    }
}
