using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Document;

namespace ERP.Web.Services;

public sealed class DocumentApiClient(HttpClient httpClient, ILogger<DocumentApiClient> logger) : ApiClientBase(httpClient, logger, "Document"), IDocumentApiClient
{
    public Task<PagedResult<DocumentCategoryDto>?> GetCategoriesAsync(string accessToken, PagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>
        {
            $"page={request.Page}",
            $"pageSize={request.PageSize}",
            $"search={Uri.EscapeDataString(request.Search ?? string.Empty)}",
            $"sortBy={Uri.EscapeDataString(request.SortBy ?? string.Empty)}",
            $"sortDirection={Uri.EscapeDataString(request.SortDirection ?? string.Empty)}"
        };

        return SendWithResultAsync<PagedResult<DocumentCategoryDto>>(HttpMethod.Get, $"api/v1/document-categories?{string.Join("&", parameters)}", accessToken, null, ct).ToDataAsync();
    }

    public Task<DocumentCategoryDto?> GetCategoryByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<DocumentCategoryDto>(HttpMethod.Get, $"api/v1/document-categories/{id}", accessToken, null, ct).ToDataAsync();

    public Task<ApiCallResult<DocumentCategoryDto>> CreateCategoryAsync(string accessToken, DocumentCategoryDto request, CancellationToken ct = default)
        => SendWithResultAsync<DocumentCategoryDto>(HttpMethod.Post, "api/v1/document-categories", accessToken, request, ct);

    public Task<ApiCallResult<DocumentCategoryDto>> UpdateCategoryAsync(string accessToken, int id, DocumentCategoryDto request, CancellationToken ct = default)
        => SendWithResultAsync<DocumentCategoryDto>(HttpMethod.Put, $"api/v1/document-categories/{id}", accessToken, request, ct);

    public Task<ApiCallResult<object?>> DeleteCategoryAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Delete, $"api/v1/document-categories/{id}", accessToken, null, ct);
}
