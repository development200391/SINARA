using ERP.Application.DTOs.Auth;

namespace ERP.Web.Services;

public interface IAuthApiClient
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task LogoutAsync(string? refreshToken, string? accessToken, CancellationToken ct = default);
    Task<ApiCallResult<object?>> ChangePasswordAsync(ChangePasswordRequest request, string accessToken, CancellationToken ct = default);
}
