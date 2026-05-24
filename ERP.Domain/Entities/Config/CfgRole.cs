using ERP.Domain.Entities.System;

namespace ERP.Domain.Entities.Config;

public sealed class CfgRole : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<CfgRoleMenuPermission> RoleMenuPermissions { get; set; } = new List<CfgRoleMenuPermission>();
    public ICollection<SysUserRole> UserRoles { get; set; } = new List<SysUserRole>();
}
