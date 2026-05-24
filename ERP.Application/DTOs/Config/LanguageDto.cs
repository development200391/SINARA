namespace ERP.Application.DTOs.Config;

public sealed class LanguageDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
