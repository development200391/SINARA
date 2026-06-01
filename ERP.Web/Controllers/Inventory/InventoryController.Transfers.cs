using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;
using ERP.Domain.Enums.Inventory;
using ERP.Web.ViewModels.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class InventoryController
{
    [HttpGet("transfers")]
    public async Task<IActionResult> Transfers(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "transferdate",
        string? sortDirection = "desc",
        string? transferNo = null,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        int? fromWarehouseId = null,
        int? toWarehouseId = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "transferdate", "transferno", "transferdate", "fromwarehousecode", "towarehousecode", "status");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);

        var itemsTask = inventoryApiClient.GetTransfersAsync(accessToken, new StockTransferPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            TransferNo = NormalizeText(transferNo),
            DateFrom = dateFrom,
            DateTo = dateTo,
            FromWarehouseId = fromWarehouseId,
            ToWarehouseId = toWarehouseId,
            Status = status
        }, ct);

        var warehouseOptionsTask = inventoryApiClient.GetWarehouseOptionsAsync(accessToken, ct);

        await Task.WhenAll(itemsTask, warehouseOptionsTask);

        ViewData["Title"] = "Stock Transfers";
        ViewData["Breadcrumb"] = "Inventory / Stock Transfers";

        return View("Transfers/Index", new InventoryTransfersIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            TransferNoFilter = NormalizeText(transferNo),
            DateFromFilter = dateFrom,
            DateToFilter = dateTo,
            FromWarehouseIdFilter = fromWarehouseId,
            ToWarehouseIdFilter = toWarehouseId,
            StatusFilter = status,
            WarehouseOptions = await warehouseOptionsTask,
            Items = await itemsTask ?? PagedResult<StockTransferDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("transfers/create")]
    public async Task<IActionResult> CreateTransfer(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new InventoryTransferEditViewModel();
        await PopulateTransferOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create Transfer";
        ViewData["Breadcrumb"] = "Inventory / Stock Transfers / Create";

        return View("Transfers/Create", model);
    }

    [HttpPost("transfers/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTransfer(InventoryTransferEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        await PopulateTransferOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Transfer";
            ViewData["Breadcrumb"] = "Inventory / Stock Transfers / Create";
            return View("Transfers/Create", model);
        }

        var created = await inventoryApiClient.CreateTransferAsync(accessToken, MapTransferDto(model), ct);
        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create transfer.");
            ViewData["Title"] = "Create Transfer";
            ViewData["Breadcrumb"] = "Inventory / Stock Transfers / Create";
            return View("Transfers/Create", model);
        }

        TempData["SuccessMessage"] = "Transfer created.";
        return RedirectToAction(nameof(Transfers));
    }

    [HttpGet("transfers/edit/{id:int}")]
    public async Task<IActionResult> EditTransfer(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var dto = await inventoryApiClient.GetTransferByIdAsync(accessToken, id, ct);
        if (dto is null)
        {
            return NotFound();
        }

        var line = dto.Lines.FirstOrDefault();
        var model = new InventoryTransferEditViewModel
        {
            Id = dto.Id,
            TransferDate = dto.TransferDate,
            FromWarehouseId = dto.FromWarehouseId,
            FromLocationId = dto.FromLocationId,
            ToWarehouseId = dto.ToWarehouseId,
            ToLocationId = dto.ToLocationId,
            ReferenceNo = dto.ReferenceNo,
            Description = dto.Description,
            Status = dto.Status,
            ItemId = line?.ItemId ?? 0,
            UomId = line?.UomId,
            QtyTransfer = line?.QtyTransfer ?? 1m,
            QtyBase = line?.QtyBase ?? 1m,
            UnitCost = line?.UnitCost ?? 0m,
            LineNotes = line?.Notes
        };

        await PopulateTransferOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit Transfer";
        ViewData["Breadcrumb"] = "Inventory / Stock Transfers / Edit";

        return View("Transfers/Edit", model);
    }

    [HttpPost("transfers/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTransfer(int id, InventoryTransferEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        await PopulateTransferOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Transfer";
            ViewData["Breadcrumb"] = "Inventory / Stock Transfers / Edit";
            return View("Transfers/Edit", model);
        }

        var updated = await inventoryApiClient.UpdateTransferAsync(accessToken, id, MapTransferDto(model), ct);
        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to update transfer.");
            ViewData["Title"] = "Edit Transfer";
            ViewData["Breadcrumb"] = "Inventory / Stock Transfers / Edit";
            return View("Transfers/Edit", model);
        }

        TempData["SuccessMessage"] = "Transfer updated.";
        return RedirectToAction(nameof(Transfers));
    }

    [HttpPost("transfers/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTransfer(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var deleted = await inventoryApiClient.DeleteTransferAsync(accessToken, id, ct);
        TempData[deleted ? "SuccessMessage" : "ErrorMessage"] = deleted ? "Transfer deleted." : "Failed to delete transfer.";
        return RedirectToAction(nameof(Transfers));
    }

    [HttpPost("transfers/confirm/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmTransfer(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await inventoryApiClient.ConfirmTransferAsync(accessToken, id, ct);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Transfer confirmed." : "Failed to confirm transfer.";
        return RedirectToAction(nameof(Transfers));
    }

    [HttpPost("transfers/cancel/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelTransfer(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await inventoryApiClient.CancelTransferAsync(accessToken, id, ct);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Transfer cancelled." : "Failed to cancel transfer.";
        return RedirectToAction(nameof(Transfers));
    }

    private async Task PopulateTransferOptionsAsync(string accessToken, InventoryTransferEditViewModel model, CancellationToken ct)
    {
        var warehouseTask = inventoryApiClient.GetWarehouseOptionsAsync(accessToken, ct);
        var fromLocationTask = GetWarehouseLocationOptionsAsync(accessToken, model.FromWarehouseId > 0 ? model.FromWarehouseId : null, ct);
        var toLocationTask = GetWarehouseLocationOptionsAsync(accessToken, model.ToWarehouseId > 0 ? model.ToWarehouseId : null, ct);
        var itemTask = inventoryApiClient.GetItemOptionsAsync(accessToken, ct);
        var uomTask = inventoryApiClient.GetUnitOptionsAsync(accessToken, ct);

        await Task.WhenAll(warehouseTask, fromLocationTask, toLocationTask, itemTask, uomTask);

        model.WarehouseOptions = await warehouseTask;
        model.FromLocationOptions = await fromLocationTask;
        model.ToLocationOptions = await toLocationTask;
        model.ItemOptions = await itemTask;
        model.UomOptions = await uomTask;
    }

    private static StockTransferDto MapTransferDto(InventoryTransferEditViewModel model)
    {
        return new StockTransferDto
        {
            Id = model.Id ?? 0,
            TransferDate = model.TransferDate,
            FromWarehouseId = model.FromWarehouseId,
            FromLocationId = model.FromLocationId,
            ToWarehouseId = model.ToWarehouseId,
            ToLocationId = model.ToLocationId,
            ReferenceNo = NormalizeText(model.ReferenceNo),
            Description = NormalizeText(model.Description),
            Lines =
            [
                new StockTransferLineDto
                {
                    LineNo = 1,
                    ItemId = model.ItemId,
                    UomId = model.UomId,
                    QtyTransfer = model.QtyTransfer,
                    QtyBase = model.QtyBase,
                    UnitCost = model.UnitCost,
                    Notes = NormalizeText(model.LineNotes)
                }
            ]
        };
    }
}
