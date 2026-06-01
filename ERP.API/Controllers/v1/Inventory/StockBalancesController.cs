using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Inventory;

[Route("api/v1/inventory/stock-balances")]
public sealed class StockBalancesController(AppDbContext dbContext) : InventoryControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] StockBalancePagedRequest request, CancellationToken ct)
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
                x.Warehouse.Code.ToLower().Contains(search) ||
                (x.Location != null && x.Location.Code.ToLower().Contains(search)));
        }

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);
        }

        if (request.ItemId.HasValue)
        {
            query = query.Where(x => x.ItemId == request.ItemId.Value);
        }

        if (request.LocationId.HasValue)
        {
            query = query.Where(x => x.LocationId == request.LocationId.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "itemcode" => isDesc ? query.OrderByDescending(x => x.Item.ItemCode) : query.OrderBy(x => x.Item.ItemCode),
            "warehousecode" => isDesc ? query.OrderByDescending(x => x.Warehouse.Code).ThenByDescending(x => x.Item.ItemCode) : query.OrderBy(x => x.Warehouse.Code).ThenBy(x => x.Item.ItemCode),
            "locationcode" => isDesc ? query.OrderByDescending(x => x.Location!.Code).ThenByDescending(x => x.Item.ItemCode) : query.OrderBy(x => x.Location!.Code).ThenBy(x => x.Item.ItemCode),
            "qtyonhand" => isDesc ? query.OrderByDescending(x => x.QtyOnHand) : query.OrderBy(x => x.QtyOnHand),
            "qtyreserved" => isDesc ? query.OrderByDescending(x => x.QtyReserved) : query.OrderBy(x => x.QtyReserved),
            "qtyavailable" => isDesc ? query.OrderByDescending(x => x.QtyAvailable) : query.OrderBy(x => x.QtyAvailable),
            "avgcost" => isDesc ? query.OrderByDescending(x => x.AvgCost) : query.OrderBy(x => x.AvgCost),
            "totalvalue" => isDesc ? query.OrderByDescending(x => x.TotalValue) : query.OrderBy(x => x.TotalValue),
            _ => isDesc ? query.OrderByDescending(x => x.Item.ItemCode) : query.OrderBy(x => x.Item.ItemCode)
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
}
