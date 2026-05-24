using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Config;
using ERP.Application.Services.Config;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.v1.Config;

[Route("api/v1/config/users")]
public sealed class UsersController(IUserService userService) : ConfigControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] PagedRequest request, CancellationToken ct)
    {
        var result = await userService.GetPagedAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var user = await userService.GetByIdAsync(id, ct);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserDto request, CancellationToken ct)
    {
        try
        {
            var created = await userService.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UserDto request, CancellationToken ct)
    {
        var updated = await userService.UpdateAsync(id, request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await userService.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}
