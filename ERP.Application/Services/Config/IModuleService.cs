using ERP.Application.DTOs.Config;

namespace ERP.Application.Services.Config;

public interface IModuleService
{
    Task<IReadOnlyList<ModuleDto>> GetAllAsync(CancellationToken ct = default);
    Task<ModuleDto?> UpdateAsync(int id, ModuleDto request, CancellationToken ct = default);
}
