using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Purchasing;

namespace ERP.Web.Services;

public interface IPurchasingApiClient
{
    Task<PurchasingDashboardDto?> GetDashboardAsync(string accessToken, CancellationToken ct = default);

    Task<PagedResult<VendorCategoryDto>?> GetVendorCategoriesAsync(string accessToken, VendorCategoryPagedRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<PurchasingOptionDto>> GetVendorCategoryOptionsAsync(string accessToken, CancellationToken ct = default);
    Task<VendorCategoryDto?> GetVendorCategoryByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<VendorCategoryDto?> CreateVendorCategoryAsync(string accessToken, VendorCategoryDto request, CancellationToken ct = default);
    Task<VendorCategoryDto?> UpdateVendorCategoryAsync(string accessToken, int id, VendorCategoryDto request, CancellationToken ct = default);
    Task<bool> DeleteVendorCategoryAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<ApprovalConfigDto>?> GetApprovalConfigsAsync(string accessToken, ApprovalConfigPagedRequest request, CancellationToken ct = default);
    Task<ApprovalConfigDto?> GetApprovalConfigByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApprovalConfigDto?> CreateApprovalConfigAsync(string accessToken, ApprovalConfigDto request, CancellationToken ct = default);
    Task<ApprovalConfigDto?> UpdateApprovalConfigAsync(string accessToken, int id, ApprovalConfigDto request, CancellationToken ct = default);
    Task<bool> DeleteApprovalConfigAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<BuyerGroupDto>?> GetBuyerGroupsAsync(string accessToken, BuyerGroupPagedRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<PurchasingOptionDto>> GetBuyerGroupOptionsAsync(string accessToken, CancellationToken ct = default);
    Task<BuyerGroupDto?> GetBuyerGroupByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<BuyerGroupDto?> CreateBuyerGroupAsync(string accessToken, BuyerGroupDto request, CancellationToken ct = default);
    Task<BuyerGroupDto?> UpdateBuyerGroupAsync(string accessToken, int id, BuyerGroupDto request, CancellationToken ct = default);
    Task<bool> DeleteBuyerGroupAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<PurchasingVendorDto>?> GetVendorsAsync(string accessToken, PurchasingVendorPagedRequest request, CancellationToken ct = default);
    Task<PurchasingVendorDto?> GetVendorByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<PurchasingVendorDto?> SetApprovedVendorAsync(string accessToken, int id, bool isApproved, DateOnly? approvedDate = null, CancellationToken ct = default);
}
