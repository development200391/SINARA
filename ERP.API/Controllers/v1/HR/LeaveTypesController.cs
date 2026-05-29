using ERP.Application.DTOs.HR;
using ERP.Application.Services.HR;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.v1.HR;

[Route("api/v1/hr/leave-types")]
public sealed class LeaveTypesController(ILeaveService leaveService) : HrControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] LeaveTypePagedRequest request, CancellationToken ct)
    {
        var result = await leaveService.GetLeaveTypesAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await leaveService.GetLeaveTypeByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LeaveTypeDto request, CancellationToken ct)
    {
        try
        {
            var created = await leaveService.CreateLeaveTypeAsync(request, ct);
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] LeaveTypeDto request, CancellationToken ct)
    {
        try
        {
            var updated = await leaveService.UpdateLeaveTypeAsync(id, request, ct);
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
            var deleted = await leaveService.DeleteLeaveTypeAsync(id, ct);
            return deleted ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
