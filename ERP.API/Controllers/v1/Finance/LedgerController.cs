using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Enums;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Finance;

[Route("api/v1/finance/ledger")]
public sealed class LedgerController(AppDbContext dbContext) : FinanceControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] LedgerPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.FinJournalEntryLines
            .AsNoTracking()
            .Where(x => !x.JournalEntry.IsDeleted && x.JournalEntry.Status != FinanceJournalStatus.Draft)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Account.Code.ToLower().Contains(search) ||
                x.Account.Name.ToLower().Contains(search) ||
                x.JournalEntry.JournalNo.ToLower().Contains(search) ||
                x.JournalEntry.Description.ToLower().Contains(search) ||
                (x.Description != null && x.Description.ToLower().Contains(search)));
        }

        if (request.AccountId.HasValue)
        {
            query = query.Where(x => x.AccountId == request.AccountId.Value);
        }

        if (request.PeriodId.HasValue)
        {
            query = query.Where(x => x.JournalEntry.PeriodId == request.PeriodId.Value);
        }

        if (request.CostCenterId.HasValue)
        {
            query = query.Where(x => x.CostCenterId == request.CostCenterId.Value);
        }

        if (request.DateFrom.HasValue)
        {
            query = query.Where(x => x.JournalEntry.Date >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(x => x.JournalEntry.Date <= request.DateTo.Value);
        }

        var rawRows = await query
            .Select(x => new LedgerRawRow
            {
                LineId = x.Id,
                LineNo = x.LineNo,
                AccountId = x.AccountId,
                AccountCode = x.Account.Code,
                AccountName = x.Account.Name,
                Date = x.JournalEntry.Date,
                JournalNo = x.JournalEntry.JournalNo,
                JournalDescription = x.JournalEntry.Description,
                LineDescription = x.Description,
                Debit = x.DebitBase,
                Credit = x.CreditBase,
                PeriodId = x.JournalEntry.PeriodId,
                PeriodName = x.JournalEntry.Period.Name,
                CostCenterId = x.CostCenterId,
                CostCenterCode = x.CostCenter != null ? x.CostCenter.Code : null,
                CostCenterName = x.CostCenter != null ? x.CostCenter.Name : null,
                Source = x.JournalEntry.Source
            })
            .ToListAsync(ct);

        var naturalRows = rawRows
            .OrderBy(x => x.AccountCode)
            .ThenBy(x => x.Date)
            .ThenBy(x => x.JournalNo)
            .ThenBy(x => x.LineNo)
            .ThenBy(x => x.LineId)
            .ToList();

        var balances = new Dictionary<int, decimal>();
        var computed = new List<LedgerEntryDto>(naturalRows.Count);

        foreach (var row in naturalRows)
        {
            var current = balances.GetValueOrDefault(row.AccountId);
            current += row.Debit - row.Credit;
            balances[row.AccountId] = current;

            computed.Add(new LedgerEntryDto
            {
                AccountId = row.AccountId,
                AccountCode = row.AccountCode,
                AccountName = row.AccountName,
                Date = row.Date,
                JournalNo = row.JournalNo,
                JournalDescription = row.JournalDescription,
                LineDescription = row.LineDescription,
                Debit = row.Debit,
                Credit = row.Credit,
                Balance = current,
                PeriodId = row.PeriodId,
                PeriodName = row.PeriodName,
                CostCenterId = row.CostCenterId,
                CostCenterCode = row.CostCenterCode,
                CostCenterName = row.CostCenterName,
                Source = row.Source
            });
        }

        var sorted = SortRows(computed, request.SortBy, isDesc);
        var totalCount = computed.Count;

        var items = sorted
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(PagedResult<LedgerEntryDto>.Create(items, totalCount, page, pageSize));
    }

    private static IEnumerable<LedgerEntryDto> SortRows(IReadOnlyList<LedgerEntryDto> rows, string? sortBy, bool isDesc)
    {
        return (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "accountcode" => isDesc ? rows.OrderByDescending(x => x.AccountCode) : rows.OrderBy(x => x.AccountCode),
            "accountname" => isDesc ? rows.OrderByDescending(x => x.AccountName) : rows.OrderBy(x => x.AccountName),
            "date" => isDesc ? rows.OrderByDescending(x => x.Date).ThenByDescending(x => x.JournalNo) : rows.OrderBy(x => x.Date).ThenBy(x => x.JournalNo),
            "journalno" => isDesc ? rows.OrderByDescending(x => x.JournalNo) : rows.OrderBy(x => x.JournalNo),
            "periodname" => isDesc ? rows.OrderByDescending(x => x.PeriodName).ThenByDescending(x => x.Date) : rows.OrderBy(x => x.PeriodName).ThenBy(x => x.Date),
            "debit" => isDesc ? rows.OrderByDescending(x => x.Debit) : rows.OrderBy(x => x.Debit),
            "credit" => isDesc ? rows.OrderByDescending(x => x.Credit) : rows.OrderBy(x => x.Credit),
            "balance" => isDesc ? rows.OrderByDescending(x => x.Balance) : rows.OrderBy(x => x.Balance),
            _ => rows
        };
    }

    private sealed class LedgerRawRow
    {
        public int LineId { get; init; }
        public int LineNo { get; init; }
        public int AccountId { get; init; }
        public string AccountCode { get; init; } = string.Empty;
        public string AccountName { get; init; } = string.Empty;
        public DateOnly Date { get; init; }
        public string JournalNo { get; init; } = string.Empty;
        public string JournalDescription { get; init; } = string.Empty;
        public string? LineDescription { get; init; }
        public decimal Debit { get; init; }
        public decimal Credit { get; init; }
        public int PeriodId { get; init; }
        public string PeriodName { get; init; } = string.Empty;
        public int? CostCenterId { get; init; }
        public string? CostCenterCode { get; init; }
        public string? CostCenterName { get; init; }
        public FinanceJournalSource Source { get; init; }
    }
}

