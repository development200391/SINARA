using ERP.Application.DTOs.Approval;
using ERP.Application.DTOs.Common;

namespace ERP.Application.Services.Approval;

public interface IApprovalRequestService
{
    /// <summary>
    /// Called by a source module to submit a document for approval. Resolves the matching active
    /// template by ReferenceType + Amount, auto-approves if below the template's threshold, otherwise
    /// creates the request and activates level 1.
    /// </summary>
    Task<ApprovalRequestDto> SubmitAsync(
        string module,
        string referenceType,
        int referenceId,
        string subject,
        decimal? amount,
        int requestedByUserId,
        string? notes,
        CancellationToken ct = default);

    Task<PagedResult<ApprovalInboxDto>> GetInboxPagedAsync(int userId, ApprovalInboxPagedRequest request, CancellationToken ct = default);

    Task<PagedResult<ApprovalRequestDto>> GetMyRequestsPagedAsync(int userId, ApprovalRequestPagedRequest request, CancellationToken ct = default);

    Task<ApprovalRequestDto> ApproveAsync(int requestId, int actorUserId, TakeApprovalActionRequest request, CancellationToken ct = default);

    Task<ApprovalRequestDto> RejectAsync(int requestId, int actorUserId, TakeApprovalActionRequest request, CancellationToken ct = default);

    Task<ApprovalRequestDto> CancelAsync(int requestId, int actorUserId, string? notes, CancellationToken ct = default);

    /// <summary>
    /// Looks up the currently open (Pending/InProgress) approval request for a source record, if any.
    /// Lets a source module's own approve/reject action delegate into the APV engine when one exists.
    /// </summary>
    Task<int?> FindActiveRequestIdAsync(string referenceType, int referenceId, CancellationToken ct = default);

    /// <summary>
    /// Invoked by the Hangfire recurring job every 30 minutes: sends SLA reminders, escalates overdue
    /// steps to their configured EscalateToLevelId (if any), and alerts on unescalatable overdue steps.
    /// </summary>
    Task ProcessEscalationsAndRemindersAsync(CancellationToken ct = default);
}
