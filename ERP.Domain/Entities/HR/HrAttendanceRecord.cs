using ERP.Domain.Enums;

namespace ERP.Domain.Entities.HR;

public sealed class HrAttendanceRecord : BaseEntity
{
    public int EmployeeId { get; set; }
    public DateOnly Date { get; set; }
    public DateTimeOffset? CheckIn { get; set; }
    public DateTimeOffset? CheckOut { get; set; }
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public string? Notes { get; set; }

    public HrEmployee Employee { get; set; } = null!;
}
