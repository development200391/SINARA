using ERP.Application.DTOs.Common;
using ERP.Domain.Enums;

namespace ERP.Application.DTOs.HR;

public sealed class EmployeeListDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int PositionId { get; set; }
    public string PositionName { get; set; } = string.Empty;
    public DateOnly HireDate { get; set; }
    public DateOnly? TerminationDate { get; set; }
    public EmploymentStatus EmploymentStatus { get; set; }
}

public sealed class EmployeeDetailDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int PositionId { get; set; }
    public string PositionName { get; set; } = string.Empty;
    public DateOnly HireDate { get; set; }
    public DateOnly? TerminationDate { get; set; }
    public EmploymentStatus EmploymentStatus { get; set; }
}

public class CreateEmployeeRequest
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int DepartmentId { get; set; }
    public int PositionId { get; set; }
    public DateOnly HireDate { get; set; }
    public EmploymentStatus EmploymentStatus { get; set; } = EmploymentStatus.Active;
}

public sealed class UpdateEmployeeRequest : CreateEmployeeRequest
{
    public DateOnly? TerminationDate { get; set; }
}

public sealed class EmployeePagedRequest : PagedRequest
{
    public string? EmployeeCode { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int? DepartmentId { get; set; }
    public int? PositionId { get; set; }
    public EmploymentStatus? EmploymentStatus { get; set; }
    public DateOnly? HireDateFrom { get; set; }
    public DateOnly? HireDateTo { get; set; }
    public DateOnly? TerminationDateFrom { get; set; }
    public DateOnly? TerminationDateTo { get; set; }
}
