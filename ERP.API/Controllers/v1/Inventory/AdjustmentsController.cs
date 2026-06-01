using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;
using ERP.Domain.Entities.Inventory;
using ERP.Domain.Enums.Inventory;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Inventory;

[Route("api/v1/inventory/adjustments")]
public sealed class AdjustmentsController(AppDbContext dbContext) : InventoryControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] StockAdjustmentPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.InvStockAdjustments
            .AsNoTracking()
            .Include(x => x.Warehouse)
            .Include(x => x.Location)
            .Include(x => x.ApprovedByUser)
            .Include(x => x.ConfirmedByUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.AdjustmentNo.ToLower().Contains(search) ||
                (x.ReferenceNo != null && x.ReferenceNo.ToLower().Contains(search)) ||
                x.Warehouse.Code.ToLower().Contains(search) ||
                x.Warehouse.Name.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.AdjustmentNo))
        {
            var adjustmentNo = request.AdjustmentNo.Trim().ToLowerInvariant();
            query = query.Where(x => x.AdjustmentNo.ToLower().Contains(adjustmentNo));
        }

        if (request.DateFrom.HasValue)
        {
            query = query.Where(x => x.AdjustmentDate >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(x => x.AdjustmentDate <= request.DateTo.Value);
        }

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);
        }

        if (request.Reason.HasValue)
        {
            query = query.Where(x => x.Reason == request.Reason.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "adjustmentno" => isDesc ? query.OrderByDescending(x => x.AdjustmentNo) : query.OrderBy(x => x.AdjustmentNo),
            "adjustmentdate" => isDesc ? query.OrderByDescending(x => x.AdjustmentDate) : query.OrderBy(x => x.AdjustmentDate),
            "warehousecode" => isDesc ? query.OrderByDescending(x => x.Warehouse.Code) : query.OrderBy(x => x.Warehouse.Code),
            "status" => isDesc ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            "reason" => isDesc ? query.OrderByDescending(x => x.Reason) : query.OrderBy(x => x.Reason),
            _ => isDesc ? query.OrderByDescending(x => x.AdjustmentDate).ThenByDescending(x => x.AdjustmentNo) : query.OrderBy(x => x.AdjustmentDate).ThenBy(x => x.AdjustmentNo)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new StockAdjustmentDto
            {
                Id = x.Id,
                AdjustmentNo = x.AdjustmentNo,
                AdjustmentDate = x.AdjustmentDate,
                WarehouseId = x.WarehouseId,
                WarehouseCode = x.Warehouse.Code,
                LocationId = x.LocationId,
                LocationCode = x.Location != null ? x.Location.Code : null,
                Reason = x.Reason,
                ReferenceNo = x.ReferenceNo,
                Description = x.Description,
                Status = x.Status,
                ApprovedBy = x.ApprovedBy,
                ApprovedByName = x.ApprovedByUser != null ? x.ApprovedByUser.FullName : null,
                ApprovedAt = x.ApprovedAt,
                ConfirmedBy = x.ConfirmedBy,
                ConfirmedByName = x.ConfirmedByUser != null ? x.ConfirmedByUser.FullName : null,
                ConfirmedAt = x.ConfirmedAt,
                JournalEntryId = x.JournalEntryId,
                TotalQuantity = x.Lines.Sum(l => Math.Abs(l.QtyAdjustment)),
                TotalCost = x.Lines.Sum(l => l.TotalCost),
            })
            .ToListAsync(ct);

        return Ok(PagedResult<StockAdjustmentDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var entity = await dbContext.InvStockAdjustments
            .AsNoTracking()
            .Include(x => x.Warehouse)
            .Include(x => x.Location)
            .Include(x => x.ApprovedByUser)
            .Include(x => x.ConfirmedByUser)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Item)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Uom)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return NotFound();
        }

        return Ok(MapDto(entity, includeLines: true));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StockAdjustmentDto request, CancellationToken ct)
    {
        try
        {
            await ValidateHeaderAsync(request.WarehouseId, request.LocationId, ct);
            var normalizedLines = await NormalizeLinesAsync(request.Lines, ct);

            var entity = new InvStockAdjustment
            {
                AdjustmentNo = await GenerateAdjustmentNoAsync(request.AdjustmentDate, ct),
                AdjustmentDate = request.AdjustmentDate,
                WarehouseId = request.WarehouseId,
                LocationId = request.LocationId,
                Reason = request.Reason,
                ReferenceNo = NormalizeOptional(request.ReferenceNo),
                Description = NormalizeOptional(request.Description),
                Status = TransactionStatus.Draft,
                RequestedBy = GetCurrentUserId(),
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system",
                Lines = normalizedLines
            };

            dbContext.InvStockAdjustments.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var created = await dbContext.InvStockAdjustments
                .AsNoTracking()
                .Include(x => x.Warehouse)
                .Include(x => x.Location)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.ConfirmedByUser)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.Item)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.Uom)
                .FirstAsync(x => x.Id == entity.Id, ct);

            return Ok(MapDto(created, includeLines: true));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] StockAdjustmentDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.InvStockAdjustments
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (entity is null)
            {
                return NotFound();
            }

            if (entity.Status != TransactionStatus.Draft)
            {
                return BadRequest(new { message = "Only draft adjustment can be edited." });
            }

            await ValidateHeaderAsync(request.WarehouseId, request.LocationId, ct);
            var normalizedLines = await NormalizeLinesAsync(request.Lines, ct);

            dbContext.InvStockAdjustmentLines.RemoveRange(entity.Lines);
            entity.Lines.Clear();

            entity.AdjustmentDate = request.AdjustmentDate;
            entity.WarehouseId = request.WarehouseId;
            entity.LocationId = request.LocationId;
            entity.Reason = request.Reason;
            entity.ReferenceNo = NormalizeOptional(request.ReferenceNo);
            entity.Description = NormalizeOptional(request.Description);
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            foreach (var line in normalizedLines)
            {
                entity.Lines.Add(line);
            }

            await dbContext.SaveChangesAsync(ct);

            var updated = await dbContext.InvStockAdjustments
                .AsNoTracking()
                .Include(x => x.Warehouse)
                .Include(x => x.Location)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.ConfirmedByUser)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.Item)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.Uom)
                .FirstAsync(x => x.Id == id, ct);

            return Ok(MapDto(updated, includeLines: true));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var entity = await dbContext.InvStockAdjustments
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != TransactionStatus.Draft)
        {
            return BadRequest(new { message = "Only draft adjustment can be deleted." });
        }

        dbContext.InvStockAdjustmentLines.RemoveRange(entity.Lines);
        dbContext.InvStockAdjustments.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPut("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        var entity = await dbContext.InvStockAdjustments
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != TransactionStatus.Draft)
        {
            return BadRequest(new { message = "Only draft adjustment can be approved." });
        }

        if (!entity.Lines.Any(x => x.QtyAdjustment < 0))
        {
            return BadRequest(new { message = "Approval is required only for negative adjustment." });
        }

        entity.ApprovedBy = GetCurrentUserId();
        entity.ApprovedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPut("{id:int}/confirm")]
    public async Task<IActionResult> Confirm(int id, CancellationToken ct)
    {
        var entity = await dbContext.InvStockAdjustments
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != TransactionStatus.Draft)
        {
            return BadRequest(new { message = "Only draft adjustment can be confirmed." });
        }

        if (entity.Lines.Count == 0)
        {
            return BadRequest(new { message = "Adjustment lines are required before confirmation." });
        }

        if (entity.Lines.Any(x => x.QtyAdjustment < 0) && !entity.ApprovedBy.HasValue)
        {
            return BadRequest(new { message = "Negative adjustment requires approval before confirmation." });
        }

        await using var trx = await dbContext.Database.BeginTransactionAsync(ct);

        try
        {
            var currentUser = GetCurrentUserId()?.ToString() ?? "system";

            foreach (var line in entity.Lines)
            {
                if (line.QtyAdjustment < 0)
                {
                    var available = await InventoryStockMutationHelper.GetAvailableAsync(
                        dbContext,
                        line.ItemId,
                        entity.WarehouseId,
                        entity.LocationId,
                        ct);

                    if (available < Math.Abs(line.QtyAdjustment))
                    {
                        throw new InvalidOperationException($"Insufficient stock for line {line.LineNo}. Available: {available:N4}, Requested: {Math.Abs(line.QtyAdjustment):N4}.");
                    }
                }

                await InventoryStockMutationHelper.ApplyMovementAsync(
                    dbContext,
                    line.ItemId,
                    entity.WarehouseId,
                    entity.LocationId,
                    line.QtyAdjustment,
                    line.UnitCost,
                    entity.AdjustmentDate,
                    line.QtyAdjustment >= 0 ? StockMovementType.AdjustmentIn : StockMovementType.AdjustmentOut,
                    "inv_stock_adjustments",
                    entity.Id,
                    line.Id,
                    currentUser,
                    line.Notes,
                    ct);
            }

            entity.Status = TransactionStatus.Confirmed;
            entity.ConfirmedBy = GetCurrentUserId();
            entity.ConfirmedAt = DateTimeOffset.UtcNow;
            entity.UpdatedBy = currentUser;
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(ct);
            await trx.CommitAsync(ct);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            await trx.RollbackAsync(ct);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        var entity = await dbContext.InvStockAdjustments.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != TransactionStatus.Draft)
        {
            return BadRequest(new { message = "Only draft adjustment can be cancelled." });
        }

        entity.Status = TransactionStatus.Cancelled;
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

    private async Task<List<InvStockAdjustmentLine>> NormalizeLinesAsync(IReadOnlyList<StockAdjustmentLineDto> lines, CancellationToken ct)
    {
        if (lines.Count == 0)
        {
            throw new InvalidOperationException("At least one adjustment line is required.");
        }

        var normalized = new List<InvStockAdjustmentLine>();

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];

            if (line.ItemId <= 0)
            {
                throw new InvalidOperationException($"Item is required on line {i + 1}.");
            }

            var itemExists = await dbContext.InvItems.AnyAsync(x => x.Id == line.ItemId, ct);
            if (!itemExists)
            {
                throw new InvalidOperationException($"Item on line {i + 1} not found.");
            }

            if (line.UomId.HasValue)
            {
                var uomExists = await dbContext.InvUnitsOfMeasure.AnyAsync(x => x.Id == line.UomId.Value, ct);
                if (!uomExists)
                {
                    throw new InvalidOperationException($"UOM on line {i + 1} not found.");
                }
            }

            if (line.QtyAdjustment == 0)
            {
                throw new InvalidOperationException($"Qty adjustment on line {i + 1} cannot be zero.");
            }

            if (line.UnitCost < 0)
            {
                throw new InvalidOperationException($"Unit cost on line {i + 1} cannot be negative.");
            }

            normalized.Add(new InvStockAdjustmentLine
            {
                LineNo = i + 1,
                ItemId = line.ItemId,
                UomId = line.UomId,
                QtyAdjustment = decimal.Round(line.QtyAdjustment, 4, MidpointRounding.AwayFromZero),
                UnitCost = decimal.Round(line.UnitCost, 4, MidpointRounding.AwayFromZero),
                Notes = NormalizeOptional(line.Notes)
            });
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

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

    private static StockAdjustmentDto MapDto(InvStockAdjustment entity, bool includeLines)
    {
        return new StockAdjustmentDto
        {
            Id = entity.Id,
            AdjustmentNo = entity.AdjustmentNo,
            AdjustmentDate = entity.AdjustmentDate,
            WarehouseId = entity.WarehouseId,
            WarehouseCode = entity.Warehouse.Code,
            LocationId = entity.LocationId,
            LocationCode = entity.Location?.Code,
            Reason = entity.Reason,
            ReferenceNo = entity.ReferenceNo,
            Description = entity.Description,
            Status = entity.Status,
            ApprovedBy = entity.ApprovedBy,
            ApprovedByName = entity.ApprovedByUser?.FullName,
            ApprovedAt = entity.ApprovedAt,
            ConfirmedBy = entity.ConfirmedBy,
            ConfirmedByName = entity.ConfirmedByUser?.FullName,
            ConfirmedAt = entity.ConfirmedAt,
            JournalEntryId = entity.JournalEntryId,
            TotalQuantity = decimal.Round(entity.Lines.Sum(x => Math.Abs(x.QtyAdjustment)), 4, MidpointRounding.AwayFromZero),
            TotalCost = decimal.Round(entity.Lines.Sum(x => x.TotalCost), 4, MidpointRounding.AwayFromZero),
            Lines = includeLines
                ? entity.Lines
                    .OrderBy(x => x.LineNo)
                    .Select(x => new StockAdjustmentLineDto
                    {
                        Id = x.Id,
                        LineNo = x.LineNo,
                        ItemId = x.ItemId,
                        ItemCode = x.Item.ItemCode,
                        ItemName = x.Item.Name,
                        UomId = x.UomId,
                        UomCode = x.Uom?.Code,
                        QtyAdjustment = x.QtyAdjustment,
                        UnitCost = x.UnitCost,
                        TotalCost = x.TotalCost,
                        Notes = x.Notes
                    })
                    .ToList()
                : []
        };
    }
}
