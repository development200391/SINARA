namespace ERP.Domain.Entities.Document;

public sealed class DocReferenceTypeConfig : BaseEntity
{
    public string ReferenceType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsMultiple { get; set; }
    public int MaxFileCount { get; set; } = 1;
    public bool IsActive { get; set; } = true;

    public ICollection<DocReferenceTypeConfigDetail> Details { get; set; } = new List<DocReferenceTypeConfigDetail>();
}
