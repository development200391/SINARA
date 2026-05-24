using System.ComponentModel.DataAnnotations;
using ERP.Application.DTOs.Config;

namespace ERP.Web.ViewModels.Config;

public sealed class ConfigMenuEditViewModel
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    public int ModuleId { get; set; }

    public int? ParentId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Url { get; set; }

    [MaxLength(50)]
    public string? Icon { get; set; }

    [Range(1, int.MaxValue)]
    public int SortOrder { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public IReadOnlyList<ModuleDto> AvailableModules { get; set; } = [];
    public IReadOnlyList<MenuDto> AvailableParents { get; set; } = [];
}
