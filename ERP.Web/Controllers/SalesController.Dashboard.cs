namespace ERP.Web.Controllers;

public sealed partial class SalesController
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var dashboard = await salesApiClient.GetDashboardAsync(accessToken, ct) ?? new SalesDashboardDto();

        ViewData["Title"] = "Sales Dashboard";
        ViewData["Breadcrumb"] = "Sales / Dashboard";

        return View("Index", new SalesDashboardViewModel
        {
            Data = dashboard
        });
    }
}
