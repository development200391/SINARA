using ERP.Application.DTOs.Config;
using ERP.Domain.Entities.Config;
using ERP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Services.Config;

public sealed class ModuleService(IUnitOfWork unitOfWork, ICacheService cacheService) : IModuleService
{
    private const string ModulesCacheKey = "ERP_cfg:modules:all";
    private const string UserPermissionsKeyPrefix = "ERP_cfg:permissions:user:";

    private static readonly TimeSpan ModulesTtl = TimeSpan.FromMinutes(60);

    public async Task<IReadOnlyList<ModuleDto>> GetAllAsync(CancellationToken ct = default)
    {
        var cached = await SafeCacheAsync(() => cacheService.GetAsync<IReadOnlyList<ModuleDto>>(ModulesCacheKey, ct));
        if (cached is not null)
        {
            return cached;
        }

        var modules = await unitOfWork.Repository<CfgModule>()
            .Query()
            .AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new ModuleDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                Icon = x.Icon,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive
            })
            .ToListAsync(ct);

        await SafeCacheAsync(() => cacheService.SetAsync(ModulesCacheKey, modules, ModulesTtl, ct));

        return modules;
    }

    public async Task<ModuleDto?> UpdateAsync(int id, ModuleDto request, CancellationToken ct = default)
    {
        var module = await unitOfWork.Repository<CfgModule>()
            .Query()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (module is null)
        {
            return null;
        }

        module.Name = request.Name;
        module.Icon = request.Icon;
        module.SortOrder = request.SortOrder;
        module.IsActive = request.IsActive;
        module.UpdatedBy = "system";
        module.UpdatedAt = DateTimeOffset.UtcNow;

        unitOfWork.Repository<CfgModule>().Update(module);
        await unitOfWork.SaveChangesAsync(ct);

        await SafeCacheAsync(() => cacheService.RemoveAsync(ModulesCacheKey, ct));
        await SafeCacheAsync(() => cacheService.RemoveByPrefixAsync(UserPermissionsKeyPrefix, ct));

        return new ModuleDto
        {
            Id = module.Id,
            Name = module.Name,
            Code = module.Code,
            Icon = module.Icon,
            SortOrder = module.SortOrder,
            IsActive = module.IsActive
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
