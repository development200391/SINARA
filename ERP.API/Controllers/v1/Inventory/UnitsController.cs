using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;
using ERP.Domain.Entities.Inventory;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Inventory;

[Route("api/v1/inventory/units")]
public sealed class UnitsController(AppDbContext dbContext) : InventoryControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] UnitOfMeasurePagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.InvUnitsOfMeasure.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(search) ||
                x.Name.ToLower().Contains(search) ||
                (x.Description != null && x.Description.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var code = request.Code.Trim().ToLowerInvariant();
            query = query.Where(x => x.Code.ToLower().Contains(code));
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var name = request.Name.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(name));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "name" => isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Code),
            _ => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapDto(x))
            .ToListAsync(ct);

        return Ok(PagedResult<UnitOfMeasureDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("options")]
    public async Task<IActionResult> GetOptions(CancellationToken ct)
    {
        var options = await dbContext.InvUnitsOfMeasure
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new InventoryOptionDto
            {
                Id = x.Id,
                Label = x.Code + " - " + x.Name
            })
            .ToListAsync(ct);

        return Ok(options);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.InvUnitsOfMeasure
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => MapDto(x))
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UnitOfMeasureDto request, CancellationToken ct)
    {
        try
        {
            var normalizedCode = NormalizeRequired(request.Code, "Unit code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Unit name is required.");

            var duplicate = await dbContext.InvUnitsOfMeasure
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Code == normalizedCode, ct);
            if (duplicate)
            {
                return BadRequest(new { message = "Unit code already exists." });
            }

            var entity = new InvUnitOfMeasure
            {
                Code = normalizedCode,
                Name = normalizedName,
                Description = NormalizeOptional(request.Description),
                IsActive = request.IsActive,
                CreatedBy = "system"
            };

            dbContext.InvUnitsOfMeasure.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            return Ok(MapDto(entity));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UnitOfMeasureDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.InvUnitsOfMeasure.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            var normalizedCode = NormalizeRequired(request.Code, "Unit code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Unit name is required.");

            var duplicate = await dbContext.InvUnitsOfMeasure
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Id != id && x.Code == normalizedCode, ct);
            if (duplicate)
            {
                return BadRequest(new { message = "Unit code already exists." });
            }

            entity.Code = normalizedCode;
            entity.Name = normalizedName;
            entity.Description = NormalizeOptional(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdatedBy = "system";
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
        var entity = await dbContext.InvUnitsOfMeasure.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        var inUseByBase = await dbContext.InvItems.AnyAsync(x => x.BaseUomId == id, ct);
        var inUseByPurchase = await dbContext.InvItems.AnyAsync(x => x.PurchaseUomId == id, ct);
        var inUseByConversion = await dbContext.InvItemUnitConversions.AnyAsync(x => x.FromUomId == id || x.ToUomId == id, ct);
        if (inUseByBase || inUseByPurchase || inUseByConversion)
        {
            return BadRequest(new { message = "Unit cannot be deleted because it is used by item data." });
        }

        dbContext.InvUnitsOfMeasure.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private static UnitOfMeasureDto MapDto(InvUnitOfMeasure entity)
    {
        return new UnitOfMeasureDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive
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

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
