namespace ERP.Domain.Entities.Inventory;

public sealed class InvGoodsReceiptLine
{
    public int Id { get; set; }
    public int GoodsReceiptId { get; set; }
    public int LineNo { get; set; }
    public int ItemId { get; set; }
    public int? UomId { get; set; }
    public decimal QtyReceived { get; set; }
    public decimal QtyBase { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public string? Notes { get; set; }

    public InvGoodsReceipt GoodsReceipt { get; set; } = null!;
    public InvItem Item { get; set; } = null!;
    public InvUnitOfMeasure? Uom { get; set; }
}
