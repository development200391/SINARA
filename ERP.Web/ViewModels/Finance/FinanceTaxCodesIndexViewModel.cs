using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Enums;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceTaxCodesIndexViewModel : PagedGridStateViewModel
{
    public FinanceTaxCodesIndexViewModel()
    {
        SortBy = "code";
        SortDirection = "asc";
    }

    public string? CodeFilter { get; set; }
    public string? NameFilter { get; set; }
    public FinanceTaxType? TypeFilter { get; set; }
    public decimal? RateFromFilter { get; set; }
    public decimal? RateToFilter { get; set; }
    public bool? IsInclusiveFilter { get; set; }
    public int? AccountIdFilter { get; set; }
    public bool? IsActiveFilter { get; set; }

    public IReadOnlyList<FinanceIdOptionViewModel> AccountOptions { get; set; } = [];

    public PagedResult<TaxCodeDto> Items { get; set; } = PagedResult<TaxCodeDto>.Create([], 0, 1, 20);
}
