using ERP.Application.DTOs.Config;

namespace ERP.Web.ViewModels.Config;

public sealed class ConfigRolesIndexViewModel
{
    public IReadOnlyList<RoleDto> Roles { get; set; } = [];
}
