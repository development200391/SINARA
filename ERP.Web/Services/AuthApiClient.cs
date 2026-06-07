using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
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

    public async Task<ApiCallResult<object?>> ChangePasswordAsync(ChangePasswordRequest request, string accessToken, CancellationToken ct = default)
    {
        try
        {
            using var message = new HttpRequestMessage(HttpMethod.Put, "api/v1/auth/change-password")
            {
                Content = JsonContent.Create(request)
            };

            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.SendAsync(message, ct);
            var statusCode = (int)response.StatusCode;
            if (response.IsSuccessStatusCode)
            {
                return ApiCallResult<object?>.Success(null, statusCode);
            }

            var apiError = await ReadApiErrorMessageAsync(response, ct);
            var errorMessage = string.IsNullOrWhiteSpace(apiError)
                ? $"Change password API returned status {statusCode} ({response.StatusCode})."
                : apiError;

            return ApiCallResult<object?>.Failure(errorMessage, statusCode);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Failed to reach auth API on change password.");
            return ApiCallResult<object?>.Failure("Failed to reach auth API on change password.");
        }
    }

    private static async Task<string?> ReadApiErrorMessageAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.Content is null)
        {
            return null;
        }

        string rawContent;
        try
        {
            rawContent = await response.Content.ReadAsStringAsync(ct);
        }
        catch
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(rawContent))
        {
            return null;
        }

        var trimmedContent = rawContent.Trim();

        try
        {
            using var document = JsonDocument.Parse(trimmedContent);
            var root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Object)
            {
                if (TryGetString(root, "message", out var message))
                {
                    return message;
                }

                if (TryGetString(root, "error", out var error))
                {
                    return error;
                }

                if (TryGetString(root, "title", out var title))
                {
                    return title;
                }

                if (TryGetString(root, "detail", out var detail))
                {
                    return detail;
                }
            }
        }
        catch (JsonException)
        {
        }

        return trimmedContent;
    }

    private static bool TryGetString(JsonElement root, string propertyName, out string value)
    {
        value = string.Empty;
        if (!root.TryGetProperty(propertyName, out var element) || element.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        var raw = element.GetString();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        value = raw;
        return true;
    }
}
