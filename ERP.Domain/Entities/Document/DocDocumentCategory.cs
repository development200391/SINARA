namespace ERP.Domain.Entities.Document;

public sealed class DocDocumentCategory : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Module { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<DocDocument> Documents { get; set; } = new List<DocDocument>();
}
