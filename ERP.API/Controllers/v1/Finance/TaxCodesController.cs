using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Entities.Finance;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Finance;

[Route("api/v1/finance/tax-codes")]
public sealed class TaxCodesController(AppDbContext dbContext) : FinanceControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] TaxCodePagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.FinTaxCodes
            .AsNoTracking()
            .Include(x => x.Account)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(search) ||
                x.Name.ToLower().Contains(search) ||
                x.Account.Code.ToLower().Contains(search) ||
                x.Account.Name.ToLower().Contains(search));
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

        if (request.Type.HasValue)
        {
            query = query.Where(x => x.Type == request.Type.Value);
        }

        if (request.RateFrom.HasValue)
        {
            query = query.Where(x => x.Rate >= request.RateFrom.Value);
        }

        if (request.RateTo.HasValue)
        {
            query = query.Where(x => x.Rate <= request.RateTo.Value);
        }

        if (request.IsInclusive.HasValue)
        {
            query = query.Where(x => x.IsInclusive == request.IsInclusive.Value);
        }

        if (request.AccountId.HasValue)
        {
            query = query.Where(x => x.AccountId == request.AccountId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "name" => isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "type" => isDesc ? query.OrderByDescending(x => x.Type) : query.OrderBy(x => x.Type),
            "rate" => isDesc ? query.OrderByDescending(x => x.Rate) : query.OrderBy(x => x.Rate),
            "isinclusive" => isDesc ? query.OrderByDescending(x => x.IsInclusive).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsInclusive).ThenBy(x => x.Code),
            "accountcode" => isDesc ? query.OrderByDescending(x => x.Account.Code).ThenByDescending(x => x.Code) : query.OrderBy(x => x.Account.Code).ThenBy(x => x.Code),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Code),
            _ => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapDto(x))
            .ToListAsync(ct);

        return Ok(PagedResult<TaxCodeDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.FinTaxCodes
            .AsNoTracking()
            .Include(x => x.Account)
            .Where(x => x.Id == id)
            .Select(x => MapDto(x))
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TaxCodeDto request, CancellationToken ct)
    {
        try
        {
            var normalizedCode = NormalizeRequired(request.Code, "Tax code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Tax name is required.");

            if (request.AccountId <= 0)
            {
                return BadRequest(new { message = "Account is required." });
            }

            if (request.Rate < 0 || request.Rate > 100)
            {
                return BadRequest(new { message = "Tax rate must be between 0 and 100." });
            }

            var accountExists = await dbContext.FinAccounts.AnyAsync(x => x.Id == request.AccountId, ct);
            if (!accountExists)
            {
                return BadRequest(new { message = "Account not found." });
            }

            var duplicate = await dbContext.FinTaxCodes
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Code == normalizedCode, ct);
            if (duplicate)
            {
                return BadRequest(new { message = "Tax code already exists." });
            }

            var entity = new FinTaxCode
            {
                Code = normalizedCode,
                Name = normalizedName,
                Type = request.Type,
                Rate = request.Rate,
                IsInclusive = request.IsInclusive,
                AccountId = request.AccountId,
                IsActive = request.IsActive,
                CreatedBy = "system"
            };

            dbContext.FinTaxCodes.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var result = await dbContext.FinTaxCodes
                .AsNoTracking()
                .Include(x => x.Account)
                .Where(x => x.Id == entity.Id)
                .Select(x => MapDto(x))
                .FirstAsync(ct);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TaxCodeDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.FinTaxCodes.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            var normalizedCode = NormalizeRequired(request.Code, "Tax code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Tax name is required.");

            if (request.AccountId <= 0)
            {
                return BadRequest(new { message = "Account is required." });
            }

            if (request.Rate < 0 || request.Rate > 100)
            {
                return BadRequest(new { message = "Tax rate must be between 0 and 100." });
            }

            var accountExists = await dbContext.FinAccounts.AnyAsync(x => x.Id == request.AccountId, ct);
            if (!accountExists)
            {
                return BadRequest(new { message = "Account not found." });
            }

            var duplicate = await dbContext.FinTaxCodes
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Id != id && x.Code == normalizedCode, ct);
            if (duplicate)
            {
                return BadRequest(new { message = "Tax code already exists." });
            }

            entity.Code = normalizedCode;
            entity.Name = normalizedName;
            entity.Type = request.Type;
            entity.Rate = request.Rate;
            entity.IsInclusive = request.IsInclusive;
            entity.AccountId = request.AccountId;
            entity.IsActive = request.IsActive;
            entity.UpdatedBy = "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(ct);

            var result = await dbContext.FinTaxCodes
                .AsNoTracking()
                .Include(x => x.Account)
                .Where(x => x.Id == entity.Id)
                .Select(x => MapDto(x))
                .FirstAsync(ct);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var entity = await dbContext.FinTaxCodes.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        dbContext.FinTaxCodes.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private static TaxCodeDto MapDto(FinTaxCode entity)
    {
        return new TaxCodeDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Type = entity.Type,
            Rate = entity.Rate,
            IsInclusive = entity.IsInclusive,
            AccountId = entity.AccountId,
            AccountCode = entity.Account?.Code ?? string.Empty,
            AccountName = entity.Account?.Name ?? string.Empty,
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
