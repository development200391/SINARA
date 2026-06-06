using ERP.Application.DTOs.Approval;
using ERP.Application.DTOs.Common;
using ERP.Domain.Enums.Approval;
using ERP.Web.ViewModels.Approval;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class ApprovalController
{
    [HttpGet("my-requests")]
    public async Task<IActionResult> MyRequests(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "requestedat",
        string? sortDirection = "desc",
        string? requestNo = null,
        string? module = null,
        string? referenceType = null,
        ApprovalRequestStatus? status = null,
        DateOnly? requestedDateFrom = null,
        DateOnly? requestedDateTo = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "requestedat", "requestno", "subject", "module", "requestedat", "status", "finalactionat");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedDateFrom, normalizedDateTo) = NormalizeDateRange(requestedDateFrom, requestedDateTo);

        var items = await approvalApiClient.GetMyRequestsAsync(accessToken, new ApprovalRequestPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            RequestNo = NormalizeText(requestNo),
            Module = NormalizeText(module),
            ReferenceType = NormalizeText(referenceType),
            Status = status,
            RequestedDateFrom = normalizedDateFrom,
            RequestedDateTo = normalizedDateTo
        }, ct);

        ViewData["Title"] = "My Approval Requests";
        ViewData["Breadcrumb"] = "Approval / My Requests";

        return View("MyRequests", new ApprovalMyRequestsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            RequestNoFilter = NormalizeText(requestNo),
            ModuleFilter = NormalizeText(module),
            ReferenceTypeFilter = NormalizeText(referenceType),
            StatusFilter = status,
            RequestedDateFromFilter = normalizedDateFrom,
            RequestedDateToFilter = normalizedDateTo,
            Items = items ?? PagedResult<ApprovalRequestDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpPost("requests/cancel/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelRequest(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var result = await approvalApiClient.CancelAsync(accessToken, id, "Cancelled by requester.", ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? "Request cancelled."
            : ResolveApiErrorMessage(result, "Failed to cancel request.");

        return RedirectToAction(nameof(MyRequests));
    }
}
