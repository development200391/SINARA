using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;
using ERP.Domain.Enums;

namespace ERP.Web.ViewModels.HR;

public sealed class HrPayrollIndexViewModel
{
    public string? Search { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
    public PayrollStatus? Status { get; set; }
    public PagedResult<PayrollRunDto> Runs { get; set; } = PagedResult<PayrollRunDto>.Create([], 0, 1, 20);
}
