using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Manufacturing;
using ERP.Domain.Enums.Manufacturing;
using ERP.Web.ViewModels.Manufacturing;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class ManufacturingController
{
    [HttpGet("work-orders")]
    public async Task<IActionResult> WorkOrders(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "code",
        string? sortDirection = "asc",
        string? code = null,
        WorkOrderStatus? status = null,
        ProductionType? productionType = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "code", "code", "status", "productiontype", "qtyplanned", "plannedstartdate", "workcentername", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedCode = NormalizeText(code);

        var items = await manufacturingApiClient.GetWorkOrdersAsync(accessToken, new ManufacturingWorkOrderPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = normalizedCode,
            Status = status,
            ProductionType = productionType,
            IsActive = isActive
        }, ct);

        ViewData["Title"] = "Work Orders";
        ViewData["Breadcrumb"] = "Manufacturing / Work Orders";

        return View("WorkOrders", new ManufacturingWorkOrdersIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = normalizedCode,
            StatusFilter = status,
            ProductionTypeFilter = productionType,
            IsActiveFilter = isActive,
            Items = items ?? PagedResult<ManufacturingWorkOrderDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("mrp")]
    public async Task<IActionResult> Mrp(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "rundate",
        string? sortDirection = "desc",
        string? code = null,
        MrpStatus? status = null,
        DateOnly? runDateFrom = null,
        DateOnly? runDateTo = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "rundate", "code", "rundate", "status", "horizondays");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedCode = NormalizeText(code);
        var (normalizedRunDateFrom, normalizedRunDateTo) = NormalizeDateRange(runDateFrom, runDateTo);

        var items = await manufacturingApiClient.GetMrpRunsAsync(accessToken, new ManufacturingMrpRunPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = normalizedCode,
            Status = status,
            RunDateFrom = normalizedRunDateFrom,
            RunDateTo = normalizedRunDateTo
        }, ct);

        ViewData["Title"] = "MRP";
        ViewData["Breadcrumb"] = "Manufacturing / MRP";

        return View("Mrp", new ManufacturingMrpRunsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = normalizedCode,
            StatusFilter = status,
            RunDateFromFilter = normalizedRunDateFrom,
            RunDateToFilter = normalizedRunDateTo,
            Items = items ?? PagedResult<ManufacturingMrpRunDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("qc")]
    public async Task<IActionResult> Qc(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "inspectedat",
        string? sortDirection = "desc",
        string? code = null,
        QcStatus? status = null,
        QcResult? result = null,
        DateTimeOffset? inspectedFrom = null,
        DateTimeOffset? inspectedTo = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "inspectedat", "code", "workordercode", "itemcode", "status", "result", "inspectedat");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedCode = NormalizeText(code);
        var (normalizedInspectedFrom, normalizedInspectedTo) = NormalizeDateTimeOffsetRange(inspectedFrom, inspectedTo);

        var items = await manufacturingApiClient.GetQcInspectionsAsync(accessToken, new ManufacturingQcInspectionPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = normalizedCode,
            Status = status,
            Result = result,
            InspectedFrom = normalizedInspectedFrom,
            InspectedTo = normalizedInspectedTo
        }, ct);

        ViewData["Title"] = "Quality Control";
        ViewData["Breadcrumb"] = "Manufacturing / Quality Control";

        return View("Qc", new ManufacturingQcInspectionsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = normalizedCode,
            StatusFilter = status,
            ResultFilter = result,
            InspectedFromFilter = normalizedInspectedFrom,
            InspectedToFilter = normalizedInspectedTo,
            Items = items ?? PagedResult<ManufacturingQcInspectionDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }
}
