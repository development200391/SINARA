using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Config;

namespace ERP.Web.Services;

public interface IConfigApiClient
{
    Task<IReadOnlyList<NavigationModuleDto>> GetNavigationAsync(string accessToken, CancellationToken ct = default);

    Task<PagedResult<UserDto>?> GetUsersAsync(string accessToken, UserPagedRequest request, CancellationToken ct = default);
    Task<UserDto?> GetUserByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<UserDto?> CreateUserAsync(string accessToken, UserDto request, CancellationToken ct = default);
    Task<UserDto?> UpdateUserAsync(string accessToken, int id, UserDto request, CancellationToken ct = default);
    Task<bool> DeleteUserAsync(string accessToken, int id, CancellationToken ct = default);

    Task<IReadOnlyList<RoleDto>> GetRolesAsync(string accessToken, CancellationToken ct = default);
    Task<PermissionMatrixDto?> GetRolePermissionsAsync(string accessToken, int roleId, CancellationToken ct = default);
    Task<bool> UpdateRolePermissionsAsync(string accessToken, int roleId, PermissionMatrixDto request, CancellationToken ct = default);

    Task<IReadOnlyList<ModuleDto>> GetModulesAsync(string accessToken, CancellationToken ct = default);
    Task<ModuleDto?> UpdateModuleAsync(string accessToken, int id, ModuleDto request, CancellationToken ct = default);

    Task<IReadOnlyList<MenuDto>> GetMenusByModuleAsync(string accessToken, int moduleId, CancellationToken ct = default);
    Task<MenuDto?> GetMenuByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<MenuDto?> CreateMenuAsync(string accessToken, MenuDto request, CancellationToken ct = default);
    Task<MenuDto?> UpdateMenuAsync(string accessToken, int id, MenuDto request, CancellationToken ct = default);
    Task<bool> DeleteMenuAsync(string accessToken, int id, CancellationToken ct = default);
    Task<bool> ReorderMenusAsync(string accessToken, int moduleId, IReadOnlyList<int> orderedMenuIds, CancellationToken ct = default);

    Task<PagedResult<AuditLogDto>?> GetAuditLogsAsync(string accessToken, AuditLogPagedRequest request, CancellationToken ct = default);

    Task<AppSettingsDto?> GetSettingsAsync(string accessToken, CancellationToken ct = default);
    Task<AppSettingsDto?> UpdateSettingsAsync(string accessToken, AppSettingsDto request, CancellationToken ct = default);

    Task<IReadOnlyList<LanguageDto>> GetLanguagesAsync(string accessToken, CancellationToken ct = default);
}
