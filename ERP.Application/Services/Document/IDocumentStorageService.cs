namespace ERP.Application.Services.Document;

public interface IDocumentStorageService
{
    Task<string> SaveAsync(Stream content, string fileExtension, string referenceType, int referenceId, CancellationToken ct = default);
    Task<Stream> OpenReadStreamAsync(string storagePath, CancellationToken ct = default);
    Task DeleteAsync(string storagePath, CancellationToken ct = default);
}
