using ERP.Application.DTOs.HR;
using ERP.Application.Services.HR;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.v1.HR;

[Route("api/v1/hr/attendance/self")]
public sealed class SelfAttendanceController(IAttendanceService attendanceService, IEmployeeService employeeService) : HrControllerBase
{
    [HttpGet("today")]
    public async Task<IActionResult> GetToday(CancellationToken ct)
    {
        var employeeId = await ResolveEmployeeIdAsync(ct);
        if (employeeId is null)
        {
            return BadRequest(new { message = "No employee profile is linked to this account." });
        }

        var result = await attendanceService.GetTodayAsync(employeeId.Value, ct);
        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct)
    {
        var employeeId = await ResolveEmployeeIdAsync(ct);
        if (employeeId is null)
        {
            return BadRequest(new { message = "No employee profile is linked to this account." });
        }

        var records = await attendanceService.GetByEmployeeAsync(employeeId.Value, ct);

        if (from.HasValue)
        {
            records = records.Where(x => x.Date >= from.Value).ToList();
        }

        if (to.HasValue)
        {
            records = records.Where(x => x.Date <= to.Value).ToList();
        }

        return Ok(records);
    }

    [HttpPost("check-in")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInOutRequest request, CancellationToken ct)
    {
        var employeeId = await ResolveEmployeeIdAsync(ct);
        if (employeeId is null)
        {
            return BadRequest(new { message = "No employee profile is linked to this account." });
        }

        try
        {
            var result = await attendanceService.CheckInAsync(employeeId.Value, request.Latitude, request.Longitude, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("check-out")]
    public async Task<IActionResult> CheckOut([FromBody] CheckInOutRequest request, CancellationToken ct)
    {
        var employeeId = await ResolveEmployeeIdAsync(ct);
        if (employeeId is null)
        {
            return BadRequest(new { message = "No employee profile is linked to this account." });
        }

        try
        {
            var result = await attendanceService.CheckOutAsync(employeeId.Value, request.Latitude, request.Longitude, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("mark")]
    public async Task<IActionResult> Mark([FromBody] MarkAttendanceStatusRequest request, CancellationToken ct)
    {
        var employeeId = await ResolveEmployeeIdAsync(ct);
        if (employeeId is null)
        {
            return BadRequest(new { message = "No employee profile is linked to this account." });
        }

        try
        {
            var result = await attendanceService.MarkStatusAsync(employeeId.Value, request, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private async Task<int?> ResolveEmployeeIdAsync(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return null;
        }

        var employee = await employeeService.GetByUserIdAsync(userId.Value, ct);
        return employee?.Id;
    }
}
