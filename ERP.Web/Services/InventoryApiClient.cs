using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;

namespace ERP.Web.Services;

public sealed class InventoryApiClient(HttpClient httpClient, ILogger<InventoryApiClient> logger) : ApiClientBase(httpClient, logger, "Inventory"), IInventoryApiClient
{
    public Task<PagedResult<ItemCategoryDto>?> GetCategoriesAsync(string accessToken, ItemCategoryPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            parameters.Add($"code={Uri.EscapeDataString(request.Code.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            parameters.Add($"name={Uri.EscapeDataString(request.Name.Trim())}");
        }

        if (request.ParentCategoryId.HasValue)
        {
            parameters.Add($"parentCategoryId={request.ParentCategoryId.Value}");
        }

        if (request.IsActive.HasValue)
        {
            parameters.Add($"isActive={(request.IsActive.Value ? "true" : "false")}");
        }

        var query = $"api/v1/inventory/categories?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<ItemCategoryDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public async Task<IReadOnlyList<InventoryOptionDto>> GetCategoryOptionsAsync(string accessToken, CancellationToken ct = default)
        => await SendAsync<IReadOnlyList<InventoryOptionDto>>(HttpMethod.Get, "api/v1/inventory/categories/options", accessToken, null, ct) ?? [];

    public Task<ItemCategoryDto?> GetCategoryByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<ItemCategoryDto>(HttpMethod.Get, $"api/v1/inventory/categories/{id}", accessToken, null, ct);

    public Task<ItemCategoryDto?> CreateCategoryAsync(string accessToken, ItemCategoryDto request, CancellationToken ct = default)
        => SendAsync<ItemCategoryDto>(HttpMethod.Post, "api/v1/inventory/categories", accessToken, request, ct);

    public Task<ItemCategoryDto?> UpdateCategoryAsync(string accessToken, int id, ItemCategoryDto request, CancellationToken ct = default)
        => SendAsync<ItemCategoryDto>(HttpMethod.Put, $"api/v1/inventory/categories/{id}", accessToken, request, ct);

    public async Task<bool> DeleteCategoryAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/inventory/categories/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<UnitOfMeasureDto>?> GetUnitsAsync(string accessToken, UnitOfMeasurePagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            parameters.Add($"code={Uri.EscapeDataString(request.Code.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            parameters.Add($"name={Uri.EscapeDataString(request.Name.Trim())}");
        }

        if (request.IsActive.HasValue)
        {
            parameters.Add($"isActive={(request.IsActive.Value ? "true" : "false")}");
        }

        var query = $"api/v1/inventory/units?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<UnitOfMeasureDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public async Task<IReadOnlyList<InventoryOptionDto>> GetUnitOptionsAsync(string accessToken, CancellationToken ct = default)
        => await SendAsync<IReadOnlyList<InventoryOptionDto>>(HttpMethod.Get, "api/v1/inventory/units/options", accessToken, null, ct) ?? [];

    public Task<UnitOfMeasureDto?> GetUnitByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<UnitOfMeasureDto>(HttpMethod.Get, $"api/v1/inventory/units/{id}", accessToken, null, ct);

    public Task<UnitOfMeasureDto?> CreateUnitAsync(string accessToken, UnitOfMeasureDto request, CancellationToken ct = default)
        => SendAsync<UnitOfMeasureDto>(HttpMethod.Post, "api/v1/inventory/units", accessToken, request, ct);

    public Task<UnitOfMeasureDto?> UpdateUnitAsync(string accessToken, int id, UnitOfMeasureDto request, CancellationToken ct = default)
        => SendAsync<UnitOfMeasureDto>(HttpMethod.Put, $"api/v1/inventory/units/{id}", accessToken, request, ct);

    public async Task<bool> DeleteUnitAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/inventory/units/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<BrandDto>?> GetBrandsAsync(string accessToken, BrandPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            parameters.Add($"name={Uri.EscapeDataString(request.Name.Trim())}");
        }

        if (request.IsActive.HasValue)
        {
            parameters.Add($"isActive={(request.IsActive.Value ? "true" : "false")}");
        }

        var query = $"api/v1/inventory/brands?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<BrandDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public async Task<IReadOnlyList<InventoryOptionDto>> GetBrandOptionsAsync(string accessToken, CancellationToken ct = default)
        => await SendAsync<IReadOnlyList<InventoryOptionDto>>(HttpMethod.Get, "api/v1/inventory/brands/options", accessToken, null, ct) ?? [];

    public Task<BrandDto?> GetBrandByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<BrandDto>(HttpMethod.Get, $"api/v1/inventory/brands/{id}", accessToken, null, ct);

    public Task<BrandDto?> CreateBrandAsync(string accessToken, BrandDto request, CancellationToken ct = default)
        => SendAsync<BrandDto>(HttpMethod.Post, "api/v1/inventory/brands", accessToken, request, ct);

    public Task<BrandDto?> UpdateBrandAsync(string accessToken, int id, BrandDto request, CancellationToken ct = default)
        => SendAsync<BrandDto>(HttpMethod.Put, $"api/v1/inventory/brands/{id}", accessToken, request, ct);

    public async Task<bool> DeleteBrandAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/inventory/brands/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<ItemDto>?> GetItemsAsync(string accessToken, ItemPagedRequest request, CancellationToken ct = default)
        => GetItemsCoreAsync(accessToken, request, "api/v1/inventory/items", ct);

    public Task<PagedResult<ItemDto>?> GetLowStockItemsAsync(string accessToken, ItemPagedRequest request, CancellationToken ct = default)
        => GetItemsCoreAsync(accessToken, request, "api/v1/inventory/items/low-stock", ct);

    public async Task<IReadOnlyList<InventoryOptionDto>> GetItemOptionsAsync(string accessToken, CancellationToken ct = default)
        => await SendAsync<IReadOnlyList<InventoryOptionDto>>(HttpMethod.Get, "api/v1/inventory/items/options", accessToken, null, ct) ?? [];

    public Task<ItemDto?> GetItemByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<ItemDto>(HttpMethod.Get, $"api/v1/inventory/items/{id}", accessToken, null, ct);

    public Task<ItemDto?> CreateItemAsync(string accessToken, ItemDto request, CancellationToken ct = default)
        => SendAsync<ItemDto>(HttpMethod.Post, "api/v1/inventory/items", accessToken, request, ct);

    public Task<ItemDto?> UpdateItemAsync(string accessToken, int id, ItemDto request, CancellationToken ct = default)
        => SendAsync<ItemDto>(HttpMethod.Put, $"api/v1/inventory/items/{id}", accessToken, request, ct);

    public async Task<bool> DeleteItemAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/inventory/items/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<ItemUnitConversionDto>?> GetItemConversionsAsync(string accessToken, ItemUnitConversionPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (request.ItemId.HasValue)
        {
            parameters.Add($"itemId={request.ItemId.Value}");
        }

        if (request.FromUomId.HasValue)
        {
            parameters.Add($"fromUomId={request.FromUomId.Value}");
        }

        if (request.ToUomId.HasValue)
        {
            parameters.Add($"toUomId={request.ToUomId.Value}");
        }

        if (request.IsActive.HasValue)
        {
            parameters.Add($"isActive={(request.IsActive.Value ? "true" : "false")}");
        }

        if (request.FactorFrom.HasValue)
        {
            parameters.Add($"factorFrom={request.FactorFrom.Value}");
        }

        if (request.FactorTo.HasValue)
        {
            parameters.Add($"factorTo={request.FactorTo.Value}");
        }

        var query = $"api/v1/inventory/item-conversions?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<ItemUnitConversionDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<ItemUnitConversionDto?> GetItemConversionByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<ItemUnitConversionDto>(HttpMethod.Get, $"api/v1/inventory/item-conversions/{id}", accessToken, null, ct);

    public Task<ItemUnitConversionDto?> CreateItemConversionAsync(string accessToken, ItemUnitConversionDto request, CancellationToken ct = default)
        => SendAsync<ItemUnitConversionDto>(HttpMethod.Post, "api/v1/inventory/item-conversions", accessToken, request, ct);

    public Task<ItemUnitConversionDto?> UpdateItemConversionAsync(string accessToken, int id, ItemUnitConversionDto request, CancellationToken ct = default)
        => SendAsync<ItemUnitConversionDto>(HttpMethod.Put, $"api/v1/inventory/item-conversions/{id}", accessToken, request, ct);

    public async Task<bool> DeleteItemConversionAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/inventory/item-conversions/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<WarehouseDto>?> GetWarehousesAsync(string accessToken, WarehousePagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            parameters.Add($"code={Uri.EscapeDataString(request.Code.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            parameters.Add($"name={Uri.EscapeDataString(request.Name.Trim())}");
        }

        if (request.ManagerId.HasValue)
        {
            parameters.Add($"managerId={request.ManagerId.Value}");
        }

        if (request.CostCenterId.HasValue)
        {
            parameters.Add($"costCenterId={request.CostCenterId.Value}");
        }

        if (request.IsTransit.HasValue)
        {
            parameters.Add($"isTransit={(request.IsTransit.Value ? "true" : "false")}");
        }

        if (request.IsActive.HasValue)
        {
            parameters.Add($"isActive={(request.IsActive.Value ? "true" : "false")}");
        }

        var query = $"api/v1/inventory/warehouses?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<WarehouseDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public async Task<IReadOnlyList<InventoryOptionDto>> GetWarehouseOptionsAsync(string accessToken, CancellationToken ct = default)
        => await SendAsync<IReadOnlyList<InventoryOptionDto>>(HttpMethod.Get, "api/v1/inventory/warehouses/options", accessToken, null, ct) ?? [];

    public Task<WarehouseDto?> GetWarehouseByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<WarehouseDto>(HttpMethod.Get, $"api/v1/inventory/warehouses/{id}", accessToken, null, ct);

    public Task<WarehouseDto?> CreateWarehouseAsync(string accessToken, WarehouseDto request, CancellationToken ct = default)
        => SendAsync<WarehouseDto>(HttpMethod.Post, "api/v1/inventory/warehouses", accessToken, request, ct);

    public Task<WarehouseDto?> UpdateWarehouseAsync(string accessToken, int id, WarehouseDto request, CancellationToken ct = default)
        => SendAsync<WarehouseDto>(HttpMethod.Put, $"api/v1/inventory/warehouses/{id}", accessToken, request, ct);

    public async Task<bool> DeleteWarehouseAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/inventory/warehouses/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<WarehouseLocationDto>?> GetWarehouseLocationsAsync(string accessToken, int warehouseId, WarehouseLocationPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            parameters.Add($"code={Uri.EscapeDataString(request.Code.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            parameters.Add($"name={Uri.EscapeDataString(request.Name.Trim())}");
        }

        if (request.IsDefault.HasValue)
        {
            parameters.Add($"isDefault={(request.IsDefault.Value ? "true" : "false")}");
        }

        if (request.IsActive.HasValue)
        {
            parameters.Add($"isActive={(request.IsActive.Value ? "true" : "false")}");
        }

        var query = $"api/v1/inventory/warehouses/{warehouseId}/locations?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<WarehouseLocationDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public async Task<IReadOnlyList<InventoryOptionDto>> GetWarehouseLocationOptionsAsync(string accessToken, int warehouseId, CancellationToken ct = default)
        => await SendAsync<IReadOnlyList<InventoryOptionDto>>(HttpMethod.Get, $"api/v1/inventory/warehouses/{warehouseId}/locations/options", accessToken, null, ct) ?? [];

    public Task<WarehouseLocationDto?> GetWarehouseLocationByIdAsync(string accessToken, int warehouseId, int id, CancellationToken ct = default)
        => SendAsync<WarehouseLocationDto>(HttpMethod.Get, $"api/v1/inventory/warehouses/{warehouseId}/locations/{id}", accessToken, null, ct);

    public Task<WarehouseLocationDto?> CreateWarehouseLocationAsync(string accessToken, int warehouseId, WarehouseLocationDto request, CancellationToken ct = default)
        => SendAsync<WarehouseLocationDto>(HttpMethod.Post, $"api/v1/inventory/warehouses/{warehouseId}/locations", accessToken, request, ct);

    public Task<WarehouseLocationDto?> UpdateWarehouseLocationAsync(string accessToken, int warehouseId, int id, WarehouseLocationDto request, CancellationToken ct = default)
        => SendAsync<WarehouseLocationDto>(HttpMethod.Put, $"api/v1/inventory/warehouses/{warehouseId}/locations/{id}", accessToken, request, ct);

    public async Task<bool> DeleteWarehouseLocationAsync(string accessToken, int warehouseId, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/inventory/warehouses/{warehouseId}/locations/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<StockBalanceDto>?> GetWarehouseStockAsync(string accessToken, int warehouseId, StockBalancePagedRequest request, CancellationToken ct = default)
        => GetStockCoreAsync(accessToken, request, $"api/v1/inventory/warehouses/{warehouseId}/stock", ct);

    public Task<PagedResult<StockBalanceDto>?> GetStockBalancesAsync(string accessToken, StockBalancePagedRequest request, CancellationToken ct = default)
        => GetStockCoreAsync(accessToken, request, "api/v1/inventory/stock-balances", ct);
    public Task<PagedResult<GoodsReceiptDto>?> GetGoodsReceiptsAsync(string accessToken, GoodsReceiptPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.ReceiptNo))
        {
            parameters.Add($"receiptNo={Uri.EscapeDataString(request.ReceiptNo.Trim())}");
        }

        AddDateOnlyParameter(parameters, "dateFrom", request.DateFrom);
        AddDateOnlyParameter(parameters, "dateTo", request.DateTo);

        if (request.WarehouseId.HasValue)
        {
            parameters.Add($"warehouseId={request.WarehouseId.Value}");
        }

        if (request.ReceiptType.HasValue)
        {
            parameters.Add($"receiptType={(int)request.ReceiptType.Value}");
        }

        if (request.Status.HasValue)
        {
            parameters.Add($"status={(int)request.Status.Value}");
        }

        if (!string.IsNullOrWhiteSpace(request.SupplierName))
        {
            parameters.Add($"supplierName={Uri.EscapeDataString(request.SupplierName.Trim())}");
        }

        return SendAsync<PagedResult<GoodsReceiptDto>>(HttpMethod.Get, BuildQuery("api/v1/inventory/goods-receipts", parameters), accessToken, null, ct);
    }

    public Task<GoodsReceiptDto?> GetGoodsReceiptByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<GoodsReceiptDto>(HttpMethod.Get, $"api/v1/inventory/goods-receipts/{id}", accessToken, null, ct);

    public Task<GoodsReceiptDto?> CreateGoodsReceiptAsync(string accessToken, GoodsReceiptDto request, CancellationToken ct = default)
        => SendAsync<GoodsReceiptDto>(HttpMethod.Post, "api/v1/inventory/goods-receipts", accessToken, request, ct);

    public Task<GoodsReceiptDto?> UpdateGoodsReceiptAsync(string accessToken, int id, GoodsReceiptDto request, CancellationToken ct = default)
        => SendAsync<GoodsReceiptDto>(HttpMethod.Put, $"api/v1/inventory/goods-receipts/{id}", accessToken, request, ct);

    public async Task<bool> DeleteGoodsReceiptAsync(string accessToken, int id, CancellationToken ct = default)
        => await SendActionAsync(HttpMethod.Delete, $"api/v1/inventory/goods-receipts/{id}", accessToken, null, ct);

    public async Task<bool> ConfirmGoodsReceiptAsync(string accessToken, int id, CancellationToken ct = default)
        => await SendActionAsync(HttpMethod.Put, $"api/v1/inventory/goods-receipts/{id}/confirm", accessToken, null, ct);

    public async Task<bool> CancelGoodsReceiptAsync(string accessToken, int id, CancellationToken ct = default)
        => await SendActionAsync(HttpMethod.Put, $"api/v1/inventory/goods-receipts/{id}/cancel", accessToken, null, ct);

    public Task<PagedResult<GoodsIssueDto>?> GetGoodsIssuesAsync(string accessToken, GoodsIssuePagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.IssueNo))
        {
            parameters.Add($"issueNo={Uri.EscapeDataString(request.IssueNo.Trim())}");
        }

        AddDateOnlyParameter(parameters, "dateFrom", request.DateFrom);
        AddDateOnlyParameter(parameters, "dateTo", request.DateTo);

        if (request.WarehouseId.HasValue)
        {
            parameters.Add($"warehouseId={request.WarehouseId.Value}");
        }

        if (request.DepartmentId.HasValue)
        {
            parameters.Add($"departmentId={request.DepartmentId.Value}");
        }

        if (request.IssueType.HasValue)
        {
            parameters.Add($"issueType={(int)request.IssueType.Value}");
        }

        if (request.Status.HasValue)
        {
            parameters.Add($"status={(int)request.Status.Value}");
        }

        return SendAsync<PagedResult<GoodsIssueDto>>(HttpMethod.Get, BuildQuery("api/v1/inventory/goods-issues", parameters), accessToken, null, ct);
    }

    public Task<GoodsIssueDto?> GetGoodsIssueByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<GoodsIssueDto>(HttpMethod.Get, $"api/v1/inventory/goods-issues/{id}", accessToken, null, ct);

    public Task<GoodsIssueDto?> CreateGoodsIssueAsync(string accessToken, GoodsIssueDto request, CancellationToken ct = default)
        => SendAsync<GoodsIssueDto>(HttpMethod.Post, "api/v1/inventory/goods-issues", accessToken, request, ct);

    public Task<GoodsIssueDto?> UpdateGoodsIssueAsync(string accessToken, int id, GoodsIssueDto request, CancellationToken ct = default)
        => SendAsync<GoodsIssueDto>(HttpMethod.Put, $"api/v1/inventory/goods-issues/{id}", accessToken, request, ct);

    public async Task<bool> DeleteGoodsIssueAsync(string accessToken, int id, CancellationToken ct = default)
        => await SendActionAsync(HttpMethod.Delete, $"api/v1/inventory/goods-issues/{id}", accessToken, null, ct);

    public async Task<bool> ConfirmGoodsIssueAsync(string accessToken, int id, CancellationToken ct = default)
        => await SendActionAsync(HttpMethod.Put, $"api/v1/inventory/goods-issues/{id}/confirm", accessToken, null, ct);

    public async Task<bool> CancelGoodsIssueAsync(string accessToken, int id, CancellationToken ct = default)
        => await SendActionAsync(HttpMethod.Put, $"api/v1/inventory/goods-issues/{id}/cancel", accessToken, null, ct);

    public Task<PagedResult<StockTransferDto>?> GetTransfersAsync(string accessToken, StockTransferPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.TransferNo))
        {
            parameters.Add($"transferNo={Uri.EscapeDataString(request.TransferNo.Trim())}");
        }

        AddDateOnlyParameter(parameters, "dateFrom", request.DateFrom);
        AddDateOnlyParameter(parameters, "dateTo", request.DateTo);

        if (request.FromWarehouseId.HasValue)
        {
            parameters.Add($"fromWarehouseId={request.FromWarehouseId.Value}");
        }

        if (request.ToWarehouseId.HasValue)
        {
            parameters.Add($"toWarehouseId={request.ToWarehouseId.Value}");
        }

        if (request.Status.HasValue)
        {
            parameters.Add($"status={(int)request.Status.Value}");
        }

        return SendAsync<PagedResult<StockTransferDto>>(HttpMethod.Get, BuildQuery("api/v1/inventory/transfers", parameters), accessToken, null, ct);
    }

    public Task<StockTransferDto?> GetTransferByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<StockTransferDto>(HttpMethod.Get, $"api/v1/inventory/transfers/{id}", accessToken, null, ct);

    public Task<StockTransferDto?> CreateTransferAsync(string accessToken, StockTransferDto request, CancellationToken ct = default)
        => SendAsync<StockTransferDto>(HttpMethod.Post, "api/v1/inventory/transfers", accessToken, request, ct);

    public Task<StockTransferDto?> UpdateTransferAsync(string accessToken, int id, StockTransferDto request, CancellationToken ct = default)
        => SendAsync<StockTransferDto>(HttpMethod.Put, $"api/v1/inventory/transfers/{id}", accessToken, request, ct);

    public async Task<bool> DeleteTransferAsync(string accessToken, int id, CancellationToken ct = default)
        => await SendActionAsync(HttpMethod.Delete, $"api/v1/inventory/transfers/{id}", accessToken, null, ct);

    public async Task<bool> ConfirmTransferAsync(string accessToken, int id, CancellationToken ct = default)
        => await SendActionAsync(HttpMethod.Put, $"api/v1/inventory/transfers/{id}/confirm", accessToken, null, ct);

    public async Task<bool> CancelTransferAsync(string accessToken, int id, CancellationToken ct = default)
        => await SendActionAsync(HttpMethod.Put, $"api/v1/inventory/transfers/{id}/cancel", accessToken, null, ct);

    public Task<PagedResult<StockAdjustmentDto>?> GetAdjustmentsAsync(string accessToken, StockAdjustmentPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.AdjustmentNo))
        {
            parameters.Add($"adjustmentNo={Uri.EscapeDataString(request.AdjustmentNo.Trim())}");
        }

        AddDateOnlyParameter(parameters, "dateFrom", request.DateFrom);
        AddDateOnlyParameter(parameters, "dateTo", request.DateTo);

        if (request.WarehouseId.HasValue)
        {
            parameters.Add($"warehouseId={request.WarehouseId.Value}");
        }

        if (request.Reason.HasValue)
        {
            parameters.Add($"reason={(int)request.Reason.Value}");
        }

        if (request.Status.HasValue)
        {
            parameters.Add($"status={(int)request.Status.Value}");
        }

        return SendAsync<PagedResult<StockAdjustmentDto>>(HttpMethod.Get, BuildQuery("api/v1/inventory/adjustments", parameters), accessToken, null, ct);
    }

    public Task<StockAdjustmentDto?> GetAdjustmentByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<StockAdjustmentDto>(HttpMethod.Get, $"api/v1/inventory/adjustments/{id}", accessToken, null, ct);

    public Task<StockAdjustmentDto?> CreateAdjustmentAsync(string accessToken, StockAdjustmentDto request, CancellationToken ct = default)
        => SendAsync<StockAdjustmentDto>(HttpMethod.Post, "api/v1/inventory/adjustments", accessToken, request, ct);

    public Task<StockAdjustmentDto?> UpdateAdjustmentAsync(string accessToken, int id, StockAdjustmentDto request, CancellationToken ct = default)
        => SendAsync<StockAdjustmentDto>(HttpMethod.Put, $"api/v1/inventory/adjustments/{id}", accessToken, request, ct);

    public async Task<bool> DeleteAdjustmentAsync(string accessToken, int id, CancellationToken ct = default)
        => await SendActionAsync(HttpMethod.Delete, $"api/v1/inventory/adjustments/{id}", accessToken, null, ct);

    public async Task<bool> ApproveAdjustmentAsync(string accessToken, int id, CancellationToken ct = default)
        => await SendActionAsync(HttpMethod.Put, $"api/v1/inventory/adjustments/{id}/approve", accessToken, null, ct);

    public async Task<bool> ConfirmAdjustmentAsync(string accessToken, int id, CancellationToken ct = default)
        => await SendActionAsync(HttpMethod.Put, $"api/v1/inventory/adjustments/{id}/confirm", accessToken, null, ct);

    public async Task<bool> CancelAdjustmentAsync(string accessToken, int id, CancellationToken ct = default)
        => await SendActionAsync(HttpMethod.Put, $"api/v1/inventory/adjustments/{id}/cancel", accessToken, null, ct);

    public Task<PagedResult<StockOpnameDto>?> GetOpnamesAsync(string accessToken, StockOpnamePagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.OpnameNo))
        {
            parameters.Add($"opnameNo={Uri.EscapeDataString(request.OpnameNo.Trim())}");
        }

        AddDateOnlyParameter(parameters, "dateFrom", request.DateFrom);
        AddDateOnlyParameter(parameters, "dateTo", request.DateTo);

        if (request.WarehouseId.HasValue)
        {
            parameters.Add($"warehouseId={request.WarehouseId.Value}");
        }

        if (request.Status.HasValue)
        {
            parameters.Add($"status={(int)request.Status.Value}");
        }

        return SendAsync<PagedResult<StockOpnameDto>>(HttpMethod.Get, BuildQuery("api/v1/inventory/opnames", parameters), accessToken, null, ct);
    }

    public Task<StockOpnameDto?> GetOpnameByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<StockOpnameDto>(HttpMethod.Get, $"api/v1/inventory/opnames/{id}", accessToken, null, ct);

    public Task<StockOpnameDto?> CreateOpnameAsync(string accessToken, StockOpnameDto request, CancellationToken ct = default)
        => SendAsync<StockOpnameDto>(HttpMethod.Post, "api/v1/inventory/opnames", accessToken, request, ct);

    public async Task<bool> UpdateOpnameAsync(string accessToken, int id, StockOpnameDto request, CancellationToken ct = default)
        => await SendActionAsync(HttpMethod.Put, $"api/v1/inventory/opnames/{id}", accessToken, request, ct);

    public async Task<bool> StartOpnameAsync(string accessToken, int id, CancellationToken ct = default)
        => await SendActionAsync(HttpMethod.Put, $"api/v1/inventory/opnames/{id}/start", accessToken, null, ct);

    public async Task<IReadOnlyList<StockOpnameLineDto>> GetOpnameLinesAsync(string accessToken, int id, CancellationToken ct = default)
        => await SendAsync<IReadOnlyList<StockOpnameLineDto>>(HttpMethod.Get, $"api/v1/inventory/opnames/{id}/lines", accessToken, null, ct) ?? [];

    public async Task<bool> UpdateOpnameLinesAsync(string accessToken, int id, IReadOnlyList<StockOpnameLineDto> lines, CancellationToken ct = default)
        => await SendActionAsync(HttpMethod.Put, $"api/v1/inventory/opnames/{id}/lines", accessToken, lines, ct);

    public async Task<bool> UpdateOpnameLineAsync(string accessToken, int id, int lineId, StockOpnameLineDto line, CancellationToken ct = default)
        => await SendActionAsync(HttpMethod.Put, $"api/v1/inventory/opnames/{id}/lines/{lineId}", accessToken, line, ct);

    public async Task<bool> CompleteOpnameAsync(string accessToken, int id, CancellationToken ct = default)
        => await SendActionAsync(HttpMethod.Put, $"api/v1/inventory/opnames/{id}/complete", accessToken, null, ct);

    public async Task<bool> ApproveOpnameAsync(string accessToken, int id, CancellationToken ct = default)
        => await SendActionAsync(HttpMethod.Put, $"api/v1/inventory/opnames/{id}/approve", accessToken, null, ct);

    public async Task<bool> CancelOpnameAsync(string accessToken, int id, CancellationToken ct = default)
        => await SendActionAsync(HttpMethod.Put, $"api/v1/inventory/opnames/{id}/cancel", accessToken, null, ct);

    public Task<PagedResult<StockBalanceDto>?> GetStockBalanceReportAsync(string accessToken, StockBalancePagedRequest request, CancellationToken ct = default)
        => GetStockCoreAsync(accessToken, request, "api/v1/inventory/stock/balance", ct);

    public async Task<decimal?> GetStockAvailableAsync(string accessToken, int itemId, int warehouseId, int? locationId, CancellationToken ct = default)
    {
        var parameters = new List<string>
        {
            $"itemId={itemId}",
            $"warehouseId={warehouseId}"
        };

        if (locationId.HasValue)
        {
            parameters.Add($"locationId={locationId.Value}");
        }

        var response = await SendAsync<StockAvailableResponse>(
            HttpMethod.Get,
            BuildQuery("api/v1/inventory/stock/available", parameters),
            accessToken,
            null,
            ct);

        return response?.QtyAvailable;
    }

    public Task<PagedResult<InventoryMovementHistoryDto>?> GetStockMovementsAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default)
        => GetInventoryReportPagedAsync<InventoryMovementHistoryDto>(accessToken, "api/v1/inventory/stock/movements", request, ct);

    public Task<PagedResult<InventoryMovementHistoryDto>?> GetStockCardRawAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default)
        => GetInventoryReportPagedAsync<InventoryMovementHistoryDto>(accessToken, "api/v1/inventory/stock/card", request, ct);

    public Task<PagedResult<InventoryValuationDto>?> GetStockValuationRawAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default)
        => GetInventoryReportPagedAsync<InventoryValuationDto>(accessToken, "api/v1/inventory/stock/valuation", request, ct);

    public Task<PagedResult<InventoryStockSummaryDto>?> GetStockSummaryReportAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default)
        => GetInventoryReportPagedAsync<InventoryStockSummaryDto>(accessToken, "api/v1/inventory/reports/stock-summary", request, ct);

    public async Task<IReadOnlyList<InventoryStockByWarehouseDto>> GetStockByWarehouseReportAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default)
        => await GetInventoryReportListAsync<InventoryStockByWarehouseDto>(accessToken, "api/v1/inventory/reports/stock-by-warehouse", request, ct);

    public async Task<IReadOnlyList<InventoryStockByCategoryDto>> GetStockByCategoryReportAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default)
        => await GetInventoryReportListAsync<InventoryStockByCategoryDto>(accessToken, "api/v1/inventory/reports/stock-by-category", request, ct);

    public Task<PagedResult<InventoryStockCardDto>?> GetStockCardReportAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default)
        => GetInventoryReportPagedAsync<InventoryStockCardDto>(accessToken, "api/v1/inventory/reports/stock-card", request, ct);

    public Task<PagedResult<InventoryLowStockDto>?> GetLowStockReportAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default)
        => GetInventoryReportPagedAsync<InventoryLowStockDto>(accessToken, "api/v1/inventory/reports/low-stock", request, ct);

    public Task<PagedResult<InventoryValuationDto>?> GetInventoryValuationReportAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default)
        => GetInventoryReportPagedAsync<InventoryValuationDto>(accessToken, "api/v1/inventory/reports/inventory-valuation", request, ct);

    public Task<PagedResult<InventoryAgingDto>?> GetInventoryAgingReportAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default)
        => GetInventoryReportPagedAsync<InventoryAgingDto>(accessToken, "api/v1/inventory/reports/inventory-aging", request, ct);

    public Task<PagedResult<InventoryMovementHistoryDto>?> GetMovementHistoryReportAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default)
        => GetInventoryReportPagedAsync<InventoryMovementHistoryDto>(accessToken, "api/v1/inventory/reports/movement-history", request, ct);

    public Task<PagedResult<InventoryReceiptSummaryDto>?> GetReceiptSummaryReportAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default)
        => GetInventoryReportPagedAsync<InventoryReceiptSummaryDto>(accessToken, "api/v1/inventory/reports/receipt-summary", request, ct);

    public Task<PagedResult<InventoryIssueSummaryDto>?> GetIssueSummaryReportAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default)
        => GetInventoryReportPagedAsync<InventoryIssueSummaryDto>(accessToken, "api/v1/inventory/reports/issue-summary", request, ct);

    public Task<PagedResult<InventoryTransferSummaryDto>?> GetTransferSummaryReportAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default)
        => GetInventoryReportPagedAsync<InventoryTransferSummaryDto>(accessToken, "api/v1/inventory/reports/transfer-summary", request, ct);

    public Task<PagedResult<InventoryAdjustmentSummaryDto>?> GetAdjustmentSummaryReportAsync(string accessToken, InventoryReportRequest request, CancellationToken ct = default)
        => GetInventoryReportPagedAsync<InventoryAdjustmentSummaryDto>(accessToken, "api/v1/inventory/reports/adjustment-summary", request, ct);

    private async Task<bool> SendActionAsync(HttpMethod method, string uri, string accessToken, object? body, CancellationToken ct)
    {
        var response = await SendRawAsync(method, uri, accessToken, body, ct);
        return response?.IsSuccessStatusCode == true;
    }

    private Task<PagedResult<T>?> GetInventoryReportPagedAsync<T>(string accessToken, string basePath, InventoryReportRequest request, CancellationToken ct)
    {
        var parameters = BuildInventoryReportParameters(request);
        return SendAsync<PagedResult<T>>(HttpMethod.Get, BuildQuery(basePath, parameters), accessToken, null, ct);
    }

    private async Task<IReadOnlyList<T>> GetInventoryReportListAsync<T>(string accessToken, string basePath, InventoryReportRequest request, CancellationToken ct)
    {
        var parameters = BuildInventoryReportParameters(request, includePaging: false);
        return await SendAsync<IReadOnlyList<T>>(HttpMethod.Get, BuildQuery(basePath, parameters), accessToken, null, ct) ?? [];
    }

    private static List<string> BuildInventoryReportParameters(InventoryReportRequest request, bool includePaging = true)
    {
        var parameters = new List<string>();

        if (includePaging)
        {
            AddPagedParameters(parameters, request);
        }

        if (request.WarehouseId.HasValue)
        {
            parameters.Add($"warehouseId={request.WarehouseId.Value}");
        }

        if (request.CategoryId.HasValue)
        {
            parameters.Add($"categoryId={request.CategoryId.Value}");
        }

        if (request.ItemId.HasValue)
        {
            parameters.Add($"itemId={request.ItemId.Value}");
        }

        if (request.DepartmentId.HasValue)
        {
            parameters.Add($"departmentId={request.DepartmentId.Value}");
        }

        AddDateOnlyParameter(parameters, "dateFrom", request.DateFrom);
        AddDateOnlyParameter(parameters, "dateTo", request.DateTo);

        if (request.ThresholdDays.HasValue)
        {
            parameters.Add($"thresholdDays={request.ThresholdDays.Value}");
        }

        if (request.MovementType.HasValue)
        {
            parameters.Add($"movementType={(int)request.MovementType.Value}");
        }

        return parameters;
    }

    private static void AddDateOnlyParameter(List<string> parameters, string name, DateOnly? value)
    {
        if (!value.HasValue)
        {
            return;
        }

        parameters.Add($"{name}={value.Value:yyyy-MM-dd}");
    }

    private static string BuildQuery(string basePath, List<string> parameters)
        => parameters.Count == 0 ? basePath : $"{basePath}?{string.Join("&", parameters)}";

    private Task<PagedResult<ItemDto>?> GetItemsCoreAsync(string accessToken, ItemPagedRequest request, string basePath, CancellationToken ct)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.ItemCode))
        {
            parameters.Add($"itemCode={Uri.EscapeDataString(request.ItemCode.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(request.Sku))
        {
            parameters.Add($"sku={Uri.EscapeDataString(request.Sku.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            parameters.Add($"name={Uri.EscapeDataString(request.Name.Trim())}");
        }

        if (request.CategoryId.HasValue)
        {
            parameters.Add($"categoryId={request.CategoryId.Value}");
        }

        if (request.BrandId.HasValue)
        {
            parameters.Add($"brandId={request.BrandId.Value}");
        }

        if (request.Type.HasValue)
        {
            parameters.Add($"type={(int)request.Type.Value}");
        }

        if (request.Status.HasValue)
        {
            parameters.Add($"status={(int)request.Status.Value}");
        }

        if (request.IsActive.HasValue)
        {
            parameters.Add($"isActive={(request.IsActive.Value ? "true" : "false")}");
        }

        if (request.MinStockFrom.HasValue)
        {
            parameters.Add($"minStockFrom={request.MinStockFrom.Value}");
        }

        if (request.MinStockTo.HasValue)
        {
            parameters.Add($"minStockTo={request.MinStockTo.Value}");
        }

        if (request.ReorderPointFrom.HasValue)
        {
            parameters.Add($"reorderPointFrom={request.ReorderPointFrom.Value}");
        }

        if (request.ReorderPointTo.HasValue)
        {
            parameters.Add($"reorderPointTo={request.ReorderPointTo.Value}");
        }

        var query = $"{basePath}?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<ItemDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    private Task<PagedResult<StockBalanceDto>?> GetStockCoreAsync(string accessToken, StockBalancePagedRequest request, string basePath, CancellationToken ct)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (request.WarehouseId.HasValue)
        {
            parameters.Add($"warehouseId={request.WarehouseId.Value}");
        }

        if (request.ItemId.HasValue)
        {
            parameters.Add($"itemId={request.ItemId.Value}");
        }

        if (request.LocationId.HasValue)
        {
            parameters.Add($"locationId={request.LocationId.Value}");
        }

        var query = $"{basePath}?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<StockBalanceDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    

    private sealed class StockAvailableResponse
    {
        public decimal QtyAvailable { get; set; }
    }

    private static void AddPagedParameters(List<string> parameters, PagedRequest request)
    {
        parameters.Add($"page={request.Page}");
        parameters.Add($"pageSize={request.PageSize}");
        parameters.Add($"search={Uri.EscapeDataString(request.Search ?? string.Empty)}");
        parameters.Add($"sortBy={Uri.EscapeDataString(request.SortBy ?? string.Empty)}");
        parameters.Add($"sortDirection={Uri.EscapeDataString(request.SortDirection ?? string.Empty)}");
    }
}

