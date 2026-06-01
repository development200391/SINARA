using ERP.Domain.Entities.Finance;
using ERP.Domain.Entities.System;
using ERP.Domain.Enums.Inventory;

namespace ERP.Domain.Entities.Inventory;

public sealed class InvStockAdjustment : BaseEntity
{
    public string AdjustmentNo { get; set; } = string.Empty;
    public DateOnly AdjustmentDate { get; set; }
    public int WarehouseId { get; set; }
    public int? LocationId { get; set; }
    public AdjustmentReason Reason { get; set; } = AdjustmentReason.DataCorrection;
    public string? ReferenceNo { get; set; }
    public string? Description { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Draft;
    public int? RequestedBy { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public int? ConfirmedBy { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
    public int? JournalEntryId { get; set; }

    public InvWarehouse Warehouse { get; set; } = null!;
    public InvWarehouseLocation? Location { get; set; }
    public SysUser? RequestedByUser { get; set; }
    public SysUser? ApprovedByUser { get; set; }
    public SysUser? ConfirmedByUser { get; set; }
    public FinJournalEntry? JournalEntry { get; set; }
    public ICollection<InvStockAdjustmentLine> Lines { get; set; } = new List<InvStockAdjustmentLine>();
}
