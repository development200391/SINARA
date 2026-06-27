using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Manufacturing;
using ERP.Domain.Enums.Manufacturing;
using ERP.Web.ViewModels.Manufacturing;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class ManufacturingController
{
    [HttpGet("reports/scrap-analysis")]
    public async Task<IActionResult> ScrapAnalysisReport(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "totalscrapcost",
        string? sortDirection = "desc",
        ScrapReason? reason = null,
        DateTimeOffset? recordedFrom = null,
        DateTimeOffset? recordedTo = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "totalscrapcost", "reason", "totalqtyscrap", "totalscrapcost");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedRecordedFrom, normalizedRecordedTo) = NormalizeDateTimeOffsetRange(recordedFrom, recordedTo);

        var items = await manufacturingApiClient.GetScrapAnalysisReportAsync(accessToken, new ManufacturingScrapAnalysisReportRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Reason = reason,
            RecordedFrom = normalizedRecordedFrom,
            RecordedTo = normalizedRecordedTo
        }, ct);

        ViewData["Title"] = "Scrap Analysis";
        ViewData["Breadcrumb"] = "Manufacturing / Reports / Scrap Analysis";

        return View("Reports/ScrapAnalysis", new ManufacturingScrapAnalysisReportViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            ReasonFilter = reason,
            RecordedFromFilter = normalizedRecordedFrom,
            RecordedToFilter = normalizedRecordedTo,
            Items = items ?? PagedResult<ManufacturingScrapAnalysisReportDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("reports/capacity")]
    public async Task<IActionResult> CapacityReport(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "utilizationpct",
        string? sortDirection = "desc",
        DateOnly? plannedStartFrom = null,
        DateOnly? plannedStartTo = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "utilizationpct", "workcentercode", "capacityhoursperday", "plannedqtytotal", "goodqtytotal", "utilizationpct");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedPlannedStartFrom, normalizedPlannedStartTo) = NormalizeDateRange(plannedStartFrom, plannedStartTo);

        var items = await manufacturingApiClient.GetCapacityReportAsync(accessToken, new ManufacturingCapacityReportRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            PlannedStartFrom = normalizedPlannedStartFrom,
            PlannedStartTo = normalizedPlannedStartTo
        }, ct);

        ViewData["Title"] = "Capacity";
        ViewData["Breadcrumb"] = "Manufacturing / Reports / Capacity";

        return View("Reports/Capacity", new ManufacturingCapacityReportViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            PlannedStartFromFilter = normalizedPlannedStartFrom,
            PlannedStartToFilter = normalizedPlannedStartTo,
            Items = items ?? PagedResult<ManufacturingCapacityReportDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }
}
