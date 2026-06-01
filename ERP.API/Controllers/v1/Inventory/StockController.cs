using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;
using ERP.Domain.Enums.Inventory;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Inventory;

[Route("api/v1/inventory/stock")]
public sealed class StockController(AppDbContext dbContext) : InventoryControllerBase
{
    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance([FromQuery] StockBalancePagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.InvStockBalances
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
                x.Warehouse.Code.ToLower().Contains(search));
        }

        if (request.ItemId.HasValue)
        {
            query = query.Where(x => x.ItemId == request.ItemId.Value);
        }

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);
        }

        if (request.LocationId.HasValue)
        {
            query = query.Where(x => x.LocationId == request.LocationId.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "itemcode" => isDesc ? query.OrderByDescending(x => x.Item.ItemCode) : query.OrderBy(x => x.Item.ItemCode),
            "warehousecode" => isDesc ? query.OrderByDescending(x => x.Warehouse.Code) : query.OrderBy(x => x.Warehouse.Code),
            "qtyonhand" => isDesc ? query.OrderByDescending(x => x.QtyOnHand) : query.OrderBy(x => x.QtyOnHand),
            "qtyavailable" => isDesc ? query.OrderByDescending(x => x.QtyAvailable) : query.OrderBy(x => x.QtyAvailable),
            "totalvalue" => isDesc ? query.OrderByDescending(x => x.TotalValue) : query.OrderBy(x => x.TotalValue),
            _ => isDesc ? query.OrderByDescending(x => x.LastMovementAt) : query.OrderBy(x => x.LastMovementAt)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new StockBalanceDto
            {
                Id = x.Id,
                ItemId = x.ItemId,
                ItemCode = x.Item.ItemCode,
                ItemName = x.Item.Name,
                WarehouseId = x.WarehouseId,
                WarehouseCode = x.Warehouse.Code,
                LocationId = x.LocationId,
                LocationCode = x.Location != null ? x.Location.Code : null,
                QtyOnHand = x.QtyOnHand,
                QtyReserved = x.QtyReserved,
                QtyAvailable = x.QtyAvailable,
                AvgCost = x.AvgCost,
                TotalValue = x.TotalValue
            })
            .ToListAsync(ct);

        return Ok(PagedResult<StockBalanceDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable([FromQuery] int itemId, [FromQuery] int warehouseId, [FromQuery] int? locationId, CancellationToken ct)
    {
        if (itemId <= 0 || warehouseId <= 0)
        {
            return BadRequest(new { message = "itemId and warehouseId are required." });
        }

        var available = await InventoryStockMutationHelper.GetAvailableAsync(dbContext, itemId, warehouseId, locationId, ct);
        return Ok(new { itemId, warehouseId, locationId, qtyAvailable = available });
    }

    [HttpGet("movements")]
    public async Task<IActionResult> GetMovements([FromQuery] InventoryReportRequest request, CancellationToken ct)
    {
        var result = await GetMovementHistoryInternalAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("card")]
    public async Task<IActionResult> GetStockCard([FromQuery] InventoryReportRequest request, CancellationToken ct)
    {
        var result = await GetMovementHistoryInternalAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("valuation")]
    public async Task<IActionResult> GetValuation([FromQuery] InventoryReportRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.InvStockBalances
            .AsNoTracking()
            .Include(x => x.Item)
            .Include(x => x.Warehouse)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Item.ItemCode.ToLower().Contains(search) ||
                x.Item.Name.ToLower().Contains(search) ||
                x.Warehouse.Code.ToLower().Contains(search));
        }

        if (request.ItemId.HasValue)
        {
            query = query.Where(x => x.ItemId == request.ItemId.Value);
        }

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(x => x.Item.CategoryId == request.CategoryId.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "itemcode" => isDesc ? query.OrderByDescending(x => x.Item.ItemCode) : query.OrderBy(x => x.Item.ItemCode),
            "warehousecode" => isDesc ? query.OrderByDescending(x => x.Warehouse.Code) : query.OrderBy(x => x.Warehouse.Code),
            "qtyonhand" => isDesc ? query.OrderByDescending(x => x.QtyOnHand) : query.OrderBy(x => x.QtyOnHand),
            "totalvalue" => isDesc ? query.OrderByDescending(x => x.TotalValue) : query.OrderBy(x => x.TotalValue),
            _ => isDesc ? query.OrderByDescending(x => x.TotalValue) : query.OrderBy(x => x.TotalValue)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
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

        return Ok(PagedResult<InventoryValuationDto>.Create(items, totalCount, page, pageSize));
    }

    private async Task<PagedResult<InventoryMovementHistoryDto>> GetMovementHistoryInternalAsync(InventoryReportRequest request, CancellationToken ct)
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
        var items = await query
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

        return PagedResult<InventoryMovementHistoryDto>.Create(items, totalCount, page, pageSize);
    }
}
