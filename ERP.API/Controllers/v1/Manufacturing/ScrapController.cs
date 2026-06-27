using System.Linq.Expressions;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Manufacturing;
using ERP.Domain.Entities.Manufacturing;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Manufacturing;

[Route("api/v1/manufacturing/scrap")]
public sealed class ScrapController(AppDbContext dbContext) : ManufacturingControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ManufacturingScrapRecordPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.MfgScrapRecords
            .AsNoTracking()
            .Include(x => x.WorkOrder)
            .Include(x => x.Item)
            .Include(x => x.WorkCenter)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(search)
                || (x.WorkOrder != null && x.WorkOrder.Code.ToLower().Contains(search))
                || (x.Item != null && x.Item.ItemCode.ToLower().Contains(search))
                || (x.Item != null && x.Item.Name.ToLower().Contains(search))
                || (x.WorkCenter != null && x.WorkCenter.Name.ToLower().Contains(search))
                || (x.Notes != null && x.Notes.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var code = request.Code.Trim().ToLowerInvariant();
            query = query.Where(x => x.Code.ToLower().Contains(code));
        }

        if (request.WorkOrderId.HasValue)
        {
            query = query.Where(x => x.WorkOrderId == request.WorkOrderId.Value);
        }

        if (request.ItemId.HasValue)
        {
            query = query.Where(x => x.ItemId == request.ItemId.Value);
        }

        if (request.WorkCenterId.HasValue)
        {
            query = query.Where(x => x.WorkCenterId == request.WorkCenterId.Value);
        }

        if (request.Reason.HasValue)
        {
            query = query.Where(x => x.Reason == request.Reason.Value);
        }

        if (request.RecordedFrom.HasValue)
        {
            query = query.Where(x => x.RecordedAt >= request.RecordedFrom.Value);
        }

        if (request.RecordedTo.HasValue)
        {
            query = query.Where(x => x.RecordedAt <= request.RecordedTo.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "reason" => isDesc ? query.OrderByDescending(x => x.Reason).ThenByDescending(x => x.Code) : query.OrderBy(x => x.Reason).ThenBy(x => x.Code),
            "qtyscrap" => isDesc ? query.OrderByDescending(x => x.QtyScrap).ThenByDescending(x => x.Code) : query.OrderBy(x => x.QtyScrap).ThenBy(x => x.Code),
            "totalscrapcost" => isDesc ? query.OrderByDescending(x => x.TotalScrapCost).ThenByDescending(x => x.Code) : query.OrderBy(x => x.TotalScrapCost).ThenBy(x => x.Code),
            "recordedat" => isDesc ? query.OrderByDescending(x => x.RecordedAt).ThenByDescending(x => x.Code) : query.OrderBy(x => x.RecordedAt).ThenBy(x => x.Code),
            _ => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapScrapDto())
            .ToListAsync(ct);

        return Ok(PagedResult<ManufacturingScrapRecordDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.MfgScrapRecords
            .AsNoTracking()
            .Include(x => x.WorkOrder)
            .Include(x => x.Item)
            .Include(x => x.WorkCenter)
            .Where(x => x.Id == id)
            .Select(MapScrapDto())
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ManufacturingScrapRecordDto request, CancellationToken ct)
    {
        try
        {
            var normalizedCode = NormalizeRequired(request.Code, "Scrap code is required.").ToUpperInvariant();
            var validation = await ValidateScrapAsync(normalizedCode, request.WorkOrderId, request.ItemId, request.WorkCenterId, request.QtyScrap, request.UnitCost, null, ct);
            if (validation is not null)
            {
                return validation;
            }

            var qtyScrap = Round4(request.QtyScrap);
            var unitCost = Round4(request.UnitCost);
            var totalScrapCost = Round4(qtyScrap * unitCost);

            var entity = new MfgScrapRecord
            {
                Code = normalizedCode,
                WorkOrderId = NormalizeOptionalId(request.WorkOrderId),
                ItemId = NormalizeOptionalId(request.ItemId),
                WorkCenterId = NormalizeOptionalId(request.WorkCenterId),
                Reason = request.Reason,
                QtyScrap = qtyScrap,
                UnitCost = unitCost,
                TotalScrapCost = totalScrapCost,
                RecordedAt = request.RecordedAt == default ? DateTimeOffset.UtcNow : request.RecordedAt,
                Notes = NormalizeOptional(request.Notes),
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            };

            dbContext.MfgScrapRecords.Add(entity);

            if (entity.WorkOrderId.HasValue)
            {
                await AdjustWorkOrderScrapAsync(entity.WorkOrderId.Value, qtyScrap, ct);
            }

            await dbContext.SaveChangesAsync(ct);

            var created = await dbContext.MfgScrapRecords
                .AsNoTracking()
                .Include(x => x.WorkOrder)
                .Include(x => x.Item)
                .Include(x => x.WorkCenter)
                .Where(x => x.Id == entity.Id)
                .Select(MapScrapDto())
                .FirstAsync(ct);

            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ManufacturingScrapRecordDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.MfgScrapRecords.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            var normalizedCode = NormalizeRequired(request.Code, "Scrap code is required.").ToUpperInvariant();
            var validation = await ValidateScrapAsync(normalizedCode, request.WorkOrderId, request.ItemId, request.WorkCenterId, request.QtyScrap, request.UnitCost, id, ct);
            if (validation is not null)
            {
                return validation;
            }

            var oldWorkOrderId = entity.WorkOrderId;
            var oldQtyScrap = entity.QtyScrap;

            var newQtyScrap = Round4(request.QtyScrap);
            var newUnitCost = Round4(request.UnitCost);
            var newTotalScrapCost = Round4(newQtyScrap * newUnitCost);
            var newWorkOrderId = NormalizeOptionalId(request.WorkOrderId);

            entity.Code = normalizedCode;
            entity.WorkOrderId = newWorkOrderId;
            entity.ItemId = NormalizeOptionalId(request.ItemId);
            entity.WorkCenterId = NormalizeOptionalId(request.WorkCenterId);
            entity.Reason = request.Reason;
            entity.QtyScrap = newQtyScrap;
            entity.UnitCost = newUnitCost;
            entity.TotalScrapCost = newTotalScrapCost;
            entity.RecordedAt = request.RecordedAt == default ? entity.RecordedAt : request.RecordedAt;
            entity.Notes = NormalizeOptional(request.Notes);
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            if (oldWorkOrderId.HasValue)
            {
                await AdjustWorkOrderScrapAsync(oldWorkOrderId.Value, -oldQtyScrap, ct);
            }

            if (newWorkOrderId.HasValue)
            {
                await AdjustWorkOrderScrapAsync(newWorkOrderId.Value, newQtyScrap, ct);
            }

            await dbContext.SaveChangesAsync(ct);

            var updated = await dbContext.MfgScrapRecords
                .AsNoTracking()
                .Include(x => x.WorkOrder)
                .Include(x => x.Item)
                .Include(x => x.WorkCenter)
                .Where(x => x.Id == id)
                .Select(MapScrapDto())
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
        var entity = await dbContext.MfgScrapRecords.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.WorkOrderId.HasValue)
        {
            await AdjustWorkOrderScrapAsync(entity.WorkOrderId.Value, -entity.QtyScrap, ct);
        }

        dbContext.MfgScrapRecords.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task<IActionResult?> ValidateScrapAsync(
        string code,
        int? workOrderId,
        int? itemId,
        int? workCenterId,
        decimal qtyScrap,
        decimal unitCost,
        int? id,
        CancellationToken ct)
    {
        if (qtyScrap <= 0)
        {
            return BadRequest(new { message = "Scrap quantity must be greater than 0." });
        }

        if (unitCost < 0)
        {
            return BadRequest(new { message = "Unit cost cannot be negative." });
        }

        var normalizedWorkOrderId = NormalizeOptionalId(workOrderId);
        if (normalizedWorkOrderId.HasValue)
        {
            var woExists = await dbContext.MfgWorkOrders.AnyAsync(x => x.Id == normalizedWorkOrderId.Value, ct);
            if (!woExists)
            {
                return BadRequest(new { message = "Work order not found." });
            }
        }

        var normalizedItemId = NormalizeOptionalId(itemId);
        if (normalizedItemId.HasValue)
        {
            var itemExists = await dbContext.InvItems.AnyAsync(x => x.Id == normalizedItemId.Value, ct);
            if (!itemExists)
            {
                return BadRequest(new { message = "Item not found." });
            }
        }

        var normalizedWorkCenterId = NormalizeOptionalId(workCenterId);
        if (normalizedWorkCenterId.HasValue)
        {
            var workCenterExists = await dbContext.MfgWorkCenters.AnyAsync(x => x.Id == normalizedWorkCenterId.Value, ct);
            if (!workCenterExists)
            {
                return BadRequest(new { message = "Work center not found." });
            }
        }

        var duplicateCode = await dbContext.MfgScrapRecords
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id != (id ?? 0) && x.Code == code, ct);

        if (duplicateCode)
        {
            return BadRequest(new { message = "Scrap code already exists." });
        }

        return null;
    }

    private async Task AdjustWorkOrderScrapAsync(int workOrderId, decimal deltaQtyScrap, CancellationToken ct)
    {
        var workOrder = await dbContext.MfgWorkOrders.FirstOrDefaultAsync(x => x.Id == workOrderId, ct);
        if (workOrder is null)
        {
            return;
        }

        var nextQtyScrap = workOrder.QtyScrap + deltaQtyScrap;
        if (nextQtyScrap < 0)
        {
            nextQtyScrap = 0;
        }

        workOrder.QtyScrap = Round4(nextQtyScrap);
        workOrder.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        workOrder.UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static int? NormalizeOptionalId(int? value)
    {
        if (!value.HasValue || value.Value <= 0)
        {
            return null;
        }

        return value.Value;
    }

    private static decimal Round4(decimal value)
        => decimal.Round(value, 4, MidpointRounding.AwayFromZero);

    private static string NormalizeRequired(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(message);
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static Expression<Func<MfgScrapRecord, ManufacturingScrapRecordDto>> MapScrapDto()
    {
        return x => new ManufacturingScrapRecordDto
        {
            Id = x.Id,
            Code = x.Code,
            WorkOrderId = x.WorkOrderId,
            WorkOrderCode = x.WorkOrder != null ? x.WorkOrder.Code : null,
            ItemId = x.ItemId,
            ItemCode = x.Item != null ? x.Item.ItemCode : null,
            ItemName = x.Item != null ? x.Item.Name : null,
            WorkCenterId = x.WorkCenterId,
            WorkCenterCode = x.WorkCenter != null ? x.WorkCenter.Code : null,
            WorkCenterName = x.WorkCenter != null ? x.WorkCenter.Name : null,
            Reason = x.Reason,
            QtyScrap = x.QtyScrap,
            UnitCost = x.UnitCost,
            TotalScrapCost = x.TotalScrapCost,
            RecordedAt = x.RecordedAt,
            Notes = x.Notes
        };
    }
}
