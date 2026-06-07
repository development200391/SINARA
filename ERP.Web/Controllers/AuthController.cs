using System.Security.Claims;
using ERP.Application.DTOs.Auth;
using ERP.Web.Services;
using ERP.Web.Services.Localization;
using ERP.Web.ViewModels.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[Route("auth")]
public sealed class AuthController(IAuthApiClient authApiClient, ITextLocalizer textLocalizer) : Controller
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

    [HttpGet("forgot-password")]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["Title"] = textLocalizer["auth.forgot_password"];
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, CancellationToken ct = default)
    {
        ViewData["Title"] = textLocalizer["auth.forgot_password"];

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await authApiClient.ForgotPasswordAsync(new ForgotPasswordRequest
        {
            Email = model.Email
        }, ct);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(
                string.Empty,
                string.IsNullOrWhiteSpace(result.ErrorMessage) ? "Failed to send reset password request." : result.ErrorMessage);

            return View(model);
        }

        TempData["SuccessMessage"] = textLocalizer["auth.forgot_password_sent"];
        return RedirectToAction(nameof(ForgotPassword));
    }

    [HttpGet("reset-password")]
    [AllowAnonymous]
    public IActionResult ResetPassword(string? email = null, string? token = null)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            TempData["ErrorMessage"] = textLocalizer["auth.reset_link_invalid"];
            return RedirectToAction(nameof(ForgotPassword));
        }

        ViewData["Title"] = textLocalizer["auth.reset_password"];

        return View(new ResetPasswordViewModel
        {
            Email = email,
            Token = token
        });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, CancellationToken ct = default)
    {
        ViewData["Title"] = textLocalizer["auth.reset_password"];

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await authApiClient.ResetPasswordAsync(new ResetPasswordRequest
        {
            Email = model.Email,
            Token = model.Token,
            NewPassword = model.NewPassword,
            ConfirmPassword = model.ConfirmPassword
        }, ct);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(
                string.Empty,
                string.IsNullOrWhiteSpace(result.ErrorMessage) ? textLocalizer["auth.reset_failed"] : result.ErrorMessage);

            return View(model);
        }

        TempData["SuccessMessage"] = textLocalizer["auth.reset_success"];
        return RedirectToAction(nameof(Login));
    }

    [HttpGet("change-password")]
    [Authorize]
    public IActionResult ChangePassword()
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action(nameof(ChangePassword), "Auth") });
        }

        ViewData["Title"] = "Change Password";
        ViewData["Breadcrumb"] = "Change Password";

        return View(new ChangePasswordViewModel());
    }

    [HttpPost("change-password")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model, CancellationToken ct = default)
    {
        ViewData["Title"] = "Change Password";
        ViewData["Breadcrumb"] = "Change Password";

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action(nameof(ChangePassword), "Auth") });
        }

        var result = await authApiClient.ChangePasswordAsync(new ChangePasswordRequest
        {
            CurrentPassword = model.CurrentPassword,
            NewPassword = model.NewPassword,
            ConfirmPassword = model.ConfirmPassword
        }, accessToken, ct);

        if (!result.IsSuccess)
        {
            if (result.StatusCode == StatusCodes.Status401Unauthorized)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action(nameof(ChangePassword), "Auth") });
            }

            ModelState.AddModelError(string.Empty,
                string.IsNullOrWhiteSpace(result.ErrorMessage) ? "Failed to change password." : result.ErrorMessage);

            return View(model);
        }

        TempData["SuccessMessage"] = "Password changed successfully.";
        return RedirectToAction(nameof(ChangePassword));
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

    private string? GetAccessToken() => User.FindFirstValue("access_token");
}

