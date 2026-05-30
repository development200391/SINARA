using System.ComponentModel.DataAnnotations;
using ERP.Domain.Enums;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceAccountEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    [Display(Name = "Group")]
    public int GroupId { get; set; }

    [Display(Name = "Type")]
    public FinanceAccountType Type { get; set; }

    [Display(Name = "Normal Balance")]
    public FinanceNormalBalance NormalBalance { get; set; }

    [Display(Name = "Header Account")]
    public bool IsHeader { get; set; }

    [Display(Name = "Parent Account")]
    public int? ParentAccountId { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Display(Name = "Bank Account")]
    public bool IsBankAccount { get; set; }

    [MaxLength(100)]
    [Display(Name = "Bank Name")]
    public string? BankName { get; set; }

    [MaxLength(50)]
    [Display(Name = "Bank Account No")]
    public string? BankAccountNo { get; set; }

    [Required]
    [MaxLength(10)]
    [Display(Name = "Currency")]
    public string CurrencyCode { get; set; } = "IDR";

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public IReadOnlyList<FinanceIdOptionViewModel> GroupOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> ParentAccountOptions { get; set; } = [];
    public IReadOnlyList<FinanceCodeOptionViewModel> CurrencyOptions { get; set; } = [];
}
