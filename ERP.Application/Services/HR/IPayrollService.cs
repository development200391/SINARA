using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;

namespace ERP.Application.Services.HR;

public interface IPayrollService
{
    Task<PagedResult<PayrollRunDto>> GetRunsAsync(PayrollRunPagedRequest request, CancellationToken ct = default);
    Task<PayrollRunDto> RunPayrollAsync(int month, int year, int processedByUserId, CancellationToken ct = default);
    Task<IReadOnlyList<PayrollRunDetailDto>> GetRunDetailsAsync(int runId, CancellationToken ct = default);
    Task<PayslipDto?> GetPayslipAsync(int runId, int employeeId, CancellationToken ct = default);
}
