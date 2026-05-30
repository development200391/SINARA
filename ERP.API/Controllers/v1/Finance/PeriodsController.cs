using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Entities.Finance;
using ERP.Domain.Enums;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Finance;

[Route("api/v1/finance/periods")]
public sealed class PeriodsController(AppDbContext dbContext) : FinanceControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] PeriodPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.FinPeriods
            .AsNoTracking()
            .Include(x => x.FiscalYear)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Name.ToLower().Contains(search) ||
                x.FiscalYear.Name.ToLower().Contains(search) ||
                x.PeriodNumber.ToString().Contains(search));
        }

        if (request.FiscalYearId.HasValue)
        {
            query = query.Where(x => x.FiscalYearId == request.FiscalYearId.Value);
        }

        if (request.PeriodNumberFrom.HasValue)
        {
            query = query.Where(x => x.PeriodNumber >= request.PeriodNumberFrom.Value);
        }

        if (request.PeriodNumberTo.HasValue)
        {
            query = query.Where(x => x.PeriodNumber <= request.PeriodNumberTo.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (request.StartDateFrom.HasValue)
        {
            query = query.Where(x => x.StartDate >= request.StartDateFrom.Value);
        }

        if (request.StartDateTo.HasValue)
        {
            query = query.Where(x => x.StartDate <= request.StartDateTo.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "fiscalyearname" => isDesc ? query.OrderByDescending(x => x.FiscalYear.Name).ThenByDescending(x => x.PeriodNumber) : query.OrderBy(x => x.FiscalYear.Name).ThenBy(x => x.PeriodNumber),
            "periodnumber" => isDesc ? query.OrderByDescending(x => x.PeriodNumber) : query.OrderBy(x => x.PeriodNumber),
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

        return Ok(PagedResult<PeriodDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.FinPeriods
            .AsNoTracking()
            .Include(x => x.FiscalYear)
            .Where(x => x.Id == id)
            .Select(x => MapDto(x))
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPut("{id:int}/close")]
    public async Task<IActionResult> Close(int id, CancellationToken ct)
    {
        var entity = await dbContext.FinPeriods.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status == FinancePeriodStatus.Closed)
        {
            var closedResult = await dbContext.FinPeriods
                .AsNoTracking()
                .Include(x => x.FiscalYear)
                .Where(x => x.Id == id)
                .Select(x => MapDto(x))
                .FirstAsync(ct);
            return Ok(closedResult);
        }

        if (entity.Status == FinancePeriodStatus.Locked)
        {
            return BadRequest(new { message = "Period is locked and cannot be closed." });
        }

        entity.Status = FinancePeriodStatus.Closed;
        entity.UpdatedBy = "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        var result = await dbContext.FinPeriods
            .AsNoTracking()
            .Include(x => x.FiscalYear)
            .Where(x => x.Id == id)
            .Select(x => MapDto(x))
            .FirstAsync(ct);

        return Ok(result);
    }

    private static PeriodDto MapDto(FinPeriod entity)
    {
        return new PeriodDto
        {
            Id = entity.Id,
            FiscalYearId = entity.FiscalYearId,
            FiscalYearName = entity.FiscalYear?.Name ?? string.Empty,
            PeriodNumber = entity.PeriodNumber,
            Name = entity.Name,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Status = entity.Status
        };
    }
}
