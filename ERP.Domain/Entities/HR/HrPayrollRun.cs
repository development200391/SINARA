using ERP.Domain.Entities.System;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities.HR;

public sealed class HrPayrollRun : BaseEntity
{
    public int PeriodMonth { get; set; }
    public int PeriodYear { get; set; }
    public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
    public int? ProcessedBy { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }

    public SysUser? ProcessedByUser { get; set; }
    public ICollection<HrPayrollDetail> PayrollDetails { get; set; } = new List<HrPayrollDetail>();
}
