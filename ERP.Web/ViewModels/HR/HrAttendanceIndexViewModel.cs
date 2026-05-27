using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;
using ERP.Domain.Enums;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.HR;

public sealed class HrAttendanceIndexViewModel : PagedGridStateViewModel
{
    public HrAttendanceIndexViewModel()
    {
        SortBy = "date";
        SortDirection = "desc";
    }

    public string? EmployeeCodeFilter { get; set; }
    public string? EmployeeNameFilter { get; set; }
    public int? EmployeeIdFilter { get; set; }
    public int? DepartmentIdFilter { get; set; }
    public DateOnly? DateFromFilter { get; set; }
    public DateOnly? DateToFilter { get; set; }
    public DateOnly? CheckInFromFilter { get; set; }
    public DateOnly? CheckInToFilter { get; set; }
    public DateOnly? CheckOutFromFilter { get; set; }
    public DateOnly? CheckOutToFilter { get; set; }
    public AttendanceStatus? StatusFilter { get; set; }
    public string? NotesFilter { get; set; }

    public IReadOnlyList<LookupDto> Employees { get; set; } = [];
    public IReadOnlyList<DepartmentDto> Departments { get; set; } = [];

    public PagedResult<AttendanceReportDto> Attendances { get; set; } = PagedResult<AttendanceReportDto>.Create([], 0, 1, 20);
}
