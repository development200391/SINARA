namespace ERP.Application.DTOs.Document;

public sealed class DocumentDto
{
    public int Id { get; set; }
    public string ReferenceType { get; set; } = string.Empty;
    public int ReferenceId { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string? Description { get; set; }
    public int UploadedBy { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public DateTimeOffset UploadedAt { get; set; }
}

public sealed class UploadDocumentRequest
{
    public Stream Content { get; set; } = Stream.Null;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string ReferenceType { get; set; } = string.Empty;
    public int ReferenceId { get; set; }
    public int? CategoryId { get; set; }
    public string? Description { get; set; }
}

public sealed class DocumentDownloadResult
{
    public Stream Content { get; set; } = Stream.Null;
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}
