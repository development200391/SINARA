using ERP.Application.DTOs.Config;
using ERP.Domain.Entities.Config;
using ERP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Services.Config;

public sealed class RoleService(IUnitOfWork unitOfWork, ICacheService cacheService) : IRoleService
{
    private const string RolePermissionsKeyPrefix = "ERP_cfg:permissions:role:";
    private const string UserPermissionsKeyPrefix = "ERP_cfg:permissions:user:";

    private static readonly TimeSpan RolePermissionsTtl = TimeSpan.FromMinutes(30);

    public async Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await unitOfWork.Repository<CfgRole>()
            .Query()
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new RoleDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                IsSystem = x.IsSystem,
                IsActive = x.IsActive
            })
            .ToListAsync(ct);
    }

    public async Task<PermissionMatrixDto?> GetPermissionsAsync(int roleId, CancellationToken ct = default)
    {
        var cacheKey = $"{RolePermissionsKeyPrefix}{roleId}";

        var cached = await SafeCacheAsync(() => cacheService.GetAsync<PermissionMatrixDto>(cacheKey, ct));
        if (cached is not null)
        {
            return cached;
        }

        var roleExists = await unitOfWork.Repository<CfgRole>()
            .Query()
            .AsNoTracking()
            .AnyAsync(x => x.Id == roleId, ct);

        if (!roleExists)
        {
            return null;
        }

        var menus = await unitOfWork.Repository<CfgMenu>()
            .Query()
            .AsNoTracking()
            .Include(x => x.Module)
            .OrderBy(x => x.Module.SortOrder)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(ct);

        var permissions = await unitOfWork.Repository<CfgRoleMenuPermission>()
            .Query()
            .AsNoTracking()
            .Where(x => x.RoleId == roleId)
            .ToListAsync(ct);

        var permissionMap = permissions.ToDictionary(x => x.MenuId);

        var result = new PermissionMatrixDto
        {
            RoleId = roleId,
            Permissions = menus.Select(menu =>
            {
                permissionMap.TryGetValue(menu.Id, out var permission);

                return new MenuPermissionDto
                {
                    MenuId = menu.Id,
                    MenuName = $"{menu.Module.Code} / {menu.Name}",
                    CanView = permission?.CanView ?? false,
                    CanCreate = permission?.CanCreate ?? false,
                    CanEdit = permission?.CanEdit ?? false,
                    CanDelete = permission?.CanDelete ?? false
                };
            }).ToList()
        };

        await SafeCacheAsync(() => cacheService.SetAsync(cacheKey, result, RolePermissionsTtl, ct));

        return result;
    }

    public async Task<bool> UpdatePermissionsAsync(int roleId, PermissionMatrixDto request, CancellationToken ct = default)
    {
        var role = await unitOfWork.Repository<CfgRole>()
            .Query()
            .FirstOrDefaultAsync(x => x.Id == roleId, ct);

        if (role is null)
        {
            return false;
        }

        var requestedMenuIds = request.Permissions.Select(x => x.MenuId).Distinct().ToHashSet();

        var existing = await unitOfWork.Repository<CfgRoleMenuPermission>()
            .Query()
            .Where(x => x.RoleId == roleId)
            .ToListAsync(ct);

        foreach (var item in request.Permissions)
        {
            var entity = existing.FirstOrDefault(x => x.MenuId == item.MenuId);
            if (entity is null)
            {
                await unitOfWork.Repository<CfgRoleMenuPermission>().AddAsync(new CfgRoleMenuPermission
                {
                    RoleId = roleId,
                    MenuId = item.MenuId,
                    CanView = item.CanView,
                    CanCreate = item.CanCreate,
                    CanEdit = item.CanEdit,
                    CanDelete = item.CanDelete
                }, ct);

                continue;
            }

            entity.CanView = item.CanView;
            entity.CanCreate = item.CanCreate;
            entity.CanEdit = item.CanEdit;
            entity.CanDelete = item.CanDelete;
            unitOfWork.Repository<CfgRoleMenuPermission>().Update(entity);
        }

        foreach (var stale in existing.Where(x => !requestedMenuIds.Contains(x.MenuId)))
        {
            unitOfWork.Repository<CfgRoleMenuPermission>().Delete(stale);
        }

        await unitOfWork.SaveChangesAsync(ct);

        await SafeCacheAsync(() => cacheService.RemoveAsync($"{RolePermissionsKeyPrefix}{roleId}", ct));
        await SafeCacheAsync(() => cacheService.RemoveByPrefixAsync(UserPermissionsKeyPrefix, ct));

        return true;
    }

    private static async Task<T?> SafeCacheAsync<T>(Func<Task<T?>> operation)
    {
        try
        {
            return await operation();
        }
        catch
        {
            return default;
        }
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
