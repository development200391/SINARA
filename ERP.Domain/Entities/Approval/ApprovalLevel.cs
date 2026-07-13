using ERP.Domain.Entities.Config;
using ERP.Domain.Entities.HR;
using ERP.Domain.Entities.System;
using ERP.Domain.Enums.Approval;

namespace ERP.Domain.Entities.Approval;

public sealed class ApprovalLevel : BaseEntity
{
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

    public ApprovalTemplate? Template { get; set; }
    public CfgRole? ApproverRole { get; set; }
    public HrPosition? ApproverPosition { get; set; }
    public SysUser? ApproverUser { get; set; }
    public ApprovalLevel? EscalateToLevel { get; set; }
}
