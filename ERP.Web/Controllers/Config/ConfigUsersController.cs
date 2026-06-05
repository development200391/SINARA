using System.Security.Claims;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Config;
using ERP.Web.Services;
using ERP.Web.ViewModels.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[Authorize]
[Route("config/users")]
public sealed class ConfigUsersController(IConfigApiClient configApiClient) : Controller
{
    private const int DefaultPageSize = 20;
    private const string DefaultSortBy = "username";

    [HttpGet("")]
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = DefaultSortBy,
        string? sortDirection = "asc",
        string? username = null,
        string? fullName = null,
        string? email = null,
        CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path + Request.QueryString });
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy);
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedUsername = NormalizeTextFilter(username);
        var normalizedFullName = NormalizeTextFilter(fullName);
        var normalizedEmail = NormalizeTextFilter(email);

        var result = await configApiClient.GetUsersAsync(accessToken, new UserPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Username = normalizedUsername,
            FullName = normalizedFullName,
            Email = normalizedEmail
        }, ct);

        var model = new ConfigUsersIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            UsernameFilter = normalizedUsername,
            FullNameFilter = normalizedFullName,
            EmailFilter = normalizedEmail,
            Users = result ?? PagedResult<UserDto>.Create([], 0, normalizedPage, normalizedPageSize)
        };

        ViewData["Title"] = "Users";
        ViewData["Breadcrumb"] = "Configuration / Users";

        return View(model);
    }

    [HttpGet("details/{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path + Request.QueryString });
        }

        var user = await configApiClient.GetUserByIdAsync(accessToken, id, ct);
        if (user is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "User Details";
        ViewData["Breadcrumb"] = "Configuration / Users / Details";

        return View(user);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var roles = await configApiClient.GetRolesAsync(accessToken, ct);

        ViewData["Title"] = "Create User";
        ViewData["Breadcrumb"] = "Configuration / Users / Create";

        return View(new ConfigUserEditViewModel { AvailableRoles = roles, IsActive = true });
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ConfigUserEditViewModel model, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var roles = await configApiClient.GetRolesAsync(accessToken, ct);
        model.AvailableRoles = roles;

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create User";
            ViewData["Breadcrumb"] = "Configuration / Users / Create";
            return View(model);
        }

        var request = new UserDto
        {
            Username = model.Username,
            FullName = model.FullName,
            Email = model.Email,
            Phone = model.Phone,
            IsActive = model.IsActive,
            Roles = model.SelectedRoles
        };

        var created = await configApiClient.CreateUserAsync(accessToken, request, ct);
        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to create user." : created.ErrorMessage);
            ViewData["Title"] = "Create User";
            ViewData["Breadcrumb"] = "Configuration / Users / Create";
            return View(model);
        }

        TempData["SuccessMessage"] = "User created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var user = await configApiClient.GetUserByIdAsync(accessToken, id, ct);
        if (user is null)
        {
            return NotFound();
        }

        var roles = await configApiClient.GetRolesAsync(accessToken, ct);

        ViewData["Title"] = "Edit User";
        ViewData["Breadcrumb"] = "Configuration / Users / Edit";

        return View(new ConfigUserEditViewModel
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            IsActive = user.IsActive,
            SelectedRoles = user.Roles.ToList(),
            AvailableRoles = roles
        });
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ConfigUserEditViewModel model, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var roles = await configApiClient.GetRolesAsync(accessToken, ct);
        model.AvailableRoles = roles;

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit User";
            ViewData["Breadcrumb"] = "Configuration / Users / Edit";
            return View(model);
        }

        var request = new UserDto
        {
            Id = id,
            Username = model.Username,
            FullName = model.FullName,
            Email = model.Email,
            Phone = model.Phone,
            IsActive = model.IsActive,
            Roles = model.SelectedRoles
        };

        var updated = await configApiClient.UpdateUserAsync(accessToken, id, request, ct);
        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update user." : updated.ErrorMessage);
            ViewData["Title"] = "Edit User";
            ViewData["Breadcrumb"] = "Configuration / Users / Edit";
            return View(model);
        }

        TempData["SuccessMessage"] = "User updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var deleted = await configApiClient.DeleteUserAsync(accessToken, id, ct);
        TempData[deleted.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = deleted.IsSuccess
            ? "User deleted successfully."
            : (string.IsNullOrWhiteSpace(deleted.ErrorMessage) ? "Failed to delete user." : deleted.ErrorMessage);

        return RedirectToAction(nameof(Index));
    }

    private static int NormalizePageSize(int pageSize) => pageSize is 20 or 50 or 100 ? pageSize : DefaultPageSize;

    private static string NormalizeSortBy(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return DefaultSortBy;
        }

        return sortBy.Trim().ToLowerInvariant() switch
        {
            "username" => "username",
            "fullname" => "fullName",
            "email" => "email",
            "isactive" => "isActive",
            _ => DefaultSortBy
        };
    }

    private static string NormalizeSortDirection(string? sortDirection) =>
        string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";

    private static string? NormalizeTextFilter(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private string? GetAccessToken() => User.FindFirstValue("access_token");
}
