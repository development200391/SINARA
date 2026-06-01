using ERP.Domain.Entities.System;
using ERP.Domain.Enums.Inventory;

namespace ERP.Domain.Entities.Inventory;

public sealed class InvStockOpname : BaseEntity
{
    public string OpnameNo { get; set; } = string.Empty;
    public DateOnly OpnameDate { get; set; }
    public int WarehouseId { get; set; }
    public int? LocationId { get; set; }
    public string? Description { get; set; }
    public OpnameStatus Status { get; set; } = OpnameStatus.Draft;
    public int? CountedBy { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public int? AdjustmentId { get; set; }

    public InvWarehouse Warehouse { get; set; } = null!;
    public InvWarehouseLocation? Location { get; set; }
    public SysUser? CountedByUser { get; set; }
    public SysUser? ApprovedByUser { get; set; }
    public InvStockAdjustment? Adjustment { get; set; }
    public ICollection<InvStockOpnameLine> Lines { get; set; } = new List<InvStockOpnameLine>();
}
