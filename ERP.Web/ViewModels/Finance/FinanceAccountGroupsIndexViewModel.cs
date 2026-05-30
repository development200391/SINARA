using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Enums;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceAccountGroupsIndexViewModel : PagedGridStateViewModel
{
    public FinanceAccountGroupsIndexViewModel()
    {
        SortBy = "sortOrder";
        SortDirection = "asc";
    }

    public string? CodeFilter { get; set; }
    public string? NameFilter { get; set; }
    public FinanceAccountType? TypeFilter { get; set; }
    public FinanceNormalBalance? NormalBalanceFilter { get; set; }
    public int? ParentGroupIdFilter { get; set; }
    public bool? IsActiveFilter { get; set; }

    public IReadOnlyList<FinanceIdOptionViewModel> ParentGroupOptions { get; set; } = [];
    public PagedResult<AccountGroupDto> Items { get; set; } = PagedResult<AccountGroupDto>.Create([], 0, 1, 20);
}
