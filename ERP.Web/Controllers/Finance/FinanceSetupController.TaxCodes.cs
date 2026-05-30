using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Enums;
using ERP.Web.ViewModels.Finance;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class FinanceSetupController
{
    [HttpGet("tax-codes")]
    public async Task<IActionResult> TaxCodes(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "code",
        string? sortDirection = "asc",
        string? code = null,
        string? name = null,
        FinanceTaxType? type = null,
        decimal? rateFrom = null,
        decimal? rateTo = null,
        bool? isInclusive = null,
        int? accountId = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "code", "code", "name", "type", "rate", "isinclusive", "accountcode", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedCode = NormalizeText(code);
        var normalizedName = NormalizeText(name);
        var (normalizedRateFrom, normalizedRateTo) = NormalizeDecimalRange(rateFrom, rateTo);

        var itemsTask = financeApiClient.GetTaxCodesAsync(accessToken, new TaxCodePagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = normalizedCode,
            Name = normalizedName,
            Type = type,
            RateFrom = normalizedRateFrom,
            RateTo = normalizedRateTo,
            IsInclusive = isInclusive,
            AccountId = accountId,
            IsActive = isActive
        }, ct);

        var accountOptionsTask = LoadAccountOptionsAsync(accessToken, ct);
        await Task.WhenAll(itemsTask, accountOptionsTask);

        ViewData["Title"] = "Tax Codes";
        ViewData["Breadcrumb"] = "Finance / Tax Codes";

        return View("TaxCodes/Index", new FinanceTaxCodesIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = normalizedCode,
            NameFilter = normalizedName,
            TypeFilter = type,
            RateFromFilter = normalizedRateFrom,
            RateToFilter = normalizedRateTo,
            IsInclusiveFilter = isInclusive,
            AccountIdFilter = accountId,
            IsActiveFilter = isActive,
            AccountOptions = await accountOptionsTask,
            Items = await itemsTask ?? PagedResult<TaxCodeDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("tax-codes/create")]
    public async Task<IActionResult> CreateTaxCode(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new FinanceTaxCodeEditViewModel();
        await PopulateTaxCodeFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create Tax Code";
        ViewData["Breadcrumb"] = "Finance / Tax Codes / Create";

        return View("TaxCodes/Create", model);
    }

    [HttpPost("tax-codes/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTaxCode(FinanceTaxCodeEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        await PopulateTaxCodeFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Tax Code";
            ViewData["Breadcrumb"] = "Finance / Tax Codes / Create";
            return View("TaxCodes/Create", model);
        }

        var created = await financeApiClient.CreateTaxCodeAsync(accessToken, new TaxCodeDto
        {
            Code = model.Code,
            Name = model.Name,
            Type = model.Type,
            Rate = model.Rate,
            IsInclusive = model.IsInclusive,
            AccountId = model.AccountId,
            IsActive = model.IsActive
        }, ct);

        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create tax code.");
            ViewData["Title"] = "Create Tax Code";
            ViewData["Breadcrumb"] = "Finance / Tax Codes / Create";
            return View("TaxCodes/Create", model);
        }

        TempData["SuccessMessage"] = "Tax code created.";
        return RedirectToAction(nameof(TaxCodes));
    }

    [HttpGet("tax-codes/edit/{id:int}")]
    public async Task<IActionResult> EditTaxCode(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await financeApiClient.GetTaxCodeByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        var model = new FinanceTaxCodeEditViewModel
        {
            Id = item.Id,
            Code = item.Code,
            Name = item.Name,
            Type = item.Type,
            Rate = item.Rate,
            IsInclusive = item.IsInclusive,
            AccountId = item.AccountId,
            IsActive = item.IsActive
        };

        await PopulateTaxCodeFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit Tax Code";
        ViewData["Breadcrumb"] = "Finance / Tax Codes / Edit";

        return View("TaxCodes/Edit", model);
    }

    [HttpPost("tax-codes/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTaxCode(int id, FinanceTaxCodeEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        await PopulateTaxCodeFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Tax Code";
            ViewData["Breadcrumb"] = "Finance / Tax Codes / Edit";
            return View("TaxCodes/Edit", model);
        }

        var updated = await financeApiClient.UpdateTaxCodeAsync(accessToken, id, new TaxCodeDto
        {
            Id = id,
            Code = model.Code,
            Name = model.Name,
            Type = model.Type,
            Rate = model.Rate,
            IsInclusive = model.IsInclusive,
            AccountId = model.AccountId,
            IsActive = model.IsActive
        }, ct);

        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to update tax code.");
            ViewData["Title"] = "Edit Tax Code";
            ViewData["Breadcrumb"] = "Finance / Tax Codes / Edit";
            return View("TaxCodes/Edit", model);
        }

        TempData["SuccessMessage"] = "Tax code updated.";
        return RedirectToAction(nameof(TaxCodes));
    }

    [HttpPost("tax-codes/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTaxCode(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await financeApiClient.DeleteTaxCodeAsync(accessToken, id, ct);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Tax code deleted." : "Failed to delete tax code.";

        return RedirectToAction(nameof(TaxCodes));
    }
}
