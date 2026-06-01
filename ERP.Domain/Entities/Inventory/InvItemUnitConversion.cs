namespace ERP.Domain.Entities.Inventory;

public sealed class InvItemUnitConversion : BaseEntity
{
    public int ItemId { get; set; }
    public int FromUomId { get; set; }
    public int ToUomId { get; set; }
    public decimal ConversionFactor { get; set; }
    public bool IsActive { get; set; } = true;

    public InvItem Item { get; set; } = null!;
    public InvUnitOfMeasure FromUom { get; set; } = null!;
    public InvUnitOfMeasure ToUom { get; set; } = null!;
}
