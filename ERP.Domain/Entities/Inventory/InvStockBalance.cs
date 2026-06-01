namespace ERP.Domain.Entities.Inventory;

public sealed class InvStockBalance : BaseEntity
{
    public int ItemId { get; set; }
    public int WarehouseId { get; set; }
    public int? LocationId { get; set; }
    public decimal QtyOnHand { get; set; }
    public decimal QtyReserved { get; set; }
    public decimal QtyAvailable { get; set; }
    public decimal AvgCost { get; set; }
    public decimal TotalValue { get; set; }
    public DateTimeOffset? LastMovementAt { get; set; }

    public InvItem Item { get; set; } = null!;
    public InvWarehouse Warehouse { get; set; } = null!;
    public InvWarehouseLocation? Location { get; set; }
}
