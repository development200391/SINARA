using ERP.Domain.Entities.System;

namespace ERP.Domain.Entities.Config;

public sealed class CfgMenu : BaseEntity
{
    public int ModuleId { get; set; }
    public int? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public CfgModule Module { get; set; } = null!;
    public CfgMenu? Parent { get; set; }
    public ICollection<CfgMenu> Children { get; set; } = new List<CfgMenu>();
    public ICollection<CfgRoleMenuPermission> RoleMenuPermissions { get; set; } = new List<CfgRoleMenuPermission>();
}
