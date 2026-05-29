using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;

namespace ERP.Application.Services.HR;

public interface IHolidayService
{
    Task<PagedResult<HolidayDto>> GetPagedAsync(HolidayPagedRequest request, CancellationToken ct = default);
    Task<HolidayDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<HolidayDto> CreateAsync(HolidayDto request, CancellationToken ct = default);
    Task<HolidayDto?> UpdateAsync(int id, HolidayDto request, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
