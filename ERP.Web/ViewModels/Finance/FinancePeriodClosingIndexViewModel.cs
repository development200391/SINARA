using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Enums;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinancePeriodClosingIndexViewModel : PagedGridStateViewModel
{
    public FinancePeriodClosingIndexViewModel()
    {
        SortBy = "startdate";
        SortDirection = "asc";
    }

    public int? FiscalYearIdFilter { get; set; }
    public FinancePeriodStatus? StatusFilter { get; set; }
    public int? DraftJournalFromFilter { get; set; }
    public int? DraftJournalToFilter { get; set; }
    public int? PendingApFromFilter { get; set; }
    public int? PendingApToFilter { get; set; }
    public int? PendingArFromFilter { get; set; }
    public int? PendingArToFilter { get; set; }
    public decimal? NetIncomeLossFromFilter { get; set; }
    public decimal? NetIncomeLossToFilter { get; set; }

    public IReadOnlyList<FinanceIdOptionViewModel> FiscalYearOptions { get; set; } = [];

    public PagedResult<PeriodClosingRowDto> Items { get; set; } = PagedResult<PeriodClosingRowDto>.Create([], 0, 1, 20);
}