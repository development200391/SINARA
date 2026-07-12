using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Document;

namespace ERP.Web.Services;

public interface IDocumentApiClient
{
    Task<PagedResult<DocumentCategoryDto>?> GetCategoriesAsync(string accessToken, PagedRequest request, CancellationToken ct = default);
    Task<DocumentCategoryDto?> GetCategoryByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<DocumentCategoryDto>> CreateCategoryAsync(string accessToken, DocumentCategoryDto request, CancellationToken ct = default);
    Task<ApiCallResult<DocumentCategoryDto>> UpdateCategoryAsync(string accessToken, int id, DocumentCategoryDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteCategoryAsync(string accessToken, int id, CancellationToken ct = default);
}
