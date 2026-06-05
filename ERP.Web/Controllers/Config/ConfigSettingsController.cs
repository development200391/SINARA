using System.Security.Claims;
using ERP.Application.DTOs.Config;
using ERP.Web.Services;
using ERP.Web.ViewModels.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[Authorize]
[Route("config/settings")]
public sealed class ConfigSettingsController(IConfigApiClient configApiClient) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var settings = await configApiClient.GetSettingsAsync(accessToken, ct) ?? new AppSettingsDto();
        var languages = await configApiClient.GetLanguagesAsync(accessToken, ct);

        ViewData["Title"] = "System Settings";
        ViewData["Breadcrumb"] = "Configuration / Settings";

        return View(new ConfigSettingsViewModel
        {
            Settings = settings,
            Languages = languages
        });
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ConfigSettingsViewModel model, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var updated = await configApiClient.UpdateSettingsAsync(accessToken, model.Settings, ct);
        var languages = await configApiClient.GetLanguagesAsync(accessToken, ct);

        model.Settings = updated.Data ?? model.Settings;
        model.Languages = languages;

        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to save settings." : updated.ErrorMessage);
        }
        else
        {
            TempData["SuccessMessage"] = "Settings saved successfully.";
        }

        ViewData["Title"] = "System Settings";
        ViewData["Breadcrumb"] = "Configuration / Settings";

        return View(model);
    }

    private string? GetAccessToken() => User.FindFirstValue("access_token");
}
