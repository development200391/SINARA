using ERP.Domain.Entities.System;

namespace ERP.Domain.Entities.Document;

public sealed class DocDocument : BaseEntity
{
    public string ReferenceType { get; set; } = string.Empty;
    public int ReferenceId { get; set; }
    public int? CategoryId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int UploadedBy { get; set; }
    public DateTimeOffset UploadedAt { get; set; }

    public DocDocumentCategory? Category { get; set; }
    public SysUser UploadedByUser { get; set; } = null!;
}
