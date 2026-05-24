using ERP.Application.DTOs.Config;
using ERP.Application.Services.Config;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.v1.Config;

[Route("api/v1/config/modules")]
public sealed class ModulesController(IModuleService moduleService) : ConfigControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var modules = await moduleService.GetAllAsync(ct);
        return Ok(modules);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ModuleDto request, CancellationToken ct)
    {
        var updated = await moduleService.UpdateAsync(id, request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }
}
