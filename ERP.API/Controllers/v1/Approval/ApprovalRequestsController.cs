using ERP.Application.DTOs.Approval;
using ERP.Application.Services.Approval;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.v1.Approval;

[Route("api/v1/approval/requests")]
public sealed class ApprovalRequestsController(IApprovalRequestService requestService) : ApprovalControllerBase
{
    [HttpGet("my")]
    public async Task<IActionResult> GetMyRequests([FromQuery] ApprovalRequestPagedRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await requestService.GetMyRequestsPagedAsync(userId.Value, request, ct);
        return Ok(result);
    }

    [HttpPost("{requestId:int}/actions/approve")]
    public async Task<IActionResult> Approve(int requestId, [FromBody] TakeApprovalActionRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var result = await requestService.ApproveAsync(requestId, userId.Value, request, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{requestId:int}/actions/reject")]
    public async Task<IActionResult> Reject(int requestId, [FromBody] TakeApprovalActionRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var result = await requestService.RejectAsync(requestId, userId.Value, request, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{requestId:int}/cancel")]
    public async Task<IActionResult> Cancel(int requestId, [FromBody] CancelApprovalRequestBody request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var result = await requestService.CancelAsync(requestId, userId.Value, request.Notes, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    public sealed class CancelApprovalRequestBody
    {
        public string? Notes { get; set; }
    }
}
