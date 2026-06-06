using ERP.Application.DTOs.Approval;
using ERP.Application.DTOs.Common;
using ERP.Web.ViewModels.Approval;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class ApprovalController
{
    [HttpGet("delegations")]
    public async Task<IActionResult> Delegations(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "startdate",
        string? sortDirection = "desc",
        int? delegatorUserId = null,
        int? delegateUserId = null,
        int? templateId = null,
        DateOnly? effectiveDateFrom = null,
        DateOnly? effectiveDateTo = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "startdate", "delegatorname", "delegatename", "templatecode", "startdate", "enddate", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedDateFrom, normalizedDateTo) = NormalizeDateRange(effectiveDateFrom, effectiveDateTo);

        var usersTask = approvalApiClient.GetApproverOptionsAsync(accessToken, ct);
        var templatesTask = approvalApiClient.GetTemplateOptionsAsync(accessToken, ct);
        var itemsTask = approvalApiClient.GetDelegationsAsync(accessToken, new ApprovalDelegationPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            DelegatorUserId = delegatorUserId,
            DelegateUserId = delegateUserId,
            TemplateId = templateId,
            EffectiveDateFrom = normalizedDateFrom,
            EffectiveDateTo = normalizedDateTo,
            IsActive = isActive
        }, ct);

        await Task.WhenAll(usersTask, templatesTask, itemsTask);

        ViewData["Title"] = "Delegations";
        ViewData["Breadcrumb"] = "Approval / Delegations";

        return View("Delegations/Index", new ApprovalDelegationsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            DelegatorUserIdFilter = delegatorUserId,
            DelegateUserIdFilter = delegateUserId,
            TemplateIdFilter = templateId,
            EffectiveDateFromFilter = normalizedDateFrom,
            EffectiveDateToFilter = normalizedDateTo,
            IsActiveFilter = isActive,
            UserOptions = await usersTask,
            TemplateOptions = await templatesTask,
            Items = await itemsTask ?? PagedResult<ApprovalDelegationDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("delegations/create")]
    public async Task<IActionResult> CreateDelegation(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new ApprovalDelegationEditViewModel();
        await PopulateDelegationFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create Delegation";
        ViewData["Breadcrumb"] = "Approval / Delegations / Create";

        return View("Delegations/Create", model);
    }

    [HttpPost("delegations/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDelegation(ApprovalDelegationEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        await PopulateDelegationFormOptionsAsync(accessToken, model, ct);
        ValidateDelegationForm(model, ModelState);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Delegation";
            ViewData["Breadcrumb"] = "Approval / Delegations / Create";
            return View("Delegations/Create", model);
        }

        var result = await approvalApiClient.CreateDelegationAsync(accessToken, MapDelegationDto(model), ct);
        if (!result.IsSuccess)
        {
            AddApiModelError(result, "Failed to create delegation.");
            ViewData["Title"] = "Create Delegation";
            ViewData["Breadcrumb"] = "Approval / Delegations / Create";
            return View("Delegations/Create", model);
        }

        TempData["SuccessMessage"] = "Delegation created.";
        return RedirectToAction(nameof(Delegations));
    }

    [HttpGet("delegations/edit/{id:int}")]
    public async Task<IActionResult> EditDelegation(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var delegation = await approvalApiClient.GetDelegationByIdAsync(accessToken, id, ct);
        if (delegation is null)
        {
            return NotFound();
        }

        var model = new ApprovalDelegationEditViewModel
        {
            Id = delegation.Id,
            DelegatorUserId = delegation.DelegatorUserId,
            DelegateUserId = delegation.DelegateUserId,
            TemplateId = delegation.TemplateId,
            StartDate = delegation.StartDate,
            EndDate = delegation.EndDate,
            Reason = delegation.Reason,
            IsActive = delegation.IsActive
        };

        await PopulateDelegationFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit Delegation";
        ViewData["Breadcrumb"] = "Approval / Delegations / Edit";

        return View("Delegations/Edit", model);
    }

    [HttpPost("delegations/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDelegation(int id, ApprovalDelegationEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        await PopulateDelegationFormOptionsAsync(accessToken, model, ct);
        ValidateDelegationForm(model, ModelState);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Delegation";
            ViewData["Breadcrumb"] = "Approval / Delegations / Edit";
            return View("Delegations/Edit", model);
        }

        var result = await approvalApiClient.UpdateDelegationAsync(accessToken, id, MapDelegationDto(model), ct);
        if (!result.IsSuccess)
        {
            AddApiModelError(result, "Failed to update delegation.");
            ViewData["Title"] = "Edit Delegation";
            ViewData["Breadcrumb"] = "Approval / Delegations / Edit";
            return View("Delegations/Edit", model);
        }

        TempData["SuccessMessage"] = "Delegation updated.";
        return RedirectToAction(nameof(Delegations));
    }

    [HttpPost("delegations/revoke/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeDelegation(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var result = await approvalApiClient.RevokeDelegationAsync(accessToken, id, ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? "Delegation revoked."
            : ResolveApiErrorMessage(result, "Failed to revoke delegation.");

        return RedirectToAction(nameof(Delegations));
    }
}
