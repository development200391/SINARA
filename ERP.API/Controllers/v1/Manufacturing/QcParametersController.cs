using System.Linq.Expressions;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Manufacturing;
using ERP.Domain.Entities.Manufacturing;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Manufacturing;

[Route("api/v1/manufacturing/qc/parameters")]
public sealed class QcParametersController(AppDbContext dbContext) : ManufacturingControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ManufacturingQcParameterPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.MfgQcParameters
            .AsNoTracking()
            .Include(x => x.Item)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(search)
                || x.Name.ToLower().Contains(search)
                || (x.Item != null && x.Item.ItemCode.ToLower().Contains(search))
                || (x.Item != null && x.Item.Name.ToLower().Contains(search))
                || (x.Notes != null && x.Notes.ToLower().Contains(search)));
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

        if (request.ItemId.HasValue)
        {
            query = query.Where(x => x.ItemId == request.ItemId.Value);
        }

        if (request.ParameterType.HasValue)
        {
            query = query.Where(x => x.ParameterType == request.ParameterType.Value);
        }

        if (request.IsCritical.HasValue)
        {
            query = query.Where(x => x.IsCritical == request.IsCritical.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "name" => isDesc ? query.OrderByDescending(x => x.Name).ThenByDescending(x => x.Code) : query.OrderBy(x => x.Name).ThenBy(x => x.Code),
            "parametertype" => isDesc ? query.OrderByDescending(x => x.ParameterType).ThenByDescending(x => x.Code) : query.OrderBy(x => x.ParameterType).ThenBy(x => x.Code),
            "iscritical" => isDesc ? query.OrderByDescending(x => x.IsCritical).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsCritical).ThenBy(x => x.Code),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Code),
            _ => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapQcParameterDto())
            .ToListAsync(ct);

        return Ok(PagedResult<ManufacturingQcParameterDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.MfgQcParameters
            .AsNoTracking()
            .Include(x => x.Item)
            .Where(x => x.Id == id)
            .Select(MapQcParameterDto())
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ManufacturingQcParameterDto request, CancellationToken ct)
    {
        try
        {
            var normalizedCode = NormalizeRequired(request.Code, "QC parameter code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "QC parameter name is required.");

            var validation = await ValidateQcParameterAsync(
                normalizedCode,
                request.ItemId,
                request.MinValue,
                request.MaxValue,
                null,
                ct);

            if (validation is not null)
            {
                return validation;
            }

            var entity = new MfgQcParameter
            {
                Code = normalizedCode,
                Name = normalizedName,
                ItemId = NormalizeOptionalId(request.ItemId),
                ParameterType = request.ParameterType,
                MinValue = NormalizeOptionalDecimal(request.MinValue, 4),
                MaxValue = NormalizeOptionalDecimal(request.MaxValue, 4),
                IsCritical = request.IsCritical,
                IsActive = request.IsActive,
                Notes = NormalizeOptional(request.Notes),
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            };

            dbContext.MfgQcParameters.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var created = await dbContext.MfgQcParameters
                .AsNoTracking()
                .Include(x => x.Item)
                .Where(x => x.Id == entity.Id)
                .Select(MapQcParameterDto())
                .FirstAsync(ct);

            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ManufacturingQcParameterDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.MfgQcParameters.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            var normalizedCode = NormalizeRequired(request.Code, "QC parameter code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "QC parameter name is required.");

            var validation = await ValidateQcParameterAsync(
                normalizedCode,
                request.ItemId,
                request.MinValue,
                request.MaxValue,
                id,
                ct);

            if (validation is not null)
            {
                return validation;
            }

            entity.Code = normalizedCode;
            entity.Name = normalizedName;
            entity.ItemId = NormalizeOptionalId(request.ItemId);
            entity.ParameterType = request.ParameterType;
            entity.MinValue = NormalizeOptionalDecimal(request.MinValue, 4);
            entity.MaxValue = NormalizeOptionalDecimal(request.MaxValue, 4);
            entity.IsCritical = request.IsCritical;
            entity.IsActive = request.IsActive;
            entity.Notes = NormalizeOptional(request.Notes);
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(ct);

            var updated = await dbContext.MfgQcParameters
                .AsNoTracking()
                .Include(x => x.Item)
                .Where(x => x.Id == id)
                .Select(MapQcParameterDto())
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
        var entity = await dbContext.MfgQcParameters.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        dbContext.MfgQcParameters.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task<IActionResult?> ValidateQcParameterAsync(
        string code,
        int? itemId,
        decimal? minValue,
        decimal? maxValue,
        int? id,
        CancellationToken ct)
    {
        var normalizedItemId = NormalizeOptionalId(itemId);
        if (normalizedItemId.HasValue)
        {
            var itemExists = await dbContext.InvItems.AnyAsync(x => x.Id == normalizedItemId.Value, ct);
            if (!itemExists)
            {
                return BadRequest(new { message = "Item not found." });
            }
        }

        if (minValue.HasValue && maxValue.HasValue && minValue.Value > maxValue.Value)
        {
            return BadRequest(new { message = "Min value must be less than or equal to max value." });
        }

        var duplicateCode = await dbContext.MfgQcParameters
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id != (id ?? 0) && x.Code == code, ct);

        if (duplicateCode)
        {
            return BadRequest(new { message = "QC parameter code already exists." });
        }

        return null;
    }

    private static int? NormalizeOptionalId(int? value)
    {
        if (!value.HasValue || value.Value <= 0)
        {
            return null;
        }

        return value.Value;
    }

    private static decimal? NormalizeOptionalDecimal(decimal? value, int precision)
    {
        if (!value.HasValue)
        {
            return null;
        }

        return decimal.Round(value.Value, precision, MidpointRounding.AwayFromZero);
    }

    private static string NormalizeRequired(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(message);
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static Expression<Func<MfgQcParameter, ManufacturingQcParameterDto>> MapQcParameterDto()
    {
        return x => new ManufacturingQcParameterDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            ItemId = x.ItemId,
            ItemCode = x.Item != null ? x.Item.ItemCode : null,
            ItemName = x.Item != null ? x.Item.Name : null,
            ParameterType = x.ParameterType,
            MinValue = x.MinValue,
            MaxValue = x.MaxValue,
            IsCritical = x.IsCritical,
            IsActive = x.IsActive,
            Notes = x.Notes
        };
    }
}
