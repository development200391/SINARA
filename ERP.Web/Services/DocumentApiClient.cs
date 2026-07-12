using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Document;

namespace ERP.Web.Services;

public sealed class DocumentApiClient(HttpClient httpClient, ILogger<DocumentApiClient> logger) : ApiClientBase(httpClient, logger, "Document"), IDocumentApiClient
{
    public Task<PagedResult<DocumentReferenceTypeConfigDto>?> GetConfigsAsync(string accessToken, PagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>
        {
            $"page={request.Page}",
            $"pageSize={request.PageSize}",
            $"search={Uri.EscapeDataString(request.Search ?? string.Empty)}",
            $"sortBy={Uri.EscapeDataString(request.SortBy ?? string.Empty)}",
            $"sortDirection={Uri.EscapeDataString(request.SortDirection ?? string.Empty)}"
        };

        return SendWithResultAsync<PagedResult<DocumentReferenceTypeConfigDto>>(HttpMethod.Get, $"api/v1/document-reference-type-configs?{string.Join("&", parameters)}", accessToken, null, ct).ToDataAsync();
    }

    public Task<DocumentReferenceTypeConfigDto?> GetConfigByIdAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<DocumentReferenceTypeConfigDto>(HttpMethod.Get, $"api/v1/document-reference-type-configs/{id}", accessToken, null, ct).ToDataAsync();

    public Task<ApiCallResult<DocumentReferenceTypeConfigDto>> CreateConfigAsync(string accessToken, DocumentReferenceTypeConfigDto request, CancellationToken ct = default)
        => SendWithResultAsync<DocumentReferenceTypeConfigDto>(HttpMethod.Post, "api/v1/document-reference-type-configs", accessToken, request, ct);

    public Task<ApiCallResult<DocumentReferenceTypeConfigDto>> UpdateConfigAsync(string accessToken, int id, DocumentReferenceTypeConfigDto request, CancellationToken ct = default)
        => SendWithResultAsync<DocumentReferenceTypeConfigDto>(HttpMethod.Put, $"api/v1/document-reference-type-configs/{id}", accessToken, request, ct);

    public Task<ApiCallResult<object?>> DeleteConfigAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Delete, $"api/v1/document-reference-type-configs/{id}", accessToken, null, ct);
}
