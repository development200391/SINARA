using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Config;

namespace ERP.Web.Services;

public sealed class ConfigApiClient(HttpClient httpClient, ILogger<ConfigApiClient> logger) : ApiClientBase(httpClient, logger, "Config"), IConfigApiClient
{
    public async Task<IReadOnlyList<NavigationModuleDto>> GetNavigationAsync(string accessToken, CancellationToken ct = default)
    {
        return await SendWithResultAsync<IReadOnlyList<NavigationModuleDto>>(HttpMethod.Get, "api/v1/config/navigation", accessToken, null, ct).ToDataAsync() ?? [];
    }

    public Task<PagedResult<UserDto>?> GetUsersAsync(string accessToken, UserPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>
        {
            $"page={request.Page}",
            $"pageSize={request.PageSize}",
            $"search={Uri.EscapeDataString(request.Search ?? string.Empty)}",
            $"sortBy={Uri.EscapeDataString(request.SortBy ?? string.Empty)}",
            $"sortDirection={Uri.EscapeDataString(request.SortDirection ?? string.Empty)}"
        };

        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            parameters.Add($"username={Uri.EscapeDataString(request.Username.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            parameters.Add($"fullName={Uri.EscapeDataString(request.FullName.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            parameters.Add($"email={Uri.EscapeDataString(request.Email.Trim())}");
        }

        var query = $"api/v1/config/users?{string.Join("&", parameters)}";
        return SendWithResultAsync<PagedResult<UserDto>>(HttpMethod.Get, query, accessToken, null, ct).ToDataAsync();
    }

    public Task<UserDto?> GetUserByIdAsync(string accessToken, int id, CancellationToken ct = default)
    {
        return SendWithResultAsync<UserDto>(HttpMethod.Get, $"api/v1/config/users/{id}", accessToken, null, ct).ToDataAsync();
    }

    public Task<ApiCallResult<UserDto>> CreateUserAsync(string accessToken, UserDto request, CancellationToken ct = default)
    {
        return SendWithResultAsync<UserDto>(HttpMethod.Post, "api/v1/config/users", accessToken, request, ct);
    }

    public Task<ApiCallResult<UserDto>> UpdateUserAsync(string accessToken, int id, UserDto request, CancellationToken ct = default)
    {
        return SendWithResultAsync<UserDto>(HttpMethod.Put, $"api/v1/config/users/{id}", accessToken, request, ct);
    }

    public Task<ApiCallResult<object?>> DeleteUserAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Delete, $"api/v1/config/users/{id}", accessToken, null, ct);

    public async Task<IReadOnlyList<RoleDto>> GetRolesAsync(string accessToken, CancellationToken ct = default)
    {
        return await SendWithResultAsync<IReadOnlyList<RoleDto>>(HttpMethod.Get, "api/v1/config/roles", accessToken, null, ct).ToDataAsync() ?? [];
    }

    public Task<PermissionMatrixDto?> GetRolePermissionsAsync(string accessToken, int roleId, CancellationToken ct = default)
    {
        return SendWithResultAsync<PermissionMatrixDto>(HttpMethod.Get, $"api/v1/config/roles/{roleId}/permissions", accessToken, null, ct).ToDataAsync();
    }

    public Task<ApiCallResult<object?>> UpdateRolePermissionsAsync(string accessToken, int roleId, PermissionMatrixDto request, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Put, $"api/v1/config/roles/{roleId}/permissions", accessToken, request, ct);

    public async Task<IReadOnlyList<ModuleDto>> GetModulesAsync(string accessToken, CancellationToken ct = default)
    {
        return await SendWithResultAsync<IReadOnlyList<ModuleDto>>(HttpMethod.Get, "api/v1/config/modules", accessToken, null, ct).ToDataAsync() ?? [];
    }

    public Task<ApiCallResult<ModuleDto>> UpdateModuleAsync(string accessToken, int id, ModuleDto request, CancellationToken ct = default)
    {
        return SendWithResultAsync<ModuleDto>(HttpMethod.Put, $"api/v1/config/modules/{id}", accessToken, request, ct);
    }

    public async Task<IReadOnlyList<MenuDto>> GetMenusByModuleAsync(string accessToken, int moduleId, CancellationToken ct = default)
    {
        return await SendWithResultAsync<IReadOnlyList<MenuDto>>(HttpMethod.Get, $"api/v1/config/menus?moduleId={moduleId}", accessToken, null, ct).ToDataAsync() ?? [];
    }

    public Task<MenuDto?> GetMenuByIdAsync(string accessToken, int id, CancellationToken ct = default)
    {
        return SendWithResultAsync<MenuDto>(HttpMethod.Get, $"api/v1/config/menus/{id}", accessToken, null, ct).ToDataAsync();
    }

    public Task<ApiCallResult<MenuDto>> CreateMenuAsync(string accessToken, MenuDto request, CancellationToken ct = default)
    {
        return SendWithResultAsync<MenuDto>(HttpMethod.Post, "api/v1/config/menus", accessToken, request, ct);
    }

    public Task<ApiCallResult<MenuDto>> UpdateMenuAsync(string accessToken, int id, MenuDto request, CancellationToken ct = default)
    {
        return SendWithResultAsync<MenuDto>(HttpMethod.Put, $"api/v1/config/menus/{id}", accessToken, request, ct);
    }

    public Task<ApiCallResult<object?>> DeleteMenuAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Delete, $"api/v1/config/menus/{id}", accessToken, null, ct);

    public async Task<ApiCallResult<object?>> ReorderMenusAsync(string accessToken, int moduleId, IReadOnlyList<int> orderedMenuIds, CancellationToken ct = default)
    {
        var payload = new { moduleId, orderedMenuIds };
        var result = await SendWithResultAsync<object?>(HttpMethod.Put, "api/v1/config/menus/reorder", accessToken, payload, ct);
        return result;
    }

    public Task<PagedResult<AuditLogDto>?> GetAuditLogsAsync(string accessToken, AuditLogPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>
        {
            $"page={request.Page}",
            $"pageSize={request.PageSize}",
            $"search={Uri.EscapeDataString(request.Search ?? string.Empty)}",
            $"sortBy={Uri.EscapeDataString(request.SortBy ?? string.Empty)}",
            $"sortDirection={Uri.EscapeDataString(request.SortDirection ?? string.Empty)}",
            $"hasIpOnly={(request.HasIpOnly ? "true" : "false")}"
        };

        if (request.DateFrom.HasValue)
        {
            parameters.Add($"dateFrom={request.DateFrom.Value:yyyy-MM-dd}");
        }

        if (request.DateTo.HasValue)
        {
            parameters.Add($"dateTo={request.DateTo.Value:yyyy-MM-dd}");
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            parameters.Add($"status={Uri.EscapeDataString(request.Status.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(request.EntityNames))
        {
            parameters.Add($"entityNames={Uri.EscapeDataString(request.EntityNames)}");
        }

        var query = $"api/v1/config/audit-logs?{string.Join("&", parameters)}";
        return SendWithResultAsync<PagedResult<AuditLogDto>>(HttpMethod.Get, query, accessToken, null, ct).ToDataAsync();
    }

    public Task<AppSettingsDto?> GetSettingsAsync(string accessToken, CancellationToken ct = default)
    {
        return SendWithResultAsync<AppSettingsDto>(HttpMethod.Get, "api/v1/config/settings", accessToken, null, ct).ToDataAsync();
    }

    public Task<ApiCallResult<AppSettingsDto>> UpdateSettingsAsync(string accessToken, AppSettingsDto request, CancellationToken ct = default)
    {
        return SendWithResultAsync<AppSettingsDto>(HttpMethod.Put, "api/v1/config/settings", accessToken, request, ct);
    }

    public async Task<IReadOnlyList<LanguageDto>> GetLanguagesAsync(string accessToken, CancellationToken ct = default)
    {
        return await SendWithResultAsync<IReadOnlyList<LanguageDto>>(HttpMethod.Get, "api/v1/config/languages", accessToken, null, ct).ToDataAsync() ?? [];
    }
}


