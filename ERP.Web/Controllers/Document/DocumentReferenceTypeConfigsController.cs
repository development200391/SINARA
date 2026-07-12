using System.Security.Claims;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Document;
using ERP.Web.Services;
using ERP.Web.ViewModels.Document;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers.Document;

[Authorize]
[Route("document/reference-type-configs")]
public sealed class DocumentReferenceTypeConfigsController(IDocumentApiClient documentApiClient) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path + Request.QueryString });
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);

        var result = await documentApiClient.GetConfigsAsync(accessToken, new PagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = "displayName",
            SortDirection = "asc"
        }, ct);

        ViewData["Title"] = "Document Settings";
        ViewData["Breadcrumb"] = "General Document / Settings";

        return View(new DocumentReferenceTypeConfigsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            Configs = result ?? PagedResult<DocumentReferenceTypeConfigDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        ViewData["Title"] = "Create Document Setting";
        ViewData["Breadcrumb"] = "General Document / Settings / Create";
        return View(new DocumentReferenceTypeConfigEditViewModel
        {
            MaxFileCount = 1,
            Details = [new DocumentReferenceTypeConfigDetailViewModel()]
        });
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DocumentReferenceTypeConfigEditViewModel model, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Document Setting";
            ViewData["Breadcrumb"] = "General Document / Settings / Create";
            return View(model);
        }

        var created = await documentApiClient.CreateConfigAsync(accessToken, new DocumentReferenceTypeConfigDto
        {
            ReferenceType = model.ReferenceType,
            DisplayName = model.DisplayName,
            IsMultiple = model.IsMultiple,
            MaxFileCount = model.MaxFileCount,
            IsActive = model.IsActive,
            Details = MapDetails(model.Details)
        }, ct);

        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to create document setting." : created.ErrorMessage);
            ViewData["Title"] = "Create Document Setting";
            ViewData["Breadcrumb"] = "General Document / Settings / Create";
            return View(model);
        }

        TempData["SuccessMessage"] = "Document setting created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var config = await documentApiClient.GetConfigByIdAsync(accessToken, id, ct);
        if (config is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Edit Document Setting";
        ViewData["Breadcrumb"] = "General Document / Settings / Edit";

        return View(new DocumentReferenceTypeConfigEditViewModel
        {
            Id = config.Id,
            ReferenceType = config.ReferenceType,
            DisplayName = config.DisplayName,
            IsMultiple = config.IsMultiple,
            MaxFileCount = config.MaxFileCount,
            IsActive = config.IsActive,
            Details = config.Details.Count > 0
                ? config.Details.Select(d => new DocumentReferenceTypeConfigDetailViewModel
                {
                    Name = d.Name,
                    MaxFileSizeBytes = d.MaxFileSizeBytes,
                    IsRequired = d.IsRequired,
                    IsActive = d.IsActive,
                    AllowedExtensions = d.AllowedExtensions
                }).ToList()
                : [new DocumentReferenceTypeConfigDetailViewModel()]
        });
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DocumentReferenceTypeConfigEditViewModel model, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Document Setting";
            ViewData["Breadcrumb"] = "General Document / Settings / Edit";
            return View(model);
        }

        var updated = await documentApiClient.UpdateConfigAsync(accessToken, id, new DocumentReferenceTypeConfigDto
        {
            Id = id,
            ReferenceType = model.ReferenceType,
            DisplayName = model.DisplayName,
            IsMultiple = model.IsMultiple,
            MaxFileCount = model.MaxFileCount,
            IsActive = model.IsActive,
            Details = MapDetails(model.Details)
        }, ct);

        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update document setting." : updated.ErrorMessage);
            ViewData["Title"] = "Edit Document Setting";
            ViewData["Breadcrumb"] = "General Document / Settings / Edit";
            return View(model);
        }

        TempData["SuccessMessage"] = "Document setting updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var deleted = await documentApiClient.DeleteConfigAsync(accessToken, id, ct);
        TempData[deleted.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = deleted.IsSuccess
            ? "Document setting deleted."
            : (string.IsNullOrWhiteSpace(deleted.ErrorMessage) ? "Failed to delete document setting." : deleted.ErrorMessage);

        return RedirectToAction(nameof(Index));
    }

    private static List<DocumentReferenceTypeConfigDetailDto> MapDetails(List<DocumentReferenceTypeConfigDetailViewModel> details)
    {
        return details
            .Select(d => new DocumentReferenceTypeConfigDetailDto
            {
                Name = d.Name,
                MaxFileSizeBytes = d.MaxFileSizeBytes,
                IsRequired = d.IsRequired,
                IsActive = d.IsActive,
                AllowedExtensions = d.AllowedExtensions
            })
            .ToList();
    }

    private static int NormalizePageSize(int pageSize) => pageSize is 20 or 50 or 100 ? pageSize : 20;

    private string? GetAccessToken() => User.FindFirstValue("access_token");
}
