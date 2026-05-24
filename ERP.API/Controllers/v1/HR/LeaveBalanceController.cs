using ERP.Application.DTOs.HR;
using ERP.Application.Services.HR;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.v1.HR;

[Route("api/v1/hr/leave-balance")]
public sealed class LeaveBalanceController(ILeaveService leaveService) : HrControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] LeaveBalanceRequest request, CancellationToken ct)
    {
        var result = await leaveService.GetBalancesAsync(request, ct);
        return Ok(result);
    }
}
