using ERP.Domain.Entities.Inventory;
using ERP.Domain.Enums.Manufacturing;

namespace ERP.Domain.Entities.Manufacturing;

public sealed class MfgReworkOrder : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public int? SourceWorkOrderId { get; set; }
    public int? WorkOrderId { get; set; }
    public int? ItemId { get; set; }
    public decimal QtyRework { get; set; }
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Draft;
    public DateTimeOffset OpenedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public string? Notes { get; set; }

    public MfgWorkOrder? SourceWorkOrder { get; set; }
    public MfgWorkOrder? WorkOrder { get; set; }
    public InvItem? Item { get; set; }
}
