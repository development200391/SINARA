namespace ERP.Domain.Entities.Inventory;

public sealed class InvBrand : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<InvItem> Items { get; set; } = new List<InvItem>();
}
