using System.Net.Http.Headers;
using System.Net.Http.Json;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Config;

namespace ERP.Web.Services;

public sealed class ConfigApiClient(HttpClient httpClient, ILogger<ConfigApiClient> logger) : IConfigApiClient
{
    public async Task<IReadOnlyList<NavigationModuleDto>> GetNavigationAsync(string accessToken, CancellationToken ct = default)
    {
        return await SendAsync<IReadOnlyList<NavigationModuleDto>>(HttpMethod.Get, "api/v1/config/navigation", accessToken, null, ct)
            ?? [];
    }

    public Task<PagedResult<UserDto>?> GetUsersAsync(string accessToken, PagedRequest request, CancellationToken ct = default)
    {
        var query = $"api/v1/config/users?page={request.Page}&pageSize={request.PageSize}&search={Uri.EscapeDataString(request.Search ?? string.Empty)}";
        return SendAsync<PagedResult<UserDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<UserDto?> GetUserByIdAsync(string accessToken, int id, CancellationToken ct = default)
    {
        return SendAsync<UserDto>(HttpMethod.Get, $"api/v1/config/users/{id}", accessToken, null, ct);
    }

    public Task<UserDto?> CreateUserAsync(string accessToken, UserDto request, CancellationToken ct = default)
    {
        return SendAsync<UserDto>(HttpMethod.Post, "api/v1/config/users", accessToken, request, ct);
    }

    public Task<UserDto?> UpdateUserAsync(string accessToken, int id, UserDto request, CancellationToken ct = default)
    {
        return SendAsync<UserDto>(HttpMethod.Put, $"api/v1/config/users/{id}", accessToken, request, ct);
    }

    public async Task<bool> DeleteUserAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/config/users/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public async Task<IReadOnlyList<RoleDto>> GetRolesAsync(string accessToken, CancellationToken ct = default)
    {
        return await SendAsync<IReadOnlyList<RoleDto>>(HttpMethod.Get, "api/v1/config/roles", accessToken, null, ct) ?? [];
    }

    public Task<PermissionMatrixDto?> GetRolePermissionsAsync(string accessToken, int roleId, CancellationToken ct = default)
    {
        return SendAsync<PermissionMatrixDto>(HttpMethod.Get, $"api/v1/config/roles/{roleId}/permissions", accessToken, null, ct);
    }

    public async Task<bool> UpdateRolePermissionsAsync(string accessToken, int roleId, PermissionMatrixDto request, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Put, $"api/v1/config/roles/{roleId}/permissions", accessToken, request, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public async Task<IReadOnlyList<ModuleDto>> GetModulesAsync(string accessToken, CancellationToken ct = default)
    {
        return await SendAsync<IReadOnlyList<ModuleDto>>(HttpMethod.Get, "api/v1/config/modules", accessToken, null, ct) ?? [];
    }

    public Task<ModuleDto?> UpdateModuleAsync(string accessToken, int id, ModuleDto request, CancellationToken ct = default)
    {
        return SendAsync<ModuleDto>(HttpMethod.Put, $"api/v1/config/modules/{id}", accessToken, request, ct);
    }

    public async Task<IReadOnlyList<MenuDto>> GetMenusByModuleAsync(string accessToken, int moduleId, CancellationToken ct = default)
    {
        return await SendAsync<IReadOnlyList<MenuDto>>(HttpMethod.Get, $"api/v1/config/menus?moduleId={moduleId}", accessToken, null, ct) ?? [];
    }

    public Task<MenuDto?> GetMenuByIdAsync(string accessToken, int id, CancellationToken ct = default)
    {
        return SendAsync<MenuDto>(HttpMethod.Get, $"api/v1/config/menus/{id}", accessToken, null, ct);
    }

    public Task<MenuDto?> CreateMenuAsync(string accessToken, MenuDto request, CancellationToken ct = default)
    {
        return SendAsync<MenuDto>(HttpMethod.Post, "api/v1/config/menus", accessToken, request, ct);
    }

    public Task<MenuDto?> UpdateMenuAsync(string accessToken, int id, MenuDto request, CancellationToken ct = default)
    {
        return SendAsync<MenuDto>(HttpMethod.Put, $"api/v1/config/menus/{id}", accessToken, request, ct);
    }

    public async Task<bool> DeleteMenuAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/config/menus/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public async Task<bool> ReorderMenusAsync(string accessToken, int moduleId, IReadOnlyList<int> orderedMenuIds, CancellationToken ct = default)
    {
        var payload = new { moduleId, orderedMenuIds };
        var response = await SendRawAsync(HttpMethod.Put, "api/v1/config/menus/reorder", accessToken, payload, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<AuditLogDto>?> GetAuditLogsAsync(string accessToken, PagedRequest request, CancellationToken ct = default)
    {
        var query = $"api/v1/config/audit-logs?page={request.Page}&pageSize={request.PageSize}&search={Uri.EscapeDataString(request.Search ?? string.Empty)}";
        return SendAsync<PagedResult<AuditLogDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<AppSettingsDto?> GetSettingsAsync(string accessToken, CancellationToken ct = default)
    {
        return SendAsync<AppSettingsDto>(HttpMethod.Get, "api/v1/config/settings", accessToken, null, ct);
    }

    public Task<AppSettingsDto?> UpdateSettingsAsync(string accessToken, AppSettingsDto request, CancellationToken ct = default)
    {
        return SendAsync<AppSettingsDto>(HttpMethod.Put, "api/v1/config/settings", accessToken, request, ct);
    }

    public async Task<IReadOnlyList<LanguageDto>> GetLanguagesAsync(string accessToken, CancellationToken ct = default)
    {
        return await SendAsync<IReadOnlyList<LanguageDto>>(HttpMethod.Get, "api/v1/config/languages", accessToken, null, ct) ?? [];
    }

    private async Task<T?> SendAsync<T>(HttpMethod method, string uri, string accessToken, object? body, CancellationToken ct)
    {
        var response = await SendRawAsync(method, uri, accessToken, body, ct);
        if (response is null || !response.IsSuccessStatusCode)
        {
            return default;
        }

        try
        {
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to deserialize response from {Uri}", uri);
            return default;
        }
    }

    private async Task<HttpResponseMessage?> SendRawAsync(HttpMethod method, string uri, string accessToken, object? body, CancellationToken ct)
    {
        try
        {
            using var request = new HttpRequestMessage(method, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            if (body is not null)
            {
                request.Content = JsonContent.Create(body);
            }

            return await httpClient.SendAsync(request, ct);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Failed to call config API endpoint {Uri}", uri);
            return null;
        }
    }
}
