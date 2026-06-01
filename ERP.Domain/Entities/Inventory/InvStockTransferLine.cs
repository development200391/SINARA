namespace ERP.Domain.Entities.Inventory;

public sealed class InvStockTransferLine
{
    public int Id { get; set; }
    public int StockTransferId { get; set; }
    public int LineNo { get; set; }
    public int ItemId { get; set; }
    public int? UomId { get; set; }
    public decimal QtyTransfer { get; set; }
    public decimal QtyBase { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public string? Notes { get; set; }

    public InvStockTransfer StockTransfer { get; set; } = null!;
    public InvItem Item { get; set; } = null!;
    public InvUnitOfMeasure? Uom { get; set; }
}
