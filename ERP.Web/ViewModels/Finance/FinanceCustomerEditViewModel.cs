using System.ComponentModel.DataAnnotations;
using ERP.Web.Validation;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceCustomerEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(20)]
    [Display(Name = "Code")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(30)]
    [Display(Name = "Tax ID")]
    public string? TaxId { get; set; }

    [MaxLength(1000)]
    [Display(Name = "Address")]
    public string? Address { get; set; }

    [MaxLength(30)]
    [Display(Name = "Phone")]
    public string? Phone { get; set; }

    [MaxLength(200)]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [MaxLength(100)]
    [Display(Name = "Contact Person")]
    public string? ContactPerson { get; set; }

    [DecimalRange("0", "79228162514264337593543950335")]
    [Display(Name = "Credit Limit")]
    public decimal CreditLimit { get; set; }

    [Range(0, 3650)]
    [Display(Name = "Payment Terms (Days)")]
    public int PaymentTermsDays { get; set; } = 30;

    [Display(Name = "Default Account")]
    public int? DefaultAccountId { get; set; }

    [Display(Name = "Default Tax Code")]
    public int? DefaultTaxCodeId { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public IReadOnlyList<FinanceIdOptionViewModel> AccountOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> TaxCodeOptions { get; set; } = [];
}
