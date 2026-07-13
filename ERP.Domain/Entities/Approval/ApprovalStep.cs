using ERP.Domain.Entities.System;
using ERP.Domain.Enums.Approval;

namespace ERP.Domain.Entities.Approval;

public sealed class ApprovalStep : BaseEntity
{
    public int RequestId { get; set; }
    public int LevelId { get; set; }
    public int LevelOrder { get; set; }
    public int ApproverUserId { get; set; }
    public bool IsDelegated { get; set; }
    public int? DelegatedFromUserId { get; set; }
    public ApprovalStepAction? Action { get; set; }
    public DateTimeOffset? ActionAt { get; set; }
    public string? Comment { get; set; }
    public DateTimeOffset DueAt { get; set; }
    public DateTimeOffset? NotifiedAt { get; set; }
    public int ReminderCount { get; set; }
    public bool IsActive { get; set; } = true;

    public ApprovalRequest? Request { get; set; }
    public ApprovalLevel? Level { get; set; }
    public SysUser? ApproverUser { get; set; }
    public SysUser? DelegatedFromUser { get; set; }
}
