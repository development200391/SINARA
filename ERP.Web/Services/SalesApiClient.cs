using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Sales;
using System.Globalization;

namespace ERP.Web.Services;

public sealed class SalesApiClient(HttpClient httpClient, ILogger<SalesApiClient> logger) : ApiClientBase(httpClient, logger, "Sales"), ISalesApiClient
{
    public Task<SalesDashboardDto?> GetDashboardAsync(string accessToken, CancellationToken ct = default)
        => SendWithResultAsync<SalesDashboardDto>(HttpMethod.Get, "api/v1/sales/dashboard", accessToken, null, ct).ToDataAsync();

    public Task<PagedResult<SalesCustomerCategoryDto>?> GetCustomerCategoriesAsync(string accessToken, SalesCustomerCategoryPagedRequest request, CancellationToken ct = default)
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

        if (request.DefaultPriceListId.HasValue)
        {
            parameters.Add($"defaultPriceListId={request.DefaultPriceListId.Value}");
        }

        if (request.DefaultPaymentTermsFrom.HasValue)
        {
            parameters.Add($"defaultPaymentTermsFrom={request.DefaultPaymentTermsFrom.Value}");
        }

        if (request.DefaultPaymentTermsTo.HasValue)
        {
            parameters.Add($"defaultPaymentTermsTo={request.DefaultPaymentTermsTo.Value}");
        }

        if (request.DefaultCreditLimitFrom.HasValue)
        {
            parameters.Add($"defaultCreditLimitFrom={request.DefaultCreditLimitFrom.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        if (request.DefaultCreditLimitTo.HasValue)
        {
            parameters.Add($"defaultCreditLimitTo={request.DefaultCreditLimitTo.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        if (request.IsActive.HasValue)
        {
            parameters.Add($"isActive={(request.IsActive.Value ? "true" : "false")}");
        }

        return SendWithResultAsync<PagedResult<SalesCustomerCategoryDto>>(HttpMethod.Get, $"api/v1/sales/customer-categories?{string.Join("&", parameters)}", accessToken, null, ct).ToDataAsync();
    }

    public async Task<IReadOnlyList<SalesOptionDto>> GetCustomerCategoryOptionsAsync(string accessToken, CancellationToken ct = default)
        => await SendWithResultAsync<IReadOnlyList<SalesOptionDto>>(HttpMethod.Get, "api/v1/sales/customer-categories/options", accessToken, null, ct).ToDataAsync() ?? [];

    public Task<SalesCustomerCategoryDto?> GetCustomerCategoryByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<SalesCustomerCategoryDto>(HttpMethod.Get, $"api/v1/sales/customer-categories/{id}", accessToken, null, ct).ToDataAsync();

    public Task<ApiCallResult<SalesCustomerCategoryDto>> CreateCustomerCategoryAsync(string accessToken, SalesCustomerCategoryDto request, CancellationToken ct = default)
        => SendWithResultAsync<SalesCustomerCategoryDto>(HttpMethod.Post, "api/v1/sales/customer-categories", accessToken, request, ct);

    public Task<ApiCallResult<SalesCustomerCategoryDto>> UpdateCustomerCategoryAsync(string accessToken, int id, SalesCustomerCategoryDto request, CancellationToken ct = default)
        => SendWithResultAsync<SalesCustomerCategoryDto>(HttpMethod.Put, $"api/v1/sales/customer-categories/{id}", accessToken, request, ct);

    public Task<ApiCallResult<object?>> DeleteCustomerCategoryAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Delete, $"api/v1/sales/customer-categories/{id}", accessToken, null, ct);

    public Task<PagedResult<SalesPriceListDto>?> GetPriceListsAsync(string accessToken, SalesPriceListPagedRequest request, CancellationToken ct = default)
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

        if (request.Type.HasValue)
        {
            parameters.Add($"type={(int)request.Type.Value}");
        }

        if (!string.IsNullOrWhiteSpace(request.CurrencyCode))
        {
            parameters.Add($"currencyCode={Uri.EscapeDataString(request.CurrencyCode.Trim())}");
        }

        AddDateOnlyParameter(parameters, "validFromFrom", request.ValidFromFrom);
        AddDateOnlyParameter(parameters, "validFromTo", request.ValidFromTo);
        AddDateOnlyParameter(parameters, "validToFrom", request.ValidToFrom);
        AddDateOnlyParameter(parameters, "validToTo", request.ValidToTo);

        if (request.IsActive.HasValue)
        {
            parameters.Add($"isActive={(request.IsActive.Value ? "true" : "false")}");
        }

        return SendWithResultAsync<PagedResult<SalesPriceListDto>>(HttpMethod.Get, $"api/v1/sales/price-lists?{string.Join("&", parameters)}", accessToken, null, ct).ToDataAsync();
    }

    public async Task<IReadOnlyList<SalesOptionDto>> GetPriceListOptionsAsync(string accessToken, CancellationToken ct = default)
        => await SendWithResultAsync<IReadOnlyList<SalesOptionDto>>(HttpMethod.Get, "api/v1/sales/price-lists/options", accessToken, null, ct).ToDataAsync() ?? [];

    public Task<SalesPriceListDto?> GetPriceListByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<SalesPriceListDto>(HttpMethod.Get, $"api/v1/sales/price-lists/{id}", accessToken, null, ct).ToDataAsync();

    public Task<ApiCallResult<SalesPriceListDto>> CreatePriceListAsync(string accessToken, SalesPriceListDto request, CancellationToken ct = default)
        => SendWithResultAsync<SalesPriceListDto>(HttpMethod.Post, "api/v1/sales/price-lists", accessToken, request, ct);

    public Task<ApiCallResult<SalesPriceListDto>> UpdatePriceListAsync(string accessToken, int id, SalesPriceListDto request, CancellationToken ct = default)
        => SendWithResultAsync<SalesPriceListDto>(HttpMethod.Put, $"api/v1/sales/price-lists/{id}", accessToken, request, ct);

    public Task<ApiCallResult<object?>> DeletePriceListAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Delete, $"api/v1/sales/price-lists/{id}", accessToken, null, ct);

    public Task<PagedResult<SalesPriceListItemDto>?> GetPriceListItemsAsync(string accessToken, int priceListId, SalesPriceListItemPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (request.ItemId.HasValue)
        {
            parameters.Add($"itemId={request.ItemId.Value}");
        }

        if (request.UomId.HasValue)
        {
            parameters.Add($"uomId={request.UomId.Value}");
        }

        AddDecimalParameter(parameters, "minQtyFrom", request.MinQtyFrom);
        AddDecimalParameter(parameters, "minQtyTo", request.MinQtyTo);
        AddDecimalParameter(parameters, "unitPriceFrom", request.UnitPriceFrom);
        AddDecimalParameter(parameters, "unitPriceTo", request.UnitPriceTo);
        AddDecimalParameter(parameters, "discountPctFrom", request.DiscountPctFrom);
        AddDecimalParameter(parameters, "discountPctTo", request.DiscountPctTo);

        return SendWithResultAsync<PagedResult<SalesPriceListItemDto>>(HttpMethod.Get, $"api/v1/sales/price-lists/{priceListId}/items?{string.Join("&", parameters)}", accessToken, null, ct).ToDataAsync();
    }

    public Task<SalesPriceListItemDto?> GetPriceListItemByIdAsync(string accessToken, int priceListId, int id, CancellationToken ct = default)
        => SendWithResultAsync<SalesPriceListItemDto>(HttpMethod.Get, $"api/v1/sales/price-lists/{priceListId}/items/{id}", accessToken, null, ct).ToDataAsync();

    public Task<ApiCallResult<SalesPriceListItemDto>> CreatePriceListItemAsync(string accessToken, int priceListId, SalesPriceListItemDto request, CancellationToken ct = default)
        => SendWithResultAsync<SalesPriceListItemDto>(HttpMethod.Post, $"api/v1/sales/price-lists/{priceListId}/items", accessToken, request, ct);

    public Task<ApiCallResult<SalesPriceListItemDto>> UpdatePriceListItemAsync(string accessToken, int priceListId, int id, SalesPriceListItemDto request, CancellationToken ct = default)
        => SendWithResultAsync<SalesPriceListItemDto>(HttpMethod.Put, $"api/v1/sales/price-lists/{priceListId}/items/{id}", accessToken, request, ct);

    public Task<ApiCallResult<object?>> DeletePriceListItemAsync(string accessToken, int priceListId, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Delete, $"api/v1/sales/price-lists/{priceListId}/items/{id}", accessToken, null, ct);

    public Task<PagedResult<SalesApprovalConfigDto>?> GetApprovalConfigsAsync(string accessToken, SalesApprovalConfigPagedRequest request, CancellationToken ct = default)
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

        AddDecimalParameter(parameters, "minAmountFrom", request.MinAmountFrom);
        AddDecimalParameter(parameters, "minAmountTo", request.MinAmountTo);
        AddDecimalParameter(parameters, "maxAmountFrom", request.MaxAmountFrom);
        AddDecimalParameter(parameters, "maxAmountTo", request.MaxAmountTo);
        AddDecimalParameter(parameters, "maxDiscountPctFrom", request.MaxDiscountPctFrom);
        AddDecimalParameter(parameters, "maxDiscountPctTo", request.MaxDiscountPctTo);

        if (request.ApproverRoleId.HasValue)
        {
            parameters.Add($"approverRoleId={request.ApproverRoleId.Value}");
        }

        if (request.ApproverEmployeeId.HasValue)
        {
            parameters.Add($"approverEmployeeId={request.ApproverEmployeeId.Value}");
        }

        if (request.IsActive.HasValue)
        {
            parameters.Add($"isActive={(request.IsActive.Value ? "true" : "false")}");
        }

        return SendWithResultAsync<PagedResult<SalesApprovalConfigDto>>(HttpMethod.Get, $"api/v1/sales/approval-configs?{string.Join("&", parameters)}", accessToken, null, ct).ToDataAsync();
    }

    public Task<SalesApprovalConfigDto?> GetApprovalConfigByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<SalesApprovalConfigDto>(HttpMethod.Get, $"api/v1/sales/approval-configs/{id}", accessToken, null, ct).ToDataAsync();

    public Task<ApiCallResult<SalesApprovalConfigDto>> CreateApprovalConfigAsync(string accessToken, SalesApprovalConfigDto request, CancellationToken ct = default)
        => SendWithResultAsync<SalesApprovalConfigDto>(HttpMethod.Post, "api/v1/sales/approval-configs", accessToken, request, ct);

    public Task<ApiCallResult<SalesApprovalConfigDto>> UpdateApprovalConfigAsync(string accessToken, int id, SalesApprovalConfigDto request, CancellationToken ct = default)
        => SendWithResultAsync<SalesApprovalConfigDto>(HttpMethod.Put, $"api/v1/sales/approval-configs/{id}", accessToken, request, ct);

    public Task<ApiCallResult<object?>> DeleteApprovalConfigAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Delete, $"api/v1/sales/approval-configs/{id}", accessToken, null, ct);

    public Task<PagedResult<SalesTeamDto>?> GetTeamsAsync(string accessToken, SalesTeamPagedRequest request, CancellationToken ct = default)
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

        if (request.TeamLeaderId.HasValue)
        {
            parameters.Add($"teamLeaderId={request.TeamLeaderId.Value}");
        }

        if (request.MemberEmployeeId.HasValue)
        {
            parameters.Add($"memberEmployeeId={request.MemberEmployeeId.Value}");
        }

        if (request.IsActive.HasValue)
        {
            parameters.Add($"isActive={(request.IsActive.Value ? "true" : "false")}");
        }

        return SendWithResultAsync<PagedResult<SalesTeamDto>>(HttpMethod.Get, $"api/v1/sales/teams?{string.Join("&", parameters)}", accessToken, null, ct).ToDataAsync();
    }

    public async Task<IReadOnlyList<SalesOptionDto>> GetTeamOptionsAsync(string accessToken, CancellationToken ct = default)
        => await SendWithResultAsync<IReadOnlyList<SalesOptionDto>>(HttpMethod.Get, "api/v1/sales/teams/options", accessToken, null, ct).ToDataAsync() ?? [];

    public Task<SalesTeamDto?> GetTeamByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<SalesTeamDto>(HttpMethod.Get, $"api/v1/sales/teams/{id}", accessToken, null, ct).ToDataAsync();

    public Task<ApiCallResult<SalesTeamDto>> CreateTeamAsync(string accessToken, SalesTeamDto request, CancellationToken ct = default)
        => SendWithResultAsync<SalesTeamDto>(HttpMethod.Post, "api/v1/sales/teams", accessToken, request, ct);

    public Task<ApiCallResult<SalesTeamDto>> UpdateTeamAsync(string accessToken, int id, SalesTeamDto request, CancellationToken ct = default)
        => SendWithResultAsync<SalesTeamDto>(HttpMethod.Put, $"api/v1/sales/teams/{id}", accessToken, request, ct);

    public Task<ApiCallResult<object?>> DeleteTeamAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Delete, $"api/v1/sales/teams/{id}", accessToken, null, ct);

    public Task<PagedResult<SalesCustomerDto>?> GetCustomersAsync(string accessToken, SalesCustomerPagedRequest request, CancellationToken ct = default)
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

        if (request.CustomerCategoryId.HasValue)
        {
            parameters.Add($"customerCategoryId={request.CustomerCategoryId.Value}");
        }

        if (request.PriceListId.HasValue)
        {
            parameters.Add($"priceListId={request.PriceListId.Value}");
        }

        if (request.SalesEmployeeId.HasValue)
        {
            parameters.Add($"salesEmployeeId={request.SalesEmployeeId.Value}");
        }

        if (request.SalesTeamId.HasValue)
        {
            parameters.Add($"salesTeamId={request.SalesTeamId.Value}");
        }

        AddDecimalParameter(parameters, "creditLimitFrom", request.CreditLimitFrom);
        AddDecimalParameter(parameters, "creditLimitTo", request.CreditLimitTo);
        AddDecimalParameter(parameters, "creditUsedFrom", request.CreditUsedFrom);
        AddDecimalParameter(parameters, "creditUsedTo", request.CreditUsedTo);
        AddDateOnlyParameter(parameters, "lastOrderDateFrom", request.LastOrderDateFrom);
        AddDateOnlyParameter(parameters, "lastOrderDateTo", request.LastOrderDateTo);
        AddDecimalParameter(parameters, "totalYtdSalesFrom", request.TotalYtdSalesFrom);
        AddDecimalParameter(parameters, "totalYtdSalesTo", request.TotalYtdSalesTo);

        if (request.IsOverCreditLimit.HasValue)
        {
            parameters.Add($"isOverCreditLimit={(request.IsOverCreditLimit.Value ? "true" : "false")}");
        }

        if (request.IsActive.HasValue)
        {
            parameters.Add($"isActive={(request.IsActive.Value ? "true" : "false")}");
        }

        return SendWithResultAsync<PagedResult<SalesCustomerDto>>(HttpMethod.Get, $"api/v1/sales/customers?{string.Join("&", parameters)}", accessToken, null, ct).ToDataAsync();
    }

    public Task<SalesCustomerDto?> GetCustomerByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<SalesCustomerDto>(HttpMethod.Get, $"api/v1/sales/customers/{id}", accessToken, null, ct).ToDataAsync();

    private static void AddPagedParameters(List<string> parameters, PagedRequest request)
    {
        parameters.Add($"page={request.Page}");
        parameters.Add($"pageSize={request.PageSize}");
        parameters.Add($"search={Uri.EscapeDataString(request.Search ?? string.Empty)}");
        parameters.Add($"sortBy={Uri.EscapeDataString(request.SortBy ?? string.Empty)}");
        parameters.Add($"sortDirection={Uri.EscapeDataString(request.SortDirection ?? string.Empty)}");
    }

    private static void AddDateOnlyParameter(List<string> parameters, string key, DateOnly? value)
    {
        if (!value.HasValue)
        {
            return;
        }

        parameters.Add($"{key}={value.Value:yyyy-MM-dd}");
    }

    private static void AddDecimalParameter(List<string> parameters, string key, decimal? value)
    {
        if (!value.HasValue)
        {
            return;
        }

        parameters.Add($"{key}={value.Value.ToString(CultureInfo.InvariantCulture)}");
    }
}
