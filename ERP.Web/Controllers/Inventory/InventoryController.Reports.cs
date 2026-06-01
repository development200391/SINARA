using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;
using ERP.Domain.Enums.Inventory;
using ERP.Web.ViewModels.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class InventoryController
{
    [HttpGet("reports/stock-summary")]
    public async Task<IActionResult> StockSummaryReport(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "totalvalue",
        string? sortDirection = "desc",
        int? warehouseId = null,
        int? categoryId = null,
        int? itemId = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "totalvalue", "itemcode", "warehousecode", "qtyavailable", "totalvalue");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);

        var itemsTask = inventoryApiClient.GetStockSummaryReportAsync(accessToken, new InventoryReportRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            WarehouseId = warehouseId,
            CategoryId = categoryId,
            ItemId = itemId
        }, ct);

        var warehouseOptionsTask = inventoryApiClient.GetWarehouseOptionsAsync(accessToken, ct);
        var categoryOptionsTask = inventoryApiClient.GetCategoryOptionsAsync(accessToken, ct);
        var itemOptionsTask = inventoryApiClient.GetItemOptionsAsync(accessToken, ct);

        await Task.WhenAll(itemsTask, warehouseOptionsTask, categoryOptionsTask, itemOptionsTask);

        ViewData["Title"] = "Stock Summary";
        ViewData["Breadcrumb"] = "Inventory / Reports / Stock Summary";

        return View("Reports/StockSummary", new InventoryStockSummaryReportViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            WarehouseIdFilter = warehouseId,
            CategoryIdFilter = categoryId,
            ItemIdFilter = itemId,
            WarehouseOptions = await warehouseOptionsTask,
            CategoryOptions = await categoryOptionsTask,
            ItemOptions = await itemOptionsTask,
            Items = await itemsTask ?? PagedResult<InventoryStockSummaryDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("reports/movement-history")]
    public async Task<IActionResult> MovementHistoryReport(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "movementdate",
        string? sortDirection = "desc",
        int? warehouseId = null,
        int? itemId = null,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        StockMovementType? movementType = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "movementdate", "movementdate", "itemcode", "warehousecode", "movementtype");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedDateFrom, normalizedDateTo) = NormalizeDateRange(dateFrom, dateTo);

        var itemsTask = inventoryApiClient.GetMovementHistoryReportAsync(accessToken, new InventoryReportRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            WarehouseId = warehouseId,
            ItemId = itemId,
            DateFrom = normalizedDateFrom,
            DateTo = normalizedDateTo,
            MovementType = movementType
        }, ct);

        var warehouseOptionsTask = inventoryApiClient.GetWarehouseOptionsAsync(accessToken, ct);
        var itemOptionsTask = inventoryApiClient.GetItemOptionsAsync(accessToken, ct);

        await Task.WhenAll(itemsTask, warehouseOptionsTask, itemOptionsTask);

        ViewData["Title"] = "Movement History";
        ViewData["Breadcrumb"] = "Inventory / Reports / Movement History";

        return View("Reports/MovementHistory", new InventoryMovementReportViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            WarehouseIdFilter = warehouseId,
            ItemIdFilter = itemId,
            DateFromFilter = normalizedDateFrom,
            DateToFilter = normalizedDateTo,
            MovementTypeFilter = movementType,
            WarehouseOptions = await warehouseOptionsTask,
            ItemOptions = await itemOptionsTask,
            Items = await itemsTask ?? PagedResult<InventoryMovementHistoryDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("reports/low-stock")]
    public async Task<IActionResult> LowStockReport(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "itemcode",
        string? sortDirection = "asc",
        int? warehouseId = null,
        int? categoryId = null,
        int? itemId = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "itemcode", "itemcode", "warehousecode", "qtyavailable", "minstock", "difference");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);

        var itemsTask = inventoryApiClient.GetLowStockReportAsync(accessToken, new InventoryReportRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            WarehouseId = warehouseId,
            CategoryId = categoryId,
            ItemId = itemId
        }, ct);

        var warehouseOptionsTask = inventoryApiClient.GetWarehouseOptionsAsync(accessToken, ct);
        var categoryOptionsTask = inventoryApiClient.GetCategoryOptionsAsync(accessToken, ct);
        var itemOptionsTask = inventoryApiClient.GetItemOptionsAsync(accessToken, ct);

        await Task.WhenAll(itemsTask, warehouseOptionsTask, categoryOptionsTask, itemOptionsTask);

        ViewData["Title"] = "Low Stock Report";
        ViewData["Breadcrumb"] = "Inventory / Reports / Low Stock";

        return View("Reports/LowStock", new InventoryLowStockReportViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            WarehouseIdFilter = warehouseId,
            CategoryIdFilter = categoryId,
            ItemIdFilter = itemId,
            WarehouseOptions = await warehouseOptionsTask,
            CategoryOptions = await categoryOptionsTask,
            ItemOptions = await itemOptionsTask,
            Items = await itemsTask ?? PagedResult<InventoryLowStockDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("reports/inventory-valuation")]
    public async Task<IActionResult> InventoryValuationReport(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "totalvalue",
        string? sortDirection = "desc",
        int? warehouseId = null,
        int? categoryId = null,
        int? itemId = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "totalvalue", "itemcode", "warehousecode", "qtyonhand", "avgcost", "totalvalue");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);

        var itemsTask = inventoryApiClient.GetInventoryValuationReportAsync(accessToken, new InventoryReportRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            WarehouseId = warehouseId,
            CategoryId = categoryId,
            ItemId = itemId
        }, ct);

        var warehouseOptionsTask = inventoryApiClient.GetWarehouseOptionsAsync(accessToken, ct);
        var categoryOptionsTask = inventoryApiClient.GetCategoryOptionsAsync(accessToken, ct);
        var itemOptionsTask = inventoryApiClient.GetItemOptionsAsync(accessToken, ct);

        await Task.WhenAll(itemsTask, warehouseOptionsTask, categoryOptionsTask, itemOptionsTask);

        ViewData["Title"] = "Inventory Valuation";
        ViewData["Breadcrumb"] = "Inventory / Reports / Inventory Valuation";

        return View("Reports/Valuation", new InventoryValuationReportViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            WarehouseIdFilter = warehouseId,
            CategoryIdFilter = categoryId,
            ItemIdFilter = itemId,
            WarehouseOptions = await warehouseOptionsTask,
            CategoryOptions = await categoryOptionsTask,
            ItemOptions = await itemOptionsTask,
            Items = await itemsTask ?? PagedResult<InventoryValuationDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("reports/inventory-aging")]
    public async Task<IActionResult> InventoryAgingReport(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "dayssincelastmovement",
        string? sortDirection = "desc",
        int? warehouseId = null,
        int? categoryId = null,
        int? itemId = null,
        int? thresholdDays = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "dayssincelastmovement", "itemcode", "warehousecode", "dayssincelastmovement", "qtyonhand", "totalvalue");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);

        var itemsTask = inventoryApiClient.GetInventoryAgingReportAsync(accessToken, new InventoryReportRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            WarehouseId = warehouseId,
            CategoryId = categoryId,
            ItemId = itemId,
            ThresholdDays = thresholdDays
        }, ct);

        var warehouseOptionsTask = inventoryApiClient.GetWarehouseOptionsAsync(accessToken, ct);
        var categoryOptionsTask = inventoryApiClient.GetCategoryOptionsAsync(accessToken, ct);
        var itemOptionsTask = inventoryApiClient.GetItemOptionsAsync(accessToken, ct);

        await Task.WhenAll(itemsTask, warehouseOptionsTask, categoryOptionsTask, itemOptionsTask);

        ViewData["Title"] = "Inventory Aging";
        ViewData["Breadcrumb"] = "Inventory / Reports / Inventory Aging";

        return View("Reports/Aging", new InventoryAgingReportViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            WarehouseIdFilter = warehouseId,
            CategoryIdFilter = categoryId,
            ItemIdFilter = itemId,
            ThresholdDaysFilter = thresholdDays,
            WarehouseOptions = await warehouseOptionsTask,
            CategoryOptions = await categoryOptionsTask,
            ItemOptions = await itemOptionsTask,
            Items = await itemsTask ?? PagedResult<InventoryAgingDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("reports/receipt-summary")]
    public async Task<IActionResult> ReceiptSummaryReport(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "receiptdate",
        string? sortDirection = "desc",
        int? warehouseId = null,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "receiptdate", "receiptno", "receiptdate", "warehousecode", "status", "receipttype", "totalquantity", "totalcost");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedDateFrom, normalizedDateTo) = NormalizeDateRange(dateFrom, dateTo);

        var itemsTask = inventoryApiClient.GetReceiptSummaryReportAsync(accessToken, new InventoryReportRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            WarehouseId = warehouseId,
            DateFrom = normalizedDateFrom,
            DateTo = normalizedDateTo
        }, ct);

        var warehouseOptionsTask = inventoryApiClient.GetWarehouseOptionsAsync(accessToken, ct);
        await Task.WhenAll(itemsTask, warehouseOptionsTask);

        ViewData["Title"] = "Receipt Summary";
        ViewData["Breadcrumb"] = "Inventory / Reports / Receipt Summary";

        return View("Reports/ReceiptSummary", new InventorySimpleReportViewModel<InventoryReceiptSummaryDto>
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            WarehouseIdFilter = warehouseId,
            DateFromFilter = normalizedDateFrom,
            DateToFilter = normalizedDateTo,
            WarehouseOptions = await warehouseOptionsTask,
            Items = await itemsTask ?? PagedResult<InventoryReceiptSummaryDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("reports/issue-summary")]
    public async Task<IActionResult> IssueSummaryReport(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "issuedate",
        string? sortDirection = "desc",
        int? warehouseId = null,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "issuedate", "issueno", "issuedate", "warehousecode", "status", "issuetype", "totalquantity", "totalcost");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedDateFrom, normalizedDateTo) = NormalizeDateRange(dateFrom, dateTo);

        var itemsTask = inventoryApiClient.GetIssueSummaryReportAsync(accessToken, new InventoryReportRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            WarehouseId = warehouseId,
            DateFrom = normalizedDateFrom,
            DateTo = normalizedDateTo
        }, ct);

        var warehouseOptionsTask = inventoryApiClient.GetWarehouseOptionsAsync(accessToken, ct);
        await Task.WhenAll(itemsTask, warehouseOptionsTask);

        ViewData["Title"] = "Issue Summary";
        ViewData["Breadcrumb"] = "Inventory / Reports / Issue Summary";

        return View("Reports/IssueSummary", new InventorySimpleReportViewModel<InventoryIssueSummaryDto>
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            WarehouseIdFilter = warehouseId,
            DateFromFilter = normalizedDateFrom,
            DateToFilter = normalizedDateTo,
            WarehouseOptions = await warehouseOptionsTask,
            Items = await itemsTask ?? PagedResult<InventoryIssueSummaryDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("reports/transfer-summary")]
    public async Task<IActionResult> TransferSummaryReport(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "transferdate",
        string? sortDirection = "desc",
        int? warehouseId = null,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "transferdate", "transferno", "transferdate", "fromwarehousecode", "towarehousecode", "status", "totalquantity", "totalcost");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedDateFrom, normalizedDateTo) = NormalizeDateRange(dateFrom, dateTo);

        var itemsTask = inventoryApiClient.GetTransferSummaryReportAsync(accessToken, new InventoryReportRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            WarehouseId = warehouseId,
            DateFrom = normalizedDateFrom,
            DateTo = normalizedDateTo
        }, ct);

        var warehouseOptionsTask = inventoryApiClient.GetWarehouseOptionsAsync(accessToken, ct);
        await Task.WhenAll(itemsTask, warehouseOptionsTask);

        ViewData["Title"] = "Transfer Summary";
        ViewData["Breadcrumb"] = "Inventory / Reports / Transfer Summary";

        return View("Reports/TransferSummary", new InventorySimpleReportViewModel<InventoryTransferSummaryDto>
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            WarehouseIdFilter = warehouseId,
            DateFromFilter = normalizedDateFrom,
            DateToFilter = normalizedDateTo,
            WarehouseOptions = await warehouseOptionsTask,
            Items = await itemsTask ?? PagedResult<InventoryTransferSummaryDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("reports/adjustment-summary")]
    public async Task<IActionResult> AdjustmentSummaryReport(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "adjustmentdate",
        string? sortDirection = "desc",
        int? warehouseId = null,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "adjustmentdate", "adjustmentno", "adjustmentdate", "warehousecode", "reason", "status", "totalquantity", "totalcost");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedDateFrom, normalizedDateTo) = NormalizeDateRange(dateFrom, dateTo);

        var itemsTask = inventoryApiClient.GetAdjustmentSummaryReportAsync(accessToken, new InventoryReportRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            WarehouseId = warehouseId,
            DateFrom = normalizedDateFrom,
            DateTo = normalizedDateTo
        }, ct);

        var warehouseOptionsTask = inventoryApiClient.GetWarehouseOptionsAsync(accessToken, ct);
        await Task.WhenAll(itemsTask, warehouseOptionsTask);

        ViewData["Title"] = "Adjustment Summary";
        ViewData["Breadcrumb"] = "Inventory / Reports / Adjustment Summary";

        return View("Reports/AdjustmentSummary", new InventorySimpleReportViewModel<InventoryAdjustmentSummaryDto>
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            WarehouseIdFilter = warehouseId,
            DateFromFilter = normalizedDateFrom,
            DateToFilter = normalizedDateTo,
            WarehouseOptions = await warehouseOptionsTask,
            Items = await itemsTask ?? PagedResult<InventoryAdjustmentSummaryDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    private static (DateOnly? From, DateOnly? To) NormalizeDateRange(DateOnly? from, DateOnly? to)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
        {
            return (to, from);
        }

        return (from, to);
    }
}


