using System.Security.Claims;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Document;
using ERP.Web.Services;
using ERP.Web.ViewModels.Document;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers.Document;

[Authorize]
[Route("document/categories")]
public sealed class DocumentCategoriesController(IDocumentApiClient documentApiClient) : Controller
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

        var result = await documentApiClient.GetCategoriesAsync(accessToken, new PagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = "name",
            SortDirection = "asc"
        }, ct);

        ViewData["Title"] = "Document Categories";
        ViewData["Breadcrumb"] = "General Document / Categories";

        return View(new DocumentCategoriesIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            Categories = result ?? PagedResult<DocumentCategoryDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        ViewData["Title"] = "Create Document Category";
        ViewData["Breadcrumb"] = "General Document / Categories / Create";
        return View(new DocumentCategoryEditViewModel());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DocumentCategoryEditViewModel model, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Document Category";
            ViewData["Breadcrumb"] = "General Document / Categories / Create";
            return View(model);
        }

        var created = await documentApiClient.CreateCategoryAsync(accessToken, new DocumentCategoryDto
        {
            Code = model.Code,
            Name = model.Name,
            Module = model.Module,
            IsActive = model.IsActive
        }, ct);

        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to create document category." : created.ErrorMessage);
            ViewData["Title"] = "Create Document Category";
            ViewData["Breadcrumb"] = "General Document / Categories / Create";
            return View(model);
        }

        TempData["SuccessMessage"] = "Document category created.";
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

        var category = await documentApiClient.GetCategoryByIdAsync(accessToken, id, ct);
        if (category is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Edit Document Category";
        ViewData["Breadcrumb"] = "General Document / Categories / Edit";

        return View(new DocumentCategoryEditViewModel
        {
            Id = category.Id,
            Code = category.Code,
            Name = category.Name,
            Module = category.Module,
            IsActive = category.IsActive
        });
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DocumentCategoryEditViewModel model, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Document Category";
            ViewData["Breadcrumb"] = "General Document / Categories / Edit";
            return View(model);
        }

        var updated = await documentApiClient.UpdateCategoryAsync(accessToken, id, new DocumentCategoryDto
        {
            Id = id,
            Code = model.Code,
            Name = model.Name,
            Module = model.Module,
            IsActive = model.IsActive
        }, ct);

        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update document category." : updated.ErrorMessage);
            ViewData["Title"] = "Edit Document Category";
            ViewData["Breadcrumb"] = "General Document / Categories / Edit";
            return View(model);
        }

        TempData["SuccessMessage"] = "Document category updated.";
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

        var deleted = await documentApiClient.DeleteCategoryAsync(accessToken, id, ct);
        TempData[deleted.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = deleted.IsSuccess
            ? "Document category deleted."
            : (string.IsNullOrWhiteSpace(deleted.ErrorMessage) ? "Failed to delete document category." : deleted.ErrorMessage);

        return RedirectToAction(nameof(Index));
    }

    private static int NormalizePageSize(int pageSize) => pageSize is 20 or 50 or 100 ? pageSize : 20;

    private string? GetAccessToken() => User.FindFirstValue("access_token");
}
