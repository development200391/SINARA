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
    public async Task<IActionResult> Index(int page = 1, string? search = null, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var logs = await configApiClient.GetAuditLogsAsync(accessToken, new PagedRequest
        {
            Page = page,
            PageSize = 20,
            Search = search
        }, ct);

        ViewData["Title"] = "Audit Log";
        ViewData["Breadcrumb"] = "Configuration / Audit Log";

        return View(new ConfigAuditIndexViewModel
        {
            Search = search,
            Logs = logs ?? PagedResult<AuditLogDto>.Create([], 0, page, 20)
        });
    }

    private string? GetAccessToken() => User.FindFirstValue("access_token");
}
