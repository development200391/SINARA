using ERP.Application.DTOs.HR;

namespace ERP.Application.Services.HR;

public interface IAttendanceService
{
    Task<IReadOnlyList<AttendanceDto>> GetByEmployeeAsync(int employeeId, CancellationToken ct = default);
    Task<AttendanceDto> RecordAsync(AttendanceDto request, CancellationToken ct = default);
    Task<IReadOnlyList<AttendanceDto>> GetReportAsync(DateOnly startDate, DateOnly endDate, CancellationToken ct = default);
}
