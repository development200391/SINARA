namespace ERP.Web.ViewModels.Document;

public sealed class GeneralDocumentUploadViewModel
{
    public string FileFieldName { get; set; } = "AttachmentFiles";
    public string NoteFieldName { get; set; } = "AttachmentNote";
    public string? NoteValue { get; set; }
    public bool AllowMultiple { get; set; }
    public bool IsRequired { get; set; }
    public bool HasExistingDocuments { get; set; }
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;
    public int MaxFileCount { get; set; } = 1;
    public IReadOnlyList<string> AllowedExtensions { get; set; } = [];
}
