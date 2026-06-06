using ERP.Application.DTOs.Common;
using ERP.Domain.Enums.Approval;

namespace ERP.Application.DTOs.Approval;

public sealed class ApprovalOptionDto
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
}

public sealed class ApprovalDashboardDto
{
    public int WaitingMyActionCount { get; set; }
    public int MyOpenRequestCount { get; set; }
    public int OverdueStepCount { get; set; }
    public int ApprovedTodayCount { get; set; }
    public int RejectedTodayCount { get; set; }
    public int ActiveTemplateCount { get; set; }
    public int ActiveDelegationCount { get; set; }
}

public sealed class ApprovalTemplateDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string ReferenceType { get; set; } = string.Empty;
    public ApprovalType ApprovalType { get; set; } = ApprovalType.Sequential;
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public decimal? AutoApproveBelow { get; set; }
    public int SlaHours { get; set; } = 24;
    public bool AllowDelegation { get; set; } = true;
    public bool RequireCommentOnReject { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public int LevelCount { get; set; }
}

public sealed class ApprovalTemplatePagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Module { get; set; }
    public string? ReferenceType { get; set; }
    public ApprovalType? ApprovalType { get; set; }
    public decimal? MinAmountFrom { get; set; }
    public decimal? MinAmountTo { get; set; }
    public decimal? MaxAmountFrom { get; set; }
    public decimal? MaxAmountTo { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class ApprovalLevelDto
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public int LevelOrder { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public ApprovalApproverType ApproverType { get; set; } = ApprovalApproverType.Role;
    public int? ApproverRoleId { get; set; }
    public int? ApproverPositionId { get; set; }
    public int? ApproverUserId { get; set; }
    public int MinApproversRequired { get; set; } = 1;
    public int? EscalationHours { get; set; }
    public int? EscalateToLevelId { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class ApprovalRequestDto
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = string.Empty;
    public int TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string ReferenceType { get; set; } = string.Empty;
    public int ReferenceId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public int RequestedBy { get; set; }
    public string RequestedByName { get; set; } = string.Empty;
    public DateTimeOffset RequestedAt { get; set; }
    public int? CurrentLevelId { get; set; }
    public string? CurrentLevelName { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public ApprovalRequestStatus Status { get; set; } = ApprovalRequestStatus.Pending;
    public DateTimeOffset? FinalActionAt { get; set; }
    public int? FinalActionBy { get; set; }
    public string? FinalActionByName { get; set; }
    public string? Notes { get; set; }
}

public sealed class ApprovalInboxDto
{
    public int RequestId { get; set; }
    public int StepId { get; set; }
    public string RequestNo { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string ReferenceType { get; set; } = string.Empty;
    public int ReferenceId { get; set; }
    public string TemplateCode { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public int LevelOrder { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public string RequestedByName { get; set; } = string.Empty;
    public DateTimeOffset RequestedAt { get; set; }
    public DateTimeOffset DueAt { get; set; }
    public bool IsOverdue { get; set; }
    public ApprovalRequestStatus Status { get; set; } = ApprovalRequestStatus.Pending;
}

public sealed class ApprovalInboxPagedRequest : PagedRequest
{
    public string? RequestNo { get; set; }
    public string? Module { get; set; }
    public string? ReferenceType { get; set; }
    public ApprovalRequestStatus? Status { get; set; }
    public DateOnly? RequestedDateFrom { get; set; }
    public DateOnly? RequestedDateTo { get; set; }
    public bool? IsOverdue { get; set; }
}

public sealed class ApprovalRequestPagedRequest : PagedRequest
{
    public string? RequestNo { get; set; }
    public string? Module { get; set; }
    public string? ReferenceType { get; set; }
    public ApprovalRequestStatus? Status { get; set; }
    public DateOnly? RequestedDateFrom { get; set; }
    public DateOnly? RequestedDateTo { get; set; }
}

public sealed class TakeApprovalActionRequest
{
    public string? Comment { get; set; }
    public int? DelegateUserId { get; set; }
}

public sealed class ApprovalDelegationDto
{
    public int Id { get; set; }
    public int DelegatorUserId { get; set; }
    public string DelegatorName { get; set; } = string.Empty;
    public int DelegateUserId { get; set; }
    public string DelegateName { get; set; } = string.Empty;
    public int? TemplateId { get; set; }
    public string? TemplateCode { get; set; }
    public string? TemplateName { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Reason { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class ApprovalDelegationPagedRequest : PagedRequest
{
    public int? DelegatorUserId { get; set; }
    public int? DelegateUserId { get; set; }
    public int? TemplateId { get; set; }
    public DateOnly? EffectiveDateFrom { get; set; }
    public DateOnly? EffectiveDateTo { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class ApprovalSlaReportDto
{
    public string Module { get; set; } = string.Empty;
    public string TemplateCode { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public decimal SlaHours { get; set; }
    public decimal AverageResponseHours { get; set; }
    public int TotalSteps { get; set; }
    public int WithinSlaCount { get; set; }
    public int OverdueCount { get; set; }
    public decimal ComplianceRate { get; set; }
}

public sealed class ApprovalSlaReportPagedRequest : PagedRequest
{
    public string? Module { get; set; }
    public int? TemplateId { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
}

public sealed class ApprovalTemplateReportDto
{
    public int TemplateId { get; set; }
    public string TemplateCode { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public int TotalRequests { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public int CancelledCount { get; set; }
    public int PendingCount { get; set; }
    public decimal AverageDurationHours { get; set; }
}

public sealed class ApprovalTemplateReportPagedRequest : PagedRequest
{
    public string? Module { get; set; }
    public int? TemplateId { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
}

public sealed class ApprovalAuditLogDto
{
    public long Id { get; set; }
    public int RequestId { get; set; }
    public int? StepId { get; set; }
    public int ActorUserId { get; set; }
    public string ActorUserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public ApprovalRequestStatus? OldStatus { get; set; }
    public ApprovalRequestStatus? NewStatus { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Comment { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class ApprovalAuditPagedRequest : PagedRequest
{
    public int? RequestId { get; set; }
    public int? ActorUserId { get; set; }
    public string? Action { get; set; }
    public string? Module { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
}

public sealed class ApprovalNotificationDto
{
    public long Id { get; set; }
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
}

