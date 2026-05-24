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
