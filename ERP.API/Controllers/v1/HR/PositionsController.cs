using ERP.Application.DTOs.HR;
using ERP.Application.Services.HR;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.v1.HR;

[Route("api/v1/hr/positions")]
public sealed class PositionsController(IPositionService positionService) : HrControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] PositionPagedRequest request, CancellationToken ct)
    {
        var result = await positionService.GetPagedAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await positionService.GetAllAsync(ct);
        return Ok(result);
    }

    [HttpGet("by-department/{departmentId:int}")]
    public async Task<IActionResult> GetByDepartment(int departmentId, CancellationToken ct)
    {
        var result = await positionService.GetByDepartmentAsync(departmentId, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await positionService.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PositionDto request, CancellationToken ct)
    {
        try
        {
            var created = await positionService.CreateAsync(request, ct);
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] PositionDto request, CancellationToken ct)
    {
        try
        {
            var updated = await positionService.UpdateAsync(id, request, ct);
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
            var deleted = await positionService.DeleteAsync(id, ct);
            return deleted ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
