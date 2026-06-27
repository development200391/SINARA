using System.Linq.Expressions;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Manufacturing;
using ERP.Domain.Entities.Manufacturing;
using ERP.Domain.Enums.Manufacturing;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Manufacturing;

[Route("api/v1/manufacturing/mrp")]
public sealed class MrpController(AppDbContext dbContext) : ManufacturingControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ManufacturingMrpRunPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.MfgMrpRuns
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(search)
                || (x.Notes != null && x.Notes.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var code = request.Code.Trim().ToLowerInvariant();
            query = query.Where(x => x.Code.ToLower().Contains(code));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (request.RunDateFrom.HasValue)
        {
            query = query.Where(x => x.RunDate >= request.RunDateFrom.Value);
        }

        if (request.RunDateTo.HasValue)
        {
            query = query.Where(x => x.RunDate <= request.RunDateTo.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "rundate" => isDesc ? query.OrderByDescending(x => x.RunDate).ThenByDescending(x => x.Code) : query.OrderBy(x => x.RunDate).ThenBy(x => x.Code),
            "status" => isDesc ? query.OrderByDescending(x => x.Status).ThenByDescending(x => x.Code) : query.OrderBy(x => x.Status).ThenBy(x => x.Code),
            "horizondays" => isDesc ? query.OrderByDescending(x => x.HorizonDays).ThenByDescending(x => x.Code) : query.OrderBy(x => x.HorizonDays).ThenBy(x => x.Code),
            _ => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapMrpRunDto())
            .ToListAsync(ct);

        return Ok(PagedResult<ManufacturingMrpRunDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.MfgMrpRuns
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(MapMrpRunDto())
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ManufacturingMrpRunDto request, CancellationToken ct)
    {
        try
        {
            var normalizedCode = NormalizeRequired(request.Code, "MRP run code is required.").ToUpperInvariant();
            var validation = await ValidateMrpRunAsync(normalizedCode, request, null, ct);
            if (validation is not null)
            {
                return validation;
            }

            var entity = new MfgMrpRun
            {
                Code = normalizedCode,
                RunDate = request.RunDate,
                Status = request.Status,
                HorizonDays = request.HorizonDays,
                TotalDemandItems = request.TotalDemandItems,
                RecommendedWoCount = request.RecommendedWoCount,
                RecommendedPrCount = request.RecommendedPrCount,
                StartedAt = request.StartedAt,
                CompletedAt = request.CompletedAt,
                Notes = NormalizeOptional(request.Notes),
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            };

            dbContext.MfgMrpRuns.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var created = await dbContext.MfgMrpRuns
                .AsNoTracking()
                .Where(x => x.Id == entity.Id)
                .Select(MapMrpRunDto())
                .FirstAsync(ct);

            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ManufacturingMrpRunDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.MfgMrpRuns.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            if (entity.Status is not MrpStatus.Draft)
            {
                return BadRequest(new { message = "Only draft MRP run can be edited." });
            }

            var normalizedCode = NormalizeRequired(request.Code, "MRP run code is required.").ToUpperInvariant();
            var validation = await ValidateMrpRunAsync(normalizedCode, request, id, ct);
            if (validation is not null)
            {
                return validation;
            }

            entity.Code = normalizedCode;
            entity.RunDate = request.RunDate;
            entity.Status = request.Status;
            entity.HorizonDays = request.HorizonDays;
            entity.TotalDemandItems = request.TotalDemandItems;
            entity.RecommendedWoCount = request.RecommendedWoCount;
            entity.RecommendedPrCount = request.RecommendedPrCount;
            entity.StartedAt = request.StartedAt;
            entity.CompletedAt = request.CompletedAt;
            entity.Notes = NormalizeOptional(request.Notes);
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(ct);

            var updated = await dbContext.MfgMrpRuns
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(MapMrpRunDto())
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
        var entity = await dbContext.MfgMrpRuns.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status is not MrpStatus.Draft)
        {
            return BadRequest(new { message = "Only draft MRP run can be deleted." });
        }

        dbContext.MfgMrpRuns.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPost("{id:int}/run")]
    public async Task<IActionResult> Run(int id, CancellationToken ct)
    {
        var entity = await dbContext.MfgMrpRuns.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status is not MrpStatus.Draft)
        {
            return BadRequest(new { message = "Only draft MRP run can be executed." });
        }

        entity.Status = MrpStatus.Running;
        entity.StartedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return Ok(new { message = "MRP run started." });
    }

    [HttpPost("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id, CancellationToken ct)
    {
        var entity = await dbContext.MfgMrpRuns.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status is not MrpStatus.Running)
        {
            return BadRequest(new { message = "Only running MRP can be completed." });
        }

        entity.Status = MrpStatus.Completed;
        entity.CompletedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return Ok(new { message = "MRP run completed." });
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        var entity = await dbContext.MfgMrpRuns.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status == MrpStatus.Completed)
        {
            return BadRequest(new { message = "Completed MRP run cannot be cancelled." });
        }

        if (entity.Status == MrpStatus.Cancelled)
        {
            return Ok(new { message = "MRP run already cancelled." });
        }

        entity.Status = MrpStatus.Cancelled;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return Ok(new { message = "MRP run cancelled." });
    }

    private async Task<IActionResult?> ValidateMrpRunAsync(string code, ManufacturingMrpRunDto request, int? id, CancellationToken ct)
    {
        if (request.RunDate == default)
        {
            return BadRequest(new { message = "Run date is required." });
        }

        if (request.HorizonDays <= 0)
        {
            return BadRequest(new { message = "Horizon days must be greater than 0." });
        }

        if (request.TotalDemandItems < 0 || request.RecommendedWoCount < 0 || request.RecommendedPrCount < 0)
        {
            return BadRequest(new { message = "Demand/recommendation count cannot be negative." });
        }

        if (request.StartedAt.HasValue && request.CompletedAt.HasValue && request.CompletedAt.Value < request.StartedAt.Value)
        {
            return BadRequest(new { message = "Completed date cannot be earlier than started date." });
        }

        var duplicateCode = await dbContext.MfgMrpRuns
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id != (id ?? 0) && x.Code == code, ct);

        if (duplicateCode)
        {
            return BadRequest(new { message = "MRP run code already exists." });
        }

        return null;
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

    private static Expression<Func<MfgMrpRun, ManufacturingMrpRunDto>> MapMrpRunDto()
    {
        return x => new ManufacturingMrpRunDto
        {
            Id = x.Id,
            Code = x.Code,
            RunDate = x.RunDate,
            Status = x.Status,
            HorizonDays = x.HorizonDays,
            TotalDemandItems = x.TotalDemandItems,
            RecommendedWoCount = x.RecommendedWoCount,
            RecommendedPrCount = x.RecommendedPrCount,
            StartedAt = x.StartedAt,
            CompletedAt = x.CompletedAt,
            Notes = x.Notes
        };
    }
}
