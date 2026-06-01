using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;
using ERP.Domain.Entities.Inventory;
using ERP.Domain.Enums.Inventory;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Inventory;

[Route("api/v1/inventory/transfers")]
public sealed class TransfersController(AppDbContext dbContext) : InventoryControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] StockTransferPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.InvStockTransfers
            .AsNoTracking()
            .Include(x => x.FromWarehouse)
            .Include(x => x.ToWarehouse)
            .Include(x => x.FromLocation)
            .Include(x => x.ToLocation)
            .Include(x => x.ConfirmedByUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.TransferNo.ToLower().Contains(search) ||
                (x.ReferenceNo != null && x.ReferenceNo.ToLower().Contains(search)) ||
                x.FromWarehouse.Code.ToLower().Contains(search) ||
                x.ToWarehouse.Code.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.TransferNo))
        {
            var transferNo = request.TransferNo.Trim().ToLowerInvariant();
            query = query.Where(x => x.TransferNo.ToLower().Contains(transferNo));
        }

        if (request.DateFrom.HasValue)
        {
            query = query.Where(x => x.TransferDate >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(x => x.TransferDate <= request.DateTo.Value);
        }

        if (request.FromWarehouseId.HasValue)
        {
            query = query.Where(x => x.FromWarehouseId == request.FromWarehouseId.Value);
        }

        if (request.ToWarehouseId.HasValue)
        {
            query = query.Where(x => x.ToWarehouseId == request.ToWarehouseId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "transferno" => isDesc ? query.OrderByDescending(x => x.TransferNo) : query.OrderBy(x => x.TransferNo),
            "transferdate" => isDesc ? query.OrderByDescending(x => x.TransferDate) : query.OrderBy(x => x.TransferDate),
            "fromwarehousecode" => isDesc ? query.OrderByDescending(x => x.FromWarehouse.Code) : query.OrderBy(x => x.FromWarehouse.Code),
            "towarehousecode" => isDesc ? query.OrderByDescending(x => x.ToWarehouse.Code) : query.OrderBy(x => x.ToWarehouse.Code),
            "status" => isDesc ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            _ => isDesc ? query.OrderByDescending(x => x.TransferDate).ThenByDescending(x => x.TransferNo) : query.OrderBy(x => x.TransferDate).ThenBy(x => x.TransferNo)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new StockTransferDto
            {
                Id = x.Id,
                TransferNo = x.TransferNo,
                TransferDate = x.TransferDate,
                FromWarehouseId = x.FromWarehouseId,
                FromWarehouseCode = x.FromWarehouse.Code,
                FromLocationId = x.FromLocationId,
                FromLocationCode = x.FromLocation != null ? x.FromLocation.Code : null,
                ToWarehouseId = x.ToWarehouseId,
                ToWarehouseCode = x.ToWarehouse.Code,
                ToLocationId = x.ToLocationId,
                ToLocationCode = x.ToLocation != null ? x.ToLocation.Code : null,
                ReferenceNo = x.ReferenceNo,
                Description = x.Description,
                Status = x.Status,
                ConfirmedBy = x.ConfirmedBy,
                ConfirmedByName = x.ConfirmedByUser != null ? x.ConfirmedByUser.FullName : null,
                ConfirmedAt = x.ConfirmedAt,
                TotalQuantity = x.Lines.Sum(l => l.QtyBase),
                TotalCost = x.Lines.Sum(l => l.TotalCost),
            })
            .ToListAsync(ct);

        return Ok(PagedResult<StockTransferDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var entity = await dbContext.InvStockTransfers
            .AsNoTracking()
            .Include(x => x.FromWarehouse)
            .Include(x => x.ToWarehouse)
            .Include(x => x.FromLocation)
            .Include(x => x.ToLocation)
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
    public async Task<IActionResult> Create([FromBody] StockTransferDto request, CancellationToken ct)
    {
        try
        {
            await ValidateHeaderAsync(request, ct);
            var normalizedLines = await NormalizeLinesAsync(request.Lines, ct);

            var entity = new InvStockTransfer
            {
                TransferNo = await GenerateTransferNoAsync(request.TransferDate, ct),
                TransferDate = request.TransferDate,
                FromWarehouseId = request.FromWarehouseId,
                FromLocationId = request.FromLocationId,
                ToWarehouseId = request.ToWarehouseId,
                ToLocationId = request.ToLocationId,
                ReferenceNo = NormalizeOptional(request.ReferenceNo),
                Description = NormalizeOptional(request.Description),
                Status = TransactionStatus.Draft,
                TransferredBy = GetCurrentUserId(),
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system",
                Lines = normalizedLines
            };

            dbContext.InvStockTransfers.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var created = await dbContext.InvStockTransfers
                .AsNoTracking()
                .Include(x => x.FromWarehouse)
                .Include(x => x.ToWarehouse)
                .Include(x => x.FromLocation)
                .Include(x => x.ToLocation)
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
    public async Task<IActionResult> Update(int id, [FromBody] StockTransferDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.InvStockTransfers
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (entity is null)
            {
                return NotFound();
            }

            if (entity.Status != TransactionStatus.Draft)
            {
                return BadRequest(new { message = "Only draft transfer can be edited." });
            }

            await ValidateHeaderAsync(request, ct);
            var normalizedLines = await NormalizeLinesAsync(request.Lines, ct);

            dbContext.InvStockTransferLines.RemoveRange(entity.Lines);
            entity.Lines.Clear();

            entity.TransferDate = request.TransferDate;
            entity.FromWarehouseId = request.FromWarehouseId;
            entity.FromLocationId = request.FromLocationId;
            entity.ToWarehouseId = request.ToWarehouseId;
            entity.ToLocationId = request.ToLocationId;
            entity.ReferenceNo = NormalizeOptional(request.ReferenceNo);
            entity.Description = NormalizeOptional(request.Description);
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            foreach (var line in normalizedLines)
            {
                entity.Lines.Add(line);
            }

            await dbContext.SaveChangesAsync(ct);

            var updated = await dbContext.InvStockTransfers
                .AsNoTracking()
                .Include(x => x.FromWarehouse)
                .Include(x => x.ToWarehouse)
                .Include(x => x.FromLocation)
                .Include(x => x.ToLocation)
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
        var entity = await dbContext.InvStockTransfers
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != TransactionStatus.Draft)
        {
            return BadRequest(new { message = "Only draft transfer can be deleted." });
        }

        dbContext.InvStockTransferLines.RemoveRange(entity.Lines);
        dbContext.InvStockTransfers.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPut("{id:int}/confirm")]
    public async Task<IActionResult> Confirm(int id, CancellationToken ct)
    {
        var entity = await dbContext.InvStockTransfers
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != TransactionStatus.Draft)
        {
            return BadRequest(new { message = "Only draft transfer can be confirmed." });
        }

        if (entity.Lines.Count == 0)
        {
            return BadRequest(new { message = "Transfer lines are required before confirmation." });
        }

        await using var trx = await dbContext.Database.BeginTransactionAsync(ct);

        try
        {
            var currentUser = GetCurrentUserId()?.ToString() ?? "system";

            foreach (var line in entity.Lines)
            {
                var available = await InventoryStockMutationHelper.GetAvailableAsync(
                    dbContext,
                    line.ItemId,
                    entity.FromWarehouseId,
                    entity.FromLocationId,
                    ct);

                if (available < line.QtyBase)
                {
                    throw new InvalidOperationException($"Insufficient stock for line {line.LineNo}. Available: {available:N4}, Requested: {line.QtyBase:N4}.");
                }

                var outResult = await InventoryStockMutationHelper.ApplyMovementAsync(
                    dbContext,
                    line.ItemId,
                    entity.FromWarehouseId,
                    entity.FromLocationId,
                    -line.QtyBase,
                    line.UnitCost,
                    entity.TransferDate,
                    StockMovementType.TransferOut,
                    "inv_stock_transfers",
                    entity.Id,
                    line.Id,
                    currentUser,
                    line.Notes,
                    ct);

                await InventoryStockMutationHelper.ApplyMovementAsync(
                    dbContext,
                    line.ItemId,
                    entity.ToWarehouseId,
                    entity.ToLocationId,
                    line.QtyBase,
                    outResult.UnitCost,
                    entity.TransferDate,
                    StockMovementType.TransferIn,
                    "inv_stock_transfers",
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
        var entity = await dbContext.InvStockTransfers.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != TransactionStatus.Draft)
        {
            return BadRequest(new { message = "Only draft transfer can be cancelled." });
        }

        entity.Status = TransactionStatus.Cancelled;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task ValidateHeaderAsync(StockTransferDto request, CancellationToken ct)
    {
        if (request.FromWarehouseId <= 0 || request.ToWarehouseId <= 0)
        {
            throw new InvalidOperationException("From warehouse and to warehouse are required.");
        }

        if (request.FromWarehouseId == request.ToWarehouseId)
        {
            throw new InvalidOperationException("From and to warehouse cannot be the same.");
        }

        var fromWarehouseExists = await dbContext.InvWarehouses.AnyAsync(x => x.Id == request.FromWarehouseId, ct);
        var toWarehouseExists = await dbContext.InvWarehouses.AnyAsync(x => x.Id == request.ToWarehouseId, ct);

        if (!fromWarehouseExists || !toWarehouseExists)
        {
            throw new InvalidOperationException("Warehouse not found.");
        }

        if (request.FromLocationId.HasValue)
        {
            var fromLocationExists = await dbContext.InvWarehouseLocations
                .AnyAsync(x => x.Id == request.FromLocationId.Value && x.WarehouseId == request.FromWarehouseId, ct);

            if (!fromLocationExists)
            {
                throw new InvalidOperationException("From location not found in selected warehouse.");
            }
        }

        if (request.ToLocationId.HasValue)
        {
            var toLocationExists = await dbContext.InvWarehouseLocations
                .AnyAsync(x => x.Id == request.ToLocationId.Value && x.WarehouseId == request.ToWarehouseId, ct);

            if (!toLocationExists)
            {
                throw new InvalidOperationException("To location not found in selected warehouse.");
            }
        }
    }

    private async Task<List<InvStockTransferLine>> NormalizeLinesAsync(IReadOnlyList<StockTransferLineDto> lines, CancellationToken ct)
    {
        if (lines.Count == 0)
        {
            throw new InvalidOperationException("At least one transfer line is required.");
        }

        var normalized = new List<InvStockTransferLine>();

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

            if (line.QtyTransfer <= 0)
            {
                throw new InvalidOperationException($"Qty transfer on line {i + 1} must be greater than zero.");
            }

            var qtyBase = line.QtyBase <= 0 ? line.QtyTransfer : line.QtyBase;
            if (qtyBase <= 0)
            {
                throw new InvalidOperationException($"Qty base on line {i + 1} must be greater than zero.");
            }

            if (line.UnitCost < 0)
            {
                throw new InvalidOperationException($"Unit cost on line {i + 1} cannot be negative.");
            }

            normalized.Add(new InvStockTransferLine
            {
                LineNo = i + 1,
                ItemId = line.ItemId,
                UomId = line.UomId,
                QtyTransfer = decimal.Round(line.QtyTransfer, 4, MidpointRounding.AwayFromZero),
                QtyBase = decimal.Round(qtyBase, 4, MidpointRounding.AwayFromZero),
                UnitCost = decimal.Round(line.UnitCost, 4, MidpointRounding.AwayFromZero),
                Notes = NormalizeOptional(line.Notes)
            });
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private async Task<string> GenerateTransferNoAsync(DateOnly transferDate, CancellationToken ct)
    {
        var prefix = $"TRF-{transferDate:yyyyMM}-";
        var latest = await dbContext.InvStockTransfers
            .IgnoreQueryFilters()
            .Where(x => x.TransferNo.StartsWith(prefix))
            .OrderByDescending(x => x.TransferNo)
            .Select(x => x.TransferNo)
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

    private static StockTransferDto MapDto(InvStockTransfer entity, bool includeLines)
    {
        return new StockTransferDto
        {
            Id = entity.Id,
            TransferNo = entity.TransferNo,
            TransferDate = entity.TransferDate,
            FromWarehouseId = entity.FromWarehouseId,
            FromWarehouseCode = entity.FromWarehouse.Code,
            FromLocationId = entity.FromLocationId,
            FromLocationCode = entity.FromLocation?.Code,
            ToWarehouseId = entity.ToWarehouseId,
            ToWarehouseCode = entity.ToWarehouse.Code,
            ToLocationId = entity.ToLocationId,
            ToLocationCode = entity.ToLocation?.Code,
            ReferenceNo = entity.ReferenceNo,
            Description = entity.Description,
            Status = entity.Status,
            ConfirmedBy = entity.ConfirmedBy,
            ConfirmedByName = entity.ConfirmedByUser?.FullName,
            ConfirmedAt = entity.ConfirmedAt,
            TotalQuantity = decimal.Round(entity.Lines.Sum(x => x.QtyBase), 4, MidpointRounding.AwayFromZero),
            TotalCost = decimal.Round(entity.Lines.Sum(x => x.TotalCost), 4, MidpointRounding.AwayFromZero),
            Lines = includeLines
                ? entity.Lines
                    .OrderBy(x => x.LineNo)
                    .Select(x => new StockTransferLineDto
                    {
                        Id = x.Id,
                        LineNo = x.LineNo,
                        ItemId = x.ItemId,
                        ItemCode = x.Item.ItemCode,
                        ItemName = x.Item.Name,
                        UomId = x.UomId,
                        UomCode = x.Uom?.Code,
                        QtyTransfer = x.QtyTransfer,
                        QtyBase = x.QtyBase,
                        UnitCost = x.UnitCost,
                        TotalCost = x.TotalCost,
                        Notes = x.Notes
                    })
                    .ToList()
                : []
        };
    }
}
