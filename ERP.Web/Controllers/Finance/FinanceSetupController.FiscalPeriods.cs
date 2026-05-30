using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Enums;
using ERP.Web.ViewModels.Finance;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class FinanceSetupController
{
    [HttpGet("fiscal-years")]
    public async Task<IActionResult> FiscalYears(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "startDate",
        string? sortDirection = "desc",
        string? name = null,
        DateOnly? startDateFrom = null,
        DateOnly? startDateTo = null,
        DateOnly? endDateFrom = null,
        DateOnly? endDateTo = null,
        FinancePeriodStatus? status = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "startdate", "name", "startdate", "enddate", "status");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedName = NormalizeText(name);
        var (normalizedStartDateFrom, normalizedStartDateTo) = NormalizeDateRange(startDateFrom, startDateTo);
        var (normalizedEndDateFrom, normalizedEndDateTo) = NormalizeDateRange(endDateFrom, endDateTo);

        var items = await financeApiClient.GetFiscalYearsAsync(accessToken, new FiscalYearPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Name = normalizedName,
            StartDateFrom = normalizedStartDateFrom,
            StartDateTo = normalizedStartDateTo,
            EndDateFrom = normalizedEndDateFrom,
            EndDateTo = normalizedEndDateTo,
            Status = status
        }, ct);

        ViewData["Title"] = "Fiscal Years";
        ViewData["Breadcrumb"] = "Finance / Fiscal Years";

        return View("FiscalYears/Index", new FinanceFiscalYearsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            NameFilter = normalizedName,
            StartDateFromFilter = normalizedStartDateFrom,
            StartDateToFilter = normalizedStartDateTo,
            EndDateFromFilter = normalizedEndDateFrom,
            EndDateToFilter = normalizedEndDateTo,
            StatusFilter = status,
            Items = items ?? PagedResult<FiscalYearDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("fiscal-years/create")]
    public IActionResult CreateFiscalYear()
    {
        var unauthorized = RequireAccessToken(out _, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        ViewData["Title"] = "Create Fiscal Year";
        ViewData["Breadcrumb"] = "Finance / Fiscal Years / Create";

        return View("FiscalYears/Create", new FinanceFiscalYearEditViewModel());
    }

    [HttpPost("fiscal-years/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFiscalYear(FinanceFiscalYearEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        if (model.StartDate > model.EndDate)
        {
            ModelState.AddModelError(nameof(model.EndDate), "End date must be after start date.");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Fiscal Year";
            ViewData["Breadcrumb"] = "Finance / Fiscal Years / Create";
            return View("FiscalYears/Create", model);
        }

        var created = await financeApiClient.CreateFiscalYearAsync(accessToken, new FiscalYearDto
        {
            Name = model.Name,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            Status = model.Status
        }, ct);

        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create fiscal year.");
            ViewData["Title"] = "Create Fiscal Year";
            ViewData["Breadcrumb"] = "Finance / Fiscal Years / Create";
            return View("FiscalYears/Create", model);
        }

        TempData["SuccessMessage"] = "Fiscal year created.";
        return RedirectToAction(nameof(FiscalYears));
    }

    [HttpGet("fiscal-years/edit/{id:int}")]
    public async Task<IActionResult> EditFiscalYear(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await financeApiClient.GetFiscalYearByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        var model = new FinanceFiscalYearEditViewModel
        {
            Id = item.Id,
            Name = item.Name,
            StartDate = item.StartDate,
            EndDate = item.EndDate,
            Status = item.Status
        };

        ViewData["Title"] = "Edit Fiscal Year";
        ViewData["Breadcrumb"] = "Finance / Fiscal Years / Edit";

        return View("FiscalYears/Edit", model);
    }

    [HttpPost("fiscal-years/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditFiscalYear(int id, FinanceFiscalYearEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;

        if (model.StartDate > model.EndDate)
        {
            ModelState.AddModelError(nameof(model.EndDate), "End date must be after start date.");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Fiscal Year";
            ViewData["Breadcrumb"] = "Finance / Fiscal Years / Edit";
            return View("FiscalYears/Edit", model);
        }

        var updated = await financeApiClient.UpdateFiscalYearAsync(accessToken, id, new FiscalYearDto
        {
            Id = id,
            Name = model.Name,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            Status = model.Status
        }, ct);

        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to update fiscal year.");
            ViewData["Title"] = "Edit Fiscal Year";
            ViewData["Breadcrumb"] = "Finance / Fiscal Years / Edit";
            return View("FiscalYears/Edit", model);
        }

        TempData["SuccessMessage"] = "Fiscal year updated.";
        return RedirectToAction(nameof(FiscalYears));
    }

    [HttpPost("fiscal-years/{id:int}/close")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CloseFiscalYear(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await financeApiClient.CloseFiscalYearAsync(accessToken, id, ct);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Fiscal year closed." : "Failed to close fiscal year.";

        return RedirectToAction(nameof(FiscalYears));
    }

    [HttpGet("periods")]
    public async Task<IActionResult> Periods(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "startDate",
        string? sortDirection = "desc",
        int? fiscalYearId = null,
        int? periodNumberFrom = null,
        int? periodNumberTo = null,
        FinancePeriodStatus? status = null,
        DateOnly? startDateFrom = null,
        DateOnly? startDateTo = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "startdate", "fiscalyearname", "periodnumber", "name", "startdate", "enddate", "status");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedPeriodFrom, normalizedPeriodTo) = NormalizeIntRange(periodNumberFrom, periodNumberTo);
        var (normalizedStartDateFrom, normalizedStartDateTo) = NormalizeDateRange(startDateFrom, startDateTo);

        var itemsTask = financeApiClient.GetPeriodsAsync(accessToken, new PeriodPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            FiscalYearId = fiscalYearId,
            PeriodNumberFrom = normalizedPeriodFrom,
            PeriodNumberTo = normalizedPeriodTo,
            Status = status,
            StartDateFrom = normalizedStartDateFrom,
            StartDateTo = normalizedStartDateTo
        }, ct);

        var fiscalYearOptionsTask = LoadFiscalYearOptionsAsync(accessToken, ct);
        await Task.WhenAll(itemsTask, fiscalYearOptionsTask);

        ViewData["Title"] = "Accounting Periods";
        ViewData["Breadcrumb"] = "Finance / Periods";

        return View("Periods/Index", new FinancePeriodsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            FiscalYearIdFilter = fiscalYearId,
            PeriodNumberFromFilter = normalizedPeriodFrom,
            PeriodNumberToFilter = normalizedPeriodTo,
            StatusFilter = status,
            StartDateFromFilter = normalizedStartDateFrom,
            StartDateToFilter = normalizedStartDateTo,
            FiscalYearOptions = await fiscalYearOptionsTask,
            Items = await itemsTask ?? PagedResult<PeriodDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpPost("periods/{id:int}/close")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClosePeriod(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await financeApiClient.ClosePeriodAsync(accessToken, id, ct);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Period closed." : "Failed to close period.";

        return RedirectToAction(nameof(Periods));
    }
}
