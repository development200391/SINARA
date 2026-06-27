using ERP.Application.DTOs.Manufacturing;
using ERP.Web.ViewModels.Manufacturing;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class ManufacturingController
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var dashboard = await manufacturingApiClient.GetDashboardAsync(accessToken, ct) ?? new ManufacturingDashboardDto();

        ViewData["Title"] = "Manufacturing Dashboard";
        ViewData["Breadcrumb"] = "Manufacturing / Dashboard";

        return View("Index", new ManufacturingDashboardViewModel
        {
            Data = dashboard
        });
    }
}
