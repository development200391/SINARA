using System.Security.Claims;
using ERP.Application.DTOs.Auth;
using ERP.Web.Services;
using ERP.Web.ViewModels.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[Route("auth")]
public sealed class AuthController(IAuthApiClient authApiClient) : Controller
{
    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        var result = await authApiClient.LoginAsync(new LoginRequest
        {
            Username = model.Username,
            Password = model.Password
        }, ct);

        if (result is null)
        {
            ModelState.AddModelError(string.Empty, "Username atau password tidak valid.");
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.User.Id.ToString()),
            new(ClaimTypes.Name, result.User.Username),
            new(ClaimTypes.Email, result.User.Email),
            new("full_name", result.User.FullName),
            new("language", result.User.LanguagePreference),
            new("access_token", result.AccessToken),
            new("refresh_token", result.RefreshToken)
        };

        claims.AddRange(result.User.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var properties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = result.ExpiresAt,
            AllowRefresh = true
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost("logout")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken ct = default)
    {
        var refreshToken = User.FindFirstValue("refresh_token");
        var accessToken = User.FindFirstValue("access_token");

        await authApiClient.LogoutAsync(refreshToken, accessToken, ct);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("Login", "Auth");
    }

    [HttpGet("access-denied")]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }
}

