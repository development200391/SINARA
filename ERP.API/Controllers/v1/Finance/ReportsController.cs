using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Entities.Finance;
using ERP.Domain.Enums;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Finance;

[Route("api/v1/finance/reports")]
public sealed class ReportsController(AppDbContext dbContext) : FinanceControllerBase
{
    [HttpGet("trial-balance")]
    public async Task<IActionResult> TrialBalance([FromQuery] TrialBalancePagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = BuildBaseLineQuery(request.PeriodId, request.DateFrom, request.DateTo, request.CostCenterId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Account.Code.ToLower().Contains(search) ||
                x.Account.Name.ToLower().Contains(search));
        }

        if (request.AccountId.HasValue)
        {
            query = query.Where(x => x.AccountId == request.AccountId.Value);
        }

        if (request.Type.HasValue)
        {
            query = query.Where(x => x.Account.Type == request.Type.Value);
        }

        var aggregates = await query
            .GroupBy(x => new
            {
                x.AccountId,
                x.Account.Code,
                x.Account.Name,
                x.Account.Type,
                x.Account.NormalBalance
            })
            .Select(x => new AccountAggregateRow
            {
                AccountId = x.Key.AccountId,
                AccountCode = x.Key.Code,
                AccountName = x.Key.Name,
                AccountType = x.Key.Type,
                NormalBalance = x.Key.NormalBalance,
                TotalDebit = x.Sum(y => y.DebitBase),
                TotalCredit = x.Sum(y => y.CreditBase)
            })
            .ToListAsync(ct);

        var rows = aggregates
            .Select(x =>
            {
                var balance = x.TotalDebit - x.TotalCredit;
                return new TrialBalanceRowDto
                {
                    AccountId = x.AccountId,
                    AccountCode = x.AccountCode,
                    AccountName = x.AccountName,
                    AccountType = x.AccountType,
                    NormalBalance = x.NormalBalance,
                    TotalDebit = x.TotalDebit,
                    TotalCredit = x.TotalCredit,
                    Balance = balance,
                    EndingDebit = balance > 0m ? balance : 0m,
                    EndingCredit = balance < 0m ? -balance : 0m
                };
            })
            .ToList();

        var sorted = SortTrialBalanceRows(rows, request.SortBy, isDesc);
        var totalCount = rows.Count;

        var items = sorted
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(PagedResult<TrialBalanceRowDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("balance-sheet")]
    public async Task<IActionResult> BalanceSheet([FromQuery] FinancialStatementPagedRequest request, CancellationToken ct)
    {
        var accountTypes = new[]
        {
            FinanceAccountType.Asset,
            FinanceAccountType.Liability,
            FinanceAccountType.Equity
        };

        var result = await BuildFinancialStatementAsync(
            request,
            accountTypes,
            ResolveBalanceSheetSection,
            ResolveBalanceSheetAmount,
            ct);

        return Ok(result);
    }

    [HttpGet("profit-loss")]
    public async Task<IActionResult> ProfitLoss([FromQuery] FinancialStatementPagedRequest request, CancellationToken ct)
    {
        var accountTypes = new[]
        {
            FinanceAccountType.Revenue,
            FinanceAccountType.Expense
        };

        var result = await BuildFinancialStatementAsync(
            request,
            accountTypes,
            ResolveProfitLossSection,
            ResolveProfitLossAmount,
            ct);

        return Ok(result);
    }

    [HttpGet("cash-flow")]
    public async Task<IActionResult> CashFlow([FromQuery] FinancialStatementPagedRequest request, CancellationToken ct)
    {
        var accountTypes = new[]
        {
            FinanceAccountType.Asset,
            FinanceAccountType.Liability,
            FinanceAccountType.Equity,
            FinanceAccountType.Revenue,
            FinanceAccountType.Expense
        };

        var result = await BuildFinancialStatementAsync(
            request,
            accountTypes,
            ResolveCashFlowSection,
            ResolveCashFlowAmount,
            ct);

        return Ok(result);
    }


    [HttpGet("budget-vs-actual")]
    public async Task<IActionResult> BudgetVsActual([FromQuery] BudgetVsActualPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.FinBudgetLines
            .AsNoTracking()
            .Where(x => !x.Budget.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Budget.BudgetNo.ToLower().Contains(search) ||
                x.Budget.Name.ToLower().Contains(search) ||
                x.Account.Code.ToLower().Contains(search) ||
                x.Account.Name.ToLower().Contains(search) ||
                x.Period.Name.ToLower().Contains(search) ||
                (x.CostCenter != null && x.CostCenter.Code.ToLower().Contains(search)) ||
                (x.CostCenter != null && x.CostCenter.Name.ToLower().Contains(search)));
        }

        if (request.BudgetId.HasValue)
        {
            query = query.Where(x => x.BudgetId == request.BudgetId.Value);
        }

        if (request.FiscalYearId.HasValue)
        {
            query = query.Where(x => x.Budget.FiscalYearId == request.FiscalYearId.Value);
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

        var lines = await query
            .Include(x => x.Budget)
                .ThenInclude(x => x.FiscalYear)
            .Include(x => x.Account)
            .Include(x => x.Period)
            .Include(x => x.CostCenter)
            .ToListAsync(ct);

        var actualMap = await BuildBudgetLineActualMapAsync(lines, ct);

        var rows = lines
            .Select(x =>
            {
                var actual = actualMap.TryGetValue(x.Id, out var value) ? value : 0m;
                var variance = decimal.Round(x.Amount - actual, 4, MidpointRounding.AwayFromZero);
                var utilization = x.Amount <= 0m
                    ? 0m
                    : decimal.Round((actual / x.Amount) * 100m, 2, MidpointRounding.AwayFromZero);

                return new BudgetVsActualRowDto
                {
                    BudgetId = x.BudgetId,
                    BudgetNo = x.Budget.BudgetNo,
                    BudgetName = x.Budget.Name,
                    FiscalYearName = x.Budget.FiscalYear.Name,
                    PeriodId = x.PeriodId,
                    PeriodName = x.Period.Name,
                    CostCenterId = x.CostCenterId,
                    CostCenterCode = x.CostCenter?.Code,
                    CostCenterName = x.CostCenter?.Name,
                    AccountId = x.AccountId,
                    AccountCode = x.Account.Code,
                    AccountName = x.Account.Name,
                    BudgetAmount = x.Amount,
                    ActualAmount = actual,
                    VarianceAmount = variance,
                    UtilizationPercentage = utilization
                };
            })
            .ToList();

        if (request.BudgetFrom.HasValue)
        {
            rows = rows.Where(x => x.BudgetAmount >= request.BudgetFrom.Value).ToList();
        }

        if (request.BudgetTo.HasValue)
        {
            rows = rows.Where(x => x.BudgetAmount <= request.BudgetTo.Value).ToList();
        }

        if (request.ActualFrom.HasValue)
        {
            rows = rows.Where(x => x.ActualAmount >= request.ActualFrom.Value).ToList();
        }

        if (request.ActualTo.HasValue)
        {
            rows = rows.Where(x => x.ActualAmount <= request.ActualTo.Value).ToList();
        }

        if (request.VarianceFrom.HasValue)
        {
            rows = rows.Where(x => x.VarianceAmount >= request.VarianceFrom.Value).ToList();
        }

        if (request.VarianceTo.HasValue)
        {
            rows = rows.Where(x => x.VarianceAmount <= request.VarianceTo.Value).ToList();
        }

        var sorted = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "budgetno" => isDesc ? rows.OrderByDescending(x => x.BudgetNo) : rows.OrderBy(x => x.BudgetNo),
            "budgetname" => isDesc ? rows.OrderByDescending(x => x.BudgetName) : rows.OrderBy(x => x.BudgetName),
            "fiscalyear" => isDesc ? rows.OrderByDescending(x => x.FiscalYearName).ThenByDescending(x => x.BudgetNo) : rows.OrderBy(x => x.FiscalYearName).ThenBy(x => x.BudgetNo),
            "period" => isDesc ? rows.OrderByDescending(x => x.PeriodName).ThenByDescending(x => x.BudgetNo) : rows.OrderBy(x => x.PeriodName).ThenBy(x => x.BudgetNo),
            "costcenter" => isDesc ? rows.OrderByDescending(x => x.CostCenterCode).ThenByDescending(x => x.BudgetNo) : rows.OrderBy(x => x.CostCenterCode).ThenBy(x => x.BudgetNo),
            "account" => isDesc ? rows.OrderByDescending(x => x.AccountCode).ThenByDescending(x => x.BudgetNo) : rows.OrderBy(x => x.AccountCode).ThenBy(x => x.BudgetNo),
            "budgetamount" => isDesc ? rows.OrderByDescending(x => x.BudgetAmount) : rows.OrderBy(x => x.BudgetAmount),
            "actualamount" => isDesc ? rows.OrderByDescending(x => x.ActualAmount) : rows.OrderBy(x => x.ActualAmount),
            "varianceamount" => isDesc ? rows.OrderByDescending(x => x.VarianceAmount) : rows.OrderBy(x => x.VarianceAmount),
            "utilizationpercentage" => isDesc ? rows.OrderByDescending(x => x.UtilizationPercentage) : rows.OrderBy(x => x.UtilizationPercentage),
            _ => isDesc ? rows.OrderByDescending(x => x.BudgetNo).ThenByDescending(x => x.PeriodName) : rows.OrderBy(x => x.BudgetNo).ThenBy(x => x.PeriodName)
        };

        var totalCount = rows.Count;
        var items = sorted
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(PagedResult<BudgetVsActualRowDto>.Create(items, totalCount, page, pageSize));
    }
    private IQueryable<FinJournalEntryLine> BuildBaseLineQuery(int? periodId, DateOnly? dateFrom, DateOnly? dateTo, int? costCenterId)
    {
        var query = dbContext.FinJournalEntryLines
            .AsNoTracking()
            .Where(x => !x.JournalEntry.IsDeleted && x.JournalEntry.Status != FinanceJournalStatus.Draft)
            .AsQueryable();

        if (periodId.HasValue)
        {
            query = query.Where(x => x.JournalEntry.PeriodId == periodId.Value);
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(x => x.JournalEntry.Date >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(x => x.JournalEntry.Date <= dateTo.Value);
        }

        if (costCenterId.HasValue)
        {
            query = query.Where(x => x.CostCenterId == costCenterId.Value);
        }

        return query;
    }

    private async Task<PagedResult<FinancialStatementRowDto>> BuildFinancialStatementAsync(
        FinancialStatementPagedRequest request,
        IReadOnlyList<FinanceAccountType> includedTypes,
        Func<FinanceAccountType, string> sectionResolver,
        Func<FinanceAccountType, decimal, decimal, decimal> amountResolver,
        CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = BuildBaseLineQuery(request.PeriodId, request.DateFrom, request.DateTo, request.CostCenterId)
            .Where(x => includedTypes.Contains(x.Account.Type));

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Account.Code.ToLower().Contains(search) ||
                x.Account.Name.ToLower().Contains(search));
        }

        if (request.AccountType.HasValue)
        {
            query = query.Where(x => x.Account.Type == request.AccountType.Value);
        }

        var aggregates = await query
            .GroupBy(x => new
            {
                x.AccountId,
                x.Account.Code,
                x.Account.Name,
                x.Account.Type
            })
            .Select(x => new
            {
                x.Key.AccountId,
                x.Key.Code,
                x.Key.Name,
                x.Key.Type,
                TotalDebit = x.Sum(y => y.DebitBase),
                TotalCredit = x.Sum(y => y.CreditBase)
            })
            .ToListAsync(ct);

        var rows = aggregates
            .Select(x => new FinancialStatementRowDto
            {
                Section = sectionResolver(x.Type),
                AccountId = x.AccountId,
                AccountCode = x.Code,
                AccountName = x.Name,
                AccountType = x.Type,
                Amount = amountResolver(x.Type, x.TotalDebit, x.TotalCredit)
            })
            .Where(x => x.Amount != 0m)
            .ToList();

        if (!string.IsNullOrWhiteSpace(request.Section))
        {
            var section = request.Section.Trim();
            rows = rows
                .Where(x => string.Equals(x.Section, section, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var sorted = SortFinancialStatementRows(rows, request.SortBy, isDesc);
        var totalCount = rows.Count;

        var items = sorted
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return PagedResult<FinancialStatementRowDto>.Create(items, totalCount, page, pageSize);
    }

    private static IEnumerable<TrialBalanceRowDto> SortTrialBalanceRows(IReadOnlyList<TrialBalanceRowDto> rows, string? sortBy, bool isDesc)
    {
        return (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "accountcode" => isDesc ? rows.OrderByDescending(x => x.AccountCode) : rows.OrderBy(x => x.AccountCode),
            "accountname" => isDesc ? rows.OrderByDescending(x => x.AccountName) : rows.OrderBy(x => x.AccountName),
            "type" => isDesc ? rows.OrderByDescending(x => x.AccountType).ThenByDescending(x => x.AccountCode) : rows.OrderBy(x => x.AccountType).ThenBy(x => x.AccountCode),
            "totaldebit" => isDesc ? rows.OrderByDescending(x => x.TotalDebit) : rows.OrderBy(x => x.TotalDebit),
            "totalcredit" => isDesc ? rows.OrderByDescending(x => x.TotalCredit) : rows.OrderBy(x => x.TotalCredit),
            "balance" => isDesc ? rows.OrderByDescending(x => x.Balance) : rows.OrderBy(x => x.Balance),
            "endingdebit" => isDesc ? rows.OrderByDescending(x => x.EndingDebit) : rows.OrderBy(x => x.EndingDebit),
            "endingcredit" => isDesc ? rows.OrderByDescending(x => x.EndingCredit) : rows.OrderBy(x => x.EndingCredit),
            _ => isDesc ? rows.OrderByDescending(x => x.AccountCode) : rows.OrderBy(x => x.AccountCode)
        };
    }

    private static IEnumerable<FinancialStatementRowDto> SortFinancialStatementRows(IReadOnlyList<FinancialStatementRowDto> rows, string? sortBy, bool isDesc)
    {
        return (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "section" => isDesc ? rows.OrderByDescending(x => x.Section).ThenByDescending(x => x.AccountCode) : rows.OrderBy(x => x.Section).ThenBy(x => x.AccountCode),
            "accountcode" => isDesc ? rows.OrderByDescending(x => x.AccountCode) : rows.OrderBy(x => x.AccountCode),
            "accountname" => isDesc ? rows.OrderByDescending(x => x.AccountName) : rows.OrderBy(x => x.AccountName),
            "type" => isDesc ? rows.OrderByDescending(x => x.AccountType).ThenByDescending(x => x.AccountCode) : rows.OrderBy(x => x.AccountType).ThenBy(x => x.AccountCode),
            "amount" => isDesc ? rows.OrderByDescending(x => x.Amount) : rows.OrderBy(x => x.Amount),
            _ => isDesc ? rows.OrderByDescending(x => x.Section).ThenByDescending(x => x.AccountCode) : rows.OrderBy(x => x.Section).ThenBy(x => x.AccountCode)
        };
    }

    private static string ResolveBalanceSheetSection(FinanceAccountType accountType)
    {
        return accountType switch
        {
            FinanceAccountType.Asset => "Asset",
            FinanceAccountType.Liability => "Liability",
            FinanceAccountType.Equity => "Equity",
            _ => "Other"
        };
    }

    private static decimal ResolveBalanceSheetAmount(FinanceAccountType accountType, decimal totalDebit, decimal totalCredit)
    {
        return accountType switch
        {
            FinanceAccountType.Asset => totalDebit - totalCredit,
            FinanceAccountType.Liability => totalCredit - totalDebit,
            FinanceAccountType.Equity => totalCredit - totalDebit,
            _ => 0m
        };
    }

    private static string ResolveProfitLossSection(FinanceAccountType accountType)
    {
        return accountType switch
        {
            FinanceAccountType.Revenue => "Revenue",
            FinanceAccountType.Expense => "Expense",
            _ => "Other"
        };
    }

    private static decimal ResolveProfitLossAmount(FinanceAccountType accountType, decimal totalDebit, decimal totalCredit)
    {
        return accountType switch
        {
            FinanceAccountType.Revenue => totalCredit - totalDebit,
            FinanceAccountType.Expense => totalDebit - totalCredit,
            _ => 0m
        };
    }

    private static string ResolveCashFlowSection(FinanceAccountType accountType)
    {
        return accountType switch
        {
            FinanceAccountType.Revenue => "Operating",
            FinanceAccountType.Expense => "Operating",
            FinanceAccountType.Asset => "Investing",
            FinanceAccountType.Liability => "Financing",
            FinanceAccountType.Equity => "Financing",
            _ => "Other"
        };
    }

    private static decimal ResolveCashFlowAmount(FinanceAccountType accountType, decimal totalDebit, decimal totalCredit)
    {
        return accountType switch
        {
            FinanceAccountType.Revenue => totalCredit - totalDebit,
            FinanceAccountType.Expense => -(totalDebit - totalCredit),
            FinanceAccountType.Asset => -(totalDebit - totalCredit),
            FinanceAccountType.Liability => totalCredit - totalDebit,
            FinanceAccountType.Equity => totalCredit - totalDebit,
            _ => 0m
        };
    }


    private async Task<IReadOnlyDictionary<int, decimal>> BuildBudgetLineActualMapAsync(IReadOnlyList<FinBudgetLine> lines, CancellationToken ct)
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
    private sealed class AccountAggregateRow
    {
        public int AccountId { get; init; }
        public string AccountCode { get; init; } = string.Empty;
        public string AccountName { get; init; } = string.Empty;
        public FinanceAccountType AccountType { get; init; }
        public FinanceNormalBalance NormalBalance { get; init; }
        public decimal TotalDebit { get; init; }
        public decimal TotalCredit { get; init; }
    }
}

