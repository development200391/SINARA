using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Entities.Finance;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Finance;

[Route("api/v1/finance/accounts")]
public sealed class AccountsController(AppDbContext dbContext) : FinanceControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] AccountPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.FinAccounts
            .AsNoTracking()
            .Include(x => x.Group)
            .Include(x => x.ParentAccount)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(search) ||
                x.Name.ToLower().Contains(search) ||
                x.Group.Name.ToLower().Contains(search));
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

        if (request.GroupId.HasValue)
        {
            query = query.Where(x => x.GroupId == request.GroupId.Value);
        }

        if (request.Type.HasValue)
        {
            query = query.Where(x => x.Type == request.Type.Value);
        }

        if (request.NormalBalance.HasValue)
        {
            query = query.Where(x => x.NormalBalance == request.NormalBalance.Value);
        }

        if (request.IsHeader.HasValue)
        {
            query = query.Where(x => x.IsHeader == request.IsHeader.Value);
        }

        if (request.ParentAccountId.HasValue)
        {
            query = query.Where(x => x.ParentAccountId == request.ParentAccountId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.CurrencyCode))
        {
            var currencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
            query = query.Where(x => x.CurrencyCode == currencyCode);
        }

        if (request.IsBankAccount.HasValue)
        {
            query = query.Where(x => x.IsBankAccount == request.IsBankAccount.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "name" => isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "groupname" => isDesc ? query.OrderByDescending(x => x.Group.Name).ThenByDescending(x => x.Code) : query.OrderBy(x => x.Group.Name).ThenBy(x => x.Code),
            "type" => isDesc ? query.OrderByDescending(x => x.Type).ThenByDescending(x => x.Code) : query.OrderBy(x => x.Type).ThenBy(x => x.Code),
            "normalbalance" => isDesc ? query.OrderByDescending(x => x.NormalBalance).ThenByDescending(x => x.Code) : query.OrderBy(x => x.NormalBalance).ThenBy(x => x.Code),
            "isheader" => isDesc ? query.OrderByDescending(x => x.IsHeader).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsHeader).ThenBy(x => x.Code),
            "currencycode" => isDesc ? query.OrderByDescending(x => x.CurrencyCode).ThenByDescending(x => x.Code) : query.OrderBy(x => x.CurrencyCode).ThenBy(x => x.Code),
            "isbankaccount" => isDesc ? query.OrderByDescending(x => x.IsBankAccount).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsBankAccount).ThenBy(x => x.Code),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Code),
            _ => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapDto(x))
            .ToListAsync(ct);

        return Ok(PagedResult<AccountDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.FinAccounts
            .AsNoTracking()
            .Include(x => x.Group)
            .Include(x => x.ParentAccount)
            .Where(x => x.Id == id)
            .Select(x => MapDto(x))
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AccountDto request, CancellationToken ct)
    {
        try
        {
            var normalizedCode = NormalizeRequired(request.Code, "Account code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Account name is required.");
            var normalizedCurrencyCode = NormalizeRequired(request.CurrencyCode, "Currency code is required.").ToUpperInvariant();

            if (request.GroupId <= 0)
            {
                return BadRequest(new { message = "Account group is required." });
            }

            var groupExists = await dbContext.FinAccountGroups.AnyAsync(x => x.Id == request.GroupId, ct);
            if (!groupExists)
            {
                return BadRequest(new { message = "Account group not found." });
            }

            var currencyExists = await dbContext.FinCurrencies.AnyAsync(x => x.Code == normalizedCurrencyCode, ct);
            if (!currencyExists)
            {
                return BadRequest(new { message = "Currency not found." });
            }

            var parentAccountId = request.ParentAccountId is > 0 ? request.ParentAccountId : null;
            if (parentAccountId.HasValue)
            {
                var parentExists = await dbContext.FinAccounts.AnyAsync(x => x.Id == parentAccountId.Value, ct);
                if (!parentExists)
                {
                    return BadRequest(new { message = "Parent account not found." });
                }
            }

            var duplicate = await dbContext.FinAccounts
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Code == normalizedCode, ct);
            if (duplicate)
            {
                return BadRequest(new { message = "Account code already exists." });
            }

            var entity = new FinAccount
            {
                Code = normalizedCode,
                Name = normalizedName,
                GroupId = request.GroupId,
                Type = request.Type,
                NormalBalance = request.NormalBalance,
                IsHeader = request.IsHeader,
                ParentAccountId = parentAccountId,
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                IsBankAccount = request.IsBankAccount,
                BankName = request.IsBankAccount ? NormalizeOptional(request.BankName) : null,
                BankAccountNo = request.IsBankAccount ? NormalizeOptional(request.BankAccountNo) : null,
                CurrencyCode = normalizedCurrencyCode,
                IsActive = request.IsActive,
                CreatedBy = "system"
            };

            dbContext.FinAccounts.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var result = await dbContext.FinAccounts
                .AsNoTracking()
                .Include(x => x.Group)
                .Include(x => x.ParentAccount)
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
    public async Task<IActionResult> Update(int id, [FromBody] AccountDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.FinAccounts.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                return NotFound();
            }

            var normalizedCode = NormalizeRequired(request.Code, "Account code is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Account name is required.");
            var normalizedCurrencyCode = NormalizeRequired(request.CurrencyCode, "Currency code is required.").ToUpperInvariant();

            if (request.GroupId <= 0)
            {
                return BadRequest(new { message = "Account group is required." });
            }

            var groupExists = await dbContext.FinAccountGroups.AnyAsync(x => x.Id == request.GroupId, ct);
            if (!groupExists)
            {
                return BadRequest(new { message = "Account group not found." });
            }

            var currencyExists = await dbContext.FinCurrencies.AnyAsync(x => x.Code == normalizedCurrencyCode, ct);
            if (!currencyExists)
            {
                return BadRequest(new { message = "Currency not found." });
            }

            var parentAccountId = request.ParentAccountId is > 0 ? request.ParentAccountId : null;
            if (parentAccountId == id)
            {
                return BadRequest(new { message = "Parent account is invalid." });
            }

            if (parentAccountId.HasValue)
            {
                var parentExists = await dbContext.FinAccounts.AnyAsync(x => x.Id == parentAccountId.Value, ct);
                if (!parentExists)
                {
                    return BadRequest(new { message = "Parent account not found." });
                }
            }

            var duplicate = await dbContext.FinAccounts
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Id != id && x.Code == normalizedCode, ct);
            if (duplicate)
            {
                return BadRequest(new { message = "Account code already exists." });
            }

            entity.Code = normalizedCode;
            entity.Name = normalizedName;
            entity.GroupId = request.GroupId;
            entity.Type = request.Type;
            entity.NormalBalance = request.NormalBalance;
            entity.IsHeader = request.IsHeader;
            entity.ParentAccountId = parentAccountId;
            entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            entity.IsBankAccount = request.IsBankAccount;
            entity.BankName = request.IsBankAccount ? NormalizeOptional(request.BankName) : null;
            entity.BankAccountNo = request.IsBankAccount ? NormalizeOptional(request.BankAccountNo) : null;
            entity.CurrencyCode = normalizedCurrencyCode;
            entity.IsActive = request.IsActive;
            entity.UpdatedBy = "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(ct);

            var result = await dbContext.FinAccounts
                .AsNoTracking()
                .Include(x => x.Group)
                .Include(x => x.ParentAccount)
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
        var entity = await dbContext.FinAccounts.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        var hasChild = await dbContext.FinAccounts.AnyAsync(x => x.ParentAccountId == id, ct);
        if (hasChild)
        {
            return BadRequest(new { message = "Account cannot be deleted because it has child accounts." });
        }

        var usedByCostCenter = await dbContext.FinCostCenters.AnyAsync(x => x.BudgetAccountId == id, ct);
        if (usedByCostCenter)
        {
            return BadRequest(new { message = "Account cannot be deleted because it is used by cost centers." });
        }

        var usedByTaxCode = await dbContext.FinTaxCodes.AnyAsync(x => x.AccountId == id, ct);
        if (usedByTaxCode)
        {
            return BadRequest(new { message = "Account cannot be deleted because it is used by tax codes." });
        }

        dbContext.FinAccounts.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private static AccountDto MapDto(FinAccount entity)
    {
        return new AccountDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            GroupId = entity.GroupId,
            GroupName = entity.Group?.Name ?? string.Empty,
            Type = entity.Type,
            NormalBalance = entity.NormalBalance,
            IsHeader = entity.IsHeader,
            ParentAccountId = entity.ParentAccountId,
            ParentAccountName = entity.ParentAccount?.Name,
            Description = entity.Description,
            IsBankAccount = entity.IsBankAccount,
            BankName = entity.BankName,
            BankAccountNo = entity.BankAccountNo,
            CurrencyCode = entity.CurrencyCode,
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

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
