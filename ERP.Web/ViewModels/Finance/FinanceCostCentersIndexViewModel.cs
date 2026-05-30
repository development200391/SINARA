using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceCostCentersIndexViewModel : PagedGridStateViewModel
{
    public FinanceCostCentersIndexViewModel()
    {
        SortBy = "code";
        SortDirection = "asc";
    }

    public string? CodeFilter { get; set; }
    public string? NameFilter { get; set; }
    public int? DepartmentIdFilter { get; set; }
    public int? ManagerIdFilter { get; set; }
    public int? BudgetAccountIdFilter { get; set; }
    public bool? IsActiveFilter { get; set; }

    public IReadOnlyList<FinanceIdOptionViewModel> DepartmentOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> ManagerOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> BudgetAccountOptions { get; set; } = [];

    public PagedResult<CostCenterDto> Items { get; set; } = PagedResult<CostCenterDto>.Create([], 0, 1, 20);
}
