using System.Linq.Expressions;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Manufacturing;
using ERP.Domain.Entities.Manufacturing;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Manufacturing;

[Route("api/v1/manufacturing/work-centers")]
public sealed class WorkCentersController(AppDbContext dbContext) : ManufacturingControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ManufacturingWorkCenterPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.MfgWorkCenters
            .AsNoTracking()
            .Include(x => x.WipAccount)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(search)
                || x.Name.ToLower().Contains(search)
                || (x.WipAccount != null && x.WipAccount.Code.ToLower().Contains(search))
                || (x.WipAccount != null && x.WipAccount.Name.ToLower().Contains(search))
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

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "name" => isDesc ? query.OrderByDescending(x => x.Name).ThenByDescending(x => x.Code) : query.OrderBy(x => x.Name).ThenBy(x => x.Code),
            "capacityhoursperday" => isDesc ? query.OrderByDescending(x => x.CapacityHoursPerDay).ThenByDescending(x => x.Code) : query.OrderBy(x => x.CapacityHoursPerDay).ThenBy(x => x.Code),
            "laborcostperhour" => isDesc ? query.OrderByDescending(x => x.LaborCostPerHour).ThenByDescending(x => x.Code) : query.OrderBy(x => x.LaborCostPerHour).ThenBy(x => x.Code),
            "overheadcostperhour" => isDesc ? query.OrderByDescending(x => x.OverheadCostPerHour).ThenByDescending(x => x.Code) : query.OrderBy(x => x.OverheadCostPerHour).ThenBy(x => x.Code),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Code),
            _ => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapWorkCenterDto())
            .ToListAsync(ct);

        return Ok(PagedResult<ManufacturingWorkCenterDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.MfgWorkCenters
            .AsNoTracking()
            .Include(x => x.WipAccount)
            .Where(x => x.Id == id)
            .Select(MapWorkCenterDto())
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ManufacturingWorkCenterDto request, CancellationToken ct)
    {
        try
        {
            var normalizedCode = NormalizeRequired(request.Code, "Work center code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Work center name is required.");

            var validation = await ValidateWorkCenterAsync(
                normalizedCode,
                request.WipAccountId,
                request.CapacityHoursPerDay,
                request.LaborCostPerHour,
                request.OverheadCostPerHour,
                null,
                ct);

            if (validation is not null)
            {
                return validation;
            }

            var entity = new MfgWorkCenter
            {
                Code = normalizedCode,
                Name = normalizedName,
                CapacityHoursPerDay = decimal.Round(request.CapacityHoursPerDay, 2, MidpointRounding.AwayFromZero),
                LaborCostPerHour = decimal.Round(request.LaborCostPerHour, 2, MidpointRounding.AwayFromZero),
                OverheadCostPerHour = decimal.Round(request.OverheadCostPerHour, 2, MidpointRounding.AwayFromZero),
                WipAccountId = NormalizeOptionalId(request.WipAccountId),
                IsActive = request.IsActive,
                Notes = NormalizeOptional(request.Notes),
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            };

            dbContext.MfgWorkCenters.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var created = await dbContext.MfgWorkCenters
                .AsNoTracking()
                .Include(x => x.WipAccount)
                .Where(x => x.Id == entity.Id)
                .Select(MapWorkCenterDto())
                .FirstAsync(ct);

            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ManufacturingWorkCenterDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.MfgWorkCenters.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            var normalizedCode = NormalizeRequired(request.Code, "Work center code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Work center name is required.");

            var validation = await ValidateWorkCenterAsync(
                normalizedCode,
                request.WipAccountId,
                request.CapacityHoursPerDay,
                request.LaborCostPerHour,
                request.OverheadCostPerHour,
                id,
                ct);

            if (validation is not null)
            {
                return validation;
            }

            entity.Code = normalizedCode;
            entity.Name = normalizedName;
            entity.CapacityHoursPerDay = decimal.Round(request.CapacityHoursPerDay, 2, MidpointRounding.AwayFromZero);
            entity.LaborCostPerHour = decimal.Round(request.LaborCostPerHour, 2, MidpointRounding.AwayFromZero);
            entity.OverheadCostPerHour = decimal.Round(request.OverheadCostPerHour, 2, MidpointRounding.AwayFromZero);
            entity.WipAccountId = NormalizeOptionalId(request.WipAccountId);
            entity.IsActive = request.IsActive;
            entity.Notes = NormalizeOptional(request.Notes);
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(ct);

            var updated = await dbContext.MfgWorkCenters
                .AsNoTracking()
                .Include(x => x.WipAccount)
                .Where(x => x.Id == id)
                .Select(MapWorkCenterDto())
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
        var entity = await dbContext.MfgWorkCenters.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        var isUsed = await dbContext.MfgRoutings.AnyAsync(x => x.WorkCenterId == id, ct)
            || await dbContext.MfgWorkOrders.AnyAsync(x => x.WorkCenterId == id, ct)
            || await dbContext.MfgScrapRecords.AnyAsync(x => x.WorkCenterId == id, ct)
            || await dbContext.MfgOeeSnapshots.AnyAsync(x => x.WorkCenterId == id, ct);

        if (isUsed)
        {
            return BadRequest(new { message = "Work center is already used and cannot be deleted." });
        }

        dbContext.MfgWorkCenters.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task<IActionResult?> ValidateWorkCenterAsync(
        string code,
        int? wipAccountId,
        decimal capacityHoursPerDay,
        decimal laborCostPerHour,
        decimal overheadCostPerHour,
        int? id,
        CancellationToken ct)
    {
        if (capacityHoursPerDay <= 0)
        {
            return BadRequest(new { message = "Capacity hours/day must be greater than 0." });
        }

        if (laborCostPerHour < 0)
        {
            return BadRequest(new { message = "Labor cost/hour cannot be negative." });
        }

        if (overheadCostPerHour < 0)
        {
            return BadRequest(new { message = "Overhead cost/hour cannot be negative." });
        }

        var normalizedWipAccountId = NormalizeOptionalId(wipAccountId);
        if (normalizedWipAccountId.HasValue)
        {
            var wipAccountExists = await dbContext.FinAccounts.AnyAsync(x => x.Id == normalizedWipAccountId.Value, ct);
            if (!wipAccountExists)
            {
                return BadRequest(new { message = "WIP account not found." });
            }
        }

        var duplicate = await dbContext.MfgWorkCenters
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id != (id ?? 0) && x.Code == code, ct);

        if (duplicate)
        {
            return BadRequest(new { message = "Work center code already exists." });
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

    private static Expression<Func<MfgWorkCenter, ManufacturingWorkCenterDto>> MapWorkCenterDto()
    {
        return x => new ManufacturingWorkCenterDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            CapacityHoursPerDay = x.CapacityHoursPerDay,
            LaborCostPerHour = x.LaborCostPerHour,
            OverheadCostPerHour = x.OverheadCostPerHour,
            WipAccountId = x.WipAccountId,
            WipAccountCode = x.WipAccount != null ? x.WipAccount.Code : null,
            WipAccountName = x.WipAccount != null ? x.WipAccount.Name : null,
            IsActive = x.IsActive,
            Notes = x.Notes
        };
    }
}

