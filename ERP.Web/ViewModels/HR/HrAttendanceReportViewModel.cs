using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.HR;

public sealed class HrAttendanceReportViewModel : PagedGridStateViewModel
{
    public HrAttendanceReportViewModel()
    {
        SortBy = "employeeName";
        SortDirection = "asc";
    }

    public string Period { get; set; } = string.Empty;
    public string PeriodDisplay { get; set; } = string.Empty;
    public int? DepartmentIdFilter { get; set; }
    public int? EmployeeIdFilter { get; set; }
    public string? StatusFilter { get; set; }

    public int TotalEmployees { get; set; }
    public int TotalPresenceDays { get; set; }
    public int TotalWorkDays { get; set; }
    public int TotalLateCount { get; set; }
    public int TotalAbsentCount { get; set; }
    public decimal AttendancePercentage { get; set; }

    public IReadOnlyList<DepartmentDto> Departments { get; set; } = [];
    public IReadOnlyList<LookupDto> Employees { get; set; } = [];
    public PagedResult<HrAttendanceReportRowViewModel> Reports { get; set; } = PagedResult<HrAttendanceReportRowViewModel>.Create([], 0, 1, 20);
}

public sealed class HrAttendanceReportRowViewModel
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int PresentCount { get; set; }
    public int LateCount { get; set; }
    public int AbsentCount { get; set; }
    public int LeaveCount { get; set; }
    public string DominantStatus { get; set; } = "Hadir";
}
