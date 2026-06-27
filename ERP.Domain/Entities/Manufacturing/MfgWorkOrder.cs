using ERP.Domain.Entities.Inventory;
using ERP.Domain.Enums.Manufacturing;

namespace ERP.Domain.Entities.Manufacturing;

public sealed class MfgWorkOrder : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public int? ItemId { get; set; }
    public int? BomId { get; set; }
    public int? RoutingId { get; set; }
    public int? WorkCenterId { get; set; }
    public int? MrpRunId { get; set; }
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Draft;
    public ProductionType ProductionType { get; set; } = ProductionType.MakeToStock;
    public decimal QtyPlanned { get; set; }
    public decimal QtyGood { get; set; }
    public decimal QtyScrap { get; set; }
    public DateOnly PlannedStartDate { get; set; }
    public DateOnly PlannedEndDate { get; set; }
    public DateTimeOffset? ActualStartAt { get; set; }
    public DateTimeOffset? ActualEndAt { get; set; }
    public decimal StandardCostTotal { get; set; }
    public decimal ActualCostTotal { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public InvItem? Item { get; set; }
    public MfgBom? Bom { get; set; }
    public MfgRouting? Routing { get; set; }
    public MfgWorkCenter? WorkCenter { get; set; }
    public MfgMrpRun? MrpRun { get; set; }
    public ICollection<MfgQcInspection> QcInspections { get; set; } = new List<MfgQcInspection>();
    public ICollection<MfgScrapRecord> ScrapRecords { get; set; } = new List<MfgScrapRecord>();
}
