using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceCustomersIndexViewModel : PagedGridStateViewModel
{
    public FinanceCustomersIndexViewModel()
    {
        SortBy = "code";
        SortDirection = "asc";
    }

    public string? CodeFilter { get; set; }
    public string? NameFilter { get; set; }
    public string? TaxIdFilter { get; set; }
    public string? ContactPersonFilter { get; set; }
    public decimal? CreditLimitFromFilter { get; set; }
    public decimal? CreditLimitToFilter { get; set; }
    public int? PaymentTermsFromFilter { get; set; }
    public int? PaymentTermsToFilter { get; set; }
    public bool? IsActiveFilter { get; set; }

    public PagedResult<CustomerDto> Items { get; set; } = PagedResult<CustomerDto>.Create([], 0, 1, 20);
}
