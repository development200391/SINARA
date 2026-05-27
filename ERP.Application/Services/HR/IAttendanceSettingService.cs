using ERP.Application.DTOs.HR;

namespace ERP.Application.Services.HR;

public interface IAttendanceSettingService
{
    Task<AttendanceSettingDto> GetAsync(CancellationToken ct = default);
    Task<AttendanceSettingDto> UpdateAsync(AttendanceSettingDto request, CancellationToken ct = default);
}
