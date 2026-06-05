using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.FixedAssets;

namespace ERP.Web.Services;

public sealed class FixedAssetsApiClient(HttpClient httpClient, ILogger<FixedAssetsApiClient> logger) : ApiClientBase(httpClient, logger, "Fixed Assets"), IFixedAssetsApiClient
{
    public Task<FixedAssetDashboardDto?> GetDashboardAsync(string accessToken, CancellationToken ct = default)
        => SendAsync<FixedAssetDashboardDto>(HttpMethod.Get, "api/v1/fixed-assets/dashboard", accessToken, null, ct);

    public Task<PagedResult<FixedAssetCategoryDto>?> GetAssetCategoriesAsync(string accessToken, FixedAssetCategoryPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);
        AddTextParameter(parameters, "code", request.Code);
        AddTextParameter(parameters, "name", request.Name);
        AddEnumParameter(parameters, "depreciationMethod", request.DepreciationMethod);
        AddBooleanParameter(parameters, "isActive", request.IsActive);

        return SendAsync<PagedResult<FixedAssetCategoryDto>>(HttpMethod.Get, $"api/v1/fixed-assets/asset-categories?{string.Join("&", parameters)}", accessToken, null, ct);
    }

    public async Task<IReadOnlyList<FixedAssetOptionDto>> GetAssetCategoryOptionsAsync(string accessToken, CancellationToken ct = default)
        => await SendAsync<IReadOnlyList<FixedAssetOptionDto>>(HttpMethod.Get, "api/v1/fixed-assets/asset-categories/options", accessToken, null, ct) ?? [];

    public Task<FixedAssetCategoryDto?> GetAssetCategoryByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<FixedAssetCategoryDto>(HttpMethod.Get, $"api/v1/fixed-assets/asset-categories/{id}", accessToken, null, ct);

    public Task<FixedAssetCategoryDto?> CreateAssetCategoryAsync(string accessToken, FixedAssetCategoryDto request, CancellationToken ct = default)
        => SendAsync<FixedAssetCategoryDto>(HttpMethod.Post, "api/v1/fixed-assets/asset-categories", accessToken, request, ct);

    public Task<FixedAssetCategoryDto?> UpdateAssetCategoryAsync(string accessToken, int id, FixedAssetCategoryDto request, CancellationToken ct = default)
        => SendAsync<FixedAssetCategoryDto>(HttpMethod.Put, $"api/v1/fixed-assets/asset-categories/{id}", accessToken, request, ct);

    public async Task<bool> DeleteAssetCategoryAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/fixed-assets/asset-categories/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<FixedAssetLocationDto>?> GetLocationsAsync(string accessToken, FixedAssetLocationPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);
        AddTextParameter(parameters, "code", request.Code);
        AddTextParameter(parameters, "name", request.Name);
        AddIntParameter(parameters, "departmentId", request.DepartmentId);
        AddBooleanParameter(parameters, "isActive", request.IsActive);

        return SendAsync<PagedResult<FixedAssetLocationDto>>(HttpMethod.Get, $"api/v1/fixed-assets/locations?{string.Join("&", parameters)}", accessToken, null, ct);
    }

    public async Task<IReadOnlyList<FixedAssetOptionDto>> GetLocationOptionsAsync(string accessToken, CancellationToken ct = default)
        => await SendAsync<IReadOnlyList<FixedAssetOptionDto>>(HttpMethod.Get, "api/v1/fixed-assets/locations/options", accessToken, null, ct) ?? [];

    public Task<FixedAssetLocationDto?> GetLocationByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<FixedAssetLocationDto>(HttpMethod.Get, $"api/v1/fixed-assets/locations/{id}", accessToken, null, ct);

    public Task<FixedAssetLocationDto?> CreateLocationAsync(string accessToken, FixedAssetLocationDto request, CancellationToken ct = default)
        => SendAsync<FixedAssetLocationDto>(HttpMethod.Post, "api/v1/fixed-assets/locations", accessToken, request, ct);

    public Task<FixedAssetLocationDto?> UpdateLocationAsync(string accessToken, int id, FixedAssetLocationDto request, CancellationToken ct = default)
        => SendAsync<FixedAssetLocationDto>(HttpMethod.Put, $"api/v1/fixed-assets/locations/{id}", accessToken, request, ct);

    public async Task<bool> DeleteLocationAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/fixed-assets/locations/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<FixedAssetDepreciationConfigDto>?> GetDepreciationConfigsAsync(string accessToken, FixedAssetDepreciationConfigPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);
        AddShortParameter(parameters, "fiscalYear", request.FiscalYear);
        AddBooleanParameter(parameters, "isAutoPostJournal", request.IsAutoPostJournal);
        AddBooleanParameter(parameters, "isActive", request.IsActive);

        return SendAsync<PagedResult<FixedAssetDepreciationConfigDto>>(HttpMethod.Get, $"api/v1/fixed-assets/depreciation-configs?{string.Join("&", parameters)}", accessToken, null, ct);
    }

    public Task<FixedAssetDepreciationConfigDto?> GetDepreciationConfigByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<FixedAssetDepreciationConfigDto>(HttpMethod.Get, $"api/v1/fixed-assets/depreciation-configs/{id}", accessToken, null, ct);

    public Task<FixedAssetDepreciationConfigDto?> CreateDepreciationConfigAsync(string accessToken, FixedAssetDepreciationConfigDto request, CancellationToken ct = default)
        => SendAsync<FixedAssetDepreciationConfigDto>(HttpMethod.Post, "api/v1/fixed-assets/depreciation-configs", accessToken, request, ct);

    public Task<FixedAssetDepreciationConfigDto?> UpdateDepreciationConfigAsync(string accessToken, int id, FixedAssetDepreciationConfigDto request, CancellationToken ct = default)
        => SendAsync<FixedAssetDepreciationConfigDto>(HttpMethod.Put, $"api/v1/fixed-assets/depreciation-configs/{id}", accessToken, request, ct);

    public async Task<bool> DeleteDepreciationConfigAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/fixed-assets/depreciation-configs/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<FixedAssetDto>?> GetAssetsAsync(string accessToken, FixedAssetPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);
        AddTextParameter(parameters, "assetCode", request.AssetCode);
        AddTextParameter(parameters, "name", request.Name);
        AddIntParameter(parameters, "categoryId", request.CategoryId);
        AddIntParameter(parameters, "locationId", request.LocationId);
        AddIntParameter(parameters, "departmentId", request.DepartmentId);
        AddEnumParameter(parameters, "status", request.Status);
        AddDecimalParameter(parameters, "bookValueFrom", request.BookValueFrom);
        AddDecimalParameter(parameters, "bookValueTo", request.BookValueTo);
        AddDateParameter(parameters, "acquisitionDateFrom", request.AcquisitionDateFrom);
        AddDateParameter(parameters, "acquisitionDateTo", request.AcquisitionDateTo);
        AddBooleanParameter(parameters, "isActive", request.IsActive);

        return SendAsync<PagedResult<FixedAssetDto>>(HttpMethod.Get, $"api/v1/fixed-assets/assets?{string.Join("&", parameters)}", accessToken, null, ct);
    }

    public async Task<IReadOnlyList<FixedAssetOptionDto>> GetAssetOptionsAsync(string accessToken, CancellationToken ct = default)
        => await SendAsync<IReadOnlyList<FixedAssetOptionDto>>(HttpMethod.Get, "api/v1/fixed-assets/assets/options", accessToken, null, ct) ?? [];

    public Task<FixedAssetDetailDto?> GetAssetByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<FixedAssetDetailDto>(HttpMethod.Get, $"api/v1/fixed-assets/assets/{id}", accessToken, null, ct);

    public Task<FixedAssetDto?> CreateAssetAsync(string accessToken, FixedAssetDto request, CancellationToken ct = default)
        => SendAsync<FixedAssetDto>(HttpMethod.Post, "api/v1/fixed-assets/assets", accessToken, request, ct);

    public Task<FixedAssetDto?> UpdateAssetAsync(string accessToken, int id, FixedAssetDto request, CancellationToken ct = default)
        => SendAsync<FixedAssetDto>(HttpMethod.Put, $"api/v1/fixed-assets/assets/{id}", accessToken, request, ct);

    public async Task<bool> DeleteAssetAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/fixed-assets/assets/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<FixedAssetDepreciationRunDto>?> GetDepreciationRunsAsync(string accessToken, FixedAssetDepreciationRunPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);
        AddShortParameter(parameters, "periodYear", request.PeriodYear);
        AddByteParameter(parameters, "periodMonth", request.PeriodMonth);
        AddEnumParameter(parameters, "status", request.Status);

        return SendAsync<PagedResult<FixedAssetDepreciationRunDto>>(HttpMethod.Get, $"api/v1/fixed-assets/depreciation-runs?{string.Join("&", parameters)}", accessToken, null, ct);
    }

    public Task<FixedAssetDepreciationRunDto?> GetDepreciationRunByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<FixedAssetDepreciationRunDto>(HttpMethod.Get, $"api/v1/fixed-assets/depreciation-runs/{id}", accessToken, null, ct);

    public Task<ApiCallResult<FixedAssetDepreciationRunDto>> RunDepreciationAsync(string accessToken, RunDepreciationRequest request, CancellationToken ct = default)
        => SendWithResultAsync<FixedAssetDepreciationRunDto>(HttpMethod.Post, "api/v1/fixed-assets/depreciation-runs/run", accessToken, request, ct);

    public async Task<bool> ApproveDepreciationRunAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Put, $"api/v1/fixed-assets/depreciation-runs/{id}/approve", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<FixedAssetTransferDto>?> GetTransfersAsync(string accessToken, FixedAssetTransferPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);
        AddIntParameter(parameters, "assetId", request.AssetId);
        AddIntParameter(parameters, "fromLocationId", request.FromLocationId);
        AddIntParameter(parameters, "toLocationId", request.ToLocationId);
        AddEnumParameter(parameters, "status", request.Status);
        AddDateParameter(parameters, "transferDateFrom", request.TransferDateFrom);
        AddDateParameter(parameters, "transferDateTo", request.TransferDateTo);

        return SendAsync<PagedResult<FixedAssetTransferDto>>(HttpMethod.Get, $"api/v1/fixed-assets/transfers?{string.Join("&", parameters)}", accessToken, null, ct);
    }

    public Task<FixedAssetTransferDto?> GetTransferByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<FixedAssetTransferDto>(HttpMethod.Get, $"api/v1/fixed-assets/transfers/{id}", accessToken, null, ct);

    public Task<FixedAssetTransferDto?> CreateTransferAsync(string accessToken, FixedAssetTransferDto request, CancellationToken ct = default)
        => SendAsync<FixedAssetTransferDto>(HttpMethod.Post, "api/v1/fixed-assets/transfers", accessToken, request, ct);

    public Task<FixedAssetTransferDto?> UpdateTransferAsync(string accessToken, int id, FixedAssetTransferDto request, CancellationToken ct = default)
        => SendAsync<FixedAssetTransferDto>(HttpMethod.Put, $"api/v1/fixed-assets/transfers/{id}", accessToken, request, ct);

    public async Task<bool> DeleteTransferAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/fixed-assets/transfers/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public async Task<bool> ApproveTransferAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Put, $"api/v1/fixed-assets/transfers/{id}/approve", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public async Task<bool> RejectTransferAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Put, $"api/v1/fixed-assets/transfers/{id}/reject", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<FixedAssetMaintenanceOrderDto>?> GetMaintenanceOrdersAsync(string accessToken, FixedAssetMaintenanceOrderPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);
        AddIntParameter(parameters, "assetId", request.AssetId);
        AddEnumParameter(parameters, "maintenanceType", request.MaintenanceType);
        AddEnumParameter(parameters, "status", request.Status);
        AddDateParameter(parameters, "orderDateFrom", request.OrderDateFrom);
        AddDateParameter(parameters, "orderDateTo", request.OrderDateTo);

        return SendAsync<PagedResult<FixedAssetMaintenanceOrderDto>>(HttpMethod.Get, $"api/v1/fixed-assets/maintenance-orders?{string.Join("&", parameters)}", accessToken, null, ct);
    }

    public Task<FixedAssetMaintenanceOrderDto?> GetMaintenanceOrderByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<FixedAssetMaintenanceOrderDto>(HttpMethod.Get, $"api/v1/fixed-assets/maintenance-orders/{id}", accessToken, null, ct);

    public Task<FixedAssetMaintenanceOrderDto?> CreateMaintenanceOrderAsync(string accessToken, FixedAssetMaintenanceOrderDto request, CancellationToken ct = default)
        => SendAsync<FixedAssetMaintenanceOrderDto>(HttpMethod.Post, "api/v1/fixed-assets/maintenance-orders", accessToken, request, ct);

    public Task<FixedAssetMaintenanceOrderDto?> UpdateMaintenanceOrderAsync(string accessToken, int id, FixedAssetMaintenanceOrderDto request, CancellationToken ct = default)
        => SendAsync<FixedAssetMaintenanceOrderDto>(HttpMethod.Put, $"api/v1/fixed-assets/maintenance-orders/{id}", accessToken, request, ct);

    public async Task<bool> DeleteMaintenanceOrderAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/fixed-assets/maintenance-orders/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public async Task<bool> StartMaintenanceOrderAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Put, $"api/v1/fixed-assets/maintenance-orders/{id}/start", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public async Task<bool> CompleteMaintenanceOrderAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Put, $"api/v1/fixed-assets/maintenance-orders/{id}/complete", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public async Task<bool> CancelMaintenanceOrderAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Put, $"api/v1/fixed-assets/maintenance-orders/{id}/cancel", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<FixedAssetDisposalDto>?> GetDisposalsAsync(string accessToken, FixedAssetDisposalPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);
        AddIntParameter(parameters, "assetId", request.AssetId);
        AddEnumParameter(parameters, "disposalType", request.DisposalType);
        AddEnumParameter(parameters, "status", request.Status);
        AddDateParameter(parameters, "disposalDateFrom", request.DisposalDateFrom);
        AddDateParameter(parameters, "disposalDateTo", request.DisposalDateTo);

        return SendAsync<PagedResult<FixedAssetDisposalDto>>(HttpMethod.Get, $"api/v1/fixed-assets/disposals?{string.Join("&", parameters)}", accessToken, null, ct);
    }

    public Task<FixedAssetDisposalDto?> GetDisposalByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<FixedAssetDisposalDto>(HttpMethod.Get, $"api/v1/fixed-assets/disposals/{id}", accessToken, null, ct);

    public Task<FixedAssetDisposalDto?> CreateDisposalAsync(string accessToken, FixedAssetDisposalDto request, CancellationToken ct = default)
        => SendAsync<FixedAssetDisposalDto>(HttpMethod.Post, "api/v1/fixed-assets/disposals", accessToken, request, ct);

    public Task<FixedAssetDisposalDto?> UpdateDisposalAsync(string accessToken, int id, FixedAssetDisposalDto request, CancellationToken ct = default)
        => SendAsync<FixedAssetDisposalDto>(HttpMethod.Put, $"api/v1/fixed-assets/disposals/{id}", accessToken, request, ct);

    public async Task<bool> DeleteDisposalAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/fixed-assets/disposals/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public async Task<bool> ApproveDisposalAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Put, $"api/v1/fixed-assets/disposals/{id}/approve", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public async Task<bool> PostDisposalAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Put, $"api/v1/fixed-assets/disposals/{id}/post", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public async Task<bool> CancelDisposalAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Put, $"api/v1/fixed-assets/disposals/{id}/cancel", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<FixedAssetRevaluationDto>?> GetRevaluationsAsync(string accessToken, FixedAssetRevaluationPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);
        AddIntParameter(parameters, "assetId", request.AssetId);
        AddEnumParameter(parameters, "status", request.Status);
        AddDateParameter(parameters, "revaluationDateFrom", request.RevaluationDateFrom);
        AddDateParameter(parameters, "revaluationDateTo", request.RevaluationDateTo);

        return SendAsync<PagedResult<FixedAssetRevaluationDto>>(HttpMethod.Get, $"api/v1/fixed-assets/revaluations?{string.Join("&", parameters)}", accessToken, null, ct);
    }

    public Task<FixedAssetRevaluationDto?> GetRevaluationByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<FixedAssetRevaluationDto>(HttpMethod.Get, $"api/v1/fixed-assets/revaluations/{id}", accessToken, null, ct);

    public Task<FixedAssetRevaluationDto?> CreateRevaluationAsync(string accessToken, FixedAssetRevaluationDto request, CancellationToken ct = default)
        => SendAsync<FixedAssetRevaluationDto>(HttpMethod.Post, "api/v1/fixed-assets/revaluations", accessToken, request, ct);

    public Task<FixedAssetRevaluationDto?> UpdateRevaluationAsync(string accessToken, int id, FixedAssetRevaluationDto request, CancellationToken ct = default)
        => SendAsync<FixedAssetRevaluationDto>(HttpMethod.Put, $"api/v1/fixed-assets/revaluations/{id}", accessToken, request, ct);

    public async Task<bool> DeleteRevaluationAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/fixed-assets/revaluations/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public async Task<bool> ApproveRevaluationAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Put, $"api/v1/fixed-assets/revaluations/{id}/approve", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public async Task<bool> PostRevaluationAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Put, $"api/v1/fixed-assets/revaluations/{id}/post", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    private static void AddPagedParameters(List<string> parameters, PagedRequest request)
    {
        parameters.Add($"page={request.Page}");
        parameters.Add($"pageSize={request.PageSize}");
        parameters.Add($"search={Uri.EscapeDataString(request.Search ?? string.Empty)}");
        parameters.Add($"sortBy={Uri.EscapeDataString(request.SortBy ?? string.Empty)}");
        parameters.Add($"sortDirection={Uri.EscapeDataString(request.SortDirection ?? string.Empty)}");
    }

    private static void AddTextParameter(List<string> parameters, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            parameters.Add($"{key}={Uri.EscapeDataString(value.Trim())}");
        }
    }

    private static void AddBooleanParameter(List<string> parameters, string key, bool? value)
    {
        if (value.HasValue)
        {
            parameters.Add($"{key}={(value.Value ? "true" : "false")}");
        }
    }

    private static void AddIntParameter(List<string> parameters, string key, int? value)
    {
        if (value.HasValue)
        {
            parameters.Add($"{key}={value.Value}");
        }
    }

    private static void AddShortParameter(List<string> parameters, string key, short? value)
    {
        if (value.HasValue)
        {
            parameters.Add($"{key}={value.Value}");
        }
    }

    private static void AddByteParameter(List<string> parameters, string key, byte? value)
    {
        if (value.HasValue)
        {
            parameters.Add($"{key}={value.Value}");
        }
    }

    private static void AddDecimalParameter(List<string> parameters, string key, decimal? value)
    {
        if (value.HasValue)
        {
            parameters.Add($"{key}={Uri.EscapeDataString(value.Value.ToString(System.Globalization.CultureInfo.InvariantCulture))}");
        }
    }

    private static void AddDateParameter(List<string> parameters, string key, DateOnly? value)
    {
        if (value.HasValue)
        {
            parameters.Add($"{key}={value.Value:yyyy-MM-dd}");
        }
    }

    private static void AddEnumParameter<TEnum>(List<string> parameters, string key, TEnum? value)
        where TEnum : struct, Enum
    {
        if (value.HasValue)
        {
            parameters.Add($"{key}={Convert.ToInt32(value.Value)}");
        }
    }
}

