using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;
using ERP.Domain.Entities.Inventory;
using ERP.Domain.Enums.Inventory;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Inventory;

[Route("api/v1/inventory/goods-receipts")]
public sealed class GoodsReceiptsController(AppDbContext dbContext) : InventoryControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GoodsReceiptPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.InvGoodsReceipts
            .AsNoTracking()
            .Include(x => x.Warehouse)
            .Include(x => x.Location)
            .Include(x => x.ConfirmedByUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.ReceiptNo.ToLower().Contains(search) ||
                (x.SupplierName != null && x.SupplierName.ToLower().Contains(search)) ||
                (x.ReferenceNo != null && x.ReferenceNo.ToLower().Contains(search)) ||
                x.Warehouse.Code.ToLower().Contains(search) ||
                x.Warehouse.Name.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.ReceiptNo))
        {
            var receiptNo = request.ReceiptNo.Trim().ToLowerInvariant();
            query = query.Where(x => x.ReceiptNo.ToLower().Contains(receiptNo));
        }

        if (request.DateFrom.HasValue)
        {
            query = query.Where(x => x.ReceiptDate >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(x => x.ReceiptDate <= request.DateTo.Value);
        }

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);
        }

        if (request.ReceiptType.HasValue)
        {
            query = query.Where(x => x.ReceiptType == request.ReceiptType.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SupplierName))
        {
            var supplierName = request.SupplierName.Trim().ToLowerInvariant();
            query = query.Where(x => x.SupplierName != null && x.SupplierName.ToLower().Contains(supplierName));
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "receiptno" => isDesc ? query.OrderByDescending(x => x.ReceiptNo) : query.OrderBy(x => x.ReceiptNo),
            "receiptdate" => isDesc ? query.OrderByDescending(x => x.ReceiptDate) : query.OrderBy(x => x.ReceiptDate),
            "warehousecode" => isDesc ? query.OrderByDescending(x => x.Warehouse.Code) : query.OrderBy(x => x.Warehouse.Code),
            "status" => isDesc ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            "receipttype" => isDesc ? query.OrderByDescending(x => x.ReceiptType) : query.OrderBy(x => x.ReceiptType),
            _ => isDesc ? query.OrderByDescending(x => x.ReceiptDate).ThenByDescending(x => x.ReceiptNo) : query.OrderBy(x => x.ReceiptDate).ThenBy(x => x.ReceiptNo)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new GoodsReceiptDto
            {
                Id = x.Id,
                ReceiptNo = x.ReceiptNo,
                ReceiptDate = x.ReceiptDate,
                ReceiptType = x.ReceiptType,
                WarehouseId = x.WarehouseId,
                WarehouseCode = x.Warehouse.Code,
                WarehouseName = x.Warehouse.Name,
                LocationId = x.LocationId,
                LocationCode = x.Location != null ? x.Location.Code : null,
                SupplierName = x.SupplierName,
                ReferenceNo = x.ReferenceNo,
                Description = x.Description,
                Status = x.Status,
                ConfirmedBy = x.ConfirmedBy,
                ConfirmedByName = x.ConfirmedByUser != null ? x.ConfirmedByUser.FullName : null,
                ConfirmedAt = x.ConfirmedAt,
                JournalEntryId = x.JournalEntryId,
                TotalQuantity = x.Lines.Sum(l => l.QtyBase),
                TotalCost = x.Lines.Sum(l => l.TotalCost),
            })
            .ToListAsync(ct);

        return Ok(PagedResult<GoodsReceiptDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var entity = await dbContext.InvGoodsReceipts
            .AsNoTracking()
            .Include(x => x.Warehouse)
            .Include(x => x.Location)
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
    public async Task<IActionResult> Create([FromBody] GoodsReceiptDto request, CancellationToken ct)
    {
        try
        {
            await ValidateHeaderAsync(request.WarehouseId, request.LocationId, ct);
            var normalizedLines = await NormalizeLinesAsync(request.Lines, ct);

            var entity = new InvGoodsReceipt
            {
                ReceiptNo = await GenerateReceiptNoAsync(request.ReceiptDate, ct),
                ReceiptDate = request.ReceiptDate,
                ReceiptType = request.ReceiptType,
                WarehouseId = request.WarehouseId,
                LocationId = request.LocationId,
                SupplierName = NormalizeOptional(request.SupplierName),
                ReferenceNo = NormalizeOptional(request.ReferenceNo),
                Description = NormalizeOptional(request.Description),
                Status = TransactionStatus.Draft,
                ReceivedBy = GetCurrentUserId(),
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system",
                Lines = normalizedLines
            };

            dbContext.InvGoodsReceipts.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var created = await dbContext.InvGoodsReceipts
                .AsNoTracking()
                .Include(x => x.Warehouse)
                .Include(x => x.Location)
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
    public async Task<IActionResult> Update(int id, [FromBody] GoodsReceiptDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.InvGoodsReceipts
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (entity is null)
            {
                return NotFound();
            }

            if (entity.Status != TransactionStatus.Draft)
            {
                return BadRequest(new { message = "Only draft receipt can be edited." });
            }

            await ValidateHeaderAsync(request.WarehouseId, request.LocationId, ct);
            var normalizedLines = await NormalizeLinesAsync(request.Lines, ct);

            dbContext.InvGoodsReceiptLines.RemoveRange(entity.Lines);
            entity.Lines.Clear();

            entity.ReceiptDate = request.ReceiptDate;
            entity.ReceiptType = request.ReceiptType;
            entity.WarehouseId = request.WarehouseId;
            entity.LocationId = request.LocationId;
            entity.SupplierName = NormalizeOptional(request.SupplierName);
            entity.ReferenceNo = NormalizeOptional(request.ReferenceNo);
            entity.Description = NormalizeOptional(request.Description);
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            foreach (var line in normalizedLines)
            {
                entity.Lines.Add(line);
            }

            await dbContext.SaveChangesAsync(ct);

            var updated = await dbContext.InvGoodsReceipts
                .AsNoTracking()
                .Include(x => x.Warehouse)
                .Include(x => x.Location)
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
        var entity = await dbContext.InvGoodsReceipts
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != TransactionStatus.Draft)
        {
            return BadRequest(new { message = "Only draft receipt can be deleted." });
        }

        dbContext.InvGoodsReceiptLines.RemoveRange(entity.Lines);
        dbContext.InvGoodsReceipts.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPut("{id:int}/confirm")]
    public async Task<IActionResult> Confirm(int id, CancellationToken ct)
    {
        var entity = await dbContext.InvGoodsReceipts
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != TransactionStatus.Draft)
        {
            return BadRequest(new { message = "Only draft receipt can be confirmed." });
        }

        if (entity.Lines.Count == 0)
        {
            return BadRequest(new { message = "Receipt lines are required before confirmation." });
        }

        await using var trx = await dbContext.Database.BeginTransactionAsync(ct);

        try
        {
            var currentUser = GetCurrentUserId()?.ToString() ?? "system";
            var movementType = entity.ReceiptType == GoodsReceiptType.OpeningBalance
                ? StockMovementType.OpeningBalance
                : StockMovementType.GoodsReceipt;

            foreach (var line in entity.Lines)
            {
                await InventoryStockMutationHelper.ApplyMovementAsync(
                    dbContext,
                    line.ItemId,
                    entity.WarehouseId,
                    entity.LocationId,
                    line.QtyBase,
                    line.UnitCost,
                    entity.ReceiptDate,
                    movementType,
                    "inv_goods_receipts",
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
        var entity = await dbContext.InvGoodsReceipts.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != TransactionStatus.Draft)
        {
            return BadRequest(new { message = "Only draft receipt can be cancelled." });
        }

        entity.Status = TransactionStatus.Cancelled;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpGet("{id:int}/print")]
    public async Task<IActionResult> Print(int id, CancellationToken ct)
    {
        var exists = await dbContext.InvGoodsReceipts.AsNoTracking().AnyAsync(x => x.Id == id, ct);
        if (!exists)
        {
            return NotFound();
        }

        return Ok(new { message = "Print output is not implemented yet.", id });
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

    private async Task<List<InvGoodsReceiptLine>> NormalizeLinesAsync(IReadOnlyList<GoodsReceiptLineDto> lines, CancellationToken ct)
    {
        if (lines.Count == 0)
        {
            throw new InvalidOperationException("At least one receipt line is required.");
        }

        var normalized = new List<InvGoodsReceiptLine>();

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

            if (line.QtyReceived <= 0)
            {
                throw new InvalidOperationException($"Qty received on line {i + 1} must be greater than zero.");
            }

            var qtyBase = line.QtyBase <= 0 ? line.QtyReceived : line.QtyBase;
            if (qtyBase <= 0)
            {
                throw new InvalidOperationException($"Qty base on line {i + 1} must be greater than zero.");
            }

            if (line.UnitCost < 0)
            {
                throw new InvalidOperationException($"Unit cost on line {i + 1} cannot be negative.");
            }

            normalized.Add(new InvGoodsReceiptLine
            {
                LineNo = i + 1,
                ItemId = line.ItemId,
                UomId = line.UomId,
                QtyReceived = decimal.Round(line.QtyReceived, 4, MidpointRounding.AwayFromZero),
                QtyBase = decimal.Round(qtyBase, 4, MidpointRounding.AwayFromZero),
                UnitCost = decimal.Round(line.UnitCost, 4, MidpointRounding.AwayFromZero),
                Notes = NormalizeOptional(line.Notes)
            });
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private async Task<string> GenerateReceiptNoAsync(DateOnly receiptDate, CancellationToken ct)
    {
        var prefix = $"GR-{receiptDate:yyyyMM}-";
        var latest = await dbContext.InvGoodsReceipts
            .IgnoreQueryFilters()
            .Where(x => x.ReceiptNo.StartsWith(prefix))
            .OrderByDescending(x => x.ReceiptNo)
            .Select(x => x.ReceiptNo)
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

    private static GoodsReceiptDto MapDto(InvGoodsReceipt entity, bool includeLines)
    {
        return new GoodsReceiptDto
        {
            Id = entity.Id,
            ReceiptNo = entity.ReceiptNo,
            ReceiptDate = entity.ReceiptDate,
            ReceiptType = entity.ReceiptType,
            WarehouseId = entity.WarehouseId,
            WarehouseCode = entity.Warehouse.Code,
            WarehouseName = entity.Warehouse.Name,
            LocationId = entity.LocationId,
            LocationCode = entity.Location?.Code,
            SupplierName = entity.SupplierName,
            ReferenceNo = entity.ReferenceNo,
            Description = entity.Description,
            Status = entity.Status,
            ConfirmedBy = entity.ConfirmedBy,
            ConfirmedByName = entity.ConfirmedByUser?.FullName,
            ConfirmedAt = entity.ConfirmedAt,
            JournalEntryId = entity.JournalEntryId,
            TotalQuantity = decimal.Round(entity.Lines.Sum(x => x.QtyBase), 4, MidpointRounding.AwayFromZero),
            TotalCost = decimal.Round(entity.Lines.Sum(x => x.TotalCost), 4, MidpointRounding.AwayFromZero),
            Lines = includeLines
                ? entity.Lines
                    .OrderBy(x => x.LineNo)
                    .Select(x => new GoodsReceiptLineDto
                    {
                        Id = x.Id,
                        LineNo = x.LineNo,
                        ItemId = x.ItemId,
                        ItemCode = x.Item.ItemCode,
                        ItemName = x.Item.Name,
                        UomId = x.UomId,
                        UomCode = x.Uom?.Code,
                        QtyReceived = x.QtyReceived,
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
