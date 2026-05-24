using ERP.Application.DTOs.Auth;

namespace ERP.Application.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken ct = default);
    Task LogoutAsync(int userId, string? refreshToken, string? ipAddress, CancellationToken ct = default);
    Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress, CancellationToken ct = default);
    Task<AuthUserDto?> GetCurrentUserAsync(int userId, CancellationToken ct = default);
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request, string? ipAddress, CancellationToken ct = default);
}
