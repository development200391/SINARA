using System.ComponentModel.DataAnnotations;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceBudgetEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(30)]
    [Display(Name = "Budget No")]
    public string BudgetNo { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    [Display(Name = "Fiscal Year")]
    public int FiscalYearId { get; set; }

    [Display(Name = "Period")]
    public int? PeriodId { get; set; }

    [Display(Name = "Cost Center")]
    public int? CostCenterId { get; set; }

    [Display(Name = "Account")]
    public int? AccountId { get; set; }

    [Required]
    [MaxLength(10)]
    [Display(Name = "Currency")]
    public string CurrencyCode { get; set; } = "IDR";

    [MaxLength(1000)]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public IList<FinanceBudgetLineEditViewModel> Lines { get; set; } =
    [
        new FinanceBudgetLineEditViewModel()
    ];

    public IReadOnlyList<FinanceIdOptionViewModel> FiscalYearOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> PeriodOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> CostCenterOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> AccountOptions { get; set; } = [];
    public IReadOnlyList<FinanceCodeOptionViewModel> CurrencyOptions { get; set; } = [];
}

public sealed class FinanceBudgetLineEditViewModel
{
    public int? Id { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Period")]
    public int PeriodId { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Account")]
    public int AccountId { get; set; }

    [Display(Name = "Cost Center")]
    public int? CostCenterId { get; set; }

    [MaxLength(200)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    [Display(Name = "Amount")]
    public decimal Amount { get; set; }
}
