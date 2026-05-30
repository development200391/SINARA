using System.ComponentModel.DataAnnotations;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceCostCenterEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Department")]
    public int? DepartmentId { get; set; }

    [Display(Name = "Manager")]
    public int? ManagerId { get; set; }

    [Display(Name = "Budget Account")]
    public int? BudgetAccountId { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public IReadOnlyList<FinanceIdOptionViewModel> DepartmentOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> ManagerOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> BudgetAccountOptions { get; set; } = [];
}
