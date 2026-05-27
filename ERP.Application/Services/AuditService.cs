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

    public async Task<PagedResult<AuditLogDto>> GetPagedAsync(AuditLogPagedRequest request, CancellationToken ct = default)
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
                (x.EntityName != null && x.EntityName.ToLower().Contains(search)) ||
                (x.EntityId != null && x.EntityId.ToLower().Contains(search)) ||
                (x.IpAddress != null && x.IpAddress.ToLower().Contains(search)));
        }

        if (request.DateFrom.HasValue)
        {
            var fromDate = request.DateFrom.Value;
            var fromUtc = new DateTimeOffset(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0, TimeSpan.Zero);
            query = query.Where(x => x.CreatedAt >= fromUtc);
        }

        if (request.DateTo.HasValue)
        {
            var toDate = request.DateTo.Value.AddDays(1);
            var toUtcExclusive = new DateTimeOffset(toDate.Year, toDate.Month, toDate.Day, 0, 0, 0, TimeSpan.Zero);
            query = query.Where(x => x.CreatedAt < toUtcExclusive);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = request.Status.Trim().ToLowerInvariant();
            query = query.Where(x => x.Action.ToLower() == status);
        }

        if (request.HasIpOnly)
        {
            query = query.Where(x => x.IpAddress != null && x.IpAddress != string.Empty);
        }

        var entityNames = ParseCsv(request.EntityNames);
        if (entityNames.Count > 0)
        {
            var normalizedEntityNames = entityNames
                .Select(x => x.ToLowerInvariant())
                .ToArray();

            query = query.Where(x => x.EntityName != null && normalizedEntityNames.Contains(x.EntityName.ToLower()));
        }

        var sortBy = request.SortBy?.Trim().ToLowerInvariant();
        var sortDirection = string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase) ? "asc" : "desc";

        var totalCount = await query.CountAsync(ct);

        var sortedQuery = (sortBy, sortDirection) switch
        {
            ("username", "asc") => query.OrderBy(x => x.Username).ThenByDescending(x => x.CreatedAt),
            ("username", "desc") => query.OrderByDescending(x => x.Username).ThenByDescending(x => x.CreatedAt),
            ("action", "asc") => query.OrderBy(x => x.Action).ThenByDescending(x => x.CreatedAt),
            ("action", "desc") => query.OrderByDescending(x => x.Action).ThenByDescending(x => x.CreatedAt),
            ("entityname", "asc") => query.OrderBy(x => x.EntityName).ThenByDescending(x => x.CreatedAt),
            ("entityname", "desc") => query.OrderByDescending(x => x.EntityName).ThenByDescending(x => x.CreatedAt),
            ("entityid", "asc") => query.OrderBy(x => x.EntityId).ThenByDescending(x => x.CreatedAt),
            ("entityid", "desc") => query.OrderByDescending(x => x.EntityId).ThenByDescending(x => x.CreatedAt),
            ("ipaddress", "asc") => query.OrderBy(x => x.IpAddress).ThenByDescending(x => x.CreatedAt),
            ("ipaddress", "desc") => query.OrderByDescending(x => x.IpAddress).ThenByDescending(x => x.CreatedAt),
            ("createdat", "asc") => query.OrderBy(x => x.CreatedAt),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        var items = await sortedQuery
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

    private static IReadOnlyList<string> ParseCsv(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
