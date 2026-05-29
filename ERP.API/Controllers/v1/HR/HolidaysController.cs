using ERP.Application.DTOs.HR;
using ERP.Application.Services.HR;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.v1.HR;

[Route("api/v1/hr/holidays")]
public sealed class HolidaysController(IHolidayService holidayService) : HrControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] HolidayPagedRequest request, CancellationToken ct)
    {
        var result = await holidayService.GetPagedAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await holidayService.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] HolidayDto request, CancellationToken ct)
    {
        try
        {
            var created = await holidayService.CreateAsync(request, ct);
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] HolidayDto request, CancellationToken ct)
    {
        try
        {
            var updated = await holidayService.UpdateAsync(id, request, ct);
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
        var deleted = await holidayService.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}
