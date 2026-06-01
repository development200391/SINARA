using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;
using ERP.Domain.Entities.Inventory;
using ERP.Domain.Enums.Inventory;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Inventory;

[Route("api/v1/inventory/goods-issues")]
public sealed class GoodsIssuesController(AppDbContext dbContext) : InventoryControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GoodsIssuePagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.InvGoodsIssues
            .AsNoTracking()
            .Include(x => x.Warehouse)
            .Include(x => x.Location)
            .Include(x => x.Department)
            .Include(x => x.CostCenter)
            .Include(x => x.ConfirmedByUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.IssueNo.ToLower().Contains(search) ||
                (x.ReferenceNo != null && x.ReferenceNo.ToLower().Contains(search)) ||
                x.Warehouse.Code.ToLower().Contains(search) ||
                x.Warehouse.Name.ToLower().Contains(search) ||
                (x.Description != null && x.Description.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.IssueNo))
        {
            var issueNo = request.IssueNo.Trim().ToLowerInvariant();
            query = query.Where(x => x.IssueNo.ToLower().Contains(issueNo));
        }

        if (request.DateFrom.HasValue)
        {
            query = query.Where(x => x.IssueDate >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(x => x.IssueDate <= request.DateTo.Value);
        }

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);
        }

        if (request.DepartmentId.HasValue)
        {
            query = query.Where(x => x.DepartmentId == request.DepartmentId.Value);
        }

        if (request.IssueType.HasValue)
        {
            query = query.Where(x => x.IssueType == request.IssueType.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "issueno" => isDesc ? query.OrderByDescending(x => x.IssueNo) : query.OrderBy(x => x.IssueNo),
            "issuedate" => isDesc ? query.OrderByDescending(x => x.IssueDate) : query.OrderBy(x => x.IssueDate),
            "warehousecode" => isDesc ? query.OrderByDescending(x => x.Warehouse.Code) : query.OrderBy(x => x.Warehouse.Code),
            "status" => isDesc ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            "issuetype" => isDesc ? query.OrderByDescending(x => x.IssueType) : query.OrderBy(x => x.IssueType),
            _ => isDesc ? query.OrderByDescending(x => x.IssueDate).ThenByDescending(x => x.IssueNo) : query.OrderBy(x => x.IssueDate).ThenBy(x => x.IssueNo)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new GoodsIssueDto
            {
                Id = x.Id,
                IssueNo = x.IssueNo,
                IssueDate = x.IssueDate,
                IssueType = x.IssueType,
                WarehouseId = x.WarehouseId,
                WarehouseCode = x.Warehouse.Code,
                WarehouseName = x.Warehouse.Name,
                LocationId = x.LocationId,
                LocationCode = x.Location != null ? x.Location.Code : null,
                DepartmentId = x.DepartmentId,
                DepartmentCode = x.Department != null ? x.Department.Code : null,
                DepartmentName = x.Department != null ? x.Department.Name : null,
                CostCenterId = x.CostCenterId,
                CostCenterCode = x.CostCenter != null ? x.CostCenter.Code : null,
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

        return Ok(PagedResult<GoodsIssueDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var entity = await dbContext.InvGoodsIssues
            .AsNoTracking()
            .Include(x => x.Warehouse)
            .Include(x => x.Location)
            .Include(x => x.Department)
            .Include(x => x.CostCenter)
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
    public async Task<IActionResult> Create([FromBody] GoodsIssueDto request, CancellationToken ct)
    {
        try
        {
            await ValidateHeaderAsync(request, ct);
            var normalizedLines = await NormalizeLinesAsync(request.Lines, ct);

            var entity = new InvGoodsIssue
            {
                IssueNo = await GenerateIssueNoAsync(request.IssueDate, ct),
                IssueDate = request.IssueDate,
                IssueType = request.IssueType,
                WarehouseId = request.WarehouseId,
                LocationId = request.LocationId,
                DepartmentId = request.DepartmentId,
                CostCenterId = request.CostCenterId,
                ReferenceNo = NormalizeOptional(request.ReferenceNo),
                Description = NormalizeOptional(request.Description),
                Status = TransactionStatus.Draft,
                RequestedBy = GetCurrentUserId(),
                IssuedBy = GetCurrentUserId(),
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system",
                Lines = normalizedLines
            };

            dbContext.InvGoodsIssues.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var created = await dbContext.InvGoodsIssues
                .AsNoTracking()
                .Include(x => x.Warehouse)
                .Include(x => x.Location)
                .Include(x => x.Department)
                .Include(x => x.CostCenter)
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
    public async Task<IActionResult> Update(int id, [FromBody] GoodsIssueDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.InvGoodsIssues
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (entity is null)
            {
                return NotFound();
            }

            if (entity.Status != TransactionStatus.Draft)
            {
                return BadRequest(new { message = "Only draft issue can be edited." });
            }

            await ValidateHeaderAsync(request, ct);
            var normalizedLines = await NormalizeLinesAsync(request.Lines, ct);

            dbContext.InvGoodsIssueLines.RemoveRange(entity.Lines);
            entity.Lines.Clear();

            entity.IssueDate = request.IssueDate;
            entity.IssueType = request.IssueType;
            entity.WarehouseId = request.WarehouseId;
            entity.LocationId = request.LocationId;
            entity.DepartmentId = request.DepartmentId;
            entity.CostCenterId = request.CostCenterId;
            entity.ReferenceNo = NormalizeOptional(request.ReferenceNo);
            entity.Description = NormalizeOptional(request.Description);
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            foreach (var line in normalizedLines)
            {
                entity.Lines.Add(line);
            }

            await dbContext.SaveChangesAsync(ct);

            var updated = await dbContext.InvGoodsIssues
                .AsNoTracking()
                .Include(x => x.Warehouse)
                .Include(x => x.Location)
                .Include(x => x.Department)
                .Include(x => x.CostCenter)
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
        var entity = await dbContext.InvGoodsIssues
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != TransactionStatus.Draft)
        {
            return BadRequest(new { message = "Only draft issue can be deleted." });
        }

        dbContext.InvGoodsIssueLines.RemoveRange(entity.Lines);
        dbContext.InvGoodsIssues.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPut("{id:int}/confirm")]
    public async Task<IActionResult> Confirm(int id, CancellationToken ct)
    {
        var entity = await dbContext.InvGoodsIssues
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != TransactionStatus.Draft)
        {
            return BadRequest(new { message = "Only draft issue can be confirmed." });
        }

        if (entity.Lines.Count == 0)
        {
            return BadRequest(new { message = "Issue lines are required before confirmation." });
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
                    entity.WarehouseId,
                    entity.LocationId,
                    ct);

                if (available < line.QtyBase)
                {
                    throw new InvalidOperationException($"Insufficient stock for line {line.LineNo}. Available: {available:N4}, Requested: {line.QtyBase:N4}.");
                }

                await InventoryStockMutationHelper.ApplyMovementAsync(
                    dbContext,
                    line.ItemId,
                    entity.WarehouseId,
                    entity.LocationId,
                    -line.QtyBase,
                    line.UnitCost,
                    entity.IssueDate,
                    StockMovementType.GoodsIssue,
                    "inv_goods_issues",
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
        var entity = await dbContext.InvGoodsIssues.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != TransactionStatus.Draft)
        {
            return BadRequest(new { message = "Only draft issue can be cancelled." });
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
        var exists = await dbContext.InvGoodsIssues.AsNoTracking().AnyAsync(x => x.Id == id, ct);
        if (!exists)
        {
            return NotFound();
        }

        return Ok(new { message = "Print output is not implemented yet.", id });
    }

    private async Task ValidateHeaderAsync(GoodsIssueDto request, CancellationToken ct)
    {
        if (request.WarehouseId <= 0)
        {
            throw new InvalidOperationException("Warehouse is required.");
        }

        var warehouseExists = await dbContext.InvWarehouses.AnyAsync(x => x.Id == request.WarehouseId, ct);
        if (!warehouseExists)
        {
            throw new InvalidOperationException("Warehouse not found.");
        }

        if (request.LocationId.HasValue)
        {
            var locationExists = await dbContext.InvWarehouseLocations
                .AnyAsync(x => x.Id == request.LocationId.Value && x.WarehouseId == request.WarehouseId, ct);

            if (!locationExists)
            {
                throw new InvalidOperationException("Location not found in selected warehouse.");
            }
        }

        if (request.DepartmentId.HasValue)
        {
            var departmentExists = await dbContext.HrDepartments.AnyAsync(x => x.Id == request.DepartmentId.Value, ct);
            if (!departmentExists)
            {
                throw new InvalidOperationException("Department not found.");
            }
        }

        if (request.CostCenterId.HasValue)
        {
            var costCenterExists = await dbContext.FinCostCenters.AnyAsync(x => x.Id == request.CostCenterId.Value, ct);
            if (!costCenterExists)
            {
                throw new InvalidOperationException("Cost center not found.");
            }
        }
    }

    private async Task<List<InvGoodsIssueLine>> NormalizeLinesAsync(IReadOnlyList<GoodsIssueLineDto> lines, CancellationToken ct)
    {
        if (lines.Count == 0)
        {
            throw new InvalidOperationException("At least one issue line is required.");
        }

        var normalized = new List<InvGoodsIssueLine>();

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

            if (line.QtyRequested <= 0)
            {
                throw new InvalidOperationException($"Qty requested on line {i + 1} must be greater than zero.");
            }

            var qtyIssued = line.QtyIssued <= 0 ? line.QtyRequested : line.QtyIssued;
            var qtyBase = line.QtyBase <= 0 ? qtyIssued : line.QtyBase;

            if (qtyIssued <= 0 || qtyBase <= 0)
            {
                throw new InvalidOperationException($"Qty issued and qty base on line {i + 1} must be greater than zero.");
            }

            if (qtyIssued > line.QtyRequested)
            {
                throw new InvalidOperationException($"Qty issued on line {i + 1} cannot exceed qty requested.");
            }

            if (line.UnitCost < 0)
            {
                throw new InvalidOperationException($"Unit cost on line {i + 1} cannot be negative.");
            }

            normalized.Add(new InvGoodsIssueLine
            {
                LineNo = i + 1,
                ItemId = line.ItemId,
                UomId = line.UomId,
                QtyRequested = decimal.Round(line.QtyRequested, 4, MidpointRounding.AwayFromZero),
                QtyIssued = decimal.Round(qtyIssued, 4, MidpointRounding.AwayFromZero),
                QtyBase = decimal.Round(qtyBase, 4, MidpointRounding.AwayFromZero),
                UnitCost = decimal.Round(line.UnitCost, 4, MidpointRounding.AwayFromZero),
                Notes = NormalizeOptional(line.Notes)
            });
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private async Task<string> GenerateIssueNoAsync(DateOnly issueDate, CancellationToken ct)
    {
        var prefix = $"GI-{issueDate:yyyyMM}-";
        var latest = await dbContext.InvGoodsIssues
            .IgnoreQueryFilters()
            .Where(x => x.IssueNo.StartsWith(prefix))
            .OrderByDescending(x => x.IssueNo)
            .Select(x => x.IssueNo)
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

    private static GoodsIssueDto MapDto(InvGoodsIssue entity, bool includeLines)
    {
        return new GoodsIssueDto
        {
            Id = entity.Id,
            IssueNo = entity.IssueNo,
            IssueDate = entity.IssueDate,
            IssueType = entity.IssueType,
            WarehouseId = entity.WarehouseId,
            WarehouseCode = entity.Warehouse.Code,
            WarehouseName = entity.Warehouse.Name,
            LocationId = entity.LocationId,
            LocationCode = entity.Location?.Code,
            DepartmentId = entity.DepartmentId,
            DepartmentCode = entity.Department?.Code,
            DepartmentName = entity.Department?.Name,
            CostCenterId = entity.CostCenterId,
            CostCenterCode = entity.CostCenter?.Code,
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
                    .Select(x => new GoodsIssueLineDto
                    {
                        Id = x.Id,
                        LineNo = x.LineNo,
                        ItemId = x.ItemId,
                        ItemCode = x.Item.ItemCode,
                        ItemName = x.Item.Name,
                        UomId = x.UomId,
                        UomCode = x.Uom?.Code,
                        QtyRequested = x.QtyRequested,
                        QtyIssued = x.QtyIssued,
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
