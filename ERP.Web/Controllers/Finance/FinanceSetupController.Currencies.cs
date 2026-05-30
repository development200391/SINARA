using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Web.ViewModels.Finance;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class FinanceSetupController
{
    [HttpGet("currencies")]
    public async Task<IActionResult> Currencies(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "code",
        string? sortDirection = "asc",
        string? code = null,
        string? name = null,
        string? symbol = null,
        bool? isBaseCurrency = null,
        bool? isActive = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "code", "code", "name", "symbol", "isbasecurrency", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedCode = NormalizeText(code);
        var normalizedName = NormalizeText(name);
        var normalizedSymbol = NormalizeText(symbol);

        var items = await financeApiClient.GetCurrenciesAsync(accessToken, new CurrencyPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = normalizedCode,
            Name = normalizedName,
            Symbol = normalizedSymbol,
            IsBaseCurrency = isBaseCurrency,
            IsActive = isActive
        }, ct);

        ViewData["Title"] = "Currencies";
        ViewData["Breadcrumb"] = "Finance / Currencies";

        return View("Currencies/Index", new FinanceCurrenciesIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = normalizedCode,
            NameFilter = normalizedName,
            SymbolFilter = normalizedSymbol,
            IsBaseCurrencyFilter = isBaseCurrency,
            IsActiveFilter = isActive,
            Items = items ?? PagedResult<CurrencyDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("currencies/create")]
    public IActionResult CreateCurrency()
    {
        var unauthorized = RequireAccessToken(out _, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        ViewData["Title"] = "Create Currency";
        ViewData["Breadcrumb"] = "Finance / Currencies / Create";

        return View("Currencies/Create", new FinanceCurrencyEditViewModel());
    }

    [HttpPost("currencies/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCurrency(FinanceCurrencyEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Currency";
            ViewData["Breadcrumb"] = "Finance / Currencies / Create";
            return View("Currencies/Create", model);
        }

        var created = await financeApiClient.CreateCurrencyAsync(accessToken, new CurrencyDto
        {
            Code = model.Code,
            Name = model.Name,
            Symbol = model.Symbol,
            IsBaseCurrency = model.IsBaseCurrency,
            IsActive = model.IsActive
        }, ct);

        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create currency.");
            ViewData["Title"] = "Create Currency";
            ViewData["Breadcrumb"] = "Finance / Currencies / Create";
            return View("Currencies/Create", model);
        }

        TempData["SuccessMessage"] = "Currency created.";
        return RedirectToAction(nameof(Currencies));
    }

    [HttpGet("currencies/edit/{id:int}")]
    public async Task<IActionResult> EditCurrency(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await financeApiClient.GetCurrencyByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        var model = new FinanceCurrencyEditViewModel
        {
            Id = item.Id,
            Code = item.Code,
            Name = item.Name,
            Symbol = item.Symbol,
            IsBaseCurrency = item.IsBaseCurrency,
            IsActive = item.IsActive
        };

        ViewData["Title"] = "Edit Currency";
        ViewData["Breadcrumb"] = "Finance / Currencies / Edit";

        return View("Currencies/Edit", model);
    }

    [HttpPost("currencies/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCurrency(int id, FinanceCurrencyEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Currency";
            ViewData["Breadcrumb"] = "Finance / Currencies / Edit";
            return View("Currencies/Edit", model);
        }

        var updated = await financeApiClient.UpdateCurrencyAsync(accessToken, id, new CurrencyDto
        {
            Id = id,
            Code = model.Code,
            Name = model.Name,
            Symbol = model.Symbol,
            IsBaseCurrency = model.IsBaseCurrency,
            IsActive = model.IsActive
        }, ct);

        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to update currency.");
            ViewData["Title"] = "Edit Currency";
            ViewData["Breadcrumb"] = "Finance / Currencies / Edit";
            return View("Currencies/Edit", model);
        }

        TempData["SuccessMessage"] = "Currency updated.";
        return RedirectToAction(nameof(Currencies));
    }

    [HttpPost("currencies/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCurrency(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await financeApiClient.DeleteCurrencyAsync(accessToken, id, ct);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Currency deleted." : "Failed to delete currency.";

        return RedirectToAction(nameof(Currencies));
    }
}
