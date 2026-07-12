using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Document;

namespace ERP.Application.Services.Document;

public interface IDocumentService
{
    Task<IReadOnlyList<DocumentDto>> GetByReferenceAsync(string referenceType, int referenceId, int currentUserId, CancellationToken ct = default);
    Task<DocumentDto> UploadAsync(UploadDocumentRequest request, int currentUserId, CancellationToken ct = default);
    Task<DocumentDownloadResult> DownloadAsync(int documentId, int currentUserId, CancellationToken ct = default);
    Task<bool> DeleteAsync(int documentId, int currentUserId, CancellationToken ct = default);
    Task<IReadOnlyList<DocumentCategoryDto>> GetCategoryOptionsAsync(CancellationToken ct = default);

    Task<PagedResult<DocumentCategoryDto>> GetCategoriesPagedAsync(PagedRequest request, CancellationToken ct = default);
    Task<DocumentCategoryDto?> GetCategoryByIdAsync(int id, CancellationToken ct = default);
    Task<DocumentCategoryDto> CreateCategoryAsync(DocumentCategoryDto request, CancellationToken ct = default);
    Task<DocumentCategoryDto?> UpdateCategoryAsync(int id, DocumentCategoryDto request, CancellationToken ct = default);
    Task<bool> DeleteCategoryAsync(int id, CancellationToken ct = default);
}
