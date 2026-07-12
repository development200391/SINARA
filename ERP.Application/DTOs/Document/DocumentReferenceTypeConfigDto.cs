namespace ERP.Application.DTOs.Document;

public sealed class DocumentReferenceTypeConfigDto
{
    public int Id { get; set; }
    public string ReferenceType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public long? MaxFileSizeBytes { get; set; }
    public int MaxFileCount { get; set; } = 1;

    /// <summary>Comma-separated extensions including the leading dot, e.g. ".pdf,.jpg,.png". Null/empty falls back to the global DocumentSettings default.</summary>
    public string? AllowedExtensions { get; set; }
    public bool IsActive { get; set; } = true;
}
