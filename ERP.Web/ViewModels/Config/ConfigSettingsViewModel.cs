using ERP.Application.DTOs.Config;

namespace ERP.Web.ViewModels.Config;

public sealed class ConfigSettingsViewModel
{
    public AppSettingsDto Settings { get; set; } = new();
    public IReadOnlyList<LanguageDto> Languages { get; set; } = [];
}
