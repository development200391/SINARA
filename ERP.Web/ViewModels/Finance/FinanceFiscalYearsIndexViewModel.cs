using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Enums;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceFiscalYearsIndexViewModel : PagedGridStateViewModel
{
    public FinanceFiscalYearsIndexViewModel()
    {
        SortBy = "startDate";
        SortDirection = "desc";
    }

    public string? NameFilter { get; set; }
    public DateOnly? StartDateFromFilter { get; set; }
    public DateOnly? StartDateToFilter { get; set; }
    public DateOnly? EndDateFromFilter { get; set; }
    public DateOnly? EndDateToFilter { get; set; }
    public FinancePeriodStatus? StatusFilter { get; set; }

    public PagedResult<FiscalYearDto> Items { get; set; } = PagedResult<FiscalYearDto>.Create([], 0, 1, 20);
}
