using ERP.Domain.Entities.Config;

namespace ERP.Domain.Entities.System;

public sealed class SysUserRole
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int RoleId { get; set; }

    public SysUser User { get; set; } = null!;
    public CfgRole Role { get; set; } = null!;
}
