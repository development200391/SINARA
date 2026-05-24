using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;

namespace ERP.Application.Services.HR;

public interface IAttendanceService
{
    Task<AttendanceDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<AttendanceDto>> GetByEmployeeAsync(int employeeId, CancellationToken ct = default);
    Task<AttendanceDto> RecordAsync(AttendanceRecordRequest request, CancellationToken ct = default);
    Task<AttendanceDto?> UpdateAsync(int id, AttendanceRecordRequest request, CancellationToken ct = default);
    Task<PagedResult<AttendanceReportDto>> GetReportAsync(AttendanceReportRequest request, CancellationToken ct = default);
}
