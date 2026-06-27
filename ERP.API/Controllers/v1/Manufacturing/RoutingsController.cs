using System.Linq.Expressions;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Manufacturing;
using ERP.Domain.Entities.Manufacturing;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Manufacturing;

[Route("api/v1/manufacturing/routings")]
public sealed class RoutingsController(AppDbContext dbContext) : ManufacturingControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ManufacturingRoutingPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.MfgRoutings
            .AsNoTracking()
            .Include(x => x.Item)
            .Include(x => x.WorkCenter)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(search)
                || x.Name.ToLower().Contains(search)
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

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var name = request.Name.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(name));
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

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "name" => isDesc ? query.OrderByDescending(x => x.Name).ThenByDescending(x => x.Code) : query.OrderBy(x => x.Name).ThenBy(x => x.Code),
            "version" => isDesc ? query.OrderByDescending(x => x.Version).ThenByDescending(x => x.Code) : query.OrderBy(x => x.Version).ThenBy(x => x.Code),
            "status" => isDesc ? query.OrderByDescending(x => x.Status).ThenByDescending(x => x.Code) : query.OrderBy(x => x.Status).ThenBy(x => x.Code),
            "totalleadtimehours" => isDesc ? query.OrderByDescending(x => x.TotalLeadTimeHours).ThenByDescending(x => x.Code) : query.OrderBy(x => x.TotalLeadTimeHours).ThenBy(x => x.Code),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Code),
            _ => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapRoutingDto())
            .ToListAsync(ct);

        return Ok(PagedResult<ManufacturingRoutingDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.MfgRoutings
            .AsNoTracking()
            .Include(x => x.Item)
            .Include(x => x.WorkCenter)
            .Where(x => x.Id == id)
            .Select(MapRoutingDto())
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ManufacturingRoutingDto request, CancellationToken ct)
    {
        try
        {
            var normalizedCode = NormalizeRequired(request.Code, "Routing code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Routing name is required.");

            var validation = await ValidateRoutingAsync(
                normalizedCode,
                request.ItemId,
                request.WorkCenterId,
                request.Version,
                request.TotalLeadTimeHours,
                null,
                ct);

            if (validation is not null)
            {
                return validation;
            }

            var entity = new MfgRouting
            {
                Code = normalizedCode,
                Name = normalizedName,
                ItemId = NormalizeOptionalId(request.ItemId),
                WorkCenterId = NormalizeOptionalId(request.WorkCenterId),
                Version = request.Version,
                Status = request.Status,
                TotalLeadTimeHours = decimal.Round(request.TotalLeadTimeHours, 2, MidpointRounding.AwayFromZero),
                IsActive = request.IsActive,
                Notes = NormalizeOptional(request.Notes),
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            };

            dbContext.MfgRoutings.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var created = await dbContext.MfgRoutings
                .AsNoTracking()
                .Include(x => x.Item)
                .Include(x => x.WorkCenter)
                .Where(x => x.Id == entity.Id)
                .Select(MapRoutingDto())
                .FirstAsync(ct);

            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ManufacturingRoutingDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.MfgRoutings.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            var normalizedCode = NormalizeRequired(request.Code, "Routing code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Routing name is required.");

            var validation = await ValidateRoutingAsync(
                normalizedCode,
                request.ItemId,
                request.WorkCenterId,
                request.Version,
                request.TotalLeadTimeHours,
                id,
                ct);

            if (validation is not null)
            {
                return validation;
            }

            entity.Code = normalizedCode;
            entity.Name = normalizedName;
            entity.ItemId = NormalizeOptionalId(request.ItemId);
            entity.WorkCenterId = NormalizeOptionalId(request.WorkCenterId);
            entity.Version = request.Version;
            entity.Status = request.Status;
            entity.TotalLeadTimeHours = decimal.Round(request.TotalLeadTimeHours, 2, MidpointRounding.AwayFromZero);
            entity.IsActive = request.IsActive;
            entity.Notes = NormalizeOptional(request.Notes);
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(ct);

            var updated = await dbContext.MfgRoutings
                .AsNoTracking()
                .Include(x => x.Item)
                .Include(x => x.WorkCenter)
                .Where(x => x.Id == id)
                .Select(MapRoutingDto())
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
        var entity = await dbContext.MfgRoutings.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        var isUsed = await dbContext.MfgBoms.AnyAsync(x => x.RoutingId == id, ct)
            || await dbContext.MfgWorkOrders.AnyAsync(x => x.RoutingId == id, ct);

        if (isUsed)
        {
            return BadRequest(new { message = "Routing is already used and cannot be deleted." });
        }

        dbContext.MfgRoutings.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task<IActionResult?> ValidateRoutingAsync(
        string code,
        int? itemId,
        int? workCenterId,
        int version,
        decimal totalLeadTimeHours,
        int? id,
        CancellationToken ct)
    {
        if (version <= 0)
        {
            return BadRequest(new { message = "Routing version must be greater than 0." });
        }

        if (totalLeadTimeHours < 0)
        {
            return BadRequest(new { message = "Total lead time cannot be negative." });
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

        var duplicate = await dbContext.MfgRoutings
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id != (id ?? 0) && x.Code == code, ct);

        if (duplicate)
        {
            return BadRequest(new { message = "Routing code already exists." });
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

    private static Expression<Func<MfgRouting, ManufacturingRoutingDto>> MapRoutingDto()
    {
        return x => new ManufacturingRoutingDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            ItemId = x.ItemId,
            ItemCode = x.Item != null ? x.Item.ItemCode : null,
            ItemName = x.Item != null ? x.Item.Name : null,
            WorkCenterId = x.WorkCenterId,
            WorkCenterCode = x.WorkCenter != null ? x.WorkCenter.Code : null,
            WorkCenterName = x.WorkCenter != null ? x.WorkCenter.Name : null,
            Version = x.Version,
            Status = x.Status,
            TotalLeadTimeHours = x.TotalLeadTimeHours,
            IsActive = x.IsActive,
            Notes = x.Notes
        };
    }
}
