using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Document;

namespace ERP.Web.Services;

public interface IDocumentApiClient
{
    Task<PagedResult<DocumentReferenceTypeConfigDto>?> GetConfigsAsync(string accessToken, PagedRequest request, CancellationToken ct = default);
    Task<DocumentReferenceTypeConfigDto?> GetConfigByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<DocumentReferenceTypeConfigDto>> CreateConfigAsync(string accessToken, DocumentReferenceTypeConfigDto request, CancellationToken ct = default);
    Task<ApiCallResult<DocumentReferenceTypeConfigDto>> UpdateConfigAsync(string accessToken, int id, DocumentReferenceTypeConfigDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteConfigAsync(string accessToken, int id, CancellationToken ct = default);
}
