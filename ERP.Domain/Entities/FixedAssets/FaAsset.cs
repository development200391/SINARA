using ERP.Domain.Entities.HR;
using ERP.Domain.Enums.FixedAssets;

namespace ERP.Domain.Entities.FixedAssets;

public sealed class FaAsset : BaseEntity
{
    public string AssetCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int LocationId { get; set; }
    public int? DepartmentId { get; set; }
    public DateOnly AcquisitionDate { get; set; }
    public DateOnly InServiceDate { get; set; }
    public decimal AcquisitionCost { get; set; }
    public decimal SalvageValue { get; set; }
    public int UsefulLifeMonths { get; set; }
    public DepreciationMethod DepreciationMethod { get; set; } = DepreciationMethod.StraightLine;
    public decimal? DepreciationRate { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public decimal BookValue { get; set; }
    public AssetStatus Status { get; set; } = AssetStatus.Active;
    public string? SerialNumber { get; set; }
    public string? VendorName { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public FaAssetCategory Category { get; set; } = null!;
    public FaLocation Location { get; set; } = null!;
    public HrDepartment? Department { get; set; }
    public ICollection<FaAssetDocument> Documents { get; set; } = new List<FaAssetDocument>();
    public ICollection<FaDepreciationSchedule> DepreciationSchedules { get; set; } = new List<FaDepreciationSchedule>();
    public ICollection<FaAssetTransfer> Transfers { get; set; } = new List<FaAssetTransfer>();
    public ICollection<FaMaintenanceOrder> MaintenanceOrders { get; set; } = new List<FaMaintenanceOrder>();
    public ICollection<FaDisposal> Disposals { get; set; } = new List<FaDisposal>();
    public ICollection<FaRevaluation> Revaluations { get; set; } = new List<FaRevaluation>();
    public ICollection<FaAssetHistory> Histories { get; set; } = new List<FaAssetHistory>();
}
