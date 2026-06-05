using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.FixedAssets;

namespace ERP.Web.Services;

public interface IFixedAssetsApiClient
{
    Task<FixedAssetDashboardDto?> GetDashboardAsync(string accessToken, CancellationToken ct = default);

    Task<PagedResult<FixedAssetCategoryDto>?> GetAssetCategoriesAsync(string accessToken, FixedAssetCategoryPagedRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<FixedAssetOptionDto>> GetAssetCategoryOptionsAsync(string accessToken, CancellationToken ct = default);
    Task<FixedAssetCategoryDto?> GetAssetCategoryByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<FixedAssetCategoryDto>> CreateAssetCategoryAsync(string accessToken, FixedAssetCategoryDto request, CancellationToken ct = default);
    Task<ApiCallResult<FixedAssetCategoryDto>> UpdateAssetCategoryAsync(string accessToken, int id, FixedAssetCategoryDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteAssetCategoryAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<FixedAssetLocationDto>?> GetLocationsAsync(string accessToken, FixedAssetLocationPagedRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<FixedAssetOptionDto>> GetLocationOptionsAsync(string accessToken, CancellationToken ct = default);
    Task<FixedAssetLocationDto?> GetLocationByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<FixedAssetLocationDto>> CreateLocationAsync(string accessToken, FixedAssetLocationDto request, CancellationToken ct = default);
    Task<ApiCallResult<FixedAssetLocationDto>> UpdateLocationAsync(string accessToken, int id, FixedAssetLocationDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteLocationAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<FixedAssetDepreciationConfigDto>?> GetDepreciationConfigsAsync(string accessToken, FixedAssetDepreciationConfigPagedRequest request, CancellationToken ct = default);
    Task<FixedAssetDepreciationConfigDto?> GetDepreciationConfigByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<FixedAssetDepreciationConfigDto>> CreateDepreciationConfigAsync(string accessToken, FixedAssetDepreciationConfigDto request, CancellationToken ct = default);
    Task<ApiCallResult<FixedAssetDepreciationConfigDto>> UpdateDepreciationConfigAsync(string accessToken, int id, FixedAssetDepreciationConfigDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteDepreciationConfigAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<FixedAssetDto>?> GetAssetsAsync(string accessToken, FixedAssetPagedRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<FixedAssetOptionDto>> GetAssetOptionsAsync(string accessToken, CancellationToken ct = default);
    Task<FixedAssetDetailDto?> GetAssetByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<FixedAssetDto>> CreateAssetAsync(string accessToken, FixedAssetDto request, CancellationToken ct = default);
    Task<ApiCallResult<FixedAssetDto>> UpdateAssetAsync(string accessToken, int id, FixedAssetDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteAssetAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<FixedAssetDepreciationRunDto>?> GetDepreciationRunsAsync(string accessToken, FixedAssetDepreciationRunPagedRequest request, CancellationToken ct = default);
    Task<FixedAssetDepreciationRunDto?> GetDepreciationRunByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<FixedAssetDepreciationRunDto>> RunDepreciationAsync(string accessToken, RunDepreciationRequest request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> ApproveDepreciationRunAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<FixedAssetTransferDto>?> GetTransfersAsync(string accessToken, FixedAssetTransferPagedRequest request, CancellationToken ct = default);
    Task<FixedAssetTransferDto?> GetTransferByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<FixedAssetTransferDto>> CreateTransferAsync(string accessToken, FixedAssetTransferDto request, CancellationToken ct = default);
    Task<ApiCallResult<FixedAssetTransferDto>> UpdateTransferAsync(string accessToken, int id, FixedAssetTransferDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteTransferAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<object?>> ApproveTransferAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<object?>> RejectTransferAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<FixedAssetMaintenanceOrderDto>?> GetMaintenanceOrdersAsync(string accessToken, FixedAssetMaintenanceOrderPagedRequest request, CancellationToken ct = default);
    Task<FixedAssetMaintenanceOrderDto?> GetMaintenanceOrderByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<FixedAssetMaintenanceOrderDto>> CreateMaintenanceOrderAsync(string accessToken, FixedAssetMaintenanceOrderDto request, CancellationToken ct = default);
    Task<ApiCallResult<FixedAssetMaintenanceOrderDto>> UpdateMaintenanceOrderAsync(string accessToken, int id, FixedAssetMaintenanceOrderDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteMaintenanceOrderAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<object?>> StartMaintenanceOrderAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<object?>> CompleteMaintenanceOrderAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<object?>> CancelMaintenanceOrderAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<FixedAssetDisposalDto>?> GetDisposalsAsync(string accessToken, FixedAssetDisposalPagedRequest request, CancellationToken ct = default);
    Task<FixedAssetDisposalDto?> GetDisposalByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<FixedAssetDisposalDto>> CreateDisposalAsync(string accessToken, FixedAssetDisposalDto request, CancellationToken ct = default);
    Task<ApiCallResult<FixedAssetDisposalDto>> UpdateDisposalAsync(string accessToken, int id, FixedAssetDisposalDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteDisposalAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<object?>> ApproveDisposalAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<object?>> PostDisposalAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<object?>> CancelDisposalAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<FixedAssetRevaluationDto>?> GetRevaluationsAsync(string accessToken, FixedAssetRevaluationPagedRequest request, CancellationToken ct = default);
    Task<FixedAssetRevaluationDto?> GetRevaluationByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<FixedAssetRevaluationDto>> CreateRevaluationAsync(string accessToken, FixedAssetRevaluationDto request, CancellationToken ct = default);
    Task<ApiCallResult<FixedAssetRevaluationDto>> UpdateRevaluationAsync(string accessToken, int id, FixedAssetRevaluationDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteRevaluationAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<object?>> ApproveRevaluationAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<object?>> PostRevaluationAsync(string accessToken, int id, CancellationToken ct = default);
}
