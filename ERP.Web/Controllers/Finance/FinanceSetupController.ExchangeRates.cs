using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Web.ViewModels.Finance;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class FinanceSetupController
{
    [HttpGet("exchange-rates")]
    public async Task<IActionResult> ExchangeRates(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "effectiveDate",
        string? sortDirection = "desc",
        string? fromCurrencyCode = null,
        string? toCurrencyCode = null,
        DateOnly? effectiveDateFrom = null,
        DateOnly? effectiveDateTo = null,
        string? source = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "effectivedate", "fromcurrencycode", "tocurrencycode", "rate", "effectivedate", "createdat");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedFromCurrencyCode = NormalizeText(fromCurrencyCode)?.ToUpperInvariant();
        var normalizedToCurrencyCode = NormalizeText(toCurrencyCode)?.ToUpperInvariant();
        var normalizedSource = NormalizeText(source);
        var (normalizedEffectiveDateFrom, normalizedEffectiveDateTo) = NormalizeDateRange(effectiveDateFrom, effectiveDateTo);

        var itemsTask = financeApiClient.GetExchangeRatesAsync(accessToken, new ExchangeRatePagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            FromCurrencyCode = normalizedFromCurrencyCode,
            ToCurrencyCode = normalizedToCurrencyCode,
            EffectiveDateFrom = normalizedEffectiveDateFrom,
            EffectiveDateTo = normalizedEffectiveDateTo,
            Source = normalizedSource
        }, ct);

        var currencyOptionsTask = LoadCurrencyOptionsAsync(accessToken, ct);
        await Task.WhenAll(itemsTask, currencyOptionsTask);

        ViewData["Title"] = "Exchange Rates";
        ViewData["Breadcrumb"] = "Finance / Exchange Rates";

        return View("ExchangeRates/Index", new FinanceExchangeRatesIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            FromCurrencyCodeFilter = normalizedFromCurrencyCode,
            ToCurrencyCodeFilter = normalizedToCurrencyCode,
            EffectiveDateFromFilter = normalizedEffectiveDateFrom,
            EffectiveDateToFilter = normalizedEffectiveDateTo,
            SourceFilter = normalizedSource,
            CurrencyOptions = await currencyOptionsTask,
            Items = await itemsTask ?? PagedResult<ExchangeRateDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("exchange-rates/create")]
    public async Task<IActionResult> CreateExchangeRate(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new FinanceExchangeRateCreateViewModel();
        await PopulateExchangeRateFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create Exchange Rate";
        ViewData["Breadcrumb"] = "Finance / Exchange Rates / Create";

        return View("ExchangeRates/Create", model);
    }

    [HttpPost("exchange-rates/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateExchangeRate(FinanceExchangeRateCreateViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        await PopulateExchangeRateFormOptionsAsync(accessToken, model, ct);

        if (string.Equals(model.FromCurrencyCode?.Trim(), model.ToCurrencyCode?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(model.ToCurrencyCode), "From and to currency cannot be the same.");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Exchange Rate";
            ViewData["Breadcrumb"] = "Finance / Exchange Rates / Create";
            return View("ExchangeRates/Create", model);
        }

        var created = await financeApiClient.CreateExchangeRateAsync(accessToken, new ExchangeRateDto
        {
            FromCurrencyCode = model.FromCurrencyCode ?? string.Empty,
            ToCurrencyCode = model.ToCurrencyCode ?? string.Empty,
            Rate = model.Rate,
            EffectiveDate = model.EffectiveDate,
            Source = model.Source
        }, ct);

        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to create exchange rate." : created.ErrorMessage);
            ViewData["Title"] = "Create Exchange Rate";
            ViewData["Breadcrumb"] = "Finance / Exchange Rates / Create";
            return View("ExchangeRates/Create", model);
        }

        TempData["SuccessMessage"] = "Exchange rate created.";
        return RedirectToAction(nameof(ExchangeRates));
    }
}

