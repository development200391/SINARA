using ERP.Domain.Entities.Finance;
using ERP.Domain.Entities.HR;

namespace ERP.Domain.Entities.Sales;

public sealed class SalSalesTeam : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int TeamLeaderId { get; set; }
    public bool IsActive { get; set; } = true;

    public HrEmployee TeamLeader { get; set; } = null!;
    public ICollection<SalSalesTeamMember> Members { get; set; } = new List<SalSalesTeamMember>();
    public ICollection<FinCustomer> Customers { get; set; } = new List<FinCustomer>();
}
