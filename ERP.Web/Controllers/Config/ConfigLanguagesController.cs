using System.Security.Claims;
using ERP.Application.DTOs.Config;
using ERP.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[Authorize]
[Route("config/languages")]
public sealed class ConfigLanguagesController(IConfigApiClient configApiClient) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var languages = await configApiClient.GetLanguagesAsync(accessToken, ct);

        ViewData["Title"] = "Languages";
        ViewData["Breadcrumb"] = "Configuration / Languages";

        return View(languages);
    }

    private string? GetAccessToken() => User.FindFirstValue("access_token");
}
