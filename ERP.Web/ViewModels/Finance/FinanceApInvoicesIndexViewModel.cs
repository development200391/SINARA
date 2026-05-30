using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Enums;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceApInvoicesIndexViewModel : PagedGridStateViewModel
{
    public FinanceApInvoicesIndexViewModel()
    {
        SortBy = "invoicedate";
        SortDirection = "desc";
    }

    public string? InvoiceNoFilter { get; set; }
    public string? VendorInvoiceNoFilter { get; set; }
    public int? VendorIdFilter { get; set; }
    public int? PeriodIdFilter { get; set; }
    public DateOnly? InvoiceDateFromFilter { get; set; }
    public DateOnly? InvoiceDateToFilter { get; set; }
    public DateOnly? DueDateFromFilter { get; set; }
    public DateOnly? DueDateToFilter { get; set; }
    public FinanceApInvoiceStatus? StatusFilter { get; set; }
    public decimal? OutstandingFromFilter { get; set; }
    public decimal? OutstandingToFilter { get; set; }
    public bool? IsOverdueFilter { get; set; }

    public IReadOnlyList<FinanceIdOptionViewModel> VendorOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> PeriodOptions { get; set; } = [];

    public PagedResult<ApInvoiceDto> Items { get; set; } = PagedResult<ApInvoiceDto>.Create([], 0, 1, 20);
}
