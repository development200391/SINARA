using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;

namespace ERP.Web.ViewModels.HR;

public sealed class HrLeaveTypesIndexViewModel
{
    public string? Search { get; set; }
    public PagedResult<LeaveTypeDto> LeaveTypes { get; set; } = PagedResult<LeaveTypeDto>.Create([], 0, 1, 20);
}
