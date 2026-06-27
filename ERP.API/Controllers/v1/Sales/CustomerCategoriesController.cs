using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Sales;
using ERP.Domain.Entities.Sales;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Sales;

[Route("api/v1/sales/customer-categories")]
public sealed class CustomerCategoriesController(AppDbContext dbContext) : SalesControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] SalesCustomerCategoryPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.SalCustomerCategories
            .AsNoTracking()
            .Include(x => x.DefaultPriceList)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(search) ||
                x.Name.ToLower().Contains(search) ||
                (x.Description != null && x.Description.ToLower().Contains(search)) ||
                (x.DefaultPriceList != null && x.DefaultPriceList.Code.ToLower().Contains(search)) ||
                (x.DefaultPriceList != null && x.DefaultPriceList.Name.ToLower().Contains(search)));
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

        if (request.DefaultPriceListId.HasValue)
        {
            query = query.Where(x => x.DefaultPriceListId == request.DefaultPriceListId.Value);
        }

        if (request.DefaultPaymentTermsFrom.HasValue)
        {
            query = query.Where(x => x.DefaultPaymentTerms >= request.DefaultPaymentTermsFrom.Value);
        }

        if (request.DefaultPaymentTermsTo.HasValue)
        {
            query = query.Where(x => x.DefaultPaymentTerms <= request.DefaultPaymentTermsTo.Value);
        }

        if (request.DefaultCreditLimitFrom.HasValue)
        {
            query = query.Where(x => x.DefaultCreditLimit >= request.DefaultCreditLimitFrom.Value);
        }

        if (request.DefaultCreditLimitTo.HasValue)
        {
            query = query.Where(x => x.DefaultCreditLimit <= request.DefaultCreditLimitTo.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "name" => isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "defaultpricelistname" => isDesc ? query.OrderByDescending(x => x.DefaultPriceList != null ? x.DefaultPriceList.Name : string.Empty).ThenByDescending(x => x.Code) : query.OrderBy(x => x.DefaultPriceList != null ? x.DefaultPriceList.Name : string.Empty).ThenBy(x => x.Code),
            "defaultpaymentterms" => isDesc ? query.OrderByDescending(x => x.DefaultPaymentTerms).ThenByDescending(x => x.Code) : query.OrderBy(x => x.DefaultPaymentTerms).ThenBy(x => x.Code),
            "defaultcreditlimit" => isDesc ? query.OrderByDescending(x => x.DefaultCreditLimit).ThenByDescending(x => x.Code) : query.OrderBy(x => x.DefaultCreditLimit).ThenBy(x => x.Code),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Code),
            _ => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapDto(x))
            .ToListAsync(ct);

        return Ok(PagedResult<SalesCustomerCategoryDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("options")]
    public async Task<IActionResult> GetOptions(CancellationToken ct)
    {
        var options = await dbContext.SalCustomerCategories
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new SalesOptionDto
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
        var item = await dbContext.SalCustomerCategories
            .AsNoTracking()
            .Include(x => x.DefaultPriceList)
            .Where(x => x.Id == id)
            .Select(x => MapDto(x))
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SalesCustomerCategoryDto request, CancellationToken ct)
    {
        try
        {
            var normalizedCode = NormalizeRequired(request.Code, "Category code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Category name is required.");
            var defaultPriceListId = request.DefaultPriceListId is > 0 ? request.DefaultPriceListId : null;

            var validation = await ValidateRequestAsync(normalizedCode, defaultPriceListId, null, request.DefaultPaymentTerms, request.DefaultCreditLimit, ct);
            if (validation is not null)
            {
                return validation;
            }

            var entity = new SalCustomerCategory
            {
                Code = normalizedCode,
                Name = normalizedName,
                DefaultPriceListId = defaultPriceListId,
                DefaultPaymentTerms = request.DefaultPaymentTerms,
                DefaultCreditLimit = decimal.Round(request.DefaultCreditLimit, 4, MidpointRounding.AwayFromZero),
                Description = NormalizeOptional(request.Description),
                IsActive = request.IsActive,
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            };

            dbContext.SalCustomerCategories.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var created = await dbContext.SalCustomerCategories
                .AsNoTracking()
                .Include(x => x.DefaultPriceList)
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
    public async Task<IActionResult> Update(int id, [FromBody] SalesCustomerCategoryDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.SalCustomerCategories.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            var normalizedCode = NormalizeRequired(request.Code, "Category code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Category name is required.");
            var defaultPriceListId = request.DefaultPriceListId is > 0 ? request.DefaultPriceListId : null;

            var validation = await ValidateRequestAsync(normalizedCode, defaultPriceListId, id, request.DefaultPaymentTerms, request.DefaultCreditLimit, ct);
            if (validation is not null)
            {
                return validation;
            }

            entity.Code = normalizedCode;
            entity.Name = normalizedName;
            entity.DefaultPriceListId = defaultPriceListId;
            entity.DefaultPaymentTerms = request.DefaultPaymentTerms;
            entity.DefaultCreditLimit = decimal.Round(request.DefaultCreditLimit, 4, MidpointRounding.AwayFromZero);
            entity.Description = NormalizeOptional(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(ct);

            var updated = await dbContext.SalCustomerCategories
                .AsNoTracking()
                .Include(x => x.DefaultPriceList)
                .Where(x => x.Id == id)
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
        var entity = await dbContext.SalCustomerCategories.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        var usedByCustomer = await dbContext.FinCustomers.AnyAsync(x => x.CustomerCategoryId == id, ct);
        if (usedByCustomer)
        {
            return BadRequest(new { message = "Category is used by customer." });
        }

        dbContext.SalCustomerCategories.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task<IActionResult?> ValidateRequestAsync(string code, int? defaultPriceListId, int? id, int defaultPaymentTerms, decimal defaultCreditLimit, CancellationToken ct)
    {
        if (defaultPaymentTerms < 0)
        {
            return BadRequest(new { message = "Default payment terms cannot be negative." });
        }

        if (defaultCreditLimit < 0)
        {
            return BadRequest(new { message = "Default credit limit cannot be negative." });
        }

        if (defaultPriceListId.HasValue)
        {
            var exists = await dbContext.SalPriceLists.AnyAsync(x => x.Id == defaultPriceListId.Value, ct);
            if (!exists)
            {
                return BadRequest(new { message = "Default price list not found." });
            }
        }

        var duplicate = await dbContext.SalCustomerCategories
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id != (id ?? 0) && x.Code == code, ct);

        if (duplicate)
        {
            return BadRequest(new { message = "Category code already exists." });
        }

        return null;
    }

    private static SalesCustomerCategoryDto MapDto(SalCustomerCategory entity)
    {
        return new SalesCustomerCategoryDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            DefaultPriceListId = entity.DefaultPriceListId,
            DefaultPriceListCode = entity.DefaultPriceList != null ? entity.DefaultPriceList.Code : null,
            DefaultPriceListName = entity.DefaultPriceList != null ? entity.DefaultPriceList.Name : null,
            DefaultPaymentTerms = entity.DefaultPaymentTerms,
            DefaultCreditLimit = entity.DefaultCreditLimit,
            Description = entity.Description,
            IsActive = entity.IsActive
        };
    }

    private static string NormalizeRequired(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(message);
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
