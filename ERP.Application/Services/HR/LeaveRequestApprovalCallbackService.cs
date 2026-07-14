using ERP.Application.Services.Approval;
using ERP.Domain.Entities.HR;
using ERP.Domain.Enums;
using ERP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Services.HR;

/// <summary>
/// Applies the routing engine's final decision back onto the source HrLeaveRequest — kept as a
/// standalone class (not on LeaveService itself) so ApprovalRequestService's
/// IEnumerable&lt;IApprovalCallbackService&gt; dependency never cycles back into ILeaveService, which
/// itself depends on IApprovalRequestService to submit new requests.
/// </summary>
public sealed class LeaveRequestApprovalCallbackService(IUnitOfWork unitOfWork) : IApprovalCallbackService
{
    public string ReferenceType => "hr_leave_requests";

    public async Task OnApprovedAsync(int referenceId, int actorUserId, CancellationToken ct = default)
    {
        var request = await unitOfWork.Repository<HrLeaveRequest>().Query()
            .FirstOrDefaultAsync(x => x.Id == referenceId, ct);
        if (request is null || request.Status != LeaveStatus.Pending)
        {
            return;
        }

        request.Status = LeaveStatus.Approved;
        request.ApprovedBy = actorUserId;
        request.ApprovedAt = DateTimeOffset.UtcNow;
        unitOfWork.Repository<HrLeaveRequest>().Update(request);

        await LeaveAttendanceSyncHelper.SyncApprovedLeaveToAttendanceAsync(unitOfWork, request, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task OnRejectedAsync(int referenceId, int actorUserId, string? comment, CancellationToken ct = default)
    {
        var request = await unitOfWork.Repository<HrLeaveRequest>().Query()
            .FirstOrDefaultAsync(x => x.Id == referenceId, ct);
        if (request is null || request.Status != LeaveStatus.Pending)
        {
            return;
        }

        request.Status = LeaveStatus.Rejected;
        request.ApprovedBy = actorUserId;
        request.ApprovedAt = DateTimeOffset.UtcNow;
        unitOfWork.Repository<HrLeaveRequest>().Update(request);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public Task OnCancelledAsync(int referenceId, int actorUserId, CancellationToken ct = default)
        => OnRejectedAsync(referenceId, actorUserId, null, ct);
}
