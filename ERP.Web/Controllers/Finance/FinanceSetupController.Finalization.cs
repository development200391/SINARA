using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Enums;
using ERP.Web.ViewModels.Finance;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class FinanceSetupController
{
    private static readonly string[] SmokeTestCategories = ["Master Data", "Transactions", "Reports"];

    [HttpGet("finalization/period-closing")]
    public async Task<IActionResult> PeriodClosing(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "startdate",
        string? sortDirection = "asc",
        int? fiscalYearId = null,
        FinancePeriodStatus? status = null,
        int? draftJournalFrom = null,
        int? draftJournalTo = null,
        int? pendingApFrom = null,
        int? pendingApTo = null,
        int? pendingArFrom = null,
        int? pendingArTo = null,
        decimal? netIncomeLossFrom = null,
        decimal? netIncomeLossTo = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "startdate", "fiscalyearname", "periodnumber", "periodname", "startdate", "enddate", "status", "draftjournalcount", "postedjournalcount", "pendingapinvoicecount", "pendingarinvoicecount", "netincomelossamount");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedDraftFrom, normalizedDraftTo) = NormalizeIntRange(draftJournalFrom, draftJournalTo);
        var (normalizedPendingApFrom, normalizedPendingApTo) = NormalizeIntRange(pendingApFrom, pendingApTo);
        var (normalizedPendingArFrom, normalizedPendingArTo) = NormalizeIntRange(pendingArFrom, pendingArTo);
        var (normalizedNetIncomeLossFrom, normalizedNetIncomeLossTo) = NormalizeDecimalRange(netIncomeLossFrom, netIncomeLossTo);

        var itemsTask = financeApiClient.GetPeriodClosingAsync(accessToken, new PeriodClosingPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            FiscalYearId = fiscalYearId,
            Status = status,
            DraftJournalFrom = normalizedDraftFrom,
            DraftJournalTo = normalizedDraftTo,
            PendingApFrom = normalizedPendingApFrom,
            PendingApTo = normalizedPendingApTo,
            PendingArFrom = normalizedPendingArFrom,
            PendingArTo = normalizedPendingArTo,
            NetIncomeLossFrom = normalizedNetIncomeLossFrom,
            NetIncomeLossTo = normalizedNetIncomeLossTo
        }, ct);

        var fiscalYearOptionsTask = LoadFiscalYearOptionsAsync(accessToken, ct);
        await Task.WhenAll(itemsTask, fiscalYearOptionsTask);

        ViewData["Title"] = "Period Closing";
        ViewData["Breadcrumb"] = "Finance / Finance Finalization / Period Closing";

        return View("Finalization/PeriodClosing", new FinancePeriodClosingIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            FiscalYearIdFilter = fiscalYearId,
            StatusFilter = status,
            DraftJournalFromFilter = normalizedDraftFrom,
            DraftJournalToFilter = normalizedDraftTo,
            PendingApFromFilter = normalizedPendingApFrom,
            PendingApToFilter = normalizedPendingApTo,
            PendingArFromFilter = normalizedPendingArFrom,
            PendingArToFilter = normalizedPendingArTo,
            NetIncomeLossFromFilter = normalizedNetIncomeLossFrom,
            NetIncomeLossToFilter = normalizedNetIncomeLossTo,
            FiscalYearOptions = await fiscalYearOptionsTask,
            Items = await itemsTask ?? PagedResult<PeriodClosingRowDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("finalization/smoke-tests")]
    public async Task<IActionResult> SmokeTests(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "sortorder",
        string? sortDirection = "asc",
        string? category = null,
        bool? passed = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "sortorder", "sortorder", "category", "checkitem", "expectedvalue", "actualvalue", "passed");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedCategory = NormalizeText(category);

        if (!string.IsNullOrWhiteSpace(normalizedCategory) && !SmokeTestCategories.Contains(normalizedCategory, StringComparer.OrdinalIgnoreCase))
        {
            normalizedCategory = null;
        }

        var items = await financeApiClient.GetSmokeTestsAsync(accessToken, new SmokeTestPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Category = normalizedCategory,
            Passed = passed
        }, ct);

        ViewData["Title"] = "Smoke Tests";
        ViewData["Breadcrumb"] = "Finance / Finance Finalization / Smoke Tests";

        return View("Finalization/SmokeTests", new FinanceSmokeTestsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CategoryFilter = normalizedCategory,
            PassedFilter = passed,
            CategoryOptions = SmokeTestCategories,
            Items = items ?? PagedResult<SmokeTestRowDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }
}