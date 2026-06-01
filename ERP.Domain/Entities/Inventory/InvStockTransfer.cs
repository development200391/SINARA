using ERP.Domain.Entities.Finance;
using ERP.Domain.Entities.System;
using ERP.Domain.Enums.Inventory;

namespace ERP.Domain.Entities.Inventory;

public sealed class InvStockTransfer : BaseEntity
{
    public string TransferNo { get; set; } = string.Empty;
    public DateOnly TransferDate { get; set; }
    public int FromWarehouseId { get; set; }
    public int? FromLocationId { get; set; }
    public int ToWarehouseId { get; set; }
    public int? ToLocationId { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Description { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Draft;
    public int? TransferredBy { get; set; }
    public int? ConfirmedBy { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
    public int? JournalEntryId { get; set; }

    public InvWarehouse FromWarehouse { get; set; } = null!;
    public InvWarehouse ToWarehouse { get; set; } = null!;
    public InvWarehouseLocation? FromLocation { get; set; }
    public InvWarehouseLocation? ToLocation { get; set; }
    public SysUser? TransferredByUser { get; set; }
    public SysUser? ConfirmedByUser { get; set; }
    public FinJournalEntry? JournalEntry { get; set; }
    public ICollection<InvStockTransferLine> Lines { get; set; } = new List<InvStockTransferLine>();
}
