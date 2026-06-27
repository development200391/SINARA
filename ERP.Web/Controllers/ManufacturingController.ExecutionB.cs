using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Manufacturing;
using ERP.Domain.Enums.Manufacturing;
using ERP.Web.ViewModels.Manufacturing;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class ManufacturingController
{
    [HttpGet("scrap")]
    public async Task<IActionResult> Scrap(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "recordedat",
        string? sortDirection = "desc",
        string? code = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "recordedat", "code", "reason", "qtyscrap", "totalscrapcost", "recordedat");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedCode = NormalizeText(code);
        var (normalizedRecordedFrom, normalizedRecordedTo) = NormalizeDateTimeOffsetRange(recordedFrom, recordedTo);

        var items = await manufacturingApiClient.GetScrapRecordsAsync(accessToken, new ManufacturingScrapRecordPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = normalizedCode,
            Reason = reason,
            RecordedFrom = normalizedRecordedFrom,
            RecordedTo = normalizedRecordedTo
        }, ct);

        ViewData["Title"] = "Scrap";
        ViewData["Breadcrumb"] = "Manufacturing / Scrap";

        return View("Scrap", new ManufacturingScrapRecordsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = normalizedCode,
            ReasonFilter = reason,
            RecordedFromFilter = normalizedRecordedFrom,
            RecordedToFilter = normalizedRecordedTo,
            Items = items ?? PagedResult<ManufacturingScrapRecordDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("rework")]
    public async Task<IActionResult> Rework(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "openedat",
        string? sortDirection = "desc",
        string? code = null,
        WorkOrderStatus? status = null,
        DateTimeOffset? openedFrom = null,
        DateTimeOffset? openedTo = null,
        DateTimeOffset? closedFrom = null,
        DateTimeOffset? closedTo = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "openedat", "code", "status", "qtyrework", "openedat", "closedat");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedCode = NormalizeText(code);
        var (normalizedOpenedFrom, normalizedOpenedTo) = NormalizeDateTimeOffsetRange(openedFrom, openedTo);
        var (normalizedClosedFrom, normalizedClosedTo) = NormalizeDateTimeOffsetRange(closedFrom, closedTo);

        var items = await manufacturingApiClient.GetReworkOrdersAsync(accessToken, new ManufacturingReworkOrderPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = normalizedCode,
            Status = status,
            OpenedFrom = normalizedOpenedFrom,
            OpenedTo = normalizedOpenedTo,
            ClosedFrom = normalizedClosedFrom,
            ClosedTo = normalizedClosedTo
        }, ct);

        ViewData["Title"] = "Rework";
        ViewData["Breadcrumb"] = "Manufacturing / Rework";

        return View("Rework", new ManufacturingReworkOrdersIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = normalizedCode,
            StatusFilter = status,
            OpenedFromFilter = normalizedOpenedFrom,
            OpenedToFilter = normalizedOpenedTo,
            ClosedFromFilter = normalizedClosedFrom,
            ClosedToFilter = normalizedClosedTo,
            Items = items ?? PagedResult<ManufacturingReworkOrderDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }
}
