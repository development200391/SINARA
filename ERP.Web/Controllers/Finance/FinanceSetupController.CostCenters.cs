using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Web.ViewModels.Finance;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class FinanceSetupController
{
    [HttpGet("cost-centers")]
    public async Task<IActionResult> CostCenters(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "code",
        string? sortDirection = "asc",
        string? code = null,
        string? name = null,
        int? departmentId = null,
        int? managerId = null,
        int? budgetAccountId = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "code", "code", "name", "departmentname", "managername", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedCode = NormalizeText(code);
        var normalizedName = NormalizeText(name);

        var itemsTask = financeApiClient.GetCostCentersAsync(accessToken, new CostCenterPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = normalizedCode,
            Name = normalizedName,
            DepartmentId = departmentId,
            ManagerId = managerId,
            BudgetAccountId = budgetAccountId,
            IsActive = isActive
        }, ct);

        var departmentOptionsTask = LoadDepartmentOptionsAsync(accessToken, ct);
        var managerOptionsTask = LoadManagerOptionsAsync(accessToken, ct);
        var budgetAccountOptionsTask = LoadAccountOptionsAsync(accessToken, ct);

        await Task.WhenAll(itemsTask, departmentOptionsTask, managerOptionsTask, budgetAccountOptionsTask);

        ViewData["Title"] = "Cost Centers";
        ViewData["Breadcrumb"] = "Finance / Cost Centers";

        return View("CostCenters/Index", new FinanceCostCentersIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = normalizedCode,
            NameFilter = normalizedName,
            DepartmentIdFilter = departmentId,
            ManagerIdFilter = managerId,
            BudgetAccountIdFilter = budgetAccountId,
            IsActiveFilter = isActive,
            DepartmentOptions = await departmentOptionsTask,
            ManagerOptions = await managerOptionsTask,
            BudgetAccountOptions = await budgetAccountOptionsTask,
            Items = await itemsTask ?? PagedResult<CostCenterDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("cost-centers/create")]
    public async Task<IActionResult> CreateCostCenter(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new FinanceCostCenterEditViewModel();
        await PopulateCostCenterFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create Cost Center";
        ViewData["Breadcrumb"] = "Finance / Cost Centers / Create";

        return View("CostCenters/Create", model);
    }

    [HttpPost("cost-centers/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCostCenter(FinanceCostCenterEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        await PopulateCostCenterFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Cost Center";
            ViewData["Breadcrumb"] = "Finance / Cost Centers / Create";
            return View("CostCenters/Create", model);
        }

        var created = await financeApiClient.CreateCostCenterAsync(accessToken, new CostCenterDto
        {
            Code = model.Code,
            Name = model.Name,
            DepartmentId = model.DepartmentId,
            ManagerId = model.ManagerId,
            BudgetAccountId = model.BudgetAccountId,
            IsActive = model.IsActive
        }, ct);

        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create cost center.");
            ViewData["Title"] = "Create Cost Center";
            ViewData["Breadcrumb"] = "Finance / Cost Centers / Create";
            return View("CostCenters/Create", model);
        }

        TempData["SuccessMessage"] = "Cost center created.";
        return RedirectToAction(nameof(CostCenters));
    }

    [HttpGet("cost-centers/edit/{id:int}")]
    public async Task<IActionResult> EditCostCenter(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await financeApiClient.GetCostCenterByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        var model = new FinanceCostCenterEditViewModel
        {
            Id = item.Id,
            Code = item.Code,
            Name = item.Name,
            DepartmentId = item.DepartmentId,
            ManagerId = item.ManagerId,
            BudgetAccountId = item.BudgetAccountId,
            IsActive = item.IsActive
        };

        await PopulateCostCenterFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit Cost Center";
        ViewData["Breadcrumb"] = "Finance / Cost Centers / Edit";

        return View("CostCenters/Edit", model);
    }

    [HttpPost("cost-centers/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCostCenter(int id, FinanceCostCenterEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        await PopulateCostCenterFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Cost Center";
            ViewData["Breadcrumb"] = "Finance / Cost Centers / Edit";
            return View("CostCenters/Edit", model);
        }

        var updated = await financeApiClient.UpdateCostCenterAsync(accessToken, id, new CostCenterDto
        {
            Id = id,
            Code = model.Code,
            Name = model.Name,
            DepartmentId = model.DepartmentId,
            ManagerId = model.ManagerId,
            BudgetAccountId = model.BudgetAccountId,
            IsActive = model.IsActive
        }, ct);

        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to update cost center.");
            ViewData["Title"] = "Edit Cost Center";
            ViewData["Breadcrumb"] = "Finance / Cost Centers / Edit";
            return View("CostCenters/Edit", model);
        }

        TempData["SuccessMessage"] = "Cost center updated.";
        return RedirectToAction(nameof(CostCenters));
    }

    [HttpPost("cost-centers/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCostCenter(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await financeApiClient.DeleteCostCenterAsync(accessToken, id, ct);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Cost center deleted." : "Failed to delete cost center.";

        return RedirectToAction(nameof(CostCenters));
    }
}
