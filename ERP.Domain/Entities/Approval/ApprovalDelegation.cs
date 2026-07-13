using ERP.Domain.Entities.System;

namespace ERP.Domain.Entities.Approval;

public sealed class ApprovalDelegation : BaseEntity
{
    public int DelegatorUserId { get; set; }
    public int DelegateUserId { get; set; }
    public int? TemplateId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Reason { get; set; }
    public bool IsActive { get; set; } = true;

    public SysUser? DelegatorUser { get; set; }
    public SysUser? DelegateUser { get; set; }
    public ApprovalTemplate? Template { get; set; }
}
