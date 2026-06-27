using System.Linq.Expressions;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Manufacturing;
using ERP.Domain.Entities.Manufacturing;
using ERP.Domain.Enums.Manufacturing;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Manufacturing;

[Route("api/v1/manufacturing/rework")]
public sealed class ReworkController(AppDbContext dbContext) : ManufacturingControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ManufacturingReworkOrderPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.MfgReworkOrders
            .AsNoTracking()
            .Include(x => x.SourceWorkOrder)
            .Include(x => x.WorkOrder)
            .Include(x => x.Item)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(search)
                || (x.SourceWorkOrder != null && x.SourceWorkOrder.Code.ToLower().Contains(search))
                || (x.WorkOrder != null && x.WorkOrder.Code.ToLower().Contains(search))
                || (x.Item != null && x.Item.ItemCode.ToLower().Contains(search))
                || (x.Item != null && x.Item.Name.ToLower().Contains(search))
                || (x.Notes != null && x.Notes.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var code = request.Code.Trim().ToLowerInvariant();
            query = query.Where(x => x.Code.ToLower().Contains(code));
        }

        if (request.ItemId.HasValue)
        {
            query = query.Where(x => x.ItemId == request.ItemId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (request.OpenedFrom.HasValue)
        {
            query = query.Where(x => x.OpenedAt >= request.OpenedFrom.Value);
        }

        if (request.OpenedTo.HasValue)
        {
            query = query.Where(x => x.OpenedAt <= request.OpenedTo.Value);
        }

        if (request.ClosedFrom.HasValue)
        {
            query = query.Where(x => x.ClosedAt.HasValue && x.ClosedAt.Value >= request.ClosedFrom.Value);
        }

        if (request.ClosedTo.HasValue)
        {
            query = query.Where(x => x.ClosedAt.HasValue && x.ClosedAt.Value <= request.ClosedTo.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "status" => isDesc ? query.OrderByDescending(x => x.Status).ThenByDescending(x => x.Code) : query.OrderBy(x => x.Status).ThenBy(x => x.Code),
            "qtyrework" => isDesc ? query.OrderByDescending(x => x.QtyRework).ThenByDescending(x => x.Code) : query.OrderBy(x => x.QtyRework).ThenBy(x => x.Code),
            "openedat" => isDesc ? query.OrderByDescending(x => x.OpenedAt).ThenByDescending(x => x.Code) : query.OrderBy(x => x.OpenedAt).ThenBy(x => x.Code),
            "closedat" => isDesc ? query.OrderByDescending(x => x.ClosedAt).ThenByDescending(x => x.Code) : query.OrderBy(x => x.ClosedAt).ThenBy(x => x.Code),
            _ => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapReworkDto())
            .ToListAsync(ct);

        return Ok(PagedResult<ManufacturingReworkOrderDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.MfgReworkOrders
            .AsNoTracking()
            .Include(x => x.SourceWorkOrder)
            .Include(x => x.WorkOrder)
            .Include(x => x.Item)
            .Where(x => x.Id == id)
            .Select(MapReworkDto())
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ManufacturingReworkOrderDto request, CancellationToken ct)
    {
        try
        {
            var normalizedCode = NormalizeRequired(request.Code, "Rework code is required.").ToUpperInvariant();
            var validation = await ValidateReworkAsync(normalizedCode, request, null, ct);
            if (validation is not null)
            {
                return validation;
            }

            var itemId = NormalizeOptionalId(request.ItemId);
            if (!itemId.HasValue && request.WorkOrderId.HasValue)
            {
                itemId = await dbContext.MfgWorkOrders
                    .Where(x => x.Id == request.WorkOrderId.Value)
                    .Select(x => x.ItemId)
                    .FirstOrDefaultAsync(ct);
            }

            var entity = new MfgReworkOrder
            {
                Code = normalizedCode,
                SourceWorkOrderId = NormalizeOptionalId(request.SourceWorkOrderId),
                WorkOrderId = NormalizeOptionalId(request.WorkOrderId),
                ItemId = itemId,
                QtyRework = Round4(request.QtyRework),
                Status = request.Status,
                OpenedAt = request.OpenedAt == default ? DateTimeOffset.UtcNow : request.OpenedAt,
                ClosedAt = request.ClosedAt,
                Notes = NormalizeOptional(request.Notes),
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            };

            dbContext.MfgReworkOrders.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var created = await dbContext.MfgReworkOrders
                .AsNoTracking()
                .Include(x => x.SourceWorkOrder)
                .Include(x => x.WorkOrder)
                .Include(x => x.Item)
                .Where(x => x.Id == entity.Id)
                .Select(MapReworkDto())
                .FirstAsync(ct);

            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ManufacturingReworkOrderDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.MfgReworkOrders.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            if (entity.Status is WorkOrderStatus.Closed or WorkOrderStatus.Cancelled)
            {
                return BadRequest(new { message = "Closed or cancelled rework cannot be edited." });
            }

            var normalizedCode = NormalizeRequired(request.Code, "Rework code is required.").ToUpperInvariant();
            var validation = await ValidateReworkAsync(normalizedCode, request, id, ct);
            if (validation is not null)
            {
                return validation;
            }

            var itemId = NormalizeOptionalId(request.ItemId);
            if (!itemId.HasValue && request.WorkOrderId.HasValue)
            {
                itemId = await dbContext.MfgWorkOrders
                    .Where(x => x.Id == request.WorkOrderId.Value)
                    .Select(x => x.ItemId)
                    .FirstOrDefaultAsync(ct);
            }

            entity.Code = normalizedCode;
            entity.SourceWorkOrderId = NormalizeOptionalId(request.SourceWorkOrderId);
            entity.WorkOrderId = NormalizeOptionalId(request.WorkOrderId);
            entity.ItemId = itemId;
            entity.QtyRework = Round4(request.QtyRework);
            entity.Status = request.Status;
            entity.OpenedAt = request.OpenedAt == default ? entity.OpenedAt : request.OpenedAt;
            entity.ClosedAt = request.ClosedAt;
            entity.Notes = NormalizeOptional(request.Notes);
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(ct);

            var updated = await dbContext.MfgReworkOrders
                .AsNoTracking()
                .Include(x => x.SourceWorkOrder)
                .Include(x => x.WorkOrder)
                .Include(x => x.Item)
                .Where(x => x.Id == id)
                .Select(MapReworkDto())
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
        var entity = await dbContext.MfgReworkOrders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status is not WorkOrderStatus.Draft)
        {
            return BadRequest(new { message = "Only draft rework can be deleted." });
        }

        dbContext.MfgReworkOrders.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPost("{id:int}/start")]
    public async Task<IActionResult> Start(int id, CancellationToken ct)
    {
        var entity = await dbContext.MfgReworkOrders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status is not WorkOrderStatus.Draft and not WorkOrderStatus.Planned and not WorkOrderStatus.Released)
        {
            return BadRequest(new { message = "Only draft/planned/released rework can be started." });
        }

        entity.Status = WorkOrderStatus.InProgress;
        entity.OpenedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return Ok(new { message = "Rework started." });
    }

    [HttpPost("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id, CancellationToken ct)
    {
        var entity = await dbContext.MfgReworkOrders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status is not WorkOrderStatus.InProgress)
        {
            return BadRequest(new { message = "Only in-progress rework can be completed." });
        }

        entity.Status = WorkOrderStatus.Completed;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return Ok(new { message = "Rework completed." });
    }

    [HttpPost("{id:int}/close")]
    public async Task<IActionResult> Close(int id, CancellationToken ct)
    {
        var entity = await dbContext.MfgReworkOrders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status is not WorkOrderStatus.Completed)
        {
            return BadRequest(new { message = "Only completed rework can be closed." });
        }

        entity.Status = WorkOrderStatus.Closed;
        entity.ClosedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return Ok(new { message = "Rework closed." });
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        var entity = await dbContext.MfgReworkOrders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status == WorkOrderStatus.Closed)
        {
            return BadRequest(new { message = "Closed rework cannot be cancelled." });
        }

        if (entity.Status == WorkOrderStatus.Cancelled)
        {
            return Ok(new { message = "Rework already cancelled." });
        }

        entity.Status = WorkOrderStatus.Cancelled;
        entity.ClosedAt ??= DateTimeOffset.UtcNow;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return Ok(new { message = "Rework cancelled." });
    }

    private async Task<IActionResult?> ValidateReworkAsync(string code, ManufacturingReworkOrderDto request, int? id, CancellationToken ct)
    {
        if (request.QtyRework <= 0)
        {
            return BadRequest(new { message = "Rework quantity must be greater than 0." });
        }

        if (request.OpenedAt != default && request.ClosedAt.HasValue && request.ClosedAt.Value < request.OpenedAt)
        {
            return BadRequest(new { message = "Closed date cannot be earlier than opened date." });
        }

        var sourceWorkOrderId = NormalizeOptionalId(request.SourceWorkOrderId);
        if (sourceWorkOrderId.HasValue)
        {
            var sourceWoExists = await dbContext.MfgWorkOrders.AnyAsync(x => x.Id == sourceWorkOrderId.Value, ct);
            if (!sourceWoExists)
            {
                return BadRequest(new { message = "Source work order not found." });
            }
        }

        var workOrderId = NormalizeOptionalId(request.WorkOrderId);
        if (workOrderId.HasValue)
        {
            var woExists = await dbContext.MfgWorkOrders.AnyAsync(x => x.Id == workOrderId.Value, ct);
            if (!woExists)
            {
                return BadRequest(new { message = "Work order not found." });
            }
        }

        var itemId = NormalizeOptionalId(request.ItemId);
        if (itemId.HasValue)
        {
            var itemExists = await dbContext.InvItems.AnyAsync(x => x.Id == itemId.Value, ct);
            if (!itemExists)
            {
                return BadRequest(new { message = "Item not found." });
            }
        }

        var duplicateCode = await dbContext.MfgReworkOrders
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id != (id ?? 0) && x.Code == code, ct);

        if (duplicateCode)
        {
            return BadRequest(new { message = "Rework code already exists." });
        }

        return null;
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

    private static Expression<Func<MfgReworkOrder, ManufacturingReworkOrderDto>> MapReworkDto()
    {
        return x => new ManufacturingReworkOrderDto
        {
            Id = x.Id,
            Code = x.Code,
            SourceWorkOrderId = x.SourceWorkOrderId,
            SourceWorkOrderCode = x.SourceWorkOrder != null ? x.SourceWorkOrder.Code : null,
            WorkOrderId = x.WorkOrderId,
            WorkOrderCode = x.WorkOrder != null ? x.WorkOrder.Code : null,
            ItemId = x.ItemId,
            ItemCode = x.Item != null ? x.Item.ItemCode : null,
            ItemName = x.Item != null ? x.Item.Name : null,
            QtyRework = x.QtyRework,
            Status = x.Status,
            OpenedAt = x.OpenedAt,
            ClosedAt = x.ClosedAt,
            Notes = x.Notes
        };
    }
}
