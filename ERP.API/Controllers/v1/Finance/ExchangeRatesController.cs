using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Entities.Finance;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Finance;

[Route("api/v1/finance/exchange-rates")]
public sealed class ExchangeRatesController(AppDbContext dbContext) : FinanceControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ExchangeRatePagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.FinExchangeRates
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.FromCurrencyCode.ToLower().Contains(search) ||
                x.ToCurrencyCode.ToLower().Contains(search) ||
                (x.Source != null && x.Source.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.FromCurrencyCode))
        {
            var from = request.FromCurrencyCode.Trim().ToUpperInvariant();
            query = query.Where(x => x.FromCurrencyCode == from);
        }

        if (!string.IsNullOrWhiteSpace(request.ToCurrencyCode))
        {
            var to = request.ToCurrencyCode.Trim().ToUpperInvariant();
            query = query.Where(x => x.ToCurrencyCode == to);
        }

        if (request.EffectiveDateFrom.HasValue)
        {
            query = query.Where(x => x.EffectiveDate >= request.EffectiveDateFrom.Value);
        }

        if (request.EffectiveDateTo.HasValue)
        {
            query = query.Where(x => x.EffectiveDate <= request.EffectiveDateTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Source))
        {
            var source = request.Source.Trim().ToLowerInvariant();
            query = query.Where(x => x.Source != null && x.Source.ToLower().Contains(source));
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "fromcurrencycode" => isDesc ? query.OrderByDescending(x => x.FromCurrencyCode) : query.OrderBy(x => x.FromCurrencyCode),
            "tocurrencycode" => isDesc ? query.OrderByDescending(x => x.ToCurrencyCode) : query.OrderBy(x => x.ToCurrencyCode),
            "rate" => isDesc ? query.OrderByDescending(x => x.Rate) : query.OrderBy(x => x.Rate),
            "effectivedate" => isDesc ? query.OrderByDescending(x => x.EffectiveDate) : query.OrderBy(x => x.EffectiveDate),
            "createdat" => isDesc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            _ => isDesc ? query.OrderByDescending(x => x.EffectiveDate).ThenByDescending(x => x.Id) : query.OrderBy(x => x.EffectiveDate).ThenBy(x => x.Id)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapDto(x))
            .ToListAsync(ct);

        return Ok(PagedResult<ExchangeRateDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.FinExchangeRates
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => MapDto(x))
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ExchangeRateDto request, CancellationToken ct)
    {
        var fromCurrencyCode = NormalizeRequired(request.FromCurrencyCode, "From currency is required.").ToUpperInvariant();
        var toCurrencyCode = NormalizeRequired(request.ToCurrencyCode, "To currency is required.").ToUpperInvariant();

        if (fromCurrencyCode == toCurrencyCode)
        {
            return BadRequest(new { message = "From and to currency cannot be the same." });
        }

        if (request.Rate <= 0)
        {
            return BadRequest(new { message = "Rate must be greater than 0." });
        }

        var fromExists = await dbContext.FinCurrencies.AnyAsync(x => x.Code == fromCurrencyCode, ct);
        var toExists = await dbContext.FinCurrencies.AnyAsync(x => x.Code == toCurrencyCode, ct);
        if (!fromExists || !toExists)
        {
            return BadRequest(new { message = "Currency not found." });
        }

        var exists = await dbContext.FinExchangeRates.AnyAsync(x =>
            x.FromCurrencyCode == fromCurrencyCode &&
            x.ToCurrencyCode == toCurrencyCode &&
            x.EffectiveDate == request.EffectiveDate,
            ct);

        if (exists)
        {
            return BadRequest(new { message = "Exchange rate for this currency pair and date already exists." });
        }

        var entity = new FinExchangeRate
        {
            FromCurrencyCode = fromCurrencyCode,
            ToCurrencyCode = toCurrencyCode,
            Rate = request.Rate,
            EffectiveDate = request.EffectiveDate,
            Source = string.IsNullOrWhiteSpace(request.Source) ? null : request.Source.Trim(),
            CreatedBy = "system",
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.FinExchangeRates.Add(entity);
        await dbContext.SaveChangesAsync(ct);

        return Ok(MapDto(entity));
    }

    private static ExchangeRateDto MapDto(FinExchangeRate entity)
    {
        return new ExchangeRateDto
        {
            Id = entity.Id,
            FromCurrencyCode = entity.FromCurrencyCode,
            ToCurrencyCode = entity.ToCurrencyCode,
            Rate = entity.Rate,
            EffectiveDate = entity.EffectiveDate,
            Source = entity.Source,
            CreatedBy = entity.CreatedBy,
            CreatedAt = entity.CreatedAt
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
