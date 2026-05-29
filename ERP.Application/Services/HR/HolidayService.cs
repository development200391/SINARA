using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;
using ERP.Domain.Entities.HR;
using ERP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Services.HR;

public sealed class HolidayService(IUnitOfWork unitOfWork) : IHolidayService
{
    private const string DefaultSortBy = "holidayDate";

    public async Task<PagedResult<HolidayDto>> GetPagedAsync(HolidayPagedRequest request, CancellationToken ct = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var normalizedSortBy = NormalizeSortBy(request.SortBy);
        var normalizedSortDirection = NormalizeSortDirection(request.SortDirection);

        var normalizedName = NormalizeText(request.Name);
        var normalizedDescription = NormalizeText(request.Description);
        var normalizedAppliesTo = NormalizeText(request.AppliesTo);
        var (normalizedHolidayDateFrom, normalizedHolidayDateTo) = NormalizeDateRange(request.HolidayDateFrom, request.HolidayDateTo);
        var (normalizedYearFrom, normalizedYearTo) = NormalizeYearRange(request.YearFrom, request.YearTo);

        var query = unitOfWork.Repository<HrHoliday>()
            .Query()
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Name.ToLower().Contains(search) ||
                x.AppliesTo.ToLower().Contains(search) ||
                (x.Description != null && x.Description.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(normalizedName))
        {
            var name = normalizedName.ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(name));
        }

        if (normalizedHolidayDateFrom.HasValue)
        {
            query = query.Where(x => x.HolidayDate >= normalizedHolidayDateFrom.Value);
        }

        if (normalizedHolidayDateTo.HasValue)
        {
            query = query.Where(x => x.HolidayDate <= normalizedHolidayDateTo.Value);
        }

        if (request.HolidayType.HasValue)
        {
            query = query.Where(x => x.HolidayType == request.HolidayType.Value);
        }

        if (!string.IsNullOrWhiteSpace(normalizedDescription))
        {
            var description = normalizedDescription.ToLowerInvariant();
            query = query.Where(x => x.Description != null && x.Description.ToLower().Contains(description));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(normalizedAppliesTo))
        {
            var appliesTo = normalizedAppliesTo.ToLowerInvariant();
            query = query.Where(x => x.AppliesTo.ToLower().Contains(appliesTo));
        }

        if (normalizedYearFrom.HasValue)
        {
            query = query.Where(x => x.Year >= normalizedYearFrom.Value);
        }

        if (normalizedYearTo.HasValue)
        {
            query = query.Where(x => x.Year <= normalizedYearTo.Value);
        }

        var total = await query.CountAsync(ct);

        query = ApplySorting(query, normalizedSortBy, normalizedSortDirection);

        var entities = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = entities
            .Select(MapHoliday)
            .ToList();

        return PagedResult<HolidayDto>.Create(items, total, page, pageSize);
    }

    public async Task<HolidayDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<HrHoliday>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return entity is null ? null : MapHoliday(entity);
    }

    public async Task<HolidayDto> CreateAsync(HolidayDto request, CancellationToken ct = default)
    {
        var normalizedName = NormalizeRequiredText(request.Name, "Holiday name is required.");
        var normalizedAppliesTo = NormalizeAppliesTo(request.AppliesTo);

        await EnsureUniqueNameAndDateAsync(normalizedName, request.HolidayDate, null, ct);

        var entity = new HrHoliday
        {
            Name = normalizedName,
            HolidayDate = request.HolidayDate,
            HolidayType = request.HolidayType,
            Description = NormalizeText(request.Description),
            IsActive = request.IsActive,
            AppliesTo = normalizedAppliesTo,
            CreatedBy = "system"
        };

        await unitOfWork.Repository<HrHoliday>().AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(entity.Id, ct)
            ?? throw new InvalidOperationException("Failed to load created holiday.");
    }

    public async Task<HolidayDto?> UpdateAsync(int id, HolidayDto request, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<HrHoliday>()
            .Query()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return null;
        }

        var normalizedName = NormalizeRequiredText(request.Name, "Holiday name is required.");
        var normalizedAppliesTo = NormalizeAppliesTo(request.AppliesTo);

        await EnsureUniqueNameAndDateAsync(normalizedName, request.HolidayDate, id, ct);

        entity.Name = normalizedName;
        entity.HolidayDate = request.HolidayDate;
        entity.HolidayType = request.HolidayType;
        entity.Description = NormalizeText(request.Description);
        entity.IsActive = request.IsActive;
        entity.AppliesTo = normalizedAppliesTo;
        entity.UpdatedBy = "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        unitOfWork.Repository<HrHoliday>().Update(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(entity.Id, ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<HrHoliday>()
            .Query()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return false;
        }

        unitOfWork.Repository<HrHoliday>().Delete(entity);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    private async Task EnsureUniqueNameAndDateAsync(string name, DateOnly holidayDate, int? currentId, CancellationToken ct)
    {
        var normalizedName = name.ToLowerInvariant();

        var exists = await unitOfWork.Repository<HrHoliday>()
            .Query()
            .IgnoreQueryFilters()
            .AnyAsync(x =>
                x.Id != (currentId ?? 0) &&
                x.HolidayDate == holidayDate &&
                x.Name.ToLower() == normalizedName,
                ct);

        if (exists)
        {
            throw new InvalidOperationException("Holiday name and date already exists.");
        }
    }

    private static IQueryable<HrHoliday> ApplySorting(IQueryable<HrHoliday> query, string sortBy, string sortDirection)
    {
        var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy switch
        {
            "id" => isDesc
                ? query.OrderByDescending(x => x.Id)
                : query.OrderBy(x => x.Id),

            "name" => isDesc
                ? query.OrderByDescending(x => x.Name).ThenByDescending(x => x.HolidayDate)
                : query.OrderBy(x => x.Name).ThenBy(x => x.HolidayDate),

            "holidayType" => isDesc
                ? query.OrderByDescending(x => x.HolidayType).ThenByDescending(x => x.HolidayDate)
                : query.OrderBy(x => x.HolidayType).ThenBy(x => x.HolidayDate),

            "description" => isDesc
                ? query.OrderByDescending(x => x.Description).ThenByDescending(x => x.HolidayDate)
                : query.OrderBy(x => x.Description).ThenBy(x => x.HolidayDate),

            "isActive" => isDesc
                ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.HolidayDate)
                : query.OrderBy(x => x.IsActive).ThenBy(x => x.HolidayDate),

            "appliesTo" => isDesc
                ? query.OrderByDescending(x => x.AppliesTo).ThenByDescending(x => x.HolidayDate)
                : query.OrderBy(x => x.AppliesTo).ThenBy(x => x.HolidayDate),

            "year" => isDesc
                ? query.OrderByDescending(x => x.Year).ThenByDescending(x => x.HolidayDate)
                : query.OrderBy(x => x.Year).ThenBy(x => x.HolidayDate),

            _ => isDesc
                ? query.OrderByDescending(x => x.HolidayDate).ThenByDescending(x => x.Name)
                : query.OrderBy(x => x.HolidayDate).ThenBy(x => x.Name)
        };
    }

    private static HolidayDto MapHoliday(HrHoliday entity)
    {
        return new HolidayDto
        {
            Id = entity.Id,
            Name = entity.Name,
            HolidayDate = entity.HolidayDate,
            HolidayType = entity.HolidayType,
            Description = entity.Description,
            IsActive = entity.IsActive,
            AppliesTo = entity.AppliesTo,
            Year = entity.Year
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
            "name" => "name",
            "holidaydate" => "holidayDate",
            "holidaytype" => "holidayType",
            "description" => "description",
            "isactive" => "isActive",
            "appliesto" => "appliesTo",
            "year" => "year",
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

    private static string NormalizeAppliesTo(string? value)
    {
        var normalized = NormalizeText(value);
        return string.IsNullOrWhiteSpace(normalized) ? "all" : normalized;
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

    private static (short? From, short? To) NormalizeYearRange(short? from, short? to)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
        {
            return (to, from);
        }

        return (from, to);
    }
}
