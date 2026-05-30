using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceCurrenciesIndexViewModel : PagedGridStateViewModel
{
    public FinanceCurrenciesIndexViewModel()
    {
        SortBy = "code";
        SortDirection = "asc";
    }

    public string? CodeFilter { get; set; }
    public string? NameFilter { get; set; }
    public string? SymbolFilter { get; set; }
    public bool? IsBaseCurrencyFilter { get; set; }
    public bool? IsActiveFilter { get; set; }

    public PagedResult<CurrencyDto> Items { get; set; } = PagedResult<CurrencyDto>.Create([], 0, 1, 20);
}
