using ERP.Domain.Entities.Inventory;
using ERP.Domain.Enums.Manufacturing;

namespace ERP.Domain.Entities.Manufacturing;

public sealed class MfgRouting : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? ItemId { get; set; }
    public int? WorkCenterId { get; set; }
    public int Version { get; set; } = 1;
    public RoutingStatus Status { get; set; } = RoutingStatus.Draft;
    public decimal TotalLeadTimeHours { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public InvItem? Item { get; set; }
    public MfgWorkCenter? WorkCenter { get; set; }
    public ICollection<MfgBom> Boms { get; set; } = new List<MfgBom>();
    public ICollection<MfgWorkOrder> WorkOrders { get; set; } = new List<MfgWorkOrder>();
}
