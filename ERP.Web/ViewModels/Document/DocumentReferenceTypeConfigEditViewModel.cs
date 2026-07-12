using System.ComponentModel.DataAnnotations;

namespace ERP.Web.ViewModels.Document;

public sealed class DocumentReferenceTypeConfigEditViewModel
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string ReferenceType { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string DisplayName { get; set; } = string.Empty;

    public bool IsRequired { get; set; }

    [Range(1, long.MaxValue)]
    public long? MaxFileSizeBytes { get; set; }

    [Range(1, 100)]
    public int MaxFileCount { get; set; } = 1;

    [MaxLength(500)]
    public string? AllowedExtensions { get; set; }

    public bool IsActive { get; set; } = true;
}
