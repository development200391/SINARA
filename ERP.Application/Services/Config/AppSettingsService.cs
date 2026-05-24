using ERP.Application.DTOs.Config;
using ERP.Domain.Interfaces;

namespace ERP.Application.Services.Config;

public sealed class AppSettingsService(ICacheService cacheService) : IAppSettingsService
{
    private const string AppSettingsCacheKey = "ERP_sys:settings:app";
    private static readonly TimeSpan SettingsTtl = TimeSpan.FromMinutes(60);

    public async Task<AppSettingsDto> GetAsync(CancellationToken ct = default)
    {
        var cached = await SafeCacheAsync(() => cacheService.GetAsync<AppSettingsDto>(AppSettingsCacheKey, ct));
        if (cached is not null)
        {
            return cached;
        }

        var defaults = new AppSettingsDto();
        await SafeCacheAsync(() => cacheService.SetAsync(AppSettingsCacheKey, defaults, SettingsTtl, ct));
        return defaults;
    }

    public async Task<AppSettingsDto> UpdateAsync(AppSettingsDto request, CancellationToken ct = default)
    {
        var model = new AppSettingsDto
        {
            AppName = string.IsNullOrWhiteSpace(request.AppName) ? "SINARA ERP" : request.AppName.Trim(),
            LogoUrl = string.IsNullOrWhiteSpace(request.LogoUrl) ? null : request.LogoUrl.Trim(),
            DefaultLanguage = request.DefaultLanguage is "id" ? "id" : "en"
        };

        await SafeCacheAsync(() => cacheService.SetAsync(AppSettingsCacheKey, model, SettingsTtl, ct));

        return model;
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
