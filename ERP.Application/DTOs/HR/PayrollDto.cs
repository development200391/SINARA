using ERP.Application.DTOs.Common;
using ERP.Domain.Enums;

namespace ERP.Application.DTOs.HR;

public sealed class PayrollRunDto
{
    public int Id { get; set; }
    public int PeriodMonth { get; set; }
    public int PeriodYear { get; set; }
    public PayrollStatus Status { get; set; }
    public int? ProcessedBy { get; set; }
    public string? ProcessedByName { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public int TotalEmployees { get; set; }
    public decimal TotalNetSalary { get; set; }
}

public sealed class PayrollRunRequest
{
    public int Month { get; set; }
    public int Year { get; set; }
}

public sealed class PayrollRunDetailDto
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public decimal Allowances { get; set; }
    public decimal Deductions { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetSalary { get; set; }
}

public sealed class PayslipDto
{
    public int PayrollRunId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public int PeriodMonth { get; set; }
    public int PeriodYear { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal Allowances { get; set; }
    public decimal Deductions { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetSalary { get; set; }
}

public sealed class PayrollRunPagedRequest : PagedRequest
{
    public int? Month { get; set; }
    public int? Year { get; set; }
    public PayrollStatus? Status { get; set; }
}
