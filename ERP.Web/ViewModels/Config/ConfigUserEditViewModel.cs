using ERP.Application.DTOs.Config;
using System.ComponentModel.DataAnnotations;

namespace ERP.Web.ViewModels.Config;

public sealed class ConfigUserEditViewModel
{
    public int Id { get; set; }

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public bool IsActive { get; set; } = true;

    public List<string> SelectedRoles { get; set; } = [];

    public IReadOnlyList<RoleDto> AvailableRoles { get; set; } = [];
}
