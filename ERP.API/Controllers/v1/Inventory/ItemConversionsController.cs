using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;
using ERP.Domain.Entities.Inventory;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Inventory;

[Route("api/v1/inventory/item-conversions")]
public sealed class ItemConversionsController(AppDbContext dbContext) : InventoryControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ItemUnitConversionPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.InvItemUnitConversions
            .AsNoTracking()
            .Include(x => x.Item)
            .Include(x => x.FromUom)
            .Include(x => x.ToUom)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Item.ItemCode.ToLower().Contains(search) ||
                x.Item.Name.ToLower().Contains(search) ||
                x.FromUom.Code.ToLower().Contains(search) ||
                x.ToUom.Code.ToLower().Contains(search));
        }

        if (request.ItemId.HasValue)
        {
            query = query.Where(x => x.ItemId == request.ItemId.Value);
        }

        if (request.FromUomId.HasValue)
        {
            query = query.Where(x => x.FromUomId == request.FromUomId.Value);
        }

        if (request.ToUomId.HasValue)
        {
            query = query.Where(x => x.ToUomId == request.ToUomId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        if (request.FactorFrom.HasValue)
        {
            query = query.Where(x => x.ConversionFactor >= request.FactorFrom.Value);
        }

        if (request.FactorTo.HasValue)
        {
            query = query.Where(x => x.ConversionFactor <= request.FactorTo.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "itemcode" => isDesc ? query.OrderByDescending(x => x.Item.ItemCode) : query.OrderBy(x => x.Item.ItemCode),
            "fromuomcode" => isDesc ? query.OrderByDescending(x => x.FromUom.Code).ThenByDescending(x => x.Item.ItemCode) : query.OrderBy(x => x.FromUom.Code).ThenBy(x => x.Item.ItemCode),
            "touomcode" => isDesc ? query.OrderByDescending(x => x.ToUom.Code).ThenByDescending(x => x.Item.ItemCode) : query.OrderBy(x => x.ToUom.Code).ThenBy(x => x.Item.ItemCode),
            "conversionfactor" => isDesc ? query.OrderByDescending(x => x.ConversionFactor) : query.OrderBy(x => x.ConversionFactor),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Item.ItemCode) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Item.ItemCode),
            _ => isDesc ? query.OrderByDescending(x => x.Item.ItemCode) : query.OrderBy(x => x.Item.ItemCode)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapDto(x))
            .ToListAsync(ct);

        return Ok(PagedResult<ItemUnitConversionDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.InvItemUnitConversions
            .AsNoTracking()
            .Include(x => x.Item)
            .Include(x => x.FromUom)
            .Include(x => x.ToUom)
            .Where(x => x.Id == id)
            .Select(x => MapDto(x))
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ItemUnitConversionDto request, CancellationToken ct)
    {
        try
        {
            await ValidateRequestAsync(request, null, ct);

            var entity = new InvItemUnitConversion
            {
                ItemId = request.ItemId,
                FromUomId = request.FromUomId,
                ToUomId = request.ToUomId,
                ConversionFactor = request.ConversionFactor,
                IsActive = request.IsActive,
                CreatedBy = "system"
            };

            dbContext.InvItemUnitConversions.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var created = await dbContext.InvItemUnitConversions
                .AsNoTracking()
                .Include(x => x.Item)
                .Include(x => x.FromUom)
                .Include(x => x.ToUom)
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
    public async Task<IActionResult> Update(int id, [FromBody] ItemUnitConversionDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.InvItemUnitConversions.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            await ValidateRequestAsync(request, id, ct);

            entity.ItemId = request.ItemId;
            entity.FromUomId = request.FromUomId;
            entity.ToUomId = request.ToUomId;
            entity.ConversionFactor = request.ConversionFactor;
            entity.IsActive = request.IsActive;
            entity.UpdatedBy = "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(ct);

            var updated = await dbContext.InvItemUnitConversions
                .AsNoTracking()
                .Include(x => x.Item)
                .Include(x => x.FromUom)
                .Include(x => x.ToUom)
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
        var entity = await dbContext.InvItemUnitConversions.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        dbContext.InvItemUnitConversions.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task ValidateRequestAsync(ItemUnitConversionDto request, int? currentId, CancellationToken ct)
    {
        if (request.ItemId <= 0 || !await dbContext.InvItems.AnyAsync(x => x.Id == request.ItemId, ct))
        {
            throw new InvalidOperationException("Item not found.");
        }

        if (request.FromUomId <= 0 || !await dbContext.InvUnitsOfMeasure.AnyAsync(x => x.Id == request.FromUomId, ct))
        {
            throw new InvalidOperationException("From UOM not found.");
        }

        if (request.ToUomId <= 0 || !await dbContext.InvUnitsOfMeasure.AnyAsync(x => x.Id == request.ToUomId, ct))
        {
            throw new InvalidOperationException("To UOM not found.");
        }

        if (request.FromUomId == request.ToUomId)
        {
            throw new InvalidOperationException("From UOM and To UOM must be different.");
        }

        if (request.ConversionFactor <= 0)
        {
            throw new InvalidOperationException("Conversion factor must be greater than zero.");
        }

        var duplicate = await dbContext.InvItemUnitConversions
            .IgnoreQueryFilters()
            .AnyAsync(x =>
                x.Id != (currentId ?? 0) &&
                x.ItemId == request.ItemId &&
                x.FromUomId == request.FromUomId &&
                x.ToUomId == request.ToUomId,
                ct);

        if (duplicate)
        {
            throw new InvalidOperationException("Conversion mapping already exists for this item and UOM pair.");
        }
    }

    private static ItemUnitConversionDto MapDto(InvItemUnitConversion entity)
    {
        return new ItemUnitConversionDto
        {
            Id = entity.Id,
            ItemId = entity.ItemId,
            ItemCode = entity.Item?.ItemCode ?? string.Empty,
            ItemName = entity.Item?.Name ?? string.Empty,
            FromUomId = entity.FromUomId,
            FromUomCode = entity.FromUom?.Code ?? string.Empty,
            ToUomId = entity.ToUomId,
            ToUomCode = entity.ToUom?.Code ?? string.Empty,
            ConversionFactor = entity.ConversionFactor,
            IsActive = entity.IsActive
        };
    }
}
