using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Document;

namespace ERP.Application.Services.Document;

public interface IDocumentService
{
    Task<IReadOnlyList<DocumentDto>> GetByReferenceAsync(string referenceType, int referenceId, int currentUserId, CancellationToken ct = default);
    Task<DocumentDto> UploadAsync(UploadDocumentRequest request, int currentUserId, CancellationToken ct = default);
    Task<DocumentDownloadResult> DownloadAsync(int documentId, int currentUserId, CancellationToken ct = default);
    Task<bool> DeleteAsync(int documentId, int currentUserId, CancellationToken ct = default);

    /// <summary>Active config for a reference type, or null if the reference type is unknown/inactive.</summary>
    Task<DocumentReferenceTypeConfigDto?> GetConfigAsync(string referenceType, CancellationToken ct = default);

    Task<PagedResult<DocumentReferenceTypeConfigDto>> GetConfigsPagedAsync(PagedRequest request, CancellationToken ct = default);
    Task<DocumentReferenceTypeConfigDto?> GetConfigByIdAsync(int id, CancellationToken ct = default);
    Task<DocumentReferenceTypeConfigDto> CreateConfigAsync(DocumentReferenceTypeConfigDto request, CancellationToken ct = default);
    Task<DocumentReferenceTypeConfigDto?> UpdateConfigAsync(int id, DocumentReferenceTypeConfigDto request, CancellationToken ct = default);
    Task<bool> DeleteConfigAsync(int id, CancellationToken ct = default);
}
