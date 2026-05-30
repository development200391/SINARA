using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Enums;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceAccountsIndexViewModel : PagedGridStateViewModel
{
    public FinanceAccountsIndexViewModel()
    {
        SortBy = "code";
        SortDirection = "asc";
    }

    public string? CodeFilter { get; set; }
    public string? NameFilter { get; set; }
    public int? GroupIdFilter { get; set; }
    public FinanceAccountType? TypeFilter { get; set; }
    public FinanceNormalBalance? NormalBalanceFilter { get; set; }
    public bool? IsHeaderFilter { get; set; }
    public int? ParentAccountIdFilter { get; set; }
    public string? CurrencyCodeFilter { get; set; }
    public bool? IsBankAccountFilter { get; set; }
    public bool? IsActiveFilter { get; set; }

    public IReadOnlyList<FinanceIdOptionViewModel> GroupOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> ParentAccountOptions { get; set; } = [];
    public IReadOnlyList<FinanceCodeOptionViewModel> CurrencyOptions { get; set; } = [];

    public PagedResult<AccountDto> Items { get; set; } = PagedResult<AccountDto>.Create([], 0, 1, 20);
}
