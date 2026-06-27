using ERP.Domain.Entities.Inventory;

namespace ERP.Domain.Entities.Sales;

public sealed class SalPriceListItem : BaseEntity
{
    public int PriceListId { get; set; }
    public int ItemId { get; set; }
    public int UomId { get; set; }
    public decimal MinQty { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal DiscountPct { get; set; }

    public SalPriceList PriceList { get; set; } = null!;
    public InvItem Item { get; set; } = null!;
    public InvUnitOfMeasure Uom { get; set; } = null!;
}
