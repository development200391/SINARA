using ERP.Application.DTOs.Approval;
using ERP.Application.DTOs.Common;
using ERP.Domain.Enums.Approval;
using ERP.Web.ViewModels.Approval;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class ApprovalController
{
    [HttpGet("inbox")]
    public async Task<IActionResult> Inbox(
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
        bool? isOverdue = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "requestedat", "requestno", "subject", "module", "requestedat", "dueat", "status");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedDateFrom, normalizedDateTo) = NormalizeDateRange(requestedDateFrom, requestedDateTo);

        var items = await approvalApiClient.GetInboxAsync(accessToken, new ApprovalInboxPagedRequest
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
            RequestedDateTo = normalizedDateTo,
            IsOverdue = isOverdue
        }, ct);

        ViewData["Title"] = "Approval Inbox";
        ViewData["Breadcrumb"] = "Approval / Inbox";

        return View("Inbox", new ApprovalInboxIndexViewModel
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
            IsOverdueFilter = isOverdue,
            Items = items ?? PagedResult<ApprovalInboxDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpPost("requests/approve/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveRequest(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var result = await approvalApiClient.ApproveAsync(accessToken, id, new TakeApprovalActionRequest
        {
            Comment = "Approved via inbox."
        }, ct);

        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? "Request approved."
            : ResolveApiErrorMessage(result, "Failed to approve request.");

        return RedirectToAction(nameof(Inbox));
    }

    [HttpPost("requests/reject/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectRequest(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var result = await approvalApiClient.RejectAsync(accessToken, id, new TakeApprovalActionRequest
        {
            Comment = "Rejected via inbox."
        }, ct);

        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? "Request rejected."
            : ResolveApiErrorMessage(result, "Failed to reject request.");

        return RedirectToAction(nameof(Inbox));
    }
}
