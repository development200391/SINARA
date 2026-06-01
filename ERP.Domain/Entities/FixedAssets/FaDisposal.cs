using ERP.Domain.Enums.FixedAssets;

namespace ERP.Domain.Entities.FixedAssets;

public sealed class FaDisposal : BaseEntity
{
    public string DisposalNo { get; set; } = string.Empty;
    public int AssetId { get; set; }
    public DateOnly DisposalDate { get; set; }
    public DisposalType DisposalType { get; set; } = DisposalType.Sale;
    public decimal? SaleAmount { get; set; }
    public decimal DisposalExpense { get; set; }
    public decimal? GainLossAmount { get; set; }
    public DisposalStatus Status { get; set; } = DisposalStatus.Draft;
    public string? Notes { get; set; }

    public FaAsset Asset { get; set; } = null!;
}
