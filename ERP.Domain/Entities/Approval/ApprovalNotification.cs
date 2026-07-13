using ERP.Domain.Entities.System;
using ERP.Domain.Enums.Approval;

namespace ERP.Domain.Entities.Approval;

public sealed class ApprovalNotification : BaseEntity
{
    public int RequestId { get; set; }
    public int? StepId { get; set; }
    public int RecipientUserId { get; set; }
    public ApprovalNotificationType NotificationType { get; set; } = ApprovalNotificationType.NewRequest;
    public ApprovalNotificationChannel Channel { get; set; } = ApprovalNotificationChannel.InApp;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public DateTimeOffset? FailedAt { get; set; }
    public int RetryCount { get; set; }

    public ApprovalRequest? Request { get; set; }
    public ApprovalStep? Step { get; set; }
    public SysUser? RecipientUser { get; set; }
}
