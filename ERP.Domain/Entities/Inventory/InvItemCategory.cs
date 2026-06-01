namespace ERP.Domain.Entities.Inventory;

public sealed class InvItemCategory : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? ParentCategoryId { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public InvItemCategory? ParentCategory { get; set; }
    public ICollection<InvItemCategory> ChildCategories { get; set; } = new List<InvItemCategory>();
    public ICollection<InvItem> Items { get; set; } = new List<InvItem>();
}
