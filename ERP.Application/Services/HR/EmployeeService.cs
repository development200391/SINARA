using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;
using ERP.Domain.Entities.HR;
using ERP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Services.HR;

public sealed class EmployeeService(IUnitOfWork unitOfWork) : IEmployeeService
{
    public async Task<PagedResult<EmployeeListDto>> GetPagedAsync(EmployeePagedRequest request, CancellationToken ct = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var query = unitOfWork.Repository<HrEmployee>()
            .Query()
            .AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.Position)
            .AsQueryable();

        if (request.DepartmentId.HasValue)
        {
            query = query.Where(x => x.DepartmentId == request.DepartmentId.Value);
        }

        if (request.EmploymentStatus.HasValue)
        {
            query = query.Where(x => x.EmploymentStatus == request.EmploymentStatus.Value);
        }

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

        var total = await query.CountAsync(ct);

        var entities = await query
            .OrderBy(x => x.FullName)
            .ThenBy(x => x.EmployeeCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = entities
            .Select(MapEmployeeList)
            .ToList();

        return PagedResult<EmployeeListDto>.Create(items, total, page, pageSize);
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
        var normalizedCode = NormalizeRequiredText(request.EmployeeCode, "Employee code is required.");
        var normalizedFullName = NormalizeRequiredText(request.FullName, "Employee full name is required.");
        var normalizedEmail = NormalizeOptionalText(request.Email);
        var normalizedPhone = NormalizeOptionalText(request.Phone);

        await ValidateDepartmentAndPositionAsync(request.DepartmentId, request.PositionId, ct);
        await EnsureUniqueEmployeeCodeAsync(normalizedCode, null, ct);

        var entity = new HrEmployee
        {
            EmployeeCode = normalizedCode,
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

        var normalizedCode = NormalizeRequiredText(request.EmployeeCode, "Employee code is required.");
        var normalizedFullName = NormalizeRequiredText(request.FullName, "Employee full name is required.");
        var normalizedEmail = NormalizeOptionalText(request.Email);
        var normalizedPhone = NormalizeOptionalText(request.Phone);

        await ValidateDepartmentAndPositionAsync(request.DepartmentId, request.PositionId, ct);
        await EnsureUniqueEmployeeCodeAsync(normalizedCode, id, ct);

        entity.EmployeeCode = normalizedCode;
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

    private async Task EnsureUniqueEmployeeCodeAsync(string employeeCode, int? currentId, CancellationToken ct)
    {
        var normalizedCode = employeeCode.ToLowerInvariant();

        var exists = await unitOfWork.Repository<HrEmployee>()
            .Query()
            .IgnoreQueryFilters()
            .AnyAsync(x =>
                x.Id != (currentId ?? 0) &&
                x.EmployeeCode.ToLower() == normalizedCode,
                ct);

        if (exists)
        {
            throw new InvalidOperationException("Employee code already exists.");
        }
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

    private static string NormalizeRequiredText(string? value, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(errorMessage);
        }

        return value.Trim();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static EmployeeListDto MapEmployeeList(HrEmployee entity)
    {
        return new EmployeeListDto
        {
            Id = entity.Id,
            EmployeeCode = entity.EmployeeCode,
            FullName = entity.FullName,
            DepartmentName = entity.Department.Name,
            PositionName = entity.Position.Name,
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
