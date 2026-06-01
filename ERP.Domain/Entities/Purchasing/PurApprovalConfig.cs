using ERP.Domain.Entities.HR;
using ERP.Domain.Enums.Purchasing;

namespace ERP.Domain.Entities.Purchasing;

public sealed class PurApprovalConfig : BaseEntity
{
    public PurchasingDocumentType DocumentType { get; set; } = PurchasingDocumentType.PurchaseRequisition;
    public int Level { get; set; }
    public decimal MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public int ApproverEmployeeId { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public HrEmployee ApproverEmployee { get; set; } = null!;
}
