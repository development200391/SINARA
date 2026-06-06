using ERP.Application.DTOs.Approval;
using ERP.Application.DTOs.Common;
using ERP.Domain.Enums.Approval;
using ERP.Web.ViewModels.Approval;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class ApprovalController
{
    [HttpGet("templates")]
    public async Task<IActionResult> Templates(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "code",
        string? sortDirection = "asc",
        string? code = null,
        string? name = null,
        string? module = null,
        string? referenceType = null,
        ApprovalType? approvalType = null,
        decimal? minAmountFrom = null,
        decimal? minAmountTo = null,
        decimal? maxAmountFrom = null,
        decimal? maxAmountTo = null,
        bool? isActive = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "code", "code", "name", "module", "referencetype", "approvaltype", "slahours", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedMinFrom, normalizedMinTo) = NormalizeDecimalRange(minAmountFrom, minAmountTo);
        var (normalizedMaxFrom, normalizedMaxTo) = NormalizeDecimalRange(maxAmountFrom, maxAmountTo);

        var items = await approvalApiClient.GetTemplatesAsync(accessToken, new ApprovalTemplatePagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = NormalizeText(code),
            Name = NormalizeText(name),
            Module = NormalizeText(module),
            ReferenceType = NormalizeText(referenceType),
            ApprovalType = approvalType,
            MinAmountFrom = normalizedMinFrom,
            MinAmountTo = normalizedMinTo,
            MaxAmountFrom = normalizedMaxFrom,
            MaxAmountTo = normalizedMaxTo,
            IsActive = isActive
        }, ct);

        ViewData["Title"] = "Approval Templates";
        ViewData["Breadcrumb"] = "Approval / Templates";

        return View("Templates/Index", new ApprovalTemplatesIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = NormalizeText(code),
            NameFilter = NormalizeText(name),
            ModuleFilter = NormalizeText(module),
            ReferenceTypeFilter = NormalizeText(referenceType),
            ApprovalTypeFilter = approvalType,
            MinAmountFromFilter = normalizedMinFrom,
            MinAmountToFilter = normalizedMinTo,
            MaxAmountFromFilter = normalizedMaxFrom,
            MaxAmountToFilter = normalizedMaxTo,
            IsActiveFilter = isActive,
            Items = items ?? PagedResult<ApprovalTemplateDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("templates/create")]
    public IActionResult CreateTemplate()
    {
        var unauthorized = RequireAccessToken(out _, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        ViewData["Title"] = "Create Approval Template";
        ViewData["Breadcrumb"] = "Approval / Templates / Create";

        return View("Templates/Create", new ApprovalTemplateEditViewModel());
    }

    [HttpPost("templates/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTemplate(ApprovalTemplateEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        ValidateTemplateForm(model, ModelState);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Approval Template";
            ViewData["Breadcrumb"] = "Approval / Templates / Create";
            return View("Templates/Create", model);
        }

        var result = await approvalApiClient.CreateTemplateAsync(accessToken, MapTemplateDto(model), ct);
        if (!result.IsSuccess)
        {
            AddApiModelError(result, "Failed to create template.");
            ViewData["Title"] = "Create Approval Template";
            ViewData["Breadcrumb"] = "Approval / Templates / Create";
            return View("Templates/Create", model);
        }

        TempData["SuccessMessage"] = "Template created.";
        return RedirectToAction(nameof(Templates));
    }

    [HttpGet("templates/edit/{id:int}")]
    public async Task<IActionResult> EditTemplate(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var template = await approvalApiClient.GetTemplateByIdAsync(accessToken, id, ct);
        if (template is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Edit Approval Template";
        ViewData["Breadcrumb"] = "Approval / Templates / Edit";

        return View("Templates/Edit", new ApprovalTemplateEditViewModel
        {
            Id = template.Id,
            Code = template.Code,
            Name = template.Name,
            Module = template.Module,
            ReferenceType = template.ReferenceType,
            ApprovalType = template.ApprovalType,
            MinAmount = template.MinAmount,
            MaxAmount = template.MaxAmount,
            AutoApproveBelow = template.AutoApproveBelow,
            SlaHours = template.SlaHours,
            AllowDelegation = template.AllowDelegation,
            RequireCommentOnReject = template.RequireCommentOnReject,
            IsActive = template.IsActive
        });
    }

    [HttpPost("templates/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTemplate(int id, ApprovalTemplateEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        ValidateTemplateForm(model, ModelState);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Approval Template";
            ViewData["Breadcrumb"] = "Approval / Templates / Edit";
            return View("Templates/Edit", model);
        }

        var result = await approvalApiClient.UpdateTemplateAsync(accessToken, id, MapTemplateDto(model), ct);
        if (!result.IsSuccess)
        {
            AddApiModelError(result, "Failed to update template.");
            ViewData["Title"] = "Edit Approval Template";
            ViewData["Breadcrumb"] = "Approval / Templates / Edit";
            return View("Templates/Edit", model);
        }

        TempData["SuccessMessage"] = "Template updated.";
        return RedirectToAction(nameof(Templates));
    }

    [HttpPost("templates/toggle-active/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleTemplateActive(int id, bool isActive, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var result = await approvalApiClient.SetTemplateActiveAsync(accessToken, id, isActive, ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? (isActive ? "Template activated." : "Template deactivated.")
            : ResolveApiErrorMessage(result, "Failed to update template status.");

        return RedirectToAction(nameof(Templates));
    }

    [HttpPost("templates/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTemplate(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var result = await approvalApiClient.DeleteTemplateAsync(accessToken, id, ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? "Template deleted."
            : ResolveApiErrorMessage(result, "Failed to delete template.");

        return RedirectToAction(nameof(Templates));
    }
}

