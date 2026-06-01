using ERP.Domain.Enums.Inventory;

namespace ERP.Domain.Entities.Inventory;

public sealed class InvStockMovement
{
    public int Id { get; set; }
    public DateOnly MovementDate { get; set; }
    public int ItemId { get; set; }
    public int WarehouseId { get; set; }
    public int? LocationId { get; set; }
    public StockMovementType MovementType { get; set; }
    public decimal QtyIn { get; set; }
    public decimal QtyOut { get; set; }
    public decimal QtyBalance { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public string SourceTable { get; set; } = string.Empty;
    public int SourceId { get; set; }
    public int? SourceLineId { get; set; }
    public string? Notes { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public InvItem Item { get; set; } = null!;
    public InvWarehouse Warehouse { get; set; } = null!;
    public InvWarehouseLocation? Location { get; set; }
}
