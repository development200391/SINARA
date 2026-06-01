using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;
using ERP.Domain.Entities.Inventory;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Inventory;

[Route("api/v1/inventory/brands")]
public sealed class BrandsController(AppDbContext dbContext) : InventoryControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] BrandPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.InvBrands.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Name.ToLower().Contains(search) ||
                (x.Description != null && x.Description.ToLower().Contains(search)));
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
            "name" => isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Name) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Name),
            _ => isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapDto(x))
            .ToListAsync(ct);

        return Ok(PagedResult<BrandDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("options")]
    public async Task<IActionResult> GetOptions(CancellationToken ct)
    {
        var options = await dbContext.InvBrands
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new InventoryOptionDto
            {
                Id = x.Id,
                Label = x.Name
            })
            .ToListAsync(ct);

        return Ok(options);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.InvBrands
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => MapDto(x))
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BrandDto request, CancellationToken ct)
    {
        try
        {
            var normalizedName = NormalizeRequired(request.Name, "Brand name is required.");

            var duplicate = await dbContext.InvBrands
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Name.ToLower() == normalizedName.ToLower(), ct);
            if (duplicate)
            {
                return BadRequest(new { message = "Brand name already exists." });
            }

            var entity = new InvBrand
            {
                Name = normalizedName,
                Description = NormalizeOptional(request.Description),
                IsActive = request.IsActive,
                CreatedBy = "system"
            };

            dbContext.InvBrands.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            return Ok(MapDto(entity));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] BrandDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.InvBrands.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            var normalizedName = NormalizeRequired(request.Name, "Brand name is required.");

            var duplicate = await dbContext.InvBrands
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Id != id && x.Name.ToLower() == normalizedName.ToLower(), ct);
            if (duplicate)
            {
                return BadRequest(new { message = "Brand name already exists." });
            }

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
        var entity = await dbContext.InvBrands.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        var inUse = await dbContext.InvItems.AnyAsync(x => x.BrandId == id, ct);
        if (inUse)
        {
            return BadRequest(new { message = "Brand cannot be deleted because it is used by items." });
        }

        dbContext.InvBrands.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private static BrandDto MapDto(InvBrand entity)
    {
        return new BrandDto
        {
            Id = entity.Id,
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
