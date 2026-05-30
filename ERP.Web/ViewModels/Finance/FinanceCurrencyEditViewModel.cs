using System.ComponentModel.DataAnnotations;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceCurrencyEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(10)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string Symbol { get; set; } = string.Empty;

    [Display(Name = "Base Currency")]
    public bool IsBaseCurrency { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}
