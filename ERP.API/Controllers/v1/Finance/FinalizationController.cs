using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Enums;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Finance;

[Route("api/v1/finance/finalization")]
public sealed class FinalizationController(AppDbContext dbContext) : FinanceControllerBase
{
    [HttpGet("period-closing")]
    public async Task<IActionResult> PeriodClosing([FromQuery] PeriodClosingPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.FinPeriods
            .AsNoTracking()
            .Include(x => x.FiscalYear)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Name.ToLower().Contains(search) ||
                x.FiscalYear.Name.ToLower().Contains(search) ||
                x.PeriodNumber.ToString().Contains(search));
        }

        if (request.FiscalYearId.HasValue)
        {
            query = query.Where(x => x.FiscalYearId == request.FiscalYearId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        var periods = await query.ToListAsync(ct);
        if (periods.Count == 0)
        {
            return Ok(PagedResult<PeriodClosingRowDto>.Create([], 0, page, pageSize));
        }

        var periodIds = periods.Select(x => x.Id).Distinct().ToList();

        var draftMap = await dbContext.FinJournalEntries
            .AsNoTracking()
            .Where(x => periodIds.Contains(x.PeriodId) && x.Status == FinanceJournalStatus.Draft)
            .GroupBy(x => x.PeriodId)
            .Select(x => new { PeriodId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.PeriodId, x => x.Count, ct);

        var postedMap = await dbContext.FinJournalEntries
            .AsNoTracking()
            .Where(x => periodIds.Contains(x.PeriodId) && x.Status == FinanceJournalStatus.Posted)
            .GroupBy(x => x.PeriodId)
            .Select(x => new { PeriodId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.PeriodId, x => x.Count, ct);

        var pendingApMap = await dbContext.FinApInvoices
            .AsNoTracking()
            .Where(x => periodIds.Contains(x.PeriodId))
            .Where(x => x.Status != FinanceApInvoiceStatus.Paid && x.Status != FinanceApInvoiceStatus.Cancelled)
            .GroupBy(x => x.PeriodId)
            .Select(x => new { PeriodId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.PeriodId, x => x.Count, ct);

        var pendingArMap = await dbContext.FinArInvoices
            .AsNoTracking()
            .Where(x => periodIds.Contains(x.PeriodId))
            .Where(x => x.Status != FinanceArInvoiceStatus.Paid && x.Status != FinanceArInvoiceStatus.Cancelled)
            .GroupBy(x => x.PeriodId)
            .Select(x => new { PeriodId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.PeriodId, x => x.Count, ct);

        var netIncomeMap = await BuildNetIncomeLossByPeriodAsync(periodIds, ct);

        var rows = periods
            .Select(x =>
            {
                var draftCount = draftMap.TryGetValue(x.Id, out var draft) ? draft : 0;
                var postedCount = postedMap.TryGetValue(x.Id, out var posted) ? posted : 0;
                var pendingAp = pendingApMap.TryGetValue(x.Id, out var ap) ? ap : 0;
                var pendingAr = pendingArMap.TryGetValue(x.Id, out var ar) ? ar : 0;
                var netIncomeLoss = netIncomeMap.TryGetValue(x.Id, out var net) ? net : 0m;

                return new PeriodClosingRowDto
                {
                    PeriodId = x.Id,
                    FiscalYearId = x.FiscalYearId,
                    FiscalYearName = x.FiscalYear.Name,
                    PeriodNumber = x.PeriodNumber,
                    PeriodName = x.Name,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Status = x.Status,
                    DraftJournalCount = draftCount,
                    PostedJournalCount = postedCount,
                    PendingApInvoiceCount = pendingAp,
                    PendingArInvoiceCount = pendingAr,
                    NetIncomeLossAmount = netIncomeLoss,
                    CanClose = x.Status == FinancePeriodStatus.Open && draftCount == 0
                };
            })
            .ToList();

        if (request.DraftJournalFrom.HasValue)
        {
            rows = rows.Where(x => x.DraftJournalCount >= request.DraftJournalFrom.Value).ToList();
        }

        if (request.DraftJournalTo.HasValue)
        {
            rows = rows.Where(x => x.DraftJournalCount <= request.DraftJournalTo.Value).ToList();
        }

        if (request.PendingApFrom.HasValue)
        {
            rows = rows.Where(x => x.PendingApInvoiceCount >= request.PendingApFrom.Value).ToList();
        }

        if (request.PendingApTo.HasValue)
        {
            rows = rows.Where(x => x.PendingApInvoiceCount <= request.PendingApTo.Value).ToList();
        }

        if (request.PendingArFrom.HasValue)
        {
            rows = rows.Where(x => x.PendingArInvoiceCount >= request.PendingArFrom.Value).ToList();
        }

        if (request.PendingArTo.HasValue)
        {
            rows = rows.Where(x => x.PendingArInvoiceCount <= request.PendingArTo.Value).ToList();
        }

        if (request.NetIncomeLossFrom.HasValue)
        {
            rows = rows.Where(x => x.NetIncomeLossAmount >= request.NetIncomeLossFrom.Value).ToList();
        }

        if (request.NetIncomeLossTo.HasValue)
        {
            rows = rows.Where(x => x.NetIncomeLossAmount <= request.NetIncomeLossTo.Value).ToList();
        }

        var sorted = SortPeriodClosingRows(rows, request.SortBy, isDesc);
        var totalCount = sorted.Count;

        var items = sorted
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(PagedResult<PeriodClosingRowDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("smoke-tests")]
    public async Task<IActionResult> SmokeTests([FromQuery] SmokeTestPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var rows = await BuildSmokeRowsAsync(ct);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            rows = rows.Where(x =>
                    x.Category.ToLower().Contains(search) ||
                    x.CheckItem.ToLower().Contains(search) ||
                    x.Notes.ToLower().Contains(search))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            var category = request.Category.Trim();
            rows = rows
                .Where(x => string.Equals(x.Category, category, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (request.Passed.HasValue)
        {
            rows = rows.Where(x => x.Passed == request.Passed.Value).ToList();
        }

        var sorted = SortSmokeRows(rows, request.SortBy, isDesc);
        var totalCount = sorted.Count;

        var items = sorted
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(PagedResult<SmokeTestRowDto>.Create(items, totalCount, page, pageSize));
    }

    private async Task<IReadOnlyDictionary<int, decimal>> BuildNetIncomeLossByPeriodAsync(IReadOnlyList<int> periodIds, CancellationToken ct)
    {
        var result = new Dictionary<int, decimal>();
        if (periodIds.Count == 0)
        {
            return result;
        }

        var aggregates = await dbContext.FinJournalEntryLines
            .AsNoTracking()
            .Where(x => periodIds.Contains(x.JournalEntry.PeriodId) && x.JournalEntry.Status != FinanceJournalStatus.Draft)
            .Where(x => x.Account.Type == FinanceAccountType.Revenue || x.Account.Type == FinanceAccountType.Expense)
            .GroupBy(x => new
            {
                PeriodId = x.JournalEntry.PeriodId,
                x.Account.Type
            })
            .Select(x => new
            {
                x.Key.PeriodId,
                x.Key.Type,
                TotalDebit = x.Sum(y => y.DebitBase),
                TotalCredit = x.Sum(y => y.CreditBase)
            })
            .ToListAsync(ct);

        foreach (var periodId in periodIds)
        {
            var revenue = aggregates
                .Where(x => x.PeriodId == periodId && x.Type == FinanceAccountType.Revenue)
                .Aggregate((Debit: 0m, Credit: 0m), (acc, row) => (acc.Debit + row.TotalDebit, acc.Credit + row.TotalCredit));

            var expense = aggregates
                .Where(x => x.PeriodId == periodId && x.Type == FinanceAccountType.Expense)
                .Aggregate((Debit: 0m, Credit: 0m), (acc, row) => (acc.Debit + row.TotalDebit, acc.Credit + row.TotalCredit));

            var revenueNet = revenue.Credit - revenue.Debit;
            var expenseNet = expense.Debit - expense.Credit;
            var netIncomeLoss = decimal.Round(revenueNet - expenseNet, 4, MidpointRounding.AwayFromZero);

            result[periodId] = netIncomeLoss;
        }

        return result;
    }

    private async Task<List<SmokeTestRowDto>> BuildSmokeRowsAsync(CancellationToken ct)
    {
        var coaCount = await dbContext.FinAccounts.AsNoTracking().CountAsync(ct);
        var mappedCostCenterCount = await dbContext.FinCostCenters.AsNoTracking().CountAsync(x => x.DepartmentId.HasValue, ct);
        var openFiscalYearCount = await dbContext.FinFiscalYears.AsNoTracking().CountAsync(x => x.Status == FinancePeriodStatus.Open, ct);
        var periodCount = await dbContext.FinPeriods.AsNoTracking().CountAsync(ct);
        var draftJournalCount = await dbContext.FinJournalEntries.AsNoTracking().CountAsync(x => x.Status == FinanceJournalStatus.Draft, ct);
        var apInvoiceCount = await dbContext.FinApInvoices.AsNoTracking().CountAsync(ct);
        var arInvoiceCount = await dbContext.FinArInvoices.AsNoTracking().CountAsync(ct);
        var exchangeRateCount = await dbContext.FinExchangeRates.AsNoTracking().CountAsync(ct);
        var budgetLineCount = await dbContext.FinBudgetLines.AsNoTracking().CountAsync(ct);

        var trialBalanceAggregate = await dbContext.FinJournalEntryLines
            .AsNoTracking()
            .Where(x => x.JournalEntry.Status != FinanceJournalStatus.Draft)
            .GroupBy(_ => 1)
            .Select(x => new
            {
                TotalDebit = x.Sum(y => y.DebitBase),
                TotalCredit = x.Sum(y => y.CreditBase)
            })
            .FirstOrDefaultAsync(ct);

        var totalDebit = trialBalanceAggregate?.TotalDebit ?? 0m;
        var totalCredit = trialBalanceAggregate?.TotalCredit ?? 0m;
        var trialBalanceDiff = decimal.Round(decimal.Abs(totalDebit - totalCredit), 4, MidpointRounding.AwayFromZero);

        return
        [
            new SmokeTestRowDto
            {
                SortOrder = 1,
                Category = "Master Data",
                CheckItem = "CoA Seeded",
                ExpectedValue = 30m,
                ActualValue = coaCount,
                Passed = coaCount >= 30,
                Notes = "Minimum recommended seeded records."
            },
            new SmokeTestRowDto
            {
                SortOrder = 2,
                Category = "Master Data",
                CheckItem = "Cost Center Linked To Department",
                ExpectedValue = 1m,
                ActualValue = mappedCostCenterCount,
                Passed = mappedCostCenterCount >= 1,
                Notes = "Minimum recommended seeded records."
            },
            new SmokeTestRowDto
            {
                SortOrder = 3,
                Category = "Master Data",
                CheckItem = "Open Fiscal Year Available",
                ExpectedValue = 1m,
                ActualValue = openFiscalYearCount,
                Passed = openFiscalYearCount >= 1,
                Notes = "Minimum recommended seeded records."
            },
            new SmokeTestRowDto
            {
                SortOrder = 4,
                Category = "Master Data",
                CheckItem = "Accounting Period Generated",
                ExpectedValue = 12m,
                ActualValue = periodCount,
                Passed = periodCount >= 12,
                Notes = "Minimum recommended seeded records."
            },
            new SmokeTestRowDto
            {
                SortOrder = 5,
                Category = "Transactions",
                CheckItem = "Draft Journal Remaining",
                ExpectedValue = 0m,
                ActualValue = draftJournalCount,
                Passed = draftJournalCount == 0,
                Notes = "Draft journals should be zero before closing."
            },
            new SmokeTestRowDto
            {
                SortOrder = 6,
                Category = "Transactions",
                CheckItem = "AP Invoices Available",
                ExpectedValue = 1m,
                ActualValue = apInvoiceCount,
                Passed = apInvoiceCount >= 1,
                Notes = "Minimum recommended seeded records."
            },
            new SmokeTestRowDto
            {
                SortOrder = 7,
                Category = "Transactions",
                CheckItem = "AR Invoices Available",
                ExpectedValue = 1m,
                ActualValue = arInvoiceCount,
                Passed = arInvoiceCount >= 1,
                Notes = "Minimum recommended seeded records."
            },
            new SmokeTestRowDto
            {
                SortOrder = 8,
                Category = "Transactions",
                CheckItem = "Exchange Rates Available",
                ExpectedValue = 1m,
                ActualValue = exchangeRateCount,
                Passed = exchangeRateCount >= 1,
                Notes = "Minimum recommended seeded records."
            },
            new SmokeTestRowDto
            {
                SortOrder = 9,
                Category = "Transactions",
                CheckItem = "Budget Lines Available",
                ExpectedValue = 1m,
                ActualValue = budgetLineCount,
                Passed = budgetLineCount >= 1,
                Notes = "Minimum recommended seeded records."
            },
            new SmokeTestRowDto
            {
                SortOrder = 10,
                Category = "Reports",
                CheckItem = "Trial Balance Is Balanced",
                ExpectedValue = 0m,
                ActualValue = trialBalanceDiff,
                Passed = trialBalanceDiff == 0m,
                Notes = "Debit and credit totals should be equal."
            }
        ];
    }

    private static IReadOnlyList<PeriodClosingRowDto> SortPeriodClosingRows(IReadOnlyList<PeriodClosingRowDto> rows, string? sortBy, bool isDesc)
    {
        return (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "fiscalyearname" => isDesc ? rows.OrderByDescending(x => x.FiscalYearName).ThenByDescending(x => x.PeriodNumber).ToList() : rows.OrderBy(x => x.FiscalYearName).ThenBy(x => x.PeriodNumber).ToList(),
            "periodnumber" => isDesc ? rows.OrderByDescending(x => x.PeriodNumber).ThenByDescending(x => x.FiscalYearName).ToList() : rows.OrderBy(x => x.PeriodNumber).ThenBy(x => x.FiscalYearName).ToList(),
            "periodname" => isDesc ? rows.OrderByDescending(x => x.PeriodName).ThenByDescending(x => x.PeriodNumber).ToList() : rows.OrderBy(x => x.PeriodName).ThenBy(x => x.PeriodNumber).ToList(),
            "startdate" => isDesc ? rows.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.PeriodNumber).ToList() : rows.OrderBy(x => x.StartDate).ThenBy(x => x.PeriodNumber).ToList(),
            "enddate" => isDesc ? rows.OrderByDescending(x => x.EndDate).ThenByDescending(x => x.PeriodNumber).ToList() : rows.OrderBy(x => x.EndDate).ThenBy(x => x.PeriodNumber).ToList(),
            "status" => isDesc ? rows.OrderByDescending(x => x.Status).ThenByDescending(x => x.PeriodNumber).ToList() : rows.OrderBy(x => x.Status).ThenBy(x => x.PeriodNumber).ToList(),
            "draftjournalcount" => isDesc ? rows.OrderByDescending(x => x.DraftJournalCount).ThenByDescending(x => x.PeriodNumber).ToList() : rows.OrderBy(x => x.DraftJournalCount).ThenBy(x => x.PeriodNumber).ToList(),
            "postedjournalcount" => isDesc ? rows.OrderByDescending(x => x.PostedJournalCount).ThenByDescending(x => x.PeriodNumber).ToList() : rows.OrderBy(x => x.PostedJournalCount).ThenBy(x => x.PeriodNumber).ToList(),
            "pendingapinvoicecount" => isDesc ? rows.OrderByDescending(x => x.PendingApInvoiceCount).ThenByDescending(x => x.PeriodNumber).ToList() : rows.OrderBy(x => x.PendingApInvoiceCount).ThenBy(x => x.PeriodNumber).ToList(),
            "pendingarinvoicecount" => isDesc ? rows.OrderByDescending(x => x.PendingArInvoiceCount).ThenByDescending(x => x.PeriodNumber).ToList() : rows.OrderBy(x => x.PendingArInvoiceCount).ThenBy(x => x.PeriodNumber).ToList(),
            "netincomelossamount" => isDesc ? rows.OrderByDescending(x => x.NetIncomeLossAmount).ThenByDescending(x => x.PeriodNumber).ToList() : rows.OrderBy(x => x.NetIncomeLossAmount).ThenBy(x => x.PeriodNumber).ToList(),
            _ => isDesc ? rows.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.PeriodNumber).ToList() : rows.OrderBy(x => x.StartDate).ThenBy(x => x.PeriodNumber).ToList()
        };
    }

    private static IReadOnlyList<SmokeTestRowDto> SortSmokeRows(IReadOnlyList<SmokeTestRowDto> rows, string? sortBy, bool isDesc)
    {
        return (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "sortorder" => isDesc ? rows.OrderByDescending(x => x.SortOrder).ToList() : rows.OrderBy(x => x.SortOrder).ToList(),
            "category" => isDesc ? rows.OrderByDescending(x => x.Category).ThenByDescending(x => x.SortOrder).ToList() : rows.OrderBy(x => x.Category).ThenBy(x => x.SortOrder).ToList(),
            "checkitem" => isDesc ? rows.OrderByDescending(x => x.CheckItem).ThenByDescending(x => x.SortOrder).ToList() : rows.OrderBy(x => x.CheckItem).ThenBy(x => x.SortOrder).ToList(),
            "expectedvalue" => isDesc ? rows.OrderByDescending(x => x.ExpectedValue).ThenByDescending(x => x.SortOrder).ToList() : rows.OrderBy(x => x.ExpectedValue).ThenBy(x => x.SortOrder).ToList(),
            "actualvalue" => isDesc ? rows.OrderByDescending(x => x.ActualValue).ThenByDescending(x => x.SortOrder).ToList() : rows.OrderBy(x => x.ActualValue).ThenBy(x => x.SortOrder).ToList(),
            "passed" => isDesc ? rows.OrderByDescending(x => x.Passed).ThenByDescending(x => x.SortOrder).ToList() : rows.OrderBy(x => x.Passed).ThenBy(x => x.SortOrder).ToList(),
            _ => isDesc ? rows.OrderByDescending(x => x.SortOrder).ToList() : rows.OrderBy(x => x.SortOrder).ToList()
        };
    }
}