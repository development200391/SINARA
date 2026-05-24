using System.Net.Http.Headers;
using System.Net.Http.Json;
using ERP.Application.DTOs.Auth;

namespace ERP.Web.Services;

public sealed class AuthApiClient(HttpClient httpClient, ILogger<AuthApiClient> logger) : IAuthApiClient
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/v1/auth/login", request, ct);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to reach auth API on login.");
            return null;
        }
    }

    public async Task LogoutAsync(string? refreshToken, string? accessToken, CancellationToken ct = default)
    {
        try
        {
            using var message = new HttpRequestMessage(HttpMethod.Post, "api/v1/auth/logout")
            {
                Content = JsonContent.Create(new RefreshTokenRequest { RefreshToken = refreshToken })
            };

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            var response = await httpClient.SendAsync(message, ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Logout API returned non-success status code: {StatusCode}", response.StatusCode);
            }
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Failed to reach auth API on logout.");
        }
    }
}
