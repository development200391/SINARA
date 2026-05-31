using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceSmokeTestsIndexViewModel : PagedGridStateViewModel
{
    public FinanceSmokeTestsIndexViewModel()
    {
        SortBy = "sortorder";
        SortDirection = "asc";
    }

    public string? CategoryFilter { get; set; }
    public bool? PassedFilter { get; set; }

    public IReadOnlyList<string> CategoryOptions { get; set; } = [];

    public PagedResult<SmokeTestRowDto> Items { get; set; } = PagedResult<SmokeTestRowDto>.Create([], 0, 1, 20);
}