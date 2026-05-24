using ERP.Application.DTOs.Config;

namespace ERP.Application.Services.Config;

public interface IAppSettingsService
{
    Task<AppSettingsDto> GetAsync(CancellationToken ct = default);
    Task<AppSettingsDto> UpdateAsync(AppSettingsDto request, CancellationToken ct = default);
}
