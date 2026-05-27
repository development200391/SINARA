using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Config;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.Config;

public sealed class ConfigUsersIndexViewModel : PagedGridStateViewModel
{
    public ConfigUsersIndexViewModel()
    {
        SortBy = "username";
        SortDirection = "asc";
    }

    public string? UsernameFilter { get; set; }
    public string? FullNameFilter { get; set; }
    public string? EmailFilter { get; set; }

    public PagedResult<UserDto> Users { get; set; } = PagedResult<UserDto>.Create([], 0, 1, 20);
}
