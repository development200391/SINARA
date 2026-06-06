using ERP.Application.DTOs.Approval;
using ERP.Application.DTOs.Common;

namespace ERP.Web.Services;

public interface IApprovalApiClient
{
    Task<ApprovalDashboardDto?> GetDashboardAsync(string accessToken, CancellationToken ct = default);

    Task<PagedResult<ApprovalInboxDto>?> GetInboxAsync(string accessToken, ApprovalInboxPagedRequest request, CancellationToken ct = default);
    Task<PagedResult<ApprovalRequestDto>?> GetMyRequestsAsync(string accessToken, ApprovalRequestPagedRequest request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> ApproveAsync(string accessToken, int requestId, TakeApprovalActionRequest request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> RejectAsync(string accessToken, int requestId, TakeApprovalActionRequest request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> CancelAsync(string accessToken, int requestId, string? note, CancellationToken ct = default);

    Task<PagedResult<ApprovalTemplateDto>?> GetTemplatesAsync(string accessToken, ApprovalTemplatePagedRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ApprovalOptionDto>> GetTemplateOptionsAsync(string accessToken, CancellationToken ct = default);
    Task<ApprovalTemplateDto?> GetTemplateByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<ApprovalTemplateDto>> CreateTemplateAsync(string accessToken, ApprovalTemplateDto request, CancellationToken ct = default);
    Task<ApiCallResult<ApprovalTemplateDto>> UpdateTemplateAsync(string accessToken, int id, ApprovalTemplateDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> SetTemplateActiveAsync(string accessToken, int id, bool isActive, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteTemplateAsync(string accessToken, int id, CancellationToken ct = default);

    Task<IReadOnlyList<ApprovalLevelDto>> GetLevelsAsync(string accessToken, int templateId, CancellationToken ct = default);
    Task<ApprovalLevelDto?> GetLevelByIdAsync(string accessToken, int templateId, int levelId, CancellationToken ct = default);
    Task<ApiCallResult<ApprovalLevelDto>> CreateLevelAsync(string accessToken, int templateId, ApprovalLevelDto request, CancellationToken ct = default);
    Task<ApiCallResult<ApprovalLevelDto>> UpdateLevelAsync(string accessToken, int templateId, int levelId, ApprovalLevelDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteLevelAsync(string accessToken, int templateId, int levelId, CancellationToken ct = default);

    Task<PagedResult<ApprovalDelegationDto>?> GetDelegationsAsync(string accessToken, ApprovalDelegationPagedRequest request, CancellationToken ct = default);
    Task<ApprovalDelegationDto?> GetDelegationByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<ApprovalDelegationDto>> CreateDelegationAsync(string accessToken, ApprovalDelegationDto request, CancellationToken ct = default);
    Task<ApiCallResult<ApprovalDelegationDto>> UpdateDelegationAsync(string accessToken, int id, ApprovalDelegationDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> RevokeDelegationAsync(string accessToken, int id, CancellationToken ct = default);

    Task<IReadOnlyList<ApprovalOptionDto>> GetApproverOptionsAsync(string accessToken, CancellationToken ct = default);

    Task<PagedResult<ApprovalSlaReportDto>?> GetSlaReportAsync(string accessToken, ApprovalSlaReportPagedRequest request, CancellationToken ct = default);
    Task<PagedResult<ApprovalTemplateReportDto>?> GetTemplateReportAsync(string accessToken, ApprovalTemplateReportPagedRequest request, CancellationToken ct = default);
    Task<PagedResult<ApprovalAuditLogDto>?> GetAuditLogsAsync(string accessToken, ApprovalAuditPagedRequest request, CancellationToken ct = default);
}
