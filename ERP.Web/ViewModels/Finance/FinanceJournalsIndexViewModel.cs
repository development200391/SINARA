using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Enums;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceJournalsIndexViewModel : PagedGridStateViewModel
{
    public FinanceJournalsIndexViewModel()
    {
        SortBy = "date";
        SortDirection = "desc";
    }

    public string? JournalNoFilter { get; set; }
    public DateOnly? DateFromFilter { get; set; }
    public DateOnly? DateToFilter { get; set; }
    public FinanceJournalSource? SourceFilter { get; set; }
    public FinanceJournalStatus? StatusFilter { get; set; }
    public int? PeriodIdFilter { get; set; }
    public string? SourceRefTypeFilter { get; set; }

    public IReadOnlyList<FinanceIdOptionViewModel> PeriodOptions { get; set; } = [];

    public PagedResult<JournalEntryDto> Items { get; set; } = PagedResult<JournalEntryDto>.Create([], 0, 1, 20);
}
