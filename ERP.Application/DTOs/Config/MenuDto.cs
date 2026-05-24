namespace ERP.Application.DTOs.Config;

public sealed class MenuDto
{
    public int Id { get; set; }
    public int ModuleId { get; set; }
    public int? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
