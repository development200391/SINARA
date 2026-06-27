using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Manufacturing;

namespace ERP.Web.Services;

public interface IManufacturingApiClient
{
    Task<ManufacturingDashboardDto?> GetDashboardAsync(string accessToken, CancellationToken ct = default);

    Task<PagedResult<ManufacturingWorkOrderDto>?> GetWorkOrdersAsync(string accessToken, ManufacturingWorkOrderPagedRequest request, CancellationToken ct = default);
    Task<ManufacturingWorkOrderDto?> GetWorkOrderByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<ManufacturingWorkOrderDto>> CreateWorkOrderAsync(string accessToken, ManufacturingWorkOrderDto request, CancellationToken ct = default);
    Task<ApiCallResult<ManufacturingWorkOrderDto>> UpdateWorkOrderAsync(string accessToken, int id, ManufacturingWorkOrderDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteWorkOrderAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<object?>> ReleaseWorkOrderAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<object?>> StartWorkOrderAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<object?>> CompleteWorkOrderAsync(string accessToken, int id, ManufacturingWorkOrderCompleteRequest? request = null, CancellationToken ct = default);
    Task<ApiCallResult<object?>> CloseWorkOrderAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<object?>> CancelWorkOrderAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<ManufacturingMrpRunDto>?> GetMrpRunsAsync(string accessToken, ManufacturingMrpRunPagedRequest request, CancellationToken ct = default);
    Task<ManufacturingMrpRunDto?> GetMrpRunByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<ManufacturingMrpRunDto>> CreateMrpRunAsync(string accessToken, ManufacturingMrpRunDto request, CancellationToken ct = default);
    Task<ApiCallResult<ManufacturingMrpRunDto>> UpdateMrpRunAsync(string accessToken, int id, ManufacturingMrpRunDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteMrpRunAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<object?>> RunMrpAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<object?>> CompleteMrpAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<object?>> CancelMrpAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<ManufacturingQcInspectionDto>?> GetQcInspectionsAsync(string accessToken, ManufacturingQcInspectionPagedRequest request, CancellationToken ct = default);
    Task<ManufacturingQcInspectionDto?> GetQcInspectionByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<ManufacturingQcInspectionDto>> CreateQcInspectionAsync(string accessToken, ManufacturingQcInspectionDto request, CancellationToken ct = default);
    Task<ApiCallResult<ManufacturingQcInspectionDto>> UpdateQcInspectionAsync(string accessToken, int id, ManufacturingQcInspectionDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteQcInspectionAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<object?>> StartQcInspectionAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<object?>> CompleteQcInspectionAsync(string accessToken, int id, ManufacturingQcCompleteRequest request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> CancelQcInspectionAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<ManufacturingScrapRecordDto>?> GetScrapRecordsAsync(string accessToken, ManufacturingScrapRecordPagedRequest request, CancellationToken ct = default);
    Task<ManufacturingScrapRecordDto?> GetScrapRecordByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<ManufacturingScrapRecordDto>> CreateScrapRecordAsync(string accessToken, ManufacturingScrapRecordDto request, CancellationToken ct = default);
    Task<ApiCallResult<ManufacturingScrapRecordDto>> UpdateScrapRecordAsync(string accessToken, int id, ManufacturingScrapRecordDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteScrapRecordAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<ManufacturingReworkOrderDto>?> GetReworkOrdersAsync(string accessToken, ManufacturingReworkOrderPagedRequest request, CancellationToken ct = default);
    Task<ManufacturingReworkOrderDto?> GetReworkOrderByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<ManufacturingReworkOrderDto>> CreateReworkOrderAsync(string accessToken, ManufacturingReworkOrderDto request, CancellationToken ct = default);
    Task<ApiCallResult<ManufacturingReworkOrderDto>> UpdateReworkOrderAsync(string accessToken, int id, ManufacturingReworkOrderDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteReworkOrderAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<object?>> StartReworkOrderAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<object?>> CompleteReworkOrderAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<object?>> CloseReworkOrderAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<object?>> CancelReworkOrderAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<ManufacturingBomDto>?> GetBomsAsync(string accessToken, ManufacturingBomPagedRequest request, CancellationToken ct = default);
    Task<ManufacturingBomDto?> GetBomByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<ManufacturingBomDto>> CreateBomAsync(string accessToken, ManufacturingBomDto request, CancellationToken ct = default);
    Task<ApiCallResult<ManufacturingBomDto>> UpdateBomAsync(string accessToken, int id, ManufacturingBomDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteBomAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<ManufacturingRoutingDto>?> GetRoutingsAsync(string accessToken, ManufacturingRoutingPagedRequest request, CancellationToken ct = default);
    Task<ManufacturingRoutingDto?> GetRoutingByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<ManufacturingRoutingDto>> CreateRoutingAsync(string accessToken, ManufacturingRoutingDto request, CancellationToken ct = default);
    Task<ApiCallResult<ManufacturingRoutingDto>> UpdateRoutingAsync(string accessToken, int id, ManufacturingRoutingDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteRoutingAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<ManufacturingWorkCenterDto>?> GetWorkCentersAsync(string accessToken, ManufacturingWorkCenterPagedRequest request, CancellationToken ct = default);
    Task<ManufacturingWorkCenterDto?> GetWorkCenterByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<ManufacturingWorkCenterDto>> CreateWorkCenterAsync(string accessToken, ManufacturingWorkCenterDto request, CancellationToken ct = default);
    Task<ApiCallResult<ManufacturingWorkCenterDto>> UpdateWorkCenterAsync(string accessToken, int id, ManufacturingWorkCenterDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteWorkCenterAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<ManufacturingQcParameterDto>?> GetQcParametersAsync(string accessToken, ManufacturingQcParameterPagedRequest request, CancellationToken ct = default);
    Task<ManufacturingQcParameterDto?> GetQcParameterByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<ManufacturingQcParameterDto>> CreateQcParameterAsync(string accessToken, ManufacturingQcParameterDto request, CancellationToken ct = default);
    Task<ApiCallResult<ManufacturingQcParameterDto>> UpdateQcParameterAsync(string accessToken, int id, ManufacturingQcParameterDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteQcParameterAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<ManufacturingProductionOutputReportDto>?> GetProductionOutputReportAsync(string accessToken, ManufacturingProductionOutputReportRequest request, CancellationToken ct = default);
    Task<PagedResult<ManufacturingOeeReportDto>?> GetOeeReportAsync(string accessToken, ManufacturingOeeReportRequest request, CancellationToken ct = default);
    Task<PagedResult<ManufacturingCostVarianceReportDto>?> GetCostVarianceReportAsync(string accessToken, ManufacturingCostVarianceReportRequest request, CancellationToken ct = default);
    Task<PagedResult<ManufacturingScrapAnalysisReportDto>?> GetScrapAnalysisReportAsync(string accessToken, ManufacturingScrapAnalysisReportRequest request, CancellationToken ct = default);
    Task<PagedResult<ManufacturingCapacityReportDto>?> GetCapacityReportAsync(string accessToken, ManufacturingCapacityReportRequest request, CancellationToken ct = default);
}
