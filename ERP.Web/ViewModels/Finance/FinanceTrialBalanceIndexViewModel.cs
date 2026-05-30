using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Enums;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceTrialBalanceIndexViewModel : PagedGridStateViewModel
{
    public FinanceTrialBalanceIndexViewModel()
    {
        SortBy = "accountcode";
        SortDirection = "asc";
    }

    public int? PeriodIdFilter { get; set; }
    public DateOnly? DateFromFilter { get; set; }
    public DateOnly? DateToFilter { get; set; }
    public int? AccountIdFilter { get; set; }
    public int? CostCenterIdFilter { get; set; }
    public FinanceAccountType? TypeFilter { get; set; }

    public IReadOnlyList<FinanceIdOptionViewModel> PeriodOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> AccountOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> CostCenterOptions { get; set; } = [];

    public PagedResult<TrialBalanceRowDto> Items { get; set; } = PagedResult<TrialBalanceRowDto>.Create([], 0, 1, 20);
}
