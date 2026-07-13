using ERP.Application.Services.Approval;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.v1.Approval;

[Route("api/v1/approval/approvers")]
public sealed class ApprovalLookupsController(IApprovalDelegationService delegationService) : ApprovalControllerBase
{
    [HttpGet("options")]
    public async Task<IActionResult> GetApproverOptions(CancellationToken ct)
    {
        var result = await delegationService.GetApproverOptionsAsync(ct);
        return Ok(result);
    }
}
