using ERP.Application.DTOs.Approval;
using ERP.Application.Services.Approval;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.v1.Approval;

[Route("api/v1/approval/reports")]
public sealed class ApprovalReportsController(IApprovalReportService reportService) : ApprovalControllerBase
{
    [HttpGet("sla")]
    public async Task<IActionResult> GetSlaReport([FromQuery] ApprovalSlaReportPagedRequest request, CancellationToken ct)
    {
        var result = await reportService.GetSlaReportAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("by-template")]
    public async Task<IActionResult> GetTemplateReport([FromQuery] ApprovalTemplateReportPagedRequest request, CancellationToken ct)
    {
        var result = await reportService.GetTemplateReportAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("audit")]
    public async Task<IActionResult> GetAuditLogs([FromQuery] ApprovalAuditPagedRequest request, CancellationToken ct)
    {
        var result = await reportService.GetAuditLogsAsync(request, ct);
        return Ok(result);
    }
}
