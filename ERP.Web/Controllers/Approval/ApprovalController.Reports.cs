using ERP.Application.DTOs.Approval;
using ERP.Application.DTOs.Common;
using ERP.Web.ViewModels.Approval;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class ApprovalController
{
    [HttpGet("reports/sla")]
    public async Task<IActionResult> SlaReport(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "module",
        string? sortDirection = "asc",
        string? module = null,
        int? templateId = null,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "module", "module", "templatecode", "slahours", "averageresponsehours", "totalsteps", "withinslacount", "overduecount", "compliancerate");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedDateFrom, normalizedDateTo) = NormalizeDateRange(dateFrom, dateTo);

        var optionsTask = approvalApiClient.GetTemplateOptionsAsync(accessToken, ct);
        var itemsTask = approvalApiClient.GetSlaReportAsync(accessToken, new ApprovalSlaReportPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Module = NormalizeText(module),
            TemplateId = templateId,
            DateFrom = normalizedDateFrom,
            DateTo = normalizedDateTo
        }, ct);

        await Task.WhenAll(optionsTask, itemsTask);

        ViewData["Title"] = "SLA Report";
        ViewData["Breadcrumb"] = "Approval / Reports / SLA";

        return View("Reports/Sla", new ApprovalSlaReportIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            ModuleFilter = NormalizeText(module),
            TemplateIdFilter = templateId,
            DateFromFilter = normalizedDateFrom,
            DateToFilter = normalizedDateTo,
            TemplateOptions = await optionsTask,
            Items = await itemsTask ?? PagedResult<ApprovalSlaReportDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("reports/by-template")]
    public async Task<IActionResult> ByTemplateReport(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "templatecode",
        string? sortDirection = "asc",
        string? module = null,
        int? templateId = null,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "templatecode", "templatecode", "module", "totalrequests", "approvedcount", "rejectedcount", "pendingcount", "averagedurationhours");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedDateFrom, normalizedDateTo) = NormalizeDateRange(dateFrom, dateTo);

        var optionsTask = approvalApiClient.GetTemplateOptionsAsync(accessToken, ct);
        var itemsTask = approvalApiClient.GetTemplateReportAsync(accessToken, new ApprovalTemplateReportPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Module = NormalizeText(module),
            TemplateId = templateId,
            DateFrom = normalizedDateFrom,
            DateTo = normalizedDateTo
        }, ct);

        await Task.WhenAll(optionsTask, itemsTask);

        ViewData["Title"] = "By Template Report";
        ViewData["Breadcrumb"] = "Approval / Reports / By Template";

        return View("Reports/ByTemplate", new ApprovalTemplateReportIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            ModuleFilter = NormalizeText(module),
            TemplateIdFilter = templateId,
            DateFromFilter = normalizedDateFrom,
            DateToFilter = normalizedDateTo,
            TemplateOptions = await optionsTask,
            Items = await itemsTask ?? PagedResult<ApprovalTemplateReportDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("reports/audit")]
    public async Task<IActionResult> AuditReport(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "createdat",
        string? sortDirection = "desc",
        int? requestId = null,
        int? actorUserId = null,
        string? action = null,
        string? module = null,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "createdat", "id", "requestid", "actorusername", "action", "oldstatus", "newstatus", "createdat");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedDateFrom, normalizedDateTo) = NormalizeDateRange(dateFrom, dateTo);

        var usersTask = approvalApiClient.GetApproverOptionsAsync(accessToken, ct);
        var itemsTask = approvalApiClient.GetAuditLogsAsync(accessToken, new ApprovalAuditPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            RequestId = requestId,
            ActorUserId = actorUserId,
            Action = NormalizeText(action),
            Module = NormalizeText(module),
            DateFrom = normalizedDateFrom,
            DateTo = normalizedDateTo
        }, ct);

        await Task.WhenAll(usersTask, itemsTask);

        ViewData["Title"] = "Audit Trail";
        ViewData["Breadcrumb"] = "Approval / Reports / Audit";

        return View("Reports/Audit", new ApprovalAuditIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            RequestIdFilter = requestId,
            ActorUserIdFilter = actorUserId,
            ActionFilter = NormalizeText(action),
            ModuleFilter = NormalizeText(module),
            DateFromFilter = normalizedDateFrom,
            DateToFilter = normalizedDateTo,
            UserOptions = await usersTask,
            Items = await itemsTask ?? PagedResult<ApprovalAuditLogDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }
}
