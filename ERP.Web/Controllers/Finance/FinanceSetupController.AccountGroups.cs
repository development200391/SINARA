using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Enums;
using ERP.Web.ViewModels.Finance;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class FinanceSetupController
{
    [HttpGet("coa/groups")]
    public async Task<IActionResult> CoaGroups(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "sortOrder",
        string? sortDirection = "asc",
        string? code = null,
        string? name = null,
        FinanceAccountType? type = null,
        FinanceNormalBalance? normalBalance = null,
        int? parentGroupId = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "sortorder", "code", "name", "type", "normalbalance", "sortorder", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedCode = NormalizeText(code);
        var normalizedName = NormalizeText(name);

        var itemsTask = financeApiClient.GetAccountGroupsAsync(accessToken, new AccountGroupPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = normalizedCode,
            Name = normalizedName,
            Type = type,
            NormalBalance = normalBalance,
            ParentGroupId = parentGroupId,
            IsActive = isActive
        }, ct);

        var parentGroupOptionsTask = LoadAccountGroupOptionsAsync(accessToken, ct);
        await Task.WhenAll(itemsTask, parentGroupOptionsTask);

        ViewData["Title"] = "Account Groups";
        ViewData["Breadcrumb"] = "Finance / Account Groups";

        return View("AccountGroups/Index", new FinanceAccountGroupsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = normalizedCode,
            NameFilter = normalizedName,
            TypeFilter = type,
            NormalBalanceFilter = normalBalance,
            ParentGroupIdFilter = parentGroupId,
            IsActiveFilter = isActive,
            ParentGroupOptions = await parentGroupOptionsTask,
            Items = await itemsTask ?? PagedResult<AccountGroupDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("coa/groups/create")]
    public async Task<IActionResult> CreateCoaGroup(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new FinanceAccountGroupEditViewModel();
        await PopulateAccountGroupFormOptionsAsync(accessToken, model, null, ct);

        ViewData["Title"] = "Create Account Group";
        ViewData["Breadcrumb"] = "Finance / Account Groups / Create";

        return View("AccountGroups/Create", model);
    }

    [HttpPost("coa/groups/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCoaGroup(FinanceAccountGroupEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        await PopulateAccountGroupFormOptionsAsync(accessToken, model, null, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Account Group";
            ViewData["Breadcrumb"] = "Finance / Account Groups / Create";
            return View("AccountGroups/Create", model);
        }

        var created = await financeApiClient.CreateAccountGroupAsync(accessToken, new AccountGroupDto
        {
            Name = model.Name,
            Code = model.Code,
            Type = model.Type,
            NormalBalance = model.NormalBalance,
            ParentGroupId = model.ParentGroupId,
            SortOrder = model.SortOrder,
            IsActive = model.IsActive
        }, ct);

        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create account group.");
            ViewData["Title"] = "Create Account Group";
            ViewData["Breadcrumb"] = "Finance / Account Groups / Create";
            return View("AccountGroups/Create", model);
        }

        TempData["SuccessMessage"] = "Account group created.";
        return RedirectToAction(nameof(CoaGroups));
    }

    [HttpGet("coa/groups/edit/{id:int}")]
    public async Task<IActionResult> EditCoaGroup(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await financeApiClient.GetAccountGroupByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        var model = new FinanceAccountGroupEditViewModel
        {
            Id = item.Id,
            Name = item.Name,
            Code = item.Code,
            Type = item.Type,
            NormalBalance = item.NormalBalance,
            ParentGroupId = item.ParentGroupId,
            SortOrder = item.SortOrder,
            IsActive = item.IsActive
        };

        await PopulateAccountGroupFormOptionsAsync(accessToken, model, id, ct);

        ViewData["Title"] = "Edit Account Group";
        ViewData["Breadcrumb"] = "Finance / Account Groups / Edit";

        return View("AccountGroups/Edit", model);
    }

    [HttpPost("coa/groups/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCoaGroup(int id, FinanceAccountGroupEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        await PopulateAccountGroupFormOptionsAsync(accessToken, model, id, ct);

        if (model.ParentGroupId == id)
        {
            ModelState.AddModelError(nameof(model.ParentGroupId), "Parent group is invalid.");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Account Group";
            ViewData["Breadcrumb"] = "Finance / Account Groups / Edit";
            return View("AccountGroups/Edit", model);
        }

        var updated = await financeApiClient.UpdateAccountGroupAsync(accessToken, id, new AccountGroupDto
        {
            Id = id,
            Name = model.Name,
            Code = model.Code,
            Type = model.Type,
            NormalBalance = model.NormalBalance,
            ParentGroupId = model.ParentGroupId,
            SortOrder = model.SortOrder,
            IsActive = model.IsActive
        }, ct);

        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to update account group.");
            ViewData["Title"] = "Edit Account Group";
            ViewData["Breadcrumb"] = "Finance / Account Groups / Edit";
            return View("AccountGroups/Edit", model);
        }

        TempData["SuccessMessage"] = "Account group updated.";
        return RedirectToAction(nameof(CoaGroups));
    }

    [HttpPost("coa/groups/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCoaGroup(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await financeApiClient.DeleteAccountGroupAsync(accessToken, id, ct);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Account group deleted." : "Failed to delete account group.";

        return RedirectToAction(nameof(CoaGroups));
    }
}
