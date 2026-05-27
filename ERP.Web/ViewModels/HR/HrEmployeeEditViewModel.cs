using System.ComponentModel.DataAnnotations;
using ERP.Application.DTOs.HR;
using ERP.Domain.Enums;

namespace ERP.Web.ViewModels.HR;

public sealed class HrEmployeeEditViewModel
{
    public int Id { get; set; }

    [MaxLength(20)]
    public string EmployeeCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }

    [Range(1, int.MaxValue)]
    public int PositionId { get; set; }

    [DataType(DataType.Date)]
    public DateOnly HireDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [DataType(DataType.Date)]
    public DateOnly? TerminationDate { get; set; }

    public EmploymentStatus EmploymentStatus { get; set; } = EmploymentStatus.Active;

    public IReadOnlyList<DepartmentDto> Departments { get; set; } = [];
    public IReadOnlyList<PositionDto> Positions { get; set; } = [];
}
