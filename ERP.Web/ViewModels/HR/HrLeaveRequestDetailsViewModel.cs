using ERP.Application.DTOs.Document;
using ERP.Application.DTOs.HR;

namespace ERP.Web.ViewModels.HR;

public sealed class HrLeaveRequestDetailsViewModel
{
    public LeaveRequestDto Request { get; set; } = new();
    public IReadOnlyList<DocumentDto> Documents { get; set; } = [];
}
