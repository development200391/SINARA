namespace ERP.Domain.Entities.Manufacturing;

public sealed class MfgOeeSnapshot : BaseEntity
{
    public DateOnly SnapshotDate { get; set; }
    public int WorkCenterId { get; set; }
    public decimal AvailabilityPct { get; set; }
    public decimal PerformancePct { get; set; }
    public decimal QualityPct { get; set; }
    public decimal OeePct { get; set; }

    public MfgWorkCenter WorkCenter { get; set; } = null!;
}
