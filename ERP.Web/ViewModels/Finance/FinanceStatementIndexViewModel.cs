using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Enums;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceStatementIndexViewModel : PagedGridStateViewModel
{
    public FinanceStatementIndexViewModel()
    {
        SortBy = "section";
        SortDirection = "asc";
    }

    public string ReportKey { get; set; } = string.Empty;
    public string ReportTitle { get; set; } = string.Empty;
    public string ExportActionName { get; set; } = string.Empty;

    public int? PeriodIdFilter { get; set; }
    public DateOnly? DateFromFilter { get; set; }
    public DateOnly? DateToFilter { get; set; }
    public int? CostCenterIdFilter { get; set; }
    public FinanceAccountType? AccountTypeFilter { get; set; }
    public string? SectionFilter { get; set; }

    public IReadOnlyList<FinanceAccountType> AccountTypeOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> PeriodOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> CostCenterOptions { get; set; } = [];
    public IReadOnlyList<string> SectionOptions { get; set; } = [];

    public PagedResult<FinancialStatementRowDto> Items { get; set; } = PagedResult<FinancialStatementRowDto>.Create([], 0, 1, 20);
}
