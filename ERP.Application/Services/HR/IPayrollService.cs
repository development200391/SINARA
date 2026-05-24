using ERP.Application.DTOs.HR;

namespace ERP.Application.Services.HR;

public interface IPayrollService
{
    Task<PayrollRunDto> RunPayrollAsync(int month, int year, int processedByUserId, CancellationToken ct = default);
    Task<PayslipDto?> GetPayslipAsync(int runId, int employeeId, CancellationToken ct = default);
    Task<IReadOnlyList<PayrollRunDto>> GetRunsAsync(CancellationToken ct = default);
}
