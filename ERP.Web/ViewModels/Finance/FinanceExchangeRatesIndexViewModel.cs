using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceExchangeRatesIndexViewModel : PagedGridStateViewModel
{
    public FinanceExchangeRatesIndexViewModel()
    {
        SortBy = "effectiveDate";
        SortDirection = "desc";
    }

    public string? FromCurrencyCodeFilter { get; set; }
    public string? ToCurrencyCodeFilter { get; set; }
    public DateOnly? EffectiveDateFromFilter { get; set; }
    public DateOnly? EffectiveDateToFilter { get; set; }
    public string? SourceFilter { get; set; }

    public IReadOnlyList<FinanceCodeOptionViewModel> CurrencyOptions { get; set; } = [];

    public PagedResult<ExchangeRateDto> Items { get; set; } = PagedResult<ExchangeRateDto>.Create([], 0, 1, 20);
}
