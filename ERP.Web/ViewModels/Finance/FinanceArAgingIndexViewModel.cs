using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceArAgingIndexViewModel : PagedGridStateViewModel
{
    public FinanceArAgingIndexViewModel()
    {
        SortBy = "totaloutstanding";
        SortDirection = "desc";
    }

    public int? CustomerIdFilter { get; set; }
    public DateOnly AsOfDateFilter { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public decimal? OutstandingMinFilter { get; set; }
    public decimal? OutstandingMaxFilter { get; set; }

    public IReadOnlyList<FinanceIdOptionViewModel> CustomerOptions { get; set; } = [];

    public PagedResult<ArAgingRowDto> Items { get; set; } = PagedResult<ArAgingRowDto>.Create([], 0, 1, 20);
}
