using ERP.Domain.Entities.HR;
using ERP.Domain.Entities.Inventory;
using ERP.Domain.Enums.Manufacturing;

namespace ERP.Domain.Entities.Manufacturing;

public sealed class MfgQcInspection : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public int? WorkOrderId { get; set; }
    public int? ItemId { get; set; }
    public int? InspectorEmployeeId { get; set; }
    public DateTimeOffset InspectedAt { get; set; }
    public QcStatus Status { get; set; } = QcStatus.Pending;
    public QcResult Result { get; set; } = QcResult.Pass;
    public string? Notes { get; set; }

    public MfgWorkOrder? WorkOrder { get; set; }
    public InvItem? Item { get; set; }
    public HrEmployee? InspectorEmployee { get; set; }
}
