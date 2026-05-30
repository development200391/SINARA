using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Enums;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceApPaymentsIndexViewModel : PagedGridStateViewModel
{
    public FinanceApPaymentsIndexViewModel()
    {
        SortBy = "paymentdate";
        SortDirection = "desc";
    }

    public string? PaymentNoFilter { get; set; }
    public int? VendorIdFilter { get; set; }
    public DateOnly? PaymentDateFromFilter { get; set; }
    public DateOnly? PaymentDateToFilter { get; set; }
    public FinanceApPaymentMethod? PaymentMethodFilter { get; set; }
    public decimal? AmountFromFilter { get; set; }
    public decimal? AmountToFilter { get; set; }

    public IReadOnlyList<FinanceIdOptionViewModel> VendorOptions { get; set; } = [];

    public PagedResult<ApPaymentDto> Items { get; set; } = PagedResult<ApPaymentDto>.Create([], 0, 1, 20);
}
