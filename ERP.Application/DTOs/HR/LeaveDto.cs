using ERP.Application.DTOs.Common;
using ERP.Domain.Enums;

namespace ERP.Application.DTOs.HR;

public sealed class LeaveRequestDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public int LeaveTypeId { get; set; }
    public string LeaveTypeCode { get; set; } = string.Empty;
    public string LeaveTypeName { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int TotalDays { get; set; }
    public string? Reason { get; set; }
    public LeaveStatus Status { get; set; }
    public int? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class SubmitLeaveRequest
{
    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Reason { get; set; }
}

public sealed class LeaveRequestPagedRequest : PagedRequest
{
    public LeaveStatus? Status { get; set; }
    public int? EmployeeId { get; set; }
}

public sealed class LeaveTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int MaxDaysPerYear { get; set; }
    public bool IsCarryOver { get; set; }
    public bool IsActive { get; set; }
}

public sealed class LeaveBalanceDto
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public int LeaveTypeId { get; set; }
    public string LeaveTypeCode { get; set; } = string.Empty;
    public string LeaveTypeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int MaxDaysPerYear { get; set; }
    public int UsedDays { get; set; }
    public int RemainingDays { get; set; }
}

public sealed class LeaveBalanceRequest : PagedRequest
{
    public int? EmployeeId { get; set; }
    public int? LeaveTypeId { get; set; }
    public int? Year { get; set; }
}

public sealed class LookupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class LeaveRequestOptionsDto
{
    public IReadOnlyList<LookupDto> Employees { get; set; } = [];
    public IReadOnlyList<LookupDto> LeaveTypes { get; set; } = [];
}
