using ERP.Web.Validation;
using System.ComponentModel.DataAnnotations;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceExchangeRateCreateViewModel
{
    [Required]
    [MaxLength(10)]
    [Display(Name = "From Currency")]
    public string FromCurrencyCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    [Display(Name = "To Currency")]
    public string ToCurrencyCode { get; set; } = string.Empty;

    [DecimalRange("0.000001", "79228162514264337593543950335")]
    public decimal Rate { get; set; }

    [Display(Name = "Effective Date")]
    public DateOnly EffectiveDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [MaxLength(100)]
    public string? Source { get; set; }

    public IReadOnlyList<FinanceCodeOptionViewModel> CurrencyOptions { get; set; } = [];
}
