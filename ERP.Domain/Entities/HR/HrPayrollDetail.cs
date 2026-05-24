namespace ERP.Domain.Entities.HR;

public sealed class HrPayrollDetail : BaseEntity
{
    public int PayrollRunId { get; set; }
    public int EmployeeId { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal Allowances { get; set; }
    public decimal Deductions { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetSalary { get; set; }

    public HrPayrollRun PayrollRun { get; set; } = null!;
    public HrEmployee Employee { get; set; } = null!;
}
