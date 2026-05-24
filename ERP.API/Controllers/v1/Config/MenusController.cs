using ERP.Application.DTOs.Config;
using ERP.Application.Services.Config;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.v1.Config;

[Route("api/v1/config/menus")]
public sealed class MenusController(IMenuService menuService) : ConfigControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetByModule([FromQuery] int moduleId, CancellationToken ct)
    {
        if (moduleId <= 0)
        {
            return BadRequest(new { message = "moduleId is required." });
        }

        var menus = await menuService.GetByModuleAsync(moduleId, ct);
        return Ok(menus);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var menu = await menuService.GetByIdAsync(id, ct);
        return menu is null ? NotFound() : Ok(menu);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MenuDto request, CancellationToken ct)
    {
        try
        {
            var created = await menuService.CreateAsync(request, ct);
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] MenuDto request, CancellationToken ct)
    {
        try
        {
            var updated = await menuService.UpdateAsync(id, request, ct);
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
            var deleted = await menuService.DeleteAsync(id, ct);
            return deleted ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("reorder")]
    public async Task<IActionResult> Reorder([FromBody] ReorderMenusRequest request, CancellationToken ct)
    {
        var reordered = await menuService.ReorderAsync(request.ModuleId, request.OrderedMenuIds, ct);
        return reordered ? NoContent() : BadRequest();
    }

    public sealed class ReorderMenusRequest
    {
        public int ModuleId { get; set; }
        public IReadOnlyList<int> OrderedMenuIds { get; set; } = [];
    }
}
