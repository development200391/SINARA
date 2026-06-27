using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Manufacturing;
using ERP.Domain.Enums.Manufacturing;
using ERP.Web.ViewModels.Manufacturing;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class ManufacturingController
{
    [HttpGet("boms")]
    public async Task<IActionResult> Boms(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "code",
        string? sortDirection = "asc",
        string? code = null,
        BomStatus? status = null,
        bool? isActive = null,
        DateOnly? effectiveDateFrom = null,
        DateOnly? effectiveDateTo = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "code", "code", "status", "version", "qtyproduced", "effectivedate", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedCode = NormalizeText(code);
        var (normalizedEffectiveDateFrom, normalizedEffectiveDateTo) = NormalizeDateRange(effectiveDateFrom, effectiveDateTo);

        var items = await manufacturingApiClient.GetBomsAsync(accessToken, new ManufacturingBomPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = normalizedCode,
            Status = status,
            IsActive = isActive,
            EffectiveDateFrom = normalizedEffectiveDateFrom,
            EffectiveDateTo = normalizedEffectiveDateTo
        }, ct);

        ViewData["Title"] = "BOMs";
        ViewData["Breadcrumb"] = "Manufacturing / BOMs";

        return View("Boms", new ManufacturingBomsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = normalizedCode,
            StatusFilter = status,
            IsActiveFilter = isActive,
            EffectiveDateFromFilter = normalizedEffectiveDateFrom,
            EffectiveDateToFilter = normalizedEffectiveDateTo,
            Items = items ?? PagedResult<ManufacturingBomDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("routings")]
    public async Task<IActionResult> Routings(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "code",
        string? sortDirection = "asc",
        string? code = null,
        string? name = null,
        RoutingStatus? status = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "code", "code", "name", "version", "status", "totalleadtimehours", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedCode = NormalizeText(code);
        var normalizedName = NormalizeText(name);

        var items = await manufacturingApiClient.GetRoutingsAsync(accessToken, new ManufacturingRoutingPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = normalizedCode,
            Name = normalizedName,
            Status = status,
            IsActive = isActive
        }, ct);

        ViewData["Title"] = "Routings";
        ViewData["Breadcrumb"] = "Manufacturing / Routings";

        return View("Routings", new ManufacturingRoutingsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = normalizedCode,
            NameFilter = normalizedName,
            StatusFilter = status,
            IsActiveFilter = isActive,
            Items = items ?? PagedResult<ManufacturingRoutingDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }
}
