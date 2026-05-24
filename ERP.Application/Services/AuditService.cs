using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Config;
using ERP.Domain.Entities.System;
using ERP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Services;

public sealed class AuditService(IUnitOfWork unitOfWork) : IAuditService
{
    public async Task LogAsync(
        string action,
        int? userId,
        string? username,
        string? entityName,
        string? entityId,
        string? oldValues,
        string? newValues,
        string? ipAddress,
        CancellationToken ct = default)
    {
        var repository = unitOfWork.Repository<SysAuditLog>();

        await repository.AddAsync(new SysAuditLog
        {
            UserId = userId,
            Username = username,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ipAddress,
            CreatedAt = DateTimeOffset.UtcNow
        }, ct);

        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<AuditLogDto>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var query = unitOfWork.Repository<SysAuditLog>()
            .Query()
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                (x.Username != null && x.Username.ToLower().Contains(search)) ||
                x.Action.ToLower().Contains(search) ||
                (x.EntityName != null && x.EntityName.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AuditLogDto
            {
                Id = x.Id,
                UserId = x.UserId,
                Username = x.Username,
                Action = x.Action,
                EntityName = x.EntityName,
                EntityId = x.EntityId,
                OldValues = x.OldValues,
                NewValues = x.NewValues,
                IpAddress = x.IpAddress,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(ct);

        return PagedResult<AuditLogDto>.Create(items, totalCount, page, pageSize);
    }
}
