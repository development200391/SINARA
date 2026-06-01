using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;
using ERP.Domain.Entities.Inventory;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Inventory;

[Route("api/v1/inventory/categories")]
public sealed class CategoriesController(AppDbContext dbContext) : InventoryControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ItemCategoryPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.InvItemCategories
            .AsNoTracking()
            .Include(x => x.ParentCategory)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(search) ||
                x.Name.ToLower().Contains(search) ||
                (x.Description != null && x.Description.ToLower().Contains(search)) ||
                (x.ParentCategory != null && x.ParentCategory.Name.ToLower().Contains(search)));
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

        if (request.ParentCategoryId.HasValue)
        {
            query = query.Where(x => x.ParentCategoryId == request.ParentCategoryId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "name" => isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "parentcategoryname" => isDesc ? query.OrderByDescending(x => x.ParentCategory!.Name).ThenByDescending(x => x.Code) : query.OrderBy(x => x.ParentCategory!.Name).ThenBy(x => x.Code),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Code),
            _ => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapDto(x))
            .ToListAsync(ct);

        return Ok(PagedResult<ItemCategoryDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("options")]
    public async Task<IActionResult> GetOptions(CancellationToken ct)
    {
        var options = await dbContext.InvItemCategories
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
        var item = await dbContext.InvItemCategories
            .AsNoTracking()
            .Include(x => x.ParentCategory)
            .Where(x => x.Id == id)
            .Select(x => MapDto(x))
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ItemCategoryDto request, CancellationToken ct)
    {
        try
        {
            var normalizedCode = NormalizeRequired(request.Code, "Category code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Category name is required.");
            var parentCategoryId = request.ParentCategoryId is > 0 ? request.ParentCategoryId : null;

            if (parentCategoryId.HasValue)
            {
                var parentExists = await dbContext.InvItemCategories.AnyAsync(x => x.Id == parentCategoryId.Value, ct);
                if (!parentExists)
                {
                    return BadRequest(new { message = "Parent category not found." });
                }
            }

            var duplicate = await dbContext.InvItemCategories
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Code == normalizedCode, ct);
            if (duplicate)
            {
                return BadRequest(new { message = "Category code already exists." });
            }

            var entity = new InvItemCategory
            {
                Code = normalizedCode,
                Name = normalizedName,
                ParentCategoryId = parentCategoryId,
                Description = NormalizeOptional(request.Description),
                IsActive = request.IsActive,
                CreatedBy = "system"
            };

            dbContext.InvItemCategories.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var created = await dbContext.InvItemCategories
                .AsNoTracking()
                .Include(x => x.ParentCategory)
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
    public async Task<IActionResult> Update(int id, [FromBody] ItemCategoryDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.InvItemCategories.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            var normalizedCode = NormalizeRequired(request.Code, "Category code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Category name is required.");
            var parentCategoryId = request.ParentCategoryId is > 0 ? request.ParentCategoryId : null;

            if (parentCategoryId == id)
            {
                return BadRequest(new { message = "Category cannot be its own parent." });
            }

            if (parentCategoryId.HasValue)
            {
                var parentExists = await dbContext.InvItemCategories.AnyAsync(x => x.Id == parentCategoryId.Value, ct);
                if (!parentExists)
                {
                    return BadRequest(new { message = "Parent category not found." });
                }
            }

            var duplicate = await dbContext.InvItemCategories
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Id != id && x.Code == normalizedCode, ct);
            if (duplicate)
            {
                return BadRequest(new { message = "Category code already exists." });
            }

            entity.Code = normalizedCode;
            entity.Name = normalizedName;
            entity.ParentCategoryId = parentCategoryId;
            entity.Description = NormalizeOptional(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdatedBy = "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(ct);

            var updated = await dbContext.InvItemCategories
                .AsNoTracking()
                .Include(x => x.ParentCategory)
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
        var entity = await dbContext.InvItemCategories.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        var hasChildren = await dbContext.InvItemCategories.AnyAsync(x => x.ParentCategoryId == id, ct);
        if (hasChildren)
        {
            return BadRequest(new { message = "Category cannot be deleted because it has child categories." });
        }

        var hasItems = await dbContext.InvItems.AnyAsync(x => x.CategoryId == id, ct);
        if (hasItems)
        {
            return BadRequest(new { message = "Category cannot be deleted because it is used by items." });
        }

        dbContext.InvItemCategories.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private static ItemCategoryDto MapDto(InvItemCategory entity)
    {
        return new ItemCategoryDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            ParentCategoryId = entity.ParentCategoryId,
            ParentCategoryName = entity.ParentCategory?.Name,
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
