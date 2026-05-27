using ERP.Application.DTOs.Config;
using ERP.Application.DTOs.Common;

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

    Task<PagedResult<AuditLogDto>> GetPagedAsync(AuditLogPagedRequest request, CancellationToken ct = default);
}
