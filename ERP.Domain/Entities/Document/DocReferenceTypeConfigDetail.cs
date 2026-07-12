namespace ERP.Domain.Entities.Document;

public sealed class DocReferenceTypeConfigDetail : BaseEntity
{
    public int ConfigId { get; set; }
    public DocReferenceTypeConfig? Config { get; set; }
    public int SortOrder { get; set; }
    public string Name { get; set; } = string.Empty;
    public long? MaxFileSizeBytes { get; set; }
    public bool IsRequired { get; set; }
    public bool IsActive { get; set; } = true;
    public string? AllowedExtensions { get; set; }
}
