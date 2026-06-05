using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;
using ERP.Domain.Enums.Inventory;
using ERP.Web.ViewModels.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class InventoryController
{
    [HttpGet("adjustments")]
    public async Task<IActionResult> Adjustments(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "adjustmentdate",
        string? sortDirection = "desc",
        string? adjustmentNo = null,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        int? warehouseId = null,
        AdjustmentReason? reason = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "adjustmentdate", "adjustmentno", "adjustmentdate", "warehousecode", "reason", "status");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);

        var itemsTask = inventoryApiClient.GetAdjustmentsAsync(accessToken, new StockAdjustmentPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            AdjustmentNo = NormalizeText(adjustmentNo),
            DateFrom = dateFrom,
            DateTo = dateTo,
            WarehouseId = warehouseId,
            Reason = reason,
            Status = status
        }, ct);

        var warehouseOptionsTask = inventoryApiClient.GetWarehouseOptionsAsync(accessToken, ct);

        await Task.WhenAll(itemsTask, warehouseOptionsTask);

        ViewData["Title"] = "Stock Adjustments";
        ViewData["Breadcrumb"] = "Inventory / Stock Adjustments";

        return View("Adjustments/Index", new InventoryAdjustmentsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            AdjustmentNoFilter = NormalizeText(adjustmentNo),
            DateFromFilter = dateFrom,
            DateToFilter = dateTo,
            WarehouseIdFilter = warehouseId,
            ReasonFilter = reason,
            StatusFilter = status,
            WarehouseOptions = await warehouseOptionsTask,
            Items = await itemsTask ?? PagedResult<StockAdjustmentDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("adjustments/create")]
    public async Task<IActionResult> CreateAdjustment(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new InventoryAdjustmentEditViewModel();
        await PopulateAdjustmentOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create Adjustment";
        ViewData["Breadcrumb"] = "Inventory / Stock Adjustments / Create";

        return View("Adjustments/Create", model);
    }

    [HttpPost("adjustments/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAdjustment(InventoryAdjustmentEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        await PopulateAdjustmentOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Adjustment";
            ViewData["Breadcrumb"] = "Inventory / Stock Adjustments / Create";
            return View("Adjustments/Create", model);
        }

        var created = await inventoryApiClient.CreateAdjustmentAsync(accessToken, MapAdjustmentDto(model), ct);
        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to create adjustment." : created.ErrorMessage);
            ViewData["Title"] = "Create Adjustment";
            ViewData["Breadcrumb"] = "Inventory / Stock Adjustments / Create";
            return View("Adjustments/Create", model);
        }

        TempData["SuccessMessage"] = "Adjustment created.";
        return RedirectToAction(nameof(Adjustments));
    }

    [HttpGet("adjustments/edit/{id:int}")]
    public async Task<IActionResult> EditAdjustment(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var dto = await inventoryApiClient.GetAdjustmentByIdAsync(accessToken, id, ct);
        if (dto is null)
        {
            return NotFound();
        }

        var line = dto.Lines.FirstOrDefault();
        var model = new InventoryAdjustmentEditViewModel
        {
            Id = dto.Id,
            AdjustmentDate = dto.AdjustmentDate,
            WarehouseId = dto.WarehouseId,
            LocationId = dto.LocationId,
            Reason = dto.Reason,
            ReferenceNo = dto.ReferenceNo,
            Description = dto.Description,
            Status = dto.Status,
            ItemId = line?.ItemId ?? 0,
            UomId = line?.UomId,
            QtyAdjustment = line?.QtyAdjustment ?? 0m,
            UnitCost = line?.UnitCost ?? 0m,
            LineNotes = line?.Notes
        };

        await PopulateAdjustmentOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit Adjustment";
        ViewData["Breadcrumb"] = "Inventory / Stock Adjustments / Edit";

        return View("Adjustments/Edit", model);
    }

    [HttpPost("adjustments/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAdjustment(int id, InventoryAdjustmentEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        await PopulateAdjustmentOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Adjustment";
            ViewData["Breadcrumb"] = "Inventory / Stock Adjustments / Edit";
            return View("Adjustments/Edit", model);
        }

        var updated = await inventoryApiClient.UpdateAdjustmentAsync(accessToken, id, MapAdjustmentDto(model), ct);
        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update adjustment." : updated.ErrorMessage);
            ViewData["Title"] = "Edit Adjustment";
            ViewData["Breadcrumb"] = "Inventory / Stock Adjustments / Edit";
            return View("Adjustments/Edit", model);
        }

        TempData["SuccessMessage"] = "Adjustment updated.";
        return RedirectToAction(nameof(Adjustments));
    }

    [HttpPost("adjustments/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAdjustment(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var deleted = await inventoryApiClient.DeleteAdjustmentAsync(accessToken, id, ct);
        TempData[deleted.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = deleted.IsSuccess ? "Adjustment deleted." : (string.IsNullOrWhiteSpace(deleted.ErrorMessage) ? "Failed to delete adjustment." : deleted.ErrorMessage);
        return RedirectToAction(nameof(Adjustments));
    }

    [HttpPost("adjustments/approve/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveAdjustment(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await inventoryApiClient.ApproveAdjustmentAsync(accessToken, id, ct);
        TempData[ok.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = ok.IsSuccess ? "Adjustment approved." : (string.IsNullOrWhiteSpace(ok.ErrorMessage) ? "Failed to approve adjustment." : ok.ErrorMessage);
        return RedirectToAction(nameof(Adjustments));
    }

    [HttpPost("adjustments/confirm/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmAdjustment(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await inventoryApiClient.ConfirmAdjustmentAsync(accessToken, id, ct);
        TempData[ok.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = ok.IsSuccess ? "Adjustment confirmed." : (string.IsNullOrWhiteSpace(ok.ErrorMessage) ? "Failed to confirm adjustment." : ok.ErrorMessage);
        return RedirectToAction(nameof(Adjustments));
    }

    [HttpPost("adjustments/cancel/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelAdjustment(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await inventoryApiClient.CancelAdjustmentAsync(accessToken, id, ct);
        TempData[ok.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = ok.IsSuccess ? "Adjustment cancelled." : (string.IsNullOrWhiteSpace(ok.ErrorMessage) ? "Failed to cancel adjustment." : ok.ErrorMessage);
        return RedirectToAction(nameof(Adjustments));
    }

    private async Task PopulateAdjustmentOptionsAsync(string accessToken, InventoryAdjustmentEditViewModel model, CancellationToken ct)
    {
        var warehouseTask = inventoryApiClient.GetWarehouseOptionsAsync(accessToken, ct);
        var locationTask = GetWarehouseLocationOptionsAsync(accessToken, model.WarehouseId > 0 ? model.WarehouseId : null, ct);
        var itemTask = inventoryApiClient.GetItemOptionsAsync(accessToken, ct);
        var uomTask = inventoryApiClient.GetUnitOptionsAsync(accessToken, ct);

        await Task.WhenAll(warehouseTask, locationTask, itemTask, uomTask);

        model.WarehouseOptions = await warehouseTask;
        model.LocationOptions = await locationTask;
        model.ItemOptions = await itemTask;
        model.UomOptions = await uomTask;
    }

    private static StockAdjustmentDto MapAdjustmentDto(InventoryAdjustmentEditViewModel model)
    {
        return new StockAdjustmentDto
        {
            Id = model.Id ?? 0,
            AdjustmentDate = model.AdjustmentDate,
            WarehouseId = model.WarehouseId,
            LocationId = model.LocationId,
            Reason = model.Reason,
            ReferenceNo = NormalizeText(model.ReferenceNo),
            Description = NormalizeText(model.Description),
            Lines =
            [
                new StockAdjustmentLineDto
                {
                    LineNo = 1,
                    ItemId = model.ItemId,
                    UomId = model.UomId,
                    QtyAdjustment = model.QtyAdjustment,
                    UnitCost = model.UnitCost,
                    Notes = NormalizeText(model.LineNotes)
                }
            ]
        };
    }
}
