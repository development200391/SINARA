using ERP.Domain.Entities.HR;

namespace ERP.Domain.Entities.Sales;

public sealed class SalSalesTeamMember : BaseEntity
{
    public int SalesTeamId { get; set; }
    public int EmployeeId { get; set; }

    public SalSalesTeam SalesTeam { get; set; } = null!;
    public HrEmployee Employee { get; set; } = null!;
}
