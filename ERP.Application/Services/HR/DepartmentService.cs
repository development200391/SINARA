using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;
using ERP.Domain.Entities.HR;
using ERP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Services.HR;

public sealed class DepartmentService(IUnitOfWork unitOfWork) : IDepartmentService
{
    private const string DefaultSortBy = "name";

    public async Task<PagedResult<DepartmentDto>> GetPagedAsync(DepartmentPagedRequest request, CancellationToken ct = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var normalizedSortBy = NormalizeSortBy(request.SortBy);
        var normalizedSortDirection = NormalizeSortDirection(request.SortDirection);
        var normalizedCode = NormalizeText(request.Code);
        var normalizedName = NormalizeText(request.Name);

        var query = unitOfWork.Repository<HrDepartment>()
            .Query()
            .AsNoTracking()
            .Include(x => x.Manager)
            .Include(x => x.ParentDepartment)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Name.ToLower().Contains(search) ||
                x.Code.ToLower().Contains(search) ||
                (x.Manager != null && x.Manager.FullName.ToLower().Contains(search)) ||
                (x.ParentDepartment != null && x.ParentDepartment.Name.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(normalizedCode))
        {
            var code = normalizedCode.ToLowerInvariant();
            query = query.Where(x => x.Code.ToLower().Contains(code));
        }

        if (!string.IsNullOrWhiteSpace(normalizedName))
        {
            var name = normalizedName.ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(name));
        }

        if (request.ManagerId.HasValue)
        {
            query = query.Where(x => x.ManagerId == request.ManagerId.Value);
        }

        if (request.ParentDepartmentId.HasValue)
        {
            query = query.Where(x => x.ParentDepartmentId == request.ParentDepartmentId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        var total = await query.CountAsync(ct);

        query = ApplySorting(query, normalizedSortBy, normalizedSortDirection);

        var entities = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = entities
            .Select(MapDepartment)
            .ToList();

        return PagedResult<DepartmentDto>.Create(items, total, page, pageSize);
    }

    public async Task<DepartmentDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<HrDepartment>()
            .Query()
            .AsNoTracking()
            .Include(x => x.Manager)
            .Include(x => x.ParentDepartment)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return entity is null ? null : MapDepartment(entity);
    }

    public async Task<IReadOnlyList<DepartmentDto>> GetAllAsync(CancellationToken ct = default)
    {
        var entities = await unitOfWork.Repository<HrDepartment>()
            .Query()
            .AsNoTracking()
            .Include(x => x.Manager)
            .Include(x => x.ParentDepartment)
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Code)
            .ToListAsync(ct);

        return entities
            .Select(MapDepartment)
            .ToList();
    }

    public async Task<DepartmentDto> CreateAsync(DepartmentDto request, CancellationToken ct = default)
    {
        var normalizedName = NormalizeRequiredText(request.Name, "Department name is required.");
        var normalizedCode = NormalizeRequiredText(request.Code, "Department code is required.");

        await EnsureUniqueCodeAsync(normalizedCode, null, ct);
        await EnsureManagerExistsAsync(request.ManagerId, ct);
        await EnsureParentExistsAsync(request.ParentDepartmentId, ct);

        var entity = new HrDepartment
        {
            Name = normalizedName,
            Code = normalizedCode,
            ManagerId = request.ManagerId,
            ParentDepartmentId = request.ParentDepartmentId,
            IsActive = request.IsActive,
            CreatedBy = "system"
        };

        await unitOfWork.Repository<HrDepartment>().AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(entity.Id, ct)
            ?? throw new InvalidOperationException("Failed to load created department.");
    }

    public async Task<DepartmentDto?> UpdateAsync(int id, DepartmentDto request, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<HrDepartment>()
            .Query()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return null;
        }

        var normalizedName = NormalizeRequiredText(request.Name, "Department name is required.");
        var normalizedCode = NormalizeRequiredText(request.Code, "Department code is required.");

        if (request.ParentDepartmentId == id)
        {
            throw new InvalidOperationException("Department cannot be its own parent.");
        }

        await EnsureUniqueCodeAsync(normalizedCode, id, ct);
        await EnsureManagerExistsAsync(request.ManagerId, ct);
        await EnsureParentExistsAsync(request.ParentDepartmentId, ct);
        await EnsureNoHierarchyCycleAsync(id, request.ParentDepartmentId, ct);

        entity.Name = normalizedName;
        entity.Code = normalizedCode;
        entity.ManagerId = request.ManagerId;
        entity.ParentDepartmentId = request.ParentDepartmentId;
        entity.IsActive = request.IsActive;
        entity.UpdatedBy = "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        unitOfWork.Repository<HrDepartment>().Update(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(entity.Id, ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<HrDepartment>()
            .Query()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return false;
        }

        var hasChildren = await unitOfWork.Repository<HrDepartment>()
            .Query()
            .AnyAsync(x => x.ParentDepartmentId == id, ct);

        if (hasChildren)
        {
            throw new InvalidOperationException("Department cannot be deleted because it has child departments.");
        }

        var hasPositions = await unitOfWork.Repository<HrPosition>()
            .Query()
            .AnyAsync(x => x.DepartmentId == id, ct);

        if (hasPositions)
        {
            throw new InvalidOperationException("Department cannot be deleted because it is used by positions.");
        }

        var hasEmployees = await unitOfWork.Repository<HrEmployee>()
            .Query()
            .AnyAsync(x => x.DepartmentId == id, ct);

        if (hasEmployees)
        {
            throw new InvalidOperationException("Department cannot be deleted because it is used by employees.");
        }

        unitOfWork.Repository<HrDepartment>().Delete(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    private static IQueryable<HrDepartment> ApplySorting(IQueryable<HrDepartment> query, string sortBy, string sortDirection)
    {
        var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy switch
        {
            "code" => isDesc
                ? query.OrderByDescending(x => x.Code).ThenBy(x => x.Name)
                : query.OrderBy(x => x.Code).ThenBy(x => x.Name),

            "managerName" => isDesc
                ? query.OrderByDescending(x => x.Manager != null ? x.Manager.FullName : string.Empty).ThenBy(x => x.Name)
                : query.OrderBy(x => x.Manager != null ? x.Manager.FullName : string.Empty).ThenBy(x => x.Name),

            "parentDepartmentName" => isDesc
                ? query.OrderByDescending(x => x.ParentDepartment != null ? x.ParentDepartment.Name : string.Empty).ThenBy(x => x.Name)
                : query.OrderBy(x => x.ParentDepartment != null ? x.ParentDepartment.Name : string.Empty).ThenBy(x => x.Name),

            "isActive" => isDesc
                ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.Name)
                : query.OrderBy(x => x.IsActive).ThenBy(x => x.Name),

            _ => isDesc
                ? query.OrderByDescending(x => x.Name).ThenBy(x => x.Code)
                : query.OrderBy(x => x.Name).ThenBy(x => x.Code)
        };
    }

    private async Task EnsureUniqueCodeAsync(string code, int? currentId, CancellationToken ct)
    {
        var codeLower = code.ToLowerInvariant();

        var exists = await unitOfWork.Repository<HrDepartment>()
            .Query()
            .IgnoreQueryFilters()
            .AnyAsync(x =>
                x.Id != (currentId ?? 0) &&
                x.Code.ToLower() == codeLower,
                ct);

        if (exists)
        {
            throw new InvalidOperationException("Department code already exists.");
        }
    }

    private async Task EnsureManagerExistsAsync(int? managerId, CancellationToken ct)
    {
        if (!managerId.HasValue)
        {
            return;
        }

        var exists = await unitOfWork.Repository<HrEmployee>()
            .Query()
            .AsNoTracking()
            .AnyAsync(x => x.Id == managerId.Value, ct);

        if (!exists)
        {
            throw new InvalidOperationException("Manager employee not found.");
        }
    }

    private async Task EnsureParentExistsAsync(int? parentDepartmentId, CancellationToken ct)
    {
        if (!parentDepartmentId.HasValue)
        {
            return;
        }

        var exists = await unitOfWork.Repository<HrDepartment>()
            .Query()
            .AsNoTracking()
            .AnyAsync(x => x.Id == parentDepartmentId.Value, ct);

        if (!exists)
        {
            throw new InvalidOperationException("Parent department not found.");
        }
    }

    private async Task EnsureNoHierarchyCycleAsync(int departmentId, int? parentDepartmentId, CancellationToken ct)
    {
        if (!parentDepartmentId.HasValue)
        {
            return;
        }

        var hierarchy = await unitOfWork.Repository<HrDepartment>()
            .Query()
            .AsNoTracking()
            .Select(x => new { x.Id, x.ParentDepartmentId })
            .ToListAsync(ct);

        var parentById = hierarchy.ToDictionary(x => x.Id, x => x.ParentDepartmentId);

        var currentParentId = parentDepartmentId;
        var visited = new HashSet<int>();

        while (currentParentId.HasValue)
        {
            var parentId = currentParentId.Value;

            if (!visited.Add(parentId))
            {
                break;
            }

            if (parentId == departmentId)
            {
                throw new InvalidOperationException("Department hierarchy cannot contain a cycle.");
            }

            currentParentId = parentById.GetValueOrDefault(parentId);
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
            "code" => "code",
            "name" => "name",
            "managername" => "managerName",
            "parentdepartmentname" => "parentDepartmentName",
            "isactive" => "isActive",
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

    private static DepartmentDto MapDepartment(HrDepartment entity)
    {
        return new DepartmentDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code,
            ManagerId = entity.ManagerId,
            ManagerName = entity.Manager?.FullName,
            ParentDepartmentId = entity.ParentDepartmentId,
            ParentDepartmentName = entity.ParentDepartment?.Name,
            IsActive = entity.IsActive
        };
    }
}
