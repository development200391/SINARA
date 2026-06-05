using System.Security.Claims;
using ERP.Application.DTOs.Config;
using ERP.Web.Services;
using ERP.Web.ViewModels.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[Authorize]
[Route("config/roles")]
public sealed class ConfigRolesController(IConfigApiClient configApiClient) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var roles = await configApiClient.GetRolesAsync(accessToken, ct);

        ViewData["Title"] = "Roles";
        ViewData["Breadcrumb"] = "Configuration / Roles";

        return View(new ConfigRolesIndexViewModel { Roles = roles });
    }

    [HttpGet("{id:int}/permissions")]
    public async Task<IActionResult> Permissions(int id, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var roles = await configApiClient.GetRolesAsync(accessToken, ct);
        var role = roles.FirstOrDefault(x => x.Id == id);

        var matrix = await configApiClient.GetRolePermissionsAsync(accessToken, id, ct);
        if (role is null || matrix is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Role Permissions";
        ViewData["Breadcrumb"] = "Configuration / Roles / Permissions";

        return View(new ConfigRolePermissionsViewModel
        {
            Role = role,
            Matrix = matrix
        });
    }

    [HttpPost("{id:int}/permissions")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Permissions(int id, ConfigRolePermissionsViewModel model, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        model.Matrix.RoleId = id;
        var updated = await configApiClient.UpdateRolePermissionsAsync(accessToken, id, model.Matrix, ct);

        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update permissions." : updated.ErrorMessage);
        }
        else
        {
            TempData["SuccessMessage"] = "Permissions updated successfully.";
        }

        return RedirectToAction(nameof(Permissions), new { id });
    }

    private string? GetAccessToken() => User.FindFirstValue("access_token");
}
