using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;

namespace ERP.Application.Services.HR;

public interface ILeaveService
{
    Task<PagedResult<LeaveRequestDto>> GetRequestsAsync(LeaveRequestPagedRequest request, CancellationToken ct = default);
    Task<LeaveRequestDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<LeaveRequestDto> SubmitAsync(SubmitLeaveRequest request, CancellationToken ct = default);
    Task<LeaveRequestDto?> UpdateAsync(int id, SubmitLeaveRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<bool> ApproveAsync(int leaveRequestId, int approverUserId, CancellationToken ct = default);
    Task<bool> RejectAsync(int leaveRequestId, int approverUserId, CancellationToken ct = default);

    Task<PagedResult<LeaveBalanceDto>> GetBalancesAsync(LeaveBalanceRequest request, CancellationToken ct = default);

    Task<PagedResult<LeaveTypeDto>> GetLeaveTypesAsync(PagedRequest request, CancellationToken ct = default);
    Task<LeaveTypeDto?> GetLeaveTypeByIdAsync(int id, CancellationToken ct = default);
    Task<LeaveTypeDto> CreateLeaveTypeAsync(LeaveTypeDto request, CancellationToken ct = default);
    Task<LeaveTypeDto?> UpdateLeaveTypeAsync(int id, LeaveTypeDto request, CancellationToken ct = default);
    Task<bool> DeleteLeaveTypeAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<LookupDto>> GetEmployeeOptionsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<LookupDto>> GetLeaveTypeOptionsAsync(CancellationToken ct = default);
}
