using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.FixedAssets;
using ERP.Domain.Entities.FixedAssets;
using ERP.Domain.Enums.FixedAssets;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.FixedAssets;

[Route("api/v1/fixed-assets/revaluations")]
public sealed class RevaluationsController(AppDbContext dbContext) : FixedAssetsControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] FixedAssetRevaluationPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.FaRevaluations
            .AsNoTracking()
            .Include(x => x.Asset)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.RevaluationNo.ToLower().Contains(search)
                || x.Asset.AssetCode.ToLower().Contains(search)
                || x.Asset.Name.ToLower().Contains(search)
                || (x.Notes != null && x.Notes.ToLower().Contains(search)));
        }

        if (request.AssetId.HasValue)
        {
            query = query.Where(x => x.AssetId == request.AssetId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (request.RevaluationDateFrom.HasValue)
        {
            query = query.Where(x => x.RevaluationDate >= request.RevaluationDateFrom.Value);
        }

        if (request.RevaluationDateTo.HasValue)
        {
            query = query.Where(x => x.RevaluationDate <= request.RevaluationDateTo.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "revaluationno" => isDesc ? query.OrderByDescending(x => x.RevaluationNo) : query.OrderBy(x => x.RevaluationNo),
            "assetcode" => isDesc ? query.OrderByDescending(x => x.Asset.AssetCode).ThenByDescending(x => x.RevaluationNo) : query.OrderBy(x => x.Asset.AssetCode).ThenBy(x => x.RevaluationNo),
            "revaluationdate" => isDesc ? query.OrderByDescending(x => x.RevaluationDate).ThenByDescending(x => x.RevaluationNo) : query.OrderBy(x => x.RevaluationDate).ThenBy(x => x.RevaluationNo),
            "oldbookvalue" => isDesc ? query.OrderByDescending(x => x.OldBookValue).ThenByDescending(x => x.RevaluationNo) : query.OrderBy(x => x.OldBookValue).ThenBy(x => x.RevaluationNo),
            "newbookvalue" => isDesc ? query.OrderByDescending(x => x.NewBookValue).ThenByDescending(x => x.RevaluationNo) : query.OrderBy(x => x.NewBookValue).ThenBy(x => x.RevaluationNo),
            "impairmentamount" => isDesc ? query.OrderByDescending(x => x.ImpairmentAmount).ThenByDescending(x => x.RevaluationNo) : query.OrderBy(x => x.ImpairmentAmount).ThenBy(x => x.RevaluationNo),
            "status" => isDesc ? query.OrderByDescending(x => x.Status).ThenByDescending(x => x.RevaluationNo) : query.OrderBy(x => x.Status).ThenBy(x => x.RevaluationNo),
            _ => isDesc ? query.OrderByDescending(x => x.RevaluationDate).ThenByDescending(x => x.RevaluationNo) : query.OrderBy(x => x.RevaluationDate).ThenBy(x => x.RevaluationNo)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapDto(x))
            .ToListAsync(ct);

        return Ok(PagedResult<FixedAssetRevaluationDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.FaRevaluations
            .AsNoTracking()
            .Include(x => x.Asset)
            .Where(x => x.Id == id)
            .Select(x => MapDto(x))
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FixedAssetRevaluationDto request, CancellationToken ct)
    {
        try
        {
            var asset = await ValidateAndGetAssetAsync(request.AssetId, ct);
            ValidateRequest(request);

            var revaluationNo = await GenerateRevaluationNoAsync(request.RevaluationDate, ct);
            var oldBookValue = request.OldBookValue > 0 ? request.OldBookValue : asset.BookValue;

            var entity = new FaRevaluation
            {
                RevaluationNo = revaluationNo,
                AssetId = request.AssetId,
                RevaluationDate = request.RevaluationDate,
                OldBookValue = NormalizeAmount(oldBookValue),
                NewBookValue = NormalizeAmount(request.NewBookValue),
                ImpairmentAmount = NormalizeAmount(request.ImpairmentAmount),
                Status = RevaluationStatus.Draft,
                Notes = NormalizeOptional(request.Notes),
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            };

            dbContext.FaRevaluations.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var created = await dbContext.FaRevaluations
                .AsNoTracking()
                .Include(x => x.Asset)
                .Where(x => x.Id == entity.Id)
                .Select(x => MapDto(x))
                .FirstAsync(ct);

            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] FixedAssetRevaluationDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.FaRevaluations.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            if (entity.Status != RevaluationStatus.Draft)
            {
                return BadRequest(new { message = "Only draft revaluation can be edited." });
            }

            await ValidateAndGetAssetAsync(request.AssetId, ct);
            ValidateRequest(request);

            entity.AssetId = request.AssetId;
            entity.RevaluationDate = request.RevaluationDate;
            entity.OldBookValue = NormalizeAmount(request.OldBookValue);
            entity.NewBookValue = NormalizeAmount(request.NewBookValue);
            entity.ImpairmentAmount = NormalizeAmount(request.ImpairmentAmount);
            entity.Notes = NormalizeOptional(request.Notes);
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(ct);

            var updated = await dbContext.FaRevaluations
                .AsNoTracking()
                .Include(x => x.Asset)
                .Where(x => x.Id == entity.Id)
                .Select(x => MapDto(x))
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
        var entity = await dbContext.FaRevaluations.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != RevaluationStatus.Draft)
        {
            return BadRequest(new { message = "Only draft revaluation can be deleted." });
        }

        dbContext.FaRevaluations.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPut("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        var entity = await dbContext.FaRevaluations.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != RevaluationStatus.Draft)
        {
            return BadRequest(new { message = "Only draft revaluation can be approved." });
        }

        entity.Status = RevaluationStatus.Approved;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPut("{id:int}/post")]
    public async Task<IActionResult> Post(int id, CancellationToken ct)
    {
        var entity = await dbContext.FaRevaluations
            .Include(x => x.Asset)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != RevaluationStatus.Approved)
        {
            return BadRequest(new { message = "Only approved revaluation can be posted." });
        }

        var amountChange = decimal.Round(entity.NewBookValue - entity.OldBookValue, 2, MidpointRounding.AwayFromZero);

        entity.Status = RevaluationStatus.Posted;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        entity.Asset.BookValue = entity.NewBookValue;
        if (entity.Asset.Status != AssetStatus.Disposed)
        {
            entity.Asset.Status = AssetStatus.Active;
        }

        entity.Asset.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.Asset.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        var historyExists = await dbContext.FaAssetHistories
            .AnyAsync(x => x.AssetId == entity.AssetId && x.EventType == AssetHistoryType.Revaluation && x.ReferenceNo == entity.RevaluationNo, ct);

        if (!historyExists)
        {
            dbContext.FaAssetHistories.Add(new FaAssetHistory
            {
                AssetId = entity.AssetId,
                EventDate = entity.RevaluationDate,
                EventType = AssetHistoryType.Revaluation,
                ReferenceNo = entity.RevaluationNo,
                Description = NormalizeOptional(entity.Notes) ?? "Asset revaluation posted",
                AmountChange = amountChange,
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            });

            await dbContext.SaveChangesAsync(ct);
        }

        return NoContent();
    }

    private async Task<FaAsset> ValidateAndGetAssetAsync(int assetId, CancellationToken ct)
    {
        if (assetId <= 0)
        {
            throw new InvalidOperationException("Asset is required.");
        }

        var asset = await dbContext.FaAssets.FirstOrDefaultAsync(x => x.Id == assetId, ct);
        if (asset is null)
        {
            throw new InvalidOperationException("Asset not found.");
        }

        if (asset.Status == AssetStatus.Disposed)
        {
            throw new InvalidOperationException("Disposed asset cannot be revaluated.");
        }

        return asset;
    }

    private static void ValidateRequest(FixedAssetRevaluationDto request)
    {
        if (request.NewBookValue < 0)
        {
            throw new InvalidOperationException("New book value cannot be negative.");
        }

        if (request.OldBookValue < 0)
        {
            throw new InvalidOperationException("Old book value cannot be negative.");
        }

        if (request.ImpairmentAmount < 0)
        {
            throw new InvalidOperationException("Impairment amount cannot be negative.");
        }
    }

    private async Task<string> GenerateRevaluationNoAsync(DateOnly revaluationDate, CancellationToken ct)
    {
        var prefix = $"FARV-{revaluationDate:yyyyMM}-";
        var latest = await dbContext.FaRevaluations
            .IgnoreQueryFilters()
            .Where(x => x.RevaluationNo.StartsWith(prefix))
            .OrderByDescending(x => x.RevaluationNo)
            .Select(x => x.RevaluationNo)
            .FirstOrDefaultAsync(ct);

        var next = 1;
        if (!string.IsNullOrWhiteSpace(latest) && latest.Length > prefix.Length)
        {
            var suffix = latest[prefix.Length..];
            if (int.TryParse(suffix, out var parsed))
            {
                next = parsed + 1;
            }
        }

        return $"{prefix}{next:0000}";
    }

    private static decimal NormalizeAmount(decimal value)
        => decimal.Round(value, 2, MidpointRounding.AwayFromZero);

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static FixedAssetRevaluationDto MapDto(FaRevaluation entity)
    {
        return new FixedAssetRevaluationDto
        {
            Id = entity.Id,
            RevaluationNo = entity.RevaluationNo,
            AssetId = entity.AssetId,
            AssetCode = entity.Asset.AssetCode,
            AssetName = entity.Asset.Name,
            RevaluationDate = entity.RevaluationDate,
            OldBookValue = entity.OldBookValue,
            NewBookValue = entity.NewBookValue,
            ImpairmentAmount = entity.ImpairmentAmount,
            Status = entity.Status,
            Notes = entity.Notes
        };
    }
}
