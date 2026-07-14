using ERP.Domain.Entities.HR;
using ERP.Domain.Enums;
using ERP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Services.HR;

internal static class LeaveAttendanceSyncHelper
{
    public static async Task SyncApprovedLeaveToAttendanceAsync(IUnitOfWork unitOfWork, HrLeaveRequest request, CancellationToken ct)
    {
        var attendanceRepository = unitOfWork.Repository<HrAttendanceRecord>();
        var existingRecords = await attendanceRepository
            .Query()
            .Where(x =>
                x.EmployeeId == request.EmployeeId &&
                x.Date >= request.StartDate &&
                x.Date <= request.EndDate)
            .ToListAsync(ct);

        for (var date = request.StartDate; date <= request.EndDate; date = date.AddDays(1))
        {
            var existing = existingRecords.FirstOrDefault(x => x.Date == date);
            if (existing is not null)
            {
                if (existing.CheckIn.HasValue || existing.CheckOut.HasValue)
                {
                    continue;
                }

                existing.Status = AttendanceStatus.Cuti;
                existing.Notes = request.Reason;
                existing.UpdatedBy = "system";
                existing.UpdatedAt = DateTimeOffset.UtcNow;
                attendanceRepository.Update(existing);
                continue;
            }

            await attendanceRepository.AddAsync(new HrAttendanceRecord
            {
                EmployeeId = request.EmployeeId,
                Date = date,
                Status = AttendanceStatus.Cuti,
                Notes = request.Reason,
                CreatedBy = "system"
            }, ct);
        }
    }
}
