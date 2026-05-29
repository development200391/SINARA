using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;

namespace ERP.Web.ViewModels.HR;

public sealed class HrLeaveBalanceIndexViewModel
{
    public string? Search { get; set; }
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "employee";
    public string SortDirection { get; set; } = "asc";
    public int? Year { get; set; }
    public int? EmployeeId { get; set; }
    public int? LeaveTypeId { get; set; }
    public IReadOnlyList<LookupDto> Employees { get; set; } = [];
    public IReadOnlyList<LookupDto> LeaveTypes { get; set; } = [];
    public PagedResult<LeaveBalanceDto> Balances { get; set; } = PagedResult<LeaveBalanceDto>.Create([], 0, 1, 20);
}
