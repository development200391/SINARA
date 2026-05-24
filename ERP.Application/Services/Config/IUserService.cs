using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Config;

namespace ERP.Application.Services.Config;

public interface IUserService
{
    Task<PagedResult<UserDto>> GetPagedAsync(PagedRequest request, CancellationToken ct = default);
    Task<UserDto> CreateAsync(UserDto request, CancellationToken ct = default);
    Task<UserDto?> UpdateAsync(int id, UserDto request, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
