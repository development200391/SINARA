using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Sales;

namespace ERP.Web.Services;

public interface ISalesApiClient
{
    Task<SalesDashboardDto?> GetDashboardAsync(string accessToken, CancellationToken ct = default);

    Task<PagedResult<SalesCustomerCategoryDto>?> GetCustomerCategoriesAsync(string accessToken, SalesCustomerCategoryPagedRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<SalesOptionDto>> GetCustomerCategoryOptionsAsync(string accessToken, CancellationToken ct = default);
    Task<SalesCustomerCategoryDto?> GetCustomerCategoryByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<SalesCustomerCategoryDto>> CreateCustomerCategoryAsync(string accessToken, SalesCustomerCategoryDto request, CancellationToken ct = default);
    Task<ApiCallResult<SalesCustomerCategoryDto>> UpdateCustomerCategoryAsync(string accessToken, int id, SalesCustomerCategoryDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteCustomerCategoryAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<SalesPriceListDto>?> GetPriceListsAsync(string accessToken, SalesPriceListPagedRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<SalesOptionDto>> GetPriceListOptionsAsync(string accessToken, CancellationToken ct = default);
    Task<SalesPriceListDto?> GetPriceListByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<SalesPriceListDto>> CreatePriceListAsync(string accessToken, SalesPriceListDto request, CancellationToken ct = default);
    Task<ApiCallResult<SalesPriceListDto>> UpdatePriceListAsync(string accessToken, int id, SalesPriceListDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeletePriceListAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<SalesPriceListItemDto>?> GetPriceListItemsAsync(string accessToken, int priceListId, SalesPriceListItemPagedRequest request, CancellationToken ct = default);
    Task<SalesPriceListItemDto?> GetPriceListItemByIdAsync(string accessToken, int priceListId, int id, CancellationToken ct = default);
    Task<ApiCallResult<SalesPriceListItemDto>> CreatePriceListItemAsync(string accessToken, int priceListId, SalesPriceListItemDto request, CancellationToken ct = default);
    Task<ApiCallResult<SalesPriceListItemDto>> UpdatePriceListItemAsync(string accessToken, int priceListId, int id, SalesPriceListItemDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeletePriceListItemAsync(string accessToken, int priceListId, int id, CancellationToken ct = default);

    Task<PagedResult<SalesApprovalConfigDto>?> GetApprovalConfigsAsync(string accessToken, SalesApprovalConfigPagedRequest request, CancellationToken ct = default);
    Task<SalesApprovalConfigDto?> GetApprovalConfigByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<SalesApprovalConfigDto>> CreateApprovalConfigAsync(string accessToken, SalesApprovalConfigDto request, CancellationToken ct = default);
    Task<ApiCallResult<SalesApprovalConfigDto>> UpdateApprovalConfigAsync(string accessToken, int id, SalesApprovalConfigDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteApprovalConfigAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<SalesTeamDto>?> GetTeamsAsync(string accessToken, SalesTeamPagedRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<SalesOptionDto>> GetTeamOptionsAsync(string accessToken, CancellationToken ct = default);
    Task<SalesTeamDto?> GetTeamByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<SalesTeamDto>> CreateTeamAsync(string accessToken, SalesTeamDto request, CancellationToken ct = default);
    Task<ApiCallResult<SalesTeamDto>> UpdateTeamAsync(string accessToken, int id, SalesTeamDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteTeamAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<SalesCustomerDto>?> GetCustomersAsync(string accessToken, SalesCustomerPagedRequest request, CancellationToken ct = default);
    Task<SalesCustomerDto?> GetCustomerByIdAsync(string accessToken, int id, CancellationToken ct = default);
}
