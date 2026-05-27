using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;
using ERP.Domain.Enums;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.HR;

public sealed class HrEmployeesIndexViewModel : PagedGridStateViewModel
{
    public HrEmployeesIndexViewModel()
    {
        SortBy = "fullName";
        SortDirection = "asc";
    }

    public string? EmployeeCodeFilter { get; set; }
    public string? FullNameFilter { get; set; }
    public string? EmailFilter { get; set; }
    public string? PhoneFilter { get; set; }
    public int? DepartmentIdFilter { get; set; }
    public int? PositionIdFilter { get; set; }
    public EmploymentStatus? EmploymentStatusFilter { get; set; }
    public DateOnly? HireDateFrom { get; set; }
    public DateOnly? HireDateTo { get; set; }
    public DateOnly? TerminationDateFrom { get; set; }
    public DateOnly? TerminationDateTo { get; set; }

    public IReadOnlyList<DepartmentDto> Departments { get; set; } = [];
    public IReadOnlyList<PositionDto> Positions { get; set; } = [];

    public PagedResult<EmployeeListDto> Employees { get; set; } = PagedResult<EmployeeListDto>.Create([], 0, 1, 20);
}
