using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.FixedAssets;
using ERP.Domain.Entities.FixedAssets;
using ERP.Domain.Enums.FixedAssets;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.FixedAssets;

[Route("api/v1/fixed-assets/transfers")]
public sealed class TransfersController(AppDbContext dbContext) : FixedAssetsControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] FixedAssetTransferPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.FaAssetTransfers
            .AsNoTracking()
            .Include(x => x.Asset)
            .Include(x => x.FromLocation)
            .Include(x => x.ToLocation)
            .Include(x => x.FromDepartment)
            .Include(x => x.ToDepartment)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.TransferNo.ToLower().Contains(search)
                || x.Asset.AssetCode.ToLower().Contains(search)
                || x.Asset.Name.ToLower().Contains(search)
                || x.FromLocation.Name.ToLower().Contains(search)
                || x.ToLocation.Name.ToLower().Contains(search));
        }

        if (request.AssetId.HasValue)
        {
            query = query.Where(x => x.AssetId == request.AssetId.Value);
        }

        if (request.FromLocationId.HasValue)
        {
            query = query.Where(x => x.FromLocationId == request.FromLocationId.Value);
        }

        if (request.ToLocationId.HasValue)
        {
            query = query.Where(x => x.ToLocationId == request.ToLocationId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (request.TransferDateFrom.HasValue)
        {
            query = query.Where(x => x.TransferDate >= request.TransferDateFrom.Value);
        }

        if (request.TransferDateTo.HasValue)
        {
            query = query.Where(x => x.TransferDate <= request.TransferDateTo.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "transferno" => isDesc ? query.OrderByDescending(x => x.TransferNo) : query.OrderBy(x => x.TransferNo),
            "assetcode" => isDesc ? query.OrderByDescending(x => x.Asset.AssetCode).ThenByDescending(x => x.TransferNo) : query.OrderBy(x => x.Asset.AssetCode).ThenBy(x => x.TransferNo),
            "transferdate" => isDesc ? query.OrderByDescending(x => x.TransferDate).ThenByDescending(x => x.TransferNo) : query.OrderBy(x => x.TransferDate).ThenBy(x => x.TransferNo),
            "fromlocationname" => isDesc ? query.OrderByDescending(x => x.FromLocation.Name).ThenByDescending(x => x.TransferNo) : query.OrderBy(x => x.FromLocation.Name).ThenBy(x => x.TransferNo),
            "tolocationname" => isDesc ? query.OrderByDescending(x => x.ToLocation.Name).ThenByDescending(x => x.TransferNo) : query.OrderBy(x => x.ToLocation.Name).ThenBy(x => x.TransferNo),
            "status" => isDesc ? query.OrderByDescending(x => x.Status).ThenByDescending(x => x.TransferNo) : query.OrderBy(x => x.Status).ThenBy(x => x.TransferNo),
            _ => isDesc ? query.OrderByDescending(x => x.TransferDate).ThenByDescending(x => x.TransferNo) : query.OrderBy(x => x.TransferDate).ThenBy(x => x.TransferNo)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapDto(x))
            .ToListAsync(ct);

        return Ok(PagedResult<FixedAssetTransferDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.FaAssetTransfers
            .AsNoTracking()
            .Include(x => x.Asset)
            .Include(x => x.FromLocation)
            .Include(x => x.ToLocation)
            .Include(x => x.FromDepartment)
            .Include(x => x.ToDepartment)
            .Where(x => x.Id == id)
            .Select(x => MapDto(x))
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FixedAssetTransferDto request, CancellationToken ct)
    {
        try
        {
            var asset = await ValidateAndGetAssetAsync(request.AssetId, ct);
            await ValidateRequestAsync(request, ct);

            var transferNo = await GenerateTransferNoAsync(request.TransferDate, ct);

            var entity = new FaAssetTransfer
            {
                TransferNo = transferNo,
                AssetId = request.AssetId,
                TransferDate = request.TransferDate,
                FromLocationId = request.FromLocationId,
                ToLocationId = request.ToLocationId,
                FromDepartmentId = NormalizeOptionalId(request.FromDepartmentId) ?? asset.DepartmentId,
                ToDepartmentId = NormalizeOptionalId(request.ToDepartmentId),
                Reason = NormalizeOptional(request.Reason),
                Status = AssetTransferStatus.Draft,
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            };

            dbContext.FaAssetTransfers.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var created = await dbContext.FaAssetTransfers
                .AsNoTracking()
                .Include(x => x.Asset)
                .Include(x => x.FromLocation)
                .Include(x => x.ToLocation)
                .Include(x => x.FromDepartment)
                .Include(x => x.ToDepartment)
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
    public async Task<IActionResult> Update(int id, [FromBody] FixedAssetTransferDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.FaAssetTransfers.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            if (entity.Status != AssetTransferStatus.Draft)
            {
                return BadRequest(new { message = "Only draft transfer can be edited." });
            }

            var asset = await ValidateAndGetAssetAsync(request.AssetId, ct);
            await ValidateRequestAsync(request, ct);

            entity.AssetId = request.AssetId;
            entity.TransferDate = request.TransferDate;
            entity.FromLocationId = request.FromLocationId;
            entity.ToLocationId = request.ToLocationId;
            entity.FromDepartmentId = NormalizeOptionalId(request.FromDepartmentId) ?? asset.DepartmentId;
            entity.ToDepartmentId = NormalizeOptionalId(request.ToDepartmentId);
            entity.Reason = NormalizeOptional(request.Reason);
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(ct);

            var updated = await dbContext.FaAssetTransfers
                .AsNoTracking()
                .Include(x => x.Asset)
                .Include(x => x.FromLocation)
                .Include(x => x.ToLocation)
                .Include(x => x.FromDepartment)
                .Include(x => x.ToDepartment)
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
        var entity = await dbContext.FaAssetTransfers.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != AssetTransferStatus.Draft)
        {
            return BadRequest(new { message = "Only draft transfer can be deleted." });
        }

        dbContext.FaAssetTransfers.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPut("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        var entity = await dbContext.FaAssetTransfers
            .Include(x => x.Asset)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != AssetTransferStatus.Draft)
        {
            return BadRequest(new { message = "Only draft transfer can be approved." });
        }

        entity.Status = AssetTransferStatus.Approved;
        entity.ApprovedBy = GetCurrentUserId();
        entity.ApprovedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        entity.Asset.LocationId = entity.ToLocationId;
        entity.Asset.DepartmentId = entity.ToDepartmentId;
        if (entity.Asset.Status == AssetStatus.InMaintenance)
        {
            entity.Asset.Status = AssetStatus.Active;
        }

        entity.Asset.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.Asset.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        var historyExists = await dbContext.FaAssetHistories
            .AnyAsync(x => x.AssetId == entity.AssetId && x.EventType == AssetHistoryType.Transfer && x.ReferenceNo == entity.TransferNo, ct);

        if (!historyExists)
        {
            dbContext.FaAssetHistories.Add(new FaAssetHistory
            {
                AssetId = entity.AssetId,
                EventDate = entity.TransferDate,
                EventType = AssetHistoryType.Transfer,
                ReferenceNo = entity.TransferNo,
                Description = NormalizeOptional(entity.Reason) ?? "Asset transfer approved",
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            });

            await dbContext.SaveChangesAsync(ct);
        }

        return NoContent();
    }

    [HttpPut("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, CancellationToken ct)
    {
        var entity = await dbContext.FaAssetTransfers.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != AssetTransferStatus.Draft)
        {
            return BadRequest(new { message = "Only draft transfer can be rejected." });
        }

        entity.Status = AssetTransferStatus.Rejected;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(ct);

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
            throw new InvalidOperationException("Disposed asset cannot be transferred.");
        }

        return asset;
    }

    private async Task ValidateRequestAsync(FixedAssetTransferDto request, CancellationToken ct)
    {
        if (request.FromLocationId <= 0 || request.ToLocationId <= 0)
        {
            throw new InvalidOperationException("From and to locations are required.");
        }

        if (request.FromLocationId == request.ToLocationId)
        {
            throw new InvalidOperationException("Transfer locations cannot be the same.");
        }

        var locationIds = new[] { request.FromLocationId, request.ToLocationId };
        var locationCount = await dbContext.FaLocations
            .AsNoTracking()
            .CountAsync(x => locationIds.Contains(x.Id), ct);

        if (locationCount != 2)
        {
            throw new InvalidOperationException("Invalid transfer location.");
        }

        var departmentIds = new[] { NormalizeOptionalId(request.FromDepartmentId), NormalizeOptionalId(request.ToDepartmentId) }
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToArray();

        if (departmentIds.Length == 0)
        {
            return;
        }

        var departmentCount = await dbContext.HrDepartments
            .AsNoTracking()
            .CountAsync(x => departmentIds.Contains(x.Id), ct);

        if (departmentCount != departmentIds.Length)
        {
            throw new InvalidOperationException("Invalid department on transfer.");
        }
    }

    private async Task<string> GenerateTransferNoAsync(DateOnly transferDate, CancellationToken ct)
    {
        var prefix = $"FATR-{transferDate:yyyyMM}-";
        var latest = await dbContext.FaAssetTransfers
            .IgnoreQueryFilters()
            .Where(x => x.TransferNo.StartsWith(prefix))
            .OrderByDescending(x => x.TransferNo)
            .Select(x => x.TransferNo)
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

    private static int? NormalizeOptionalId(int? value) => value is > 0 ? value : null;

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static FixedAssetTransferDto MapDto(FaAssetTransfer entity)
    {
        return new FixedAssetTransferDto
        {
            Id = entity.Id,
            TransferNo = entity.TransferNo,
            AssetId = entity.AssetId,
            AssetCode = entity.Asset.AssetCode,
            AssetName = entity.Asset.Name,
            TransferDate = entity.TransferDate,
            FromLocationId = entity.FromLocationId,
            FromLocationName = entity.FromLocation.Name,
            ToLocationId = entity.ToLocationId,
            ToLocationName = entity.ToLocation.Name,
            FromDepartmentId = entity.FromDepartmentId,
            FromDepartmentName = entity.FromDepartment != null ? entity.FromDepartment.Name : null,
            ToDepartmentId = entity.ToDepartmentId,
            ToDepartmentName = entity.ToDepartment != null ? entity.ToDepartment.Name : null,
            Reason = entity.Reason,
            Status = entity.Status
        };
    }
}
