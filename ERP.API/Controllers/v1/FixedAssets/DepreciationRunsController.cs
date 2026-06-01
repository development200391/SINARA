using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.FixedAssets;
using ERP.Domain.Entities.FixedAssets;
using ERP.Domain.Enums.FixedAssets;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.FixedAssets;

[Route("api/v1/fixed-assets/depreciation-runs")]
public sealed class DepreciationRunsController(AppDbContext dbContext) : FixedAssetsControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] FixedAssetDepreciationRunPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.FaDepreciationRuns
            .AsNoTracking()
            .Include(x => x.ApprovedByUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x => x.RunNo.ToLower().Contains(search));
        }

        if (request.PeriodYear.HasValue)
        {
            query = query.Where(x => x.PeriodYear == request.PeriodYear.Value);
        }

        if (request.PeriodMonth.HasValue)
        {
            query = query.Where(x => x.PeriodMonth == request.PeriodMonth.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "runno" => isDesc ? query.OrderByDescending(x => x.RunNo) : query.OrderBy(x => x.RunNo),
            "periodyear" => isDesc ? query.OrderByDescending(x => x.PeriodYear).ThenByDescending(x => x.PeriodMonth) : query.OrderBy(x => x.PeriodYear).ThenBy(x => x.PeriodMonth),
            "periodmonth" => isDesc ? query.OrderByDescending(x => x.PeriodMonth).ThenByDescending(x => x.PeriodYear) : query.OrderBy(x => x.PeriodMonth).ThenBy(x => x.PeriodYear),
            "rundate" => isDesc ? query.OrderByDescending(x => x.RunDate).ThenByDescending(x => x.RunNo) : query.OrderBy(x => x.RunDate).ThenBy(x => x.RunNo),
            "totalassetcount" => isDesc ? query.OrderByDescending(x => x.TotalAssetCount).ThenByDescending(x => x.RunNo) : query.OrderBy(x => x.TotalAssetCount).ThenBy(x => x.RunNo),
            "totaldepreciationamount" => isDesc ? query.OrderByDescending(x => x.TotalDepreciationAmount).ThenByDescending(x => x.RunNo) : query.OrderBy(x => x.TotalDepreciationAmount).ThenBy(x => x.RunNo),
            "status" => isDesc ? query.OrderByDescending(x => x.Status).ThenByDescending(x => x.RunNo) : query.OrderBy(x => x.Status).ThenBy(x => x.RunNo),
            _ => isDesc ? query.OrderByDescending(x => x.PeriodYear).ThenByDescending(x => x.PeriodMonth) : query.OrderBy(x => x.PeriodYear).ThenBy(x => x.PeriodMonth)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapDto(x))
            .ToListAsync(ct);

        return Ok(PagedResult<FixedAssetDepreciationRunDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.FaDepreciationRuns
            .AsNoTracking()
            .Include(x => x.ApprovedByUser)
            .Where(x => x.Id == id)
            .Select(x => MapDto(x))
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost("run")]
    public async Task<IActionResult> Run([FromBody] RunDepreciationRequest request, CancellationToken ct)
    {
        try
        {
            if (request.PeriodYear <= 1900)
            {
                throw new InvalidOperationException("Period year is invalid.");
            }

            if (request.PeriodMonth is < 1 or > 12)
            {
                throw new InvalidOperationException("Period month must be between 1 and 12.");
            }

            var runNo = $"FADR-{request.PeriodYear}{request.PeriodMonth:00}";

            var existing = await dbContext.FaDepreciationRuns
                .FirstOrDefaultAsync(x => x.RunNo == runNo, ct);

            if (existing is not null && existing.Status == DepreciationRunStatus.Approved)
            {
                return BadRequest(new { message = "Selected depreciation run has already been approved." });
            }

            var schedules = await dbContext.FaDepreciationSchedules
                .Where(x => x.PeriodYear == request.PeriodYear && x.PeriodMonth == request.PeriodMonth && x.Status == DepreciationScheduleStatus.Pending)
                .OrderBy(x => x.AssetId)
                .ThenBy(x => x.DepreciationDate)
                .ToListAsync(ct);

            if (schedules.Count == 0)
            {
                return BadRequest(new { message = "No pending depreciation schedules found for selected period." });
            }

            var run = existing ?? new FaDepreciationRun
            {
                RunNo = runNo,
                PeriodYear = request.PeriodYear,
                PeriodMonth = request.PeriodMonth,
                RunDate = new DateOnly(request.PeriodYear, request.PeriodMonth, DateTime.DaysInMonth(request.PeriodYear, request.PeriodMonth)),
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            };

            run.TotalAssetCount = schedules.Select(x => x.AssetId).Distinct().Count();
            run.TotalDepreciationAmount = decimal.Round(schedules.Sum(x => x.DepreciationAmount), 2, MidpointRounding.AwayFromZero);
            run.Status = request.ApproveImmediately ? DepreciationRunStatus.Approved : DepreciationRunStatus.Processed;
            run.ApprovedBy = request.ApproveImmediately ? GetCurrentUserId() : null;
            run.ApprovedAt = request.ApproveImmediately ? DateTimeOffset.UtcNow : null;
            run.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            run.UpdatedAt = DateTimeOffset.UtcNow;

            if (existing is null)
            {
                dbContext.FaDepreciationRuns.Add(run);
                await dbContext.SaveChangesAsync(ct);
            }

            foreach (var schedule in schedules)
            {
                schedule.RunId = run.Id;
                schedule.Status = DepreciationScheduleStatus.Processed;
                schedule.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
                schedule.UpdatedAt = DateTimeOffset.UtcNow;
            }

            var assetIds = schedules
                .Select(x => x.AssetId)
                .Distinct()
                .ToList();

            var assets = await dbContext.FaAssets
                .Where(x => assetIds.Contains(x.Id))
                .ToListAsync(ct);

            foreach (var asset in assets)
            {
                var latest = schedules
                    .Where(x => x.AssetId == asset.Id)
                    .OrderByDescending(x => x.DepreciationDate)
                    .First();

                asset.AccumulatedDepreciation = latest.AccumulatedDepreciation;
                asset.BookValue = latest.BookValue;
                if (asset.BookValue <= asset.SalvageValue)
                {
                    asset.Status = AssetStatus.FullyDepreciated;
                }

                asset.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
                asset.UpdatedAt = DateTimeOffset.UtcNow;
            }

            await dbContext.SaveChangesAsync(ct);

            foreach (var assetId in assetIds)
            {
                var amount = schedules
                    .Where(x => x.AssetId == assetId)
                    .Sum(x => x.DepreciationAmount);

                var exists = await dbContext.FaAssetHistories
                    .AnyAsync(x => x.AssetId == assetId && x.EventType == AssetHistoryType.Depreciation && x.ReferenceNo == runNo, ct);

                if (!exists)
                {
                    dbContext.FaAssetHistories.Add(new FaAssetHistory
                    {
                        AssetId = assetId,
                        EventDate = new DateOnly(request.PeriodYear, request.PeriodMonth, 1),
                        EventType = AssetHistoryType.Depreciation,
                        ReferenceNo = runNo,
                        Description = $"Depreciation run {request.PeriodYear}-{request.PeriodMonth:00}",
                        AmountChange = decimal.Round(amount, 2, MidpointRounding.AwayFromZero),
                        CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
                    });
                }
            }

            await dbContext.SaveChangesAsync(ct);

            var result = await dbContext.FaDepreciationRuns
                .AsNoTracking()
                .Include(x => x.ApprovedByUser)
                .Where(x => x.Id == run.Id)
                .Select(x => MapDto(x))
                .FirstAsync(ct);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        var entity = await dbContext.FaDepreciationRuns.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status == DepreciationRunStatus.Approved)
        {
            return NoContent();
        }

        entity.Status = DepreciationRunStatus.Approved;
        entity.ApprovedBy = GetCurrentUserId();
        entity.ApprovedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private static FixedAssetDepreciationRunDto MapDto(FaDepreciationRun entity)
    {
        return new FixedAssetDepreciationRunDto
        {
            Id = entity.Id,
            RunNo = entity.RunNo,
            PeriodYear = entity.PeriodYear,
            PeriodMonth = entity.PeriodMonth,
            RunDate = entity.RunDate,
            TotalAssetCount = entity.TotalAssetCount,
            TotalDepreciationAmount = entity.TotalDepreciationAmount,
            Status = entity.Status,
            ApprovedByName = entity.ApprovedByUser != null ? entity.ApprovedByUser.FullName : null,
            ApprovedAt = entity.ApprovedAt,
            JournalEntryId = entity.JournalEntryId
        };
    }
}
