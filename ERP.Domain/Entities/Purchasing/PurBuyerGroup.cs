using ERP.Domain.Entities.Finance;
using ERP.Domain.Entities.HR;

namespace ERP.Domain.Entities.Purchasing;

public sealed class PurBuyerGroup : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int BuyerEmployeeId { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public HrEmployee BuyerEmployee { get; set; } = null!;
    public ICollection<PurBuyerGroupCategory> CategoryMappings { get; set; } = new List<PurBuyerGroupCategory>();
    public ICollection<FinVendor> Vendors { get; set; } = new List<FinVendor>();
}
