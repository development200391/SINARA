using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;
using ERP.Domain.Enums;

namespace ERP.Web.ViewModels.HR;

public sealed class HrLeaveRequestsIndexViewModel
{
    public string? Search { get; set; }
    public LeaveStatus? Status { get; set; }
    public PagedResult<LeaveRequestDto> Requests { get; set; } = PagedResult<LeaveRequestDto>.Create([], 0, 1, 20);
}
