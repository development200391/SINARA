using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Config;
using ERP.Application.DTOs.HR;

namespace ERP.Application.Services.Config;

public interface IUserService
{
    Task<PagedResult<UserDto>> GetPagedAsync(UserPagedRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<LookupDto>> GetOptionsAsync(CancellationToken ct = default);
    Task<UserDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<UserDto> CreateAsync(UserDto request, CancellationToken ct = default);
    Task<UserDto?> UpdateAsync(int id, UserDto request, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
