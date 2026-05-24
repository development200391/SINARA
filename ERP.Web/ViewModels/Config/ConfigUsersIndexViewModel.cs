using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Config;

namespace ERP.Web.ViewModels.Config;

public sealed class ConfigUsersIndexViewModel
{
    public string? Search { get; set; }
    public PagedResult<UserDto> Users { get; set; } = PagedResult<UserDto>.Create([], 0, 1, 20);
}
