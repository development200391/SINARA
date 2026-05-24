using System.ComponentModel.DataAnnotations;

namespace ERP.Web.ViewModels.HR;

public sealed class HrPayrollRunViewModel
{
    [Range(1, 12)]
    public int Month { get; set; } = DateTime.Today.Month;

    [Range(2000, 9999)]
    public int Year { get; set; } = DateTime.Today.Year;
}
