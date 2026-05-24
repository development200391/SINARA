using ERP.Application.DTOs.Config;

namespace ERP.Web.ViewModels.Config;

public sealed class ConfigMenusIndexViewModel
{
    public int SelectedModuleId { get; set; }
    public ModuleDto? SelectedModule { get; set; }
    public IReadOnlyList<ModuleDto> Modules { get; set; } = [];
    public IReadOnlyList<MenuDto> Menus { get; set; } = [];
    public IReadOnlyList<ConfigMenuRowViewModel> MenuRows { get; set; } = [];
}

public sealed class ConfigMenuRowViewModel
{
    public MenuDto Menu { get; set; } = new();
    public int Level { get; set; }
    public string? ParentName { get; set; }
}
