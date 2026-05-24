using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;

namespace ERP.Web.Services;

public interface IHrApiClient
{
    Task<PagedResult<LeaveRequestDto>?> GetLeaveRequestsAsync(string accessToken, LeaveRequestPagedRequest request, CancellationToken ct = default);
    Task<LeaveRequestOptionsDto?> GetLeaveRequestOptionsAsync(string accessToken, CancellationToken ct = default);
    Task<LeaveRequestDto?> SubmitLeaveRequestAsync(string accessToken, SubmitLeaveRequest request, CancellationToken ct = default);
    Task<bool> ApproveLeaveRequestAsync(string accessToken, int id, CancellationToken ct = default);
    Task<bool> RejectLeaveRequestAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<LeaveBalanceDto>?> GetLeaveBalancesAsync(string accessToken, LeaveBalanceRequest request, CancellationToken ct = default);

    Task<PagedResult<LeaveTypeDto>?> GetLeaveTypesAsync(string accessToken, PagedRequest request, CancellationToken ct = default);
    Task<LeaveTypeDto?> GetLeaveTypeByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<LeaveTypeDto?> CreateLeaveTypeAsync(string accessToken, LeaveTypeDto request, CancellationToken ct = default);
    Task<LeaveTypeDto?> UpdateLeaveTypeAsync(string accessToken, int id, LeaveTypeDto request, CancellationToken ct = default);
    Task<bool> DeleteLeaveTypeAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<PayrollRunDto>?> GetPayrollRunsAsync(string accessToken, PayrollRunPagedRequest request, CancellationToken ct = default);
    Task<PayrollRunDto?> RunPayrollAsync(string accessToken, PayrollRunRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<PayrollRunDetailDto>> GetPayrollRunDetailsAsync(string accessToken, int runId, CancellationToken ct = default);
    Task<PayslipDto?> GetPayslipAsync(string accessToken, int runId, int employeeId, CancellationToken ct = default);
}
