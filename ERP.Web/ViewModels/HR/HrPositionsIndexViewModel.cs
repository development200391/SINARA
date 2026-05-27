using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.HR;

public sealed class HrPositionsIndexViewModel : PagedGridStateViewModel
{
    public HrPositionsIndexViewModel()
    {
        SortBy = "name";
        SortDirection = "asc";
    }

    public string? CodeFilter { get; set; }
    public string? NameFilter { get; set; }
    public int? DepartmentIdFilter { get; set; }
    public int? LevelFrom { get; set; }
    public int? LevelTo { get; set; }
    public bool? IsActiveFilter { get; set; }

    public IReadOnlyList<DepartmentDto> Departments { get; set; } = [];

    public PagedResult<PositionDto> Positions { get; set; } = PagedResult<PositionDto>.Create([], 0, 1, 20);
}
