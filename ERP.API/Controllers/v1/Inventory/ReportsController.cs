using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;
using ERP.Domain.Enums.Inventory;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Inventory;

[Route("api/v1/inventory/reports")]
public sealed class ReportsController(AppDbContext dbContext) : InventoryControllerBase
{
    [HttpGet("stock-summary")]
    public async Task<IActionResult> StockSummary([FromQuery] InventoryReportRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.InvStockBalances
            .AsNoTracking()
            .Include(x => x.Item)
                .ThenInclude(x => x.Category)
            .Include(x => x.Warehouse)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Item.ItemCode.ToLower().Contains(search) ||
                x.Item.Name.ToLower().Contains(search) ||
                x.Item.Category.Name.ToLower().Contains(search) ||
                x.Warehouse.Code.ToLower().Contains(search));
        }

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(x => x.Item.CategoryId == request.CategoryId.Value);
        }

        if (request.ItemId.HasValue)
        {
            query = query.Where(x => x.ItemId == request.ItemId.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "itemcode" => isDesc ? query.OrderByDescending(x => x.Item.ItemCode) : query.OrderBy(x => x.Item.ItemCode),
            "warehousecode" => isDesc ? query.OrderByDescending(x => x.Warehouse.Code) : query.OrderBy(x => x.Warehouse.Code),
            "qtyavailable" => isDesc ? query.OrderByDescending(x => x.QtyAvailable) : query.OrderBy(x => x.QtyAvailable),
            "totalvalue" => isDesc ? query.OrderByDescending(x => x.TotalValue) : query.OrderBy(x => x.TotalValue),
            _ => isDesc ? query.OrderByDescending(x => x.TotalValue) : query.OrderBy(x => x.TotalValue)
        };

        var totalCount = await query.CountAsync(ct);
        var rows = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new InventoryStockSummaryDto
            {
                ItemId = x.ItemId,
                ItemCode = x.Item.ItemCode,
                ItemName = x.Item.Name,
                CategoryName = x.Item.Category.Name,
                WarehouseId = x.WarehouseId,
                WarehouseCode = x.Warehouse.Code,
                WarehouseName = x.Warehouse.Name,
                QtyOnHand = x.QtyOnHand,
                QtyAvailable = x.QtyAvailable,
                AvgCost = x.AvgCost,
                TotalValue = x.TotalValue
            })
            .ToListAsync(ct);

        return Ok(PagedResult<InventoryStockSummaryDto>.Create(rows, totalCount, page, pageSize));
    }

    [HttpGet("stock-by-warehouse")]
    public async Task<IActionResult> StockByWarehouse([FromQuery] InventoryReportRequest request, CancellationToken ct)
    {
        var query = dbContext.InvStockBalances
            .AsNoTracking()
            .Include(x => x.Warehouse)
            .AsQueryable();

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);
        }

        if (request.ItemId.HasValue)
        {
            query = query.Where(x => x.ItemId == request.ItemId.Value);
        }

        var rows = await query
            .GroupBy(x => new { x.WarehouseId, x.Warehouse.Code, x.Warehouse.Name })
            .Select(x => new InventoryStockByWarehouseDto
            {
                WarehouseId = x.Key.WarehouseId,
                WarehouseCode = x.Key.Code,
                WarehouseName = x.Key.Name,
                QtyOnHand = x.Sum(v => v.QtyOnHand),
                QtyAvailable = x.Sum(v => v.QtyAvailable),
                TotalValue = x.Sum(v => v.TotalValue)
            })
            .OrderBy(x => x.WarehouseCode)
            .ToListAsync(ct);

        return Ok(rows);
    }

    [HttpGet("stock-by-category")]
    public async Task<IActionResult> StockByCategory([FromQuery] InventoryReportRequest request, CancellationToken ct)
    {
        var query = dbContext.InvStockBalances
            .AsNoTracking()
            .Include(x => x.Item)
                .ThenInclude(x => x.Category)
            .AsQueryable();

        if (request.CategoryId.HasValue)
        {
            query = query.Where(x => x.Item.CategoryId == request.CategoryId.Value);
        }

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);
        }

        var rows = await query
            .GroupBy(x => new { x.Item.CategoryId, x.Item.Category.Code, x.Item.Category.Name })
            .Select(x => new InventoryStockByCategoryDto
            {
                CategoryId = x.Key.CategoryId,
                CategoryCode = x.Key.Code,
                CategoryName = x.Key.Name,
                QtyOnHand = x.Sum(v => v.QtyOnHand),
                QtyAvailable = x.Sum(v => v.QtyAvailable),
                TotalValue = x.Sum(v => v.TotalValue)
            })
            .OrderBy(x => x.CategoryCode)
            .ToListAsync(ct);

        return Ok(rows);
    }

    [HttpGet("stock-card")]
    public async Task<IActionResult> StockCard([FromQuery] InventoryReportRequest request, CancellationToken ct)
        => Ok(await BuildStockCardAsync(request, ct));

    [HttpGet("low-stock")]
    public async Task<IActionResult> LowStock([FromQuery] InventoryReportRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var query = dbContext.InvStockBalances
            .AsNoTracking()
            .Include(x => x.Item)
            .Include(x => x.Warehouse)
            .Where(x => x.QtyAvailable <= x.Item.MinStock)
            .AsQueryable();

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(x => x.Item.CategoryId == request.CategoryId.Value);
        }

        if (request.ItemId.HasValue)
        {
            query = query.Where(x => x.ItemId == request.ItemId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Item.ItemCode.ToLower().Contains(search) ||
                x.Item.Name.ToLower().Contains(search) ||
                x.Warehouse.Code.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(ct);
        var rows = await query
            .OrderBy(x => x.Item.ItemCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new InventoryLowStockDto
            {
                ItemId = x.ItemId,
                ItemCode = x.Item.ItemCode,
                ItemName = x.Item.Name,
                WarehouseCode = x.Warehouse.Code,
                QtyAvailable = x.QtyAvailable,
                MinStock = x.Item.MinStock,
                Difference = x.QtyAvailable - x.Item.MinStock
            })
            .ToListAsync(ct);

        return Ok(PagedResult<InventoryLowStockDto>.Create(rows, totalCount, page, pageSize));
    }

    [HttpGet("inventory-valuation")]
    public async Task<IActionResult> InventoryValuation([FromQuery] InventoryReportRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var query = dbContext.InvStockBalances
            .AsNoTracking()
            .Include(x => x.Item)
            .Include(x => x.Warehouse)
            .AsQueryable();

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(x => x.Item.CategoryId == request.CategoryId.Value);
        }

        if (request.ItemId.HasValue)
        {
            query = query.Where(x => x.ItemId == request.ItemId.Value);
        }

        var totalCount = await query.CountAsync(ct);
        var rows = await query
            .OrderByDescending(x => x.TotalValue)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new InventoryValuationDto
            {
                ItemId = x.ItemId,
                ItemCode = x.Item.ItemCode,
                ItemName = x.Item.Name,
                WarehouseCode = x.Warehouse.Code,
                QtyOnHand = x.QtyOnHand,
                AvgCost = x.AvgCost,
                TotalValue = x.TotalValue
            })
            .ToListAsync(ct);

        return Ok(PagedResult<InventoryValuationDto>.Create(rows, totalCount, page, pageSize));
    }

    [HttpGet("inventory-aging")]
    public async Task<IActionResult> InventoryAging([FromQuery] InventoryReportRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var threshold = request.ThresholdDays.GetValueOrDefault(0);

        var query = dbContext.InvStockBalances
            .AsNoTracking()
            .Include(x => x.Item)
            .Include(x => x.Warehouse)
            .Where(x => x.QtyOnHand > 0)
            .AsQueryable();

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(x => x.Item.CategoryId == request.CategoryId.Value);
        }

        var rowsQuery = query.Select(x => new InventoryAgingDto
        {
            ItemId = x.ItemId,
            ItemCode = x.Item.ItemCode,
            ItemName = x.Item.Name,
            WarehouseCode = x.Warehouse.Code,
            LastMovementDate = x.LastMovementAt.HasValue ? DateOnly.FromDateTime(x.LastMovementAt.Value.UtcDateTime.Date) : null,
            DaysSinceLastMovement = x.LastMovementAt.HasValue ? (today.DayNumber - DateOnly.FromDateTime(x.LastMovementAt.Value.UtcDateTime.Date).DayNumber) : 9999,
            QtyOnHand = x.QtyOnHand,
            TotalValue = x.TotalValue
        });

        if (threshold > 0)
        {
            rowsQuery = rowsQuery.Where(x => x.DaysSinceLastMovement >= threshold);
        }

        var totalCount = await rowsQuery.CountAsync(ct);
        var rows = await rowsQuery
            .OrderByDescending(x => x.DaysSinceLastMovement)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return Ok(PagedResult<InventoryAgingDto>.Create(rows, totalCount, page, pageSize));
    }

    [HttpGet("movement-history")]
    public async Task<IActionResult> MovementHistory([FromQuery] InventoryReportRequest request, CancellationToken ct)
        => Ok(await BuildMovementHistoryAsync(request, ct));

    [HttpGet("receipt-summary")]
    public async Task<IActionResult> ReceiptSummary([FromQuery] InventoryReportRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var query = dbContext.InvGoodsReceipts
            .AsNoTracking()
            .Include(x => x.Warehouse)
            .AsQueryable();

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);
        }

        if (request.DateFrom.HasValue)
        {
            query = query.Where(x => x.ReceiptDate >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(x => x.ReceiptDate <= request.DateTo.Value);
        }

        var totalCount = await query.CountAsync(ct);
        var rows = await query
            .OrderByDescending(x => x.ReceiptDate)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new InventoryReceiptSummaryDto
            {
                Id = x.Id,
                ReceiptNo = x.ReceiptNo,
                ReceiptDate = x.ReceiptDate,
                WarehouseCode = x.Warehouse.Code,
                ReceiptType = x.ReceiptType,
                Status = x.Status,
                TotalQuantity = x.Lines.Sum(l => l.QtyBase),
                TotalCost = x.Lines.Sum(l => l.TotalCost)
            })
            .ToListAsync(ct);

        return Ok(PagedResult<InventoryReceiptSummaryDto>.Create(rows, totalCount, page, pageSize));
    }

    [HttpGet("issue-summary")]
    public async Task<IActionResult> IssueSummary([FromQuery] InventoryReportRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var query = dbContext.InvGoodsIssues
            .AsNoTracking()
            .Include(x => x.Warehouse)
            .AsQueryable();

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);
        }

        if (request.DateFrom.HasValue)
        {
            query = query.Where(x => x.IssueDate >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(x => x.IssueDate <= request.DateTo.Value);
        }

        var totalCount = await query.CountAsync(ct);
        var rows = await query
            .OrderByDescending(x => x.IssueDate)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new InventoryIssueSummaryDto
            {
                Id = x.Id,
                IssueNo = x.IssueNo,
                IssueDate = x.IssueDate,
                WarehouseCode = x.Warehouse.Code,
                IssueType = x.IssueType,
                Status = x.Status,
                TotalQuantity = x.Lines.Sum(l => l.QtyBase),
                TotalCost = x.Lines.Sum(l => l.TotalCost)
            })
            .ToListAsync(ct);

        return Ok(PagedResult<InventoryIssueSummaryDto>.Create(rows, totalCount, page, pageSize));
    }

    [HttpGet("transfer-summary")]
    public async Task<IActionResult> TransferSummary([FromQuery] InventoryReportRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var query = dbContext.InvStockTransfers
            .AsNoTracking()
            .Include(x => x.FromWarehouse)
            .Include(x => x.ToWarehouse)
            .AsQueryable();

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(x => x.FromWarehouseId == request.WarehouseId.Value || x.ToWarehouseId == request.WarehouseId.Value);
        }

        if (request.DateFrom.HasValue)
        {
            query = query.Where(x => x.TransferDate >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(x => x.TransferDate <= request.DateTo.Value);
        }

        var totalCount = await query.CountAsync(ct);
        var rows = await query
            .OrderByDescending(x => x.TransferDate)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new InventoryTransferSummaryDto
            {
                Id = x.Id,
                TransferNo = x.TransferNo,
                TransferDate = x.TransferDate,
                FromWarehouseCode = x.FromWarehouse.Code,
                ToWarehouseCode = x.ToWarehouse.Code,
                Status = x.Status,
                TotalQuantity = x.Lines.Sum(l => l.QtyBase),
                TotalCost = x.Lines.Sum(l => l.TotalCost)
            })
            .ToListAsync(ct);

        return Ok(PagedResult<InventoryTransferSummaryDto>.Create(rows, totalCount, page, pageSize));
    }

    [HttpGet("adjustment-summary")]
    public async Task<IActionResult> AdjustmentSummary([FromQuery] InventoryReportRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var query = dbContext.InvStockAdjustments
            .AsNoTracking()
            .Include(x => x.Warehouse)
            .AsQueryable();

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);
        }

        if (request.DateFrom.HasValue)
        {
            query = query.Where(x => x.AdjustmentDate >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(x => x.AdjustmentDate <= request.DateTo.Value);
        }

        var totalCount = await query.CountAsync(ct);
        var rows = await query
            .OrderByDescending(x => x.AdjustmentDate)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new InventoryAdjustmentSummaryDto
            {
                Id = x.Id,
                AdjustmentNo = x.AdjustmentNo,
                AdjustmentDate = x.AdjustmentDate,
                WarehouseCode = x.Warehouse.Code,
                Reason = x.Reason,
                Status = x.Status,
                TotalQuantity = x.Lines.Sum(l => Math.Abs(l.QtyAdjustment)),
                TotalCost = x.Lines.Sum(l => l.TotalCost)
            })
            .ToListAsync(ct);

        return Ok(PagedResult<InventoryAdjustmentSummaryDto>.Create(rows, totalCount, page, pageSize));
    }

    [HttpGet("stock-card/export")]
    public IActionResult StockCardExport() => Ok(new { message = "Export is not implemented yet." });

    [HttpGet("inventory-valuation/export")]
    public IActionResult InventoryValuationExport() => Ok(new { message = "Export is not implemented yet." });

    [HttpGet("low-stock/export")]
    public IActionResult LowStockExport() => Ok(new { message = "Export is not implemented yet." });

    private async Task<PagedResult<InventoryStockCardDto>> BuildStockCardAsync(InventoryReportRequest request, CancellationToken ct)
    {
        var movementResult = await BuildMovementHistoryAsync(request, ct);
        var mapped = movementResult.Items
            .Select(x => new InventoryStockCardDto
            {
                MovementDate = x.MovementDate,
                ItemCode = x.ItemCode,
                ItemName = x.ItemName,
                WarehouseCode = x.WarehouseCode,
                LocationCode = x.LocationCode,
                MovementType = x.MovementType,
                QtyIn = x.QtyIn,
                QtyOut = x.QtyOut,
                QtyBalance = x.QtyBalance,
                UnitCost = x.UnitCost,
                TotalCost = x.TotalCost,
                SourceTable = x.SourceTable,
                SourceId = x.SourceId,
                Notes = x.Notes
            })
            .ToList();

        return PagedResult<InventoryStockCardDto>.Create(mapped, movementResult.TotalCount, movementResult.Page, movementResult.PageSize);
    }

    private async Task<PagedResult<InventoryMovementHistoryDto>> BuildMovementHistoryAsync(InventoryReportRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.InvStockMovements
            .AsNoTracking()
            .Include(x => x.Item)
            .Include(x => x.Warehouse)
            .Include(x => x.Location)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Item.ItemCode.ToLower().Contains(search) ||
                x.Item.Name.ToLower().Contains(search) ||
                x.Warehouse.Code.ToLower().Contains(search) ||
                x.SourceTable.ToLower().Contains(search));
        }

        if (request.ItemId.HasValue)
        {
            query = query.Where(x => x.ItemId == request.ItemId.Value);
        }

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);
        }

        if (request.DateFrom.HasValue)
        {
            query = query.Where(x => x.MovementDate >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(x => x.MovementDate <= request.DateTo.Value);
        }

        if (request.MovementType.HasValue)
        {
            query = query.Where(x => x.MovementType == request.MovementType.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "movementdate" => isDesc ? query.OrderByDescending(x => x.MovementDate).ThenByDescending(x => x.Id) : query.OrderBy(x => x.MovementDate).ThenBy(x => x.Id),
            "itemcode" => isDesc ? query.OrderByDescending(x => x.Item.ItemCode) : query.OrderBy(x => x.Item.ItemCode),
            "warehousecode" => isDesc ? query.OrderByDescending(x => x.Warehouse.Code) : query.OrderBy(x => x.Warehouse.Code),
            "movementtype" => isDesc ? query.OrderByDescending(x => x.MovementType) : query.OrderBy(x => x.MovementType),
            _ => isDesc ? query.OrderByDescending(x => x.MovementDate).ThenByDescending(x => x.Id) : query.OrderBy(x => x.MovementDate).ThenBy(x => x.Id)
        };

        var totalCount = await query.CountAsync(ct);
        var rows = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new InventoryMovementHistoryDto
            {
                MovementDate = x.MovementDate,
                ItemCode = x.Item.ItemCode,
                ItemName = x.Item.Name,
                WarehouseCode = x.Warehouse.Code,
                LocationCode = x.Location != null ? x.Location.Code : null,
                MovementType = x.MovementType,
                QtyIn = x.QtyIn,
                QtyOut = x.QtyOut,
                QtyBalance = x.QtyBalance,
                UnitCost = x.UnitCost,
                TotalCost = x.TotalCost,
                SourceTable = x.SourceTable,
                SourceId = x.SourceId,
                Notes = x.Notes
            })
            .ToListAsync(ct);

        return PagedResult<InventoryMovementHistoryDto>.Create(rows, totalCount, page, pageSize);
    }
}
