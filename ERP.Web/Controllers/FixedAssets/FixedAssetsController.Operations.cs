using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.FixedAssets;
using ERP.Domain.Enums.FixedAssets;
using ERP.Web.ViewModels.FixedAssets;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class FixedAssetsController
{
    [HttpGet("transfers")]
    public async Task<IActionResult> Transfers(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "transferdate",
        string? sortDirection = "desc",
        int? assetId = null,
        int? fromLocationId = null,
        int? toLocationId = null,
        AssetTransferStatus? status = null,
        DateOnly? transferDateFrom = null,
        DateOnly? transferDateTo = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "transferdate", "transferno", "assetcode", "transferdate", "fromlocationname", "tolocationname", "status");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedDateFrom, normalizedDateTo) = NormalizeDateRange(transferDateFrom, transferDateTo);

        var assetOptionsTask = fixedAssetsApiClient.GetAssetOptionsAsync(accessToken, ct);
        var locationOptionsTask = fixedAssetsApiClient.GetLocationOptionsAsync(accessToken, ct);
        var itemsTask = fixedAssetsApiClient.GetTransfersAsync(accessToken, new FixedAssetTransferPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            AssetId = assetId,
            FromLocationId = fromLocationId,
            ToLocationId = toLocationId,
            Status = status,
            TransferDateFrom = normalizedDateFrom,
            TransferDateTo = normalizedDateTo
        }, ct);

        await Task.WhenAll(assetOptionsTask, locationOptionsTask, itemsTask);

        ViewData["Title"] = "Asset Transfers";
        ViewData["Breadcrumb"] = "Fixed Assets / Asset Transfers";

        return View("Transfers/Index", new FixedAssetTransfersIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            AssetIdFilter = assetId,
            FromLocationIdFilter = fromLocationId,
            ToLocationIdFilter = toLocationId,
            StatusFilter = status,
            TransferDateFromFilter = normalizedDateFrom,
            TransferDateToFilter = normalizedDateTo,
            AssetOptions = await assetOptionsTask,
            LocationOptions = await locationOptionsTask,
            Items = await itemsTask ?? PagedResult<FixedAssetTransferDto>.Create([], 0, normalizedPage, normalizedPageSize)
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

        var model = new FixedAssetTransferEditViewModel();
        await PopulateTransferFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create Transfer";
        ViewData["Breadcrumb"] = "Fixed Assets / Asset Transfers / Create";

        return View("Transfers/Create", model);
    }

    [HttpPost("transfers/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTransfer(FixedAssetTransferEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        await PopulateTransferFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Transfer";
            ViewData["Breadcrumb"] = "Fixed Assets / Asset Transfers / Create";
            return View("Transfers/Create", model);
        }

        var created = await fixedAssetsApiClient.CreateTransferAsync(accessToken, MapTransferDto(model), ct);
        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create transfer.");
            ViewData["Title"] = "Create Transfer";
            ViewData["Breadcrumb"] = "Fixed Assets / Asset Transfers / Create";
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

        var item = await fixedAssetsApiClient.GetTransferByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        var model = new FixedAssetTransferEditViewModel
        {
            Id = item.Id,
            AssetId = item.AssetId,
            TransferDate = item.TransferDate,
            FromLocationId = item.FromLocationId,
            ToLocationId = item.ToLocationId,
            FromDepartmentId = item.FromDepartmentId,
            ToDepartmentId = item.ToDepartmentId,
            Reason = item.Reason
        };

        await PopulateTransferFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit Transfer";
        ViewData["Breadcrumb"] = "Fixed Assets / Asset Transfers / Edit";

        return View("Transfers/Edit", model);
    }

    [HttpPost("transfers/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTransfer(int id, FixedAssetTransferEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        await PopulateTransferFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Transfer";
            ViewData["Breadcrumb"] = "Fixed Assets / Asset Transfers / Edit";
            return View("Transfers/Edit", model);
        }

        var updated = await fixedAssetsApiClient.UpdateTransferAsync(accessToken, id, MapTransferDto(model), ct);
        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to update transfer.");
            ViewData["Title"] = "Edit Transfer";
            ViewData["Breadcrumb"] = "Fixed Assets / Asset Transfers / Edit";
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

        var deleted = await fixedAssetsApiClient.DeleteTransferAsync(accessToken, id, ct);
        TempData[deleted ? "SuccessMessage" : "ErrorMessage"] = deleted
            ? "Transfer deleted."
            : "Failed to delete transfer.";

        return RedirectToAction(nameof(Transfers));
    }

    [HttpPost("transfers/approve/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveTransfer(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await fixedAssetsApiClient.ApproveTransferAsync(accessToken, id, ct);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
            ? "Transfer approved."
            : "Failed to approve transfer.";

        return RedirectToAction(nameof(Transfers));
    }

    [HttpPost("transfers/reject/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectTransfer(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await fixedAssetsApiClient.RejectTransferAsync(accessToken, id, ct);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
            ? "Transfer rejected."
            : "Failed to reject transfer.";

        return RedirectToAction(nameof(Transfers));
    }
    [HttpGet("maintenance-orders")]
    public async Task<IActionResult> MaintenanceOrders(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "orderdate",
        string? sortDirection = "desc",
        int? assetId = null,
        MaintenanceType? maintenanceType = null,
        MaintenanceStatus? status = null,
        DateOnly? orderDateFrom = null,
        DateOnly? orderDateTo = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "orderdate", "workorderno", "assetcode", "orderdate", "maintenancetype", "cost", "status");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedDateFrom, normalizedDateTo) = NormalizeDateRange(orderDateFrom, orderDateTo);

        var assetOptionsTask = fixedAssetsApiClient.GetAssetOptionsAsync(accessToken, ct);
        var itemsTask = fixedAssetsApiClient.GetMaintenanceOrdersAsync(accessToken, new FixedAssetMaintenanceOrderPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            AssetId = assetId,
            MaintenanceType = maintenanceType,
            Status = status,
            OrderDateFrom = normalizedDateFrom,
            OrderDateTo = normalizedDateTo
        }, ct);

        await Task.WhenAll(assetOptionsTask, itemsTask);

        ViewData["Title"] = "Maintenance Orders";
        ViewData["Breadcrumb"] = "Fixed Assets / Maintenance Orders";

        return View("MaintenanceOrders/Index", new FixedAssetMaintenanceOrdersIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            AssetIdFilter = assetId,
            MaintenanceTypeFilter = maintenanceType,
            StatusFilter = status,
            OrderDateFromFilter = normalizedDateFrom,
            OrderDateToFilter = normalizedDateTo,
            AssetOptions = await assetOptionsTask,
            Items = await itemsTask ?? PagedResult<FixedAssetMaintenanceOrderDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("maintenance-orders/create")]
    public async Task<IActionResult> CreateMaintenanceOrder(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new FixedAssetMaintenanceOrderEditViewModel();
        await PopulateMaintenanceOrderFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create Maintenance Order";
        ViewData["Breadcrumb"] = "Fixed Assets / Maintenance Orders / Create";

        return View("MaintenanceOrders/Create", model);
    }

    [HttpPost("maintenance-orders/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateMaintenanceOrder(FixedAssetMaintenanceOrderEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        await PopulateMaintenanceOrderFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Maintenance Order";
            ViewData["Breadcrumb"] = "Fixed Assets / Maintenance Orders / Create";
            return View("MaintenanceOrders/Create", model);
        }

        var created = await fixedAssetsApiClient.CreateMaintenanceOrderAsync(accessToken, MapMaintenanceOrderDto(model), ct);
        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create maintenance order.");
            ViewData["Title"] = "Create Maintenance Order";
            ViewData["Breadcrumb"] = "Fixed Assets / Maintenance Orders / Create";
            return View("MaintenanceOrders/Create", model);
        }

        TempData["SuccessMessage"] = "Maintenance order created.";
        return RedirectToAction(nameof(MaintenanceOrders));
    }
    [HttpGet("maintenance-orders/edit/{id:int}")]
    public async Task<IActionResult> EditMaintenanceOrder(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await fixedAssetsApiClient.GetMaintenanceOrderByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        var model = new FixedAssetMaintenanceOrderEditViewModel
        {
            Id = item.Id,
            AssetId = item.AssetId,
            OrderDate = item.OrderDate,
            MaintenanceType = item.MaintenanceType,
            VendorName = item.VendorName,
            Cost = item.Cost,
            IsCapitalized = item.IsCapitalized,
            Notes = item.Notes
        };

        await PopulateMaintenanceOrderFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit Maintenance Order";
        ViewData["Breadcrumb"] = "Fixed Assets / Maintenance Orders / Edit";

        return View("MaintenanceOrders/Edit", model);
    }

    [HttpPost("maintenance-orders/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditMaintenanceOrder(int id, FixedAssetMaintenanceOrderEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        await PopulateMaintenanceOrderFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Maintenance Order";
            ViewData["Breadcrumb"] = "Fixed Assets / Maintenance Orders / Edit";
            return View("MaintenanceOrders/Edit", model);
        }

        var updated = await fixedAssetsApiClient.UpdateMaintenanceOrderAsync(accessToken, id, MapMaintenanceOrderDto(model), ct);
        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to update maintenance order.");
            ViewData["Title"] = "Edit Maintenance Order";
            ViewData["Breadcrumb"] = "Fixed Assets / Maintenance Orders / Edit";
            return View("MaintenanceOrders/Edit", model);
        }

        TempData["SuccessMessage"] = "Maintenance order updated.";
        return RedirectToAction(nameof(MaintenanceOrders));
    }

    [HttpPost("maintenance-orders/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMaintenanceOrder(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var deleted = await fixedAssetsApiClient.DeleteMaintenanceOrderAsync(accessToken, id, ct);
        TempData[deleted ? "SuccessMessage" : "ErrorMessage"] = deleted
            ? "Maintenance order deleted."
            : "Failed to delete maintenance order.";

        return RedirectToAction(nameof(MaintenanceOrders));
    }

    [HttpPost("maintenance-orders/process/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessMaintenanceOrder(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await fixedAssetsApiClient.GetMaintenanceOrderByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            TempData["ErrorMessage"] = "Maintenance order not found.";
            return RedirectToAction(nameof(MaintenanceOrders));
        }

        var ok = item.Status switch
        {
            MaintenanceStatus.Open => await fixedAssetsApiClient.StartMaintenanceOrderAsync(accessToken, id, ct),
            MaintenanceStatus.InProgress => await fixedAssetsApiClient.CompleteMaintenanceOrderAsync(accessToken, id, ct),
            _ => false
        };

        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
            ? "Maintenance order processed."
            : "Failed to process maintenance order.";

        return RedirectToAction(nameof(MaintenanceOrders));
    }

    [HttpPost("maintenance-orders/cancel/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelMaintenanceOrder(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await fixedAssetsApiClient.CancelMaintenanceOrderAsync(accessToken, id, ct);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
            ? "Maintenance order cancelled."
            : "Failed to cancel maintenance order.";

        return RedirectToAction(nameof(MaintenanceOrders));
    }
}
