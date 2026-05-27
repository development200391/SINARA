using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;
using ERP.Domain.Enums;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.HR;

public sealed class HrEmployeesIndexViewModel : PagedGridStateViewModel
{
    public int? DepartmentId { get; set; }
    public EmploymentStatus? EmploymentStatus { get; set; }
    public IReadOnlyList<DepartmentDto> Departments { get; set; } = [];
    public PagedResult<EmployeeListDto> Employees { get; set; } = PagedResult<EmployeeListDto>.Create([], 0, 1, 20);
}
