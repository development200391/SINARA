using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Manufacturing;
using ERP.Domain.Enums.Manufacturing;
using ERP.Web.ViewModels.Manufacturing;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class ManufacturingController
{
    [HttpGet("work-centers")]
    public async Task<IActionResult> WorkCenters(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "code",
        string? sortDirection = "asc",
        string? code = null,
        string? name = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "code", "code", "name", "capacityhoursperday", "laborcostperhour", "overheadcostperhour", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedCode = NormalizeText(code);
        var normalizedName = NormalizeText(name);

        var items = await manufacturingApiClient.GetWorkCentersAsync(accessToken, new ManufacturingWorkCenterPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = normalizedCode,
            Name = normalizedName,
            IsActive = isActive
        }, ct);

        ViewData["Title"] = "Work Centers";
        ViewData["Breadcrumb"] = "Manufacturing / Work Centers";

        return View("WorkCenters", new ManufacturingWorkCentersIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = normalizedCode,
            NameFilter = normalizedName,
            IsActiveFilter = isActive,
            Items = items ?? PagedResult<ManufacturingWorkCenterDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("qc/parameters")]
    public async Task<IActionResult> QcParameters(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "code",
        string? sortDirection = "asc",
        string? code = null,
        string? name = null,
        QcParameterType? parameterType = null,
        bool? isCritical = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "code", "code", "name", "parametertype", "iscritical", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedCode = NormalizeText(code);
        var normalizedName = NormalizeText(name);

        var items = await manufacturingApiClient.GetQcParametersAsync(accessToken, new ManufacturingQcParameterPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = normalizedCode,
            Name = normalizedName,
            ParameterType = parameterType,
            IsCritical = isCritical,
            IsActive = isActive
        }, ct);

        ViewData["Title"] = "QC Parameters";
        ViewData["Breadcrumb"] = "Manufacturing / QC Parameters";

        return View("QcParameters", new ManufacturingQcParametersIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = normalizedCode,
            NameFilter = normalizedName,
            ParameterTypeFilter = parameterType,
            IsCriticalFilter = isCritical,
            IsActiveFilter = isActive,
            Items = items ?? PagedResult<ManufacturingQcParameterDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }
}
