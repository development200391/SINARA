using System.ComponentModel.DataAnnotations;
using ERP.Domain.Enums;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceTaxCodeEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Tax Type")]
    public FinanceTaxType Type { get; set; }

    [Range(0, 100)]
    public decimal Rate { get; set; }

    [Display(Name = "Inclusive")]
    public bool IsInclusive { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Account")]
    public int AccountId { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public IReadOnlyList<FinanceIdOptionViewModel> AccountOptions { get; set; } = [];
}
