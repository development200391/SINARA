using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.FixedAssets;
using ERP.Domain.Entities.FixedAssets;
using ERP.Domain.Enums.FixedAssets;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.FixedAssets;

[Route("api/v1/fixed-assets/asset-categories")]
public sealed class AssetCategoriesController(AppDbContext dbContext) : FixedAssetsControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] FixedAssetCategoryPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.FaAssetCategories
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(search) ||
                x.Name.ToLower().Contains(search));
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

        if (request.DepreciationMethod.HasValue)
        {
            query = query.Where(x => x.DepreciationMethod == request.DepreciationMethod.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "name" => isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "depreciationmethod" => isDesc ? query.OrderByDescending(x => x.DepreciationMethod).ThenByDescending(x => x.Code) : query.OrderBy(x => x.DepreciationMethod).ThenBy(x => x.Code),
            "usefullifemonths" => isDesc ? query.OrderByDescending(x => x.UsefulLifeMonths).ThenByDescending(x => x.Code) : query.OrderBy(x => x.UsefulLifeMonths).ThenBy(x => x.Code),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Code),
            _ => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapDto(x))
            .ToListAsync(ct);

        return Ok(PagedResult<FixedAssetCategoryDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("options")]
    public async Task<IActionResult> GetOptions(CancellationToken ct)
    {
        var options = await dbContext.FaAssetCategories
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new FixedAssetOptionDto
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
        var item = await dbContext.FaAssetCategories
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => MapDto(x))
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FixedAssetCategoryDto request, CancellationToken ct)
    {
        try
        {
            var normalizedCode = NormalizeRequired(request.Code, "Category code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Category name is required.");

            ValidateCategoryRequest(request);
            await ValidateAccountIdsAsync(request, ct);

            var duplicate = await dbContext.FaAssetCategories
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Code == normalizedCode, ct);

            if (duplicate)
            {
                return BadRequest(new { message = "Asset category code already exists." });
            }

            var entity = new FaAssetCategory
            {
                Code = normalizedCode,
                Name = normalizedName,
                DepreciationMethod = request.DepreciationMethod,
                UsefulLifeMonths = request.UsefulLifeMonths,
                DepreciationRate = NormalizeRate(request.DepreciationRate),
                AssetAccountId = NormalizeOptionalId(request.AssetAccountId),
                AccumulatedDepreciationAccountId = NormalizeOptionalId(request.AccumulatedDepreciationAccountId),
                DepreciationExpenseAccountId = NormalizeOptionalId(request.DepreciationExpenseAccountId),
                IsActive = request.IsActive,
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            };

            dbContext.FaAssetCategories.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            return Ok(MapDto(entity));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] FixedAssetCategoryDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.FaAssetCategories.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            var normalizedCode = NormalizeRequired(request.Code, "Category code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Category name is required.");

            ValidateCategoryRequest(request);
            await ValidateAccountIdsAsync(request, ct);

            var duplicate = await dbContext.FaAssetCategories
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Id != id && x.Code == normalizedCode, ct);

            if (duplicate)
            {
                return BadRequest(new { message = "Asset category code already exists." });
            }

            entity.Code = normalizedCode;
            entity.Name = normalizedName;
            entity.DepreciationMethod = request.DepreciationMethod;
            entity.UsefulLifeMonths = request.UsefulLifeMonths;
            entity.DepreciationRate = NormalizeRate(request.DepreciationRate);
            entity.AssetAccountId = NormalizeOptionalId(request.AssetAccountId);
            entity.AccumulatedDepreciationAccountId = NormalizeOptionalId(request.AccumulatedDepreciationAccountId);
            entity.DepreciationExpenseAccountId = NormalizeOptionalId(request.DepreciationExpenseAccountId);
            entity.IsActive = request.IsActive;
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
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
        var entity = await dbContext.FaAssetCategories.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        var inUse = await dbContext.FaAssets.AnyAsync(x => x.CategoryId == id, ct);
        if (inUse)
        {
            return BadRequest(new { message = "Category is already used by assets." });
        }

        dbContext.FaAssetCategories.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task ValidateAccountIdsAsync(FixedAssetCategoryDto request, CancellationToken ct)
    {
        var accountIds = new[]
        {
            NormalizeOptionalId(request.AssetAccountId),
            NormalizeOptionalId(request.AccumulatedDepreciationAccountId),
            NormalizeOptionalId(request.DepreciationExpenseAccountId)
        }
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToArray();

        if (accountIds.Length == 0)
        {
            return;
        }

        var existingIds = await dbContext.FinAccounts
            .AsNoTracking()
            .Where(x => accountIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(ct);

        if (existingIds.Count != accountIds.Length)
        {
            throw new InvalidOperationException("One or more GL accounts are invalid.");
        }
    }

    private static void ValidateCategoryRequest(FixedAssetCategoryDto request)
    {
        if (request.UsefulLifeMonths <= 0)
        {
            throw new InvalidOperationException("Useful life months must be greater than zero.");
        }

        if (request.DepreciationRate.HasValue)
        {
            var rate = request.DepreciationRate.Value;
            if (rate <= 0 || rate > 100)
            {
                throw new InvalidOperationException("Depreciation rate must be between 0 and 100.");
            }
        }

        if (request.DepreciationMethod == DepreciationMethod.DecliningBalance && !request.DepreciationRate.HasValue)
        {
            throw new InvalidOperationException("Depreciation rate is required for declining balance method.");
        }
    }

    private static int? NormalizeOptionalId(int? value) => value is > 0 ? value : null;

    private static decimal? NormalizeRate(decimal? value)
        => value.HasValue ? decimal.Round(value.Value, 4, MidpointRounding.AwayFromZero) : null;

    private static FixedAssetCategoryDto MapDto(FaAssetCategory entity)
    {
        return new FixedAssetCategoryDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            DepreciationMethod = entity.DepreciationMethod,
            UsefulLifeMonths = entity.UsefulLifeMonths,
            DepreciationRate = entity.DepreciationRate,
            AssetAccountId = entity.AssetAccountId,
            AccumulatedDepreciationAccountId = entity.AccumulatedDepreciationAccountId,
            DepreciationExpenseAccountId = entity.DepreciationExpenseAccountId,
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
}
