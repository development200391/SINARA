using ERP.Domain.Entities.Finance;

namespace ERP.Domain.Entities.Manufacturing;

public sealed class MfgWorkCenter : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal CapacityHoursPerDay { get; set; }
    public decimal LaborCostPerHour { get; set; }
    public decimal OverheadCostPerHour { get; set; }
    public int? WipAccountId { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public FinAccount? WipAccount { get; set; }
    public ICollection<MfgRouting> Routings { get; set; } = new List<MfgRouting>();
    public ICollection<MfgWorkOrder> WorkOrders { get; set; } = new List<MfgWorkOrder>();
    public ICollection<MfgScrapRecord> ScrapRecords { get; set; } = new List<MfgScrapRecord>();
    public ICollection<MfgOeeSnapshot> OeeSnapshots { get; set; } = new List<MfgOeeSnapshot>();
}
