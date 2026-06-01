using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.FixedAssets;
using ERP.Domain.Entities.FixedAssets;
using ERP.Domain.Enums.FixedAssets;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.FixedAssets;

[Route("api/v1/fixed-assets/disposals")]
public sealed class DisposalsController(AppDbContext dbContext) : FixedAssetsControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] FixedAssetDisposalPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.FaDisposals
            .AsNoTracking()
            .Include(x => x.Asset)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.DisposalNo.ToLower().Contains(search)
                || x.Asset.AssetCode.ToLower().Contains(search)
                || x.Asset.Name.ToLower().Contains(search)
                || (x.Notes != null && x.Notes.ToLower().Contains(search)));
        }

        if (request.AssetId.HasValue)
        {
            query = query.Where(x => x.AssetId == request.AssetId.Value);
        }

        if (request.DisposalType.HasValue)
        {
            query = query.Where(x => x.DisposalType == request.DisposalType.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (request.DisposalDateFrom.HasValue)
        {
            query = query.Where(x => x.DisposalDate >= request.DisposalDateFrom.Value);
        }

        if (request.DisposalDateTo.HasValue)
        {
            query = query.Where(x => x.DisposalDate <= request.DisposalDateTo.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "disposalno" => isDesc ? query.OrderByDescending(x => x.DisposalNo) : query.OrderBy(x => x.DisposalNo),
            "assetcode" => isDesc ? query.OrderByDescending(x => x.Asset.AssetCode).ThenByDescending(x => x.DisposalNo) : query.OrderBy(x => x.Asset.AssetCode).ThenBy(x => x.DisposalNo),
            "disposaldate" => isDesc ? query.OrderByDescending(x => x.DisposalDate).ThenByDescending(x => x.DisposalNo) : query.OrderBy(x => x.DisposalDate).ThenBy(x => x.DisposalNo),
            "disposaltype" => isDesc ? query.OrderByDescending(x => x.DisposalType).ThenByDescending(x => x.DisposalNo) : query.OrderBy(x => x.DisposalType).ThenBy(x => x.DisposalNo),
            "saleamount" => isDesc ? query.OrderByDescending(x => x.SaleAmount).ThenByDescending(x => x.DisposalNo) : query.OrderBy(x => x.SaleAmount).ThenBy(x => x.DisposalNo),
            "gainlossamount" => isDesc ? query.OrderByDescending(x => x.GainLossAmount).ThenByDescending(x => x.DisposalNo) : query.OrderBy(x => x.GainLossAmount).ThenBy(x => x.DisposalNo),
            "status" => isDesc ? query.OrderByDescending(x => x.Status).ThenByDescending(x => x.DisposalNo) : query.OrderBy(x => x.Status).ThenBy(x => x.DisposalNo),
            _ => isDesc ? query.OrderByDescending(x => x.DisposalDate).ThenByDescending(x => x.DisposalNo) : query.OrderBy(x => x.DisposalDate).ThenBy(x => x.DisposalNo)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapDto(x))
            .ToListAsync(ct);

        return Ok(PagedResult<FixedAssetDisposalDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.FaDisposals
            .AsNoTracking()
            .Include(x => x.Asset)
            .Where(x => x.Id == id)
            .Select(x => MapDto(x))
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FixedAssetDisposalDto request, CancellationToken ct)
    {
        try
        {
            await ValidateRequestAsync(request, ct);
            var disposalNo = await GenerateDisposalNoAsync(request.DisposalDate, ct);

            var entity = new FaDisposal
            {
                DisposalNo = disposalNo,
                AssetId = request.AssetId,
                DisposalDate = request.DisposalDate,
                DisposalType = request.DisposalType,
                SaleAmount = NormalizeOptionalAmount(request.SaleAmount),
                DisposalExpense = NormalizeAmount(request.DisposalExpense),
                GainLossAmount = null,
                Status = DisposalStatus.Draft,
                Notes = NormalizeOptional(request.Notes),
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            };

            dbContext.FaDisposals.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var created = await dbContext.FaDisposals
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
    public async Task<IActionResult> Update(int id, [FromBody] FixedAssetDisposalDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.FaDisposals.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            if (entity.Status != DisposalStatus.Draft)
            {
                return BadRequest(new { message = "Only draft disposal can be edited." });
            }

            await ValidateRequestAsync(request, ct);

            entity.AssetId = request.AssetId;
            entity.DisposalDate = request.DisposalDate;
            entity.DisposalType = request.DisposalType;
            entity.SaleAmount = NormalizeOptionalAmount(request.SaleAmount);
            entity.DisposalExpense = NormalizeAmount(request.DisposalExpense);
            entity.Notes = NormalizeOptional(request.Notes);
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(ct);

            var updated = await dbContext.FaDisposals
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
        var entity = await dbContext.FaDisposals.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != DisposalStatus.Draft)
        {
            return BadRequest(new { message = "Only draft disposal can be deleted." });
        }

        dbContext.FaDisposals.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPut("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        var entity = await dbContext.FaDisposals.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != DisposalStatus.Draft)
        {
            return BadRequest(new { message = "Only draft disposal can be approved." });
        }

        entity.Status = DisposalStatus.Approved;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPut("{id:int}/post")]
    public async Task<IActionResult> Post(int id, CancellationToken ct)
    {
        var entity = await dbContext.FaDisposals
            .Include(x => x.Asset)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != DisposalStatus.Approved)
        {
            return BadRequest(new { message = "Only approved disposal can be posted." });
        }

        var saleAmount = entity.SaleAmount ?? 0m;
        var gainLoss = decimal.Round(saleAmount - entity.DisposalExpense - entity.Asset.BookValue, 2, MidpointRounding.AwayFromZero);

        entity.GainLossAmount = gainLoss;
        entity.Status = DisposalStatus.Posted;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        entity.Asset.Status = AssetStatus.Disposed;
        entity.Asset.IsActive = false;
        entity.Asset.BookValue = 0m;
        entity.Asset.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.Asset.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        var historyExists = await dbContext.FaAssetHistories
            .AnyAsync(x => x.AssetId == entity.AssetId && x.EventType == AssetHistoryType.Disposal && x.ReferenceNo == entity.DisposalNo, ct);

        if (!historyExists)
        {
            dbContext.FaAssetHistories.Add(new FaAssetHistory
            {
                AssetId = entity.AssetId,
                EventDate = entity.DisposalDate,
                EventType = AssetHistoryType.Disposal,
                ReferenceNo = entity.DisposalNo,
                Description = NormalizeOptional(entity.Notes) ?? "Asset disposal posted",
                AmountChange = gainLoss,
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            });

            await dbContext.SaveChangesAsync(ct);
        }

        return NoContent();
    }

    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        var entity = await dbContext.FaDisposals.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status is DisposalStatus.Posted or DisposalStatus.Cancelled)
        {
            return BadRequest(new { message = "Posted or cancelled disposal cannot be cancelled." });
        }

        entity.Status = DisposalStatus.Cancelled;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task ValidateRequestAsync(FixedAssetDisposalDto request, CancellationToken ct)
    {
        if (request.AssetId <= 0)
        {
            throw new InvalidOperationException("Asset is required.");
        }

        var asset = await dbContext.FaAssets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.AssetId, ct);

        if (asset is null)
        {
            throw new InvalidOperationException("Asset not found.");
        }

        if (asset.Status == AssetStatus.Disposed)
        {
            throw new InvalidOperationException("Asset is already disposed.");
        }

        if (request.DisposalExpense < 0)
        {
            throw new InvalidOperationException("Disposal expense cannot be negative.");
        }

        if (request.SaleAmount.HasValue && request.SaleAmount.Value < 0)
        {
            throw new InvalidOperationException("Sale amount cannot be negative.");
        }
    }

    private async Task<string> GenerateDisposalNoAsync(DateOnly disposalDate, CancellationToken ct)
    {
        var prefix = $"FADP-{disposalDate:yyyyMM}-";
        var latest = await dbContext.FaDisposals
            .IgnoreQueryFilters()
            .Where(x => x.DisposalNo.StartsWith(prefix))
            .OrderByDescending(x => x.DisposalNo)
            .Select(x => x.DisposalNo)
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

    private static decimal? NormalizeOptionalAmount(decimal? value)
        => value.HasValue ? NormalizeAmount(value.Value) : null;

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static FixedAssetDisposalDto MapDto(FaDisposal entity)
    {
        return new FixedAssetDisposalDto
        {
            Id = entity.Id,
            DisposalNo = entity.DisposalNo,
            AssetId = entity.AssetId,
            AssetCode = entity.Asset.AssetCode,
            AssetName = entity.Asset.Name,
            DisposalDate = entity.DisposalDate,
            DisposalType = entity.DisposalType,
            SaleAmount = entity.SaleAmount,
            DisposalExpense = entity.DisposalExpense,
            GainLossAmount = entity.GainLossAmount,
            Status = entity.Status,
            Notes = entity.Notes
        };
    }
}
