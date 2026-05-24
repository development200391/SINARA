using ERP.Application.DTOs.Config;
using ERP.Application.Services.Config;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.v1.Config;

[Route("api/v1/config/roles")]
public sealed class RolesController(IRoleService roleService) : ConfigControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var roles = await roleService.GetAllAsync(ct);
        return Ok(roles);
    }

    [HttpGet("{id:int}/permissions")]
    public async Task<IActionResult> GetPermissions(int id, CancellationToken ct)
    {
        var matrix = await roleService.GetPermissionsAsync(id, ct);
        return matrix is null ? NotFound() : Ok(matrix);
    }

    [HttpPut("{id:int}/permissions")]
    public async Task<IActionResult> UpdatePermissions(int id, [FromBody] PermissionMatrixDto request, CancellationToken ct)
    {
        if (id != request.RoleId)
        {
            request.RoleId = id;
        }

        var updated = await roleService.UpdatePermissionsAsync(id, request, ct);
        return updated ? NoContent() : NotFound();
    }
}
