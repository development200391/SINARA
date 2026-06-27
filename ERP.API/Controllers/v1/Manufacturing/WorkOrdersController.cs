using System.Linq.Expressions;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Manufacturing;
using ERP.Domain.Entities.Manufacturing;
using ERP.Domain.Enums.Manufacturing;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Manufacturing;

[Route("api/v1/manufacturing/work-orders")]
public sealed class WorkOrdersController(AppDbContext dbContext) : ManufacturingControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ManufacturingWorkOrderPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.MfgWorkOrders
            .AsNoTracking()
            .Include(x => x.Item)
            .Include(x => x.Bom)
            .Include(x => x.Routing)
            .Include(x => x.WorkCenter)
            .Include(x => x.MrpRun)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(search)
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

        if (request.ItemId.HasValue)
        {
            query = query.Where(x => x.ItemId == request.ItemId.Value);
        }

        if (request.WorkCenterId.HasValue)
        {
            query = query.Where(x => x.WorkCenterId == request.WorkCenterId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (request.ProductionType.HasValue)
        {
            query = query.Where(x => x.ProductionType == request.ProductionType.Value);
        }

        if (request.PlannedStartFrom.HasValue)
        {
            query = query.Where(x => x.PlannedStartDate >= request.PlannedStartFrom.Value);
        }

        if (request.PlannedStartTo.HasValue)
        {
            query = query.Where(x => x.PlannedStartDate <= request.PlannedStartTo.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "status" => isDesc ? query.OrderByDescending(x => x.Status).ThenByDescending(x => x.Code) : query.OrderBy(x => x.Status).ThenBy(x => x.Code),
            "productiontype" => isDesc ? query.OrderByDescending(x => x.ProductionType).ThenByDescending(x => x.Code) : query.OrderBy(x => x.ProductionType).ThenBy(x => x.Code),
            "qtyplanned" => isDesc ? query.OrderByDescending(x => x.QtyPlanned).ThenByDescending(x => x.Code) : query.OrderBy(x => x.QtyPlanned).ThenBy(x => x.Code),
            "plannedstartdate" => isDesc ? query.OrderByDescending(x => x.PlannedStartDate).ThenByDescending(x => x.Code) : query.OrderBy(x => x.PlannedStartDate).ThenBy(x => x.Code),
            "workcentername" => isDesc ? query.OrderByDescending(x => x.WorkCenter != null ? x.WorkCenter.Name : string.Empty).ThenByDescending(x => x.Code) : query.OrderBy(x => x.WorkCenter != null ? x.WorkCenter.Name : string.Empty).ThenBy(x => x.Code),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Code),
            _ => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapWorkOrderDto())
            .ToListAsync(ct);

        return Ok(PagedResult<ManufacturingWorkOrderDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.MfgWorkOrders
            .AsNoTracking()
            .Include(x => x.Item)
            .Include(x => x.Bom)
            .Include(x => x.Routing)
            .Include(x => x.WorkCenter)
            .Include(x => x.MrpRun)
            .Where(x => x.Id == id)
            .Select(MapWorkOrderDto())
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ManufacturingWorkOrderDto request, CancellationToken ct)
    {
        try
        {
            var normalizedCode = NormalizeRequired(request.Code, "Work order code is required.").ToUpperInvariant();
            var validation = await ValidateWorkOrderAsync(normalizedCode, request, null, ct);
            if (validation is not null)
            {
                return validation;
            }

            var entity = new MfgWorkOrder
            {
                Code = normalizedCode,
                ItemId = request.ItemId,
                BomId = request.BomId,
                RoutingId = NormalizeOptionalId(request.RoutingId),
                WorkCenterId = NormalizeOptionalId(request.WorkCenterId),
                MrpRunId = NormalizeOptionalId(request.MrpRunId),
                Status = request.Status,
                ProductionType = request.ProductionType,
                QtyPlanned = Round4(request.QtyPlanned),
                QtyGood = Round4(request.QtyGood),
                QtyScrap = Round4(request.QtyScrap),
                PlannedStartDate = request.PlannedStartDate,
                PlannedEndDate = request.PlannedEndDate,
                ActualStartAt = request.ActualStartAt,
                ActualEndAt = request.ActualEndAt,
                StandardCostTotal = Round4(request.StandardCostTotal),
                ActualCostTotal = Round4(request.ActualCostTotal),
                IsActive = request.IsActive,
                Notes = NormalizeOptional(request.Notes),
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            };

            dbContext.MfgWorkOrders.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var created = await dbContext.MfgWorkOrders
                .AsNoTracking()
                .Include(x => x.Item)
                .Include(x => x.Bom)
                .Include(x => x.Routing)
                .Include(x => x.WorkCenter)
                .Include(x => x.MrpRun)
                .Where(x => x.Id == entity.Id)
                .Select(MapWorkOrderDto())
                .FirstAsync(ct);

            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ManufacturingWorkOrderDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.MfgWorkOrders.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            if (entity.Status is WorkOrderStatus.Closed or WorkOrderStatus.Cancelled)
            {
                return BadRequest(new { message = "Closed or cancelled work order cannot be edited." });
            }

            var normalizedCode = NormalizeRequired(request.Code, "Work order code is required.").ToUpperInvariant();
            var validation = await ValidateWorkOrderAsync(normalizedCode, request, id, ct);
            if (validation is not null)
            {
                return validation;
            }

            entity.Code = normalizedCode;
            entity.ItemId = request.ItemId;
            entity.BomId = request.BomId;
            entity.RoutingId = NormalizeOptionalId(request.RoutingId);
            entity.WorkCenterId = NormalizeOptionalId(request.WorkCenterId);
            entity.MrpRunId = NormalizeOptionalId(request.MrpRunId);
            entity.Status = request.Status;
            entity.ProductionType = request.ProductionType;
            entity.QtyPlanned = Round4(request.QtyPlanned);
            entity.QtyGood = Round4(request.QtyGood);
            entity.QtyScrap = Round4(request.QtyScrap);
            entity.PlannedStartDate = request.PlannedStartDate;
            entity.PlannedEndDate = request.PlannedEndDate;
            entity.ActualStartAt = request.ActualStartAt;
            entity.ActualEndAt = request.ActualEndAt;
            entity.StandardCostTotal = Round4(request.StandardCostTotal);
            entity.ActualCostTotal = Round4(request.ActualCostTotal);
            entity.IsActive = request.IsActive;
            entity.Notes = NormalizeOptional(request.Notes);
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(ct);

            var updated = await dbContext.MfgWorkOrders
                .AsNoTracking()
                .Include(x => x.Item)
                .Include(x => x.Bom)
                .Include(x => x.Routing)
                .Include(x => x.WorkCenter)
                .Include(x => x.MrpRun)
                .Where(x => x.Id == id)
                .Select(MapWorkOrderDto())
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
        var entity = await dbContext.MfgWorkOrders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status is not WorkOrderStatus.Draft and not WorkOrderStatus.Planned)
        {
            return BadRequest(new { message = "Only draft or planned work order can be deleted." });
        }

        var hasRelated = await dbContext.MfgQcInspections.AnyAsync(x => x.WorkOrderId == id, ct)
            || await dbContext.MfgScrapRecords.AnyAsync(x => x.WorkOrderId == id, ct)
            || await dbContext.MfgReworkOrders.AnyAsync(x => x.WorkOrderId == id || x.SourceWorkOrderId == id, ct);

        if (hasRelated)
        {
            return BadRequest(new { message = "Work order already has related transactions and cannot be deleted." });
        }

        dbContext.MfgWorkOrders.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPost("{id:int}/release")]
    public async Task<IActionResult> Release(int id, CancellationToken ct)
    {
        var entity = await dbContext.MfgWorkOrders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status is not WorkOrderStatus.Draft and not WorkOrderStatus.Planned)
        {
            return BadRequest(new { message = "Only draft or planned work order can be released." });
        }

        entity.Status = WorkOrderStatus.Released;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return Ok(new { message = "Work order released." });
    }

    [HttpPost("{id:int}/start")]
    public async Task<IActionResult> Start(int id, CancellationToken ct)
    {
        var entity = await dbContext.MfgWorkOrders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status is not WorkOrderStatus.Released and not WorkOrderStatus.Planned)
        {
            return BadRequest(new { message = "Only released or planned work order can be started." });
        }

        entity.Status = WorkOrderStatus.InProgress;
        entity.ActualStartAt ??= DateTimeOffset.UtcNow;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return Ok(new { message = "Work order started." });
    }

    [HttpPost("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id, [FromBody] ManufacturingWorkOrderCompleteRequest? request, CancellationToken ct)
    {
        var entity = await dbContext.MfgWorkOrders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status is not WorkOrderStatus.InProgress and not WorkOrderStatus.Released)
        {
            return BadRequest(new { message = "Only in-progress or released work order can be completed." });
        }

        var qtyGood = request?.QtyGood ?? entity.QtyGood;
        var qtyScrap = request?.QtyScrap ?? entity.QtyScrap;

        if (qtyGood < 0 || qtyScrap < 0)
        {
            return BadRequest(new { message = "Good qty and scrap qty cannot be negative." });
        }

        if (qtyGood + qtyScrap <= 0)
        {
            return BadRequest(new { message = "Good qty and scrap qty cannot both be zero." });
        }

        entity.QtyGood = Round4(qtyGood);
        entity.QtyScrap = Round4(qtyScrap);
        entity.Status = WorkOrderStatus.Completed;
        entity.ActualStartAt ??= DateTimeOffset.UtcNow;
        entity.ActualEndAt = DateTimeOffset.UtcNow;
        entity.Notes = MergeNotes(entity.Notes, request?.Notes);
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return Ok(new { message = "Work order completed." });
    }

    [HttpPost("{id:int}/close")]
    public async Task<IActionResult> Close(int id, CancellationToken ct)
    {
        var entity = await dbContext.MfgWorkOrders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != WorkOrderStatus.Completed)
        {
            return BadRequest(new { message = "Only completed work order can be closed." });
        }

        entity.Status = WorkOrderStatus.Closed;
        entity.ActualEndAt ??= DateTimeOffset.UtcNow;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return Ok(new { message = "Work order closed." });
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        var entity = await dbContext.MfgWorkOrders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status == WorkOrderStatus.Closed)
        {
            return BadRequest(new { message = "Closed work order cannot be cancelled." });
        }

        if (entity.Status == WorkOrderStatus.Cancelled)
        {
            return Ok(new { message = "Work order already cancelled." });
        }

        entity.Status = WorkOrderStatus.Cancelled;
        entity.IsActive = false;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return Ok(new { message = "Work order cancelled." });
    }

    private async Task<IActionResult?> ValidateWorkOrderAsync(string code, ManufacturingWorkOrderDto request, int? id, CancellationToken ct)
    {
        if (!request.ItemId.HasValue || request.ItemId.Value <= 0)
        {
            return BadRequest(new { message = "Item is required." });
        }

        if (!request.BomId.HasValue || request.BomId.Value <= 0)
        {
            return BadRequest(new { message = "BOM is required." });
        }

        if (request.QtyPlanned <= 0)
        {
            return BadRequest(new { message = "Planned quantity must be greater than 0." });
        }

        if (request.QtyGood < 0 || request.QtyScrap < 0)
        {
            return BadRequest(new { message = "Good qty and scrap qty cannot be negative." });
        }

        if (request.PlannedStartDate == default || request.PlannedEndDate == default)
        {
            return BadRequest(new { message = "Planned start/end date is required." });
        }

        if (request.PlannedEndDate < request.PlannedStartDate)
        {
            return BadRequest(new { message = "Planned end date must be greater than or equal to planned start date." });
        }

        if (request.StandardCostTotal < 0 || request.ActualCostTotal < 0)
        {
            return BadRequest(new { message = "Cost cannot be negative." });
        }

        var itemExists = await dbContext.InvItems.AnyAsync(x => x.Id == request.ItemId.Value, ct);
        if (!itemExists)
        {
            return BadRequest(new { message = "Item not found." });
        }

        var bom = await dbContext.MfgBoms.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.BomId.Value, ct);
        if (bom is null)
        {
            return BadRequest(new { message = "BOM not found." });
        }

        if (bom.ItemId.HasValue && bom.ItemId.Value != request.ItemId.Value)
        {
            return BadRequest(new { message = "BOM item does not match work order item." });
        }

        var normalizedRoutingId = NormalizeOptionalId(request.RoutingId);
        if (normalizedRoutingId.HasValue)
        {
            var routingExists = await dbContext.MfgRoutings.AnyAsync(x => x.Id == normalizedRoutingId.Value, ct);
            if (!routingExists)
            {
                return BadRequest(new { message = "Routing not found." });
            }
        }

        var normalizedWorkCenterId = NormalizeOptionalId(request.WorkCenterId);
        if (normalizedWorkCenterId.HasValue)
        {
            var workCenterExists = await dbContext.MfgWorkCenters.AnyAsync(x => x.Id == normalizedWorkCenterId.Value, ct);
            if (!workCenterExists)
            {
                return BadRequest(new { message = "Work center not found." });
            }
        }

        var normalizedMrpRunId = NormalizeOptionalId(request.MrpRunId);
        if (normalizedMrpRunId.HasValue)
        {
            var mrpExists = await dbContext.MfgMrpRuns.AnyAsync(x => x.Id == normalizedMrpRunId.Value, ct);
            if (!mrpExists)
            {
                return BadRequest(new { message = "MRP run not found." });
            }
        }

        var duplicateCode = await dbContext.MfgWorkOrders
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id != (id ?? 0) && x.Code == code, ct);

        if (duplicateCode)
        {
            return BadRequest(new { message = "Work order code already exists." });
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

    private static Expression<Func<MfgWorkOrder, ManufacturingWorkOrderDto>> MapWorkOrderDto()
    {
        return x => new ManufacturingWorkOrderDto
        {
            Id = x.Id,
            Code = x.Code,
            ItemId = x.ItemId,
            ItemCode = x.Item != null ? x.Item.ItemCode : null,
            ItemName = x.Item != null ? x.Item.Name : null,
            BomId = x.BomId,
            BomCode = x.Bom != null ? x.Bom.Code : null,
            RoutingId = x.RoutingId,
            RoutingCode = x.Routing != null ? x.Routing.Code : null,
            WorkCenterId = x.WorkCenterId,
            WorkCenterCode = x.WorkCenter != null ? x.WorkCenter.Code : null,
            WorkCenterName = x.WorkCenter != null ? x.WorkCenter.Name : null,
            MrpRunId = x.MrpRunId,
            MrpRunCode = x.MrpRun != null ? x.MrpRun.Code : null,
            Status = x.Status,
            ProductionType = x.ProductionType,
            QtyPlanned = x.QtyPlanned,
            QtyGood = x.QtyGood,
            QtyScrap = x.QtyScrap,
            PlannedStartDate = x.PlannedStartDate,
            PlannedEndDate = x.PlannedEndDate,
            ActualStartAt = x.ActualStartAt,
            ActualEndAt = x.ActualEndAt,
            StandardCostTotal = x.StandardCostTotal,
            ActualCostTotal = x.ActualCostTotal,
            IsActive = x.IsActive,
            Notes = x.Notes
        };
    }
}
