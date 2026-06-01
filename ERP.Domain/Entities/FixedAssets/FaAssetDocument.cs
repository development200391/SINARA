namespace ERP.Domain.Entities.FixedAssets;

public sealed class FaAssetDocument : BaseEntity
{
    public int AssetId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public FaAsset Asset { get; set; } = null!;
}
