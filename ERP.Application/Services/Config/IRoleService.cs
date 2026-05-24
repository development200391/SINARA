using ERP.Application.DTOs.Config;

namespace ERP.Application.Services.Config;

public interface IRoleService
{
    Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken ct = default);
    Task<PermissionMatrixDto?> GetPermissionsAsync(int roleId, CancellationToken ct = default);
    Task<bool> UpdatePermissionsAsync(int roleId, PermissionMatrixDto request, CancellationToken ct = default);
}
