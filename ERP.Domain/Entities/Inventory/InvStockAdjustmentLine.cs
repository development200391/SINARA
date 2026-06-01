namespace ERP.Domain.Entities.Inventory;

public sealed class InvStockAdjustmentLine
{
    public int Id { get; set; }
    public int StockAdjustmentId { get; set; }
    public int LineNo { get; set; }
    public int ItemId { get; set; }
    public int? UomId { get; set; }
    public decimal QtyAdjustment { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public string? Notes { get; set; }

    public InvStockAdjustment StockAdjustment { get; set; } = null!;
    public InvItem Item { get; set; } = null!;
    public InvUnitOfMeasure? Uom { get; set; }
}
