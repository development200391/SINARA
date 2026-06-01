using ERP.Application.DTOs.Common;
using ERP.Domain.Enums.Inventory;

namespace ERP.Application.DTOs.Inventory;

public sealed class InventoryOptionDto
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
}

public sealed class InventoryCodeOptionDto
{
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public sealed class ItemCategoryDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class ItemCategoryPagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public int? ParentCategoryId { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class UnitOfMeasureDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UnitOfMeasurePagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class BrandDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class BrandPagedRequest : PagedRequest
{
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class ItemDto
{
    public int Id { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int? BrandId { get; set; }
    public string? BrandName { get; set; }
    public ItemType Type { get; set; } = ItemType.Product;
    public int BaseUomId { get; set; }
    public string BaseUomCode { get; set; } = string.Empty;
    public int? PurchaseUomId { get; set; }
    public string? PurchaseUomCode { get; set; }
    public ItemStatus Status { get; set; } = ItemStatus.Active;
    public ValuationMethod ValuationMethod { get; set; } = ValuationMethod.WeightedAverageCost;
    public decimal? LastPurchasePrice { get; set; }
    public decimal AvgCost { get; set; }
    public decimal MinStock { get; set; }
    public decimal MaxStock { get; set; }
    public decimal ReorderPoint { get; set; }
    public int LeadTimeDays { get; set; }
    public int? InventoryAccountId { get; set; }
    public string? InventoryAccountCode { get; set; }
    public int? CogsAccountId { get; set; }
    public string? CogsAccountCode { get; set; }
    public int? AdjustmentAccountId { get; set; }
    public string? AdjustmentAccountCode { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal QtyAvailable { get; set; }
}

public sealed class ItemPagedRequest : PagedRequest
{
    public string? ItemCode { get; set; }
    public string? Sku { get; set; }
    public string? Name { get; set; }
    public int? CategoryId { get; set; }
    public int? BrandId { get; set; }
    public ItemType? Type { get; set; }
    public ItemStatus? Status { get; set; }
    public bool? IsActive { get; set; }
    public decimal? MinStockFrom { get; set; }
    public decimal? MinStockTo { get; set; }
    public decimal? ReorderPointFrom { get; set; }
    public decimal? ReorderPointTo { get; set; }
}

public sealed class ItemUnitConversionDto
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public int FromUomId { get; set; }
    public string FromUomCode { get; set; } = string.Empty;
    public int ToUomId { get; set; }
    public string ToUomCode { get; set; } = string.Empty;
    public decimal ConversionFactor { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class ItemUnitConversionPagedRequest : PagedRequest
{
    public int? ItemId { get; set; }
    public int? FromUomId { get; set; }
    public int? ToUomId { get; set; }
    public bool? IsActive { get; set; }
    public decimal? FactorFrom { get; set; }
    public decimal? FactorTo { get; set; }
}

public sealed class WarehouseDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public int? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public int? CostCenterId { get; set; }
    public string? CostCenterCode { get; set; }
    public bool IsTransit { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class WarehousePagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public int? ManagerId { get; set; }
    public int? CostCenterId { get; set; }
    public bool? IsTransit { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class WarehouseLocationDto
{
    public int Id { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class WarehouseLocationPagedRequest : PagedRequest
{
    public int? WarehouseId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public bool? IsDefault { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class StockBalanceDto
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public int? LocationId { get; set; }
    public string? LocationCode { get; set; }
    public decimal QtyOnHand { get; set; }
    public decimal QtyReserved { get; set; }
    public decimal QtyAvailable { get; set; }
    public decimal AvgCost { get; set; }
    public decimal TotalValue { get; set; }
}

public sealed class StockBalancePagedRequest : PagedRequest
{
    public int? WarehouseId { get; set; }
    public int? ItemId { get; set; }
    public int? LocationId { get; set; }
}

public sealed class GoodsReceiptLineDto
{
    public int Id { get; set; }
    public int LineNo { get; set; }
    public int ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public int? UomId { get; set; }
    public string? UomCode { get; set; }
    public decimal QtyReceived { get; set; }
    public decimal QtyBase { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public string? Notes { get; set; }
}

public sealed class GoodsReceiptDto
{
    public int Id { get; set; }
    public string ReceiptNo { get; set; } = string.Empty;
    public DateOnly ReceiptDate { get; set; }
    public GoodsReceiptType ReceiptType { get; set; } = GoodsReceiptType.PurchaseReceipt;
    public int WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public int? LocationId { get; set; }
    public string? LocationCode { get; set; }
    public string? SupplierName { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Description { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Draft;
    public int? ConfirmedBy { get; set; }
    public string? ConfirmedByName { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
    public int? JournalEntryId { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalCost { get; set; }
    public List<GoodsReceiptLineDto> Lines { get; set; } = [];
}

public sealed class GoodsReceiptPagedRequest : PagedRequest
{
    public string? ReceiptNo { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public int? WarehouseId { get; set; }
    public GoodsReceiptType? ReceiptType { get; set; }
    public TransactionStatus? Status { get; set; }
    public string? SupplierName { get; set; }
}

public sealed class GoodsIssueLineDto
{
    public int Id { get; set; }
    public int LineNo { get; set; }
    public int ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public int? UomId { get; set; }
    public string? UomCode { get; set; }
    public decimal QtyRequested { get; set; }
    public decimal QtyIssued { get; set; }
    public decimal QtyBase { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public string? Notes { get; set; }
}

public sealed class GoodsIssueDto
{
    public int Id { get; set; }
    public string IssueNo { get; set; } = string.Empty;
    public DateOnly IssueDate { get; set; }
    public GoodsIssueType IssueType { get; set; } = GoodsIssueType.DepartmentalUse;
    public int WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public int? LocationId { get; set; }
    public string? LocationCode { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentCode { get; set; }
    public string? DepartmentName { get; set; }
    public int? CostCenterId { get; set; }
    public string? CostCenterCode { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Description { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Draft;
    public int? ConfirmedBy { get; set; }
    public string? ConfirmedByName { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
    public int? JournalEntryId { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalCost { get; set; }
    public List<GoodsIssueLineDto> Lines { get; set; } = [];
}

public sealed class GoodsIssuePagedRequest : PagedRequest
{
    public string? IssueNo { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public int? WarehouseId { get; set; }
    public int? DepartmentId { get; set; }
    public GoodsIssueType? IssueType { get; set; }
    public TransactionStatus? Status { get; set; }
}

public sealed class StockTransferLineDto
{
    public int Id { get; set; }
    public int LineNo { get; set; }
    public int ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public int? UomId { get; set; }
    public string? UomCode { get; set; }
    public decimal QtyTransfer { get; set; }
    public decimal QtyBase { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public string? Notes { get; set; }
}

public sealed class StockTransferDto
{
    public int Id { get; set; }
    public string TransferNo { get; set; } = string.Empty;
    public DateOnly TransferDate { get; set; }
    public int FromWarehouseId { get; set; }
    public string FromWarehouseCode { get; set; } = string.Empty;
    public int? FromLocationId { get; set; }
    public string? FromLocationCode { get; set; }
    public int ToWarehouseId { get; set; }
    public string ToWarehouseCode { get; set; } = string.Empty;
    public int? ToLocationId { get; set; }
    public string? ToLocationCode { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Description { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Draft;
    public int? ConfirmedBy { get; set; }
    public string? ConfirmedByName { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalCost { get; set; }
    public List<StockTransferLineDto> Lines { get; set; } = [];
}

public sealed class StockTransferPagedRequest : PagedRequest
{
    public string? TransferNo { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public int? FromWarehouseId { get; set; }
    public int? ToWarehouseId { get; set; }
    public TransactionStatus? Status { get; set; }
}

public sealed class StockAdjustmentLineDto
{
    public int Id { get; set; }
    public int LineNo { get; set; }
    public int ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public int? UomId { get; set; }
    public string? UomCode { get; set; }
    public decimal QtyAdjustment { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public string? Notes { get; set; }
}

public sealed class StockAdjustmentDto
{
    public int Id { get; set; }
    public string AdjustmentNo { get; set; } = string.Empty;
    public DateOnly AdjustmentDate { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public int? LocationId { get; set; }
    public string? LocationCode { get; set; }
    public AdjustmentReason Reason { get; set; } = AdjustmentReason.DataCorrection;
    public string? ReferenceNo { get; set; }
    public string? Description { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Draft;
    public int? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public int? ConfirmedBy { get; set; }
    public string? ConfirmedByName { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
    public int? JournalEntryId { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalCost { get; set; }
    public List<StockAdjustmentLineDto> Lines { get; set; } = [];
}

public sealed class StockAdjustmentPagedRequest : PagedRequest
{
    public string? AdjustmentNo { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public int? WarehouseId { get; set; }
    public AdjustmentReason? Reason { get; set; }
    public TransactionStatus? Status { get; set; }
}

public sealed class StockOpnameLineDto
{
    public int Id { get; set; }
    public int LineNo { get; set; }
    public int ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public int? LocationId { get; set; }
    public string? LocationCode { get; set; }
    public decimal QtySystem { get; set; }
    public decimal QtyCounted { get; set; }
    public decimal QtyVariance { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalVarianceValue { get; set; }
    public string? Notes { get; set; }
}

public sealed class StockOpnameDto
{
    public int Id { get; set; }
    public string OpnameNo { get; set; } = string.Empty;
    public DateOnly OpnameDate { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public int? LocationId { get; set; }
    public string? LocationCode { get; set; }
    public string? Description { get; set; }
    public OpnameStatus Status { get; set; } = OpnameStatus.Draft;
    public int? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public int? AdjustmentId { get; set; }
    public decimal TotalVarianceValue { get; set; }
    public List<StockOpnameLineDto> Lines { get; set; } = [];
}

public sealed class StockOpnamePagedRequest : PagedRequest
{
    public string? OpnameNo { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public int? WarehouseId { get; set; }
    public OpnameStatus? Status { get; set; }
}

public sealed class InventoryReportRequest : PagedRequest
{
    public int? WarehouseId { get; set; }
    public int? CategoryId { get; set; }
    public int? ItemId { get; set; }
    public int? DepartmentId { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public int? ThresholdDays { get; set; }
    public StockMovementType? MovementType { get; set; }
}

public sealed class InventoryStockSummaryDto
{
    public int ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public decimal QtyOnHand { get; set; }
    public decimal QtyAvailable { get; set; }
    public decimal AvgCost { get; set; }
    public decimal TotalValue { get; set; }
}

public sealed class InventoryStockByWarehouseDto
{
    public int WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public decimal QtyOnHand { get; set; }
    public decimal QtyAvailable { get; set; }
    public decimal TotalValue { get; set; }
}

public sealed class InventoryStockByCategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal QtyOnHand { get; set; }
    public decimal QtyAvailable { get; set; }
    public decimal TotalValue { get; set; }
}

public sealed class InventoryStockCardDto
{
    public DateOnly MovementDate { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public string? LocationCode { get; set; }
    public StockMovementType MovementType { get; set; }
    public decimal QtyIn { get; set; }
    public decimal QtyOut { get; set; }
    public decimal QtyBalance { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public string SourceTable { get; set; } = string.Empty;
    public int SourceId { get; set; }
    public string? Notes { get; set; }
}

public sealed class InventoryLowStockDto
{
    public int ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public decimal QtyAvailable { get; set; }
    public decimal MinStock { get; set; }
    public decimal Difference { get; set; }
}

public sealed class InventoryValuationDto
{
    public int ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public decimal QtyOnHand { get; set; }
    public decimal AvgCost { get; set; }
    public decimal TotalValue { get; set; }
}

public sealed class InventoryAgingDto
{
    public int ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public DateOnly? LastMovementDate { get; set; }
    public int DaysSinceLastMovement { get; set; }
    public decimal QtyOnHand { get; set; }
    public decimal TotalValue { get; set; }
}

public sealed class InventoryMovementHistoryDto
{
    public DateOnly MovementDate { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public string? LocationCode { get; set; }
    public StockMovementType MovementType { get; set; }
    public decimal QtyIn { get; set; }
    public decimal QtyOut { get; set; }
    public decimal QtyBalance { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public string SourceTable { get; set; } = string.Empty;
    public int SourceId { get; set; }
    public string? Notes { get; set; }
}

public sealed class InventoryReceiptSummaryDto
{
    public int Id { get; set; }
    public string ReceiptNo { get; set; } = string.Empty;
    public DateOnly ReceiptDate { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public GoodsReceiptType ReceiptType { get; set; }
    public TransactionStatus Status { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalCost { get; set; }
}

public sealed class InventoryIssueSummaryDto
{
    public int Id { get; set; }
    public string IssueNo { get; set; } = string.Empty;
    public DateOnly IssueDate { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public GoodsIssueType IssueType { get; set; }
    public TransactionStatus Status { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalCost { get; set; }
}

public sealed class InventoryTransferSummaryDto
{
    public int Id { get; set; }
    public string TransferNo { get; set; } = string.Empty;
    public DateOnly TransferDate { get; set; }
    public string FromWarehouseCode { get; set; } = string.Empty;
    public string ToWarehouseCode { get; set; } = string.Empty;
    public TransactionStatus Status { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalCost { get; set; }
}

public sealed class InventoryAdjustmentSummaryDto
{
    public int Id { get; set; }
    public string AdjustmentNo { get; set; } = string.Empty;
    public DateOnly AdjustmentDate { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public AdjustmentReason Reason { get; set; }
    public TransactionStatus Status { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalCost { get; set; }
}
