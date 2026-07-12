namespace ERP.Web.ViewModels.Document;

public sealed class GeneralDocumentUploadViewModel
{
    public string FileFieldName { get; set; } = "AttachmentFiles";
    public string NoteFieldName { get; set; } = "AttachmentNote";
    public string? NoteValue { get; set; }
    public bool HasExistingDocuments { get; set; }
    public IReadOnlyList<GeneralDocumentUploadSlotViewModel> Slots { get; set; } = [];
}

public sealed class GeneralDocumentUploadSlotViewModel
{
    public string Name { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public long MaxFileSizeBytes { get; set; }
    public IReadOnlyList<string> AllowedExtensions { get; set; } = [];
}
