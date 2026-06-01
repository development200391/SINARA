using ERP.Domain.Entities.Finance;
using ERP.Domain.Enums.FixedAssets;

namespace ERP.Domain.Entities.FixedAssets;

public sealed class FaAssetCategory : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DepreciationMethod DepreciationMethod { get; set; } = DepreciationMethod.StraightLine;
    public int UsefulLifeMonths { get; set; }
    public decimal? DepreciationRate { get; set; }
    public int? AssetAccountId { get; set; }
    public int? AccumulatedDepreciationAccountId { get; set; }
    public int? DepreciationExpenseAccountId { get; set; }
    public bool IsActive { get; set; } = true;

    public FinAccount? AssetAccount { get; set; }
    public FinAccount? AccumulatedDepreciationAccount { get; set; }
    public FinAccount? DepreciationExpenseAccount { get; set; }
    public ICollection<FaAsset> Assets { get; set; } = new List<FaAsset>();
}
