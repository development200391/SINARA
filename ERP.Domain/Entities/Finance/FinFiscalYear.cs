using ERP.Domain.Enums;

namespace ERP.Domain.Entities.Finance;

public sealed class FinFiscalYear : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public FinancePeriodStatus Status { get; set; } = FinancePeriodStatus.Open;

    public ICollection<FinPeriod> Periods { get; set; } = new List<FinPeriod>();
}
