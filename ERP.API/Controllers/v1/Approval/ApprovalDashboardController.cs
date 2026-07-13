using ERP.Application.Services.Approval;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.v1.Approval;

[Route("api/v1/approval/dashboard")]
public sealed class ApprovalDashboardController(IApprovalReportService reportService) : ApprovalControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var dashboard = await reportService.GetDashboardAsync(userId.Value, ct);
        return Ok(dashboard);
    }
}
