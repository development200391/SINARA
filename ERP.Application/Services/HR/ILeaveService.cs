using ERP.Application.DTOs.HR;

namespace ERP.Application.Services.HR;

public interface ILeaveService
{
    Task<IReadOnlyList<LeaveRequestDto>> GetRequestsAsync(CancellationToken ct = default);
    Task<LeaveRequestDto> SubmitAsync(LeaveRequestDto request, CancellationToken ct = default);
    Task<bool> ApproveAsync(int leaveRequestId, int approverUserId, CancellationToken ct = default);
    Task<bool> RejectAsync(int leaveRequestId, int approverUserId, CancellationToken ct = default);
}
