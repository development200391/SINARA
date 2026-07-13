using ERP.Domain.Enums.Approval;

namespace ERP.Domain.Entities.Approval;

public sealed class ApprovalTemplate : BaseEntity
{
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

    public ICollection<ApprovalLevel> Levels { get; set; } = new List<ApprovalLevel>();
    public ICollection<ApprovalRequest> Requests { get; set; } = new List<ApprovalRequest>();
    public ICollection<ApprovalDelegation> Delegations { get; set; } = new List<ApprovalDelegation>();
}
