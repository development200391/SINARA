using ERP.Domain.Enums.Manufacturing;

namespace ERP.Domain.Entities.Manufacturing;

public sealed class MfgMrpRun : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public DateOnly RunDate { get; set; }
    public MrpStatus Status { get; set; } = MrpStatus.Draft;
    public int HorizonDays { get; set; } = 30;
    public int TotalDemandItems { get; set; }
    public int RecommendedWoCount { get; set; }
    public int RecommendedPrCount { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? Notes { get; set; }

    public ICollection<MfgWorkOrder> WorkOrders { get; set; } = new List<MfgWorkOrder>();
}
