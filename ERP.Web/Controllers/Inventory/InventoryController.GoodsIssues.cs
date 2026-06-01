using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;
using ERP.Domain.Enums.Inventory;
using ERP.Web.ViewModels.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class InventoryController
{
    [HttpGet("goods-issues")]
    public async Task<IActionResult> GoodsIssues(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "issuedate",
        string? sortDirection = "desc",
        string? issueNo = null,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        int? warehouseId = null,
        int? departmentId = null,
        GoodsIssueType? issueType = null,
        TransactionStatus? status = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "issuedate", "issueno", "issuedate", "warehousecode", "issuetype", "status");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);

        var itemsTask = inventoryApiClient.GetGoodsIssuesAsync(accessToken, new GoodsIssuePagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            IssueNo = NormalizeText(issueNo),
            DateFrom = dateFrom,
            DateTo = dateTo,
            WarehouseId = warehouseId,
            DepartmentId = departmentId,
            IssueType = issueType,
            Status = status
        }, ct);

        var warehouseOptionsTask = inventoryApiClient.GetWarehouseOptionsAsync(accessToken, ct);
        var departmentOptionsTask = GetDepartmentOptionsAsync(accessToken, ct);

        await Task.WhenAll(itemsTask, warehouseOptionsTask, departmentOptionsTask);

        ViewData["Title"] = "Goods Issues";
        ViewData["Breadcrumb"] = "Inventory / Goods Issues";

        return View("GoodsIssues/Index", new InventoryGoodsIssuesIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            IssueNoFilter = NormalizeText(issueNo),
            DateFromFilter = dateFrom,
            DateToFilter = dateTo,
            WarehouseIdFilter = warehouseId,
            DepartmentIdFilter = departmentId,
            IssueTypeFilter = issueType,
            StatusFilter = status,
            WarehouseOptions = await warehouseOptionsTask,
            DepartmentOptions = await departmentOptionsTask,
            Items = await itemsTask ?? PagedResult<GoodsIssueDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("goods-issues/create")]
    public async Task<IActionResult> CreateGoodsIssue(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new InventoryGoodsIssueEditViewModel();
        await PopulateGoodsIssueOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create Goods Issue";
        ViewData["Breadcrumb"] = "Inventory / Goods Issues / Create";

        return View("GoodsIssues/Create", model);
    }

    [HttpPost("goods-issues/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateGoodsIssue(InventoryGoodsIssueEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        await PopulateGoodsIssueOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Goods Issue";
            ViewData["Breadcrumb"] = "Inventory / Goods Issues / Create";
            return View("GoodsIssues/Create", model);
        }

        var created = await inventoryApiClient.CreateGoodsIssueAsync(accessToken, MapGoodsIssueDto(model), ct);
        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create goods issue.");
            ViewData["Title"] = "Create Goods Issue";
            ViewData["Breadcrumb"] = "Inventory / Goods Issues / Create";
            return View("GoodsIssues/Create", model);
        }

        TempData["SuccessMessage"] = "Goods issue created.";
        return RedirectToAction(nameof(GoodsIssues));
    }

    [HttpGet("goods-issues/edit/{id:int}")]
    public async Task<IActionResult> EditGoodsIssue(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var dto = await inventoryApiClient.GetGoodsIssueByIdAsync(accessToken, id, ct);
        if (dto is null)
        {
            return NotFound();
        }

        var line = dto.Lines.FirstOrDefault();
        var model = new InventoryGoodsIssueEditViewModel
        {
            Id = dto.Id,
            IssueDate = dto.IssueDate,
            IssueType = dto.IssueType,
            WarehouseId = dto.WarehouseId,
            LocationId = dto.LocationId,
            DepartmentId = dto.DepartmentId,
            CostCenterId = dto.CostCenterId,
            ReferenceNo = dto.ReferenceNo,
            Description = dto.Description,
            Status = dto.Status,
            ItemId = line?.ItemId ?? 0,
            UomId = line?.UomId,
            QtyRequested = line?.QtyRequested ?? 1m,
            QtyIssued = line?.QtyIssued ?? 1m,
            QtyBase = line?.QtyBase ?? 1m,
            UnitCost = line?.UnitCost ?? 0m,
            LineNotes = line?.Notes
        };

        await PopulateGoodsIssueOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit Goods Issue";
        ViewData["Breadcrumb"] = "Inventory / Goods Issues / Edit";

        return View("GoodsIssues/Edit", model);
    }

    [HttpPost("goods-issues/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditGoodsIssue(int id, InventoryGoodsIssueEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        await PopulateGoodsIssueOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Goods Issue";
            ViewData["Breadcrumb"] = "Inventory / Goods Issues / Edit";
            return View("GoodsIssues/Edit", model);
        }

        var updated = await inventoryApiClient.UpdateGoodsIssueAsync(accessToken, id, MapGoodsIssueDto(model), ct);
        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to update goods issue.");
            ViewData["Title"] = "Edit Goods Issue";
            ViewData["Breadcrumb"] = "Inventory / Goods Issues / Edit";
            return View("GoodsIssues/Edit", model);
        }

        TempData["SuccessMessage"] = "Goods issue updated.";
        return RedirectToAction(nameof(GoodsIssues));
    }

    [HttpPost("goods-issues/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteGoodsIssue(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var deleted = await inventoryApiClient.DeleteGoodsIssueAsync(accessToken, id, ct);
        TempData[deleted ? "SuccessMessage" : "ErrorMessage"] = deleted ? "Goods issue deleted." : "Failed to delete goods issue.";
        return RedirectToAction(nameof(GoodsIssues));
    }

    [HttpPost("goods-issues/confirm/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmGoodsIssue(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await inventoryApiClient.ConfirmGoodsIssueAsync(accessToken, id, ct);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Goods issue confirmed." : "Failed to confirm goods issue.";
        return RedirectToAction(nameof(GoodsIssues));
    }

    [HttpPost("goods-issues/cancel/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelGoodsIssue(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await inventoryApiClient.CancelGoodsIssueAsync(accessToken, id, ct);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Goods issue cancelled." : "Failed to cancel goods issue.";
        return RedirectToAction(nameof(GoodsIssues));
    }

    private async Task PopulateGoodsIssueOptionsAsync(string accessToken, InventoryGoodsIssueEditViewModel model, CancellationToken ct)
    {
        var warehouseTask = inventoryApiClient.GetWarehouseOptionsAsync(accessToken, ct);
        var locationTask = GetWarehouseLocationOptionsAsync(accessToken, model.WarehouseId > 0 ? model.WarehouseId : null, ct);
        var departmentTask = GetDepartmentOptionsAsync(accessToken, ct);
        var costCenterTask = GetCostCenterOptionsAsync(accessToken, ct);
        var itemTask = inventoryApiClient.GetItemOptionsAsync(accessToken, ct);
        var uomTask = inventoryApiClient.GetUnitOptionsAsync(accessToken, ct);

        await Task.WhenAll(warehouseTask, locationTask, departmentTask, costCenterTask, itemTask, uomTask);

        model.WarehouseOptions = await warehouseTask;
        model.LocationOptions = await locationTask;
        model.DepartmentOptions = await departmentTask;
        model.CostCenterOptions = await costCenterTask;
        model.ItemOptions = await itemTask;
        model.UomOptions = await uomTask;
    }

    private static GoodsIssueDto MapGoodsIssueDto(InventoryGoodsIssueEditViewModel model)
    {
        return new GoodsIssueDto
        {
            Id = model.Id ?? 0,
            IssueDate = model.IssueDate,
            IssueType = model.IssueType,
            WarehouseId = model.WarehouseId,
            LocationId = model.LocationId,
            DepartmentId = model.DepartmentId,
            CostCenterId = model.CostCenterId,
            ReferenceNo = NormalizeText(model.ReferenceNo),
            Description = NormalizeText(model.Description),
            Lines =
            [
                new GoodsIssueLineDto
                {
                    LineNo = 1,
                    ItemId = model.ItemId,
                    UomId = model.UomId,
                    QtyRequested = model.QtyRequested,
                    QtyIssued = model.QtyIssued,
                    QtyBase = model.QtyBase,
                    UnitCost = model.UnitCost,
                    Notes = NormalizeText(model.LineNotes)
                }
            ]
        };
    }
}
