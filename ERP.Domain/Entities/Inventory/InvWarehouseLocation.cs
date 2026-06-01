namespace ERP.Domain.Entities.Inventory;

public sealed class InvWarehouseLocation : BaseEntity
{
    public int WarehouseId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;

    public InvWarehouse Warehouse { get; set; } = null!;
    public ICollection<InvStockBalance> StockBalances { get; set; } = new List<InvStockBalance>();
}
