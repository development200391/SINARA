using System.Linq.Expressions;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Manufacturing;
using ERP.Domain.Entities.Manufacturing;
using ERP.Domain.Enums.Manufacturing;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Manufacturing;

[Route("api/v1/manufacturing/qc")]
public sealed class QcController(AppDbContext dbContext) : ManufacturingControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ManufacturingQcInspectionPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.MfgQcInspections
            .AsNoTracking()
            .Include(x => x.WorkOrder)
            .Include(x => x.Item)
            .Include(x => x.InspectorEmployee)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(search)
                || (x.WorkOrder != null && x.WorkOrder.Code.ToLower().Contains(search))
                || (x.Item != null && x.Item.ItemCode.ToLower().Contains(search))
                || (x.Item != null && x.Item.Name.ToLower().Contains(search))
                || (x.InspectorEmployee != null && x.InspectorEmployee.FullName.ToLower().Contains(search))
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

        if (request.InspectorEmployeeId.HasValue)
        {
            query = query.Where(x => x.InspectorEmployeeId == request.InspectorEmployeeId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (request.Result.HasValue)
        {
            query = query.Where(x => x.Result == request.Result.Value);
        }

        if (request.InspectedFrom.HasValue)
        {
            query = query.Where(x => x.InspectedAt >= request.InspectedFrom.Value);
        }

        if (request.InspectedTo.HasValue)
        {
            query = query.Where(x => x.InspectedAt <= request.InspectedTo.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "workordercode" => isDesc ? query.OrderByDescending(x => x.WorkOrder != null ? x.WorkOrder.Code : string.Empty).ThenByDescending(x => x.Code) : query.OrderBy(x => x.WorkOrder != null ? x.WorkOrder.Code : string.Empty).ThenBy(x => x.Code),
            "itemcode" => isDesc ? query.OrderByDescending(x => x.Item != null ? x.Item.ItemCode : string.Empty).ThenByDescending(x => x.Code) : query.OrderBy(x => x.Item != null ? x.Item.ItemCode : string.Empty).ThenBy(x => x.Code),
            "status" => isDesc ? query.OrderByDescending(x => x.Status).ThenByDescending(x => x.Code) : query.OrderBy(x => x.Status).ThenBy(x => x.Code),
            "result" => isDesc ? query.OrderByDescending(x => x.Result).ThenByDescending(x => x.Code) : query.OrderBy(x => x.Result).ThenBy(x => x.Code),
            "inspectedat" => isDesc ? query.OrderByDescending(x => x.InspectedAt).ThenByDescending(x => x.Code) : query.OrderBy(x => x.InspectedAt).ThenBy(x => x.Code),
            _ => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapQcInspectionDto())
            .ToListAsync(ct);

        return Ok(PagedResult<ManufacturingQcInspectionDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.MfgQcInspections
            .AsNoTracking()
            .Include(x => x.WorkOrder)
            .Include(x => x.Item)
            .Include(x => x.InspectorEmployee)
            .Where(x => x.Id == id)
            .Select(MapQcInspectionDto())
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ManufacturingQcInspectionDto request, CancellationToken ct)
    {
        try
        {
            var normalizedCode = NormalizeRequired(request.Code, "QC inspection code is required.").ToUpperInvariant();
            var normalizedInspectedAt = request.InspectedAt == default ? DateTimeOffset.UtcNow : request.InspectedAt;

            var validation = await ValidateQcInspectionAsync(normalizedCode, request.WorkOrderId, request.ItemId, request.InspectorEmployeeId, null, ct);
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

            var entity = new MfgQcInspection
            {
                Code = normalizedCode,
                WorkOrderId = NormalizeOptionalId(request.WorkOrderId),
                ItemId = itemId,
                InspectorEmployeeId = NormalizeOptionalId(request.InspectorEmployeeId),
                InspectedAt = normalizedInspectedAt,
                Status = request.Status,
                Result = request.Result,
                Notes = NormalizeOptional(request.Notes),
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            };

            dbContext.MfgQcInspections.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var created = await dbContext.MfgQcInspections
                .AsNoTracking()
                .Include(x => x.WorkOrder)
                .Include(x => x.Item)
                .Include(x => x.InspectorEmployee)
                .Where(x => x.Id == entity.Id)
                .Select(MapQcInspectionDto())
                .FirstAsync(ct);

            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ManufacturingQcInspectionDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.MfgQcInspections.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            if (entity.Status is QcStatus.Passed or QcStatus.Failed or QcStatus.ConditionalPass or QcStatus.Cancelled)
            {
                return BadRequest(new { message = "Completed or cancelled QC inspection cannot be edited." });
            }

            var normalizedCode = NormalizeRequired(request.Code, "QC inspection code is required.").ToUpperInvariant();
            var normalizedInspectedAt = request.InspectedAt == default ? entity.InspectedAt : request.InspectedAt;

            var validation = await ValidateQcInspectionAsync(normalizedCode, request.WorkOrderId, request.ItemId, request.InspectorEmployeeId, id, ct);
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
            entity.WorkOrderId = NormalizeOptionalId(request.WorkOrderId);
            entity.ItemId = itemId;
            entity.InspectorEmployeeId = NormalizeOptionalId(request.InspectorEmployeeId);
            entity.InspectedAt = normalizedInspectedAt;
            entity.Status = request.Status;
            entity.Result = request.Result;
            entity.Notes = NormalizeOptional(request.Notes);
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(ct);

            var updated = await dbContext.MfgQcInspections
                .AsNoTracking()
                .Include(x => x.WorkOrder)
                .Include(x => x.Item)
                .Include(x => x.InspectorEmployee)
                .Where(x => x.Id == id)
                .Select(MapQcInspectionDto())
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
        var entity = await dbContext.MfgQcInspections.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status is not QcStatus.Pending)
        {
            return BadRequest(new { message = "Only pending QC inspection can be deleted." });
        }

        dbContext.MfgQcInspections.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPost("{id:int}/start")]
    public async Task<IActionResult> Start(int id, CancellationToken ct)
    {
        var entity = await dbContext.MfgQcInspections.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status is not QcStatus.Pending)
        {
            return BadRequest(new { message = "Only pending QC inspection can be started." });
        }

        entity.Status = QcStatus.InProgress;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return Ok(new { message = "QC inspection started." });
    }

    [HttpPost("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id, [FromBody] ManufacturingQcCompleteRequest request, CancellationToken ct)
    {
        var entity = await dbContext.MfgQcInspections.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status is not QcStatus.Pending and not QcStatus.InProgress)
        {
            return BadRequest(new { message = "Only pending or in-progress QC inspection can be completed." });
        }

        entity.Result = request.Result;
        entity.Status = request.Result switch
        {
            QcResult.Pass => QcStatus.Passed,
            QcResult.Fail => QcStatus.Failed,
            QcResult.ConditionalPass => QcStatus.ConditionalPass,
            _ => QcStatus.Passed
        };
        entity.InspectedAt = DateTimeOffset.UtcNow;
        entity.Notes = MergeNotes(entity.Notes, request.Notes);
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return Ok(new { message = "QC inspection completed." });
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        var entity = await dbContext.MfgQcInspections.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status is QcStatus.Passed or QcStatus.Failed or QcStatus.ConditionalPass)
        {
            return BadRequest(new { message = "Completed QC inspection cannot be cancelled." });
        }

        if (entity.Status == QcStatus.Cancelled)
        {
            return Ok(new { message = "QC inspection already cancelled." });
        }

        entity.Status = QcStatus.Cancelled;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return Ok(new { message = "QC inspection cancelled." });
    }

    private async Task<IActionResult?> ValidateQcInspectionAsync(string code, int? workOrderId, int? itemId, int? inspectorEmployeeId, int? id, CancellationToken ct)
    {
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

        var normalizedInspectorEmployeeId = NormalizeOptionalId(inspectorEmployeeId);
        if (normalizedInspectorEmployeeId.HasValue)
        {
            var employeeExists = await dbContext.HrEmployees.AnyAsync(x => x.Id == normalizedInspectorEmployeeId.Value, ct);
            if (!employeeExists)
            {
                return BadRequest(new { message = "Inspector employee not found." });
            }
        }

        var duplicateCode = await dbContext.MfgQcInspections
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id != (id ?? 0) && x.Code == code, ct);

        if (duplicateCode)
        {
            return BadRequest(new { message = "QC inspection code already exists." });
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

    private static string? MergeNotes(string? current, string? append)
    {
        if (string.IsNullOrWhiteSpace(append))
        {
            return current;
        }

        var normalizedAppend = append.Trim();
        if (string.IsNullOrWhiteSpace(current))
        {
            return normalizedAppend;
        }

        return current.Trim() + Environment.NewLine + normalizedAppend;
    }

    private static Expression<Func<MfgQcInspection, ManufacturingQcInspectionDto>> MapQcInspectionDto()
    {
        return x => new ManufacturingQcInspectionDto
        {
            Id = x.Id,
            Code = x.Code,
            WorkOrderId = x.WorkOrderId,
            WorkOrderCode = x.WorkOrder != null ? x.WorkOrder.Code : null,
            ItemId = x.ItemId,
            ItemCode = x.Item != null ? x.Item.ItemCode : null,
            ItemName = x.Item != null ? x.Item.Name : null,
            InspectorEmployeeId = x.InspectorEmployeeId,
            InspectorEmployeeCode = x.InspectorEmployee != null ? x.InspectorEmployee.EmployeeCode : null,
            InspectorEmployeeName = x.InspectorEmployee != null ? x.InspectorEmployee.FullName : null,
            InspectedAt = x.InspectedAt,
            Status = x.Status,
            Result = x.Result,
            Notes = x.Notes
        };
    }
}
