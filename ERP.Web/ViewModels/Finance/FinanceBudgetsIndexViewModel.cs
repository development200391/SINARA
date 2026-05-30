using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceBudgetsIndexViewModel : PagedGridStateViewModel
{
    public FinanceBudgetsIndexViewModel()
    {
        SortBy = "budgetno";
        SortDirection = "asc";
    }

    public string? BudgetNoFilter { get; set; }
    public string? NameFilter { get; set; }
    public int? FiscalYearIdFilter { get; set; }
    public int? PeriodIdFilter { get; set; }
    public int? CostCenterIdFilter { get; set; }
    public int? AccountIdFilter { get; set; }
    public bool? IsActiveFilter { get; set; }
    public decimal? AmountFromFilter { get; set; }
    public decimal? AmountToFilter { get; set; }
    public decimal? ActualFromFilter { get; set; }
    public decimal? ActualToFilter { get; set; }
    public decimal? VarianceFromFilter { get; set; }
    public decimal? VarianceToFilter { get; set; }

    public IReadOnlyList<FinanceIdOptionViewModel> FiscalYearOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> PeriodOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> CostCenterOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> AccountOptions { get; set; } = [];

    public PagedResult<BudgetDto> Items { get; set; } = PagedResult<BudgetDto>.Create([], 0, 1, 20);
}
