using ERP.Domain.Enums.Approval;

namespace ERP.Application.Services.Approval;

/// <summary>
/// Records an apv_notifications row and dispatches it over the recipient's preferred channel
/// (in-app via SignalR and/or email via SMTP). Concrete implementation lives in ERP.API since it
/// needs host-specific infra (IHubContext, SMTP client).
/// </summary>
public interface IApprovalNotificationService
{
    Task NotifyAsync(
        int requestId,
        int? stepId,
        int recipientUserId,
        ApprovalNotificationType type,
        string subject,
        string body,
        CancellationToken ct = default);
}
