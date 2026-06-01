using System.ComponentModel.DataAnnotations;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;
using ERP.Domain.Enums.Inventory;

namespace ERP.Web.ViewModels.Inventory;

public sealed class InventoryCategoriesIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "code";
    public string SortDirection { get; init; } = "asc";
    public string? CodeFilter { get; init; }
    public string? NameFilter { get; init; }
    public int? ParentCategoryIdFilter { get; init; }
    public bool? IsActiveFilter { get; init; }
    public IReadOnlyList<InventoryOptionDto> ParentCategories { get; init; } = [];
    public PagedResult<ItemCategoryDto> Items { get; init; } = PagedResult<ItemCategoryDto>.Create([], 0, 1, 20);
}

public sealed class InventoryCategoryEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [StringLength(30)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public int? ParentCategoryId { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public IReadOnlyList<InventoryOptionDto> ParentCategories { get; set; } = [];
}

public sealed class InventoryUnitsIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "code";
    public string SortDirection { get; init; } = "asc";
    public string? CodeFilter { get; init; }
    public string? NameFilter { get; init; }
    public bool? IsActiveFilter { get; init; }
    public PagedResult<UnitOfMeasureDto> Items { get; init; } = PagedResult<UnitOfMeasureDto>.Create([], 0, 1, 20);
}

public sealed class InventoryUnitEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class InventoryBrandsIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "name";
    public string SortDirection { get; init; } = "asc";
    public string? NameFilter { get; init; }
    public bool? IsActiveFilter { get; init; }
    public PagedResult<BrandDto> Items { get; init; } = PagedResult<BrandDto>.Create([], 0, 1, 20);
}

public sealed class InventoryBrandEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class InventoryItemsIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "itemcode";
    public string SortDirection { get; init; } = "asc";
    public string? ItemCodeFilter { get; init; }
    public string? SkuFilter { get; init; }
    public string? NameFilter { get; init; }
    public int? CategoryIdFilter { get; init; }
    public int? BrandIdFilter { get; init; }
    public ItemType? TypeFilter { get; init; }
    public ItemStatus? StatusFilter { get; init; }
    public bool? IsActiveFilter { get; init; }
    public decimal? MinStockFromFilter { get; init; }
    public decimal? MinStockToFilter { get; init; }
    public decimal? ReorderPointFromFilter { get; init; }
    public decimal? ReorderPointToFilter { get; init; }
    public bool LowStockOnly { get; init; }
    public IReadOnlyList<InventoryOptionDto> Categories { get; init; } = [];
    public IReadOnlyList<InventoryOptionDto> Brands { get; init; } = [];
    public PagedResult<ItemDto> Items { get; init; } = PagedResult<ItemDto>.Create([], 0, 1, 20);
}

public sealed class InventoryItemEditViewModel
{
    public int? Id { get; set; }

    [StringLength(30)]
    public string ItemCode { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Sku { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? Description { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    public int? BrandId { get; set; }

    [Required]
    public ItemType Type { get; set; } = ItemType.Product;

    [Range(1, int.MaxValue)]
    public int BaseUomId { get; set; }

    public int? PurchaseUomId { get; set; }

    [Required]
    public ItemStatus Status { get; set; } = ItemStatus.Active;

    [Required]
    public ValuationMethod ValuationMethod { get; set; } = ValuationMethod.WeightedAverageCost;

    [Range(0, 9999999999999999)]
    public decimal? LastPurchasePrice { get; set; }

    [Range(0, 9999999999999999)]
    public decimal AvgCost { get; set; }

    [Range(0, 9999999999999999)]
    public decimal MinStock { get; set; }

    [Range(0, 9999999999999999)]
    public decimal MaxStock { get; set; }

    [Range(0, 9999999999999999)]
    public decimal ReorderPoint { get; set; }

    [Range(0, 99999)]
    public int LeadTimeDays { get; set; }

    public int? InventoryAccountId { get; set; }
    public int? CogsAccountId { get; set; }
    public int? AdjustmentAccountId { get; set; }

    [StringLength(4000)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public IReadOnlyList<InventoryOptionDto> CategoryOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> BrandOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> UomOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> AccountOptions { get; set; } = [];
}

public sealed class InventoryItemConversionsIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "itemcode";
    public string SortDirection { get; init; } = "asc";
    public int? ItemIdFilter { get; init; }
    public int? FromUomIdFilter { get; init; }
    public int? ToUomIdFilter { get; init; }
    public bool? IsActiveFilter { get; init; }
    public decimal? FactorFromFilter { get; init; }
    public decimal? FactorToFilter { get; init; }
    public IReadOnlyList<InventoryOptionDto> ItemOptions { get; init; } = [];
    public IReadOnlyList<InventoryOptionDto> UomOptions { get; init; } = [];
    public PagedResult<ItemUnitConversionDto> Items { get; init; } = PagedResult<ItemUnitConversionDto>.Create([], 0, 1, 20);
}

public sealed class InventoryItemConversionEditViewModel
{
    public int? Id { get; set; }

    [Range(1, int.MaxValue)]
    public int ItemId { get; set; }

    [Range(1, int.MaxValue)]
    public int FromUomId { get; set; }

    [Range(1, int.MaxValue)]
    public int ToUomId { get; set; }

    [Range(0.000001, 9999999999999999)]
    public decimal ConversionFactor { get; set; } = 1m;

    public bool IsActive { get; set; } = true;

    public IReadOnlyList<InventoryOptionDto> ItemOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> UomOptions { get; set; } = [];
}

public sealed class InventoryWarehousesIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "code";
    public string SortDirection { get; init; } = "asc";
    public string? CodeFilter { get; init; }
    public string? NameFilter { get; init; }
    public int? ManagerIdFilter { get; init; }
    public int? CostCenterIdFilter { get; init; }
    public bool? IsTransitFilter { get; init; }
    public bool? IsActiveFilter { get; init; }
    public IReadOnlyList<InventoryOptionDto> ManagerOptions { get; init; } = [];
    public IReadOnlyList<InventoryOptionDto> CostCenterOptions { get; init; } = [];
    public PagedResult<WarehouseDto> Items { get; init; } = PagedResult<WarehouseDto>.Create([], 0, 1, 20);
}

public sealed class InventoryWarehouseEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? Description { get; set; }

    [StringLength(4000)]
    public string? Address { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    public int? ManagerId { get; set; }
    public int? CostCenterId { get; set; }
    public bool IsTransit { get; set; }
    public bool IsActive { get; set; } = true;

    public IReadOnlyList<InventoryOptionDto> ManagerOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> CostCenterOptions { get; set; } = [];
}

public sealed class InventoryWarehouseLocationsIndexViewModel
{
    public int WarehouseId { get; init; }
    public string WarehouseCode { get; init; } = string.Empty;
    public string WarehouseName { get; init; } = string.Empty;
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "code";
    public string SortDirection { get; init; } = "asc";
    public string? CodeFilter { get; init; }
    public string? NameFilter { get; init; }
    public bool? IsDefaultFilter { get; init; }
    public bool? IsActiveFilter { get; init; }
    public PagedResult<WarehouseLocationDto> Items { get; init; } = PagedResult<WarehouseLocationDto>.Create([], 0, 1, 20);
}

public sealed class InventoryWarehouseLocationEditViewModel
{
    public int WarehouseId { get; set; }
    public int? Id { get; set; }

    [Required]
    [StringLength(30)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? Description { get; set; }

    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class InventoryWarehouseStockIndexViewModel
{
    public int WarehouseId { get; init; }
    public string WarehouseCode { get; init; } = string.Empty;
    public string WarehouseName { get; init; } = string.Empty;
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "itemcode";
    public string SortDirection { get; init; } = "asc";
    public int? ItemIdFilter { get; init; }
    public int? LocationIdFilter { get; init; }
    public IReadOnlyList<InventoryOptionDto> ItemOptions { get; init; } = [];
    public IReadOnlyList<InventoryOptionDto> LocationOptions { get; init; } = [];
    public PagedResult<StockBalanceDto> Items { get; init; } = PagedResult<StockBalanceDto>.Create([], 0, 1, 20);
}

public sealed class InventoryGoodsReceiptsIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "receiptdate";
    public string SortDirection { get; init; } = "desc";
    public string? ReceiptNoFilter { get; init; }
    public DateOnly? DateFromFilter { get; init; }
    public DateOnly? DateToFilter { get; init; }
    public int? WarehouseIdFilter { get; init; }
    public GoodsReceiptType? ReceiptTypeFilter { get; init; }
    public TransactionStatus? StatusFilter { get; init; }
    public string? SupplierNameFilter { get; init; }
    public IReadOnlyList<InventoryOptionDto> WarehouseOptions { get; init; } = [];
    public PagedResult<GoodsReceiptDto> Items { get; init; } = PagedResult<GoodsReceiptDto>.Create([], 0, 1, 20);
}

public sealed class InventoryGoodsReceiptEditViewModel
{
    public int? Id { get; set; }

    [Required]
    public DateOnly ReceiptDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.Date);

    [Required]
    public GoodsReceiptType ReceiptType { get; set; } = GoodsReceiptType.PurchaseReceipt;

    [Range(1, int.MaxValue)]
    public int WarehouseId { get; set; }

    public int? LocationId { get; set; }

    [StringLength(200)]
    public string? SupplierName { get; set; }

    [StringLength(100)]
    public string? ReferenceNo { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    public TransactionStatus Status { get; set; } = TransactionStatus.Draft;

    [Range(1, int.MaxValue)]
    public int ItemId { get; set; }

    public int? UomId { get; set; }

    [Range(0.0001, 9999999999999999d)]
    public decimal QtyReceived { get; set; } = 1m;

    [Range(0.0001, 9999999999999999d)]
    public decimal QtyBase { get; set; } = 1m;

    [Range(0, 9999999999999999d)]
    public decimal UnitCost { get; set; }

    [StringLength(1000)]
    public string? LineNotes { get; set; }

    public IReadOnlyList<InventoryOptionDto> WarehouseOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> LocationOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> ItemOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> UomOptions { get; set; } = [];
}

public sealed class InventoryGoodsIssuesIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "issuedate";
    public string SortDirection { get; init; } = "desc";
    public string? IssueNoFilter { get; init; }
    public DateOnly? DateFromFilter { get; init; }
    public DateOnly? DateToFilter { get; init; }
    public int? WarehouseIdFilter { get; init; }
    public int? DepartmentIdFilter { get; init; }
    public GoodsIssueType? IssueTypeFilter { get; init; }
    public TransactionStatus? StatusFilter { get; init; }
    public IReadOnlyList<InventoryOptionDto> WarehouseOptions { get; init; } = [];
    public IReadOnlyList<InventoryOptionDto> DepartmentOptions { get; init; } = [];
    public PagedResult<GoodsIssueDto> Items { get; init; } = PagedResult<GoodsIssueDto>.Create([], 0, 1, 20);
}

public sealed class InventoryGoodsIssueEditViewModel
{
    public int? Id { get; set; }

    [Required]
    public DateOnly IssueDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.Date);

    [Required]
    public GoodsIssueType IssueType { get; set; } = GoodsIssueType.DepartmentalUse;

    [Range(1, int.MaxValue)]
    public int WarehouseId { get; set; }

    public int? LocationId { get; set; }
    public int? DepartmentId { get; set; }
    public int? CostCenterId { get; set; }

    [StringLength(100)]
    public string? ReferenceNo { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    public TransactionStatus Status { get; set; } = TransactionStatus.Draft;

    [Range(1, int.MaxValue)]
    public int ItemId { get; set; }

    public int? UomId { get; set; }

    [Range(0.0001, 9999999999999999d)]
    public decimal QtyRequested { get; set; } = 1m;

    [Range(0.0001, 9999999999999999d)]
    public decimal QtyIssued { get; set; } = 1m;

    [Range(0.0001, 9999999999999999d)]
    public decimal QtyBase { get; set; } = 1m;

    [Range(0, 9999999999999999d)]
    public decimal UnitCost { get; set; }

    [StringLength(1000)]
    public string? LineNotes { get; set; }

    public IReadOnlyList<InventoryOptionDto> WarehouseOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> LocationOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> DepartmentOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> CostCenterOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> ItemOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> UomOptions { get; set; } = [];
}

public sealed class InventoryTransfersIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "transferdate";
    public string SortDirection { get; init; } = "desc";
    public string? TransferNoFilter { get; init; }
    public DateOnly? DateFromFilter { get; init; }
    public DateOnly? DateToFilter { get; init; }
    public int? FromWarehouseIdFilter { get; init; }
    public int? ToWarehouseIdFilter { get; init; }
    public TransactionStatus? StatusFilter { get; init; }
    public IReadOnlyList<InventoryOptionDto> WarehouseOptions { get; init; } = [];
    public PagedResult<StockTransferDto> Items { get; init; } = PagedResult<StockTransferDto>.Create([], 0, 1, 20);
}

public sealed class InventoryTransferEditViewModel
{
    public int? Id { get; set; }

    [Required]
    public DateOnly TransferDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.Date);

    [Range(1, int.MaxValue)]
    public int FromWarehouseId { get; set; }

    public int? FromLocationId { get; set; }

    [Range(1, int.MaxValue)]
    public int ToWarehouseId { get; set; }

    public int? ToLocationId { get; set; }

    [StringLength(100)]
    public string? ReferenceNo { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    public TransactionStatus Status { get; set; } = TransactionStatus.Draft;

    [Range(1, int.MaxValue)]
    public int ItemId { get; set; }

    public int? UomId { get; set; }

    [Range(0.0001, 9999999999999999d)]
    public decimal QtyTransfer { get; set; } = 1m;

    [Range(0.0001, 9999999999999999d)]
    public decimal QtyBase { get; set; } = 1m;

    [Range(0, 9999999999999999d)]
    public decimal UnitCost { get; set; }

    [StringLength(1000)]
    public string? LineNotes { get; set; }

    public IReadOnlyList<InventoryOptionDto> WarehouseOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> FromLocationOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> ToLocationOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> ItemOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> UomOptions { get; set; } = [];
}

public sealed class InventoryAdjustmentsIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "adjustmentdate";
    public string SortDirection { get; init; } = "desc";
    public string? AdjustmentNoFilter { get; init; }
    public DateOnly? DateFromFilter { get; init; }
    public DateOnly? DateToFilter { get; init; }
    public int? WarehouseIdFilter { get; init; }
    public AdjustmentReason? ReasonFilter { get; init; }
    public TransactionStatus? StatusFilter { get; init; }
    public IReadOnlyList<InventoryOptionDto> WarehouseOptions { get; init; } = [];
    public PagedResult<StockAdjustmentDto> Items { get; init; } = PagedResult<StockAdjustmentDto>.Create([], 0, 1, 20);
}

public sealed class InventoryAdjustmentEditViewModel
{
    public int? Id { get; set; }

    [Required]
    public DateOnly AdjustmentDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.Date);

    [Range(1, int.MaxValue)]
    public int WarehouseId { get; set; }

    public int? LocationId { get; set; }

    [Required]
    public AdjustmentReason Reason { get; set; } = AdjustmentReason.DataCorrection;

    [StringLength(100)]
    public string? ReferenceNo { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    public TransactionStatus Status { get; set; } = TransactionStatus.Draft;

    [Range(1, int.MaxValue)]
    public int ItemId { get; set; }

    public int? UomId { get; set; }

    [Range(-9999999999999999d, 9999999999999999d)]
    public decimal QtyAdjustment { get; set; }

    [Range(0, 9999999999999999d)]
    public decimal UnitCost { get; set; }

    [StringLength(1000)]
    public string? LineNotes { get; set; }

    public IReadOnlyList<InventoryOptionDto> WarehouseOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> LocationOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> ItemOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> UomOptions { get; set; } = [];
}

public sealed class InventoryOpnamesIndexViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "opnamedate";
    public string SortDirection { get; init; } = "desc";
    public string? OpnameNoFilter { get; init; }
    public DateOnly? DateFromFilter { get; init; }
    public DateOnly? DateToFilter { get; init; }
    public int? WarehouseIdFilter { get; init; }
    public OpnameStatus? StatusFilter { get; init; }
    public IReadOnlyList<InventoryOptionDto> WarehouseOptions { get; init; } = [];
    public PagedResult<StockOpnameDto> Items { get; init; } = PagedResult<StockOpnameDto>.Create([], 0, 1, 20);
}

public sealed class InventoryOpnameEditViewModel
{
    public int? Id { get; set; }

    [Required]
    public DateOnly OpnameDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.Date);

    [Range(1, int.MaxValue)]
    public int WarehouseId { get; set; }

    public int? LocationId { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    public OpnameStatus Status { get; set; } = OpnameStatus.Draft;

    public IReadOnlyList<InventoryOptionDto> WarehouseOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> LocationOptions { get; set; } = [];
}

public sealed class InventoryStockSummaryReportViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "totalvalue";
    public string SortDirection { get; init; } = "desc";
    public int? WarehouseIdFilter { get; init; }
    public int? CategoryIdFilter { get; init; }
    public int? ItemIdFilter { get; init; }
    public int? ThresholdDaysFilter { get; init; }
    public IReadOnlyList<InventoryOptionDto> WarehouseOptions { get; init; } = [];
    public IReadOnlyList<InventoryOptionDto> CategoryOptions { get; init; } = [];
    public IReadOnlyList<InventoryOptionDto> ItemOptions { get; init; } = [];
    public PagedResult<InventoryStockSummaryDto> Items { get; init; } = PagedResult<InventoryStockSummaryDto>.Create([], 0, 1, 20);
}

public sealed class InventoryLowStockReportViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "itemcode";
    public string SortDirection { get; init; } = "asc";
    public int? WarehouseIdFilter { get; init; }
    public int? CategoryIdFilter { get; init; }
    public int? ItemIdFilter { get; init; }
    public IReadOnlyList<InventoryOptionDto> WarehouseOptions { get; init; } = [];
    public IReadOnlyList<InventoryOptionDto> CategoryOptions { get; init; } = [];
    public IReadOnlyList<InventoryOptionDto> ItemOptions { get; init; } = [];
    public PagedResult<InventoryLowStockDto> Items { get; init; } = PagedResult<InventoryLowStockDto>.Create([], 0, 1, 20);
}

public sealed class InventoryValuationReportViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "totalvalue";
    public string SortDirection { get; init; } = "desc";
    public int? WarehouseIdFilter { get; init; }
    public int? CategoryIdFilter { get; init; }
    public int? ItemIdFilter { get; init; }
    public IReadOnlyList<InventoryOptionDto> WarehouseOptions { get; init; } = [];
    public IReadOnlyList<InventoryOptionDto> CategoryOptions { get; init; } = [];
    public IReadOnlyList<InventoryOptionDto> ItemOptions { get; init; } = [];
    public PagedResult<InventoryValuationDto> Items { get; init; } = PagedResult<InventoryValuationDto>.Create([], 0, 1, 20);
}

public sealed class InventoryAgingReportViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "dayssincelastmovement";
    public string SortDirection { get; init; } = "desc";
    public int? WarehouseIdFilter { get; init; }
    public int? CategoryIdFilter { get; init; }
    public int? ItemIdFilter { get; init; }
    public int? ThresholdDaysFilter { get; init; }
    public IReadOnlyList<InventoryOptionDto> WarehouseOptions { get; init; } = [];
    public IReadOnlyList<InventoryOptionDto> CategoryOptions { get; init; } = [];
    public IReadOnlyList<InventoryOptionDto> ItemOptions { get; init; } = [];
    public PagedResult<InventoryAgingDto> Items { get; init; } = PagedResult<InventoryAgingDto>.Create([], 0, 1, 20);
}

public sealed class InventoryMovementReportViewModel
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "movementdate";
    public string SortDirection { get; init; } = "desc";
    public int? WarehouseIdFilter { get; init; }
    public int? ItemIdFilter { get; init; }
    public DateOnly? DateFromFilter { get; init; }
    public DateOnly? DateToFilter { get; init; }
    public StockMovementType? MovementTypeFilter { get; init; }
    public IReadOnlyList<InventoryOptionDto> WarehouseOptions { get; init; } = [];
    public IReadOnlyList<InventoryOptionDto> ItemOptions { get; init; } = [];
    public PagedResult<InventoryMovementHistoryDto> Items { get; init; } = PagedResult<InventoryMovementHistoryDto>.Create([], 0, 1, 20);
}

public sealed class InventorySimpleReportViewModel<T>
{
    public string? Search { get; init; }
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "";
    public string SortDirection { get; init; } = "desc";
    public int? WarehouseIdFilter { get; init; }
    public DateOnly? DateFromFilter { get; init; }
    public DateOnly? DateToFilter { get; init; }
    public IReadOnlyList<InventoryOptionDto> WarehouseOptions { get; init; } = [];
    public PagedResult<T> Items { get; init; } = PagedResult<T>.Create([], 0, 1, 20);
}

