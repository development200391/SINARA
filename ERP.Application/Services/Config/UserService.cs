using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Config;
using ERP.Domain.Entities.Config;
using ERP.Domain.Entities.System;
using ERP.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Services.Config;

public sealed class UserService(
    IUnitOfWork unitOfWork,
    IPasswordHasher<SysUser> passwordHasher,
    ICacheService cacheService) : IUserService
{
    private const string UserPermissionsKeyPrefix = "ERP_cfg:permissions:user:";

    public async Task<PagedResult<UserDto>> GetPagedAsync(UserPagedRequest request, CancellationToken ct = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var normalizedSortBy = NormalizeSortBy(request.SortBy);
        var normalizedSortDirection = NormalizeSortDirection(request.SortDirection);

        var query = unitOfWork.Repository<SysUser>()
            .Query()
            .AsNoTracking()
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Username.ToLower().Contains(search) ||
                x.FullName.ToLower().Contains(search) ||
                x.Email.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            var usernameFilter = request.Username.Trim().ToLowerInvariant();
            query = query.Where(x => x.Username.ToLower().Contains(usernameFilter));
        }

        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            var fullNameFilter = request.FullName.Trim().ToLowerInvariant();
            query = query.Where(x => x.FullName.ToLower().Contains(fullNameFilter));
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailFilter = request.Email.Trim().ToLowerInvariant();
            query = query.Where(x => x.Email.ToLower().Contains(emailFilter));
        }

        var total = await query.CountAsync(ct);

        query = ApplySorting(query, normalizedSortBy, normalizedSortDirection);

        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UserDto
            {
                Id = x.Id,
                Username = x.Username,
                FullName = x.FullName,
                Email = x.Email,
                Phone = x.Phone,
                IsActive = x.IsActive,
                Roles = x.UserRoles
                    .Select(ur => ur.Role.Name)
                    .OrderBy(r => r)
                    .ToList()
            })
            .ToListAsync(ct);

        return PagedResult<UserDto>.Create(users, total, page, pageSize);
    }

    public async Task<UserDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await GetByIdInternalAsync(id, ct);
    }

    public async Task<UserDto> CreateAsync(UserDto request, CancellationToken ct = default)
    {
        var usernameExists = await unitOfWork.Repository<SysUser>()
            .Query()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Username == request.Username, ct);

        if (usernameExists)
        {
            throw new InvalidOperationException("Username already exists.");
        }

        var emailExists = await unitOfWork.Repository<SysUser>()
            .Query()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Email == request.Email, ct);

        if (emailExists)
        {
            throw new InvalidOperationException("Email already exists.");
        }

        var user = new SysUser
        {
            Username = request.Username,
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            IsActive = request.IsActive,
            LanguagePreference = "en",
            CreatedBy = "system"
        };

        user.PasswordHash = passwordHasher.HashPassword(user, "ChangeMe123!");

        await unitOfWork.Repository<SysUser>().AddAsync(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await SyncUserRolesAsync(user.Id, request.Roles, ct);

        return await GetByIdInternalAsync(user.Id, ct)
            ?? throw new InvalidOperationException("Failed to load created user.");
    }

    public async Task<UserDto?> UpdateAsync(int id, UserDto request, CancellationToken ct = default)
    {
        var user = await unitOfWork.Repository<SysUser>()
            .Query()
            .Include(x => x.UserRoles)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (user is null)
        {
            return null;
        }

        user.FullName = request.FullName;
        user.Email = request.Email;
        user.Phone = request.Phone;
        user.IsActive = request.IsActive;
        user.UpdatedBy = "system";
        user.UpdatedAt = DateTimeOffset.UtcNow;

        unitOfWork.Repository<SysUser>().Update(user);
        await unitOfWork.SaveChangesAsync(ct);

        await SyncUserRolesAsync(user.Id, request.Roles, ct);
        await SafeCacheAsync(() => cacheService.RemoveAsync($"{UserPermissionsKeyPrefix}{user.Id}", ct));

        return await GetByIdInternalAsync(id, ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var user = await unitOfWork.Repository<SysUser>()
            .Query()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (user is null)
        {
            return false;
        }

        unitOfWork.Repository<SysUser>().Delete(user);
        await unitOfWork.SaveChangesAsync(ct);

        await SafeCacheAsync(() => cacheService.RemoveAsync($"{UserPermissionsKeyPrefix}{id}", ct));

        return true;
    }

    private static string NormalizeSortBy(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return "username";
        }

        return sortBy.Trim().ToLowerInvariant() switch
        {
            "username" => "username",
            "fullname" => "fullName",
            "email" => "email",
            "isactive" => "isActive",
            _ => "username"
        };
    }

    private static string NormalizeSortDirection(string? sortDirection) =>
        string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";

    private static IQueryable<SysUser> ApplySorting(IQueryable<SysUser> query, string sortBy, string sortDirection)
    {
        var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy switch
        {
            "fullName" => isDesc
                ? query.OrderByDescending(x => x.FullName).ThenBy(x => x.Username)
                : query.OrderBy(x => x.FullName).ThenBy(x => x.Username),

            "email" => isDesc
                ? query.OrderByDescending(x => x.Email).ThenBy(x => x.Username)
                : query.OrderBy(x => x.Email).ThenBy(x => x.Username),

            "isActive" => isDesc
                ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.Username)
                : query.OrderBy(x => x.IsActive).ThenBy(x => x.Username),

            _ => isDesc
                ? query.OrderByDescending(x => x.Username)
                : query.OrderBy(x => x.Username)
        };
    }

    private async Task SyncUserRolesAsync(int userId, IReadOnlyList<string> requestedRoles, CancellationToken ct)
    {
        var normalizedRequested = requestedRoles
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var roles = await unitOfWork.Repository<CfgRole>()
            .Query()
            .Where(x => normalizedRequested.Contains(x.Name))
            .ToListAsync(ct);

        var roleIds = roles.Select(x => x.Id).ToHashSet();

        var existing = await unitOfWork.Repository<SysUserRole>()
            .Query()
            .Where(x => x.UserId == userId)
            .ToListAsync(ct);

        foreach (var entry in existing.Where(x => !roleIds.Contains(x.RoleId)))
        {
            unitOfWork.Repository<SysUserRole>().Delete(entry);
        }

        foreach (var roleId in roleIds)
        {
            if (existing.Any(x => x.RoleId == roleId))
            {
                continue;
            }

            await unitOfWork.Repository<SysUserRole>().AddAsync(new SysUserRole
            {
                UserId = userId,
                RoleId = roleId
            }, ct);
        }

        await unitOfWork.SaveChangesAsync(ct);
        await SafeCacheAsync(() => cacheService.RemoveAsync($"{UserPermissionsKeyPrefix}{userId}", ct));
    }

    private async Task<UserDto?> GetByIdInternalAsync(int id, CancellationToken ct)
    {
        return await unitOfWork.Repository<SysUser>()
            .Query()
            .AsNoTracking()
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .Where(x => x.Id == id)
            .Select(x => new UserDto
            {
                Id = x.Id,
                Username = x.Username,
                FullName = x.FullName,
                Email = x.Email,
                Phone = x.Phone,
                IsActive = x.IsActive,
                Roles = x.UserRoles.Select(ur => ur.Role.Name).OrderBy(r => r).ToList()
            })
            .FirstOrDefaultAsync(ct);
    }

    private static async Task SafeCacheAsync(Func<Task> operation)
    {
        try
        {
            await operation();
        }
        catch
        {
            // Cache failures should not block core workflow.
        }
    }
}
