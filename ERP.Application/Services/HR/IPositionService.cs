using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;

namespace ERP.Application.Services.HR;

public interface IPositionService
{
    Task<PagedResult<PositionDto>> GetPagedAsync(PositionPagedRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<PositionDto>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<PositionDto>> GetByDepartmentAsync(int departmentId, CancellationToken ct = default);
    Task<PositionDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PositionDto> CreateAsync(PositionDto request, CancellationToken ct = default);
    Task<PositionDto?> UpdateAsync(int id, PositionDto request, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
