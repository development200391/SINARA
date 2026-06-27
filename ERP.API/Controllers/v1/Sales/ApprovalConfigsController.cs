using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Sales;
using ERP.Domain.Entities.Sales;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Sales;

[Route("api/v1/sales/approval-configs")]
public sealed class ApprovalConfigsController(AppDbContext dbContext) : SalesControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] SalesApprovalConfigPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.SalApprovalConfigs
            .AsNoTracking()
            .Include(x => x.ApproverRole)
            .Include(x => x.ApproverEmployee)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                (x.ApproverRole != null && x.ApproverRole.Name.ToLower().Contains(search)) ||
                (x.ApproverEmployee != null && x.ApproverEmployee.FullName.ToLower().Contains(search)));
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

        if (request.MaxDiscountPctFrom.HasValue)
        {
            query = query.Where(x => x.MaxDiscountPct.HasValue && x.MaxDiscountPct.Value >= request.MaxDiscountPctFrom.Value);
        }

        if (request.MaxDiscountPctTo.HasValue)
        {
            query = query.Where(x => x.MaxDiscountPct.HasValue && x.MaxDiscountPct.Value <= request.MaxDiscountPctTo.Value);
        }

        if (request.ApproverRoleId.HasValue)
        {
            query = query.Where(x => x.ApproverRoleId == request.ApproverRoleId.Value);
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
            "level" => isDesc ? query.OrderByDescending(x => x.Level).ThenByDescending(x => x.DocumentType) : query.OrderBy(x => x.Level).ThenBy(x => x.DocumentType),
            "minamount" => isDesc ? query.OrderByDescending(x => x.MinAmount).ThenByDescending(x => x.Level) : query.OrderBy(x => x.MinAmount).ThenBy(x => x.Level),
            "maxamount" => isDesc ? query.OrderByDescending(x => x.MaxAmount).ThenByDescending(x => x.Level) : query.OrderBy(x => x.MaxAmount).ThenBy(x => x.Level),
            "maxdiscountpct" => isDesc ? query.OrderByDescending(x => x.MaxDiscountPct).ThenByDescending(x => x.Level) : query.OrderBy(x => x.MaxDiscountPct).ThenBy(x => x.Level),
            "approverrolename" => isDesc ? query.OrderByDescending(x => x.ApproverRole != null ? x.ApproverRole.Name : string.Empty).ThenByDescending(x => x.Level) : query.OrderBy(x => x.ApproverRole != null ? x.ApproverRole.Name : string.Empty).ThenBy(x => x.Level),
            "approveremployeename" => isDesc ? query.OrderByDescending(x => x.ApproverEmployee != null ? x.ApproverEmployee.FullName : string.Empty).ThenByDescending(x => x.Level) : query.OrderBy(x => x.ApproverEmployee != null ? x.ApproverEmployee.FullName : string.Empty).ThenBy(x => x.Level),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Level) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Level),
            _ => isDesc ? query.OrderByDescending(x => x.DocumentType).ThenByDescending(x => x.Level) : query.OrderBy(x => x.DocumentType).ThenBy(x => x.Level)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapDto(x))
            .ToListAsync(ct);

        return Ok(PagedResult<SalesApprovalConfigDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.SalApprovalConfigs
            .AsNoTracking()
            .Include(x => x.ApproverRole)
            .Include(x => x.ApproverEmployee)
            .Where(x => x.Id == id)
            .Select(x => MapDto(x))
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SalesApprovalConfigDto request, CancellationToken ct)
    {
        var validation = await ValidateRequestAsync(request, null, ct);
        if (validation is not null)
        {
            return validation;
        }

        var entity = new SalApprovalConfig
        {
            DocumentType = request.DocumentType,
            Level = request.Level,
            MinAmount = decimal.Round(request.MinAmount, 4, MidpointRounding.AwayFromZero),
            MaxAmount = request.MaxAmount.HasValue ? decimal.Round(request.MaxAmount.Value, 4, MidpointRounding.AwayFromZero) : null,
            MaxDiscountPct = request.MaxDiscountPct.HasValue ? decimal.Round(request.MaxDiscountPct.Value, 2, MidpointRounding.AwayFromZero) : null,
            ApproverRoleId = request.ApproverRoleId is > 0 ? request.ApproverRoleId : null,
            ApproverEmployeeId = request.ApproverEmployeeId is > 0 ? request.ApproverEmployeeId : null,
            TimeoutHours = request.TimeoutHours,
            AutoApproveIfTimeout = request.AutoApproveIfTimeout,
            IsActive = request.IsActive,
            CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
        };

        dbContext.SalApprovalConfigs.Add(entity);
        await dbContext.SaveChangesAsync(ct);

        var created = await dbContext.SalApprovalConfigs
            .AsNoTracking()
            .Include(x => x.ApproverRole)
            .Include(x => x.ApproverEmployee)
            .Where(x => x.Id == entity.Id)
            .Select(x => MapDto(x))
            .FirstAsync(ct);

        return Ok(created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SalesApprovalConfigDto request, CancellationToken ct)
    {
        var entity = await dbContext.SalApprovalConfigs.FirstOrDefaultAsync(x => x.Id == id, ct);
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
        entity.MinAmount = decimal.Round(request.MinAmount, 4, MidpointRounding.AwayFromZero);
        entity.MaxAmount = request.MaxAmount.HasValue ? decimal.Round(request.MaxAmount.Value, 4, MidpointRounding.AwayFromZero) : null;
        entity.MaxDiscountPct = request.MaxDiscountPct.HasValue ? decimal.Round(request.MaxDiscountPct.Value, 2, MidpointRounding.AwayFromZero) : null;
        entity.ApproverRoleId = request.ApproverRoleId is > 0 ? request.ApproverRoleId : null;
        entity.ApproverEmployeeId = request.ApproverEmployeeId is > 0 ? request.ApproverEmployeeId : null;
        entity.TimeoutHours = request.TimeoutHours;
        entity.AutoApproveIfTimeout = request.AutoApproveIfTimeout;
        entity.IsActive = request.IsActive;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        var updated = await dbContext.SalApprovalConfigs
            .AsNoTracking()
            .Include(x => x.ApproverRole)
            .Include(x => x.ApproverEmployee)
            .Where(x => x.Id == id)
            .Select(x => MapDto(x))
            .FirstAsync(ct);

        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var entity = await dbContext.SalApprovalConfigs.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        dbContext.SalApprovalConfigs.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task<IActionResult?> ValidateRequestAsync(SalesApprovalConfigDto request, int? id, CancellationToken ct)
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

        if (request.MaxDiscountPct.HasValue && (request.MaxDiscountPct.Value < 0 || request.MaxDiscountPct.Value > 100))
        {
            return BadRequest(new { message = "Maximum discount must be in range 0..100." });
        }

        if (request.TimeoutHours <= 0)
        {
            return BadRequest(new { message = "Timeout hours must be greater than zero." });
        }

        var approverRoleId = request.ApproverRoleId is > 0 ? request.ApproverRoleId : null;
        var approverEmployeeId = request.ApproverEmployeeId is > 0 ? request.ApproverEmployeeId : null;

        if (!approverRoleId.HasValue && !approverEmployeeId.HasValue)
        {
            return BadRequest(new { message = "At least approver role or approver employee must be set." });
        }

        if (approverRoleId.HasValue)
        {
            var roleExists = await dbContext.CfgRoles.AnyAsync(x => x.Id == approverRoleId.Value, ct);
            if (!roleExists)
            {
                return BadRequest(new { message = "Approver role not found." });
            }
        }

        if (approverEmployeeId.HasValue)
        {
            var employeeExists = await dbContext.HrEmployees.AnyAsync(x => x.Id == approverEmployeeId.Value, ct);
            if (!employeeExists)
            {
                return BadRequest(new { message = "Approver employee not found." });
            }
        }

        var duplicate = await dbContext.SalApprovalConfigs
            .IgnoreQueryFilters()
            .AnyAsync(x =>
                x.Id != (id ?? 0)
                && x.DocumentType == request.DocumentType
                && x.Level == request.Level,
                ct);

        if (duplicate)
        {
            return BadRequest(new { message = "Approval config already exists for this document type and level." });
        }

        return null;
    }

    private static SalesApprovalConfigDto MapDto(SalApprovalConfig entity)
    {
        return new SalesApprovalConfigDto
        {
            Id = entity.Id,
            DocumentType = entity.DocumentType,
            Level = entity.Level,
            MinAmount = entity.MinAmount,
            MaxAmount = entity.MaxAmount,
            MaxDiscountPct = entity.MaxDiscountPct,
            ApproverRoleId = entity.ApproverRoleId,
            ApproverRoleName = entity.ApproverRole != null ? entity.ApproverRole.Name : null,
            ApproverEmployeeId = entity.ApproverEmployeeId,
            ApproverEmployeeName = entity.ApproverEmployee != null ? entity.ApproverEmployee.FullName : null,
            TimeoutHours = entity.TimeoutHours,
            AutoApproveIfTimeout = entity.AutoApproveIfTimeout,
            IsActive = entity.IsActive
        };
    }
}
