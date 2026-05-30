using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Enums;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinancePeriodsIndexViewModel : PagedGridStateViewModel
{
    public FinancePeriodsIndexViewModel()
    {
        SortBy = "startDate";
        SortDirection = "desc";
    }

    public int? FiscalYearIdFilter { get; set; }
    public int? PeriodNumberFromFilter { get; set; }
    public int? PeriodNumberToFilter { get; set; }
    public FinancePeriodStatus? StatusFilter { get; set; }
    public DateOnly? StartDateFromFilter { get; set; }
    public DateOnly? StartDateToFilter { get; set; }

    public IReadOnlyList<FinanceIdOptionViewModel> FiscalYearOptions { get; set; } = [];

    public PagedResult<PeriodDto> Items { get; set; } = PagedResult<PeriodDto>.Create([], 0, 1, 20);
}
