using ERP.Domain.Entities.System;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities.HR;

public sealed class HrEmployee : BaseEntity
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int DepartmentId { get; set; }
    public int PositionId { get; set; }
    public DateOnly HireDate { get; set; }
    public DateOnly? TerminationDate { get; set; }
    public EmploymentStatus EmploymentStatus { get; set; } = EmploymentStatus.Active;
    public int? UserId { get; set; }

    public HrDepartment Department { get; set; } = null!;
    public HrPosition Position { get; set; } = null!;
    public SysUser? User { get; set; }
    public ICollection<HrDepartment> DepartmentsAsManager { get; set; } = new List<HrDepartment>();
    public ICollection<HrAttendanceRecord> AttendanceRecords { get; set; } = new List<HrAttendanceRecord>();
    public ICollection<HrLeaveRequest> LeaveRequests { get; set; } = new List<HrLeaveRequest>();
    public ICollection<HrPayrollDetail> PayrollDetails { get; set; } = new List<HrPayrollDetail>();
}
