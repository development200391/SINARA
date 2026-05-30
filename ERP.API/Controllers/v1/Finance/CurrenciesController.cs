using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Entities.Finance;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Finance;

[Route("api/v1/finance/currencies")]
public sealed class CurrenciesController(AppDbContext dbContext) : FinanceControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] CurrencyPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.FinCurrencies.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(search) ||
                x.Name.ToLower().Contains(search) ||
                x.Symbol.ToLower().Contains(search));
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

        if (!string.IsNullOrWhiteSpace(request.Symbol))
        {
            var symbol = request.Symbol.Trim().ToLowerInvariant();
            query = query.Where(x => x.Symbol.ToLower().Contains(symbol));
        }

        if (request.IsBaseCurrency.HasValue)
        {
            query = query.Where(x => x.IsBaseCurrency == request.IsBaseCurrency.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "name" => isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "symbol" => isDesc ? query.OrderByDescending(x => x.Symbol) : query.OrderBy(x => x.Symbol),
            "isbasecurrency" => isDesc ? query.OrderByDescending(x => x.IsBaseCurrency).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsBaseCurrency).ThenBy(x => x.Code),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Code),
            _ => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapDto(x))
            .ToListAsync(ct);

        return Ok(PagedResult<CurrencyDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.FinCurrencies
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => MapDto(x))
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CurrencyDto request, CancellationToken ct)
    {
        try
        {
            var normalizedCode = NormalizeRequired(request.Code, "Currency code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Currency name is required.");
            var normalizedSymbol = NormalizeRequired(request.Symbol, "Currency symbol is required.");

            var duplicate = await dbContext.FinCurrencies
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Code == normalizedCode, ct);
            if (duplicate)
            {
                return BadRequest(new { message = "Currency code already exists." });
            }

            if (request.IsBaseCurrency)
            {
                var baseCurrencies = await dbContext.FinCurrencies.Where(x => x.IsBaseCurrency).ToListAsync(ct);
                foreach (var baseCurrency in baseCurrencies)
                {
                    baseCurrency.IsBaseCurrency = false;
                }
            }

            var entity = new FinCurrency
            {
                Code = normalizedCode,
                Name = normalizedName,
                Symbol = normalizedSymbol,
                IsBaseCurrency = request.IsBaseCurrency,
                IsActive = request.IsActive,
                CreatedBy = "system"
            };

            dbContext.FinCurrencies.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            return Ok(MapDto(entity));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CurrencyDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.FinCurrencies.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            var normalizedCode = NormalizeRequired(request.Code, "Currency code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Currency name is required.");
            var normalizedSymbol = NormalizeRequired(request.Symbol, "Currency symbol is required.");

            var duplicate = await dbContext.FinCurrencies
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Id != id && x.Code == normalizedCode, ct);
            if (duplicate)
            {
                return BadRequest(new { message = "Currency code already exists." });
            }

            if (request.IsBaseCurrency)
            {
                var baseCurrencies = await dbContext.FinCurrencies.Where(x => x.IsBaseCurrency && x.Id != id).ToListAsync(ct);
                foreach (var baseCurrency in baseCurrencies)
                {
                    baseCurrency.IsBaseCurrency = false;
                }
            }

            entity.Code = normalizedCode;
            entity.Name = normalizedName;
            entity.Symbol = normalizedSymbol;
            entity.IsBaseCurrency = request.IsBaseCurrency;
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
        var entity = await dbContext.FinCurrencies.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        var usedByAccounts = await dbContext.FinAccounts.AnyAsync(x => x.CurrencyCode == entity.Code, ct);
        if (usedByAccounts)
        {
            return BadRequest(new { message = "Currency cannot be deleted because it is used by accounts." });
        }

        var usedByRates = await dbContext.FinExchangeRates.AnyAsync(x => x.FromCurrencyCode == entity.Code || x.ToCurrencyCode == entity.Code, ct);
        if (usedByRates)
        {
            return BadRequest(new { message = "Currency cannot be deleted because it is used by exchange rates." });
        }

        dbContext.FinCurrencies.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private static CurrencyDto MapDto(FinCurrency entity)
    {
        return new CurrencyDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Symbol = entity.Symbol,
            IsBaseCurrency = entity.IsBaseCurrency,
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
