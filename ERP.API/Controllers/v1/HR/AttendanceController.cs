using ERP.Application.DTOs.HR;
using ERP.Application.Services.HR;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.v1.HR;

[Route("api/v1/hr/attendance")]
public sealed class AttendanceController(IAttendanceService attendanceService) : HrControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] AttendanceReportRequest request, CancellationToken ct)
    {
        var result = await attendanceService.GetPagedAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await attendanceService.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("by-employee/{employeeId:int}")]
    public async Task<IActionResult> GetByEmployee(int employeeId, CancellationToken ct)
    {
        var result = await attendanceService.GetByEmployeeAsync(employeeId, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AttendanceRecordRequest request, CancellationToken ct)
    {
        try
        {
            var created = await attendanceService.CreateAsync(request, ct);
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AttendanceRecordRequest request, CancellationToken ct)
    {
        try
        {
            var updated = await attendanceService.UpdateAsync(id, request, ct);
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
        var deleted = await attendanceService.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}
