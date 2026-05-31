using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Entities.Finance;
using ERP.Domain.Enums;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Finance;

[Route("api/v1/finance/fiscal-years")]
public sealed class FiscalYearsController(AppDbContext dbContext) : FinanceControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] FiscalYearPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.FinFiscalYears.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var name = request.Name.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(name));
        }

        if (request.StartDateFrom.HasValue)
        {
            query = query.Where(x => x.StartDate >= request.StartDateFrom.Value);
        }

        if (request.StartDateTo.HasValue)
        {
            query = query.Where(x => x.StartDate <= request.StartDateTo.Value);
        }

        if (request.EndDateFrom.HasValue)
        {
            query = query.Where(x => x.EndDate >= request.EndDateFrom.Value);
        }

        if (request.EndDateTo.HasValue)
        {
            query = query.Where(x => x.EndDate <= request.EndDateTo.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "name" => isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "startdate" => isDesc ? query.OrderByDescending(x => x.StartDate) : query.OrderBy(x => x.StartDate),
            "enddate" => isDesc ? query.OrderByDescending(x => x.EndDate) : query.OrderBy(x => x.EndDate),
            "status" => isDesc ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            _ => isDesc ? query.OrderByDescending(x => x.StartDate) : query.OrderBy(x => x.StartDate)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapDto(x))
            .ToListAsync(ct);

        return Ok(PagedResult<FiscalYearDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.FinFiscalYears
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => MapDto(x))
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FiscalYearDto request, CancellationToken ct)
    {
        var normalizedName = NormalizeRequired(request.Name, "Fiscal year name is required.");
        if (request.StartDate > request.EndDate)
        {
            return BadRequest(new { message = "Fiscal year date range is invalid." });
        }

        var duplicateName = await dbContext.FinFiscalYears
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Name.ToLower() == normalizedName.ToLower(), ct);
        if (duplicateName)
        {
            return BadRequest(new { message = "Fiscal year name already exists." });
        }

        var overlap = await dbContext.FinFiscalYears.AnyAsync(x =>
            request.StartDate <= x.EndDate &&
            request.EndDate >= x.StartDate,
            ct);

        if (overlap)
        {
            return BadRequest(new { message = "Fiscal year date range overlaps with existing fiscal year." });
        }

        var entity = new FinFiscalYear
        {
            Name = normalizedName,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = request.Status,
            CreatedBy = "system"
        };

        dbContext.FinFiscalYears.Add(entity);
        await dbContext.SaveChangesAsync(ct);

        await SeedPeriodsForFiscalYearAsync(entity, ct);

        return Ok(MapDto(entity));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] FiscalYearDto request, CancellationToken ct)
    {
        var entity = await dbContext.FinFiscalYears.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != FinancePeriodStatus.Open)
        {
            return BadRequest(new { message = "Only open fiscal year can be updated." });
        }

        var normalizedName = NormalizeRequired(request.Name, "Fiscal year name is required.");
        if (request.StartDate > request.EndDate)
        {
            return BadRequest(new { message = "Fiscal year date range is invalid." });
        }

        var duplicateName = await dbContext.FinFiscalYears
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id != id && x.Name.ToLower() == normalizedName.ToLower(), ct);
        if (duplicateName)
        {
            return BadRequest(new { message = "Fiscal year name already exists." });
        }

        var overlap = await dbContext.FinFiscalYears.AnyAsync(x =>
            x.Id != id &&
            request.StartDate <= x.EndDate &&
            request.EndDate >= x.StartDate,
            ct);

        if (overlap)
        {
            return BadRequest(new { message = "Fiscal year date range overlaps with existing fiscal year." });
        }

        entity.Name = normalizedName;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.Status = request.Status;
        entity.UpdatedBy = "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        return Ok(MapDto(entity));
    }

    [HttpPut("{id:int}/close")]
    public async Task<IActionResult> Close(int id, CancellationToken ct)
    {
        var entity = await dbContext.FinFiscalYears.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status == FinancePeriodStatus.Closed)
        {
            return Ok(MapDto(entity));
        }

        if (entity.Status == FinancePeriodStatus.Locked)
        {
            return BadRequest(new { message = "Fiscal year is locked and cannot be closed." });
        }

        var draftJournalCount = await dbContext.FinJournalEntries
            .AsNoTracking()
            .CountAsync(x => x.Period.FiscalYearId == id && x.Status == FinanceJournalStatus.Draft, ct);

        if (draftJournalCount > 0)
        {
            return BadRequest(new { message = "Fiscal year still has draft journals. Post or delete them before closing." });
        }

        entity.Status = FinancePeriodStatus.Closed;
        entity.UpdatedBy = "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        var periods = await dbContext.FinPeriods
            .Where(x => x.FiscalYearId == entity.Id && x.Status == FinancePeriodStatus.Open)
            .ToListAsync(ct);

        foreach (var period in periods)
        {
            period.Status = FinancePeriodStatus.Closed;
            period.UpdatedBy = "system";
            period.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(ct);

        return Ok(MapDto(entity));
    }

    private async Task SeedPeriodsForFiscalYearAsync(FinFiscalYear fiscalYear, CancellationToken ct)
    {
        var start = fiscalYear.StartDate;
        var end = fiscalYear.EndDate;

        var periodNumber = 1;
        var cursor = new DateOnly(start.Year, start.Month, 1);

        while (cursor <= end)
        {
            var periodStart = cursor < start ? start : cursor;
            var monthEnd = new DateOnly(cursor.Year, cursor.Month, DateTime.DaysInMonth(cursor.Year, cursor.Month));
            var periodEnd = monthEnd > end ? end : monthEnd;

            dbContext.FinPeriods.Add(new FinPeriod
            {
                FiscalYearId = fiscalYear.Id,
                PeriodNumber = periodNumber,
                Name = periodStart.ToString("MMMM yyyy"),
                StartDate = periodStart,
                EndDate = periodEnd,
                Status = FinancePeriodStatus.Open,
                CreatedBy = "system"
            });

            periodNumber++;
            cursor = cursor.AddMonths(1);
        }

        await dbContext.SaveChangesAsync(ct);
    }

    private static FiscalYearDto MapDto(FinFiscalYear entity)
    {
        return new FiscalYearDto
        {
            Id = entity.Id,
            Name = entity.Name,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Status = entity.Status
        };
    }

    private static string NormalizeRequired(string? value, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(errorMessage);
        }

        return value.Trim();
    }
}
