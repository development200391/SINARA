using System.ComponentModel.DataAnnotations;
using ERP.Domain.Enums;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceAccountGroupEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Display(Name = "Type")]
    public FinanceAccountType Type { get; set; }

    [Display(Name = "Normal Balance")]
    public FinanceNormalBalance NormalBalance { get; set; }

    [Display(Name = "Parent Group")]
    public int? ParentGroupId { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Sort Order")]
    public int SortOrder { get; set; } = 1;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public IReadOnlyList<FinanceIdOptionViewModel> ParentGroupOptions { get; set; } = [];
}
