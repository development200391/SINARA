using ERP.Application.DTOs.Config;

namespace ERP.Web.ViewModels.Config;

public sealed class ConfigRolePermissionsViewModel
{
    public RoleDto? Role { get; set; }
    public PermissionMatrixDto Matrix { get; set; } = new();
}
