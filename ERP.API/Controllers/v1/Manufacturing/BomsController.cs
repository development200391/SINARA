using System.Linq.Expressions;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Manufacturing;
using ERP.Domain.Entities.Manufacturing;
using ERP.Domain.Enums.Manufacturing;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Manufacturing;

[Route("api/v1/manufacturing/boms")]
public sealed class BomsController(AppDbContext dbContext) : ManufacturingControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ManufacturingBomPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.MfgBoms
            .AsNoTracking()
            .Include(x => x.Item)
            .Include(x => x.Routing)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(search)
                || (x.Item != null && x.Item.ItemCode.ToLower().Contains(search))
                || (x.Item != null && x.Item.Name.ToLower().Contains(search))
                || (x.Routing != null && x.Routing.Code.ToLower().Contains(search))
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

        if (request.RoutingId.HasValue)
        {
            query = query.Where(x => x.RoutingId == request.RoutingId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (request.EffectiveDateFrom.HasValue)
        {
            query = query.Where(x => x.EffectiveDate >= request.EffectiveDateFrom.Value);
        }

        if (request.EffectiveDateTo.HasValue)
        {
            query = query.Where(x => x.EffectiveDate <= request.EffectiveDateTo.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "status" => isDesc ? query.OrderByDescending(x => x.Status).ThenByDescending(x => x.Code) : query.OrderBy(x => x.Status).ThenBy(x => x.Code),
            "version" => isDesc ? query.OrderByDescending(x => x.Version).ThenByDescending(x => x.Code) : query.OrderBy(x => x.Version).ThenBy(x => x.Code),
            "qtyproduced" => isDesc ? query.OrderByDescending(x => x.QtyProduced).ThenByDescending(x => x.Code) : query.OrderBy(x => x.QtyProduced).ThenBy(x => x.Code),
            "effectivedate" => isDesc ? query.OrderByDescending(x => x.EffectiveDate).ThenByDescending(x => x.Code) : query.OrderBy(x => x.EffectiveDate).ThenBy(x => x.Code),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Code),
            _ => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapBomDto())
            .ToListAsync(ct);

        return Ok(PagedResult<ManufacturingBomDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.MfgBoms
            .AsNoTracking()
            .Include(x => x.Item)
            .Include(x => x.Routing)
            .Where(x => x.Id == id)
            .Select(MapBomDto())
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ManufacturingBomDto request, CancellationToken ct)
    {
        try
        {
            var normalizedCode = NormalizeRequired(request.Code, "BOM code is required.").ToUpperInvariant();
            var validation = await ValidateBomAsync(
                normalizedCode,
                request.ItemId,
                request.RoutingId,
                request.Version,
                request.QtyProduced,
                request.StandardCost,
                request.EffectiveDate,
                request.Status,
                request.IsActive,
                null,
                ct);

            if (validation is not null)
            {
                return validation;
            }

            var entity = new MfgBom
            {
                Code = normalizedCode,
                ItemId = request.ItemId,
                RoutingId = NormalizeOptionalId(request.RoutingId),
                Version = request.Version,
                Status = request.Status,
                QtyProduced = decimal.Round(request.QtyProduced, 4, MidpointRounding.AwayFromZero),
                StandardCost = decimal.Round(request.StandardCost, 4, MidpointRounding.AwayFromZero),
                EffectiveDate = request.EffectiveDate,
                IsActive = request.IsActive,
                Notes = NormalizeOptional(request.Notes),
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            };

            dbContext.MfgBoms.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var created = await dbContext.MfgBoms
                .AsNoTracking()
                .Include(x => x.Item)
                .Include(x => x.Routing)
                .Where(x => x.Id == entity.Id)
                .Select(MapBomDto())
                .FirstAsync(ct);

            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ManufacturingBomDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.MfgBoms.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            var normalizedCode = NormalizeRequired(request.Code, "BOM code is required.").ToUpperInvariant();
            var validation = await ValidateBomAsync(
                normalizedCode,
                request.ItemId,
                request.RoutingId,
                request.Version,
                request.QtyProduced,
                request.StandardCost,
                request.EffectiveDate,
                request.Status,
                request.IsActive,
                id,
                ct);

            if (validation is not null)
            {
                return validation;
            }

            entity.Code = normalizedCode;
            entity.ItemId = request.ItemId;
            entity.RoutingId = NormalizeOptionalId(request.RoutingId);
            entity.Version = request.Version;
            entity.Status = request.Status;
            entity.QtyProduced = decimal.Round(request.QtyProduced, 4, MidpointRounding.AwayFromZero);
            entity.StandardCost = decimal.Round(request.StandardCost, 4, MidpointRounding.AwayFromZero);
            entity.EffectiveDate = request.EffectiveDate;
            entity.IsActive = request.IsActive;
            entity.Notes = NormalizeOptional(request.Notes);
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(ct);

            var updated = await dbContext.MfgBoms
                .AsNoTracking()
                .Include(x => x.Item)
                .Include(x => x.Routing)
                .Where(x => x.Id == id)
                .Select(MapBomDto())
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
        var entity = await dbContext.MfgBoms.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        var isUsed = await dbContext.MfgWorkOrders.AnyAsync(x => x.BomId == id, ct);
        if (isUsed)
        {
            return BadRequest(new { message = "BOM is already used by work order and cannot be deleted." });
        }

        dbContext.MfgBoms.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task<IActionResult?> ValidateBomAsync(
        string code,
        int? itemId,
        int? routingId,
        int version,
        decimal qtyProduced,
        decimal standardCost,
        DateOnly effectiveDate,
        BomStatus status,
        bool isActive,
        int? id,
        CancellationToken ct)
    {
        if (!itemId.HasValue || itemId.Value <= 0)
        {
            return BadRequest(new { message = "Item is required." });
        }

        if (effectiveDate == default)
        {
            return BadRequest(new { message = "Effective date is required." });
        }

        if (version <= 0)
        {
            return BadRequest(new { message = "BOM version must be greater than 0." });
        }

        if (qtyProduced <= 0)
        {
            return BadRequest(new { message = "Qty produced must be greater than 0." });
        }

        if (standardCost < 0)
        {
            return BadRequest(new { message = "Standard cost cannot be negative." });
        }

        var itemExists = await dbContext.InvItems.AnyAsync(x => x.Id == itemId.Value, ct);
        if (!itemExists)
        {
            return BadRequest(new { message = "Item not found." });
        }

        var normalizedRoutingId = NormalizeOptionalId(routingId);
        if (normalizedRoutingId.HasValue)
        {
            var routingExists = await dbContext.MfgRoutings.AnyAsync(x => x.Id == normalizedRoutingId.Value, ct);
            if (!routingExists)
            {
                return BadRequest(new { message = "Routing not found." });
            }
        }

        var duplicateCode = await dbContext.MfgBoms
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id != (id ?? 0) && x.Code == code, ct);

        if (duplicateCode)
        {
            return BadRequest(new { message = "BOM code already exists." });
        }

        if (status == BomStatus.Active && isActive)
        {
            var duplicateActiveForItem = await dbContext.MfgBoms
                .AnyAsync(x => x.Id != (id ?? 0) && x.ItemId == itemId.Value && x.IsActive && x.Status == BomStatus.Active, ct);

            if (duplicateActiveForItem)
            {
                return BadRequest(new { message = "Another active BOM for this item already exists." });
            }
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

    private static Expression<Func<MfgBom, ManufacturingBomDto>> MapBomDto()
    {
        return x => new ManufacturingBomDto
        {
            Id = x.Id,
            Code = x.Code,
            ItemId = x.ItemId,
            ItemCode = x.Item != null ? x.Item.ItemCode : null,
            ItemName = x.Item != null ? x.Item.Name : null,
            RoutingId = x.RoutingId,
            RoutingCode = x.Routing != null ? x.Routing.Code : null,
            Version = x.Version,
            Status = x.Status,
            QtyProduced = x.QtyProduced,
            StandardCost = x.StandardCost,
            EffectiveDate = x.EffectiveDate,
            IsActive = x.IsActive,
            Notes = x.Notes
        };
    }
}
