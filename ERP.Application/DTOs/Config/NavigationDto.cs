namespace ERP.Application.DTOs.Config;

public sealed class NavigationModuleDto
{
    public int ModuleId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string ModuleCode { get; set; } = string.Empty;
    public string? ModuleIcon { get; set; }
    public int SortOrder { get; set; }
    public IReadOnlyList<NavigationMenuDto> Menus { get; set; } = [];
}

public sealed class NavigationMenuDto
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public IReadOnlyList<NavigationMenuDto> Children { get; set; } = [];
}
