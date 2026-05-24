namespace ERP.Application.DTOs.Config;

public sealed class AppSettingsDto
{
    public string AppName { get; set; } = "SINARA ERP";
    public string? LogoUrl { get; set; }
    public string DefaultLanguage { get; set; } = "en";
}
