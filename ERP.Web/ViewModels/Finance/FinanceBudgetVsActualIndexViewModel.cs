using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceBudgetVsActualIndexViewModel : PagedGridStateViewModel
{
    public FinanceBudgetVsActualIndexViewModel()
    {
        SortBy = "budgetno";
        SortDirection = "asc";
    }

    public int? BudgetIdFilter { get; set; }
    public int? FiscalYearIdFilter { get; set; }
    public int? PeriodIdFilter { get; set; }
    public int? CostCenterIdFilter { get; set; }
    public int? AccountIdFilter { get; set; }
    public decimal? BudgetFromFilter { get; set; }
    public decimal? BudgetToFilter { get; set; }
    public decimal? ActualFromFilter { get; set; }
    public decimal? ActualToFilter { get; set; }
    public decimal? VarianceFromFilter { get; set; }
    public decimal? VarianceToFilter { get; set; }

    public IReadOnlyList<FinanceIdOptionViewModel> BudgetOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> FiscalYearOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> PeriodOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> CostCenterOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> AccountOptions { get; set; } = [];

    public PagedResult<BudgetVsActualRowDto> Items { get; set; } = PagedResult<BudgetVsActualRowDto>.Create([], 0, 1, 20);
}
