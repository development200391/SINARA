using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.HR;

public sealed class HrPositionsIndexViewModel : PagedGridStateViewModel
{
    public int? DepartmentId { get; set; }
    public bool? IsActive { get; set; }
    public IReadOnlyList<DepartmentDto> Departments { get; set; } = [];
    public PagedResult<PositionDto> Positions { get; set; } = PagedResult<PositionDto>.Create([], 0, 1, 20);
}
