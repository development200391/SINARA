namespace ERP.Domain.Entities.Inventory;

public sealed class InvStockOpnameLine
{
    public int Id { get; set; }
    public int StockOpnameId { get; set; }
    public int LineNo { get; set; }
    public int ItemId { get; set; }
    public int? LocationId { get; set; }
    public decimal QtySystem { get; set; }
    public decimal QtyCounted { get; set; }
    public decimal QtyVariance { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalVarianceValue { get; set; }
    public string? Notes { get; set; }

    public InvStockOpname StockOpname { get; set; } = null!;
    public InvItem Item { get; set; } = null!;
    public InvWarehouseLocation? Location { get; set; }
}
