namespace ERP.Application.Services.Inventory;

public static class InventoryCacheKeys
{
    public static readonly TimeSpan ItemsActiveTtl = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan LowStockTtl = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan CategoriesTreeTtl = TimeSpan.FromMinutes(60);
    public static readonly TimeSpan UnitsAllTtl = TimeSpan.FromMinutes(60);
    public static readonly TimeSpan WarehousesActiveTtl = TimeSpan.FromMinutes(60);
    public static readonly TimeSpan WarehouseLocationsTtl = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan StockBalanceTtl = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan StockAvailableTtl = TimeSpan.FromMinutes(2);
    public static readonly TimeSpan StockSummaryWarehouseTtl = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan ReportValuationTtl = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan NumberSequenceTtl = TimeSpan.FromMinutes(5);

    public const string ItemsActive = "ERP_inv:items:active";
    public const string ItemsLowStock = "ERP_inv:items:low-stock";
    public const string CategoriesTree = "ERP_inv:categories:tree";
    public const string UnitsAll = "ERP_inv:units:all";
    public const string WarehousesActive = "ERP_inv:warehouses:active";
    public const string ReportLowStock = "ERP_inv:report:low-stock";

    public static string Item(int itemId) => $"ERP_inv:item:{itemId}";

    public static string WarehouseLocations(int warehouseId) =>
        $"ERP_inv:warehouse:{warehouseId}:locations";

    public static string StockBalance(int itemId, int warehouseId) =>
        $"ERP_inv:stock:balance:{itemId}:{warehouseId}";

    public static string StockBalance(int itemId, int warehouseId, int locationId) =>
        $"ERP_inv:stock:balance:{itemId}:{warehouseId}:{locationId}";

    public static string StockAvailable(int itemId, int warehouseId) =>
        $"ERP_inv:stock:available:{itemId}:{warehouseId}";

    public static string StockSummaryWarehouse(int warehouseId) =>
        $"ERP_inv:stock:summary:warehouse:{warehouseId}";

    public static string ReportValuation(DateOnly date) =>
        $"ERP_inv:report:valuation:{date:yyyy-MM-dd}";

    public static string GoodsReceiptNumberLatest(int year) =>
        $"ERP_inv:gr:number:latest:{year}";

    public static string GoodsIssueNumberLatest(int year) =>
        $"ERP_inv:gi:number:latest:{year}";

    public static string TransferNumberLatest(int year) =>
        $"ERP_inv:trf:number:latest:{year}";

    public static string AdjustmentNumberLatest(int year) =>
        $"ERP_inv:adj:number:latest:{year}";

    public static string OpnameNumberLatest(int year) =>
        $"ERP_inv:opn:number:latest:{year}";
}
