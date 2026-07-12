namespace ERP.Application.DTOs.Document;

public sealed class DocumentCategoryDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Module { get; set; }
    public bool IsActive { get; set; } = true;
}
