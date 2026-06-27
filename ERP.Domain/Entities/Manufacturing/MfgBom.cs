using ERP.Domain.Entities.Inventory;
using ERP.Domain.Enums.Manufacturing;

namespace ERP.Domain.Entities.Manufacturing;

public sealed class MfgBom : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public int? ItemId { get; set; }
    public int? RoutingId { get; set; }
    public int Version { get; set; } = 1;
    public BomStatus Status { get; set; } = BomStatus.Draft;
    public decimal QtyProduced { get; set; } = 1m;
    public decimal StandardCost { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public InvItem? Item { get; set; }
    public MfgRouting? Routing { get; set; }
    public ICollection<MfgWorkOrder> WorkOrders { get; set; } = new List<MfgWorkOrder>();
}
