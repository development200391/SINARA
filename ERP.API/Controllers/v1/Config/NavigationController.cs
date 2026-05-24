using ERP.Application.Services.Config;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.v1.Config;

[Route("api/v1/config/navigation")]
public sealed class NavigationController(IMenuService menuService) : ConfigControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var navigation = await menuService.GetNavigationForUserAsync(userId.Value, ct);
        return Ok(navigation);
    }
}
