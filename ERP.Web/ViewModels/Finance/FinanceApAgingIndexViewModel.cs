using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceApAgingIndexViewModel : PagedGridStateViewModel
{
    public FinanceApAgingIndexViewModel()
    {
        SortBy = "totaloutstanding";
        SortDirection = "desc";
    }

    public int? VendorIdFilter { get; set; }
    public DateOnly AsOfDateFilter { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public decimal? OutstandingMinFilter { get; set; }
    public decimal? OutstandingMaxFilter { get; set; }

    public IReadOnlyList<FinanceIdOptionViewModel> VendorOptions { get; set; } = [];

    public PagedResult<ApAgingRowDto> Items { get; set; } = PagedResult<ApAgingRowDto>.Create([], 0, 1, 20);
}
