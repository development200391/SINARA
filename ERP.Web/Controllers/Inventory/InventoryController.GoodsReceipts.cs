using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;
using ERP.Domain.Enums.Inventory;
using ERP.Web.ViewModels.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class InventoryController
{
    [HttpGet("goods-receipts")]
    public async Task<IActionResult> GoodsReceipts(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "receiptdate",
        string? sortDirection = "desc",
        string? receiptNo = null,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        int? warehouseId = null,
        GoodsReceiptType? receiptType = null,
        TransactionStatus? status = null,
        string? supplierName = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "receiptdate", "receiptno", "receiptdate", "warehousecode", "status", "receipttype");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);

        var itemsTask = inventoryApiClient.GetGoodsReceiptsAsync(accessToken, new GoodsReceiptPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            ReceiptNo = NormalizeText(receiptNo),
            DateFrom = dateFrom,
            DateTo = dateTo,
            WarehouseId = warehouseId,
            ReceiptType = receiptType,
            Status = status,
            SupplierName = NormalizeText(supplierName)
        }, ct);

        var warehouseTask = inventoryApiClient.GetWarehouseOptionsAsync(accessToken, ct);

        await Task.WhenAll(itemsTask, warehouseTask);

        ViewData["Title"] = "Goods Receipts";
        ViewData["Breadcrumb"] = "Inventory / Goods Receipts";

        return View("GoodsReceipts/Index", new InventoryGoodsReceiptsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            ReceiptNoFilter = NormalizeText(receiptNo),
            DateFromFilter = dateFrom,
            DateToFilter = dateTo,
            WarehouseIdFilter = warehouseId,
            ReceiptTypeFilter = receiptType,
            StatusFilter = status,
            SupplierNameFilter = NormalizeText(supplierName),
            WarehouseOptions = await warehouseTask,
            Items = await itemsTask ?? PagedResult<GoodsReceiptDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("goods-receipts/create")]
    public async Task<IActionResult> CreateGoodsReceipt(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new InventoryGoodsReceiptEditViewModel();
        await PopulateGoodsReceiptOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create Goods Receipt";
        ViewData["Breadcrumb"] = "Inventory / Goods Receipts / Create";

        return View("GoodsReceipts/Create", model);
    }

    [HttpPost("goods-receipts/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateGoodsReceipt(InventoryGoodsReceiptEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        await PopulateGoodsReceiptOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Goods Receipt";
            ViewData["Breadcrumb"] = "Inventory / Goods Receipts / Create";
            return View("GoodsReceipts/Create", model);
        }

        var created = await inventoryApiClient.CreateGoodsReceiptAsync(accessToken, MapGoodsReceiptDto(model), ct);
        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create goods receipt.");
            ViewData["Title"] = "Create Goods Receipt";
            ViewData["Breadcrumb"] = "Inventory / Goods Receipts / Create";
            return View("GoodsReceipts/Create", model);
        }

        TempData["SuccessMessage"] = "Goods receipt created.";
        return RedirectToAction(nameof(GoodsReceipts));
    }

    [HttpGet("goods-receipts/edit/{id:int}")]
    public async Task<IActionResult> EditGoodsReceipt(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var dto = await inventoryApiClient.GetGoodsReceiptByIdAsync(accessToken, id, ct);
        if (dto is null)
        {
            return NotFound();
        }

        var line = dto.Lines.FirstOrDefault();
        var model = new InventoryGoodsReceiptEditViewModel
        {
            Id = dto.Id,
            ReceiptDate = dto.ReceiptDate,
            ReceiptType = dto.ReceiptType,
            WarehouseId = dto.WarehouseId,
            LocationId = dto.LocationId,
            SupplierName = dto.SupplierName,
            ReferenceNo = dto.ReferenceNo,
            Description = dto.Description,
            Status = dto.Status,
            ItemId = line?.ItemId ?? 0,
            UomId = line?.UomId,
            QtyReceived = line?.QtyReceived ?? 1m,
            QtyBase = line?.QtyBase ?? 1m,
            UnitCost = line?.UnitCost ?? 0m,
            LineNotes = line?.Notes
        };

        await PopulateGoodsReceiptOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit Goods Receipt";
        ViewData["Breadcrumb"] = "Inventory / Goods Receipts / Edit";

        return View("GoodsReceipts/Edit", model);
    }

    [HttpPost("goods-receipts/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditGoodsReceipt(int id, InventoryGoodsReceiptEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        await PopulateGoodsReceiptOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Goods Receipt";
            ViewData["Breadcrumb"] = "Inventory / Goods Receipts / Edit";
            return View("GoodsReceipts/Edit", model);
        }

        var updated = await inventoryApiClient.UpdateGoodsReceiptAsync(accessToken, id, MapGoodsReceiptDto(model), ct);
        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to update goods receipt.");
            ViewData["Title"] = "Edit Goods Receipt";
            ViewData["Breadcrumb"] = "Inventory / Goods Receipts / Edit";
            return View("GoodsReceipts/Edit", model);
        }

        TempData["SuccessMessage"] = "Goods receipt updated.";
        return RedirectToAction(nameof(GoodsReceipts));
    }

    [HttpPost("goods-receipts/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteGoodsReceipt(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var deleted = await inventoryApiClient.DeleteGoodsReceiptAsync(accessToken, id, ct);
        TempData[deleted ? "SuccessMessage" : "ErrorMessage"] = deleted ? "Goods receipt deleted." : "Failed to delete goods receipt.";
        return RedirectToAction(nameof(GoodsReceipts));
    }

    [HttpPost("goods-receipts/confirm/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmGoodsReceipt(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await inventoryApiClient.ConfirmGoodsReceiptAsync(accessToken, id, ct);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Goods receipt confirmed." : "Failed to confirm goods receipt.";
        return RedirectToAction(nameof(GoodsReceipts));
    }

    [HttpPost("goods-receipts/cancel/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelGoodsReceipt(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await inventoryApiClient.CancelGoodsReceiptAsync(accessToken, id, ct);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Goods receipt cancelled." : "Failed to cancel goods receipt.";
        return RedirectToAction(nameof(GoodsReceipts));
    }

    private async Task PopulateGoodsReceiptOptionsAsync(string accessToken, InventoryGoodsReceiptEditViewModel model, CancellationToken ct)
    {
        var warehouseTask = inventoryApiClient.GetWarehouseOptionsAsync(accessToken, ct);
        var itemTask = inventoryApiClient.GetItemOptionsAsync(accessToken, ct);
        var uomTask = inventoryApiClient.GetUnitOptionsAsync(accessToken, ct);
        var locationTask = GetWarehouseLocationOptionsAsync(accessToken, model.WarehouseId > 0 ? model.WarehouseId : null, ct);

        await Task.WhenAll(warehouseTask, itemTask, uomTask, locationTask);

        model.WarehouseOptions = await warehouseTask;
        model.ItemOptions = await itemTask;
        model.UomOptions = await uomTask;
        model.LocationOptions = await locationTask;
    }

    private static GoodsReceiptDto MapGoodsReceiptDto(InventoryGoodsReceiptEditViewModel model)
    {
        return new GoodsReceiptDto
        {
            Id = model.Id ?? 0,
            ReceiptDate = model.ReceiptDate,
            ReceiptType = model.ReceiptType,
            WarehouseId = model.WarehouseId,
            LocationId = model.LocationId,
            SupplierName = NormalizeText(model.SupplierName),
            ReferenceNo = NormalizeText(model.ReferenceNo),
            Description = NormalizeText(model.Description),
            Lines =
            [
                new GoodsReceiptLineDto
                {
                    LineNo = 1,
                    ItemId = model.ItemId,
                    UomId = model.UomId,
                    QtyReceived = model.QtyReceived,
                    QtyBase = model.QtyBase,
                    UnitCost = model.UnitCost,
                    Notes = NormalizeText(model.LineNotes)
                }
            ]
        };
    }
}
