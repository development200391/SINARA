using ERP.Application.DTOs.Config;
using ERP.Domain.Entities.Config;
using ERP.Domain.Entities.System;
using ERP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Services.Config;

public sealed class MenuService(IUnitOfWork unitOfWork, ICacheService cacheService) : IMenuService
{
    private const string MenusByModuleKeyPrefix = "ERP_cfg:menus:module:";
    private const string RolePermissionsKeyPrefix = "ERP_cfg:permissions:role:";
    private const string UserPermissionsKeyPrefix = "ERP_cfg:permissions:user:";

    private static readonly TimeSpan MenusTtl = TimeSpan.FromMinutes(60);
    private static readonly TimeSpan RolePermissionsTtl = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan UserPermissionsTtl = TimeSpan.FromMinutes(15);

    public async Task<IReadOnlyList<NavigationModuleDto>> GetNavigationForUserAsync(int userId, CancellationToken ct = default)
    {
        var cacheKey = $"{UserPermissionsKeyPrefix}{userId}";

        var cached = await SafeCacheAsync(() => cacheService.GetAsync<IReadOnlyList<NavigationModuleDto>>(cacheKey, ct));
        if (cached is not null)
        {
            return cached;
        }

        var roleIds = await unitOfWork.Repository<SysUserRole>()
            .Query()
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.RoleId)
            .Distinct()
            .ToListAsync(ct);

        if (roleIds.Count == 0)
        {
            return [];
        }

        var viewableMenuIds = new HashSet<int>();
        foreach (var roleId in roleIds)
        {
            var roleMenuIds = await GetRoleViewMenuIdsAsync(roleId, ct);
            foreach (var menuId in roleMenuIds)
            {
                viewableMenuIds.Add(menuId);
            }
        }

        if (viewableMenuIds.Count == 0)
        {
            return [];
        }

        var modules = await unitOfWork.Repository<CfgModule>()
            .Query()
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(ct);

        var allMenus = await unitOfWork.Repository<CfgMenu>()
            .Query()
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(ct);

        var menuMap = allMenus.ToDictionary(x => x.Id);
        var visibleMenuIds = new HashSet<int>(viewableMenuIds);

        foreach (var menuId in viewableMenuIds)
        {
            var current = menuId;
            while (menuMap.TryGetValue(current, out var menu) && menu.ParentId.HasValue)
            {
                if (!visibleMenuIds.Add(menu.ParentId.Value))
                {
                    break;
                }

                current = menu.ParentId.Value;
            }
        }

        var moduleResults = modules
            .Select(module => new NavigationModuleDto
            {
                ModuleId = module.Id,
                ModuleName = module.Name,
                ModuleCode = module.Code,
                ModuleIcon = module.Icon,
                SortOrder = module.SortOrder,
                Menus = BuildMenuTree(allMenus, module.Id, null, visibleMenuIds)
            })
            .Where(x => x.Menus.Count > 0)
            .OrderBy(x => x.SortOrder)
            .ToList();

        await SafeCacheAsync(() => cacheService.SetAsync(cacheKey, moduleResults, UserPermissionsTtl, ct));

        return moduleResults;
    }

    public async Task<IReadOnlyList<MenuDto>> GetByModuleAsync(int moduleId, CancellationToken ct = default)
    {
        var cacheKey = $"{MenusByModuleKeyPrefix}{moduleId}";

        var cached = await SafeCacheAsync(() => cacheService.GetAsync<IReadOnlyList<MenuDto>>(cacheKey, ct));
        if (cached is not null)
        {
            return cached;
        }

        var menus = await unitOfWork.Repository<CfgMenu>()
            .Query()
            .AsNoTracking()
            .Where(x => x.ModuleId == moduleId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new MenuDto
            {
                Id = x.Id,
                ModuleId = x.ModuleId,
                ParentId = x.ParentId,
                Name = x.Name,
                Url = x.Url,
                Icon = x.Icon,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive
            })
            .ToListAsync(ct);

        await SafeCacheAsync(() => cacheService.SetAsync(cacheKey, menus, MenusTtl, ct));

        return menus;
    }

    public async Task<MenuDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await unitOfWork.Repository<CfgMenu>()
            .Query()
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new MenuDto
            {
                Id = x.Id,
                ModuleId = x.ModuleId,
                ParentId = x.ParentId,
                Name = x.Name,
                Url = x.Url,
                Icon = x.Icon,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<MenuDto> CreateAsync(MenuDto request, CancellationToken ct = default)
    {
        if (request.ModuleId <= 0)
        {
            throw new InvalidOperationException("Module is required.");
        }

        if (request.ParentId.HasValue)
        {
            var parent = await unitOfWork.Repository<CfgMenu>()
                .Query()
                .AsNoTracking()
                .Where(x => x.Id == request.ParentId.Value)
                .Select(x => new { x.Id, x.ModuleId })
                .FirstOrDefaultAsync(ct);

            if (parent is null || parent.ModuleId != request.ModuleId)
            {
                throw new InvalidOperationException("Parent menu is invalid.");
            }
        }

        var entity = new CfgMenu
        {
            ModuleId = request.ModuleId,
            ParentId = request.ParentId,
            Name = request.Name,
            Url = request.Url,
            Icon = request.Icon,
            SortOrder = request.SortOrder <= 0 ? 1 : request.SortOrder,
            IsActive = request.IsActive,
            CreatedBy = "system"
        };

        await unitOfWork.Repository<CfgMenu>().AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await InvalidateNavigationCachesAsync(request.ModuleId, ct);

        return MapMenu(entity);
    }

    public async Task<MenuDto?> UpdateAsync(int id, MenuDto request, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<CfgMenu>()
            .Query()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return null;
        }

        var originalModuleId = entity.ModuleId;
        var targetModuleId = request.ModuleId <= 0 ? entity.ModuleId : request.ModuleId;

        if (request.ParentId == id)
        {
            throw new InvalidOperationException("Menu cannot be its own parent.");
        }

        if (request.ParentId.HasValue)
        {
            var parent = await unitOfWork.Repository<CfgMenu>()
                .Query()
                .AsNoTracking()
                .Where(x => x.Id == request.ParentId.Value)
                .Select(x => new { x.Id, x.ModuleId })
                .FirstOrDefaultAsync(ct);

            if (parent is null || parent.ModuleId != targetModuleId)
            {
                throw new InvalidOperationException("Parent menu is invalid.");
            }

            var moduleMenus = await unitOfWork.Repository<CfgMenu>()
                .Query()
                .AsNoTracking()
                .Where(x => x.ModuleId == targetModuleId)
                .Select(x => new { x.Id, x.ParentId })
                .ToListAsync(ct);

            var parentMap = moduleMenus.ToDictionary(x => x.Id, x => x.ParentId);
            var visited = new HashSet<int>();
            var currentParentId = request.ParentId;

            while (currentParentId.HasValue)
            {
                if (!visited.Add(currentParentId.Value))
                {
                    throw new InvalidOperationException("Circular parent relationship detected.");
                }

                if (currentParentId.Value == id)
                {
                    throw new InvalidOperationException("Menu cannot use one of its descendants as parent.");
                }

                if (!parentMap.TryGetValue(currentParentId.Value, out var nextParentId))
                {
                    break;
                }

                currentParentId = nextParentId;
            }
        }

        entity.ModuleId = targetModuleId;
        entity.ParentId = request.ParentId;
        entity.Name = request.Name;
        entity.Url = request.Url;
        entity.Icon = request.Icon;
        entity.SortOrder = request.SortOrder <= 0 ? 1 : request.SortOrder;
        entity.IsActive = request.IsActive;
        entity.UpdatedBy = "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        unitOfWork.Repository<CfgMenu>().Update(entity);
        await unitOfWork.SaveChangesAsync(ct);

        await InvalidateNavigationCachesAsync(originalModuleId, ct);
        if (originalModuleId != entity.ModuleId)
        {
            await InvalidateNavigationCachesAsync(entity.ModuleId, ct);
        }

        return MapMenu(entity);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<CfgMenu>()
            .Query()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return false;
        }

        var hasChildren = await unitOfWork.Repository<CfgMenu>()
            .Query()
            .AnyAsync(x => x.ParentId == id, ct);

        if (hasChildren)
        {
            throw new InvalidOperationException("Cannot delete menu with existing child menus.");
        }

        var moduleId = entity.ModuleId;

        unitOfWork.Repository<CfgMenu>().Delete(entity);
        await unitOfWork.SaveChangesAsync(ct);

        await InvalidateNavigationCachesAsync(moduleId, ct);
        await SafeCacheAsync(() => cacheService.RemoveByPrefixAsync(RolePermissionsKeyPrefix, ct));

        return true;
    }

    public async Task<bool> ReorderAsync(int moduleId, IReadOnlyList<int> orderedMenuIds, CancellationToken ct = default)
    {
        if (orderedMenuIds.Count == 0)
        {
            return true;
        }

        var menus = await unitOfWork.Repository<CfgMenu>()
            .Query()
            .Where(x => x.ModuleId == moduleId && orderedMenuIds.Contains(x.Id))
            .ToListAsync(ct);

        var sortOrder = 1;
        var changed = false;

        foreach (var menuId in orderedMenuIds)
        {
            var menu = menus.FirstOrDefault(x => x.Id == menuId);
            if (menu is null)
            {
                continue;
            }

            if (menu.SortOrder != sortOrder)
            {
                menu.SortOrder = sortOrder;
                menu.UpdatedBy = "system";
                menu.UpdatedAt = DateTimeOffset.UtcNow;
                unitOfWork.Repository<CfgMenu>().Update(menu);
                changed = true;
            }

            sortOrder++;
        }

        if (changed)
        {
            await unitOfWork.SaveChangesAsync(ct);
            await InvalidateNavigationCachesAsync(moduleId, ct);
        }

        return true;
    }

    private async Task<IReadOnlyList<int>> GetRoleViewMenuIdsAsync(int roleId, CancellationToken ct)
    {
        var cacheKey = $"{RolePermissionsKeyPrefix}{roleId}";
        var cached = await SafeCacheAsync(() => cacheService.GetAsync<IReadOnlyList<int>>(cacheKey, ct));
        if (cached is not null)
        {
            return cached;
        }

        var menuIds = await unitOfWork.Repository<CfgRoleMenuPermission>()
            .Query()
            .AsNoTracking()
            .Where(x => x.RoleId == roleId && x.CanView)
            .Select(x => x.MenuId)
            .Distinct()
            .ToListAsync(ct);

        await SafeCacheAsync(() => cacheService.SetAsync(cacheKey, menuIds, RolePermissionsTtl, ct));

        return menuIds;
    }

    private static IReadOnlyList<NavigationMenuDto> BuildMenuTree(
        IReadOnlyList<CfgMenu> allMenus,
        int moduleId,
        int? parentId,
        HashSet<int> visibleMenuIds)
    {
        var nodes = allMenus
            .Where(x => x.ModuleId == moduleId && x.ParentId == parentId && visibleMenuIds.Contains(x.Id))
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new NavigationMenuDto
            {
                Id = x.Id,
                ParentId = x.ParentId,
                Name = x.Name,
                Url = x.Url,
                Icon = x.Icon,
                SortOrder = x.SortOrder,
                Children = BuildMenuTree(allMenus, moduleId, x.Id, visibleMenuIds)
            })
            .ToList();

        return nodes;
    }

    private async Task InvalidateNavigationCachesAsync(int moduleId, CancellationToken ct)
    {
        await SafeCacheAsync(() => cacheService.RemoveAsync($"{MenusByModuleKeyPrefix}{moduleId}", ct));
        await SafeCacheAsync(() => cacheService.RemoveByPrefixAsync(UserPermissionsKeyPrefix, ct));
    }

    private static MenuDto MapMenu(CfgMenu entity)
    {
        return new MenuDto
        {
            Id = entity.Id,
            ModuleId = entity.ModuleId,
            ParentId = entity.ParentId,
            Name = entity.Name,
            Url = entity.Url,
            Icon = entity.Icon,
            SortOrder = entity.SortOrder,
            IsActive = entity.IsActive
        };
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

