using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;
using ERP.Domain.Entities.HR;
using ERP.Domain.Enums;
using ERP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Services.HR;

public sealed class AttendanceService(IUnitOfWork unitOfWork) : IAttendanceService
{
    private const string DefaultSortBy = "date";
    private static readonly TimeSpan LocalOffset = TimeSpan.FromHours(7);
    private static readonly AttendanceStatus[] SelfReportableStatuses =
    [
        AttendanceStatus.HalfDay,
        AttendanceStatus.Absent
    ];

    private static readonly AttendanceStatus[] LeaveManagedStatuses =
    [
        AttendanceStatus.Sick,
        AttendanceStatus.Cuti
    ];

    public async Task<PagedResult<AttendanceReportDto>> GetPagedAsync(AttendanceReportRequest request, CancellationToken ct = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var normalizedSortBy = NormalizeSortBy(request.SortBy);
        var normalizedSortDirection = NormalizeSortDirection(request.SortDirection);

        var normalizedEmployeeCode = NormalizeText(request.EmployeeCode);
        var normalizedEmployeeName = NormalizeText(request.EmployeeName);
        var normalizedNotes = NormalizeText(request.Notes);

        var (normalizedDateFrom, normalizedDateTo) = NormalizeDateRange(request.DateFrom, request.DateTo);
        var (normalizedCheckInFrom, normalizedCheckInTo) = NormalizeDateRange(request.CheckInFrom, request.CheckInTo);
        var (normalizedCheckOutFrom, normalizedCheckOutTo) = NormalizeDateRange(request.CheckOutFrom, request.CheckOutTo);

        var checkInFromUtc = ToStartOfDayUtc(normalizedCheckInFrom);
        var checkInToUtcExclusive = ToStartOfDayUtc(normalizedCheckInTo?.AddDays(1));
        var checkOutFromUtc = ToStartOfDayUtc(normalizedCheckOutFrom);
        var checkOutToUtcExclusive = ToStartOfDayUtc(normalizedCheckOutTo?.AddDays(1));

        var query = unitOfWork.Repository<HrAttendanceRecord>()
            .Query()
            .AsNoTracking()
            .Include(x => x.Employee)
            .ThenInclude(x => x.Department)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Employee.EmployeeCode.ToLower().Contains(search) ||
                x.Employee.FullName.ToLower().Contains(search) ||
                x.Employee.Department.Name.ToLower().Contains(search) ||
                (x.Notes != null && x.Notes.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(normalizedEmployeeCode))
        {
            var employeeCode = normalizedEmployeeCode.ToLowerInvariant();
            query = query.Where(x => x.Employee.EmployeeCode.ToLower().Contains(employeeCode));
        }

        if (!string.IsNullOrWhiteSpace(normalizedEmployeeName))
        {
            var employeeName = normalizedEmployeeName.ToLowerInvariant();
            query = query.Where(x => x.Employee.FullName.ToLower().Contains(employeeName));
        }

        if (request.EmployeeId.HasValue)
        {
            query = query.Where(x => x.EmployeeId == request.EmployeeId.Value);
        }

        if (request.DepartmentId.HasValue)
        {
            query = query.Where(x => x.Employee.DepartmentId == request.DepartmentId.Value);
        }

        if (normalizedDateFrom.HasValue)
        {
            query = query.Where(x => x.Date >= normalizedDateFrom.Value);
        }

        if (normalizedDateTo.HasValue)
        {
            query = query.Where(x => x.Date <= normalizedDateTo.Value);
        }

        if (checkInFromUtc.HasValue)
        {
            query = query.Where(x => x.CheckIn.HasValue && x.CheckIn.Value >= checkInFromUtc.Value);
        }

        if (checkInToUtcExclusive.HasValue)
        {
            query = query.Where(x => x.CheckIn.HasValue && x.CheckIn.Value < checkInToUtcExclusive.Value);
        }

        if (checkOutFromUtc.HasValue)
        {
            query = query.Where(x => x.CheckOut.HasValue && x.CheckOut.Value >= checkOutFromUtc.Value);
        }

        if (checkOutToUtcExclusive.HasValue)
        {
            query = query.Where(x => x.CheckOut.HasValue && x.CheckOut.Value < checkOutToUtcExclusive.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(normalizedNotes))
        {
            var notes = normalizedNotes.ToLowerInvariant();
            query = query.Where(x => x.Notes != null && x.Notes.ToLower().Contains(notes));
        }

        var total = await query.CountAsync(ct);

        query = ApplySorting(query, normalizedSortBy, normalizedSortDirection);

        var entities = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = entities
            .Select(MapAttendanceReport)
            .ToList();

        return PagedResult<AttendanceReportDto>.Create(items, total, page, pageSize);
    }

    public async Task<AttendanceDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<HrAttendanceRecord>()
            .Query()
            .AsNoTracking()
            .Include(x => x.Employee)
            .ThenInclude(x => x.Department)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return entity is null ? null : MapAttendance(entity);
    }

    public async Task<IReadOnlyList<AttendanceDto>> GetByEmployeeAsync(int employeeId, CancellationToken ct = default)
    {
        if (employeeId <= 0)
        {
            return [];
        }

        var entities = await unitOfWork.Repository<HrAttendanceRecord>()
            .Query()
            .AsNoTracking()
            .Include(x => x.Employee)
            .ThenInclude(x => x.Department)
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.CheckIn)
            .ToListAsync(ct);

        return entities
            .Select(MapAttendance)
            .ToList();
    }

    public async Task<AttendanceDto> CreateAsync(AttendanceRecordRequest request, CancellationToken ct = default)
    {
        if (LeaveManagedStatuses.Contains(request.Status))
        {
            throw new InvalidOperationException("Sick and Cuti status can only be set through an approved leave request.");
        }

        await ValidateEmployeeAsync(request.EmployeeId, ct);
        ValidateCheckInOut(request.CheckIn, request.CheckOut);

        var duplicate = await unitOfWork.Repository<HrAttendanceRecord>()
            .Query()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.EmployeeId == request.EmployeeId && x.Date == request.Date, ct);

        if (duplicate)
        {
            throw new InvalidOperationException("Attendance for this employee and date already exists.");
        }

        var entity = new HrAttendanceRecord
        {
            EmployeeId = request.EmployeeId,
            Date = request.Date,
            CheckIn = NormalizeDateTime(request.CheckIn),
            CheckOut = NormalizeDateTime(request.CheckOut),
            Status = request.Status,
            Notes = NormalizeText(request.Notes),
            CreatedBy = "system"
        };

        await unitOfWork.Repository<HrAttendanceRecord>().AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(entity.Id, ct)
            ?? throw new InvalidOperationException("Failed to load created attendance record.");
    }

    public async Task<AttendanceDto?> UpdateAsync(int id, AttendanceRecordRequest request, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<HrAttendanceRecord>()
            .Query()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return null;
        }

        if (LeaveManagedStatuses.Contains(request.Status) && entity.Status != request.Status)
        {
            throw new InvalidOperationException("Sick and Cuti status can only be set through an approved leave request.");
        }

        await ValidateEmployeeAsync(request.EmployeeId, ct);
        ValidateCheckInOut(request.CheckIn, request.CheckOut);

        var duplicate = await unitOfWork.Repository<HrAttendanceRecord>()
            .Query()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id != id && x.EmployeeId == request.EmployeeId && x.Date == request.Date, ct);

        if (duplicate)
        {
            throw new InvalidOperationException("Attendance for this employee and date already exists.");
        }

        entity.EmployeeId = request.EmployeeId;
        entity.Date = request.Date;
        entity.CheckIn = NormalizeDateTime(request.CheckIn);
        entity.CheckOut = NormalizeDateTime(request.CheckOut);
        entity.Status = request.Status;
        entity.Notes = NormalizeText(request.Notes);
        entity.UpdatedBy = "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        unitOfWork.Repository<HrAttendanceRecord>().Update(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<HrAttendanceRecord>()
            .Query()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return false;
        }

        unitOfWork.Repository<HrAttendanceRecord>().Delete(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    public async Task<AttendanceDto?> GetTodayAsync(int employeeId, CancellationToken ct = default)
    {
        var today = TodayLocal();

        var entity = await unitOfWork.Repository<HrAttendanceRecord>()
            .Query()
            .AsNoTracking()
            .Include(x => x.Employee)
            .ThenInclude(x => x.Department)
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.Date == today, ct);

        return entity is null ? null : MapAttendance(entity);
    }

    public async Task<AttendanceDto> CheckInAsync(int employeeId, decimal latitude, decimal longitude, CancellationToken ct = default)
    {
        var today = TodayLocal();

        var entity = await unitOfWork.Repository<HrAttendanceRecord>()
            .Query()
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.Date == today, ct);

        if (entity is not null && entity.CheckIn.HasValue)
        {
            throw new InvalidOperationException("You have already checked in today.");
        }

        await ValidateWithinOfficeRadiusAsync(latitude, longitude, ct);

        var now = DateTimeOffset.UtcNow;

        if (entity is null)
        {
            entity = new HrAttendanceRecord
            {
                EmployeeId = employeeId,
                Date = today,
                CheckIn = now,
                CheckInLatitude = latitude,
                CheckInLongitude = longitude,
                Status = AttendanceStatus.Present,
                CreatedBy = "system"
            };

            await unitOfWork.Repository<HrAttendanceRecord>().AddAsync(entity, ct);
        }
        else
        {
            entity.CheckIn = now;
            entity.CheckInLatitude = latitude;
            entity.CheckInLongitude = longitude;
            entity.UpdatedBy = "system";
            entity.UpdatedAt = now;
            unitOfWork.Repository<HrAttendanceRecord>().Update(entity);
        }

        await unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(entity.Id, ct)
            ?? throw new InvalidOperationException("Failed to load attendance record.");
    }

    public async Task<AttendanceDto> CheckOutAsync(int employeeId, decimal latitude, decimal longitude, CancellationToken ct = default)
    {
        var today = TodayLocal();

        var entity = await unitOfWork.Repository<HrAttendanceRecord>()
            .Query()
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.Date == today, ct);

        if (entity is null || !entity.CheckIn.HasValue)
        {
            throw new InvalidOperationException("You must check in before checking out.");
        }

        await ValidateWithinOfficeRadiusAsync(latitude, longitude, ct);

        var now = DateTimeOffset.UtcNow;

        entity.CheckOut = now;
        entity.CheckOutLatitude = latitude;
        entity.CheckOutLongitude = longitude;
        entity.UpdatedBy = "system";
        entity.UpdatedAt = now;

        unitOfWork.Repository<HrAttendanceRecord>().Update(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(entity.Id, ct)
            ?? throw new InvalidOperationException("Failed to load attendance record.");
    }

    public async Task<AttendanceDto> MarkStatusAsync(int employeeId, MarkAttendanceStatusRequest request, CancellationToken ct = default)
    {
        if (!SelfReportableStatuses.Contains(request.Status))
        {
            throw new InvalidOperationException("Status is not self-reportable.");
        }

        var entity = await unitOfWork.Repository<HrAttendanceRecord>()
            .Query()
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.Date == request.Date, ct);

        if (entity is not null && (entity.CheckIn.HasValue || entity.CheckOut.HasValue))
        {
            throw new InvalidOperationException("Cannot mark status for a day that already has check-in/check-out.");
        }

        var now = DateTimeOffset.UtcNow;
        var notes = NormalizeText(request.Notes);

        if (entity is null)
        {
            entity = new HrAttendanceRecord
            {
                EmployeeId = employeeId,
                Date = request.Date,
                Status = request.Status,
                Notes = notes,
                CreatedBy = "system"
            };

            await unitOfWork.Repository<HrAttendanceRecord>().AddAsync(entity, ct);
        }
        else
        {
            entity.Status = request.Status;
            entity.Notes = notes;
            entity.UpdatedBy = "system";
            entity.UpdatedAt = now;
            unitOfWork.Repository<HrAttendanceRecord>().Update(entity);
        }

        await unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(entity.Id, ct)
            ?? throw new InvalidOperationException("Failed to load attendance record.");
    }

    private async Task ValidateWithinOfficeRadiusAsync(decimal latitude, decimal longitude, CancellationToken ct)
    {
        var setting = await unitOfWork.Repository<HrAttendanceSetting>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

        if (setting?.OfficeLatitude is null || setting.OfficeLongitude is null)
        {
            return;
        }

        var distance = GeoUtils.DistanceMeters(setting.OfficeLatitude.Value, setting.OfficeLongitude.Value, latitude, longitude);

        if (distance > setting.RadiusMeters)
        {
            throw new InvalidOperationException(
                $"Anda berada {Math.Round(distance)}m dari kantor, maksimum {setting.RadiusMeters}m.");
        }
    }

    private static DateOnly TodayLocal() => DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(LocalOffset).DateTime);

    private async Task ValidateEmployeeAsync(int employeeId, CancellationToken ct)
    {
        if (employeeId <= 0)
        {
            throw new InvalidOperationException("Employee is required.");
        }

        var exists = await unitOfWork.Repository<HrEmployee>()
            .Query()
            .AsNoTracking()
            .AnyAsync(x => x.Id == employeeId, ct);

        if (!exists)
        {
            throw new InvalidOperationException("Employee not found.");
        }
    }

    private static void ValidateCheckInOut(DateTimeOffset? checkIn, DateTimeOffset? checkOut)
    {
        if (checkIn.HasValue && checkOut.HasValue && checkOut.Value < checkIn.Value)
        {
            throw new InvalidOperationException("Check-out cannot be earlier than check-in.");
        }
    }

    private static DateTimeOffset? NormalizeDateTime(DateTimeOffset? value)
    {
        return value?.ToUniversalTime();
    }

    private static IQueryable<HrAttendanceRecord> ApplySorting(IQueryable<HrAttendanceRecord> query, string sortBy, string sortDirection)
    {
        var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy switch
        {
            "id" => isDesc
                ? query.OrderByDescending(x => x.Id)
                : query.OrderBy(x => x.Id),

            "employeeCode" => isDesc
                ? query.OrderByDescending(x => x.Employee.EmployeeCode).ThenByDescending(x => x.Date)
                : query.OrderBy(x => x.Employee.EmployeeCode).ThenByDescending(x => x.Date),

            "employeeName" => isDesc
                ? query.OrderByDescending(x => x.Employee.FullName).ThenByDescending(x => x.Date)
                : query.OrderBy(x => x.Employee.FullName).ThenByDescending(x => x.Date),

            "departmentName" => isDesc
                ? query.OrderByDescending(x => x.Employee.Department.Name).ThenByDescending(x => x.Date)
                : query.OrderBy(x => x.Employee.Department.Name).ThenByDescending(x => x.Date),

            "checkIn" => isDesc
                ? query.OrderByDescending(x => x.CheckIn).ThenByDescending(x => x.Date)
                : query.OrderBy(x => x.CheckIn).ThenByDescending(x => x.Date),

            "checkOut" => isDesc
                ? query.OrderByDescending(x => x.CheckOut).ThenByDescending(x => x.Date)
                : query.OrderBy(x => x.CheckOut).ThenByDescending(x => x.Date),

            "status" => isDesc
                ? query.OrderByDescending(x => x.Status).ThenByDescending(x => x.Date)
                : query.OrderBy(x => x.Status).ThenByDescending(x => x.Date),

            "notes" => isDesc
                ? query.OrderByDescending(x => x.Notes).ThenByDescending(x => x.Date)
                : query.OrderBy(x => x.Notes).ThenByDescending(x => x.Date),

            _ => isDesc
                ? query.OrderByDescending(x => x.Date).ThenByDescending(x => x.Id)
                : query.OrderBy(x => x.Date).ThenBy(x => x.Id)
        };
    }

    private static string NormalizeSortBy(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return DefaultSortBy;
        }

        return sortBy.Trim().ToLowerInvariant() switch
        {
            "id" => "id",
            "employeecode" => "employeeCode",
            "employeename" => "employeeName",
            "departmentname" => "departmentName",
            "date" => "date",
            "checkin" => "checkIn",
            "checkout" => "checkOut",
            "status" => "status",
            "notes" => "notes",
            _ => DefaultSortBy
        };
    }

    private static string NormalizeSortDirection(string? sortDirection) =>
        string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";

    private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static (DateOnly? From, DateOnly? To) NormalizeDateRange(DateOnly? from, DateOnly? to)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
        {
            return (to, from);
        }

        return (from, to);
    }

    private static DateTimeOffset? ToStartOfDayUtc(DateOnly? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        return new DateTimeOffset(value.Value.Year, value.Value.Month, value.Value.Day, 0, 0, 0, TimeSpan.Zero);
    }

    private static AttendanceDto MapAttendance(HrAttendanceRecord entity)
    {
        return new AttendanceDto
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            EmployeeCode = entity.Employee?.EmployeeCode ?? string.Empty,
            EmployeeName = entity.Employee?.FullName ?? string.Empty,
            DepartmentId = entity.Employee?.DepartmentId ?? 0,
            DepartmentName = entity.Employee?.Department?.Name ?? string.Empty,
            Date = entity.Date,
            CheckIn = entity.CheckIn,
            CheckOut = entity.CheckOut,
            Status = entity.Status,
            Notes = entity.Notes
        };
    }

    private static AttendanceReportDto MapAttendanceReport(HrAttendanceRecord entity)
    {
        return new AttendanceReportDto
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            EmployeeCode = entity.Employee.EmployeeCode,
            EmployeeName = entity.Employee.FullName,
            DepartmentId = entity.Employee.DepartmentId,
            DepartmentName = entity.Employee.Department.Name,
            Date = entity.Date,
            CheckIn = entity.CheckIn,
            CheckOut = entity.CheckOut,
            Status = entity.Status,
            Notes = entity.Notes
        };
    }
}


