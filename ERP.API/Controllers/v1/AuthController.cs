using System.Security.Claims;
using ERP.Application.DTOs.Auth;
using ERP.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ERP.API.Controllers.v1;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await authService.LoginAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        if (result is null)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAt);
        return Ok(result);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        await authService.RequestPasswordResetAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        return NoContent();
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        var reset = await authService.ResetPasswordAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        if (!reset)
        {
            return BadRequest(new { message = "Reset token is invalid or expired." });
        }

        return NoContent();
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var token = request.RefreshToken;
        if (string.IsNullOrWhiteSpace(token) && Request.Cookies.TryGetValue("erp_refresh_token", out var cookieToken))
        {
            token = cookieToken;
        }

        await authService.LogoutAsync(userId.Value, token, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);

        Response.Cookies.Delete("erp_refresh_token");
        return NoContent();
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken) && Request.Cookies.TryGetValue("erp_refresh_token", out var cookieToken))
        {
            request.RefreshToken = cookieToken;
        }

        var result = await authService.RefreshTokenAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        if (result is null)
        {
            return Unauthorized(new { message = "Invalid refresh token." });
        }

        SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAt);
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var user = await authService.GetCurrentUserAsync(userId.Value, ct);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPut("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var changed = await authService.ChangePasswordAsync(userId.Value, request, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        if (!changed)
        {
            return BadRequest(new { message = "Current password is invalid." });
        }

        return NoContent();
    }

    private int? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }

    private void SetRefreshTokenCookie(string refreshToken, DateTimeOffset expires)
    {
        Response.Cookies.Append("erp_refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = expires
        });
    }
}

