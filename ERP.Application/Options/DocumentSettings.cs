namespace ERP.Application.Options;

public sealed class DocumentSettings
{
    public const string SectionName = "DocumentSettings";

    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;
    public string[] AllowedExtensions { get; set; } = [".pdf", ".jpg", ".jpeg", ".png", ".docx"];
    public string StorageDirectory { get; set; } = "App_Data/uploads/documents";
}
