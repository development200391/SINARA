namespace ERP.Domain.Entities.Config;

public sealed class CfgModule : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<CfgMenu> Menus { get; set; } = new List<CfgMenu>();
}
