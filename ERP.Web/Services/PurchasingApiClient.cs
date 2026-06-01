using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Purchasing;

namespace ERP.Web.Services;

public sealed class PurchasingApiClient(HttpClient httpClient, ILogger<PurchasingApiClient> logger) : IPurchasingApiClient
{
    public Task<PurchasingDashboardDto?> GetDashboardAsync(string accessToken, CancellationToken ct = default)
        => SendAsync<PurchasingDashboardDto>(HttpMethod.Get, "api/v1/purchasing/dashboard", accessToken, null, ct);

    public Task<PagedResult<VendorCategoryDto>?> GetVendorCategoriesAsync(string accessToken, VendorCategoryPagedRequest request, CancellationToken ct = default)
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

        var query = $"api/v1/purchasing/vendor-categories?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<VendorCategoryDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public async Task<IReadOnlyList<PurchasingOptionDto>> GetVendorCategoryOptionsAsync(string accessToken, CancellationToken ct = default)
        => await SendAsync<IReadOnlyList<PurchasingOptionDto>>(HttpMethod.Get, "api/v1/purchasing/vendor-categories/options", accessToken, null, ct) ?? [];

    public Task<VendorCategoryDto?> GetVendorCategoryByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<VendorCategoryDto>(HttpMethod.Get, $"api/v1/purchasing/vendor-categories/{id}", accessToken, null, ct);

    public Task<VendorCategoryDto?> CreateVendorCategoryAsync(string accessToken, VendorCategoryDto request, CancellationToken ct = default)
        => SendAsync<VendorCategoryDto>(HttpMethod.Post, "api/v1/purchasing/vendor-categories", accessToken, request, ct);

    public Task<VendorCategoryDto?> UpdateVendorCategoryAsync(string accessToken, int id, VendorCategoryDto request, CancellationToken ct = default)
        => SendAsync<VendorCategoryDto>(HttpMethod.Put, $"api/v1/purchasing/vendor-categories/{id}", accessToken, request, ct);

    public async Task<bool> DeleteVendorCategoryAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/purchasing/vendor-categories/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<ApprovalConfigDto>?> GetApprovalConfigsAsync(string accessToken, ApprovalConfigPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (request.DocumentType.HasValue)
        {
            parameters.Add($"documentType={(int)request.DocumentType.Value}");
        }

        if (request.Level.HasValue)
        {
            parameters.Add($"level={request.Level.Value}");
        }

        if (request.MinAmountFrom.HasValue)
        {
            parameters.Add($"minAmountFrom={request.MinAmountFrom.Value}");
        }

        if (request.MinAmountTo.HasValue)
        {
            parameters.Add($"minAmountTo={request.MinAmountTo.Value}");
        }

        if (request.MaxAmountFrom.HasValue)
        {
            parameters.Add($"maxAmountFrom={request.MaxAmountFrom.Value}");
        }

        if (request.MaxAmountTo.HasValue)
        {
            parameters.Add($"maxAmountTo={request.MaxAmountTo.Value}");
        }

        if (request.ApproverEmployeeId.HasValue)
        {
            parameters.Add($"approverEmployeeId={request.ApproverEmployeeId.Value}");
        }

        if (request.IsActive.HasValue)
        {
            parameters.Add($"isActive={(request.IsActive.Value ? "true" : "false")}");
        }

        var query = $"api/v1/purchasing/approval-configs?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<ApprovalConfigDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<ApprovalConfigDto?> GetApprovalConfigByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<ApprovalConfigDto>(HttpMethod.Get, $"api/v1/purchasing/approval-configs/{id}", accessToken, null, ct);

    public Task<ApprovalConfigDto?> CreateApprovalConfigAsync(string accessToken, ApprovalConfigDto request, CancellationToken ct = default)
        => SendAsync<ApprovalConfigDto>(HttpMethod.Post, "api/v1/purchasing/approval-configs", accessToken, request, ct);

    public Task<ApprovalConfigDto?> UpdateApprovalConfigAsync(string accessToken, int id, ApprovalConfigDto request, CancellationToken ct = default)
        => SendAsync<ApprovalConfigDto>(HttpMethod.Put, $"api/v1/purchasing/approval-configs/{id}", accessToken, request, ct);

    public async Task<bool> DeleteApprovalConfigAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/purchasing/approval-configs/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<BuyerGroupDto>?> GetBuyerGroupsAsync(string accessToken, BuyerGroupPagedRequest request, CancellationToken ct = default)
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

        if (request.BuyerEmployeeId.HasValue)
        {
            parameters.Add($"buyerEmployeeId={request.BuyerEmployeeId.Value}");
        }

        if (request.ItemCategoryId.HasValue)
        {
            parameters.Add($"itemCategoryId={request.ItemCategoryId.Value}");
        }

        if (request.IsActive.HasValue)
        {
            parameters.Add($"isActive={(request.IsActive.Value ? "true" : "false")}");
        }

        var query = $"api/v1/purchasing/buyer-groups?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<BuyerGroupDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public async Task<IReadOnlyList<PurchasingOptionDto>> GetBuyerGroupOptionsAsync(string accessToken, CancellationToken ct = default)
        => await SendAsync<IReadOnlyList<PurchasingOptionDto>>(HttpMethod.Get, "api/v1/purchasing/buyer-groups/options", accessToken, null, ct) ?? [];

    public Task<BuyerGroupDto?> GetBuyerGroupByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<BuyerGroupDto>(HttpMethod.Get, $"api/v1/purchasing/buyer-groups/{id}", accessToken, null, ct);

    public Task<BuyerGroupDto?> CreateBuyerGroupAsync(string accessToken, BuyerGroupDto request, CancellationToken ct = default)
        => SendAsync<BuyerGroupDto>(HttpMethod.Post, "api/v1/purchasing/buyer-groups", accessToken, request, ct);

    public Task<BuyerGroupDto?> UpdateBuyerGroupAsync(string accessToken, int id, BuyerGroupDto request, CancellationToken ct = default)
        => SendAsync<BuyerGroupDto>(HttpMethod.Put, $"api/v1/purchasing/buyer-groups/{id}", accessToken, request, ct);

    public async Task<bool> DeleteBuyerGroupAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/purchasing/buyer-groups/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<PurchasingVendorDto>?> GetVendorsAsync(string accessToken, PurchasingVendorPagedRequest request, CancellationToken ct = default)
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

        if (request.VendorCategoryId.HasValue)
        {
            parameters.Add($"vendorCategoryId={request.VendorCategoryId.Value}");
        }

        if (request.BuyerGroupId.HasValue)
        {
            parameters.Add($"buyerGroupId={request.BuyerGroupId.Value}");
        }

        if (request.IsApprovedVendor.HasValue)
        {
            parameters.Add($"isApprovedVendor={(request.IsApprovedVendor.Value ? "true" : "false")}");
        }

        if (request.PerformanceScoreFrom.HasValue)
        {
            parameters.Add($"performanceScoreFrom={request.PerformanceScoreFrom.Value}");
        }

        if (request.PerformanceScoreTo.HasValue)
        {
            parameters.Add($"performanceScoreTo={request.PerformanceScoreTo.Value}");
        }

        if (request.PaymentTermsFrom.HasValue)
        {
            parameters.Add($"paymentTermsFrom={request.PaymentTermsFrom.Value}");
        }

        if (request.PaymentTermsTo.HasValue)
        {
            parameters.Add($"paymentTermsTo={request.PaymentTermsTo.Value}");
        }

        if (request.IsActive.HasValue)
        {
            parameters.Add($"isActive={(request.IsActive.Value ? "true" : "false")}");
        }

        var query = $"api/v1/purchasing/vendors?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<PurchasingVendorDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<PurchasingVendorDto?> GetVendorByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendAsync<PurchasingVendorDto>(HttpMethod.Get, $"api/v1/purchasing/vendors/{id}", accessToken, null, ct);

    public Task<PurchasingVendorDto?> SetApprovedVendorAsync(string accessToken, int id, bool isApproved, DateOnly? approvedDate = null, CancellationToken ct = default)
        => SendAsync<PurchasingVendorDto>(HttpMethod.Put, $"api/v1/purchasing/vendors/{id}/set-approved", accessToken, new SetApprovedVendorRequest
        {
            IsApproved = isApproved,
            ApprovedDate = approvedDate
        }, ct);

    private static void AddPagedParameters(List<string> parameters, PagedRequest request)
    {
        parameters.Add($"page={request.Page}");
        parameters.Add($"pageSize={request.PageSize}");
        parameters.Add($"search={Uri.EscapeDataString(request.Search ?? string.Empty)}");
        parameters.Add($"sortBy={Uri.EscapeDataString(request.SortBy ?? string.Empty)}");
        parameters.Add($"sortDirection={Uri.EscapeDataString(request.SortDirection ?? string.Empty)}");
    }

    private async Task<T?> SendAsync<T>(HttpMethod method, string uri, string accessToken, object? body, CancellationToken ct)
    {
        var response = await SendRawAsync(method, uri, accessToken, body, ct);
        if (response is null || !response.IsSuccessStatusCode)
        {
            return default;
        }

        try
        {
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to deserialize Purchasing API response from {Uri}", uri);
            return default;
        }
    }

    private async Task<HttpResponseMessage?> SendRawAsync(HttpMethod method, string uri, string accessToken, object? body, CancellationToken ct)
    {
        try
        {
            using var request = new HttpRequestMessage(method, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            if (body is not null)
            {
                request.Content = JsonContent.Create(body);
            }

            var response = await httpClient.SendAsync(request, ct);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new ApiUnauthorizedException(uri);
            }

            return response;
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Failed to call Purchasing API endpoint {Uri}", uri);
            return null;
        }
    }

    private sealed class SetApprovedVendorRequest
    {
        public bool IsApproved { get; set; }
        public DateOnly? ApprovedDate { get; set; }
    }
}
