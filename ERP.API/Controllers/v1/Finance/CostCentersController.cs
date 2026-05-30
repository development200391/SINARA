using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Entities.Finance;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Finance;

[Route("api/v1/finance/cost-centers")]
public sealed class CostCentersController(AppDbContext dbContext) : FinanceControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] CostCenterPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.FinCostCenters
            .AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.Manager)
            .Include(x => x.BudgetAccount)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(search) ||
                x.Name.ToLower().Contains(search) ||
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

        if (request.ManagerId.HasValue)
        {
            query = query.Where(x => x.ManagerId == request.ManagerId.Value);
        }

        if (request.BudgetAccountId.HasValue)
        {
            query = query.Where(x => x.BudgetAccountId == request.BudgetAccountId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "name" => isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "departmentname" => isDesc ? query.OrderByDescending(x => x.Department!.Name).ThenByDescending(x => x.Code) : query.OrderBy(x => x.Department!.Name).ThenBy(x => x.Code),
            "managername" => isDesc ? query.OrderByDescending(x => x.Manager!.FullName).ThenByDescending(x => x.Code) : query.OrderBy(x => x.Manager!.FullName).ThenBy(x => x.Code),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Code),
            _ => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapDto(x))
            .ToListAsync(ct);

        return Ok(PagedResult<CostCenterDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.FinCostCenters
            .AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.Manager)
            .Include(x => x.BudgetAccount)
            .Where(x => x.Id == id)
            .Select(x => MapDto(x))
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CostCenterDto request, CancellationToken ct)
    {
        try
        {
            var normalizedCode = NormalizeRequired(request.Code, "Cost center code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Cost center name is required.");

            if (request.DepartmentId.HasValue && request.DepartmentId.Value > 0)
            {
                var exists = await dbContext.HrDepartments.AnyAsync(x => x.Id == request.DepartmentId.Value, ct);
                if (!exists)
                {
                    return BadRequest(new { message = "Department not found." });
                }
            }

            if (request.ManagerId.HasValue && request.ManagerId.Value > 0)
            {
                var exists = await dbContext.HrEmployees.AnyAsync(x => x.Id == request.ManagerId.Value, ct);
                if (!exists)
                {
                    return BadRequest(new { message = "Manager employee not found." });
                }
            }

            if (request.BudgetAccountId.HasValue && request.BudgetAccountId.Value > 0)
            {
                var exists = await dbContext.FinAccounts.AnyAsync(x => x.Id == request.BudgetAccountId.Value, ct);
                if (!exists)
                {
                    return BadRequest(new { message = "Budget account not found." });
                }
            }

            var duplicate = await dbContext.FinCostCenters
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Code == normalizedCode, ct);
            if (duplicate)
            {
                return BadRequest(new { message = "Cost center code already exists." });
            }

            var entity = new FinCostCenter
            {
                Code = normalizedCode,
                Name = normalizedName,
                DepartmentId = request.DepartmentId is > 0 ? request.DepartmentId : null,
                ManagerId = request.ManagerId is > 0 ? request.ManagerId : null,
                BudgetAccountId = request.BudgetAccountId is > 0 ? request.BudgetAccountId : null,
                IsActive = request.IsActive,
                CreatedBy = "system"
            };

            dbContext.FinCostCenters.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var result = await dbContext.FinCostCenters
                .AsNoTracking()
                .Include(x => x.Department)
                .Include(x => x.Manager)
                .Include(x => x.BudgetAccount)
                .Where(x => x.Id == entity.Id)
                .Select(x => MapDto(x))
                .FirstAsync(ct);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CostCenterDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.FinCostCenters.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            var normalizedCode = NormalizeRequired(request.Code, "Cost center code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Cost center name is required.");

            if (request.DepartmentId.HasValue && request.DepartmentId.Value > 0)
            {
                var exists = await dbContext.HrDepartments.AnyAsync(x => x.Id == request.DepartmentId.Value, ct);
                if (!exists)
                {
                    return BadRequest(new { message = "Department not found." });
                }
            }

            if (request.ManagerId.HasValue && request.ManagerId.Value > 0)
            {
                var exists = await dbContext.HrEmployees.AnyAsync(x => x.Id == request.ManagerId.Value, ct);
                if (!exists)
                {
                    return BadRequest(new { message = "Manager employee not found." });
                }
            }

            if (request.BudgetAccountId.HasValue && request.BudgetAccountId.Value > 0)
            {
                var exists = await dbContext.FinAccounts.AnyAsync(x => x.Id == request.BudgetAccountId.Value, ct);
                if (!exists)
                {
                    return BadRequest(new { message = "Budget account not found." });
                }
            }

            var duplicate = await dbContext.FinCostCenters
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Id != id && x.Code == normalizedCode, ct);
            if (duplicate)
            {
                return BadRequest(new { message = "Cost center code already exists." });
            }

            entity.Code = normalizedCode;
            entity.Name = normalizedName;
            entity.DepartmentId = request.DepartmentId is > 0 ? request.DepartmentId : null;
            entity.ManagerId = request.ManagerId is > 0 ? request.ManagerId : null;
            entity.BudgetAccountId = request.BudgetAccountId is > 0 ? request.BudgetAccountId : null;
            entity.IsActive = request.IsActive;
            entity.UpdatedBy = "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(ct);

            var result = await dbContext.FinCostCenters
                .AsNoTracking()
                .Include(x => x.Department)
                .Include(x => x.Manager)
                .Include(x => x.BudgetAccount)
                .Where(x => x.Id == entity.Id)
                .Select(x => MapDto(x))
                .FirstAsync(ct);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var entity = await dbContext.FinCostCenters.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        dbContext.FinCostCenters.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private static CostCenterDto MapDto(FinCostCenter entity)
    {
        return new CostCenterDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            DepartmentId = entity.DepartmentId,
            DepartmentName = entity.Department?.Name,
            ManagerId = entity.ManagerId,
            ManagerName = entity.Manager?.FullName,
            BudgetAccountId = entity.BudgetAccountId,
            BudgetAccountName = entity.BudgetAccount is null ? null : $"{entity.BudgetAccount.Code} - {entity.BudgetAccount.Name}",
            IsActive = entity.IsActive
        };
    }

    private static string NormalizeRequired(string? value, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(errorMessage);
        }

        return value.Trim();
    }
}
