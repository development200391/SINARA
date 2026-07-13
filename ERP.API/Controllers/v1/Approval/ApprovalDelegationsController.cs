using ERP.Application.DTOs.Approval;
using ERP.Application.Services.Approval;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.v1.Approval;

[Route("api/v1/approval/delegations")]
public sealed class ApprovalDelegationsController(IApprovalDelegationService delegationService) : ApprovalControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ApprovalDelegationPagedRequest request, CancellationToken ct)
    {
        var result = await delegationService.GetDelegationsPagedAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var delegation = await delegationService.GetDelegationByIdAsync(id, ct);
        return delegation is null ? NotFound() : Ok(delegation);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ApprovalDelegationDto request, CancellationToken ct)
    {
        try
        {
            var result = await delegationService.CreateDelegationAsync(request, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ApprovalDelegationDto request, CancellationToken ct)
    {
        try
        {
            var result = await delegationService.UpdateDelegationAsync(id, request, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/revoke")]
    public async Task<IActionResult> Revoke(int id, CancellationToken ct)
    {
        try
        {
            await delegationService.RevokeDelegationAsync(id, ct);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
