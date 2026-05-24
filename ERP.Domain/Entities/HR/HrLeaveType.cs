namespace ERP.Domain.Entities.HR;

public sealed class HrLeaveType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int MaxDaysPerYear { get; set; }
    public bool IsCarryOver { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<HrLeaveRequest> LeaveRequests { get; set; } = new List<HrLeaveRequest>();
}
