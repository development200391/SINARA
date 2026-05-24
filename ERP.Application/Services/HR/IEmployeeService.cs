using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;

namespace ERP.Application.Services.HR;

public interface IEmployeeService
{
    Task<PagedResult<EmployeeListDto>> GetPagedAsync(PagedRequest request, CancellationToken ct = default);
    Task<EmployeeDetailDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<EmployeeDetailDto> CreateAsync(CreateEmployeeRequest request, CancellationToken ct = default);
    Task<EmployeeDetailDto?> UpdateAsync(int id, UpdateEmployeeRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
