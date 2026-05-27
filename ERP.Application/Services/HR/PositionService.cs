using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;
using ERP.Domain.Entities.HR;
using ERP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Services.HR;

public sealed class PositionService(IUnitOfWork unitOfWork) : IPositionService
{
    private const string DefaultSortBy = "name";

    public async Task<PagedResult<PositionDto>> GetPagedAsync(PositionPagedRequest request, CancellationToken ct = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var normalizedSortBy = NormalizeSortBy(request.SortBy);
        var normalizedSortDirection = NormalizeSortDirection(request.SortDirection);
        var normalizedCode = NormalizeText(request.Code);
        var normalizedName = NormalizeText(request.Name);
        var (levelFrom, levelTo) = NormalizeNumberRange(request.LevelFrom, request.LevelTo);

        var query = unitOfWork.Repository<HrPosition>()
            .Query()
            .AsNoTracking()
            .Include(x => x.Department)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Name.ToLower().Contains(search) ||
                x.Code.ToLower().Contains(search) ||
                x.Department.Name.ToLower().Contains(search) ||
                x.Level.ToString().Contains(search));
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

        if (request.DepartmentId.HasValue)
        {
            query = query.Where(x => x.DepartmentId == request.DepartmentId.Value);
        }

        if (levelFrom.HasValue)
        {
            query = query.Where(x => x.Level >= levelFrom.Value);
        }

        if (levelTo.HasValue)
        {
            query = query.Where(x => x.Level <= levelTo.Value);
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
            .Select(MapPosition)
            .ToList();

        return PagedResult<PositionDto>.Create(items, total, page, pageSize);
    }

    public async Task<IReadOnlyList<PositionDto>> GetAllAsync(CancellationToken ct = default)
    {
        var entities = await unitOfWork.Repository<HrPosition>()
            .Query()
            .AsNoTracking()
            .Include(x => x.Department)
            .OrderBy(x => x.Department.Name)
            .ThenBy(x => x.Level)
            .ThenBy(x => x.Name)
            .ToListAsync(ct);

        return entities
            .Select(MapPosition)
            .ToList();
    }

    public async Task<IReadOnlyList<PositionDto>> GetByDepartmentAsync(int departmentId, CancellationToken ct = default)
    {
        if (departmentId <= 0)
        {
            return [];
        }

        var entities = await unitOfWork.Repository<HrPosition>()
            .Query()
            .AsNoTracking()
            .Include(x => x.Department)
            .Where(x => x.DepartmentId == departmentId && x.IsActive)
            .OrderBy(x => x.Level)
            .ThenBy(x => x.Name)
            .ToListAsync(ct);

        return entities
            .Select(MapPosition)
            .ToList();
    }

    public async Task<PositionDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<HrPosition>()
            .Query()
            .AsNoTracking()
            .Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return entity is null ? null : MapPosition(entity);
    }

    public async Task<PositionDto> CreateAsync(PositionDto request, CancellationToken ct = default)
    {
        var normalizedName = NormalizeRequiredText(request.Name, "Position name is required.");
        var normalizedCode = NormalizeRequiredText(request.Code, "Position code is required.");

        if (request.DepartmentId <= 0)
        {
            throw new InvalidOperationException("Department is required.");
        }

        if (request.Level <= 0)
        {
            throw new InvalidOperationException("Position level must be greater than 0.");
        }

        await EnsureDepartmentExistsAsync(request.DepartmentId, ct);
        await EnsureUniqueCodeAsync(normalizedCode, null, ct);

        var entity = new HrPosition
        {
            Name = normalizedName,
            Code = normalizedCode,
            DepartmentId = request.DepartmentId,
            Level = request.Level,
            IsActive = request.IsActive,
            CreatedBy = "system"
        };

        await unitOfWork.Repository<HrPosition>().AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(entity.Id, ct)
            ?? throw new InvalidOperationException("Failed to load created position.");
    }

    public async Task<PositionDto?> UpdateAsync(int id, PositionDto request, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<HrPosition>()
            .Query()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return null;
        }

        var normalizedName = NormalizeRequiredText(request.Name, "Position name is required.");
        var normalizedCode = NormalizeRequiredText(request.Code, "Position code is required.");

        if (request.DepartmentId <= 0)
        {
            throw new InvalidOperationException("Department is required.");
        }

        if (request.Level <= 0)
        {
            throw new InvalidOperationException("Position level must be greater than 0.");
        }

        await EnsureDepartmentExistsAsync(request.DepartmentId, ct);
        await EnsureUniqueCodeAsync(normalizedCode, id, ct);

        entity.Name = normalizedName;
        entity.Code = normalizedCode;
        entity.DepartmentId = request.DepartmentId;
        entity.Level = request.Level;
        entity.IsActive = request.IsActive;
        entity.UpdatedBy = "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        unitOfWork.Repository<HrPosition>().Update(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(entity.Id, ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<HrPosition>()
            .Query()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return false;
        }

        var hasEmployees = await unitOfWork.Repository<HrEmployee>()
            .Query()
            .AnyAsync(x => x.PositionId == id, ct);

        if (hasEmployees)
        {
            throw new InvalidOperationException("Position cannot be deleted because it is used by employees.");
        }

        unitOfWork.Repository<HrPosition>().Delete(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    private static IQueryable<HrPosition> ApplySorting(IQueryable<HrPosition> query, string sortBy, string sortDirection)
    {
        var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy switch
        {
            "code" => isDesc
                ? query.OrderByDescending(x => x.Code).ThenBy(x => x.Name)
                : query.OrderBy(x => x.Code).ThenBy(x => x.Name),

            "departmentName" => isDesc
                ? query.OrderByDescending(x => x.Department.Name).ThenBy(x => x.Name)
                : query.OrderBy(x => x.Department.Name).ThenBy(x => x.Name),

            "level" => isDesc
                ? query.OrderByDescending(x => x.Level).ThenBy(x => x.Name)
                : query.OrderBy(x => x.Level).ThenBy(x => x.Name),

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

        var exists = await unitOfWork.Repository<HrPosition>()
            .Query()
            .IgnoreQueryFilters()
            .AnyAsync(x =>
                x.Id != (currentId ?? 0) &&
                x.Code.ToLower() == codeLower,
                ct);

        if (exists)
        {
            throw new InvalidOperationException("Position code already exists.");
        }
    }

    private async Task EnsureDepartmentExistsAsync(int departmentId, CancellationToken ct)
    {
        var exists = await unitOfWork.Repository<HrDepartment>()
            .Query()
            .AsNoTracking()
            .AnyAsync(x => x.Id == departmentId, ct);

        if (!exists)
        {
            throw new InvalidOperationException("Department not found.");
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
            "departmentname" => "departmentName",
            "level" => "level",
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

    private static (int? From, int? To) NormalizeNumberRange(int? from, int? to)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
        {
            return (to, from);
        }

        return (from, to);
    }

    private static PositionDto MapPosition(HrPosition entity)
    {
        return new PositionDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code,
            DepartmentId = entity.DepartmentId,
            DepartmentName = entity.Department.Name,
            Level = entity.Level,
            IsActive = entity.IsActive
        };
    }
}
