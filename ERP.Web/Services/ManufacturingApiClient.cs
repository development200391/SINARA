using System.Globalization;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Manufacturing;

namespace ERP.Web.Services;

public sealed class ManufacturingApiClient(HttpClient httpClient, ILogger<ManufacturingApiClient> logger) : ApiClientBase(httpClient, logger, "Manufacturing"), IManufacturingApiClient
{
    public Task<ManufacturingDashboardDto?> GetDashboardAsync(string accessToken, CancellationToken ct = default)
        => SendWithResultAsync<ManufacturingDashboardDto>(HttpMethod.Get, "api/v1/manufacturing/dashboard", accessToken, null, ct).ToDataAsync();

    public Task<PagedResult<ManufacturingWorkOrderDto>?> GetWorkOrdersAsync(string accessToken, ManufacturingWorkOrderPagedRequest request, CancellationToken ct = default)
    {
        var parameters = CreatePagedParameters(request);
        AddStringParameter(parameters, "code", request.Code);
        AddIntParameter(parameters, "itemId", request.ItemId);
        AddIntParameter(parameters, "workCenterId", request.WorkCenterId);
        AddEnumParameter(parameters, "status", request.Status);
        AddEnumParameter(parameters, "productionType", request.ProductionType);
        AddDateOnlyParameter(parameters, "plannedStartFrom", request.PlannedStartFrom);
        AddDateOnlyParameter(parameters, "plannedStartTo", request.PlannedStartTo);
        AddBoolParameter(parameters, "isActive", request.IsActive);

        return GetPagedAsync<ManufacturingWorkOrderDto>("api/v1/manufacturing/work-orders", accessToken, parameters, ct);
    }

    public Task<ManufacturingWorkOrderDto?> GetWorkOrderByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<ManufacturingWorkOrderDto>(HttpMethod.Get, $"api/v1/manufacturing/work-orders/{id}", accessToken, null, ct).ToDataAsync();

    public Task<ApiCallResult<ManufacturingWorkOrderDto>> CreateWorkOrderAsync(string accessToken, ManufacturingWorkOrderDto request, CancellationToken ct = default)
        => SendWithResultAsync<ManufacturingWorkOrderDto>(HttpMethod.Post, "api/v1/manufacturing/work-orders", accessToken, request, ct);

    public Task<ApiCallResult<ManufacturingWorkOrderDto>> UpdateWorkOrderAsync(string accessToken, int id, ManufacturingWorkOrderDto request, CancellationToken ct = default)
        => SendWithResultAsync<ManufacturingWorkOrderDto>(HttpMethod.Put, $"api/v1/manufacturing/work-orders/{id}", accessToken, request, ct);

    public Task<ApiCallResult<object?>> DeleteWorkOrderAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Delete, $"api/v1/manufacturing/work-orders/{id}", accessToken, null, ct);

    public Task<ApiCallResult<object?>> ReleaseWorkOrderAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Post, $"api/v1/manufacturing/work-orders/{id}/release", accessToken, null, ct);

    public Task<ApiCallResult<object?>> StartWorkOrderAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Post, $"api/v1/manufacturing/work-orders/{id}/start", accessToken, null, ct);

    public Task<ApiCallResult<object?>> CompleteWorkOrderAsync(string accessToken, int id, ManufacturingWorkOrderCompleteRequest? request = null, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Post, $"api/v1/manufacturing/work-orders/{id}/complete", accessToken, request, ct);

    public Task<ApiCallResult<object?>> CloseWorkOrderAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Post, $"api/v1/manufacturing/work-orders/{id}/close", accessToken, null, ct);

    public Task<ApiCallResult<object?>> CancelWorkOrderAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Post, $"api/v1/manufacturing/work-orders/{id}/cancel", accessToken, null, ct);

    public Task<PagedResult<ManufacturingMrpRunDto>?> GetMrpRunsAsync(string accessToken, ManufacturingMrpRunPagedRequest request, CancellationToken ct = default)
    {
        var parameters = CreatePagedParameters(request);
        AddStringParameter(parameters, "code", request.Code);
        AddEnumParameter(parameters, "status", request.Status);
        AddDateOnlyParameter(parameters, "runDateFrom", request.RunDateFrom);
        AddDateOnlyParameter(parameters, "runDateTo", request.RunDateTo);

        return GetPagedAsync<ManufacturingMrpRunDto>("api/v1/manufacturing/mrp", accessToken, parameters, ct);
    }

    public Task<ManufacturingMrpRunDto?> GetMrpRunByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<ManufacturingMrpRunDto>(HttpMethod.Get, $"api/v1/manufacturing/mrp/{id}", accessToken, null, ct).ToDataAsync();

    public Task<ApiCallResult<ManufacturingMrpRunDto>> CreateMrpRunAsync(string accessToken, ManufacturingMrpRunDto request, CancellationToken ct = default)
        => SendWithResultAsync<ManufacturingMrpRunDto>(HttpMethod.Post, "api/v1/manufacturing/mrp", accessToken, request, ct);

    public Task<ApiCallResult<ManufacturingMrpRunDto>> UpdateMrpRunAsync(string accessToken, int id, ManufacturingMrpRunDto request, CancellationToken ct = default)
        => SendWithResultAsync<ManufacturingMrpRunDto>(HttpMethod.Put, $"api/v1/manufacturing/mrp/{id}", accessToken, request, ct);

    public Task<ApiCallResult<object?>> DeleteMrpRunAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Delete, $"api/v1/manufacturing/mrp/{id}", accessToken, null, ct);

    public Task<ApiCallResult<object?>> RunMrpAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Post, $"api/v1/manufacturing/mrp/{id}/run", accessToken, null, ct);

    public Task<ApiCallResult<object?>> CompleteMrpAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Post, $"api/v1/manufacturing/mrp/{id}/complete", accessToken, null, ct);

    public Task<ApiCallResult<object?>> CancelMrpAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Post, $"api/v1/manufacturing/mrp/{id}/cancel", accessToken, null, ct);

    public Task<PagedResult<ManufacturingQcInspectionDto>?> GetQcInspectionsAsync(string accessToken, ManufacturingQcInspectionPagedRequest request, CancellationToken ct = default)
    {
        var parameters = CreatePagedParameters(request);
        AddStringParameter(parameters, "code", request.Code);
        AddIntParameter(parameters, "workOrderId", request.WorkOrderId);
        AddIntParameter(parameters, "itemId", request.ItemId);
        AddIntParameter(parameters, "inspectorEmployeeId", request.InspectorEmployeeId);
        AddEnumParameter(parameters, "status", request.Status);
        AddEnumParameter(parameters, "result", request.Result);
        AddDateTimeOffsetParameter(parameters, "inspectedFrom", request.InspectedFrom);
        AddDateTimeOffsetParameter(parameters, "inspectedTo", request.InspectedTo);

        return GetPagedAsync<ManufacturingQcInspectionDto>("api/v1/manufacturing/qc", accessToken, parameters, ct);
    }

    public Task<ManufacturingQcInspectionDto?> GetQcInspectionByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<ManufacturingQcInspectionDto>(HttpMethod.Get, $"api/v1/manufacturing/qc/{id}", accessToken, null, ct).ToDataAsync();

    public Task<ApiCallResult<ManufacturingQcInspectionDto>> CreateQcInspectionAsync(string accessToken, ManufacturingQcInspectionDto request, CancellationToken ct = default)
        => SendWithResultAsync<ManufacturingQcInspectionDto>(HttpMethod.Post, "api/v1/manufacturing/qc", accessToken, request, ct);

    public Task<ApiCallResult<ManufacturingQcInspectionDto>> UpdateQcInspectionAsync(string accessToken, int id, ManufacturingQcInspectionDto request, CancellationToken ct = default)
        => SendWithResultAsync<ManufacturingQcInspectionDto>(HttpMethod.Put, $"api/v1/manufacturing/qc/{id}", accessToken, request, ct);

    public Task<ApiCallResult<object?>> DeleteQcInspectionAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Delete, $"api/v1/manufacturing/qc/{id}", accessToken, null, ct);

    public Task<ApiCallResult<object?>> StartQcInspectionAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Post, $"api/v1/manufacturing/qc/{id}/start", accessToken, null, ct);

    public Task<ApiCallResult<object?>> CompleteQcInspectionAsync(string accessToken, int id, ManufacturingQcCompleteRequest request, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Post, $"api/v1/manufacturing/qc/{id}/complete", accessToken, request, ct);

    public Task<ApiCallResult<object?>> CancelQcInspectionAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Post, $"api/v1/manufacturing/qc/{id}/cancel", accessToken, null, ct);

    public Task<PagedResult<ManufacturingScrapRecordDto>?> GetScrapRecordsAsync(string accessToken, ManufacturingScrapRecordPagedRequest request, CancellationToken ct = default)
    {
        var parameters = CreatePagedParameters(request);
        AddStringParameter(parameters, "code", request.Code);
        AddIntParameter(parameters, "workOrderId", request.WorkOrderId);
        AddIntParameter(parameters, "itemId", request.ItemId);
        AddIntParameter(parameters, "workCenterId", request.WorkCenterId);
        AddEnumParameter(parameters, "reason", request.Reason);
        AddDateTimeOffsetParameter(parameters, "recordedFrom", request.RecordedFrom);
        AddDateTimeOffsetParameter(parameters, "recordedTo", request.RecordedTo);

        return GetPagedAsync<ManufacturingScrapRecordDto>("api/v1/manufacturing/scrap", accessToken, parameters, ct);
    }

    public Task<ManufacturingScrapRecordDto?> GetScrapRecordByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<ManufacturingScrapRecordDto>(HttpMethod.Get, $"api/v1/manufacturing/scrap/{id}", accessToken, null, ct).ToDataAsync();

    public Task<ApiCallResult<ManufacturingScrapRecordDto>> CreateScrapRecordAsync(string accessToken, ManufacturingScrapRecordDto request, CancellationToken ct = default)
        => SendWithResultAsync<ManufacturingScrapRecordDto>(HttpMethod.Post, "api/v1/manufacturing/scrap", accessToken, request, ct);

    public Task<ApiCallResult<ManufacturingScrapRecordDto>> UpdateScrapRecordAsync(string accessToken, int id, ManufacturingScrapRecordDto request, CancellationToken ct = default)
        => SendWithResultAsync<ManufacturingScrapRecordDto>(HttpMethod.Put, $"api/v1/manufacturing/scrap/{id}", accessToken, request, ct);

    public Task<ApiCallResult<object?>> DeleteScrapRecordAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Delete, $"api/v1/manufacturing/scrap/{id}", accessToken, null, ct);

    public Task<PagedResult<ManufacturingReworkOrderDto>?> GetReworkOrdersAsync(string accessToken, ManufacturingReworkOrderPagedRequest request, CancellationToken ct = default)
    {
        var parameters = CreatePagedParameters(request);
        AddStringParameter(parameters, "code", request.Code);
        AddIntParameter(parameters, "itemId", request.ItemId);
        AddEnumParameter(parameters, "status", request.Status);
        AddDateTimeOffsetParameter(parameters, "openedFrom", request.OpenedFrom);
        AddDateTimeOffsetParameter(parameters, "openedTo", request.OpenedTo);
        AddDateTimeOffsetParameter(parameters, "closedFrom", request.ClosedFrom);
        AddDateTimeOffsetParameter(parameters, "closedTo", request.ClosedTo);

        return GetPagedAsync<ManufacturingReworkOrderDto>("api/v1/manufacturing/rework", accessToken, parameters, ct);
    }

    public Task<ManufacturingReworkOrderDto?> GetReworkOrderByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<ManufacturingReworkOrderDto>(HttpMethod.Get, $"api/v1/manufacturing/rework/{id}", accessToken, null, ct).ToDataAsync();

    public Task<ApiCallResult<ManufacturingReworkOrderDto>> CreateReworkOrderAsync(string accessToken, ManufacturingReworkOrderDto request, CancellationToken ct = default)
        => SendWithResultAsync<ManufacturingReworkOrderDto>(HttpMethod.Post, "api/v1/manufacturing/rework", accessToken, request, ct);

    public Task<ApiCallResult<ManufacturingReworkOrderDto>> UpdateReworkOrderAsync(string accessToken, int id, ManufacturingReworkOrderDto request, CancellationToken ct = default)
        => SendWithResultAsync<ManufacturingReworkOrderDto>(HttpMethod.Put, $"api/v1/manufacturing/rework/{id}", accessToken, request, ct);

    public Task<ApiCallResult<object?>> DeleteReworkOrderAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Delete, $"api/v1/manufacturing/rework/{id}", accessToken, null, ct);

    public Task<ApiCallResult<object?>> StartReworkOrderAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Post, $"api/v1/manufacturing/rework/{id}/start", accessToken, null, ct);

    public Task<ApiCallResult<object?>> CompleteReworkOrderAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Post, $"api/v1/manufacturing/rework/{id}/complete", accessToken, null, ct);

    public Task<ApiCallResult<object?>> CloseReworkOrderAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Post, $"api/v1/manufacturing/rework/{id}/close", accessToken, null, ct);

    public Task<ApiCallResult<object?>> CancelReworkOrderAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Post, $"api/v1/manufacturing/rework/{id}/cancel", accessToken, null, ct);

    public Task<PagedResult<ManufacturingBomDto>?> GetBomsAsync(string accessToken, ManufacturingBomPagedRequest request, CancellationToken ct = default)
    {
        var parameters = CreatePagedParameters(request);
        AddStringParameter(parameters, "code", request.Code);
        AddIntParameter(parameters, "itemId", request.ItemId);
        AddIntParameter(parameters, "routingId", request.RoutingId);
        AddEnumParameter(parameters, "status", request.Status);
        AddDateOnlyParameter(parameters, "effectiveDateFrom", request.EffectiveDateFrom);
        AddDateOnlyParameter(parameters, "effectiveDateTo", request.EffectiveDateTo);
        AddBoolParameter(parameters, "isActive", request.IsActive);

        return GetPagedAsync<ManufacturingBomDto>("api/v1/manufacturing/boms", accessToken, parameters, ct);
    }

    public Task<ManufacturingBomDto?> GetBomByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<ManufacturingBomDto>(HttpMethod.Get, $"api/v1/manufacturing/boms/{id}", accessToken, null, ct).ToDataAsync();

    public Task<ApiCallResult<ManufacturingBomDto>> CreateBomAsync(string accessToken, ManufacturingBomDto request, CancellationToken ct = default)
        => SendWithResultAsync<ManufacturingBomDto>(HttpMethod.Post, "api/v1/manufacturing/boms", accessToken, request, ct);

    public Task<ApiCallResult<ManufacturingBomDto>> UpdateBomAsync(string accessToken, int id, ManufacturingBomDto request, CancellationToken ct = default)
        => SendWithResultAsync<ManufacturingBomDto>(HttpMethod.Put, $"api/v1/manufacturing/boms/{id}", accessToken, request, ct);

    public Task<ApiCallResult<object?>> DeleteBomAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Delete, $"api/v1/manufacturing/boms/{id}", accessToken, null, ct);

    public Task<PagedResult<ManufacturingRoutingDto>?> GetRoutingsAsync(string accessToken, ManufacturingRoutingPagedRequest request, CancellationToken ct = default)
    {
        var parameters = CreatePagedParameters(request);
        AddStringParameter(parameters, "code", request.Code);
        AddStringParameter(parameters, "name", request.Name);
        AddIntParameter(parameters, "itemId", request.ItemId);
        AddIntParameter(parameters, "workCenterId", request.WorkCenterId);
        AddEnumParameter(parameters, "status", request.Status);
        AddBoolParameter(parameters, "isActive", request.IsActive);

        return GetPagedAsync<ManufacturingRoutingDto>("api/v1/manufacturing/routings", accessToken, parameters, ct);
    }

    public Task<ManufacturingRoutingDto?> GetRoutingByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<ManufacturingRoutingDto>(HttpMethod.Get, $"api/v1/manufacturing/routings/{id}", accessToken, null, ct).ToDataAsync();

    public Task<ApiCallResult<ManufacturingRoutingDto>> CreateRoutingAsync(string accessToken, ManufacturingRoutingDto request, CancellationToken ct = default)
        => SendWithResultAsync<ManufacturingRoutingDto>(HttpMethod.Post, "api/v1/manufacturing/routings", accessToken, request, ct);

    public Task<ApiCallResult<ManufacturingRoutingDto>> UpdateRoutingAsync(string accessToken, int id, ManufacturingRoutingDto request, CancellationToken ct = default)
        => SendWithResultAsync<ManufacturingRoutingDto>(HttpMethod.Put, $"api/v1/manufacturing/routings/{id}", accessToken, request, ct);

    public Task<ApiCallResult<object?>> DeleteRoutingAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Delete, $"api/v1/manufacturing/routings/{id}", accessToken, null, ct);

    public Task<PagedResult<ManufacturingWorkCenterDto>?> GetWorkCentersAsync(string accessToken, ManufacturingWorkCenterPagedRequest request, CancellationToken ct = default)
    {
        var parameters = CreatePagedParameters(request);
        AddStringParameter(parameters, "code", request.Code);
        AddStringParameter(parameters, "name", request.Name);
        AddBoolParameter(parameters, "isActive", request.IsActive);

        return GetPagedAsync<ManufacturingWorkCenterDto>("api/v1/manufacturing/work-centers", accessToken, parameters, ct);
    }

    public Task<ManufacturingWorkCenterDto?> GetWorkCenterByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<ManufacturingWorkCenterDto>(HttpMethod.Get, $"api/v1/manufacturing/work-centers/{id}", accessToken, null, ct).ToDataAsync();

    public Task<ApiCallResult<ManufacturingWorkCenterDto>> CreateWorkCenterAsync(string accessToken, ManufacturingWorkCenterDto request, CancellationToken ct = default)
        => SendWithResultAsync<ManufacturingWorkCenterDto>(HttpMethod.Post, "api/v1/manufacturing/work-centers", accessToken, request, ct);

    public Task<ApiCallResult<ManufacturingWorkCenterDto>> UpdateWorkCenterAsync(string accessToken, int id, ManufacturingWorkCenterDto request, CancellationToken ct = default)
        => SendWithResultAsync<ManufacturingWorkCenterDto>(HttpMethod.Put, $"api/v1/manufacturing/work-centers/{id}", accessToken, request, ct);

    public Task<ApiCallResult<object?>> DeleteWorkCenterAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Delete, $"api/v1/manufacturing/work-centers/{id}", accessToken, null, ct);

    public Task<PagedResult<ManufacturingQcParameterDto>?> GetQcParametersAsync(string accessToken, ManufacturingQcParameterPagedRequest request, CancellationToken ct = default)
    {
        var parameters = CreatePagedParameters(request);
        AddStringParameter(parameters, "code", request.Code);
        AddStringParameter(parameters, "name", request.Name);
        AddIntParameter(parameters, "itemId", request.ItemId);
        AddEnumParameter(parameters, "parameterType", request.ParameterType);
        AddBoolParameter(parameters, "isCritical", request.IsCritical);
        AddBoolParameter(parameters, "isActive", request.IsActive);

        return GetPagedAsync<ManufacturingQcParameterDto>("api/v1/manufacturing/qc/parameters", accessToken, parameters, ct);
    }

    public Task<ManufacturingQcParameterDto?> GetQcParameterByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<ManufacturingQcParameterDto>(HttpMethod.Get, $"api/v1/manufacturing/qc/parameters/{id}", accessToken, null, ct).ToDataAsync();

    public Task<ApiCallResult<ManufacturingQcParameterDto>> CreateQcParameterAsync(string accessToken, ManufacturingQcParameterDto request, CancellationToken ct = default)
        => SendWithResultAsync<ManufacturingQcParameterDto>(HttpMethod.Post, "api/v1/manufacturing/qc/parameters", accessToken, request, ct);

    public Task<ApiCallResult<ManufacturingQcParameterDto>> UpdateQcParameterAsync(string accessToken, int id, ManufacturingQcParameterDto request, CancellationToken ct = default)
        => SendWithResultAsync<ManufacturingQcParameterDto>(HttpMethod.Put, $"api/v1/manufacturing/qc/parameters/{id}", accessToken, request, ct);

    public Task<ApiCallResult<object?>> DeleteQcParameterAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Delete, $"api/v1/manufacturing/qc/parameters/{id}", accessToken, null, ct);

    public Task<PagedResult<ManufacturingProductionOutputReportDto>?> GetProductionOutputReportAsync(string accessToken, ManufacturingProductionOutputReportRequest request, CancellationToken ct = default)
    {
        var parameters = CreatePagedParameters(request);
        AddEnumParameter(parameters, "status", request.Status);
        AddIntParameter(parameters, "workCenterId", request.WorkCenterId);
        AddDateOnlyParameter(parameters, "plannedStartFrom", request.PlannedStartFrom);
        AddDateOnlyParameter(parameters, "plannedStartTo", request.PlannedStartTo);

        return GetPagedAsync<ManufacturingProductionOutputReportDto>("api/v1/manufacturing/reports/production-output", accessToken, parameters, ct);
    }

    public Task<PagedResult<ManufacturingOeeReportDto>?> GetOeeReportAsync(string accessToken, ManufacturingOeeReportRequest request, CancellationToken ct = default)
    {
        var parameters = CreatePagedParameters(request);
        AddIntParameter(parameters, "workCenterId", request.WorkCenterId);
        AddDateOnlyParameter(parameters, "snapshotDateFrom", request.SnapshotDateFrom);
        AddDateOnlyParameter(parameters, "snapshotDateTo", request.SnapshotDateTo);

        return GetPagedAsync<ManufacturingOeeReportDto>("api/v1/manufacturing/reports/oee", accessToken, parameters, ct);
    }

    public Task<PagedResult<ManufacturingCostVarianceReportDto>?> GetCostVarianceReportAsync(string accessToken, ManufacturingCostVarianceReportRequest request, CancellationToken ct = default)
    {
        var parameters = CreatePagedParameters(request);
        AddEnumParameter(parameters, "status", request.Status);
        AddIntParameter(parameters, "workCenterId", request.WorkCenterId);

        return GetPagedAsync<ManufacturingCostVarianceReportDto>("api/v1/manufacturing/reports/cost-variance", accessToken, parameters, ct);
    }

    public Task<PagedResult<ManufacturingScrapAnalysisReportDto>?> GetScrapAnalysisReportAsync(string accessToken, ManufacturingScrapAnalysisReportRequest request, CancellationToken ct = default)
    {
        var parameters = CreatePagedParameters(request);
        AddEnumParameter(parameters, "reason", request.Reason);
        AddDateTimeOffsetParameter(parameters, "recordedFrom", request.RecordedFrom);
        AddDateTimeOffsetParameter(parameters, "recordedTo", request.RecordedTo);

        return GetPagedAsync<ManufacturingScrapAnalysisReportDto>("api/v1/manufacturing/reports/scrap-analysis", accessToken, parameters, ct);
    }

    public Task<PagedResult<ManufacturingCapacityReportDto>?> GetCapacityReportAsync(string accessToken, ManufacturingCapacityReportRequest request, CancellationToken ct = default)
    {
        var parameters = CreatePagedParameters(request);
        AddIntParameter(parameters, "workCenterId", request.WorkCenterId);
        AddDateOnlyParameter(parameters, "plannedStartFrom", request.PlannedStartFrom);
        AddDateOnlyParameter(parameters, "plannedStartTo", request.PlannedStartTo);

        return GetPagedAsync<ManufacturingCapacityReportDto>("api/v1/manufacturing/reports/capacity", accessToken, parameters, ct);
    }

    private Task<PagedResult<T>?> GetPagedAsync<T>(string path, string accessToken, List<string> parameters, CancellationToken ct)
    {
        var query = string.Join("&", parameters);
        return SendWithResultAsync<PagedResult<T>>(HttpMethod.Get, $"{path}?{query}", accessToken, null, ct).ToDataAsync();
    }

    private static List<string> CreatePagedParameters(PagedRequest request)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);
        return parameters;
    }

    private static void AddPagedParameters(List<string> parameters, PagedRequest request)
    {
        parameters.Add($"page={request.Page}");
        parameters.Add($"pageSize={request.PageSize}");
        parameters.Add($"search={Uri.EscapeDataString(request.Search ?? string.Empty)}");
        parameters.Add($"sortBy={Uri.EscapeDataString(request.SortBy ?? string.Empty)}");
        parameters.Add($"sortDirection={Uri.EscapeDataString(request.SortDirection ?? string.Empty)}");
    }

    private static void AddStringParameter(List<string> parameters, string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        parameters.Add($"{key}={Uri.EscapeDataString(value.Trim())}");
    }

    private static void AddIntParameter(List<string> parameters, string key, int? value)
    {
        if (!value.HasValue)
        {
            return;
        }

        parameters.Add($"{key}={value.Value}");
    }

    private static void AddBoolParameter(List<string> parameters, string key, bool? value)
    {
        if (!value.HasValue)
        {
            return;
        }

        parameters.Add($"{key}={(value.Value ? "true" : "false")}");
    }

    private static void AddEnumParameter<TEnum>(List<string> parameters, string key, TEnum? value)
        where TEnum : struct, Enum
    {
        if (!value.HasValue)
        {
            return;
        }

        parameters.Add($"{key}={Convert.ToInt32(value.Value, CultureInfo.InvariantCulture)}");
    }

    private static void AddDateOnlyParameter(List<string> parameters, string key, DateOnly? value)
    {
        if (!value.HasValue)
        {
            return;
        }

        parameters.Add($"{key}={value.Value:yyyy-MM-dd}");
    }

    private static void AddDateTimeOffsetParameter(List<string> parameters, string key, DateTimeOffset? value)
    {
        if (!value.HasValue)
        {
            return;
        }

        parameters.Add($"{key}={Uri.EscapeDataString(value.Value.ToString("o", CultureInfo.InvariantCulture))}");
    }
}
