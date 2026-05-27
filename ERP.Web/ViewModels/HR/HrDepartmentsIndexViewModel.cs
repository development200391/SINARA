using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.HR;

public sealed class HrDepartmentsIndexViewModel : PagedGridStateViewModel
{
    public bool? IsActive { get; set; }
    public PagedResult<DepartmentDto> Departments { get; set; } = PagedResult<DepartmentDto>.Create([], 0, 1, 20);
}
