using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.FixedAssets;
using ERP.Domain.Entities.FixedAssets;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.FixedAssets;

[Route("api/v1/fixed-assets/depreciation-configs")]
public sealed class DepreciationConfigsController(AppDbContext dbContext) : FixedAssetsControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] FixedAssetDepreciationConfigPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.FaDepreciationConfigs
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(search));
        }

        if (request.FiscalYear.HasValue)
        {
            query = query.Where(x => x.FiscalYear == request.FiscalYear.Value);
        }

        if (request.IsAutoPostJournal.HasValue)
        {
            query = query.Where(x => x.IsAutoPostJournal == request.IsAutoPostJournal.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "name" => isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "fiscalyear" => isDesc ? query.OrderByDescending(x => x.FiscalYear).ThenByDescending(x => x.Name) : query.OrderBy(x => x.FiscalYear).ThenBy(x => x.Name),
            "startdate" => isDesc ? query.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.Name) : query.OrderBy(x => x.StartDate).ThenBy(x => x.Name),
            "enddate" => isDesc ? query.OrderByDescending(x => x.EndDate).ThenByDescending(x => x.Name) : query.OrderBy(x => x.EndDate).ThenBy(x => x.Name),
            "runday" => isDesc ? query.OrderByDescending(x => x.RunDay).ThenByDescending(x => x.Name) : query.OrderBy(x => x.RunDay).ThenBy(x => x.Name),
            "isautopostjournal" => isDesc ? query.OrderByDescending(x => x.IsAutoPostJournal).ThenByDescending(x => x.Name) : query.OrderBy(x => x.IsAutoPostJournal).ThenBy(x => x.Name),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Name) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Name),
            _ => isDesc ? query.OrderByDescending(x => x.FiscalYear).ThenByDescending(x => x.Name) : query.OrderBy(x => x.FiscalYear).ThenBy(x => x.Name)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapDto(x))
            .ToListAsync(ct);

        return Ok(PagedResult<FixedAssetDepreciationConfigDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.FaDepreciationConfigs
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => MapDto(x))
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FixedAssetDepreciationConfigDto request, CancellationToken ct)
    {
        try
        {
            ValidateRequest(request);
            var normalizedName = NormalizeRequired(request.Name, "Configuration name is required.");

            var duplicate = await dbContext.FaDepreciationConfigs
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Name == normalizedName && x.FiscalYear == request.FiscalYear, ct);

            if (duplicate)
            {
                return BadRequest(new { message = "Depreciation config for selected fiscal year already exists." });
            }

            var entity = new FaDepreciationConfig
            {
                Name = normalizedName,
                FiscalYear = request.FiscalYear,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                RunDay = request.RunDay,
                IsAutoPostJournal = request.IsAutoPostJournal,
                IsActive = request.IsActive,
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            };

            dbContext.FaDepreciationConfigs.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            return Ok(MapDto(entity));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] FixedAssetDepreciationConfigDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.FaDepreciationConfigs.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            ValidateRequest(request);
            var normalizedName = NormalizeRequired(request.Name, "Configuration name is required.");

            var duplicate = await dbContext.FaDepreciationConfigs
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Id != id && x.Name == normalizedName && x.FiscalYear == request.FiscalYear, ct);

            if (duplicate)
            {
                return BadRequest(new { message = "Depreciation config for selected fiscal year already exists." });
            }

            entity.Name = normalizedName;
            entity.FiscalYear = request.FiscalYear;
            entity.StartDate = request.StartDate;
            entity.EndDate = request.EndDate;
            entity.RunDay = request.RunDay;
            entity.IsAutoPostJournal = request.IsAutoPostJournal;
            entity.IsActive = request.IsActive;
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(ct);

            return Ok(MapDto(entity));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var entity = await dbContext.FaDepreciationConfigs.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        var hasRun = await dbContext.FaDepreciationRuns
            .AsNoTracking()
            .AnyAsync(x => x.PeriodYear == entity.FiscalYear, ct);

        if (hasRun)
        {
            return BadRequest(new { message = "Configuration is already used in depreciation runs." });
        }

        dbContext.FaDepreciationConfigs.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private static void ValidateRequest(FixedAssetDepreciationConfigDto request)
    {
        if (request.FiscalYear <= 1900)
        {
            throw new InvalidOperationException("Fiscal year is invalid.");
        }

        if (request.StartDate > request.EndDate)
        {
            throw new InvalidOperationException("Start date must be less than or equal to end date.");
        }

        if (request.RunDay is < 1 or > 31)
        {
            throw new InvalidOperationException("Run day must be between 1 and 31.");
        }
    }

    private static string NormalizeRequired(string? value, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(errorMessage);
        }

        return value.Trim();
    }

    private static FixedAssetDepreciationConfigDto MapDto(FaDepreciationConfig entity)
    {
        return new FixedAssetDepreciationConfigDto
        {
            Id = entity.Id,
            Name = entity.Name,
            FiscalYear = entity.FiscalYear,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            RunDay = entity.RunDay,
            IsAutoPostJournal = entity.IsAutoPostJournal,
            IsActive = entity.IsActive
        };
    }
}
