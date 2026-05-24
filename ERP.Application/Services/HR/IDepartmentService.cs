using ERP.Application.DTOs.HR;

namespace ERP.Application.Services.HR;

public interface IDepartmentService
{
    Task<IReadOnlyList<DepartmentDto>> GetAllAsync(CancellationToken ct = default);
    Task<DepartmentDto> CreateAsync(DepartmentDto request, CancellationToken ct = default);
    Task<DepartmentDto?> UpdateAsync(int id, DepartmentDto request, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
