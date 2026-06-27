using ERP.Domain.Entities.Inventory;
using ERP.Domain.Enums.Manufacturing;

namespace ERP.Domain.Entities.Manufacturing;

public sealed class MfgScrapRecord : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public int? WorkOrderId { get; set; }
    public int? ItemId { get; set; }
    public int? WorkCenterId { get; set; }
    public ScrapReason Reason { get; set; } = ScrapReason.Other;
    public decimal QtyScrap { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalScrapCost { get; set; }
    public DateTimeOffset RecordedAt { get; set; }
    public string? Notes { get; set; }

    public MfgWorkOrder? WorkOrder { get; set; }
    public InvItem? Item { get; set; }
    public MfgWorkCenter? WorkCenter { get; set; }
}
