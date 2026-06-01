using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.FixedAssets;
using ERP.Domain.Entities.FixedAssets;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.FixedAssets;

[Route("api/v1/fixed-assets/locations")]
public sealed class LocationsController(AppDbContext dbContext) : FixedAssetsControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] FixedAssetLocationPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.FaLocations
            .AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.Manager)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(search) ||
                x.Name.ToLower().Contains(search) ||
                (x.Address != null && x.Address.ToLower().Contains(search)) ||
                (x.Department != null && x.Department.Name.ToLower().Contains(search)) ||
                (x.Manager != null && x.Manager.FullName.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var code = request.Code.Trim().ToLowerInvariant();
            query = query.Where(x => x.Code.ToLower().Contains(code));
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var name = request.Name.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(name));
        }

        if (request.DepartmentId.HasValue)
        {
            query = query.Where(x => x.DepartmentId == request.DepartmentId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "name" => isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "departmentname" => isDesc
                ? query.OrderByDescending(x => x.Department != null ? x.Department.Name : string.Empty).ThenByDescending(x => x.Code)
                : query.OrderBy(x => x.Department != null ? x.Department.Name : string.Empty).ThenBy(x => x.Code),
            "managername" => isDesc
                ? query.OrderByDescending(x => x.Manager != null ? x.Manager.FullName : string.Empty).ThenByDescending(x => x.Code)
                : query.OrderBy(x => x.Manager != null ? x.Manager.FullName : string.Empty).ThenBy(x => x.Code),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Code),
            _ => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapDto(x))
            .ToListAsync(ct);

        return Ok(PagedResult<FixedAssetLocationDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("options")]
    public async Task<IActionResult> GetOptions(CancellationToken ct)
    {
        var options = await dbContext.FaLocations
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new FixedAssetOptionDto
            {
                Id = x.Id,
                Label = x.Code + " - " + x.Name
            })
            .ToListAsync(ct);

        return Ok(options);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.FaLocations
            .AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.Manager)
            .Where(x => x.Id == id)
            .Select(x => MapDto(x))
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FixedAssetLocationDto request, CancellationToken ct)
    {
        try
        {
            var normalizedCode = NormalizeRequired(request.Code, "Location code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Location name is required.");

            await ValidateDependenciesAsync(request.DepartmentId, request.ManagerId, ct);

            var duplicate = await dbContext.FaLocations
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Code == normalizedCode, ct);

            if (duplicate)
            {
                return BadRequest(new { message = "Location code already exists." });
            }

            var entity = new FaLocation
            {
                Code = normalizedCode,
                Name = normalizedName,
                Address = NormalizeOptional(request.Address),
                DepartmentId = NormalizeOptionalId(request.DepartmentId),
                ManagerId = NormalizeOptionalId(request.ManagerId),
                IsActive = request.IsActive,
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            };

            dbContext.FaLocations.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var created = await dbContext.FaLocations
                .AsNoTracking()
                .Include(x => x.Department)
                .Include(x => x.Manager)
                .Where(x => x.Id == entity.Id)
                .Select(x => MapDto(x))
                .FirstAsync(ct);

            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] FixedAssetLocationDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.FaLocations.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            var normalizedCode = NormalizeRequired(request.Code, "Location code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Location name is required.");

            await ValidateDependenciesAsync(request.DepartmentId, request.ManagerId, ct);

            var duplicate = await dbContext.FaLocations
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Id != id && x.Code == normalizedCode, ct);

            if (duplicate)
            {
                return BadRequest(new { message = "Location code already exists." });
            }

            entity.Code = normalizedCode;
            entity.Name = normalizedName;
            entity.Address = NormalizeOptional(request.Address);
            entity.DepartmentId = NormalizeOptionalId(request.DepartmentId);
            entity.ManagerId = NormalizeOptionalId(request.ManagerId);
            entity.IsActive = request.IsActive;
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(ct);

            var updated = await dbContext.FaLocations
                .AsNoTracking()
                .Include(x => x.Department)
                .Include(x => x.Manager)
                .Where(x => x.Id == entity.Id)
                .Select(x => MapDto(x))
                .FirstAsync(ct);

            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var entity = await dbContext.FaLocations.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        var hasAsset = await dbContext.FaAssets.AnyAsync(x => x.LocationId == id, ct);
        if (hasAsset)
        {
            return BadRequest(new { message = "Location is already used by assets." });
        }

        var hasTransfer = await dbContext.FaAssetTransfers.AnyAsync(x => x.FromLocationId == id || x.ToLocationId == id, ct);
        if (hasTransfer)
        {
            return BadRequest(new { message = "Location is already used by transfer records." });
        }

        dbContext.FaLocations.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task ValidateDependenciesAsync(int? departmentId, int? managerId, CancellationToken ct)
    {
        if (departmentId is > 0)
        {
            var exists = await dbContext.HrDepartments
                .AsNoTracking()
                .AnyAsync(x => x.Id == departmentId.Value, ct);

            if (!exists)
            {
                throw new InvalidOperationException("Department not found.");
            }
        }

        if (managerId is > 0)
        {
            var exists = await dbContext.HrEmployees
                .AsNoTracking()
                .AnyAsync(x => x.Id == managerId.Value, ct);

            if (!exists)
            {
                throw new InvalidOperationException("Manager not found.");
            }
        }
    }

    private static int? NormalizeOptionalId(int? value) => value is > 0 ? value : null;

    private static string NormalizeRequired(string? value, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(errorMessage);
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static FixedAssetLocationDto MapDto(FaLocation entity)
    {
        return new FixedAssetLocationDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Address = entity.Address,
            DepartmentId = entity.DepartmentId,
            DepartmentName = entity.Department?.Name,
            ManagerId = entity.ManagerId,
            ManagerName = entity.Manager?.FullName,
            IsActive = entity.IsActive
        };
    }
}
