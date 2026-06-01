using ERP.Domain.Enums.FixedAssets;

namespace ERP.Domain.Entities.FixedAssets;

public sealed class FaAssetHistory : BaseEntity
{
    public int AssetId { get; set; }
    public DateOnly EventDate { get; set; }
    public AssetHistoryType EventType { get; set; } = AssetHistoryType.Registration;
    public string? ReferenceNo { get; set; }
    public string? Description { get; set; }
    public decimal? AmountChange { get; set; }

    public FaAsset Asset { get; set; } = null!;
}
