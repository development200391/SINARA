namespace ERP.Domain.Entities.Inventory;

public sealed class InvUnitOfMeasure : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<InvItem> BaseItems { get; set; } = new List<InvItem>();
    public ICollection<InvItem> PurchaseItems { get; set; } = new List<InvItem>();
    public ICollection<InvItemUnitConversion> FromConversions { get; set; } = new List<InvItemUnitConversion>();
    public ICollection<InvItemUnitConversion> ToConversions { get; set; } = new List<InvItemUnitConversion>();
}
