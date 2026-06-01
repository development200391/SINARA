using ERP.Domain.Entities.Finance;
using ERP.Domain.Entities.HR;

namespace ERP.Domain.Entities.Inventory;

public sealed class InvWarehouse : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public int? ManagerId { get; set; }
    public int? CostCenterId { get; set; }
    public bool IsTransit { get; set; }
    public bool IsActive { get; set; } = true;

    public HrEmployee? Manager { get; set; }
    public FinCostCenter? CostCenter { get; set; }
    public ICollection<InvWarehouseLocation> Locations { get; set; } = new List<InvWarehouseLocation>();
    public ICollection<InvStockBalance> StockBalances { get; set; } = new List<InvStockBalance>();
}
