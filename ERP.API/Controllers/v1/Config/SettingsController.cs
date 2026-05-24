using ERP.Application.DTOs.Config;
using ERP.Application.Services.Config;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.v1.Config;

[Route("api/v1/config/settings")]
public sealed class SettingsController(IAppSettingsService appSettingsService) : ConfigControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var settings = await appSettingsService.GetAsync(ct);
        return Ok(settings);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] AppSettingsDto request, CancellationToken ct)
    {
        var updated = await appSettingsService.UpdateAsync(request, ct);
        return Ok(updated);
    }
}
