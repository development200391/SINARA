using ERP.Application.DTOs.Document;

namespace ERP.Web.ViewModels.Document;

public sealed class GeneralDocumentListViewModel
{
    public IReadOnlyList<DocumentDto> Documents { get; set; } = [];
    public Func<int, string>? DownloadUrl { get; set; }
    public Func<int, string>? DeleteUrl { get; set; }
    public string DeleteConfirmMessage { get; set; } = "Delete this document?";
    public string EmptyMessage { get; set; } = "No documents attached yet.";
}
