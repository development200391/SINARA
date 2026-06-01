using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;

namespace ERP.Web.Services;

public interface IInventoryApiClient
{
    Task<PagedResult<ItemCategoryDto>?> GetCategoriesAsync(string accessToken, ItemCategoryPagedRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryOptionDto>> GetCategoryOptionsAsync(string accessToken, CancellationToken ct = default);
    Task<ItemCategoryDto?> GetCategoryByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ItemCategoryDto?> CreateCategoryAsync(string accessToken, ItemCategoryDto request, CancellationToken ct = default);
    Task<ItemCategoryDto?> UpdateCategoryAsync(string accessToken, int id, ItemCategoryDto request, CancellationToken ct = default);
    Task<bool> DeleteCategoryAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<UnitOfMeasureDto>?> GetUnitsAsync(string accessToken, UnitOfMeasurePagedRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryOptionDto>> GetUnitOptionsAsync(string accessToken, CancellationToken ct = default);
    Task<UnitOfMeasureDto?> GetUnitByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<UnitOfMeasureDto?> CreateUnitAsync(string accessToken, UnitOfMeasureDto request, CancellationToken ct = default);
    Task<UnitOfMeasureDto?> UpdateUnitAsync(string accessToken, int id, UnitOfMeasureDto request, CancellationToken ct = default);
    Task<bool> DeleteUnitAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<BrandDto>?> GetBrandsAsync(string accessToken, BrandPagedRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryOptionDto>> GetBrandOptionsAsync(string accessToken, CancellationToken ct = default);
    Task<BrandDto?> GetBrandByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<BrandDto?> CreateBrandAsync(string accessToken, BrandDto request, CancellationToken ct = default);
    Task<BrandDto?> UpdateBrandAsync(string accessToken, int id, BrandDto request, CancellationToken ct = default);
    Task<bool> DeleteBrandAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<ItemDto>?> GetItemsAsync(string accessToken, ItemPagedRequest request, CancellationToken ct = default);
    Task<PagedResult<ItemDto>?> GetLowStockItemsAsync(string accessToken, ItemPagedRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryOptionDto>> GetItemOptionsAsync(string accessToken, CancellationToken ct = default);
    Task<ItemDto?> GetItemByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ItemDto?> CreateItemAsync(string accessToken, ItemDto request, CancellationToken ct = default);
    Task<ItemDto?> UpdateItemAsync(string accessToken, int id, ItemDto request, CancellationToken ct = default);
    Task<bool> DeleteItemAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<ItemUnitConversionDto>?> GetItemConversionsAsync(string accessToken, ItemUnitConversionPagedRequest request, CancellationToken ct = default);
    Task<ItemUnitConversionDto?> GetItemConversionByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ItemUnitConversionDto?> CreateItemConversionAsync(string accessToken, ItemUnitConversionDto request, CancellationToken ct = default);
    Task<ItemUnitConversionDto?> UpdateItemConversionAsync(string accessToken, int id, ItemUnitConversionDto request, CancellationToken ct = default);
    Task<bool> DeleteItemConversionAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<WarehouseDto>?> GetWarehousesAsync(string accessToken, WarehousePagedRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryOptionDto>> GetWarehouseOptionsAsync(string accessToken, CancellationToken ct = default);
    Task<WarehouseDto?> GetWarehouseByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<WarehouseDto?> CreateWarehouseAsync(string accessToken, WarehouseDto request, CancellationToken ct = default);
    Task<WarehouseDto?> UpdateWarehouseAsync(string accessToken, int id, WarehouseDto request, CancellationToken ct = default);
    Task<bool> DeleteWarehouseAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<WarehouseLocationDto>?> GetWarehouseLocationsAsync(string accessToken, int warehouseId, WarehouseLocationPagedRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryOptionDto>> GetWarehouseLocationOptionsAsync(string accessToken, int warehouseId, CancellationToken ct = default);
    Task<WarehouseLocationDto?> GetWarehouseLocationByIdAsync(string accessToken, int warehouseId, int id, CancellationToken ct = default);
    Task<WarehouseLocationDto?> CreateWarehouseLocationAsync(string accessToken, int warehouseId, WarehouseLocationDto request, CancellationToken ct = default);
    Task<WarehouseLocationDto?> UpdateWarehouseLocationAsync(string accessToken, int warehouseId, int id, WarehouseLocationDto request, CancellationToken ct = default);
    Task<bool> DeleteWarehouseLocationAsync(string accessToken, int warehouseId, int id, CancellationToken ct = default);

    Task<PagedResult<StockBalanceDto>?> GetWarehouseStockAsync(string accessToken, int warehouseId, StockBalancePagedRequest request, CancellationToken ct = default);
    Task<PagedResult<StockBalanceDto>?> GetStockBalancesAsync(string accessToken, StockBalancePagedRequest request, CancellationToken ct = default);
    Task<PagedResult<GoodsReceiptDto>?> GetGoodsReceiptsAsync(string accessToken, GoodsReceiptPagedRequest request, CancellationToken ct = default);
    Task<GoodsReceiptDto?> GetGoodsReceiptByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<GoodsReceiptDto?> CreateGoodsReceiptAsync(string accessToken, GoodsReceiptDto request, CancellationToken ct = default);
    Task<GoodsReceiptDto?> UpdateGoodsReceiptAsync(string accessToken, int id, GoodsReceiptDto request, CancellationToken ct = default);
    Task<bool> DeleteGoodsReceiptAsync(string accessToken, int id, CancellationToken ct = default);
    Task<bool> ConfirmGoodsReceiptAsync(string accessToken, int id, CancellationToken ct = default);
    Task<bool> CancelGoodsReceiptAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<GoodsIssueDto>?> GetGoodsIssuesAsync(string accessToken, GoodsIssuePagedRequest request, CancellationToken ct = default);
    Task<GoodsIssueDto?> GetGoodsIssueByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<GoodsIssueDto?> CreateGoodsIssueAsync(string accessToken, GoodsIssueDto request, CancellationToken ct = default);
    Task<GoodsIssueDto?> UpdateGoodsIssueAsync(string accessToken, int id, GoodsIssueDto request, CancellationToken ct = default);
    Task<bool> DeleteGoodsIssueAsync(string accessToken, int id, CancellationToken ct = default);
    Task<bool> ConfirmGoodsIssueAsync(string accessToken, int id, CancellationToken ct = default);
    Task<bool> CancelGoodsIssueAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<StockTransferDto>?> GetTransfersAsync(string accessToken, StockTransferPagedRequest request, CancellationToken ct = default);
    Task<StockTransferDto?> GetTransferByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<StockTransferDto?> CreateTransferAsync(string accessToken, StockTransferDto request, CancellationToken ct = default);
    Task<StockTransferDto?> UpdateTransferAsync(string accessToken, int id, StockTransferDto request, CancellationToken ct = default);
    Task<bool> DeleteTransferAsync(string accessToken, int id, CancellationToken ct = default);
    Task<bool> ConfirmTransferAsync(string accessToken, int id, CancellationToken ct = default);
    Task<bool> CancelTransferAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<StockAdjustmentDto>?> GetAdjustmentsAsync(string accessToken, StockAdjustmentPagedRequest request, CancellationToken ct = default);
    Task<StockAdjustmentDto?> GetAdjustmentByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<StockAdjustmentDto?> CreateAdjustmentAsync(string accessToken, StockAdjustmentDto request, CancellationToken ct = default);
    Task<StockAdjustmentDto?> UpdateAdjustmentAsync(string accessToken, int id, StockAdjustmentDto request, CancellationToken ct = default);
    Task<bool> DeleteAdjustmentAsync(string accessToken, int id, CancellationToken ct = default);
    Task<bool> ApproveAdjustmentAsync(string accessToken, int id, CancellationToken ct = default);
    Task<bool> ConfirmAdjustmentAsync(string accessToken, int id, CancellationToken ct = default);
    Task<bool> CancelAdjustmentAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<StockOpnameDto>?> GetOpnamesAsync(string accessToken, StockOpnamePagedRequest request, CancellationToken ct = default);
    Task<StockOpnameDto?> GetOpnameByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<StockOpnameDto?> CreateOpnameAsync(string accessToken, StockOpnameDto request, CancellationToken ct = default);
    Task<bool> UpdateOpnameAsync(string accessToken, int id, StockOpnameDto request, CancellationToken ct = default);
    Task<bool> StartOpnameAsync(string accessToken, int id, CancellationToken ct = default);
    Task<IReadOnlyList<StockOpnameLineDto>> GetOpnameLinesAsync(string accessToken, int id, CancellationToken ct = default);
    Task<bool> UpdateOpnameLinesAsync(string accessToken, int id, IReadOnlyList<StockOpnameLineDto> lines, CancellationToken ct = default);
    Task<bool> UpdateOpnameLineAsync(string accessToken, int id, int lineId, StockOpnameLineDto line, CancellationToken ct = default);
    Task<bool> CompleteOpnameAsync(string accessToken, int id, CancellationToken ct = default);
    Task<bool> ApproveOpnameAsync(string accessToken, int id, CancellationToken ct = default);
    Task<bool> CancelOpnameAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<StockBalanceDto>?> GetStockBalanceReportAsync(string accessToken, StockBalancePagedRequest request, CancellationToken ct = default);
    Task<decimal?> GetStockAvailableAsync(string accessToken, int itemId, int warehouseId, int? locationId, CancellationToken ct = default);
    Task<PagedResult<InventoryMovementHistoryDto>?> GetStockMovementsAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default);
    Task<PagedResult<InventoryMovementHistoryDto>?> GetStockCardRawAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default);
    Task<PagedResult<InventoryValuationDto>?> GetStockValuationRawAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default);

    Task<PagedResult<InventoryStockSummaryDto>?> GetStockSummaryReportAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryStockByWarehouseDto>> GetStockByWarehouseReportAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryStockByCategoryDto>> GetStockByCategoryReportAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default);
    Task<PagedResult<InventoryStockCardDto>?> GetStockCardReportAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default);
    Task<PagedResult<InventoryLowStockDto>?> GetLowStockReportAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default);
    Task<PagedResult<InventoryValuationDto>?> GetInventoryValuationReportAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default);
    Task<PagedResult<InventoryAgingDto>?> GetInventoryAgingReportAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default);
    Task<PagedResult<InventoryMovementHistoryDto>?> GetMovementHistoryReportAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default);
    Task<PagedResult<InventoryReceiptSummaryDto>?> GetReceiptSummaryReportAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default);
    Task<PagedResult<InventoryIssueSummaryDto>?> GetIssueSummaryReportAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default);
    Task<PagedResult<InventoryTransferSummaryDto>?> GetTransferSummaryReportAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default);
    Task<PagedResult<InventoryAdjustmentSummaryDto>?> GetAdjustmentSummaryReportAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default);
}
