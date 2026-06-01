using ERP.Domain.Enums.FixedAssets;

namespace ERP.Domain.Entities.FixedAssets;

public sealed class FaDepreciationSchedule : BaseEntity
{
    public int AssetId { get; set; }
    public short PeriodYear { get; set; }
    public byte PeriodMonth { get; set; }
    public DateOnly DepreciationDate { get; set; }
    public decimal DepreciationAmount { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public decimal BookValue { get; set; }
    public DepreciationScheduleStatus Status { get; set; } = DepreciationScheduleStatus.Pending;
    public int? RunId { get; set; }

    public FaAsset Asset { get; set; } = null!;
    public FaDepreciationRun? Run { get; set; }
}
