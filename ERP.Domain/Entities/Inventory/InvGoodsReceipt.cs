using ERP.Domain.Entities.Finance;
using ERP.Domain.Entities.System;
using ERP.Domain.Enums.Inventory;

namespace ERP.Domain.Entities.Inventory;

public sealed class InvGoodsReceipt : BaseEntity
{
    public string ReceiptNo { get; set; } = string.Empty;
    public DateOnly ReceiptDate { get; set; }
    public GoodsReceiptType ReceiptType { get; set; } = GoodsReceiptType.PurchaseReceipt;
    public int WarehouseId { get; set; }
    public int? LocationId { get; set; }
    public string? SupplierName { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Description { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Draft;
    public int? ReceivedBy { get; set; }
    public int? ConfirmedBy { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
    public int? JournalEntryId { get; set; }

    public InvWarehouse Warehouse { get; set; } = null!;
    public InvWarehouseLocation? Location { get; set; }
    public SysUser? ReceivedByUser { get; set; }
    public SysUser? ConfirmedByUser { get; set; }
    public FinJournalEntry? JournalEntry { get; set; }
    public ICollection<InvGoodsReceiptLine> Lines { get; set; } = new List<InvGoodsReceiptLine>();
}
