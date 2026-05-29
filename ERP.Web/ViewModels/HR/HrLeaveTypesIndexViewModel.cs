using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;

namespace ERP.Web.ViewModels.HR;

public sealed class HrLeaveTypesIndexViewModel
{
    public string? Search { get; set; }
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "name";
    public string SortDirection { get; set; } = "asc";

    public string? NameFilter { get; set; }
    public string? CodeFilter { get; set; }
    public int? MaxDaysPerYearFromFilter { get; set; }
    public int? MaxDaysPerYearToFilter { get; set; }
    public bool? IsCarryOverFilter { get; set; }
    public bool? IsActiveFilter { get; set; }

    public PagedResult<LeaveTypeDto> LeaveTypes { get; set; } = PagedResult<LeaveTypeDto>.Create([], 0, 1, 20);
}
