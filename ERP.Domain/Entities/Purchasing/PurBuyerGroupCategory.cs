using ERP.Domain.Entities.Inventory;

namespace ERP.Domain.Entities.Purchasing;

public sealed class PurBuyerGroupCategory : BaseEntity
{
    public int BuyerGroupId { get; set; }
    public int ItemCategoryId { get; set; }

    public PurBuyerGroup BuyerGroup { get; set; } = null!;
    public InvItemCategory ItemCategory { get; set; } = null!;
}
