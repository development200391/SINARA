using System.Globalization;
using ERP.Application.DTOs.Approval;
using ERP.Application.DTOs.Common;

namespace ERP.Web.Services;

public sealed class ApprovalApiClient(HttpClient httpClient, ILogger<ApprovalApiClient> logger) : ApiClientBase(httpClient, logger, "Approval"), IApprovalApiClient
{
    public Task<ApprovalDashboardDto?> GetDashboardAsync(string accessToken, CancellationToken ct = default)
        => SendWithResultAsync<ApprovalDashboardDto>(HttpMethod.Get, "api/v1/approval/dashboard", accessToken, null, ct).ToDataAsync();

    public Task<PagedResult<ApprovalInboxDto>?> GetInboxAsync(string accessToken, ApprovalInboxPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);
        AddTextParameter(parameters, "requestNo", request.RequestNo);
        AddTextParameter(parameters, "module", request.Module);
        AddTextParameter(parameters, "referenceType", request.ReferenceType);
        AddEnumParameter(parameters, "status", request.Status);
        AddDateParameter(parameters, "requestedDateFrom", request.RequestedDateFrom);
        AddDateParameter(parameters, "requestedDateTo", request.RequestedDateTo);
        AddBooleanParameter(parameters, "isOverdue", request.IsOverdue);

        return SendWithResultAsync<PagedResult<ApprovalInboxDto>>(HttpMethod.Get, $"api/v1/approval/inbox?{string.Join("&", parameters)}", accessToken, null, ct).ToDataAsync();
    }

    public Task<PagedResult<ApprovalRequestDto>?> GetMyRequestsAsync(string accessToken, ApprovalRequestPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);
        AddTextParameter(parameters, "requestNo", request.RequestNo);
        AddTextParameter(parameters, "module", request.Module);
        AddTextParameter(parameters, "referenceType", request.ReferenceType);
        AddEnumParameter(parameters, "status", request.Status);
        AddDateParameter(parameters, "requestedDateFrom", request.RequestedDateFrom);
        AddDateParameter(parameters, "requestedDateTo", request.RequestedDateTo);

        return SendWithResultAsync<PagedResult<ApprovalRequestDto>>(HttpMethod.Get, $"api/v1/approval/requests/my?{string.Join("&", parameters)}", accessToken, null, ct).ToDataAsync();
    }

    public Task<ApiCallResult<object?>> ApproveAsync(string accessToken, int requestId, TakeApprovalActionRequest request, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Post, $"api/v1/approval/requests/{requestId}/actions/approve", accessToken, request, ct);

    public Task<ApiCallResult<object?>> RejectAsync(string accessToken, int requestId, TakeApprovalActionRequest request, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Post, $"api/v1/approval/requests/{requestId}/actions/reject", accessToken, request, ct);

    public Task<ApiCallResult<object?>> CancelAsync(string accessToken, int requestId, string? note, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Put, $"api/v1/approval/requests/{requestId}/cancel", accessToken, new { notes = note }, ct);

    public Task<PagedResult<ApprovalTemplateDto>?> GetTemplatesAsync(string accessToken, ApprovalTemplatePagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);
        AddTextParameter(parameters, "code", request.Code);
        AddTextParameter(parameters, "name", request.Name);
        AddTextParameter(parameters, "module", request.Module);
        AddTextParameter(parameters, "referenceType", request.ReferenceType);
        AddEnumParameter(parameters, "approvalType", request.ApprovalType);
        AddDecimalParameter(parameters, "minAmountFrom", request.MinAmountFrom);
        AddDecimalParameter(parameters, "minAmountTo", request.MinAmountTo);
        AddDecimalParameter(parameters, "maxAmountFrom", request.MaxAmountFrom);
        AddDecimalParameter(parameters, "maxAmountTo", request.MaxAmountTo);
        AddBooleanParameter(parameters, "isActive", request.IsActive);

        return SendWithResultAsync<PagedResult<ApprovalTemplateDto>>(HttpMethod.Get, $"api/v1/approval/templates?{string.Join("&", parameters)}", accessToken, null, ct).ToDataAsync();
    }

    public async Task<IReadOnlyList<ApprovalOptionDto>> GetTemplateOptionsAsync(string accessToken, CancellationToken ct = default)
        => await SendWithResultAsync<IReadOnlyList<ApprovalOptionDto>>(HttpMethod.Get, "api/v1/approval/templates/options", accessToken, null, ct).ToDataAsync() ?? [];

    public Task<ApprovalTemplateDto?> GetTemplateByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<ApprovalTemplateDto>(HttpMethod.Get, $"api/v1/approval/templates/{id}", accessToken, null, ct).ToDataAsync();

    public Task<ApiCallResult<ApprovalTemplateDto>> CreateTemplateAsync(string accessToken, ApprovalTemplateDto request, CancellationToken ct = default)
        => SendWithResultAsync<ApprovalTemplateDto>(HttpMethod.Post, "api/v1/approval/templates", accessToken, request, ct);

    public Task<ApiCallResult<ApprovalTemplateDto>> UpdateTemplateAsync(string accessToken, int id, ApprovalTemplateDto request, CancellationToken ct = default)
        => SendWithResultAsync<ApprovalTemplateDto>(HttpMethod.Put, $"api/v1/approval/templates/{id}", accessToken, request, ct);

    public Task<ApiCallResult<object?>> SetTemplateActiveAsync(string accessToken, int id, bool isActive, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Put, $"api/v1/approval/templates/{id}/set-active", accessToken, new SetActiveRequest { IsActive = isActive }, ct);

    public Task<ApiCallResult<object?>> DeleteTemplateAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Delete, $"api/v1/approval/templates/{id}", accessToken, null, ct);

    public async Task<IReadOnlyList<ApprovalLevelDto>> GetLevelsAsync(string accessToken, int templateId, CancellationToken ct = default)
        => await SendWithResultAsync<IReadOnlyList<ApprovalLevelDto>>(HttpMethod.Get, $"api/v1/approval/templates/{templateId}/levels", accessToken, null, ct).ToDataAsync() ?? [];

    public Task<ApprovalLevelDto?> GetLevelByIdAsync(string accessToken, int templateId, int levelId, CancellationToken ct = default)
        => SendWithResultAsync<ApprovalLevelDto>(HttpMethod.Get, $"api/v1/approval/templates/{templateId}/levels/{levelId}", accessToken, null, ct).ToDataAsync();

    public Task<ApiCallResult<ApprovalLevelDto>> CreateLevelAsync(string accessToken, int templateId, ApprovalLevelDto request, CancellationToken ct = default)
        => SendWithResultAsync<ApprovalLevelDto>(HttpMethod.Post, $"api/v1/approval/templates/{templateId}/levels", accessToken, request, ct);

    public Task<ApiCallResult<ApprovalLevelDto>> UpdateLevelAsync(string accessToken, int templateId, int levelId, ApprovalLevelDto request, CancellationToken ct = default)
        => SendWithResultAsync<ApprovalLevelDto>(HttpMethod.Put, $"api/v1/approval/templates/{templateId}/levels/{levelId}", accessToken, request, ct);

    public Task<ApiCallResult<object?>> DeleteLevelAsync(string accessToken, int templateId, int levelId, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Delete, $"api/v1/approval/templates/{templateId}/levels/{levelId}", accessToken, null, ct);

    public Task<PagedResult<ApprovalDelegationDto>?> GetDelegationsAsync(string accessToken, ApprovalDelegationPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);
        AddIntParameter(parameters, "delegatorUserId", request.DelegatorUserId);
        AddIntParameter(parameters, "delegateUserId", request.DelegateUserId);
        AddIntParameter(parameters, "templateId", request.TemplateId);
        AddDateParameter(parameters, "effectiveDateFrom", request.EffectiveDateFrom);
        AddDateParameter(parameters, "effectiveDateTo", request.EffectiveDateTo);
        AddBooleanParameter(parameters, "isActive", request.IsActive);

        return SendWithResultAsync<PagedResult<ApprovalDelegationDto>>(HttpMethod.Get, $"api/v1/approval/delegations?{string.Join("&", parameters)}", accessToken, null, ct).ToDataAsync();
    }

    public Task<ApprovalDelegationDto?> GetDelegationByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<ApprovalDelegationDto>(HttpMethod.Get, $"api/v1/approval/delegations/{id}", accessToken, null, ct).ToDataAsync();

    public Task<ApiCallResult<ApprovalDelegationDto>> CreateDelegationAsync(string accessToken, ApprovalDelegationDto request, CancellationToken ct = default)
        => SendWithResultAsync<ApprovalDelegationDto>(HttpMethod.Post, "api/v1/approval/delegations", accessToken, request, ct);

    public Task<ApiCallResult<ApprovalDelegationDto>> UpdateDelegationAsync(string accessToken, int id, ApprovalDelegationDto request, CancellationToken ct = default)
        => SendWithResultAsync<ApprovalDelegationDto>(HttpMethod.Put, $"api/v1/approval/delegations/{id}", accessToken, request, ct);

    public Task<ApiCallResult<object?>> RevokeDelegationAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Put, $"api/v1/approval/delegations/{id}/revoke", accessToken, null, ct);

    public async Task<IReadOnlyList<ApprovalOptionDto>> GetApproverOptionsAsync(string accessToken, CancellationToken ct = default)
        => await SendWithResultAsync<IReadOnlyList<ApprovalOptionDto>>(HttpMethod.Get, "api/v1/approval/approvers/options", accessToken, null, ct).ToDataAsync() ?? [];

    public Task<PagedResult<ApprovalSlaReportDto>?> GetSlaReportAsync(string accessToken, ApprovalSlaReportPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);
        AddTextParameter(parameters, "module", request.Module);
        AddIntParameter(parameters, "templateId", request.TemplateId);
        AddDateParameter(parameters, "dateFrom", request.DateFrom);
        AddDateParameter(parameters, "dateTo", request.DateTo);

        return SendWithResultAsync<PagedResult<ApprovalSlaReportDto>>(HttpMethod.Get, $"api/v1/approval/reports/sla?{string.Join("&", parameters)}", accessToken, null, ct).ToDataAsync();
    }

    public Task<PagedResult<ApprovalTemplateReportDto>?> GetTemplateReportAsync(string accessToken, ApprovalTemplateReportPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);
        AddTextParameter(parameters, "module", request.Module);
        AddIntParameter(parameters, "templateId", request.TemplateId);
        AddDateParameter(parameters, "dateFrom", request.DateFrom);
        AddDateParameter(parameters, "dateTo", request.DateTo);

        return SendWithResultAsync<PagedResult<ApprovalTemplateReportDto>>(HttpMethod.Get, $"api/v1/approval/reports/by-template?{string.Join("&", parameters)}", accessToken, null, ct).ToDataAsync();
    }

    public Task<PagedResult<ApprovalAuditLogDto>?> GetAuditLogsAsync(string accessToken, ApprovalAuditPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);
        AddIntParameter(parameters, "requestId", request.RequestId);
        AddIntParameter(parameters, "actorUserId", request.ActorUserId);
        AddTextParameter(parameters, "action", request.Action);
        AddTextParameter(parameters, "module", request.Module);
        AddDateParameter(parameters, "dateFrom", request.DateFrom);
        AddDateParameter(parameters, "dateTo", request.DateTo);

        return SendWithResultAsync<PagedResult<ApprovalAuditLogDto>>(HttpMethod.Get, $"api/v1/approval/reports/audit?{string.Join("&", parameters)}", accessToken, null, ct).ToDataAsync();
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

    private static void AddIntParameter(List<string> parameters, string key, int? value)
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
            parameters.Add($"{key}={Uri.EscapeDataString(value.Value.ToString(CultureInfo.InvariantCulture))}");
        }
    }

    private static void AddBooleanParameter(List<string> parameters, string key, bool? value)
    {
        if (value.HasValue)
        {
            parameters.Add($"{key}={(value.Value ? "true" : "false")}");
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
            parameters.Add($"{key}={Convert.ToInt32(value.Value, CultureInfo.InvariantCulture)}");
        }
    }

    private sealed class SetActiveRequest
    {
        public bool IsActive { get; set; }
    }
}
