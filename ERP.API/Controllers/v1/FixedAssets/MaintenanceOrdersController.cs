using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.FixedAssets;
using ERP.Domain.Entities.FixedAssets;
using ERP.Domain.Enums.FixedAssets;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.FixedAssets;

[Route("api/v1/fixed-assets/maintenance-orders")]
public sealed class MaintenanceOrdersController(AppDbContext dbContext) : FixedAssetsControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] FixedAssetMaintenanceOrderPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.FaMaintenanceOrders
            .AsNoTracking()
            .Include(x => x.Asset)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.WorkOrderNo.ToLower().Contains(search)
                || x.Asset.AssetCode.ToLower().Contains(search)
                || x.Asset.Name.ToLower().Contains(search)
                || (x.VendorName != null && x.VendorName.ToLower().Contains(search)));
        }

        if (request.AssetId.HasValue)
        {
            query = query.Where(x => x.AssetId == request.AssetId.Value);
        }

        if (request.MaintenanceType.HasValue)
        {
            query = query.Where(x => x.MaintenanceType == request.MaintenanceType.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (request.OrderDateFrom.HasValue)
        {
            query = query.Where(x => x.OrderDate >= request.OrderDateFrom.Value);
        }

        if (request.OrderDateTo.HasValue)
        {
            query = query.Where(x => x.OrderDate <= request.OrderDateTo.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "workorderno" => isDesc ? query.OrderByDescending(x => x.WorkOrderNo) : query.OrderBy(x => x.WorkOrderNo),
            "assetcode" => isDesc ? query.OrderByDescending(x => x.Asset.AssetCode).ThenByDescending(x => x.WorkOrderNo) : query.OrderBy(x => x.Asset.AssetCode).ThenBy(x => x.WorkOrderNo),
            "orderdate" => isDesc ? query.OrderByDescending(x => x.OrderDate).ThenByDescending(x => x.WorkOrderNo) : query.OrderBy(x => x.OrderDate).ThenBy(x => x.WorkOrderNo),
            "maintenancetype" => isDesc ? query.OrderByDescending(x => x.MaintenanceType).ThenByDescending(x => x.WorkOrderNo) : query.OrderBy(x => x.MaintenanceType).ThenBy(x => x.WorkOrderNo),
            "cost" => isDesc ? query.OrderByDescending(x => x.Cost).ThenByDescending(x => x.WorkOrderNo) : query.OrderBy(x => x.Cost).ThenBy(x => x.WorkOrderNo),
            "status" => isDesc ? query.OrderByDescending(x => x.Status).ThenByDescending(x => x.WorkOrderNo) : query.OrderBy(x => x.Status).ThenBy(x => x.WorkOrderNo),
            _ => isDesc ? query.OrderByDescending(x => x.OrderDate).ThenByDescending(x => x.WorkOrderNo) : query.OrderBy(x => x.OrderDate).ThenBy(x => x.WorkOrderNo)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapDto(x))
            .ToListAsync(ct);

        return Ok(PagedResult<FixedAssetMaintenanceOrderDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.FaMaintenanceOrders
            .AsNoTracking()
            .Include(x => x.Asset)
            .Where(x => x.Id == id)
            .Select(x => MapDto(x))
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FixedAssetMaintenanceOrderDto request, CancellationToken ct)
    {
        try
        {
            var asset = await ValidateAndGetAssetAsync(request.AssetId, ct);
            ValidateRequest(request);

            var workOrderNo = await GenerateWorkOrderNoAsync(request.OrderDate, ct);

            var entity = new FaMaintenanceOrder
            {
                WorkOrderNo = workOrderNo,
                AssetId = request.AssetId,
                OrderDate = request.OrderDate,
                MaintenanceType = request.MaintenanceType,
                VendorName = NormalizeOptional(request.VendorName),
                Cost = NormalizeAmount(request.Cost),
                IsCapitalized = request.IsCapitalized,
                Status = MaintenanceStatus.Open,
                Notes = NormalizeOptional(request.Notes),
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            };

            dbContext.FaMaintenanceOrders.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            if (asset.Status != AssetStatus.Disposed)
            {
                asset.Status = AssetStatus.InMaintenance;
                asset.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
                asset.UpdatedAt = DateTimeOffset.UtcNow;
                await dbContext.SaveChangesAsync(ct);
            }

            var created = await dbContext.FaMaintenanceOrders
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
    public async Task<IActionResult> Update(int id, [FromBody] FixedAssetMaintenanceOrderDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.FaMaintenanceOrders.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            if (entity.Status is MaintenanceStatus.Completed or MaintenanceStatus.Cancelled)
            {
                return BadRequest(new { message = "Completed or cancelled maintenance order cannot be edited." });
            }

            await ValidateAndGetAssetAsync(request.AssetId, ct);
            ValidateRequest(request);

            entity.AssetId = request.AssetId;
            entity.OrderDate = request.OrderDate;
            entity.MaintenanceType = request.MaintenanceType;
            entity.VendorName = NormalizeOptional(request.VendorName);
            entity.Cost = NormalizeAmount(request.Cost);
            entity.IsCapitalized = request.IsCapitalized;
            entity.Notes = NormalizeOptional(request.Notes);
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(ct);

            var updated = await dbContext.FaMaintenanceOrders
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
        var entity = await dbContext.FaMaintenanceOrders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != MaintenanceStatus.Open)
        {
            return BadRequest(new { message = "Only open maintenance order can be deleted." });
        }

        dbContext.FaMaintenanceOrders.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPut("{id:int}/start")]
    public async Task<IActionResult> Start(int id, CancellationToken ct)
    {
        var entity = await dbContext.FaMaintenanceOrders
            .Include(x => x.Asset)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != MaintenanceStatus.Open)
        {
            return BadRequest(new { message = "Only open maintenance order can be started." });
        }

        entity.Status = MaintenanceStatus.InProgress;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        if (entity.Asset.Status != AssetStatus.Disposed)
        {
            entity.Asset.Status = AssetStatus.InMaintenance;
            entity.Asset.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.Asset.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPut("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id, CancellationToken ct)
    {
        var entity = await dbContext.FaMaintenanceOrders
            .Include(x => x.Asset)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status is MaintenanceStatus.Completed or MaintenanceStatus.Cancelled)
        {
            return BadRequest(new { message = "Maintenance order is already completed or cancelled." });
        }

        entity.Status = MaintenanceStatus.Completed;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        if (entity.IsCapitalized && entity.Cost > 0)
        {
            entity.Asset.AcquisitionCost = decimal.Round(entity.Asset.AcquisitionCost + entity.Cost, 2, MidpointRounding.AwayFromZero);
            entity.Asset.BookValue = decimal.Round(entity.Asset.BookValue + entity.Cost, 2, MidpointRounding.AwayFromZero);
        }

        if (entity.Asset.Status != AssetStatus.Disposed)
        {
            entity.Asset.Status = AssetStatus.Active;
        }

        entity.Asset.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.Asset.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        var historyExists = await dbContext.FaAssetHistories
            .AnyAsync(x => x.AssetId == entity.AssetId && x.EventType == AssetHistoryType.Maintenance && x.ReferenceNo == entity.WorkOrderNo, ct);

        if (!historyExists)
        {
            dbContext.FaAssetHistories.Add(new FaAssetHistory
            {
                AssetId = entity.AssetId,
                EventDate = entity.OrderDate,
                EventType = AssetHistoryType.Maintenance,
                ReferenceNo = entity.WorkOrderNo,
                Description = "Maintenance completed",
                AmountChange = entity.IsCapitalized
                    ? NormalizeAmount(entity.Cost)
                    : NormalizeAmount(-entity.Cost),
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            });

            await dbContext.SaveChangesAsync(ct);
        }

        return NoContent();
    }

    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        var entity = await dbContext.FaMaintenanceOrders
            .Include(x => x.Asset)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status is MaintenanceStatus.Completed or MaintenanceStatus.Cancelled)
        {
            return BadRequest(new { message = "Maintenance order is already completed or cancelled." });
        }

        entity.Status = MaintenanceStatus.Cancelled;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        if (entity.Asset.Status != AssetStatus.Disposed)
        {
            entity.Asset.Status = AssetStatus.Active;
            entity.Asset.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.Asset.UpdatedAt = DateTimeOffset.UtcNow;
        }

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
            throw new InvalidOperationException("Disposed asset cannot be maintained.");
        }

        return asset;
    }

    private static void ValidateRequest(FixedAssetMaintenanceOrderDto request)
    {
        if (request.OrderDate == default)
        {
            throw new InvalidOperationException("Order date is required.");
        }

        if (request.Cost < 0)
        {
            throw new InvalidOperationException("Cost cannot be negative.");
        }
    }

    private async Task<string> GenerateWorkOrderNoAsync(DateOnly orderDate, CancellationToken ct)
    {
        var prefix = $"FAMO-{orderDate:yyyyMM}-";
        var latest = await dbContext.FaMaintenanceOrders
            .IgnoreQueryFilters()
            .Where(x => x.WorkOrderNo.StartsWith(prefix))
            .OrderByDescending(x => x.WorkOrderNo)
            .Select(x => x.WorkOrderNo)
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

    private static FixedAssetMaintenanceOrderDto MapDto(FaMaintenanceOrder entity)
    {
        return new FixedAssetMaintenanceOrderDto
        {
            Id = entity.Id,
            WorkOrderNo = entity.WorkOrderNo,
            AssetId = entity.AssetId,
            AssetCode = entity.Asset.AssetCode,
            AssetName = entity.Asset.Name,
            OrderDate = entity.OrderDate,
            MaintenanceType = entity.MaintenanceType,
            VendorName = entity.VendorName,
            Cost = entity.Cost,
            IsCapitalized = entity.IsCapitalized,
            Status = entity.Status,
            Notes = entity.Notes
        };
    }
}
