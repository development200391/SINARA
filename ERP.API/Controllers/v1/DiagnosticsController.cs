using ERP.Application.DTOs.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ERP.API.Controllers.v1;

[ApiController]
[Route("api/v1/diagnostics")]
[AllowAnonymous]
public sealed class DiagnosticsController : ControllerBase
{
    // RequestLoggingMiddleware already captures this request's body (and the
    // X-Correlation-Id header) into the same requests-*.txt log used for HTTP
    // traffic, so there is nothing left for this action to persist itself.
    [HttpPost("client-log")]
    [EnableRateLimiting("client-log")]
    public IActionResult ClientLog([FromBody] ClientLogRequest request)
    {
        return NoContent();
    }

    [HttpGet("server-time")]
    public IActionResult ServerTime()
    {
        return Ok(new ServerTimeResponse { ServerTime = DateTimeOffset.UtcNow });
    }
}
