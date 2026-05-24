namespace ERP.Domain.Entities.Config;

public sealed class CfgRoleMenuPermission
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public int MenuId { get; set; }
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }

    public CfgRole Role { get; set; } = null!;
    public CfgMenu Menu { get; set; } = null!;
}
