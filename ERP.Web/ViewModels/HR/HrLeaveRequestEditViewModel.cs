using System.ComponentModel.DataAnnotations;
using ERP.Application.DTOs.HR;

namespace ERP.Web.ViewModels.HR;

public sealed class HrLeaveRequestEditViewModel
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

    [Range(1, int.MaxValue)]
    public int LeaveTypeId { get; set; }

    [DataType(DataType.Date)]
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [DataType(DataType.Date)]
    public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [MaxLength(500)]
    public string? Reason { get; set; }

    public IReadOnlyList<LookupDto> Employees { get; set; } = [];
    public IReadOnlyList<LookupDto> LeaveTypes { get; set; } = [];
}
