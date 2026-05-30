using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Entities.Finance;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Finance;

[Route("api/v1/finance/account-groups")]
public sealed class AccountGroupsController(AppDbContext dbContext) : FinanceControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] AccountGroupPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.FinAccountGroups
            .AsNoTracking()
            .Include(x => x.ParentGroup)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Name.ToLower().Contains(search) ||
                x.Code.ToLower().Contains(search));
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

        if (request.Type.HasValue)
        {
            query = query.Where(x => x.Type == request.Type.Value);
        }

        if (request.NormalBalance.HasValue)
        {
            query = query.Where(x => x.NormalBalance == request.NormalBalance.Value);
        }

        if (request.ParentGroupId.HasValue)
        {
            query = query.Where(x => x.ParentGroupId == request.ParentGroupId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "name" => isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "type" => isDesc ? query.OrderByDescending(x => x.Type) : query.OrderBy(x => x.Type),
            "normalbalance" => isDesc ? query.OrderByDescending(x => x.NormalBalance) : query.OrderBy(x => x.NormalBalance),
            "sortorder" => isDesc ? query.OrderByDescending(x => x.SortOrder) : query.OrderBy(x => x.SortOrder),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
            _ => isDesc ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.Code) : query.OrderBy(x => x.SortOrder).ThenBy(x => x.Code)
        };

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapDto(x))
            .ToListAsync(ct);

        return Ok(PagedResult<AccountGroupDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.FinAccountGroups
            .AsNoTracking()
            .Include(x => x.ParentGroup)
            .Where(x => x.Id == id)
            .Select(x => MapDto(x))
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AccountGroupDto request, CancellationToken ct)
    {
        try
        {
            var normalizedName = NormalizeRequired(request.Name, "Group name is required.");
            var normalizedCode = NormalizeRequired(request.Code, "Group code is required.").ToUpperInvariant();

            var parentId = request.ParentGroupId is > 0 ? request.ParentGroupId : null;
            if (parentId.HasValue)
            {
                var parentExists = await dbContext.FinAccountGroups.AnyAsync(x => x.Id == parentId.Value, ct);
                if (!parentExists)
                {
                    return BadRequest(new { message = "Parent account group not found." });
                }
            }

            var duplicate = await dbContext.FinAccountGroups
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Code == normalizedCode, ct);
            if (duplicate)
            {
                return BadRequest(new { message = "Account group code already exists." });
            }

            var entity = new FinAccountGroup
            {
                Name = normalizedName,
                Code = normalizedCode,
                Type = request.Type,
                NormalBalance = request.NormalBalance,
                ParentGroupId = parentId,
                SortOrder = request.SortOrder <= 0 ? 1 : request.SortOrder,
                IsActive = request.IsActive,
                CreatedBy = "system"
            };

            dbContext.FinAccountGroups.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var result = await dbContext.FinAccountGroups
                .AsNoTracking()
                .Include(x => x.ParentGroup)
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
    public async Task<IActionResult> Update(int id, [FromBody] AccountGroupDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.FinAccountGroups.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            var normalizedName = NormalizeRequired(request.Name, "Group name is required.");
            var normalizedCode = NormalizeRequired(request.Code, "Group code is required.").ToUpperInvariant();

            var parentId = request.ParentGroupId is > 0 ? request.ParentGroupId : null;
            if (parentId == id)
            {
                return BadRequest(new { message = "Parent account group is invalid." });
            }

            if (parentId.HasValue)
            {
                var parentExists = await dbContext.FinAccountGroups.AnyAsync(x => x.Id == parentId.Value, ct);
                if (!parentExists)
                {
                    return BadRequest(new { message = "Parent account group not found." });
                }
            }

            var duplicate = await dbContext.FinAccountGroups
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Id != id && x.Code == normalizedCode, ct);
            if (duplicate)
            {
                return BadRequest(new { message = "Account group code already exists." });
            }

            entity.Name = normalizedName;
            entity.Code = normalizedCode;
            entity.Type = request.Type;
            entity.NormalBalance = request.NormalBalance;
            entity.ParentGroupId = parentId;
            entity.SortOrder = request.SortOrder <= 0 ? 1 : request.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdatedBy = "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(ct);

            var result = await dbContext.FinAccountGroups
                .AsNoTracking()
                .Include(x => x.ParentGroup)
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
        var entity = await dbContext.FinAccountGroups.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        var hasChildren = await dbContext.FinAccountGroups.AnyAsync(x => x.ParentGroupId == id, ct);
        if (hasChildren)
        {
            return BadRequest(new { message = "Account group cannot be deleted because it has child groups." });
        }

        var hasAccounts = await dbContext.FinAccounts.AnyAsync(x => x.GroupId == id, ct);
        if (hasAccounts)
        {
            return BadRequest(new { message = "Account group cannot be deleted because it is used by accounts." });
        }

        dbContext.FinAccountGroups.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private static AccountGroupDto MapDto(FinAccountGroup entity)
    {
        return new AccountGroupDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code,
            Type = entity.Type,
            NormalBalance = entity.NormalBalance,
            ParentGroupId = entity.ParentGroupId,
            ParentGroupName = entity.ParentGroup?.Name,
            SortOrder = entity.SortOrder,
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
