using ERP.Application.DTOs.HR;

namespace ERP.Web.ViewModels.HR;

public sealed class HrPayrollDetailsViewModel
{
    public int RunId { get; set; }
    public IReadOnlyList<PayrollRunDetailDto> Details { get; set; } = [];
}
