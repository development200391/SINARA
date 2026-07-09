using System.ComponentModel.DataAnnotations;
using ERP.Application.DTOs.HR;
using ERP.Domain.Enums;

namespace ERP.Web.ViewModels.HR;

public sealed class HrAttendanceEditViewModel
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

    [DataType(DataType.Date)]
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Check In")]
    public DateTime? CheckInLocal { get; set; }

    [Display(Name = "Check Out")]
    public DateTime? CheckOutLocal { get; set; }

    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

    [MaxLength(4000)]
    public string? Notes { get; set; }

    public IReadOnlyList<LookupDto> Employees { get; set; } = [];
}

