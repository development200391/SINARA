using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.FixedAssets;

namespace ERP.Web.Services;

public interface IFixedAssetsApiClient
{
    Task<FixedAssetDashboardDto?> GetDashboardAsync(string accessToken, CancellationToken ct = default);

    Task<PagedResult<FixedAssetCategoryDto>?> GetAssetCategoriesAsync(string accessToken, FixedAssetCategoryPagedRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<FixedAssetOptionDto>> GetAssetCategoryOptionsAsync(string accessToken, CancellationToken ct = default);
    Task<FixedAssetCategoryDto?> GetAssetCategoryByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<FixedAssetCategoryDto?> CreateAssetCategoryAsync(string accessToken, FixedAssetCategoryDto request, CancellationToken ct = default);
    Task<FixedAssetCategoryDto?> UpdateAssetCategoryAsync(string accessToken, int id, FixedAssetCategoryDto request, CancellationToken ct = default);
    Task<bool> DeleteAssetCategoryAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<FixedAssetLocationDto>?> GetLocationsAsync(string accessToken, FixedAssetLocationPagedRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<FixedAssetOptionDto>> GetLocationOptionsAsync(string accessToken, CancellationToken ct = default);
    Task<FixedAssetLocationDto?> GetLocationByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<FixedAssetLocationDto?> CreateLocationAsync(string accessToken, FixedAssetLocationDto request, CancellationToken ct = default);
    Task<FixedAssetLocationDto?> UpdateLocationAsync(string accessToken, int id, FixedAssetLocationDto request, CancellationToken ct = default);
    Task<bool> DeleteLocationAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<FixedAssetDepreciationConfigDto>?> GetDepreciationConfigsAsync(string accessToken, FixedAssetDepreciationConfigPagedRequest request, CancellationToken ct = default);
    Task<FixedAssetDepreciationConfigDto?> GetDepreciationConfigByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<FixedAssetDepreciationConfigDto?> CreateDepreciationConfigAsync(string accessToken, FixedAssetDepreciationConfigDto request, CancellationToken ct = default);
    Task<FixedAssetDepreciationConfigDto?> UpdateDepreciationConfigAsync(string accessToken, int id, FixedAssetDepreciationConfigDto request, CancellationToken ct = default);
    Task<bool> DeleteDepreciationConfigAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<FixedAssetDto>?> GetAssetsAsync(string accessToken, FixedAssetPagedRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<FixedAssetOptionDto>> GetAssetOptionsAsync(string accessToken, CancellationToken ct = default);
    Task<FixedAssetDetailDto?> GetAssetByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<FixedAssetDto?> CreateAssetAsync(string accessToken, FixedAssetDto request, CancellationToken ct = default);
    Task<FixedAssetDto?> UpdateAssetAsync(string accessToken, int id, FixedAssetDto request, CancellationToken ct = default);
    Task<bool> DeleteAssetAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<FixedAssetDepreciationRunDto>?> GetDepreciationRunsAsync(string accessToken, FixedAssetDepreciationRunPagedRequest request, CancellationToken ct = default);
    Task<FixedAssetDepreciationRunDto?> GetDepreciationRunByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<FixedAssetDepreciationRunDto?> RunDepreciationAsync(string accessToken, RunDepreciationRequest request, CancellationToken ct = default);
    Task<bool> ApproveDepreciationRunAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<FixedAssetTransferDto>?> GetTransfersAsync(string accessToken, FixedAssetTransferPagedRequest request, CancellationToken ct = default);
    Task<FixedAssetTransferDto?> GetTransferByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<FixedAssetTransferDto?> CreateTransferAsync(string accessToken, FixedAssetTransferDto request, CancellationToken ct = default);
    Task<FixedAssetTransferDto?> UpdateTransferAsync(string accessToken, int id, FixedAssetTransferDto request, CancellationToken ct = default);
    Task<bool> DeleteTransferAsync(string accessToken, int id, CancellationToken ct = default);
    Task<bool> ApproveTransferAsync(string accessToken, int id, CancellationToken ct = default);
    Task<bool> RejectTransferAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<FixedAssetMaintenanceOrderDto>?> GetMaintenanceOrdersAsync(string accessToken, FixedAssetMaintenanceOrderPagedRequest request, CancellationToken ct = default);
    Task<FixedAssetMaintenanceOrderDto?> GetMaintenanceOrderByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<FixedAssetMaintenanceOrderDto?> CreateMaintenanceOrderAsync(string accessToken, FixedAssetMaintenanceOrderDto request, CancellationToken ct = default);
    Task<FixedAssetMaintenanceOrderDto?> UpdateMaintenanceOrderAsync(string accessToken, int id, FixedAssetMaintenanceOrderDto request, CancellationToken ct = default);
    Task<bool> DeleteMaintenanceOrderAsync(string accessToken, int id, CancellationToken ct = default);
    Task<bool> StartMaintenanceOrderAsync(string accessToken, int id, CancellationToken ct = default);
    Task<bool> CompleteMaintenanceOrderAsync(string accessToken, int id, CancellationToken ct = default);
    Task<bool> CancelMaintenanceOrderAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<FixedAssetDisposalDto>?> GetDisposalsAsync(string accessToken, FixedAssetDisposalPagedRequest request, CancellationToken ct = default);
    Task<FixedAssetDisposalDto?> GetDisposalByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<FixedAssetDisposalDto?> CreateDisposalAsync(string accessToken, FixedAssetDisposalDto request, CancellationToken ct = default);
    Task<FixedAssetDisposalDto?> UpdateDisposalAsync(string accessToken, int id, FixedAssetDisposalDto request, CancellationToken ct = default);
    Task<bool> DeleteDisposalAsync(string accessToken, int id, CancellationToken ct = default);
    Task<bool> ApproveDisposalAsync(string accessToken, int id, CancellationToken ct = default);
    Task<bool> PostDisposalAsync(string accessToken, int id, CancellationToken ct = default);
    Task<bool> CancelDisposalAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<FixedAssetRevaluationDto>?> GetRevaluationsAsync(string accessToken, FixedAssetRevaluationPagedRequest request, CancellationToken ct = default);
    Task<FixedAssetRevaluationDto?> GetRevaluationByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<FixedAssetRevaluationDto?> CreateRevaluationAsync(string accessToken, FixedAssetRevaluationDto request, CancellationToken ct = default);
    Task<FixedAssetRevaluationDto?> UpdateRevaluationAsync(string accessToken, int id, FixedAssetRevaluationDto request, CancellationToken ct = default);
    Task<bool> DeleteRevaluationAsync(string accessToken, int id, CancellationToken ct = default);
    Task<bool> ApproveRevaluationAsync(string accessToken, int id, CancellationToken ct = default);
    Task<bool> PostRevaluationAsync(string accessToken, int id, CancellationToken ct = default);
}
