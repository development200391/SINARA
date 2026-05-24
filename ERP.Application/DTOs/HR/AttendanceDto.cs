using ERP.Application.DTOs.Common;
using ERP.Domain.Enums;

namespace ERP.Application.DTOs.HR;

public sealed class AttendanceDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateOnly Date { get; set; }
    public DateTimeOffset? CheckIn { get; set; }
    public DateTimeOffset? CheckOut { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Notes { get; set; }
}

public sealed class AttendanceRecordRequest
{
    public int EmployeeId { get; set; }
    public DateOnly Date { get; set; }
    public DateTimeOffset? CheckIn { get; set; }
    public DateTimeOffset? CheckOut { get; set; }
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public string? Notes { get; set; }
}

public sealed class AttendanceReportDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public DateTimeOffset? CheckIn { get; set; }
    public DateTimeOffset? CheckOut { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Notes { get; set; }
}

public sealed class AttendanceReportRequest : PagedRequest
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int? EmployeeId { get; set; }
    public int? DepartmentId { get; set; }
    public AttendanceStatus? Status { get; set; }
}
