using ERP.Application.DTOs.Config;

namespace ERP.Application.Services.Config;

public interface IMenuService
{
    Task<IReadOnlyList<NavigationModuleDto>> GetNavigationForUserAsync(int userId, CancellationToken ct = default);
    Task<IReadOnlyList<MenuDto>> GetByModuleAsync(int moduleId, CancellationToken ct = default);
    Task<MenuDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<MenuDto> CreateAsync(MenuDto request, CancellationToken ct = default);
    Task<MenuDto?> UpdateAsync(int id, MenuDto request, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<bool> ReorderAsync(int moduleId, IReadOnlyList<int> orderedMenuIds, CancellationToken ct = default);
}
