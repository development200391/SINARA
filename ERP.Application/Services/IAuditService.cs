using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Config;

namespace ERP.Application.Services;

public interface IAuditService
{
    Task LogAsync(
        string action,
        int? userId,
        string? username,
        string? entityName,
        string? entityId,
        string? oldValues,
        string? newValues,
        string? ipAddress,
        CancellationToken ct = default);

    Task<PagedResult<AuditLogDto>> GetPagedAsync(PagedRequest request, CancellationToken ct = default);
}
