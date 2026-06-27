using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Sales;
using ERP.Domain.Entities.Sales;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Sales;

[Route("api/v1/sales/teams")]
public sealed class TeamsController(AppDbContext dbContext) : SalesControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] SalesTeamPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.SalSalesTeams
            .AsNoTracking()
            .Include(x => x.TeamLeader)
            .Include(x => x.Members)
                .ThenInclude(x => x.Employee)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(search) ||
                x.Name.ToLower().Contains(search) ||
                x.TeamLeader.FullName.ToLower().Contains(search) ||
                x.Members.Any(m => m.Employee.FullName.ToLower().Contains(search)));
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

        if (request.TeamLeaderId.HasValue)
        {
            query = query.Where(x => x.TeamLeaderId == request.TeamLeaderId.Value);
        }

        if (request.MemberEmployeeId.HasValue)
        {
            query = query.Where(x => x.Members.Any(m => m.EmployeeId == request.MemberEmployeeId.Value));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "name" => isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "teamleadername" => isDesc ? query.OrderByDescending(x => x.TeamLeader.FullName).ThenByDescending(x => x.Code) : query.OrderBy(x => x.TeamLeader.FullName).ThenBy(x => x.Code),
            "membercount" => isDesc ? query.OrderByDescending(x => x.Members.Count).ThenByDescending(x => x.Code) : query.OrderBy(x => x.Members.Count).ThenBy(x => x.Code),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Code),
            _ => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };

        var totalCount = await query.CountAsync(ct);
        var entities = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = entities.Select(MapDto).ToList();
        return Ok(PagedResult<SalesTeamDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("options")]
    public async Task<IActionResult> GetOptions(CancellationToken ct)
    {
        var options = await dbContext.SalSalesTeams
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new SalesOptionDto
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
        var entity = await dbContext.SalSalesTeams
            .AsNoTracking()
            .Include(x => x.TeamLeader)
            .Include(x => x.Members)
                .ThenInclude(x => x.Employee)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return entity is null ? NotFound() : Ok(MapDto(entity));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SalesTeamDto request, CancellationToken ct)
    {
        try
        {
            var normalizedCode = NormalizeRequired(request.Code, "Sales team code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Sales team name is required.");
            var memberIds = NormalizeEmployeeIds(request.MemberEmployeeIds);

            var validation = await ValidateRequestAsync(request.TeamLeaderId, normalizedCode, null, memberIds, ct);
            if (validation is not null)
            {
                return validation;
            }

            var entity = new SalSalesTeam
            {
                Code = normalizedCode,
                Name = normalizedName,
                TeamLeaderId = request.TeamLeaderId,
                IsActive = request.IsActive,
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            };

            foreach (var memberId in memberIds)
            {
                entity.Members.Add(new SalSalesTeamMember
                {
                    EmployeeId = memberId,
                    CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
                });
            }

            dbContext.SalSalesTeams.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var created = await dbContext.SalSalesTeams
                .AsNoTracking()
                .Include(x => x.TeamLeader)
                .Include(x => x.Members)
                    .ThenInclude(x => x.Employee)
                .FirstAsync(x => x.Id == entity.Id, ct);

            return Ok(MapDto(created));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SalesTeamDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.SalSalesTeams
                .Include(x => x.Members)
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (entity is null)
            {
                return NotFound();
            }

            var normalizedCode = NormalizeRequired(request.Code, "Sales team code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Sales team name is required.");
            var memberIds = NormalizeEmployeeIds(request.MemberEmployeeIds);

            var validation = await ValidateRequestAsync(request.TeamLeaderId, normalizedCode, id, memberIds, ct);
            if (validation is not null)
            {
                return validation;
            }

            entity.Code = normalizedCode;
            entity.Name = normalizedName;
            entity.TeamLeaderId = request.TeamLeaderId;
            entity.IsActive = request.IsActive;
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            dbContext.SalSalesTeamMembers.RemoveRange(entity.Members);
            entity.Members.Clear();

            foreach (var memberId in memberIds)
            {
                entity.Members.Add(new SalSalesTeamMember
                {
                    EmployeeId = memberId,
                    CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
                });
            }

            await dbContext.SaveChangesAsync(ct);

            var updated = await dbContext.SalSalesTeams
                .AsNoTracking()
                .Include(x => x.TeamLeader)
                .Include(x => x.Members)
                    .ThenInclude(x => x.Employee)
                .FirstAsync(x => x.Id == id, ct);

            return Ok(MapDto(updated));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var entity = await dbContext.SalSalesTeams.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        var usedByCustomer = await dbContext.FinCustomers.AnyAsync(x => x.SalesTeamId == id, ct);
        if (usedByCustomer)
        {
            return BadRequest(new { message = "Sales team is used by customer." });
        }

        dbContext.SalSalesTeams.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task<IActionResult?> ValidateRequestAsync(int teamLeaderId, string code, int? id, IReadOnlyList<int> memberIds, CancellationToken ct)
    {
        if (teamLeaderId <= 0)
        {
            return BadRequest(new { message = "Team leader is required." });
        }

        var teamLeaderExists = await dbContext.HrEmployees.AnyAsync(x => x.Id == teamLeaderId, ct);
        if (!teamLeaderExists)
        {
            return BadRequest(new { message = "Team leader not found." });
        }

        var duplicate = await dbContext.SalSalesTeams
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id != (id ?? 0) && x.Code == code, ct);

        if (duplicate)
        {
            return BadRequest(new { message = "Sales team code already exists." });
        }

        if (memberIds.Count > 0)
        {
            var validEmployeeIds = await dbContext.HrEmployees
                .AsNoTracking()
                .Where(x => memberIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync(ct);

            if (validEmployeeIds.Count != memberIds.Count)
            {
                return BadRequest(new { message = "One or more team members are invalid." });
            }
        }

        return null;
    }

    private static SalesTeamDto MapDto(SalSalesTeam entity)
    {
        var members = entity.Members
            .OrderBy(x => x.Employee.FullName)
            .ToList();

        var memberIds = members
            .Select(x => x.EmployeeId)
            .Distinct()
            .ToList();

        var memberNames = members
            .Select(x => x.Employee.FullName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new SalesTeamDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            TeamLeaderId = entity.TeamLeaderId,
            TeamLeaderName = entity.TeamLeader.FullName,
            IsActive = entity.IsActive,
            MemberCount = memberIds.Count,
            MemberEmployeeIds = memberIds,
            MemberEmployeeNames = memberNames
        };
    }

    private static IReadOnlyList<int> NormalizeEmployeeIds(IReadOnlyList<int> ids)
    {
        return ids
            .Where(x => x > 0)
            .Distinct()
            .ToList();
    }

    private static string NormalizeRequired(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(message);
        }

        return value.Trim();
    }
}
