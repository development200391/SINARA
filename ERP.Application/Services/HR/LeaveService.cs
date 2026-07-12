using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;
using ERP.Domain.Entities.HR;
using ERP.Domain.Enums;
using ERP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Services.HR;

public sealed class LeaveService(IUnitOfWork unitOfWork) : ILeaveService
{
    public async Task<PagedResult<LeaveRequestDto>> GetRequestsAsync(LeaveRequestPagedRequest request, CancellationToken ct = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var query = unitOfWork.Repository<HrLeaveRequest>()
            .Query()
            .AsNoTracking()
            .Include(x => x.Employee)
            .Include(x => x.LeaveType)
            .Include(x => x.ApprovedByUser)
            .AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (request.EmployeeId.HasValue)
        {
            query = query.Where(x => x.EmployeeId == request.EmployeeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Employee.FullName.ToLower().Contains(search) ||
                x.Employee.EmployeeCode.ToLower().Contains(search) ||
                x.LeaveType.Name.ToLower().Contains(search) ||
                (x.Reason != null && x.Reason.ToLower().Contains(search)));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapLeaveRequest(x))
            .ToListAsync(ct);

        return PagedResult<LeaveRequestDto>.Create(items, total, page, pageSize);
    }

    public async Task<LeaveRequestDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await unitOfWork.Repository<HrLeaveRequest>()
            .Query()
            .AsNoTracking()
            .Include(x => x.Employee)
            .Include(x => x.LeaveType)
            .Include(x => x.ApprovedByUser)
            .Where(x => x.Id == id)
            .Select(x => MapLeaveRequest(x))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<LeaveRequestDto> SubmitAsync(SubmitLeaveRequest request, CancellationToken ct = default)
    {
        if (request.EmployeeId <= 0)
        {
            throw new InvalidOperationException("Employee is required.");
        }

        if (request.LeaveTypeId <= 0)
        {
            throw new InvalidOperationException("Leave type is required.");
        }

        if (request.EndDate < request.StartDate)
        {
            throw new InvalidOperationException("End date cannot be earlier than start date.");
        }

        var employee = await unitOfWork.Repository<HrEmployee>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.EmployeeId, ct);

        if (employee is null)
        {
            throw new InvalidOperationException("Employee not found.");
        }

        var leaveType = await unitOfWork.Repository<HrLeaveType>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.LeaveTypeId && x.IsActive, ct);

        if (leaveType is null)
        {
            throw new InvalidOperationException("Leave type not found or inactive.");
        }

        var overlapExists = await unitOfWork.Repository<HrLeaveRequest>()
            .Query()
            .AnyAsync(x =>
                x.EmployeeId == request.EmployeeId &&
                x.Status == LeaveStatus.Pending &&
                x.StartDate <= request.EndDate &&
                x.EndDate >= request.StartDate,
                ct);

        if (overlapExists)
        {
            throw new InvalidOperationException("Employee already has pending leave in selected period.");
        }

        var totalDays = request.EndDate.DayNumber - request.StartDate.DayNumber + 1;

        var usedDays = await unitOfWork.Repository<HrLeaveRequest>()
            .Query()
            .AsNoTracking()
            .Where(x =>
                x.EmployeeId == request.EmployeeId &&
                x.LeaveTypeId == request.LeaveTypeId &&
                x.StartDate.Year == request.StartDate.Year &&
                (x.Status == LeaveStatus.Approved || x.Status == LeaveStatus.Pending))
            .SumAsync(x => x.TotalDays, ct);

        if (usedDays + totalDays > leaveType.MaxDaysPerYear)
        {
            var remaining = leaveType.MaxDaysPerYear - usedDays;
            throw new InvalidOperationException(
                $"Leave quota exceeded. {leaveType.Name} has {(remaining < 0 ? 0 : remaining)} day(s) remaining for {request.StartDate.Year}.");
        }

        var entity = new HrLeaveRequest
        {
            EmployeeId = request.EmployeeId,
            LeaveTypeId = request.LeaveTypeId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TotalDays = totalDays,
            Reason = request.Reason,
            Status = LeaveStatus.Pending,
            CreatedBy = "system"
        };

        await unitOfWork.Repository<HrLeaveRequest>().AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(entity.Id, ct)
            ?? throw new InvalidOperationException("Failed to load submitted leave request.");
    }

    public async Task<LeaveRequestDto?> UpdateAsync(int id, SubmitLeaveRequest request, CancellationToken ct = default)
    {
        if (request.EmployeeId <= 0)
        {
            throw new InvalidOperationException("Employee is required.");
        }

        if (request.LeaveTypeId <= 0)
        {
            throw new InvalidOperationException("Leave type is required.");
        }

        if (request.EndDate < request.StartDate)
        {
            throw new InvalidOperationException("End date cannot be earlier than start date.");
        }

        var entity = await unitOfWork.Repository<HrLeaveRequest>()
            .Query()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return null;
        }

        if (entity.Status != LeaveStatus.Pending)
        {
            throw new InvalidOperationException("Only pending leave request can be updated.");
        }

        var employee = await unitOfWork.Repository<HrEmployee>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.EmployeeId, ct);

        if (employee is null)
        {
            throw new InvalidOperationException("Employee not found.");
        }

        var leaveType = await unitOfWork.Repository<HrLeaveType>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.LeaveTypeId && x.IsActive, ct);

        if (leaveType is null)
        {
            throw new InvalidOperationException("Leave type not found or inactive.");
        }

        var overlapExists = await unitOfWork.Repository<HrLeaveRequest>()
            .Query()
            .AnyAsync(x =>
                x.Id != id &&
                x.EmployeeId == request.EmployeeId &&
                x.Status == LeaveStatus.Pending &&
                x.StartDate <= request.EndDate &&
                x.EndDate >= request.StartDate,
                ct);

        if (overlapExists)
        {
            throw new InvalidOperationException("Employee already has pending leave in selected period.");
        }

        var totalDays = request.EndDate.DayNumber - request.StartDate.DayNumber + 1;

        entity.EmployeeId = request.EmployeeId;
        entity.LeaveTypeId = request.LeaveTypeId;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.TotalDays = totalDays;
        entity.Reason = request.Reason;
        entity.UpdatedBy = "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        unitOfWork.Repository<HrLeaveRequest>().Update(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(entity.Id, ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<HrLeaveRequest>()
            .Query()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return false;
        }

        if (entity.Status != LeaveStatus.Pending)
        {
            throw new InvalidOperationException("Only pending leave request can be deleted.");
        }

        unitOfWork.Repository<HrLeaveRequest>().Delete(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }
    public async Task<bool> ApproveAsync(int leaveRequestId, int approverUserId, CancellationToken ct = default)
    {
        var request = await unitOfWork.Repository<HrLeaveRequest>()
            .Query()
            .FirstOrDefaultAsync(x => x.Id == leaveRequestId, ct);

        if (request is null || request.Status != LeaveStatus.Pending)
        {
            return false;
        }

        request.Status = LeaveStatus.Approved;
        request.ApprovedBy = approverUserId;
        request.ApprovedAt = DateTimeOffset.UtcNow;
        request.UpdatedBy = "system";
        request.UpdatedAt = DateTimeOffset.UtcNow;

        unitOfWork.Repository<HrLeaveRequest>().Update(request);
        await SyncApprovedLeaveToAttendanceAsync(request, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    private async Task SyncApprovedLeaveToAttendanceAsync(HrLeaveRequest request, CancellationToken ct)
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

    public async Task<bool> RejectAsync(int leaveRequestId, int approverUserId, CancellationToken ct = default)
    {
        var request = await unitOfWork.Repository<HrLeaveRequest>()
            .Query()
            .FirstOrDefaultAsync(x => x.Id == leaveRequestId, ct);

        if (request is null || request.Status != LeaveStatus.Pending)
        {
            return false;
        }

        request.Status = LeaveStatus.Rejected;
        request.ApprovedBy = approverUserId;
        request.ApprovedAt = DateTimeOffset.UtcNow;
        request.UpdatedBy = "system";
        request.UpdatedAt = DateTimeOffset.UtcNow;

        unitOfWork.Repository<HrLeaveRequest>().Update(request);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    public async Task<PagedResult<LeaveBalanceDto>> GetBalancesAsync(LeaveBalanceRequest request, CancellationToken ct = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var year = request.Year ?? DateTime.UtcNow.Year;
        var normalizedSortBy = NormalizeLeaveBalanceSortBy(request.SortBy);
        var normalizedSortDirection = NormalizeSortDirection(request.SortDirection);

        var employeesQuery = unitOfWork.Repository<HrEmployee>()
            .Query()
            .AsNoTracking()
            .Where(x => x.EmploymentStatus != EmploymentStatus.Inactive)
            .AsQueryable();

        if (request.EmployeeId.HasValue)
        {
            employeesQuery = employeesQuery.Where(x => x.Id == request.EmployeeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            employeesQuery = employeesQuery.Where(x =>
                x.FullName.ToLower().Contains(search) ||
                x.EmployeeCode.ToLower().Contains(search));
        }

        var employees = await employeesQuery
            .Select(x => new { x.Id, x.EmployeeCode, x.FullName })
            .OrderBy(x => x.FullName)
            .ToListAsync(ct);

        var leaveTypesQuery = unitOfWork.Repository<HrLeaveType>()
            .Query()
            .AsNoTracking()
            .Where(x => x.IsActive)
            .AsQueryable();

        if (request.LeaveTypeId.HasValue)
        {
            leaveTypesQuery = leaveTypesQuery.Where(x => x.Id == request.LeaveTypeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            leaveTypesQuery = leaveTypesQuery.Where(x =>
                x.Name.ToLower().Contains(search) ||
                x.Code.ToLower().Contains(search));
        }

        var leaveTypes = await leaveTypesQuery
            .Select(x => new { x.Id, x.Code, x.Name, x.MaxDaysPerYear })
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

        var usedDays = await unitOfWork.Repository<HrLeaveRequest>()
            .Query()
            .AsNoTracking()
            .Where(x => x.Status == LeaveStatus.Approved && x.StartDate.Year == year)
            .GroupBy(x => new { x.EmployeeId, x.LeaveTypeId })
            .Select(g => new
            {
                g.Key.EmployeeId,
                g.Key.LeaveTypeId,
                UsedDays = g.Sum(x => x.TotalDays)
            })
            .ToListAsync(ct);

        var usedLookup = usedDays.ToDictionary(
            x => (x.EmployeeId, x.LeaveTypeId),
            x => x.UsedDays);

        var rows = new List<LeaveBalanceDto>();
        foreach (var employee in employees)
        {
            foreach (var leaveType in leaveTypes)
            {
                var used = usedLookup.GetValueOrDefault((employee.Id, leaveType.Id));
                var remaining = leaveType.MaxDaysPerYear - used;

                rows.Add(new LeaveBalanceDto
                {
                    EmployeeId = employee.Id,
                    EmployeeCode = employee.EmployeeCode,
                    EmployeeName = employee.FullName,
                    LeaveTypeId = leaveType.Id,
                    LeaveTypeCode = leaveType.Code,
                    LeaveTypeName = leaveType.Name,
                    Year = year,
                    MaxDaysPerYear = leaveType.MaxDaysPerYear,
                    UsedDays = used,
                    RemainingDays = remaining < 0 ? 0 : remaining
                });
            }
        }

        var total = rows.Count;

        var sortedRows = ApplyLeaveBalanceSorting(rows, normalizedSortBy, normalizedSortDirection);

        var paged = sortedRows
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return PagedResult<LeaveBalanceDto>.Create(paged, total, page, pageSize);
    }

    public async Task<PagedResult<LeaveTypeDto>> GetLeaveTypesAsync(LeaveTypePagedRequest request, CancellationToken ct = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var normalizedSortBy = NormalizeLeaveTypeSortBy(request.SortBy);
        var normalizedSortDirection = NormalizeSortDirection(request.SortDirection);
        var normalizedName = NormalizeText(request.Name);
        var normalizedCode = NormalizeText(request.Code);
        var (normalizedMaxDaysFrom, normalizedMaxDaysTo) = NormalizeIntRange(request.MaxDaysPerYearFrom, request.MaxDaysPerYearTo);

        var query = unitOfWork.Repository<HrLeaveType>()
            .Query()
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Name.ToLower().Contains(search) ||
                x.Code.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(normalizedName))
        {
            var name = normalizedName.ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(normalizedCode))
        {
            var code = normalizedCode.ToLowerInvariant();
            query = query.Where(x => x.Code.ToLower().Contains(code));
        }

        if (request.IsCarryOver.HasValue)
        {
            query = query.Where(x => x.IsCarryOver == request.IsCarryOver.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        if (normalizedMaxDaysFrom.HasValue)
        {
            query = query.Where(x => x.MaxDaysPerYear >= normalizedMaxDaysFrom.Value);
        }

        if (normalizedMaxDaysTo.HasValue)
        {
            query = query.Where(x => x.MaxDaysPerYear <= normalizedMaxDaysTo.Value);
        }

        query = ApplyLeaveTypeSorting(query, normalizedSortBy, normalizedSortDirection);

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new LeaveTypeDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                MaxDaysPerYear = x.MaxDaysPerYear,
                IsCarryOver = x.IsCarryOver,
                IsActive = x.IsActive
            })
            .ToListAsync(ct);

        return PagedResult<LeaveTypeDto>.Create(items, total, page, pageSize);
    }

    public async Task<LeaveTypeDto?> GetLeaveTypeByIdAsync(int id, CancellationToken ct = default)
    {
        return await unitOfWork.Repository<HrLeaveType>()
            .Query()
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new LeaveTypeDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                MaxDaysPerYear = x.MaxDaysPerYear,
                IsCarryOver = x.IsCarryOver,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<LeaveTypeDto> CreateLeaveTypeAsync(LeaveTypeDto request, CancellationToken ct = default)
    {
        var exists = await unitOfWork.Repository<HrLeaveType>()
            .Query()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Code == request.Code, ct);

        if (exists)
        {
            throw new InvalidOperationException("Leave type code already exists.");
        }

        var entity = new HrLeaveType
        {
            Name = request.Name,
            Code = request.Code,
            MaxDaysPerYear = request.MaxDaysPerYear,
            IsCarryOver = request.IsCarryOver,
            IsActive = request.IsActive,
            CreatedBy = "system"
        };

        await unitOfWork.Repository<HrLeaveType>().AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return await GetLeaveTypeByIdAsync(entity.Id, ct)
            ?? throw new InvalidOperationException("Failed to load created leave type.");
    }

    public async Task<LeaveTypeDto?> UpdateLeaveTypeAsync(int id, LeaveTypeDto request, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<HrLeaveType>()
            .Query()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return null;
        }

        var duplicateCode = await unitOfWork.Repository<HrLeaveType>()
            .Query()
            .AnyAsync(x => x.Id != id && x.Code == request.Code, ct);

        if (duplicateCode)
        {
            throw new InvalidOperationException("Leave type code already exists.");
        }

        entity.Name = request.Name;
        entity.Code = request.Code;
        entity.MaxDaysPerYear = request.MaxDaysPerYear;
        entity.IsCarryOver = request.IsCarryOver;
        entity.IsActive = request.IsActive;
        entity.UpdatedBy = "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        unitOfWork.Repository<HrLeaveType>().Update(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return await GetLeaveTypeByIdAsync(entity.Id, ct);
    }

    public async Task<bool> DeleteLeaveTypeAsync(int id, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<HrLeaveType>()
            .Query()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return false;
        }

        var hasRequests = await unitOfWork.Repository<HrLeaveRequest>()
            .Query()
            .AnyAsync(x => x.LeaveTypeId == id, ct);

        if (hasRequests)
        {
            throw new InvalidOperationException("Leave type cannot be deleted because it is already used by leave requests.");
        }

        unitOfWork.Repository<HrLeaveType>().Delete(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    public async Task<IReadOnlyList<LookupDto>> GetEmployeeOptionsAsync(CancellationToken ct = default)
    {
        return await unitOfWork.Repository<HrEmployee>()
            .Query()
            .AsNoTracking()
            .Where(x => x.EmploymentStatus != EmploymentStatus.Inactive)
            .OrderBy(x => x.FullName)
            .Select(x => new LookupDto
            {
                Id = x.Id,
                Name = x.EmployeeCode + " - " + x.FullName
            })
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<LookupDto>> GetLeaveTypeOptionsAsync(CancellationToken ct = default)
    {
        return await unitOfWork.Repository<HrLeaveType>()
            .Query()
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new LookupDto
            {
                Id = x.Id,
                Name = x.Code + " - " + x.Name
            })
            .ToListAsync(ct);
    }

    private static IEnumerable<LeaveBalanceDto> ApplyLeaveBalanceSorting(
        IEnumerable<LeaveBalanceDto> rows,
        string sortBy,
        string sortDirection)
    {
        var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy switch
        {
            "leavetype" => isDesc
                ? rows.OrderByDescending(x => x.LeaveTypeName).ThenByDescending(x => x.EmployeeName)
                : rows.OrderBy(x => x.LeaveTypeName).ThenBy(x => x.EmployeeName),

            "year" => isDesc
                ? rows.OrderByDescending(x => x.Year).ThenByDescending(x => x.EmployeeName)
                : rows.OrderBy(x => x.Year).ThenBy(x => x.EmployeeName),

            "maxdays" => isDesc
                ? rows.OrderByDescending(x => x.MaxDaysPerYear).ThenByDescending(x => x.EmployeeName)
                : rows.OrderBy(x => x.MaxDaysPerYear).ThenBy(x => x.EmployeeName),

            "useddays" => isDesc
                ? rows.OrderByDescending(x => x.UsedDays).ThenByDescending(x => x.EmployeeName)
                : rows.OrderBy(x => x.UsedDays).ThenBy(x => x.EmployeeName),

            "remainingdays" => isDesc
                ? rows.OrderByDescending(x => x.RemainingDays).ThenByDescending(x => x.EmployeeName)
                : rows.OrderBy(x => x.RemainingDays).ThenBy(x => x.EmployeeName),

            _ => isDesc
                ? rows.OrderByDescending(x => x.EmployeeName).ThenByDescending(x => x.LeaveTypeName)
                : rows.OrderBy(x => x.EmployeeName).ThenBy(x => x.LeaveTypeName)
        };
    }

    private static IQueryable<HrLeaveType> ApplyLeaveTypeSorting(IQueryable<HrLeaveType> query, string sortBy, string sortDirection)
    {
        var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy switch
        {
            "code" => isDesc
                ? query.OrderByDescending(x => x.Code).ThenByDescending(x => x.Name)
                : query.OrderBy(x => x.Code).ThenBy(x => x.Name),

            "maxdays" => isDesc
                ? query.OrderByDescending(x => x.MaxDaysPerYear).ThenByDescending(x => x.Name)
                : query.OrderBy(x => x.MaxDaysPerYear).ThenBy(x => x.Name),

            "iscarryover" => isDesc
                ? query.OrderByDescending(x => x.IsCarryOver).ThenByDescending(x => x.Name)
                : query.OrderBy(x => x.IsCarryOver).ThenBy(x => x.Name),

            "isactive" => isDesc
                ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Name)
                : query.OrderBy(x => x.IsActive).ThenBy(x => x.Name),

            _ => isDesc
                ? query.OrderByDescending(x => x.Name).ThenByDescending(x => x.Code)
                : query.OrderBy(x => x.Name).ThenBy(x => x.Code)
        };
    }

    private static string NormalizeSortDirection(string? sortDirection) =>
        string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";

    private static string NormalizeLeaveBalanceSortBy(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return "employee";
        }

        return sortBy.Trim().ToLowerInvariant() switch
        {
            "employee" => "employee",
            "leavetype" => "leavetype",
            "year" => "year",
            "maxdays" => "maxdays",
            "useddays" => "useddays",
            "remainingdays" => "remainingdays",
            _ => "employee"
        };
    }

    private static string NormalizeLeaveTypeSortBy(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return "name";
        }

        return sortBy.Trim().ToLowerInvariant() switch
        {
            "name" => "name",
            "code" => "code",
            "maxdays" => "maxdays",
            "iscarryover" => "iscarryover",
            "isactive" => "isactive",
            _ => "name"
        };
    }

    private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static (int? From, int? To) NormalizeIntRange(int? from, int? to)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
        {
            return (to, from);
        }

        return (from, to);
    }
    private static LeaveRequestDto MapLeaveRequest(HrLeaveRequest x)
    {
        return new LeaveRequestDto
        {
            Id = x.Id,
            EmployeeId = x.EmployeeId,
            EmployeeCode = x.Employee.EmployeeCode,
            EmployeeName = x.Employee.FullName,
            LeaveTypeId = x.LeaveTypeId,
            LeaveTypeCode = x.LeaveType.Code,
            LeaveTypeName = x.LeaveType.Name,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            TotalDays = x.TotalDays,
            Reason = x.Reason,
            Status = x.Status,
            ApprovedBy = x.ApprovedBy,
            ApprovedByName = x.ApprovedByUser != null ? x.ApprovedByUser.FullName : null,
            ApprovedAt = x.ApprovedAt,
            CreatedAt = x.CreatedAt
        };
    }
}




