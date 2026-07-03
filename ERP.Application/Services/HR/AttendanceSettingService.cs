using ERP.Application.DTOs.HR;
using ERP.Domain.Entities.HR;
using ERP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Services.HR;

public sealed class AttendanceSettingService(IUnitOfWork unitOfWork) : IAttendanceSettingService
{
    private const string SingletonKey = "default";

    public async Task<AttendanceSettingDto> GetAsync(CancellationToken ct = default)
    {
        var entity = await GetOrCreateEntityAsync(ct);
        return Map(entity);
    }

    public async Task<AttendanceSettingDto> UpdateAsync(AttendanceSettingDto request, CancellationToken ct = default)
    {
        if (request.AttendancePeriodStartDay is < 1 or > 31)
        {
            throw new InvalidOperationException("Attendance period start day must be between 1 and 31.");
        }

        if (request.AttendancePeriodEndDay is < 1 or > 31)
        {
            throw new InvalidOperationException("Attendance period end day must be between 1 and 31.");
        }

        if (request.CheckInToleranceMinutes < 0)
        {
            throw new InvalidOperationException("Check-in tolerance cannot be negative.");
        }

        if (request.MinimumOtMinutes < 0)
        {
            throw new InvalidOperationException("Minimum OT cannot be negative.");
        }

        if (request.WorkEnd <= request.WorkStart)
        {
            throw new InvalidOperationException("Work end time must be later than work start time.");
        }

        if (request.BreakEnd <= request.BreakStart)
        {
            throw new InvalidOperationException("Break end time must be later than break start time.");
        }

        if (request.BreakStart < request.WorkStart || request.BreakEnd > request.WorkEnd)
        {
            throw new InvalidOperationException("Break time must be within work schedule.");
        }

        if (request.RadiusMeters <= 0)
        {
            throw new InvalidOperationException("Radius must be greater than zero.");
        }

        var entity = await GetOrCreateEntityAsync(ct);

        entity.AttendancePeriodStartDay = request.AttendancePeriodStartDay;
        entity.AttendancePeriodEndDay = request.AttendancePeriodEndDay;
        entity.CheckInToleranceMinutes = request.CheckInToleranceMinutes;
        entity.WorkStart = request.WorkStart;
        entity.WorkEnd = request.WorkEnd;
        entity.BreakStart = request.BreakStart;
        entity.BreakEnd = request.BreakEnd;
        entity.MinimumOtMinutes = request.MinimumOtMinutes;
        entity.OfficeLatitude = request.OfficeLatitude;
        entity.OfficeLongitude = request.OfficeLongitude;
        entity.RadiusMeters = request.RadiusMeters;
        entity.UpdatedBy = "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        unitOfWork.Repository<HrAttendanceSetting>().Update(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return Map(entity);
    }

    private async Task<HrAttendanceSetting> GetOrCreateEntityAsync(CancellationToken ct)
    {
        var repository = unitOfWork.Repository<HrAttendanceSetting>();
        var entities = await repository
            .Query()
            .IgnoreQueryFilters()
            .OrderBy(x => x.Id)
            .ToListAsync(ct);

        var entity = entities.FirstOrDefault(x => string.Equals(x.SingletonKey, SingletonKey, StringComparison.OrdinalIgnoreCase))
            ?? entities.FirstOrDefault();

        if (entity is null)
        {
            entity = new HrAttendanceSetting
            {
                SingletonKey = SingletonKey,
                CreatedBy = "system"
            };

            await repository.AddAsync(entity, ct);
            await unitOfWork.SaveChangesAsync(ct);
            return entity;
        }

        var changed = false;

        if (entity.IsDeleted)
        {
            entity.IsDeleted = false;
            entity.DeletedAt = null;
            changed = true;
        }

        if (!string.Equals(entity.SingletonKey, SingletonKey, StringComparison.OrdinalIgnoreCase))
        {
            entity.SingletonKey = SingletonKey;
            changed = true;
        }

        foreach (var extra in entities.Where(x => x.Id != entity.Id && !x.IsDeleted))
        {
            repository.Delete(extra);
            changed = true;
        }

        if (changed)
        {
            entity.UpdatedBy = "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            repository.Update(entity);
            await unitOfWork.SaveChangesAsync(ct);
        }

        return entity;
    }

    private static AttendanceSettingDto Map(HrAttendanceSetting entity)
    {
        return new AttendanceSettingDto
        {
            AttendancePeriodStartDay = entity.AttendancePeriodStartDay,
            AttendancePeriodEndDay = entity.AttendancePeriodEndDay,
            CheckInToleranceMinutes = entity.CheckInToleranceMinutes,
            WorkStart = entity.WorkStart,
            WorkEnd = entity.WorkEnd,
            BreakStart = entity.BreakStart,
            BreakEnd = entity.BreakEnd,
            MinimumOtMinutes = entity.MinimumOtMinutes,
            OfficeLatitude = entity.OfficeLatitude,
            OfficeLongitude = entity.OfficeLongitude,
            RadiusMeters = entity.RadiusMeters
        };
    }
}



