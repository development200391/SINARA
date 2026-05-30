using System.ComponentModel.DataAnnotations;
using ERP.Domain.Enums;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceFiscalYearEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Start Date")]
    public DateOnly StartDate { get; set; } = new(DateTime.Today.Year, 1, 1);

    [Display(Name = "End Date")]
    public DateOnly EndDate { get; set; } = new(DateTime.Today.Year, 12, 31);

    [Display(Name = "Status")]
    public FinancePeriodStatus Status { get; set; } = FinancePeriodStatus.Open;
}
