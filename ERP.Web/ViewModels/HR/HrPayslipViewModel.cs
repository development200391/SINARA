using ERP.Application.DTOs.HR;

namespace ERP.Web.ViewModels.HR;

public sealed class HrPayslipViewModel
{
    public PayslipDto Payslip { get; set; } = new();
}
