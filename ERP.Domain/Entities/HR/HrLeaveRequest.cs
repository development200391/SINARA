using ERP.Domain.Entities.System;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities.HR;

public sealed class HrLeaveRequest : BaseEntity
{
    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int TotalDays { get; set; }
    public string? Reason { get; set; }
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public int? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }

    public HrEmployee Employee { get; set; } = null!;
    public HrLeaveType LeaveType { get; set; } = null!;
    public SysUser? ApprovedByUser { get; set; }
}
