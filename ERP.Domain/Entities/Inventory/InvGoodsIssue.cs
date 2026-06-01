using ERP.Domain.Entities.Finance;
using ERP.Domain.Entities.HR;
using ERP.Domain.Entities.System;
using ERP.Domain.Enums.Inventory;

namespace ERP.Domain.Entities.Inventory;

public sealed class InvGoodsIssue : BaseEntity
{
    public string IssueNo { get; set; } = string.Empty;
    public DateOnly IssueDate { get; set; }
    public GoodsIssueType IssueType { get; set; } = GoodsIssueType.DepartmentalUse;
    public int WarehouseId { get; set; }
    public int? LocationId { get; set; }
    public int? DepartmentId { get; set; }
    public int? CostCenterId { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Description { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Draft;
    public int? RequestedBy { get; set; }
    public int? IssuedBy { get; set; }
    public int? ConfirmedBy { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
    public int? JournalEntryId { get; set; }

    public InvWarehouse Warehouse { get; set; } = null!;
    public InvWarehouseLocation? Location { get; set; }
    public HrDepartment? Department { get; set; }
    public FinCostCenter? CostCenter { get; set; }
    public SysUser? RequestedByUser { get; set; }
    public SysUser? IssuedByUser { get; set; }
    public SysUser? ConfirmedByUser { get; set; }
    public FinJournalEntry? JournalEntry { get; set; }
    public ICollection<InvGoodsIssueLine> Lines { get; set; } = new List<InvGoodsIssueLine>();
}
