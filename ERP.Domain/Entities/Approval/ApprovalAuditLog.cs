using ERP.Domain.Enums.Approval;

namespace ERP.Domain.Entities.Approval;

/// <summary>
/// Append-only audit trail — deliberately does NOT implement ISoftDelete/BaseEntity;
/// rows are never updated or deleted once written (see ReadMeGeneralApproval.md).
/// </summary>
public sealed class ApprovalAuditLog
{
    public long Id { get; set; }
    public int RequestId { get; set; }
    public int? StepId { get; set; }
    public int ActorUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public ApprovalRequestStatus? OldStatus { get; set; }
    public ApprovalRequestStatus? NewStatus { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Comment { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
