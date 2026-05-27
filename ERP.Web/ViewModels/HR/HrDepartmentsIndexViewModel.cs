using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.HR;

public sealed class HrDepartmentsIndexViewModel : PagedGridStateViewModel
{
    public HrDepartmentsIndexViewModel()
    {
        SortBy = "name";
        SortDirection = "asc";
    }

    public string? CodeFilter { get; set; }
    public string? NameFilter { get; set; }
    public int? ManagerIdFilter { get; set; }
    public int? ParentDepartmentIdFilter { get; set; }
    public bool? IsActiveFilter { get; set; }

    public IReadOnlyList<LookupDto> Managers { get; set; } = [];
    public IReadOnlyList<DepartmentDto> DepartmentOptions { get; set; } = [];

    public PagedResult<DepartmentDto> Departments { get; set; } = PagedResult<DepartmentDto>.Create([], 0, 1, 20);
}
