using ERP.Domain.Enums.FixedAssets;

namespace ERP.Domain.Entities.FixedAssets;

public sealed class FaRevaluation : BaseEntity
{
    public string RevaluationNo { get; set; } = string.Empty;
    public int AssetId { get; set; }
    public DateOnly RevaluationDate { get; set; }
    public decimal OldBookValue { get; set; }
    public decimal NewBookValue { get; set; }
    public decimal ImpairmentAmount { get; set; }
    public RevaluationStatus Status { get; set; } = RevaluationStatus.Draft;
    public string? Notes { get; set; }

    public FaAsset Asset { get; set; } = null!;
}
