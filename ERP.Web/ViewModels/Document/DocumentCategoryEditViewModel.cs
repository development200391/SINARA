using System.ComponentModel.DataAnnotations;

namespace ERP.Web.ViewModels.Document;

public sealed class DocumentCategoryEditViewModel
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Module { get; set; }

    public bool IsActive { get; set; } = true;
}
