using ERP.Domain.Entities.Finance;
using ERP.Domain.Enums.Inventory;

namespace ERP.Domain.Entities.Inventory;

public sealed class InvItem : BaseEntity
{
    public string ItemCode { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public int? BrandId { get; set; }
    public ItemType Type { get; set; } = ItemType.Product;
    public int BaseUomId { get; set; }
    public int? PurchaseUomId { get; set; }
    public ItemStatus Status { get; set; } = ItemStatus.Active;
    public ValuationMethod ValuationMethod { get; set; } = ValuationMethod.WeightedAverageCost;
    public decimal? LastPurchasePrice { get; set; }
    public decimal AvgCost { get; set; }
    public decimal MinStock { get; set; }
    public decimal MaxStock { get; set; }
    public decimal ReorderPoint { get; set; }
    public int LeadTimeDays { get; set; }
    public int? InventoryAccountId { get; set; }
    public int? CogsAccountId { get; set; }
    public int? AdjustmentAccountId { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public InvItemCategory Category { get; set; } = null!;
    public InvBrand? Brand { get; set; }
    public InvUnitOfMeasure BaseUom { get; set; } = null!;
    public InvUnitOfMeasure? PurchaseUom { get; set; }
    public FinAccount? InventoryAccount { get; set; }
    public FinAccount? CogsAccount { get; set; }
    public FinAccount? AdjustmentAccount { get; set; }
    public ICollection<InvItemUnitConversion> UnitConversions { get; set; } = new List<InvItemUnitConversion>();
    public ICollection<InvStockBalance> StockBalances { get; set; } = new List<InvStockBalance>();
}
