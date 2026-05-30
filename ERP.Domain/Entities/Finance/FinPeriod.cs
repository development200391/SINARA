using ERP.Domain.Enums;

namespace ERP.Domain.Entities.Finance;

public sealed class FinPeriod : BaseEntity
{
    public int FiscalYearId { get; set; }
    public int PeriodNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public FinancePeriodStatus Status { get; set; } = FinancePeriodStatus.Open;

    public FinFiscalYear FiscalYear { get; set; } = null!;
}
