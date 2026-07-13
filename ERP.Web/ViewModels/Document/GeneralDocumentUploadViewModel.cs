namespace ERP.Web.ViewModels.Document;

public sealed class GeneralDocumentUploadViewModel
{
    public string FileFieldName { get; set; } = "AttachmentFiles";
    public string NoteFieldName { get; set; } = "AttachmentNotes";

    /// <summary>View-only display (e.g. Details page): no file picker, no note input, no delete.</summary>
    public bool ReadOnly { get; set; }

    public IReadOnlyList<GeneralDocumentUploadSlotViewModel> Slots { get; set; } = [];
}

public sealed class GeneralDocumentUploadSlotViewModel
{
    public string Name { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public long MaxFileSizeBytes { get; set; }
    public IReadOnlyList<string> AllowedExtensions { get; set; } = [];
    public string? NoteValue { get; set; }
    public GeneralDocumentExistingFileViewModel? ExistingFile { get; set; }
}

public sealed class GeneralDocumentExistingFileViewModel
{
    public int DocumentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string? DownloadUrl { get; set; }

    /// <summary>Id of the standalone &lt;form&gt; (rendered outside the main form to avoid HTML form nesting) that this slot's delete button submits via the form="" attribute.</summary>
    public string? DeleteFormId { get; set; }
}
