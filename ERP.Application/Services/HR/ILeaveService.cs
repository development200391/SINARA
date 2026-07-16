using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;

namespace ERP.Application.Services.HR;

public interface ILeaveService
{
    /// <summary>
    /// <paramref name="currentUserId"/> is optional and only used to populate <see cref="LeaveRequestDto.CanApprove"/>
    /// per row (see ApprovalRequestService.GetActionablePermissionsAsync) — omit it when the caller doesn't
    /// need that field (e.g. internal reuse right after Submit/Update).
    /// </summary>
    Task<PagedResult<LeaveRequestDto>> GetRequestsAsync(LeaveRequestPagedRequest request, int? currentUserId = null, CancellationToken ct = default);
    Task<LeaveRequestDto?> GetByIdAsync(int id, int? currentUserId = null, CancellationToken ct = default);
    Task<LeaveRequestDto> SubmitAsync(SubmitLeaveRequest request, CancellationToken ct = default);
    Task<LeaveRequestDto?> UpdateAsync(int id, SubmitLeaveRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<bool> ApproveAsync(int leaveRequestId, int approverUserId, CancellationToken ct = default);
    Task<bool> RejectAsync(int leaveRequestId, int approverUserId, CancellationToken ct = default);

    Task<PagedResult<LeaveBalanceDto>> GetBalancesAsync(LeaveBalanceRequest request, CancellationToken ct = default);

    Task<PagedResult<LeaveTypeDto>> GetLeaveTypesAsync(LeaveTypePagedRequest request, CancellationToken ct = default);
    Task<LeaveTypeDto?> GetLeaveTypeByIdAsync(int id, CancellationToken ct = default);
    Task<LeaveTypeDto> CreateLeaveTypeAsync(LeaveTypeDto request, CancellationToken ct = default);
    Task<LeaveTypeDto?> UpdateLeaveTypeAsync(int id, LeaveTypeDto request, CancellationToken ct = default);
    Task<bool> DeleteLeaveTypeAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// <paramref name="currentUserId"/> is optional — when provided, the result is scoped to what
    /// that user may submit/manage leave for (Super Admin/HR Manager/HR Staff see everyone; anyone
    /// else sees only themselves and employees in a department they manage). Omit it for callers
    /// that intentionally need the unrestricted list (e.g. Leave Balance's employee filter).
    /// </summary>
    Task<IReadOnlyList<LookupDto>> GetEmployeeOptionsAsync(int? currentUserId = null, CancellationToken ct = default);
    Task<IReadOnlyList<LookupDto>> GetLeaveTypeOptionsAsync(CancellationToken ct = default);
}

