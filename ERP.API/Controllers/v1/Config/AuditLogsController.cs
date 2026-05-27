using ERP.Application.DTOs.Config;
using ERP.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.v1.Config;

[Route("api/v1/config/audit-logs")]
public sealed class AuditLogsController(IAuditService auditService) : ConfigControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] AuditLogPagedRequest request, CancellationToken ct)
    {
        var result = await auditService.GetPagedAsync(request, ct);
        return Ok(result);
    }
}
