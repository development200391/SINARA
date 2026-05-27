using System.Security.Claims;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Config;
using ERP.Web.Services;
using ERP.Web.ViewModels.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[Authorize]
[Route("config/audit")]
public sealed class ConfigAuditController(IConfigApiClient configApiClient) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = 20,
        string? search = null,
        string? sortBy = "createdAt",
        string? sortDirection = "desc",
        CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var normalizedPageSize = pageSize is 20 or 50 or 100 ? pageSize : 20;
        var normalizedSortBy = string.IsNullOrWhiteSpace(sortBy) ? "createdAt" : sortBy.Trim();
        var normalizedSortDirection = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase)
            ? "asc"
            : "desc";

        var logs = await configApiClient.GetAuditLogsAsync(accessToken, new PagedRequest
        {
            Page = page,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection
        }, ct);

        ViewData["Title"] = "Audit Log";
        ViewData["Breadcrumb"] = "Configuration / Audit Log";

        return View(new ConfigAuditIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Logs = logs ?? PagedResult<AuditLogDto>.Create([], 0, page, normalizedPageSize)
        });
    }

    private string? GetAccessToken() => User.FindFirstValue("access_token");
}
