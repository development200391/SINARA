using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Manufacturing;
using ERP.Domain.Enums.Manufacturing;
using ERP.Web.ViewModels.Manufacturing;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class ManufacturingController
{
    [HttpGet("reports/production-output")]
    public async Task<IActionResult> ProductionOutputReport(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "workordercode",
        string? sortDirection = "asc",
        WorkOrderStatus? status = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "workordercode", "workordercode", "itemcode", "qtyplanned", "qtygood", "qtyscrap", "completionratepct");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedPlannedStartFrom, normalizedPlannedStartTo) = NormalizeDateRange(plannedStartFrom, plannedStartTo);

        var items = await manufacturingApiClient.GetProductionOutputReportAsync(accessToken, new ManufacturingProductionOutputReportRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Status = status,
            PlannedStartFrom = normalizedPlannedStartFrom,
            PlannedStartTo = normalizedPlannedStartTo
        }, ct);

        ViewData["Title"] = "Production Output";
        ViewData["Breadcrumb"] = "Manufacturing / Reports / Production Output";

        return View("Reports/ProductionOutput", new ManufacturingProductionOutputReportViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            StatusFilter = status,
            PlannedStartFromFilter = normalizedPlannedStartFrom,
            PlannedStartToFilter = normalizedPlannedStartTo,
            Items = items ?? PagedResult<ManufacturingProductionOutputReportDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("reports/oee")]
    public async Task<IActionResult> OeeReport(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "snapshotdate",
        string? sortDirection = "desc",
        DateOnly? snapshotDateFrom = null,
        DateOnly? snapshotDateTo = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "snapshotdate", "workcentercode", "snapshotdate", "availabilitypct", "performancepct", "qualitypct", "oeepct");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedSnapshotFrom, normalizedSnapshotTo) = NormalizeDateRange(snapshotDateFrom, snapshotDateTo);

        var items = await manufacturingApiClient.GetOeeReportAsync(accessToken, new ManufacturingOeeReportRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            SnapshotDateFrom = normalizedSnapshotFrom,
            SnapshotDateTo = normalizedSnapshotTo
        }, ct);

        ViewData["Title"] = "OEE Report";
        ViewData["Breadcrumb"] = "Manufacturing / Reports / OEE";

        return View("Reports/Oee", new ManufacturingOeeReportViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            SnapshotDateFromFilter = normalizedSnapshotFrom,
            SnapshotDateToFilter = normalizedSnapshotTo,
            Items = items ?? PagedResult<ManufacturingOeeReportDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("reports/cost-variance")]
    public async Task<IActionResult> CostVarianceReport(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "varianceamount",
        string? sortDirection = "desc",
        WorkOrderStatus? status = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "varianceamount", "workordercode", "standardcosttotal", "actualcosttotal", "varianceamount", "variancepct");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);

        var items = await manufacturingApiClient.GetCostVarianceReportAsync(accessToken, new ManufacturingCostVarianceReportRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Status = status
        }, ct);

        ViewData["Title"] = "Cost Variance";
        ViewData["Breadcrumb"] = "Manufacturing / Reports / Cost Variance";

        return View("Reports/CostVariance", new ManufacturingCostVarianceReportViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            StatusFilter = status,
            Items = items ?? PagedResult<ManufacturingCostVarianceReportDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }
}
