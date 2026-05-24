using ERP.Domain.Enums;

namespace ERP.Application.DTOs.HR;

public sealed class LeaveRequestDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int TotalDays { get; set; }
    public string? Reason { get; set; }
    public LeaveStatus Status { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
}
