using ERP.Domain.Entities.Config;
using ERP.Domain.Entities.HR;
using ERP.Domain.Enums.Sales;

namespace ERP.Domain.Entities.Sales;

public sealed class SalApprovalConfig : BaseEntity
{
    public SalesDocumentType DocumentType { get; set; } = SalesDocumentType.SalesQuotation;
    public int Level { get; set; }
    public decimal MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public decimal? MaxDiscountPct { get; set; }
    public int? ApproverRoleId { get; set; }
    public int? ApproverEmployeeId { get; set; }
    public int TimeoutHours { get; set; } = 48;
    public bool AutoApproveIfTimeout { get; set; }
    public bool IsActive { get; set; } = true;

    public CfgRole? ApproverRole { get; set; }
    public HrEmployee? ApproverEmployee { get; set; }
}
