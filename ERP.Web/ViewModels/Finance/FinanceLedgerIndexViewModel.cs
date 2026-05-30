using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceLedgerIndexViewModel : PagedGridStateViewModel
{
    public FinanceLedgerIndexViewModel()
    {
        SortBy = "accountcode";
        SortDirection = "asc";
    }

    public int? AccountIdFilter { get; set; }
    public int? PeriodIdFilter { get; set; }
    public int? CostCenterIdFilter { get; set; }
    public DateOnly? DateFromFilter { get; set; }
    public DateOnly? DateToFilter { get; set; }

    public IReadOnlyList<FinanceIdOptionViewModel> AccountOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> PeriodOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> CostCenterOptions { get; set; } = [];

    public PagedResult<LedgerEntryDto> Items { get; set; } = PagedResult<LedgerEntryDto>.Create([], 0, 1, 20);
}
