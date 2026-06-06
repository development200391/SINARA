using ERP.Web.ViewModels.Approval;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class ApprovalController
{
    [HttpGet("")]
    [HttpGet("dashboard")]
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        ViewData["Title"] = "Approval Dashboard";
        ViewData["Breadcrumb"] = "Approval / Dashboard";

        return View("Index", new ApprovalDashboardViewModel
        {
            Data = await approvalApiClient.GetDashboardAsync(accessToken, ct) ?? new()
        });
    }
}
