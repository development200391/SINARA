namespace ERP.Domain.Entities.Document;

public sealed class DocReferenceTypeConfig : BaseEntity
{
    public string ReferenceType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public long? MaxFileSizeBytes { get; set; }
    public int MaxFileCount { get; set; } = 1;
    public string? AllowedExtensions { get; set; }
    public bool IsActive { get; set; } = true;
}
