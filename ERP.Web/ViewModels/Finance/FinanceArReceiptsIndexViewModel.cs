using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Enums;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceArReceiptsIndexViewModel : PagedGridStateViewModel
{
    public FinanceArReceiptsIndexViewModel()
    {
        SortBy = "receiptdate";
        SortDirection = "desc";
    }

    public string? ReceiptNoFilter { get; set; }
    public int? CustomerIdFilter { get; set; }
    public DateOnly? ReceiptDateFromFilter { get; set; }
    public DateOnly? ReceiptDateToFilter { get; set; }
    public FinanceArReceiptMethod? PaymentMethodFilter { get; set; }
    public decimal? AmountFromFilter { get; set; }
    public decimal? AmountToFilter { get; set; }

    public IReadOnlyList<FinanceIdOptionViewModel> CustomerOptions { get; set; } = [];

    public PagedResult<ArReceiptDto> Items { get; set; } = PagedResult<ArReceiptDto>.Create([], 0, 1, 20);
}
