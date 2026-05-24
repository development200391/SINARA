using ERP.Domain.Enums;

namespace ERP.Application.DTOs.HR;

public sealed class PayrollRunDto
{
    public int Id { get; set; }
    public int PeriodMonth { get; set; }
    public int PeriodYear { get; set; }
    public PayrollStatus Status { get; set; }
    public int? ProcessedBy { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
}

public sealed class PayslipDto
{
    public int PayrollRunId { get; set; }
    public int EmployeeId { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal Allowances { get; set; }
    public decimal Deductions { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetSalary { get; set; }
}
