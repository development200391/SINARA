using ERP.Application.DTOs.HR;
using ERP.Application.Services.HR;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.v1.HR;

[Route("api/v1/hr/attendance/settings")]
public sealed class AttendanceSettingsController(IAttendanceSettingService attendanceSettingService) : HrControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await attendanceSettingService.GetAsync(ct);
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] AttendanceSettingDto request, CancellationToken ct)
    {
        try
        {
            var result = await attendanceSettingService.UpdateAsync(request, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
