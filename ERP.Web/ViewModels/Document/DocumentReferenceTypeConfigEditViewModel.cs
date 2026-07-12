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

    public bool IsMultiple { get; set; }

    [Range(1, 100)]
    public int MaxFileCount { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public List<DocumentReferenceTypeConfigDetailViewModel> Details { get; set; } = [];
}

public sealed class DocumentReferenceTypeConfigDetailViewModel
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Range(1, long.MaxValue)]
    public long? MaxFileSizeBytes { get; set; }

    public bool IsRequired { get; set; }
    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? AllowedExtensions { get; set; }
}
