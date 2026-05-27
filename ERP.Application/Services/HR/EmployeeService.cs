using System.Globalization;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;
using ERP.Domain.Entities.HR;
using ERP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Services.HR;

public sealed class EmployeeService(IUnitOfWork unitOfWork) : IEmployeeService
{
    private const string DefaultSortBy = "fullName";
    private const string EmployeeCodePrefix = "EMP";
    private const int EmployeeCodeDigits = 7;

    public async Task<PagedResult<EmployeeListDto>> GetPagedAsync(EmployeePagedRequest request, CancellationToken ct = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var normalizedSortBy = NormalizeSortBy(request.SortBy);
        var normalizedSortDirection = NormalizeSortDirection(request.SortDirection);
        var normalizedEmployeeCode = NormalizeText(request.EmployeeCode);
        var normalizedFullName = NormalizeText(request.FullName);
        var normalizedEmail = NormalizeText(request.Email);
        var normalizedPhone = NormalizeText(request.Phone);
        var (normalizedHireDateFrom, normalizedHireDateTo) = NormalizeDateRange(request.HireDateFrom, request.HireDateTo);
        var (normalizedTerminationDateFrom, normalizedTerminationDateTo) = NormalizeDateRange(request.TerminationDateFrom, request.TerminationDateTo);

        var query = unitOfWork.Repository<HrEmployee>()
            .Query()
            .AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.Position)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.EmployeeCode.ToLower().Contains(search) ||
                x.FullName.ToLower().Contains(search) ||
                (x.Email != null && x.Email.ToLower().Contains(search)) ||
                (x.Phone != null && x.Phone.ToLower().Contains(search)) ||
                x.Department.Name.ToLower().Contains(search) ||
                x.Position.Name.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(normalizedEmployeeCode))
        {
            var employeeCode = normalizedEmployeeCode.ToLowerInvariant();
            query = query.Where(x => x.EmployeeCode.ToLower().Contains(employeeCode));
        }

        if (!string.IsNullOrWhiteSpace(normalizedFullName))
        {
            var fullName = normalizedFullName.ToLowerInvariant();
            query = query.Where(x => x.FullName.ToLower().Contains(fullName));
        }

        if (!string.IsNullOrWhiteSpace(normalizedEmail))
        {
            var email = normalizedEmail.ToLowerInvariant();
            query = query.Where(x => x.Email != null && x.Email.ToLower().Contains(email));
        }

        if (!string.IsNullOrWhiteSpace(normalizedPhone))
        {
            var phone = normalizedPhone.ToLowerInvariant();
            query = query.Where(x => x.Phone != null && x.Phone.ToLower().Contains(phone));
        }

        if (request.DepartmentId.HasValue)
        {
            query = query.Where(x => x.DepartmentId == request.DepartmentId.Value);
        }

        if (request.PositionId.HasValue)
        {
            query = query.Where(x => x.PositionId == request.PositionId.Value);
        }

        if (request.EmploymentStatus.HasValue)
        {
            query = query.Where(x => x.EmploymentStatus == request.EmploymentStatus.Value);
        }

        if (normalizedHireDateFrom.HasValue)
        {
            query = query.Where(x => x.HireDate >= normalizedHireDateFrom.Value);
        }

        if (normalizedHireDateTo.HasValue)
        {
            query = query.Where(x => x.HireDate <= normalizedHireDateTo.Value);
        }

        if (normalizedTerminationDateFrom.HasValue)
        {
            query = query.Where(x => x.TerminationDate.HasValue && x.TerminationDate.Value >= normalizedTerminationDateFrom.Value);
        }

        if (normalizedTerminationDateTo.HasValue)
        {
            query = query.Where(x => x.TerminationDate.HasValue && x.TerminationDate.Value <= normalizedTerminationDateTo.Value);
        }

        var total = await query.CountAsync(ct);

        query = ApplySorting(query, normalizedSortBy, normalizedSortDirection);

        var entities = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = entities
            .Select(MapEmployeeList)
            .ToList();

        return PagedResult<EmployeeListDto>.Create(items, total, page, pageSize);
    }

    public async Task<IReadOnlyList<LookupDto>> GetOptionsAsync(CancellationToken ct = default)
    {
        return await unitOfWork.Repository<HrEmployee>()
            .Query()
            .AsNoTracking()
            .OrderBy(x => x.FullName)
            .ThenBy(x => x.EmployeeCode)
            .Select(x => new LookupDto
            {
                Id = x.Id,
                Name = x.EmployeeCode + " - " + x.FullName
            })
            .ToListAsync(ct);
    }

    public async Task<EmployeeDetailDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<HrEmployee>()
            .Query()
            .AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.Position)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return entity is null ? null : MapEmployeeDetail(entity);
    }

    public async Task<EmployeeDetailDto> CreateAsync(CreateEmployeeRequest request, CancellationToken ct = default)
    {
        var generatedCode = await GenerateNextEmployeeCodeAsync(ct);
        var normalizedFullName = NormalizeRequiredText(request.FullName, "Employee full name is required.");
        var normalizedEmail = NormalizeText(request.Email);
        var normalizedPhone = NormalizeText(request.Phone);

        await ValidateDepartmentAndPositionAsync(request.DepartmentId, request.PositionId, ct);

        var entity = new HrEmployee
        {
            EmployeeCode = generatedCode,
            FullName = normalizedFullName,
            Email = normalizedEmail,
            Phone = normalizedPhone,
            DepartmentId = request.DepartmentId,
            PositionId = request.PositionId,
            HireDate = request.HireDate,
            EmploymentStatus = request.EmploymentStatus,
            CreatedBy = "system"
        };

        await unitOfWork.Repository<HrEmployee>().AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(entity.Id, ct)
            ?? throw new InvalidOperationException("Failed to load created employee.");
    }

    public async Task<EmployeeDetailDto?> UpdateAsync(int id, UpdateEmployeeRequest request, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<HrEmployee>()
            .Query()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return null;
        }

        var normalizedFullName = NormalizeRequiredText(request.FullName, "Employee full name is required.");
        var normalizedEmail = NormalizeText(request.Email);
        var normalizedPhone = NormalizeText(request.Phone);

        if (request.TerminationDate.HasValue && request.TerminationDate.Value < request.HireDate)
        {
            throw new InvalidOperationException("Termination date cannot be earlier than hire date.");
        }

        await ValidateDepartmentAndPositionAsync(request.DepartmentId, request.PositionId, ct);

        entity.FullName = normalizedFullName;
        entity.Email = normalizedEmail;
        entity.Phone = normalizedPhone;
        entity.DepartmentId = request.DepartmentId;
        entity.PositionId = request.PositionId;
        entity.HireDate = request.HireDate;
        entity.TerminationDate = request.TerminationDate;
        entity.EmploymentStatus = request.EmploymentStatus;
        entity.UpdatedBy = "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        unitOfWork.Repository<HrEmployee>().Update(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(entity.Id, ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<HrEmployee>()
            .Query()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return false;
        }

        var hasAttendance = await unitOfWork.Repository<HrAttendanceRecord>()
            .Query()
            .AnyAsync(x => x.EmployeeId == id, ct);

        if (hasAttendance)
        {
            throw new InvalidOperationException("Employee cannot be deleted because attendance records exist.");
        }

        var hasLeaveRequests = await unitOfWork.Repository<HrLeaveRequest>()
            .Query()
            .AnyAsync(x => x.EmployeeId == id, ct);

        if (hasLeaveRequests)
        {
            throw new InvalidOperationException("Employee cannot be deleted because leave requests exist.");
        }

        var hasPayrollDetails = await unitOfWork.Repository<HrPayrollDetail>()
            .Query()
            .AnyAsync(x => x.EmployeeId == id, ct);

        if (hasPayrollDetails)
        {
            throw new InvalidOperationException("Employee cannot be deleted because payroll records exist.");
        }

        var managedDepartments = await unitOfWork.Repository<HrDepartment>()
            .Query()
            .Where(x => x.ManagerId == id)
            .ToListAsync(ct);

        foreach (var department in managedDepartments)
        {
            department.ManagerId = null;
            department.UpdatedBy = "system";
            department.UpdatedAt = DateTimeOffset.UtcNow;
            unitOfWork.Repository<HrDepartment>().Update(department);
        }

        unitOfWork.Repository<HrEmployee>().Delete(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    private static IQueryable<HrEmployee> ApplySorting(IQueryable<HrEmployee> query, string sortBy, string sortDirection)
    {
        var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy switch
        {
            "employeeCode" => isDesc
                ? query.OrderByDescending(x => x.EmployeeCode)
                : query.OrderBy(x => x.EmployeeCode),

            "email" => isDesc
                ? query.OrderByDescending(x => x.Email).ThenBy(x => x.FullName)
                : query.OrderBy(x => x.Email).ThenBy(x => x.FullName),

            "phone" => isDesc
                ? query.OrderByDescending(x => x.Phone).ThenBy(x => x.FullName)
                : query.OrderBy(x => x.Phone).ThenBy(x => x.FullName),

            "departmentName" => isDesc
                ? query.OrderByDescending(x => x.Department.Name).ThenBy(x => x.FullName)
                : query.OrderBy(x => x.Department.Name).ThenBy(x => x.FullName),

            "positionName" => isDesc
                ? query.OrderByDescending(x => x.Position.Name).ThenBy(x => x.FullName)
                : query.OrderBy(x => x.Position.Name).ThenBy(x => x.FullName),

            "hireDate" => isDesc
                ? query.OrderByDescending(x => x.HireDate).ThenBy(x => x.FullName)
                : query.OrderBy(x => x.HireDate).ThenBy(x => x.FullName),

            "terminationDate" => isDesc
                ? query.OrderByDescending(x => x.TerminationDate).ThenBy(x => x.FullName)
                : query.OrderBy(x => x.TerminationDate).ThenBy(x => x.FullName),

            "employmentStatus" => isDesc
                ? query.OrderByDescending(x => x.EmploymentStatus).ThenBy(x => x.FullName)
                : query.OrderBy(x => x.EmploymentStatus).ThenBy(x => x.FullName),

            _ => isDesc
                ? query.OrderByDescending(x => x.FullName).ThenBy(x => x.EmployeeCode)
                : query.OrderBy(x => x.FullName).ThenBy(x => x.EmployeeCode)
        };
    }

    private async Task<string> GenerateNextEmployeeCodeAsync(CancellationToken ct)
    {
        var existingCodes = await unitOfWork.Repository<HrEmployee>()
            .Query()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Select(x => x.EmployeeCode)
            .ToListAsync(ct);

        var maxNumber = 0;

        foreach (var code in existingCodes)
        {
            var parsedNumber = ParseEmployeeCodeNumber(code);
            if (parsedNumber > maxNumber)
            {
                maxNumber = parsedNumber;
            }
        }

        var nextNumber = maxNumber + 1;
        return EmployeeCodePrefix + nextNumber.ToString($"D{EmployeeCodeDigits}", CultureInfo.InvariantCulture);
    }

    private static int ParseEmployeeCodeNumber(string? employeeCode)
    {
        if (string.IsNullOrWhiteSpace(employeeCode))
        {
            return 0;
        }

        var normalized = employeeCode.Trim();
        if (!normalized.StartsWith(EmployeeCodePrefix, StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        var numberPart = normalized[EmployeeCodePrefix.Length..];
        return int.TryParse(numberPart, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0;
    }

    private async Task ValidateDepartmentAndPositionAsync(int departmentId, int positionId, CancellationToken ct)
    {
        if (departmentId <= 0)
        {
            throw new InvalidOperationException("Department is required.");
        }

        if (positionId <= 0)
        {
            throw new InvalidOperationException("Position is required.");
        }

        var departmentExists = await unitOfWork.Repository<HrDepartment>()
            .Query()
            .AsNoTracking()
            .AnyAsync(x => x.Id == departmentId, ct);

        if (!departmentExists)
        {
            throw new InvalidOperationException("Department not found.");
        }

        var position = await unitOfWork.Repository<HrPosition>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == positionId, ct);

        if (position is null)
        {
            throw new InvalidOperationException("Position not found.");
        }

        if (position.DepartmentId != departmentId)
        {
            throw new InvalidOperationException("Position does not belong to selected department.");
        }
    }

    private static string NormalizeSortBy(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return DefaultSortBy;
        }

        return sortBy.Trim().ToLowerInvariant() switch
        {
            "employeecode" => "employeeCode",
            "fullname" => "fullName",
            "email" => "email",
            "phone" => "phone",
            "departmentname" => "departmentName",
            "positionname" => "positionName",
            "hiredate" => "hireDate",
            "terminationdate" => "terminationDate",
            "employmentstatus" => "employmentStatus",
            _ => DefaultSortBy
        };
    }

    private static string NormalizeSortDirection(string? sortDirection) =>
        string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";

    private static string NormalizeRequiredText(string? value, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(errorMessage);
        }

        return value.Trim();
    }

    private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static (DateOnly? From, DateOnly? To) NormalizeDateRange(DateOnly? from, DateOnly? to)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
        {
            return (to, from);
        }

        return (from, to);
    }

    private static EmployeeListDto MapEmployeeList(HrEmployee entity)
    {
        return new EmployeeListDto
        {
            Id = entity.Id,
            EmployeeCode = entity.EmployeeCode,
            FullName = entity.FullName,
            Email = entity.Email,
            Phone = entity.Phone,
            DepartmentId = entity.DepartmentId,
            DepartmentName = entity.Department.Name,
            PositionId = entity.PositionId,
            PositionName = entity.Position.Name,
            HireDate = entity.HireDate,
            TerminationDate = entity.TerminationDate,
            EmploymentStatus = entity.EmploymentStatus
        };
    }

    private static EmployeeDetailDto MapEmployeeDetail(HrEmployee entity)
    {
        return new EmployeeDetailDto
        {
            Id = entity.Id,
            EmployeeCode = entity.EmployeeCode,
            FullName = entity.FullName,
            Email = entity.Email,
            Phone = entity.Phone,
            DepartmentId = entity.DepartmentId,
            DepartmentName = entity.Department.Name,
            PositionId = entity.PositionId,
            PositionName = entity.Position.Name,
            HireDate = entity.HireDate,
            TerminationDate = entity.TerminationDate,
            EmploymentStatus = entity.EmploymentStatus
        };
    }
}
