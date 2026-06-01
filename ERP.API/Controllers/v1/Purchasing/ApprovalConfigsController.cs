using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Purchasing;
using ERP.Domain.Entities.Purchasing;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Purchasing;

[Route("api/v1/purchasing/approval-configs")]
public sealed class ApprovalConfigsController(AppDbContext dbContext) : PurchasingControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ApprovalConfigPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.PurApprovalConfigs
            .AsNoTracking()
            .Include(x => x.ApproverEmployee)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.ApproverEmployee.FullName.ToLower().Contains(search) ||
                (x.Notes != null && x.Notes.ToLower().Contains(search)));
        }

        if (request.DocumentType.HasValue)
        {
            query = query.Where(x => x.DocumentType == request.DocumentType.Value);
        }

        if (request.Level.HasValue)
        {
            query = query.Where(x => x.Level == request.Level.Value);
        }

        if (request.MinAmountFrom.HasValue)
        {
            query = query.Where(x => x.MinAmount >= request.MinAmountFrom.Value);
        }

        if (request.MinAmountTo.HasValue)
        {
            query = query.Where(x => x.MinAmount <= request.MinAmountTo.Value);
        }

        if (request.MaxAmountFrom.HasValue)
        {
            query = query.Where(x => x.MaxAmount.HasValue && x.MaxAmount.Value >= request.MaxAmountFrom.Value);
        }

        if (request.MaxAmountTo.HasValue)
        {
            query = query.Where(x => x.MaxAmount.HasValue && x.MaxAmount.Value <= request.MaxAmountTo.Value);
        }

        if (request.ApproverEmployeeId.HasValue)
        {
            query = query.Where(x => x.ApproverEmployeeId == request.ApproverEmployeeId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "documenttype" => isDesc ? query.OrderByDescending(x => x.DocumentType).ThenByDescending(x => x.Level) : query.OrderBy(x => x.DocumentType).ThenBy(x => x.Level),
            "level" => isDesc ? query.OrderByDescending(x => x.Level) : query.OrderBy(x => x.Level),
            "minamount" => isDesc ? query.OrderByDescending(x => x.MinAmount) : query.OrderBy(x => x.MinAmount),
            "maxamount" => isDesc ? query.OrderByDescending(x => x.MaxAmount) : query.OrderBy(x => x.MaxAmount),
            "approveremployeename" => isDesc ? query.OrderByDescending(x => x.ApproverEmployee.FullName) : query.OrderBy(x => x.ApproverEmployee.FullName),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Level) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Level),
            _ => isDesc ? query.OrderByDescending(x => x.DocumentType).ThenByDescending(x => x.Level) : query.OrderBy(x => x.DocumentType).ThenBy(x => x.Level)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapDto(x))
            .ToListAsync(ct);

        return Ok(PagedResult<ApprovalConfigDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.PurApprovalConfigs
            .AsNoTracking()
            .Include(x => x.ApproverEmployee)
            .Where(x => x.Id == id)
            .Select(x => MapDto(x))
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ApprovalConfigDto request, CancellationToken ct)
    {
        var validation = await ValidateRequestAsync(request, null, ct);
        if (validation is not null)
        {
            return validation;
        }

        var entity = new PurApprovalConfig
        {
            DocumentType = request.DocumentType,
            Level = request.Level,
            MinAmount = request.MinAmount,
            MaxAmount = request.MaxAmount,
            ApproverEmployeeId = request.ApproverEmployeeId,
            IsActive = request.IsActive,
            Notes = NormalizeOptional(request.Notes),
            CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
        };

        dbContext.PurApprovalConfigs.Add(entity);
        await dbContext.SaveChangesAsync(ct);

        var created = await dbContext.PurApprovalConfigs
            .AsNoTracking()
            .Include(x => x.ApproverEmployee)
            .Where(x => x.Id == entity.Id)
            .Select(x => MapDto(x))
            .FirstAsync(ct);

        return Ok(created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ApprovalConfigDto request, CancellationToken ct)
    {
        var entity = await dbContext.PurApprovalConfigs.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        var validation = await ValidateRequestAsync(request, id, ct);
        if (validation is not null)
        {
            return validation;
        }

        entity.DocumentType = request.DocumentType;
        entity.Level = request.Level;
        entity.MinAmount = request.MinAmount;
        entity.MaxAmount = request.MaxAmount;
        entity.ApproverEmployeeId = request.ApproverEmployeeId;
        entity.IsActive = request.IsActive;
        entity.Notes = NormalizeOptional(request.Notes);
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        var updated = await dbContext.PurApprovalConfigs
            .AsNoTracking()
            .Include(x => x.ApproverEmployee)
            .Where(x => x.Id == id)
            .Select(x => MapDto(x))
            .FirstAsync(ct);

        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var entity = await dbContext.PurApprovalConfigs.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        dbContext.PurApprovalConfigs.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task<IActionResult?> ValidateRequestAsync(ApprovalConfigDto request, int? id, CancellationToken ct)
    {
        if (request.Level <= 0)
        {
            return BadRequest(new { message = "Approval level must be greater than zero." });
        }

        if (request.MinAmount < 0)
        {
            return BadRequest(new { message = "Minimum amount cannot be negative." });
        }

        if (request.MaxAmount.HasValue && request.MaxAmount.Value < request.MinAmount)
        {
            return BadRequest(new { message = "Maximum amount must be greater than or equal to minimum amount." });
        }

        var approverExists = await dbContext.HrEmployees.AnyAsync(x => x.Id == request.ApproverEmployeeId, ct);
        if (!approverExists)
        {
            return BadRequest(new { message = "Approver employee not found." });
        }

        var duplicate = await dbContext.PurApprovalConfigs
            .IgnoreQueryFilters()
            .AnyAsync(x =>
                x.Id != (id ?? 0) &&
                x.DocumentType == request.DocumentType &&
                x.Level == request.Level &&
                x.MinAmount == request.MinAmount &&
                x.MaxAmount == request.MaxAmount,
                ct);

        if (duplicate)
        {
            return BadRequest(new { message = "Approval config already exists for this range." });
        }

        return null;
    }

    private static ApprovalConfigDto MapDto(PurApprovalConfig entity)
    {
        return new ApprovalConfigDto
        {
            Id = entity.Id,
            DocumentType = entity.DocumentType,
            Level = entity.Level,
            MinAmount = entity.MinAmount,
            MaxAmount = entity.MaxAmount,
            ApproverEmployeeId = entity.ApproverEmployeeId,
            ApproverEmployeeName = entity.ApproverEmployee.FullName,
            IsActive = entity.IsActive,
            Notes = entity.Notes
        };
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

