using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Purchasing;
using ERP.Domain.Entities.Purchasing;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Purchasing;

[Route("api/v1/purchasing/buyer-groups")]
public sealed class BuyerGroupsController(AppDbContext dbContext) : PurchasingControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] BuyerGroupPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.PurBuyerGroups
            .AsNoTracking()
            .Include(x => x.BuyerEmployee)
            .Include(x => x.CategoryMappings)
                .ThenInclude(x => x.ItemCategory)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(search) ||
                x.Name.ToLower().Contains(search) ||
                x.BuyerEmployee.FullName.ToLower().Contains(search) ||
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

        if (request.BuyerEmployeeId.HasValue)
        {
            query = query.Where(x => x.BuyerEmployeeId == request.BuyerEmployeeId.Value);
        }

        if (request.ItemCategoryId.HasValue)
        {
            query = query.Where(x => x.CategoryMappings.Any(m => m.ItemCategoryId == request.ItemCategoryId.Value));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "name" => isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "buyeremployeename" => isDesc ? query.OrderByDescending(x => x.BuyerEmployee.FullName).ThenByDescending(x => x.Code) : query.OrderBy(x => x.BuyerEmployee.FullName).ThenBy(x => x.Code),
            "mappedcategorycount" => isDesc ? query.OrderByDescending(x => x.CategoryMappings.Count).ThenByDescending(x => x.Code) : query.OrderBy(x => x.CategoryMappings.Count).ThenBy(x => x.Code),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Code),
            _ => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };

        var totalCount = await query.CountAsync(ct);
        var entities = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = entities.Select(MapDto).ToList();
        return Ok(PagedResult<BuyerGroupDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("options")]
    public async Task<IActionResult> GetOptions(CancellationToken ct)
    {
        var options = await dbContext.PurBuyerGroups
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new PurchasingOptionDto
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
        var entity = await dbContext.PurBuyerGroups
            .AsNoTracking()
            .Include(x => x.BuyerEmployee)
            .Include(x => x.CategoryMappings)
                .ThenInclude(x => x.ItemCategory)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return entity is null ? NotFound() : Ok(MapDto(entity));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BuyerGroupDto request, CancellationToken ct)
    {
        var normalizedCode = NormalizeRequired(request.Code, "Buyer group code is required.").ToUpperInvariant();
        var normalizedName = NormalizeRequired(request.Name, "Buyer group name is required.");
        var normalizedCategoryIds = NormalizeCategoryIds(request.ItemCategoryIds);

        var validation = await ValidateRequestAsync(request.BuyerEmployeeId, normalizedCode, null, normalizedCategoryIds, ct);
        if (validation is not null)
        {
            return validation;
        }

        var entity = new PurBuyerGroup
        {
            Code = normalizedCode,
            Name = normalizedName,
            BuyerEmployeeId = request.BuyerEmployeeId,
            Description = NormalizeOptional(request.Description),
            IsActive = request.IsActive,
            CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
        };

        foreach (var categoryId in normalizedCategoryIds)
        {
            entity.CategoryMappings.Add(new PurBuyerGroupCategory
            {
                ItemCategoryId = categoryId,
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            });
        }

        dbContext.PurBuyerGroups.Add(entity);
        await dbContext.SaveChangesAsync(ct);

        var created = await dbContext.PurBuyerGroups
            .AsNoTracking()
            .Include(x => x.BuyerEmployee)
            .Include(x => x.CategoryMappings)
                .ThenInclude(x => x.ItemCategory)
            .FirstAsync(x => x.Id == entity.Id, ct);

        return Ok(MapDto(created));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] BuyerGroupDto request, CancellationToken ct)
    {
        var entity = await dbContext.PurBuyerGroups
            .Include(x => x.CategoryMappings)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return NotFound();
        }

        var normalizedCode = NormalizeRequired(request.Code, "Buyer group code is required.").ToUpperInvariant();
        var normalizedName = NormalizeRequired(request.Name, "Buyer group name is required.");
        var normalizedCategoryIds = NormalizeCategoryIds(request.ItemCategoryIds);

        var validation = await ValidateRequestAsync(request.BuyerEmployeeId, normalizedCode, id, normalizedCategoryIds, ct);
        if (validation is not null)
        {
            return validation;
        }

        entity.Code = normalizedCode;
        entity.Name = normalizedName;
        entity.BuyerEmployeeId = request.BuyerEmployeeId;
        entity.Description = NormalizeOptional(request.Description);
        entity.IsActive = request.IsActive;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        dbContext.PurBuyerGroupCategories.RemoveRange(entity.CategoryMappings);
        entity.CategoryMappings.Clear();

        foreach (var categoryId in normalizedCategoryIds)
        {
            entity.CategoryMappings.Add(new PurBuyerGroupCategory
            {
                ItemCategoryId = categoryId,
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            });
        }

        await dbContext.SaveChangesAsync(ct);

        var updated = await dbContext.PurBuyerGroups
            .AsNoTracking()
            .Include(x => x.BuyerEmployee)
            .Include(x => x.CategoryMappings)
                .ThenInclude(x => x.ItemCategory)
            .FirstAsync(x => x.Id == id, ct);

        return Ok(MapDto(updated));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var entity = await dbContext.PurBuyerGroups.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        var hasVendors = await dbContext.FinVendors.AnyAsync(x => x.BuyerGroupId == id, ct);
        if (hasVendors)
        {
            return BadRequest(new { message = "Buyer group is already used by vendors." });
        }

        dbContext.PurBuyerGroups.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task<IActionResult?> ValidateRequestAsync(int buyerEmployeeId, string code, int? id, IReadOnlyList<int> categoryIds, CancellationToken ct)
    {
        if (buyerEmployeeId <= 0)
        {
            return BadRequest(new { message = "Buyer employee is required." });
        }

        var employeeExists = await dbContext.HrEmployees.AnyAsync(x => x.Id == buyerEmployeeId, ct);
        if (!employeeExists)
        {
            return BadRequest(new { message = "Buyer employee not found." });
        }

        var duplicate = await dbContext.PurBuyerGroups
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id != (id ?? 0) && x.Code == code, ct);

        if (duplicate)
        {
            return BadRequest(new { message = "Buyer group code already exists." });
        }

        if (categoryIds.Count > 0)
        {
            var existingIds = await dbContext.InvItemCategories
                .AsNoTracking()
                .Where(x => categoryIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync(ct);

            if (existingIds.Count != categoryIds.Count)
            {
                return BadRequest(new { message = "One or more item categories are invalid." });
            }
        }

        return null;
    }

    private static BuyerGroupDto MapDto(PurBuyerGroup entity)
    {
        var mappings = entity.CategoryMappings
            .OrderBy(x => x.ItemCategory.Code)
            .ToList();

        var categoryIds = mappings
            .Select(x => x.ItemCategoryId)
            .Distinct()
            .ToList();

        var categoryNames = string.Join(", ", mappings.Select(x => x.ItemCategory.Code + " - " + x.ItemCategory.Name));

        return new BuyerGroupDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            BuyerEmployeeId = entity.BuyerEmployeeId,
            BuyerEmployeeName = entity.BuyerEmployee.FullName,
            Description = entity.Description,
            IsActive = entity.IsActive,
            MappedCategoryCount = categoryIds.Count,
            ItemCategoryNames = string.IsNullOrWhiteSpace(categoryNames) ? null : categoryNames,
            ItemCategoryIds = categoryIds
        };
    }

    private static IReadOnlyList<int> NormalizeCategoryIds(IReadOnlyList<int> ids)
    {
        return ids
            .Where(x => x > 0)
            .Distinct()
            .ToList();
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
