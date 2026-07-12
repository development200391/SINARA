namespace ERP.Application.DTOs.Document;

public sealed class DocumentReferenceTypeConfigDto
{
    public int Id { get; set; }
    public string ReferenceType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsMultiple { get; set; }
    public int MaxFileCount { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public List<DocumentReferenceTypeConfigDetailDto> Details { get; set; } = [];

    // Backward-compatible view sourced from the first (lowest SortOrder) detail row,
    // consumed by the upload validation pipeline and the existing Web/mobile leave-request
    // attachment UI until they're reworked to be slot-aware (see ReadMeDocumentGeneral.md).
    public bool IsRequired => Details.OrderBy(x => x.SortOrder).FirstOrDefault()?.IsRequired ?? false;
    public long? MaxFileSizeBytes => Details.OrderBy(x => x.SortOrder).FirstOrDefault()?.MaxFileSizeBytes;
    public string? AllowedExtensions => Details.OrderBy(x => x.SortOrder).FirstOrDefault()?.AllowedExtensions;
}

public sealed class DocumentReferenceTypeConfigDetailDto
{
    public int Id { get; set; }
    public int SortOrder { get; set; }
    public string Name { get; set; } = string.Empty;
    public long? MaxFileSizeBytes { get; set; }
    public bool IsRequired { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>Comma-separated extensions including the leading dot, e.g. ".pdf,.jpg,.png". Null/empty falls back to the global DocumentSettings default.</summary>
    public string? AllowedExtensions { get; set; }
}
