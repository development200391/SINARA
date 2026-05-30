using ERP.Domain.Entities.HR;

namespace ERP.Domain.Entities.Finance;

public sealed class FinCostCenter : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public int? ManagerId { get; set; }
    public int? BudgetAccountId { get; set; }
    public bool IsActive { get; set; } = true;

    public HrDepartment? Department { get; set; }
    public HrEmployee? Manager { get; set; }
    public FinAccount? BudgetAccount { get; set; }
    public ICollection<FinJournalEntryLine> JournalLines { get; set; } = new List<FinJournalEntryLine>();
    public ICollection<FinApInvoiceLine> ApInvoiceLines { get; set; } = new List<FinApInvoiceLine>();
}
