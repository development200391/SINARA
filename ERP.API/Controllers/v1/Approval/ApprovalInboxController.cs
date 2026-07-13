using ERP.Application.DTOs.Approval;
using ERP.Application.Services.Approval;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.v1.Approval;

[Route("api/v1/approval/inbox")]
public sealed class ApprovalInboxController(IApprovalRequestService requestService) : ApprovalControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ApprovalInboxPagedRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await requestService.GetInboxPagedAsync(userId.Value, request, ct);
        return Ok(result);
    }
}
