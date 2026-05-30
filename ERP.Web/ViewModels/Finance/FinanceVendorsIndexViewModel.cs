using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceVendorsIndexViewModel : PagedGridStateViewModel
{
    public FinanceVendorsIndexViewModel()
    {
        SortBy = "code";
        SortDirection = "asc";
    }

    public string? CodeFilter { get; set; }
    public string? NameFilter { get; set; }
    public string? TaxIdFilter { get; set; }
    public string? ContactPersonFilter { get; set; }
    public int? PaymentTermsFromFilter { get; set; }
    public int? PaymentTermsToFilter { get; set; }
    public bool? IsActiveFilter { get; set; }

    public PagedResult<VendorDto> Items { get; set; } = PagedResult<VendorDto>.Create([], 0, 1, 20);
}
