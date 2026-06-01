using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;
using ERP.Domain.Entities.Inventory;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Inventory;

[Route("api/v1/inventory/warehouses")]
public sealed class WarehousesController(AppDbContext dbContext) : InventoryControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] WarehousePagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.InvWarehouses
            .AsNoTracking()
            .Include(x => x.Manager)
            .Include(x => x.CostCenter)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(search) ||
                x.Name.ToLower().Contains(search) ||
                (x.Address != null && x.Address.ToLower().Contains(search)) ||
                (x.Manager != null && x.Manager.FullName.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var code = request.Code.Trim().ToLowerInvariant();
            query = query.Where(x => x.Code.ToLower().Contains(code));
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var name = request.Name.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(name));
        }

        if (request.ManagerId.HasValue)
        {
            query = query.Where(x => x.ManagerId == request.ManagerId.Value);
        }

        if (request.CostCenterId.HasValue)
        {
            query = query.Where(x => x.CostCenterId == request.CostCenterId.Value);
        }

        if (request.IsTransit.HasValue)
        {
            query = query.Where(x => x.IsTransit == request.IsTransit.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "name" => isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "managername" => isDesc ? query.OrderByDescending(x => x.Manager!.FullName).ThenByDescending(x => x.Code) : query.OrderBy(x => x.Manager!.FullName).ThenBy(x => x.Code),
            "costcentercode" => isDesc ? query.OrderByDescending(x => x.CostCenter!.Code).ThenByDescending(x => x.Code) : query.OrderBy(x => x.CostCenter!.Code).ThenBy(x => x.Code),
            "istransit" => isDesc ? query.OrderByDescending(x => x.IsTransit).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsTransit).ThenBy(x => x.Code),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Code),
            _ => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapDto(x))
            .ToListAsync(ct);

        return Ok(PagedResult<WarehouseDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("options")]
    public async Task<IActionResult> GetOptions(CancellationToken ct)
    {
        var options = await dbContext.InvWarehouses
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new InventoryOptionDto
            {
                Id = x.Id,
                Label = x.Code + " - " + x.Name
            })
            .ToListAsync(ct);

        return Ok(options);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.InvWarehouses
            .AsNoTracking()
            .Include(x => x.Manager)
            .Include(x => x.CostCenter)
            .Where(x => x.Id == id)
            .Select(x => MapDto(x))
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] WarehouseDto request, CancellationToken ct)
    {
        try
        {
            var normalizedCode = NormalizeRequired(request.Code, "Warehouse code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Warehouse name is required.");

            await ValidateWarehouseDependenciesAsync(request, ct);

            var duplicate = await dbContext.InvWarehouses
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Code == normalizedCode, ct);
            if (duplicate)
            {
                return BadRequest(new { message = "Warehouse code already exists." });
            }

            var entity = new InvWarehouse
            {
                Code = normalizedCode,
                Name = normalizedName,
                Description = NormalizeOptional(request.Description),
                Address = NormalizeOptional(request.Address),
                Phone = NormalizeOptional(request.Phone),
                ManagerId = request.ManagerId is > 0 ? request.ManagerId : null,
                CostCenterId = request.CostCenterId is > 0 ? request.CostCenterId : null,
                IsTransit = request.IsTransit,
                IsActive = request.IsActive,
                CreatedBy = "system"
            };

            dbContext.InvWarehouses.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var created = await dbContext.InvWarehouses
                .AsNoTracking()
                .Include(x => x.Manager)
                .Include(x => x.CostCenter)
                .Where(x => x.Id == entity.Id)
                .Select(x => MapDto(x))
                .FirstAsync(ct);

            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] WarehouseDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.InvWarehouses.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            var normalizedCode = NormalizeRequired(request.Code, "Warehouse code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Warehouse name is required.");

            await ValidateWarehouseDependenciesAsync(request, ct);

            var duplicate = await dbContext.InvWarehouses
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Id != id && x.Code == normalizedCode, ct);
            if (duplicate)
            {
                return BadRequest(new { message = "Warehouse code already exists." });
            }

            entity.Code = normalizedCode;
            entity.Name = normalizedName;
            entity.Description = NormalizeOptional(request.Description);
            entity.Address = NormalizeOptional(request.Address);
            entity.Phone = NormalizeOptional(request.Phone);
            entity.ManagerId = request.ManagerId is > 0 ? request.ManagerId : null;
            entity.CostCenterId = request.CostCenterId is > 0 ? request.CostCenterId : null;
            entity.IsTransit = request.IsTransit;
            entity.IsActive = request.IsActive;
            entity.UpdatedBy = "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(ct);

            var updated = await dbContext.InvWarehouses
                .AsNoTracking()
                .Include(x => x.Manager)
                .Include(x => x.CostCenter)
                .Where(x => x.Id == entity.Id)
                .Select(x => MapDto(x))
                .FirstAsync(ct);

            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var entity = await dbContext.InvWarehouses.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        var hasStock = await dbContext.InvStockBalances.AnyAsync(x => x.WarehouseId == id, ct);
        if (hasStock)
        {
            return BadRequest(new { message = "Warehouse cannot be deleted because it has stock balances." });
        }

        dbContext.InvWarehouses.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpGet("{warehouseId:int}/locations")]
    public async Task<IActionResult> GetLocations(int warehouseId, [FromQuery] WarehouseLocationPagedRequest request, CancellationToken ct)
    {
        var warehouseExists = await dbContext.InvWarehouses.AnyAsync(x => x.Id == warehouseId, ct);
        if (!warehouseExists)
        {
            return NotFound();
        }

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.InvWarehouseLocations
            .AsNoTracking()
            .Include(x => x.Warehouse)
            .Where(x => x.WarehouseId == warehouseId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(search) ||
                x.Name.ToLower().Contains(search) ||
                (x.Description != null && x.Description.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var code = request.Code.Trim().ToLowerInvariant();
            query = query.Where(x => x.Code.ToLower().Contains(code));
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var name = request.Name.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(name));
        }

        if (request.IsDefault.HasValue)
        {
            query = query.Where(x => x.IsDefault == request.IsDefault.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "name" => isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "isdefault" => isDesc ? query.OrderByDescending(x => x.IsDefault).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsDefault).ThenBy(x => x.Code),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Code),
            _ => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapLocationDto(x))
            .ToListAsync(ct);

        return Ok(PagedResult<WarehouseLocationDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{warehouseId:int}/locations/options")]
    public async Task<IActionResult> GetLocationOptions(int warehouseId, CancellationToken ct)
    {
        var options = await dbContext.InvWarehouseLocations
            .AsNoTracking()
            .Where(x => x.WarehouseId == warehouseId && x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new InventoryOptionDto
            {
                Id = x.Id,
                Label = x.Code + " - " + x.Name
            })
            .ToListAsync(ct);

        return Ok(options);
    }

    [HttpGet("{warehouseId:int}/locations/{id:int}")]
    public async Task<IActionResult> GetLocationById(int warehouseId, int id, CancellationToken ct)
    {
        var item = await dbContext.InvWarehouseLocations
            .AsNoTracking()
            .Include(x => x.Warehouse)
            .Where(x => x.WarehouseId == warehouseId && x.Id == id)
            .Select(x => MapLocationDto(x))
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost("{warehouseId:int}/locations")]
    public async Task<IActionResult> CreateLocation(int warehouseId, [FromBody] WarehouseLocationDto request, CancellationToken ct)
    {
        try
        {
            var warehouseExists = await dbContext.InvWarehouses.AnyAsync(x => x.Id == warehouseId, ct);
            if (!warehouseExists)
            {
                return NotFound();
            }

            var normalizedCode = NormalizeRequired(request.Code, "Location code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Location name is required.");

            var duplicate = await dbContext.InvWarehouseLocations
                .IgnoreQueryFilters()
                .AnyAsync(x => x.WarehouseId == warehouseId && x.Code == normalizedCode, ct);
            if (duplicate)
            {
                return BadRequest(new { message = "Location code already exists in this warehouse." });
            }

            if (request.IsDefault)
            {
                var defaults = await dbContext.InvWarehouseLocations
                    .Where(x => x.WarehouseId == warehouseId && x.IsDefault)
                    .ToListAsync(ct);

                foreach (var currentDefault in defaults)
                {
                    currentDefault.IsDefault = false;
                    currentDefault.UpdatedBy = "system";
                    currentDefault.UpdatedAt = DateTimeOffset.UtcNow;
                }
            }

            var entity = new InvWarehouseLocation
            {
                WarehouseId = warehouseId,
                Code = normalizedCode,
                Name = normalizedName,
                Description = NormalizeOptional(request.Description),
                IsDefault = request.IsDefault,
                IsActive = request.IsActive,
                CreatedBy = "system"
            };

            dbContext.InvWarehouseLocations.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var created = await dbContext.InvWarehouseLocations
                .AsNoTracking()
                .Include(x => x.Warehouse)
                .Where(x => x.Id == entity.Id)
                .Select(x => MapLocationDto(x))
                .FirstAsync(ct);

            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{warehouseId:int}/locations/{id:int}")]
    public async Task<IActionResult> UpdateLocation(int warehouseId, int id, [FromBody] WarehouseLocationDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.InvWarehouseLocations
                .FirstOrDefaultAsync(x => x.WarehouseId == warehouseId && x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            var normalizedCode = NormalizeRequired(request.Code, "Location code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Location name is required.");

            var duplicate = await dbContext.InvWarehouseLocations
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Id != id && x.WarehouseId == warehouseId && x.Code == normalizedCode, ct);
            if (duplicate)
            {
                return BadRequest(new { message = "Location code already exists in this warehouse." });
            }

            if (request.IsDefault)
            {
                var defaults = await dbContext.InvWarehouseLocations
                    .Where(x => x.WarehouseId == warehouseId && x.Id != id && x.IsDefault)
                    .ToListAsync(ct);

                foreach (var currentDefault in defaults)
                {
                    currentDefault.IsDefault = false;
                    currentDefault.UpdatedBy = "system";
                    currentDefault.UpdatedAt = DateTimeOffset.UtcNow;
                }
            }

            entity.Code = normalizedCode;
            entity.Name = normalizedName;
            entity.Description = NormalizeOptional(request.Description);
            entity.IsDefault = request.IsDefault;
            entity.IsActive = request.IsActive;
            entity.UpdatedBy = "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(ct);

            var updated = await dbContext.InvWarehouseLocations
                .AsNoTracking()
                .Include(x => x.Warehouse)
                .Where(x => x.Id == entity.Id)
                .Select(x => MapLocationDto(x))
                .FirstAsync(ct);

            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{warehouseId:int}/locations/{id:int}")]
    public async Task<IActionResult> DeleteLocation(int warehouseId, int id, CancellationToken ct)
    {
        var entity = await dbContext.InvWarehouseLocations
            .FirstOrDefaultAsync(x => x.WarehouseId == warehouseId && x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        var hasStock = await dbContext.InvStockBalances.AnyAsync(x => x.LocationId == id, ct);
        if (hasStock)
        {
            return BadRequest(new { message = "Location cannot be deleted because it has stock balances." });
        }

        dbContext.InvWarehouseLocations.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpGet("{warehouseId:int}/stock")]
    public async Task<IActionResult> GetWarehouseStock(int warehouseId, [FromQuery] StockBalancePagedRequest request, CancellationToken ct)
    {
        var warehouseExists = await dbContext.InvWarehouses.AnyAsync(x => x.Id == warehouseId, ct);
        if (!warehouseExists)
        {
            return NotFound();
        }

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.InvStockBalances
            .AsNoTracking()
            .Include(x => x.Item)
            .Include(x => x.Warehouse)
            .Include(x => x.Location)
            .Where(x => x.WarehouseId == warehouseId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Item.ItemCode.ToLower().Contains(search) ||
                x.Item.Name.ToLower().Contains(search) ||
                (x.Location != null && x.Location.Code.ToLower().Contains(search)));
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
            .Select(x => MapStockDto(x))
            .ToListAsync(ct);

        return Ok(PagedResult<StockBalanceDto>.Create(items, totalCount, page, pageSize));
    }

    private async Task ValidateWarehouseDependenciesAsync(WarehouseDto request, CancellationToken ct)
    {
        if (request.ManagerId is > 0 && !await dbContext.HrEmployees.AnyAsync(x => x.Id == request.ManagerId.Value, ct))
        {
            throw new InvalidOperationException("Manager employee not found.");
        }

        if (request.CostCenterId is > 0 && !await dbContext.FinCostCenters.AnyAsync(x => x.Id == request.CostCenterId.Value, ct))
        {
            throw new InvalidOperationException("Cost center not found.");
        }
    }

    private static WarehouseDto MapDto(InvWarehouse entity)
    {
        return new WarehouseDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            Address = entity.Address,
            Phone = entity.Phone,
            ManagerId = entity.ManagerId,
            ManagerName = entity.Manager?.FullName,
            CostCenterId = entity.CostCenterId,
            CostCenterCode = entity.CostCenter?.Code,
            IsTransit = entity.IsTransit,
            IsActive = entity.IsActive
        };
    }

    private static WarehouseLocationDto MapLocationDto(InvWarehouseLocation entity)
    {
        return new WarehouseLocationDto
        {
            Id = entity.Id,
            WarehouseId = entity.WarehouseId,
            WarehouseCode = entity.Warehouse?.Code ?? string.Empty,
            WarehouseName = entity.Warehouse?.Name ?? string.Empty,
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            IsDefault = entity.IsDefault,
            IsActive = entity.IsActive
        };
    }

    private static StockBalanceDto MapStockDto(InvStockBalance entity)
    {
        return new StockBalanceDto
        {
            Id = entity.Id,
            ItemId = entity.ItemId,
            ItemCode = entity.Item?.ItemCode ?? string.Empty,
            ItemName = entity.Item?.Name ?? string.Empty,
            WarehouseId = entity.WarehouseId,
            WarehouseCode = entity.Warehouse?.Code ?? string.Empty,
            LocationId = entity.LocationId,
            LocationCode = entity.Location?.Code,
            QtyOnHand = entity.QtyOnHand,
            QtyReserved = entity.QtyReserved,
            QtyAvailable = entity.QtyAvailable,
            AvgCost = entity.AvgCost,
            TotalValue = entity.TotalValue
        };
    }

    private static string NormalizeRequired(string? value, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(errorMessage);
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
