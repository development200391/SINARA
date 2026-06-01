using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;
using ERP.Domain.Entities.Inventory;
using ERP.Domain.Enums.Inventory;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Inventory;

[Route("api/v1/inventory/opnames")]
public sealed class OpnamesController(AppDbContext dbContext) : InventoryControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] StockOpnamePagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.InvStockOpnames
            .AsNoTracking()
            .Include(x => x.Warehouse)
            .Include(x => x.Location)
            .Include(x => x.ApprovedByUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.OpnameNo.ToLower().Contains(search) ||
                x.Warehouse.Code.ToLower().Contains(search) ||
                x.Warehouse.Name.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.OpnameNo))
        {
            var opnameNo = request.OpnameNo.Trim().ToLowerInvariant();
            query = query.Where(x => x.OpnameNo.ToLower().Contains(opnameNo));
        }

        if (request.DateFrom.HasValue)
        {
            query = query.Where(x => x.OpnameDate >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(x => x.OpnameDate <= request.DateTo.Value);
        }

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "opnameno" => isDesc ? query.OrderByDescending(x => x.OpnameNo) : query.OrderBy(x => x.OpnameNo),
            "opnamedate" => isDesc ? query.OrderByDescending(x => x.OpnameDate) : query.OrderBy(x => x.OpnameDate),
            "warehousecode" => isDesc ? query.OrderByDescending(x => x.Warehouse.Code) : query.OrderBy(x => x.Warehouse.Code),
            "status" => isDesc ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            _ => isDesc ? query.OrderByDescending(x => x.OpnameDate).ThenByDescending(x => x.OpnameNo) : query.OrderBy(x => x.OpnameDate).ThenBy(x => x.OpnameNo)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new StockOpnameDto
            {
                Id = x.Id,
                OpnameNo = x.OpnameNo,
                OpnameDate = x.OpnameDate,
                WarehouseId = x.WarehouseId,
                WarehouseCode = x.Warehouse.Code,
                LocationId = x.LocationId,
                LocationCode = x.Location != null ? x.Location.Code : null,
                Description = x.Description,
                Status = x.Status,
                ApprovedBy = x.ApprovedBy,
                ApprovedByName = x.ApprovedByUser != null ? x.ApprovedByUser.FullName : null,
                ApprovedAt = x.ApprovedAt,
                AdjustmentId = x.AdjustmentId,
                TotalVarianceValue = x.Lines.Sum(l => l.TotalVarianceValue),
            })
            .ToListAsync(ct);

        return Ok(PagedResult<StockOpnameDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var entity = await dbContext.InvStockOpnames
            .AsNoTracking()
            .Include(x => x.Warehouse)
            .Include(x => x.Location)
            .Include(x => x.ApprovedByUser)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Item)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Location)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return NotFound();
        }

        return Ok(MapDto(entity, includeLines: true));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StockOpnameDto request, CancellationToken ct)
    {
        try
        {
            await ValidateHeaderAsync(request.WarehouseId, request.LocationId, ct);

            var entity = new InvStockOpname
            {
                OpnameNo = await GenerateOpnameNoAsync(request.OpnameDate, ct),
                OpnameDate = request.OpnameDate,
                WarehouseId = request.WarehouseId,
                LocationId = request.LocationId,
                Description = NormalizeOptional(request.Description),
                Status = OpnameStatus.Draft,
                CountedBy = GetCurrentUserId(),
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            };

            dbContext.InvStockOpnames.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var created = await dbContext.InvStockOpnames
                .AsNoTracking()
                .Include(x => x.Warehouse)
                .Include(x => x.Location)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.Item)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.Location)
                .FirstAsync(x => x.Id == entity.Id, ct);

            return Ok(MapDto(created, includeLines: true));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] StockOpnameDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.InvStockOpnames.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            if (entity.Status != OpnameStatus.Draft)
            {
                return BadRequest(new { message = "Only draft opname can be edited." });
            }

            await ValidateHeaderAsync(request.WarehouseId, request.LocationId, ct);

            entity.OpnameDate = request.OpnameDate;
            entity.WarehouseId = request.WarehouseId;
            entity.LocationId = request.LocationId;
            entity.Description = NormalizeOptional(request.Description);
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(ct);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/start")]
    public async Task<IActionResult> Start(int id, CancellationToken ct)
    {
        var entity = await dbContext.InvStockOpnames
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != OpnameStatus.Draft)
        {
            return BadRequest(new { message = "Only draft opname can be started." });
        }

        if (entity.Lines.Count == 0)
        {
            var balanceQuery = dbContext.InvStockBalances
                .AsNoTracking()
                .Where(x => x.WarehouseId == entity.WarehouseId);

            if (entity.LocationId.HasValue)
            {
                balanceQuery = balanceQuery.Where(x => x.LocationId == entity.LocationId.Value);
            }

            var balances = await balanceQuery
                .OrderBy(x => x.ItemId)
                .ThenBy(x => x.LocationId)
                .ToListAsync(ct);

            if (balances.Count == 0)
            {
                return BadRequest(new { message = "No stock balances found to start opname." });
            }

            var lineNo = 1;
            foreach (var balance in balances)
            {
                entity.Lines.Add(new InvStockOpnameLine
                {
                    LineNo = lineNo++,
                    ItemId = balance.ItemId,
                    LocationId = balance.LocationId,
                    QtySystem = balance.QtyOnHand,
                    QtyCounted = balance.QtyOnHand,
                    UnitCost = balance.AvgCost
                });
            }
        }

        entity.Status = OpnameStatus.InProgress;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("{id:int}/lines")]
    public async Task<IActionResult> GetLines(int id, CancellationToken ct)
    {
        var exists = await dbContext.InvStockOpnames.AsNoTracking().AnyAsync(x => x.Id == id, ct);
        if (!exists)
        {
            return NotFound();
        }

        var lines = await dbContext.InvStockOpnameLines
            .AsNoTracking()
            .Include(x => x.Item)
            .Include(x => x.Location)
            .Where(x => x.StockOpnameId == id)
            .OrderBy(x => x.LineNo)
            .Select(x => new StockOpnameLineDto
            {
                Id = x.Id,
                LineNo = x.LineNo,
                ItemId = x.ItemId,
                ItemCode = x.Item.ItemCode,
                ItemName = x.Item.Name,
                LocationId = x.LocationId,
                LocationCode = x.Location != null ? x.Location.Code : null,
                QtySystem = x.QtySystem,
                QtyCounted = x.QtyCounted,
                QtyVariance = x.QtyVariance,
                UnitCost = x.UnitCost,
                TotalVarianceValue = x.TotalVarianceValue,
                Notes = x.Notes
            })
            .ToListAsync(ct);

        return Ok(lines);
    }

    [HttpPut("{id:int}/lines")]
    public async Task<IActionResult> UpdateLines(int id, [FromBody] List<StockOpnameLineDto> lines, CancellationToken ct)
    {
        var entity = await dbContext.InvStockOpnames
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != OpnameStatus.InProgress)
        {
            return BadRequest(new { message = "Only in-progress opname can update lines." });
        }

        foreach (var input in lines)
        {
            var line = entity.Lines.FirstOrDefault(x => x.Id == input.Id);
            if (line is null)
            {
                continue;
            }

            if (input.QtyCounted < 0)
            {
                return BadRequest(new { message = $"Qty counted cannot be negative on line {line.LineNo}." });
            }

            line.QtyCounted = decimal.Round(input.QtyCounted, 4, MidpointRounding.AwayFromZero);
            line.Notes = NormalizeOptional(input.Notes);
        }

        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPut("{id:int}/lines/{lineId:int}")]
    public async Task<IActionResult> UpdateLine(int id, int lineId, [FromBody] StockOpnameLineDto input, CancellationToken ct)
    {
        var entity = await dbContext.InvStockOpnames.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != OpnameStatus.InProgress)
        {
            return BadRequest(new { message = "Only in-progress opname can update lines." });
        }

        var line = await dbContext.InvStockOpnameLines.FirstOrDefaultAsync(x => x.StockOpnameId == id && x.Id == lineId, ct);
        if (line is null)
        {
            return NotFound();
        }

        if (input.QtyCounted < 0)
        {
            return BadRequest(new { message = "Qty counted cannot be negative." });
        }

        line.QtyCounted = decimal.Round(input.QtyCounted, 4, MidpointRounding.AwayFromZero);
        line.Notes = NormalizeOptional(input.Notes);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPut("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id, CancellationToken ct)
    {
        var entity = await dbContext.InvStockOpnames
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != OpnameStatus.InProgress)
        {
            return BadRequest(new { message = "Only in-progress opname can be completed." });
        }

        if (entity.Lines.Count == 0)
        {
            return BadRequest(new { message = "Opname lines are required before completion." });
        }

        entity.Status = OpnameStatus.Completed;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPut("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        var entity = await dbContext.InvStockOpnames
            .Include(x => x.Warehouse)
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != OpnameStatus.Completed)
        {
            return BadRequest(new { message = "Only completed opname can be approved." });
        }

        if (entity.AdjustmentId.HasValue)
        {
            return BadRequest(new { message = "Opname has already generated adjustment." });
        }

        var varianceLines = entity.Lines
            .Where(x => x.QtyVariance != 0)
            .ToList();

        if (varianceLines.Count > 0)
        {
            var adjustment = new InvStockAdjustment
            {
                AdjustmentNo = await GenerateAdjustmentNoAsync(entity.OpnameDate, ct),
                AdjustmentDate = entity.OpnameDate,
                WarehouseId = entity.WarehouseId,
                LocationId = entity.LocationId,
                Reason = AdjustmentReason.DataCorrection,
                ReferenceNo = entity.OpnameNo,
                Description = $"Generated from opname {entity.OpnameNo}",
                Status = TransactionStatus.Draft,
                RequestedBy = GetCurrentUserId(),
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            };

            var lineNo = 1;
            foreach (var variance in varianceLines)
            {
                adjustment.Lines.Add(new InvStockAdjustmentLine
                {
                    LineNo = lineNo++,
                    ItemId = variance.ItemId,
                    UomId = null,
                    QtyAdjustment = variance.QtyVariance,
                    UnitCost = variance.UnitCost,
                    Notes = variance.Notes
                });
            }

            dbContext.InvStockAdjustments.Add(adjustment);
            await dbContext.SaveChangesAsync(ct);

            entity.AdjustmentId = adjustment.Id;
        }

        entity.ApprovedBy = GetCurrentUserId();
        entity.ApprovedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        var entity = await dbContext.InvStockOpnames.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status is not OpnameStatus.Draft and not OpnameStatus.InProgress)
        {
            return BadRequest(new { message = "Only draft or in-progress opname can be cancelled." });
        }

        entity.Status = OpnameStatus.Cancelled;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task ValidateHeaderAsync(int warehouseId, int? locationId, CancellationToken ct)
    {
        if (warehouseId <= 0)
        {
            throw new InvalidOperationException("Warehouse is required.");
        }

        var warehouseExists = await dbContext.InvWarehouses.AnyAsync(x => x.Id == warehouseId, ct);
        if (!warehouseExists)
        {
            throw new InvalidOperationException("Warehouse not found.");
        }

        if (!locationId.HasValue)
        {
            return;
        }

        var locationExists = await dbContext.InvWarehouseLocations
            .AnyAsync(x => x.Id == locationId.Value && x.WarehouseId == warehouseId, ct);

        if (!locationExists)
        {
            throw new InvalidOperationException("Location not found in selected warehouse.");
        }
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private async Task<string> GenerateOpnameNoAsync(DateOnly opnameDate, CancellationToken ct)
    {
        var prefix = $"OPN-{opnameDate:yyyyMM}-";
        var latest = await dbContext.InvStockOpnames
            .IgnoreQueryFilters()
            .Where(x => x.OpnameNo.StartsWith(prefix))
            .OrderByDescending(x => x.OpnameNo)
            .Select(x => x.OpnameNo)
            .FirstOrDefaultAsync(ct);

        var next = 1;
        if (!string.IsNullOrWhiteSpace(latest) && latest.Length >= prefix.Length + 4)
        {
            var suffix = latest[prefix.Length..];
            if (int.TryParse(suffix, out var parsed))
            {
                next = parsed + 1;
            }
        }

        return $"{prefix}{next:0000}";
    }

    private async Task<string> GenerateAdjustmentNoAsync(DateOnly adjustmentDate, CancellationToken ct)
    {
        var prefix = $"ADJ-{adjustmentDate:yyyyMM}-";
        var latest = await dbContext.InvStockAdjustments
            .IgnoreQueryFilters()
            .Where(x => x.AdjustmentNo.StartsWith(prefix))
            .OrderByDescending(x => x.AdjustmentNo)
            .Select(x => x.AdjustmentNo)
            .FirstOrDefaultAsync(ct);

        var next = 1;
        if (!string.IsNullOrWhiteSpace(latest) && latest.Length >= prefix.Length + 4)
        {
            var suffix = latest[prefix.Length..];
            if (int.TryParse(suffix, out var parsed))
            {
                next = parsed + 1;
            }
        }

        return $"{prefix}{next:0000}";
    }

    private static StockOpnameDto MapDto(InvStockOpname entity, bool includeLines)
    {
        return new StockOpnameDto
        {
            Id = entity.Id,
            OpnameNo = entity.OpnameNo,
            OpnameDate = entity.OpnameDate,
            WarehouseId = entity.WarehouseId,
            WarehouseCode = entity.Warehouse.Code,
            LocationId = entity.LocationId,
            LocationCode = entity.Location?.Code,
            Description = entity.Description,
            Status = entity.Status,
            ApprovedBy = entity.ApprovedBy,
            ApprovedByName = entity.ApprovedByUser?.FullName,
            ApprovedAt = entity.ApprovedAt,
            AdjustmentId = entity.AdjustmentId,
            TotalVarianceValue = decimal.Round(entity.Lines.Sum(x => x.TotalVarianceValue), 4, MidpointRounding.AwayFromZero),
            Lines = includeLines
                ? entity.Lines
                    .OrderBy(x => x.LineNo)
                    .Select(x => new StockOpnameLineDto
                    {
                        Id = x.Id,
                        LineNo = x.LineNo,
                        ItemId = x.ItemId,
                        ItemCode = x.Item.ItemCode,
                        ItemName = x.Item.Name,
                        LocationId = x.LocationId,
                        LocationCode = x.Location?.Code,
                        QtySystem = x.QtySystem,
                        QtyCounted = x.QtyCounted,
                        QtyVariance = x.QtyVariance,
                        UnitCost = x.UnitCost,
                        TotalVarianceValue = x.TotalVarianceValue,
                        Notes = x.Notes
                    })
                    .ToList()
                : []
        };
    }
}
