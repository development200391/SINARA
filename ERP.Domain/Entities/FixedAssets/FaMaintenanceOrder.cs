using ERP.Domain.Enums.FixedAssets;

namespace ERP.Domain.Entities.FixedAssets;

public sealed class FaMaintenanceOrder : BaseEntity
{
    public string WorkOrderNo { get; set; } = string.Empty;
    public int AssetId { get; set; }
    public DateOnly OrderDate { get; set; }
    public MaintenanceType MaintenanceType { get; set; } = MaintenanceType.Preventive;
    public string? VendorName { get; set; }
    public decimal Cost { get; set; }
    public bool IsCapitalized { get; set; }
    public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Open;
    public string? Notes { get; set; }

    public FaAsset Asset { get; set; } = null!;
}
