using ERP.Application.DTOs.HR;
using ERP.Application.Services.HR;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.v1.HR;

[Route("api/v1/hr/leave-requests")]
public sealed class LeaveRequestsController(ILeaveService leaveService) : HrControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] LeaveRequestPagedRequest request, CancellationToken ct)
    {
        var result = await leaveService.GetRequestsAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await leaveService.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("options")]
    public async Task<IActionResult> GetOptions(CancellationToken ct)
    {
        var employees = await leaveService.GetEmployeeOptionsAsync(ct);
        var leaveTypes = await leaveService.GetLeaveTypeOptionsAsync(ct);

        return Ok(new LeaveRequestOptionsDto
        {
            Employees = employees,
            LeaveTypes = leaveTypes
        });
    }

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] SubmitLeaveRequest request, CancellationToken ct)
    {
        try
        {
            var created = await leaveService.SubmitAsync(request, ct);
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SubmitLeaveRequest request, CancellationToken ct)
    {
        try
        {
            var updated = await leaveService.UpdateAsync(id, request, ct);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            var deleted = await leaveService.DeleteAsync(id, ct);
            return deleted ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var approved = await leaveService.ApproveAsync(id, userId.Value, ct);
        return approved ? NoContent() : NotFound();
    }

    [HttpPut("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var rejected = await leaveService.RejectAsync(id, userId.Value, ct);
        return rejected ? NoContent() : NotFound();
    }
}
