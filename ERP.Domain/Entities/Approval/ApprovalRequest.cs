using ERP.Domain.Entities.System;
using ERP.Domain.Enums.Approval;

namespace ERP.Domain.Entities.Approval;

public sealed class ApprovalRequest : BaseEntity
{
    public string RequestNo { get; set; } = string.Empty;
    public int TemplateId { get; set; }
    public string Module { get; set; } = string.Empty;
    public string ReferenceType { get; set; } = string.Empty;
    public int ReferenceId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public int RequestedBy { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
    public int? CurrentLevelId { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public ApprovalRequestStatus Status { get; set; } = ApprovalRequestStatus.Pending;
    public DateTimeOffset? FinalActionAt { get; set; }
    public int? FinalActionBy { get; set; }
    public string? Notes { get; set; }

    public ApprovalTemplate? Template { get; set; }
    public ApprovalLevel? CurrentLevel { get; set; }
    public SysUser? RequestedByUser { get; set; }
    public SysUser? FinalActionByUser { get; set; }
    public ICollection<ApprovalStep> Steps { get; set; } = new List<ApprovalStep>();
    public ICollection<ApprovalNotification> Notifications { get; set; } = new List<ApprovalNotification>();
}
