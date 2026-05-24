namespace ERP.Application.DTOs.Config;

public sealed class PermissionMatrixDto
{
    public int RoleId { get; set; }
    public List<MenuPermissionDto> Permissions { get; set; } = [];
}

public sealed class MenuPermissionDto
{
    public int MenuId { get; set; }
    public string MenuName { get; set; } = string.Empty;
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}
