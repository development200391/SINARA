using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Entities.Finance;
using ERP.Domain.Enums;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Finance;

[Route("api/v1/finance/budgets")]
public sealed class BudgetsController(AppDbContext dbContext) : FinanceControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] BudgetPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.FinBudgets
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.BudgetNo.ToLower().Contains(search) ||
                x.Name.ToLower().Contains(search) ||
                (x.Notes != null && x.Notes.ToLower().Contains(search)) ||
                (x.CostCenter != null && x.CostCenter.Code.ToLower().Contains(search)) ||
                (x.CostCenter != null && x.CostCenter.Name.ToLower().Contains(search)) ||
                (x.Account != null && x.Account.Code.ToLower().Contains(search)) ||
                (x.Account != null && x.Account.Name.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.BudgetNo))
        {
            var budgetNo = request.BudgetNo.Trim().ToLowerInvariant();
            query = query.Where(x => x.BudgetNo.ToLower().Contains(budgetNo));
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var name = request.Name.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(name));
        }

        if (request.FiscalYearId.HasValue)
        {
            query = query.Where(x => x.FiscalYearId == request.FiscalYearId.Value);
        }

        if (request.PeriodId.HasValue)
        {
            query = query.Where(x => x.PeriodId == request.PeriodId.Value);
        }

        if (request.CostCenterId.HasValue)
        {
            query = query.Where(x => x.CostCenterId == request.CostCenterId.Value);
        }

        if (request.AccountId.HasValue)
        {
            query = query.Where(x => x.AccountId == request.AccountId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        if (request.AmountFrom.HasValue)
        {
            query = query.Where(x => x.TotalAmount >= request.AmountFrom.Value);
        }

        if (request.AmountTo.HasValue)
        {
            query = query.Where(x => x.TotalAmount <= request.AmountTo.Value);
        }

        var budgets = await query
            .Include(x => x.FiscalYear)
            .Include(x => x.Period)
            .Include(x => x.CostCenter)
            .Include(x => x.Account)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Period)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Account)
            .Include(x => x.Lines)
                .ThenInclude(x => x.CostCenter)
            .ToListAsync(ct);

        var lineActualMap = await BuildLineActualMapAsync(budgets.SelectMany(x => x.Lines).ToList(), ct);

        var rows = budgets
            .Select(x => MapBudgetDto(x, lineActualMap, includeLines: false))
            .ToList();

        if (request.ActualFrom.HasValue)
        {
            rows = rows.Where(x => x.TotalActualAmount >= request.ActualFrom.Value).ToList();
        }

        if (request.ActualTo.HasValue)
        {
            rows = rows.Where(x => x.TotalActualAmount <= request.ActualTo.Value).ToList();
        }

        if (request.VarianceFrom.HasValue)
        {
            rows = rows.Where(x => x.TotalVarianceAmount >= request.VarianceFrom.Value).ToList();
        }

        if (request.VarianceTo.HasValue)
        {
            rows = rows.Where(x => x.TotalVarianceAmount <= request.VarianceTo.Value).ToList();
        }

        var sorted = SortBudgetRows(rows, request.SortBy, isDesc);
        var totalCount = sorted.Count;

        var items = sorted
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(PagedResult<BudgetDto>.Create(items, totalCount, page, pageSize));
    }
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var budget = await dbContext.FinBudgets
            .AsNoTracking()
            .Include(x => x.FiscalYear)
            .Include(x => x.Period)
            .Include(x => x.CostCenter)
            .Include(x => x.Account)
            .Include(x => x.Lines.OrderBy(y => y.LineNo))
                .ThenInclude(x => x.Period)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Account)
            .Include(x => x.Lines)
                .ThenInclude(x => x.CostCenter)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (budget is null)
        {
            return NotFound();
        }

        var lineActualMap = await BuildLineActualMapAsync(budget.Lines.ToList(), ct);
        return Ok(MapBudgetDto(budget, lineActualMap, includeLines: true));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BudgetDto request, CancellationToken ct)
    {
        try
        {
            var normalizedBudgetNo = NormalizeRequired(request.BudgetNo, "Budget number is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Budget name is required.");

            if (request.Lines.Count == 0)
            {
                return BadRequest(new { message = "At least one budget line is required." });
            }

            if (await dbContext.FinBudgets.IgnoreQueryFilters().AnyAsync(x => x.BudgetNo == normalizedBudgetNo, ct))
            {
                return BadRequest(new { message = "Budget number already exists." });
            }

            await ValidateBudgetReferencesAsync(request, ct);

            var entity = new FinBudget
            {
                BudgetNo = normalizedBudgetNo,
                Name = normalizedName,
                FiscalYearId = request.FiscalYearId,
                PeriodId = request.PeriodId is > 0 ? request.PeriodId : null,
                CostCenterId = request.CostCenterId is > 0 ? request.CostCenterId : null,
                AccountId = request.AccountId is > 0 ? request.AccountId : null,
                CurrencyCode = string.IsNullOrWhiteSpace(request.CurrencyCode) ? "IDR" : request.CurrencyCode.Trim().ToUpperInvariant(),
                Notes = NormalizeOptional(request.Notes),
                IsActive = request.IsActive,
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system"
            };

            ApplyBudgetLines(entity, request.Lines);
            entity.TotalAmount = decimal.Round(entity.Lines.Sum(x => x.Amount), 4, MidpointRounding.AwayFromZero);

            dbContext.FinBudgets.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var created = await dbContext.FinBudgets
                .AsNoTracking()
                .Include(x => x.FiscalYear)
                .Include(x => x.Period)
                .Include(x => x.CostCenter)
                .Include(x => x.Account)
                .Include(x => x.Lines.OrderBy(y => y.LineNo))
                    .ThenInclude(x => x.Period)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.Account)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.CostCenter)
                .FirstAsync(x => x.Id == entity.Id, ct);

            var lineActualMap = await BuildLineActualMapAsync(created.Lines.ToList(), ct);
            return Ok(MapBudgetDto(created, lineActualMap, includeLines: true));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] BudgetDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.FinBudgets
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (entity is null)
            {
                return NotFound();
            }

            var normalizedBudgetNo = NormalizeRequired(request.BudgetNo, "Budget number is required.").ToUpperInvariant();
            var normalizedName = NormalizeRequired(request.Name, "Budget name is required.");

            if (request.Lines.Count == 0)
            {
                return BadRequest(new { message = "At least one budget line is required." });
            }

            if (await dbContext.FinBudgets.IgnoreQueryFilters().AnyAsync(x => x.BudgetNo == normalizedBudgetNo && x.Id != id, ct))
            {
                return BadRequest(new { message = "Budget number already exists." });
            }

            await ValidateBudgetReferencesAsync(request, ct);

            entity.BudgetNo = normalizedBudgetNo;
            entity.Name = normalizedName;
            entity.FiscalYearId = request.FiscalYearId;
            entity.PeriodId = request.PeriodId is > 0 ? request.PeriodId : null;
            entity.CostCenterId = request.CostCenterId is > 0 ? request.CostCenterId : null;
            entity.AccountId = request.AccountId is > 0 ? request.AccountId : null;
            entity.CurrencyCode = string.IsNullOrWhiteSpace(request.CurrencyCode) ? "IDR" : request.CurrencyCode.Trim().ToUpperInvariant();
            entity.Notes = NormalizeOptional(request.Notes);
            entity.IsActive = request.IsActive;
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            dbContext.FinBudgetLines.RemoveRange(entity.Lines);
            entity.Lines.Clear();
            ApplyBudgetLines(entity, request.Lines);
            entity.TotalAmount = decimal.Round(entity.Lines.Sum(x => x.Amount), 4, MidpointRounding.AwayFromZero);

            await dbContext.SaveChangesAsync(ct);

            var updated = await dbContext.FinBudgets
                .AsNoTracking()
                .Include(x => x.FiscalYear)
                .Include(x => x.Period)
                .Include(x => x.CostCenter)
                .Include(x => x.Account)
                .Include(x => x.Lines.OrderBy(y => y.LineNo))
                    .ThenInclude(x => x.Period)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.Account)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.CostCenter)
                .FirstAsync(x => x.Id == id, ct);

            var lineActualMap = await BuildLineActualMapAsync(updated.Lines.ToList(), ct);
            return Ok(MapBudgetDto(updated, lineActualMap, includeLines: true));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var entity = await dbContext.FinBudgets.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        dbContext.FinBudgets.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task ValidateBudgetReferencesAsync(BudgetDto request, CancellationToken ct)
    {
        if (!await dbContext.FinFiscalYears.AnyAsync(x => x.Id == request.FiscalYearId, ct))
        {
            throw new InvalidOperationException("Fiscal year not found.");
        }

        if (request.PeriodId is > 0 && !await dbContext.FinPeriods.AnyAsync(x => x.Id == request.PeriodId.Value, ct))
        {
            throw new InvalidOperationException("Period not found.");
        }

        if (request.CostCenterId is > 0 && !await dbContext.FinCostCenters.AnyAsync(x => x.Id == request.CostCenterId.Value, ct))
        {
            throw new InvalidOperationException("Cost center not found.");
        }

        if (request.AccountId is > 0 && !await dbContext.FinAccounts.AnyAsync(x => x.Id == request.AccountId.Value, ct))
        {
            throw new InvalidOperationException("Account not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.CurrencyCode))
        {
            var code = request.CurrencyCode.Trim().ToUpperInvariant();
            if (!await dbContext.FinCurrencies.AnyAsync(x => x.Code == code, ct))
            {
                throw new InvalidOperationException("Currency not found.");
            }
        }

        var lineNoSet = new HashSet<int>();

        for (var index = 0; index < request.Lines.Count; index++)
        {
            var line = request.Lines[index];

            if (line.PeriodId <= 0)
            {
                throw new InvalidOperationException($"Period is required at line {index + 1}.");
            }

            if (line.AccountId <= 0)
            {
                throw new InvalidOperationException($"Account is required at line {index + 1}.");
            }

            if (line.Amount < 0)
            {
                throw new InvalidOperationException($"Amount cannot be negative at line {index + 1}.");
            }

            var lineNo = line.LineNo <= 0 ? index + 1 : line.LineNo;
            if (!lineNoSet.Add(lineNo))
            {
                throw new InvalidOperationException($"Duplicate line number {lineNo}.");
            }
        }

        var periodIds = request.Lines.Select(x => x.PeriodId).Distinct().ToList();
        var periodCount = await dbContext.FinPeriods.CountAsync(x => periodIds.Contains(x.Id), ct);
        if (periodCount != periodIds.Count)
        {
            throw new InvalidOperationException("One or more budget line periods are invalid.");
        }

        var accountIds = request.Lines.Select(x => x.AccountId).Distinct().ToList();
        var accountCount = await dbContext.FinAccounts.CountAsync(x => accountIds.Contains(x.Id), ct);
        if (accountCount != accountIds.Count)
        {
            throw new InvalidOperationException("One or more budget line accounts are invalid.");
        }

        var costCenterIds = request.Lines.Where(x => x.CostCenterId is > 0).Select(x => x.CostCenterId!.Value).Distinct().ToList();
        if (costCenterIds.Count > 0)
        {
            var costCenterCount = await dbContext.FinCostCenters.CountAsync(x => costCenterIds.Contains(x.Id), ct);
            if (costCenterCount != costCenterIds.Count)
            {
                throw new InvalidOperationException("One or more budget line cost centers are invalid.");
            }
        }
    }

    private static void ApplyBudgetLines(FinBudget budget, IReadOnlyList<BudgetLineDto> lines)
    {
        for (var index = 0; index < lines.Count; index++)
        {
            var line = lines[index];
            var lineNo = line.LineNo <= 0 ? index + 1 : line.LineNo;

            budget.Lines.Add(new FinBudgetLine
            {
                LineNo = lineNo,
                PeriodId = line.PeriodId,
                AccountId = line.AccountId,
                CostCenterId = line.CostCenterId is > 0 ? line.CostCenterId : null,
                Description = NormalizeOptional(line.Description),
                Amount = decimal.Round(line.Amount, 4, MidpointRounding.AwayFromZero)
            });
        }
    }

    private async Task<IReadOnlyDictionary<int, decimal>> BuildLineActualMapAsync(IReadOnlyList<FinBudgetLine> lines, CancellationToken ct)
    {
        var result = new Dictionary<int, decimal>();
        if (lines.Count == 0)
        {
            return result;
        }

        var periodIds = lines.Select(x => x.PeriodId).Distinct().ToList();
        var accountIds = lines.Select(x => x.AccountId).Distinct().ToList();

        var aggregateRows = await dbContext.FinJournalEntryLines
            .AsNoTracking()
            .Where(x => !x.JournalEntry.IsDeleted && x.JournalEntry.Status != FinanceJournalStatus.Draft)
            .Where(x => periodIds.Contains(x.JournalEntry.PeriodId) && accountIds.Contains(x.AccountId))
            .GroupBy(x => new
            {
                PeriodId = x.JournalEntry.PeriodId,
                x.AccountId,
                x.CostCenterId
            })
            .Select(x => new
            {
                x.Key.PeriodId,
                x.Key.AccountId,
                x.Key.CostCenterId,
                Debit = x.Sum(y => y.DebitBase),
                Credit = x.Sum(y => y.CreditBase)
            })
            .ToListAsync(ct);

        var byPeriodAccount = aggregateRows
            .GroupBy(x => new { x.PeriodId, x.AccountId })
            .ToDictionary(
                x => (x.Key.PeriodId, x.Key.AccountId),
                x => (Debit: x.Sum(y => y.Debit), Credit: x.Sum(y => y.Credit)));

        var byPeriodAccountCostCenter = aggregateRows.ToDictionary(
            x => (x.PeriodId, x.AccountId, x.CostCenterId),
            x => (x.Debit, x.Credit));

        foreach (var line in lines)
        {
            decimal debit;
            decimal credit;

            if (line.CostCenterId.HasValue && byPeriodAccountCostCenter.TryGetValue((line.PeriodId, line.AccountId, line.CostCenterId), out var exact))
            {
                debit = exact.Debit;
                credit = exact.Credit;
            }
            else if (byPeriodAccount.TryGetValue((line.PeriodId, line.AccountId), out var total))
            {
                debit = total.Debit;
                credit = total.Credit;
            }
            else
            {
                debit = 0m;
                credit = 0m;
            }

            var actual = line.Account.NormalBalance == FinanceNormalBalance.Credit
                ? credit - debit
                : debit - credit;

            result[line.Id] = decimal.Round(actual, 4, MidpointRounding.AwayFromZero);
        }

        return result;
    }

    private static IReadOnlyList<BudgetDto> SortBudgetRows(IReadOnlyList<BudgetDto> rows, string? sortBy, bool isDesc)
    {
        var ordered = (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "budgetno" => isDesc ? rows.OrderByDescending(x => x.BudgetNo) : rows.OrderBy(x => x.BudgetNo),
            "name" => isDesc ? rows.OrderByDescending(x => x.Name) : rows.OrderBy(x => x.Name),
            "fiscalyear" => isDesc
                ? rows.OrderByDescending(x => x.FiscalYearName).ThenByDescending(x => x.BudgetNo)
                : rows.OrderBy(x => x.FiscalYearName).ThenBy(x => x.BudgetNo),
            "period" => isDesc
                ? rows.OrderByDescending(x => x.PeriodName ?? string.Empty).ThenByDescending(x => x.BudgetNo)
                : rows.OrderBy(x => x.PeriodName ?? string.Empty).ThenBy(x => x.BudgetNo),
            "costcenter" => isDesc
                ? rows.OrderByDescending(x => x.CostCenterCode ?? string.Empty).ThenByDescending(x => x.BudgetNo)
                : rows.OrderBy(x => x.CostCenterCode ?? string.Empty).ThenBy(x => x.BudgetNo),
            "account" => isDesc
                ? rows.OrderByDescending(x => x.AccountCode ?? string.Empty).ThenByDescending(x => x.BudgetNo)
                : rows.OrderBy(x => x.AccountCode ?? string.Empty).ThenBy(x => x.BudgetNo),
            "totalamount" => isDesc
                ? rows.OrderByDescending(x => x.TotalAmount).ThenByDescending(x => x.BudgetNo)
                : rows.OrderBy(x => x.TotalAmount).ThenBy(x => x.BudgetNo),
            "totalactualamount" => isDesc
                ? rows.OrderByDescending(x => x.TotalActualAmount).ThenByDescending(x => x.BudgetNo)
                : rows.OrderBy(x => x.TotalActualAmount).ThenBy(x => x.BudgetNo),
            "totalvarianceamount" => isDesc
                ? rows.OrderByDescending(x => x.TotalVarianceAmount).ThenByDescending(x => x.BudgetNo)
                : rows.OrderBy(x => x.TotalVarianceAmount).ThenBy(x => x.BudgetNo),
            "isactive" => isDesc
                ? rows.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.BudgetNo)
                : rows.OrderBy(x => x.IsActive).ThenBy(x => x.BudgetNo),
            _ => isDesc ? rows.OrderByDescending(x => x.BudgetNo) : rows.OrderBy(x => x.BudgetNo)
        };

        return ordered.ToList();
    }
    private static BudgetDto MapBudgetDto(FinBudget budget, IReadOnlyDictionary<int, decimal> lineActualMap, bool includeLines)
    {
        var lines = budget.Lines
            .OrderBy(x => x.LineNo)
            .Select(x =>
            {
                var actual = lineActualMap.TryGetValue(x.Id, out var value)
                    ? value
                    : 0m;

                return new BudgetLineDto
                {
                    Id = x.Id,
                    LineNo = x.LineNo,
                    PeriodId = x.PeriodId,
                    PeriodName = x.Period.Name,
                    AccountId = x.AccountId,
                    AccountCode = x.Account.Code,
                    AccountName = x.Account.Name,
                    CostCenterId = x.CostCenterId,
                    CostCenterCode = x.CostCenter?.Code,
                    CostCenterName = x.CostCenter?.Name,
                    Description = x.Description,
                    Amount = x.Amount,
                    ActualAmount = actual,
                    VarianceAmount = decimal.Round(x.Amount - actual, 4, MidpointRounding.AwayFromZero)
                };
            })
            .ToList();

        var totalBudget = decimal.Round(lines.Sum(x => x.Amount), 4, MidpointRounding.AwayFromZero);
        var totalActual = decimal.Round(lines.Sum(x => x.ActualAmount), 4, MidpointRounding.AwayFromZero);

        return new BudgetDto
        {
            Id = budget.Id,
            BudgetNo = budget.BudgetNo,
            Name = budget.Name,
            FiscalYearId = budget.FiscalYearId,
            FiscalYearName = budget.FiscalYear.Name,
            PeriodId = budget.PeriodId,
            PeriodName = budget.Period?.Name,
            CostCenterId = budget.CostCenterId,
            CostCenterCode = budget.CostCenter?.Code,
            CostCenterName = budget.CostCenter?.Name,
            AccountId = budget.AccountId,
            AccountCode = budget.Account?.Code,
            AccountName = budget.Account?.Name,
            CurrencyCode = budget.CurrencyCode,
            TotalAmount = totalBudget,
            TotalActualAmount = totalActual,
            TotalVarianceAmount = decimal.Round(totalBudget - totalActual, 4, MidpointRounding.AwayFromZero),
            Notes = budget.Notes,
            IsActive = budget.IsActive,
            Lines = includeLines ? lines : []
        };
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
}


