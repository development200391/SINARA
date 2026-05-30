using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Entities.Finance;
using ERP.Domain.Enums;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Finance;

[Route("api/v1/finance/journals")]
public sealed class JournalsController(AppDbContext dbContext) : FinanceControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] JournalPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.FinJournalEntries
            .AsNoTracking()
            .Include(x => x.Period)
            .Include(x => x.PostedByUser)
            .Include(x => x.Lines)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.JournalNo.ToLower().Contains(search) ||
                x.Description.ToLower().Contains(search) ||
                (x.SourceRefType != null && x.SourceRefType.ToLower().Contains(search)) ||
                x.Period.Name.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.JournalNo))
        {
            var journalNo = request.JournalNo.Trim().ToLowerInvariant();
            query = query.Where(x => x.JournalNo.ToLower().Contains(journalNo));
        }

        if (request.DateFrom.HasValue)
        {
            query = query.Where(x => x.Date >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(x => x.Date <= request.DateTo.Value);
        }

        if (request.Source.HasValue)
        {
            query = query.Where(x => x.Source == request.Source.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (request.PeriodId.HasValue)
        {
            query = query.Where(x => x.PeriodId == request.PeriodId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SourceRefType))
        {
            var sourceRefType = request.SourceRefType.Trim().ToLowerInvariant();
            query = query.Where(x => x.SourceRefType != null && x.SourceRefType.ToLower().Contains(sourceRefType));
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "journalno" => isDesc ? query.OrderByDescending(x => x.JournalNo) : query.OrderBy(x => x.JournalNo),
            "date" => isDesc ? query.OrderByDescending(x => x.Date).ThenByDescending(x => x.JournalNo) : query.OrderBy(x => x.Date).ThenBy(x => x.JournalNo),
            "periodname" => isDesc ? query.OrderByDescending(x => x.Period.Name).ThenByDescending(x => x.Date) : query.OrderBy(x => x.Period.Name).ThenBy(x => x.Date),
            "source" => isDesc ? query.OrderByDescending(x => x.Source).ThenByDescending(x => x.Date) : query.OrderBy(x => x.Source).ThenBy(x => x.Date),
            "status" => isDesc ? query.OrderByDescending(x => x.Status).ThenByDescending(x => x.Date) : query.OrderBy(x => x.Status).ThenBy(x => x.Date),
            "postedat" => isDesc ? query.OrderByDescending(x => x.PostedAt).ThenByDescending(x => x.Date) : query.OrderBy(x => x.PostedAt).ThenBy(x => x.Date),
            "createdat" => isDesc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            _ => isDesc ? query.OrderByDescending(x => x.Date).ThenByDescending(x => x.JournalNo) : query.OrderBy(x => x.Date).ThenBy(x => x.JournalNo)
        };

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new JournalEntryDto
            {
                Id = x.Id,
                JournalNo = x.JournalNo,
                PeriodId = x.PeriodId,
                PeriodName = x.Period.Name,
                Date = x.Date,
                Description = x.Description,
                Source = x.Source,
                SourceRefId = x.SourceRefId,
                SourceRefType = x.SourceRefType,
                Status = x.Status,
                PostedBy = x.PostedBy,
                PostedByName = x.PostedByUser != null ? x.PostedByUser.FullName : null,
                PostedAt = x.PostedAt,
                ReversedJournalId = x.ReversedJournalId,
                CurrencyCode = x.CurrencyCode,
                ExchangeRate = x.ExchangeRate,
                TotalDebit = x.Lines.Select(l => (decimal?)l.Debit).Sum() ?? 0m,
                TotalCredit = x.Lines.Select(l => (decimal?)l.Credit).Sum() ?? 0m,
                TotalDebitBase = x.Lines.Select(l => (decimal?)l.DebitBase).Sum() ?? 0m,
                TotalCreditBase = x.Lines.Select(l => (decimal?)l.CreditBase).Sum() ?? 0m
            })
            .ToListAsync(ct);

        return Ok(PagedResult<JournalEntryDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var entity = await dbContext.FinJournalEntries
            .AsNoTracking()
            .Include(x => x.Period)
            .Include(x => x.PostedByUser)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Account)
            .Include(x => x.Lines)
                .ThenInclude(x => x.CostCenter)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return entity is null ? NotFound() : Ok(MapDto(entity));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] JournalEntryDto request, CancellationToken ct)
    {
        try
        {
            var period = await dbContext.FinPeriods.FirstOrDefaultAsync(x => x.Id == request.PeriodId, ct);
            if (period is null)
            {
                return BadRequest(new { message = "Period not found." });
            }

            if (period.Status != FinancePeriodStatus.Open)
            {
                return BadRequest(new { message = "Period is not open." });
            }

            var normalizedDescription = NormalizeRequired(request.Description, "Description is required.");
            var normalizedCurrencyCode = NormalizeRequired(request.CurrencyCode, "Currency code is required.").ToUpperInvariant();

            var currencyExists = await dbContext.FinCurrencies.AnyAsync(x => x.Code == normalizedCurrencyCode, ct);
            if (!currencyExists)
            {
                return BadRequest(new { message = "Currency not found." });
            }

            var normalizedLines = NormalizeLines(request.Lines, request.ExchangeRate <= 0 ? 1m : request.ExchangeRate, out var accountIds, out var costCenterIds);

            await ValidateReferencesAsync(accountIds, costCenterIds, ct);

            var entity = new FinJournalEntry
            {
                JournalNo = await GenerateJournalNoAsync(request.Date, ct),
                PeriodId = request.PeriodId,
                Date = request.Date,
                Description = normalizedDescription,
                Source = request.Source,
                SourceRefId = request.SourceRefId,
                SourceRefType = NormalizeOptional(request.SourceRefType),
                Status = FinanceJournalStatus.Draft,
                CurrencyCode = normalizedCurrencyCode,
                ExchangeRate = request.ExchangeRate <= 0 ? 1m : request.ExchangeRate,
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system",
                Lines = normalizedLines
            };

            dbContext.FinJournalEntries.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var result = await dbContext.FinJournalEntries
                .AsNoTracking()
                .Include(x => x.Period)
                .Include(x => x.PostedByUser)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.Account)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.CostCenter)
                .FirstAsync(x => x.Id == entity.Id, ct);

            return Ok(MapDto(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] JournalEntryDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.FinJournalEntries
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (entity is null)
            {
                return NotFound();
            }

            if (entity.Status != FinanceJournalStatus.Draft)
            {
                return BadRequest(new { message = "Only draft journal can be edited." });
            }

            var period = await dbContext.FinPeriods.FirstOrDefaultAsync(x => x.Id == request.PeriodId, ct);
            if (period is null)
            {
                return BadRequest(new { message = "Period not found." });
            }

            if (period.Status != FinancePeriodStatus.Open)
            {
                return BadRequest(new { message = "Period is not open." });
            }

            var normalizedDescription = NormalizeRequired(request.Description, "Description is required.");
            var normalizedCurrencyCode = NormalizeRequired(request.CurrencyCode, "Currency code is required.").ToUpperInvariant();

            var currencyExists = await dbContext.FinCurrencies.AnyAsync(x => x.Code == normalizedCurrencyCode, ct);
            if (!currencyExists)
            {
                return BadRequest(new { message = "Currency not found." });
            }

            var normalizedLines = NormalizeLines(request.Lines, request.ExchangeRate <= 0 ? 1m : request.ExchangeRate, out var accountIds, out var costCenterIds);
            await ValidateReferencesAsync(accountIds, costCenterIds, ct);

            dbContext.FinJournalEntryLines.RemoveRange(entity.Lines);

            entity.PeriodId = request.PeriodId;
            entity.Date = request.Date;
            entity.Description = normalizedDescription;
            entity.Source = request.Source;
            entity.SourceRefId = request.SourceRefId;
            entity.SourceRefType = NormalizeOptional(request.SourceRefType);
            entity.CurrencyCode = normalizedCurrencyCode;
            entity.ExchangeRate = request.ExchangeRate <= 0 ? 1m : request.ExchangeRate;
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            foreach (var line in normalizedLines)
            {
                entity.Lines.Add(line);
            }

            await dbContext.SaveChangesAsync(ct);

            var result = await dbContext.FinJournalEntries
                .AsNoTracking()
                .Include(x => x.Period)
                .Include(x => x.PostedByUser)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.Account)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.CostCenter)
                .FirstAsync(x => x.Id == id, ct);

            return Ok(MapDto(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/post")]
    public async Task<IActionResult> Post(int id, CancellationToken ct)
    {
        var entity = await dbContext.FinJournalEntries
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != FinanceJournalStatus.Draft)
        {
            return BadRequest(new { message = "Only draft journal can be posted." });
        }

        if (entity.Lines.Count == 0)
        {
            return BadRequest(new { message = "Journal lines are required." });
        }

        var totalDebitBase = entity.Lines.Sum(x => x.DebitBase);
        var totalCreditBase = entity.Lines.Sum(x => x.CreditBase);

        if (totalDebitBase != totalCreditBase)
        {
            return BadRequest(new { message = "Journal is not balanced. Total debit and credit must be equal before posting." });
        }

        entity.Status = FinanceJournalStatus.Posted;
        entity.PostedBy = GetCurrentUserId();
        entity.PostedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        var result = await dbContext.FinJournalEntries
            .AsNoTracking()
            .Include(x => x.Period)
            .Include(x => x.PostedByUser)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Account)
            .Include(x => x.Lines)
                .ThenInclude(x => x.CostCenter)
            .FirstAsync(x => x.Id == id, ct);

        return Ok(MapDto(result));
    }

    [HttpPut("{id:int}/reverse")]
    public async Task<IActionResult> Reverse(int id, CancellationToken ct)
    {
        var entity = await dbContext.FinJournalEntries
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != FinanceJournalStatus.Posted)
        {
            return BadRequest(new { message = "Only posted journal can be reversed." });
        }

        if (entity.ReversedJournalId.HasValue)
        {
            return BadRequest(new { message = "Journal has already been reversed." });
        }

        var reversal = new FinJournalEntry
        {
            JournalNo = await GenerateJournalNoAsync(entity.Date, ct),
            PeriodId = entity.PeriodId,
            Date = entity.Date,
            Description = $"Reverse {entity.JournalNo} - {entity.Description}",
            Source = entity.Source,
            SourceRefId = entity.SourceRefId,
            SourceRefType = entity.SourceRefType,
            Status = FinanceJournalStatus.Posted,
            PostedBy = GetCurrentUserId(),
            PostedAt = DateTimeOffset.UtcNow,
            CurrencyCode = entity.CurrencyCode,
            ExchangeRate = entity.ExchangeRate,
            CreatedBy = GetCurrentUserId()?.ToString() ?? "system",
            Lines = entity.Lines
                .OrderBy(x => x.LineNo)
                .Select((line, index) => new FinJournalEntryLine
                {
                    LineNo = index + 1,
                    AccountId = line.AccountId,
                    CostCenterId = line.CostCenterId,
                    Description = line.Description,
                    Debit = line.Credit,
                    Credit = line.Debit,
                    DebitBase = line.CreditBase,
                    CreditBase = line.DebitBase
                })
                .ToList()
        };

        dbContext.FinJournalEntries.Add(reversal);
        await dbContext.SaveChangesAsync(ct);

        entity.Status = FinanceJournalStatus.Reversed;
        entity.ReversedJournalId = reversal.Id;
        entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        var result = await dbContext.FinJournalEntries
            .AsNoTracking()
            .Include(x => x.Period)
            .Include(x => x.PostedByUser)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Account)
            .Include(x => x.Lines)
                .ThenInclude(x => x.CostCenter)
            .FirstAsync(x => x.Id == reversal.Id, ct);

        return Ok(MapDto(result));
    }

    private async Task<string> GenerateJournalNoAsync(DateOnly date, CancellationToken ct)
    {
        var prefix = $"JE-{date.Year}-";

        var existingNos = await dbContext.FinJournalEntries
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.JournalNo.StartsWith(prefix))
            .Select(x => x.JournalNo)
            .ToListAsync(ct);

        var maxSequence = 0;
        foreach (var journalNo in existingNos)
        {
            if (journalNo.Length <= prefix.Length)
            {
                continue;
            }

            var suffix = journalNo[prefix.Length..];
            if (int.TryParse(suffix, out var parsed) && parsed > maxSequence)
            {
                maxSequence = parsed;
            }
        }

        return $"{prefix}{maxSequence + 1:D6}";
    }

    private static ICollection<FinJournalEntryLine> NormalizeLines(
        IReadOnlyList<JournalLineDto> lines,
        decimal exchangeRate,
        out HashSet<int> accountIds,
        out HashSet<int> costCenterIds)
    {
        if (lines.Count == 0)
        {
            throw new InvalidOperationException("Journal lines are required.");
        }

        accountIds = [];
        costCenterIds = [];

        var normalizedLines = new List<FinJournalEntryLine>(lines.Count);

        for (var index = 0; index < lines.Count; index++)
        {
            var line = lines[index];

            if (line.AccountId <= 0)
            {
                throw new InvalidOperationException($"Account is required on line {index + 1}.");
            }

            if (line.Debit < 0 || line.Credit < 0)
            {
                throw new InvalidOperationException($"Debit/Credit cannot be negative on line {index + 1}.");
            }

            if (line.Debit > 0 && line.Credit > 0)
            {
                throw new InvalidOperationException($"Line {index + 1} can only have debit or credit.");
            }

            if (line.Debit == 0 && line.Credit == 0)
            {
                throw new InvalidOperationException($"Line {index + 1} must have debit or credit value.");
            }

            accountIds.Add(line.AccountId);
            if (line.CostCenterId.HasValue && line.CostCenterId.Value > 0)
            {
                costCenterIds.Add(line.CostCenterId.Value);
            }

            var debitBase = decimal.Round(line.Debit * exchangeRate, 4, MidpointRounding.AwayFromZero);
            var creditBase = decimal.Round(line.Credit * exchangeRate, 4, MidpointRounding.AwayFromZero);

            normalizedLines.Add(new FinJournalEntryLine
            {
                LineNo = index + 1,
                AccountId = line.AccountId,
                CostCenterId = line.CostCenterId is > 0 ? line.CostCenterId : null,
                Description = NormalizeOptional(line.Description),
                Debit = line.Debit,
                Credit = line.Credit,
                DebitBase = debitBase,
                CreditBase = creditBase
            });
        }

        return normalizedLines;
    }

    private async Task ValidateReferencesAsync(HashSet<int> accountIds, HashSet<int> costCenterIds, CancellationToken ct)
    {
        var existingAccountIds = await dbContext.FinAccounts
            .AsNoTracking()
            .Where(x => accountIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(ct);

        if (existingAccountIds.Count != accountIds.Count)
        {
            throw new InvalidOperationException("One or more accounts are invalid.");
        }

        if (costCenterIds.Count == 0)
        {
            return;
        }

        var existingCostCenterIds = await dbContext.FinCostCenters
            .AsNoTracking()
            .Where(x => costCenterIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(ct);

        if (existingCostCenterIds.Count != costCenterIds.Count)
        {
            throw new InvalidOperationException("One or more cost centers are invalid.");
        }
    }

    private static JournalEntryDto MapDto(FinJournalEntry entity)
    {
        var orderedLines = entity.Lines
            .OrderBy(x => x.LineNo)
            .ThenBy(x => x.Id)
            .ToList();

        return new JournalEntryDto
        {
            Id = entity.Id,
            JournalNo = entity.JournalNo,
            PeriodId = entity.PeriodId,
            PeriodName = entity.Period?.Name ?? string.Empty,
            Date = entity.Date,
            Description = entity.Description,
            Source = entity.Source,
            SourceRefId = entity.SourceRefId,
            SourceRefType = entity.SourceRefType,
            Status = entity.Status,
            PostedBy = entity.PostedBy,
            PostedByName = entity.PostedByUser?.FullName,
            PostedAt = entity.PostedAt,
            ReversedJournalId = entity.ReversedJournalId,
            CurrencyCode = entity.CurrencyCode,
            ExchangeRate = entity.ExchangeRate,
            TotalDebit = orderedLines.Sum(x => x.Debit),
            TotalCredit = orderedLines.Sum(x => x.Credit),
            TotalDebitBase = orderedLines.Sum(x => x.DebitBase),
            TotalCreditBase = orderedLines.Sum(x => x.CreditBase),
            Lines = orderedLines.Select(x => new JournalLineDto
            {
                Id = x.Id,
                LineNo = x.LineNo,
                AccountId = x.AccountId,
                AccountCode = x.Account?.Code ?? string.Empty,
                AccountName = x.Account?.Name ?? string.Empty,
                CostCenterId = x.CostCenterId,
                CostCenterCode = x.CostCenter?.Code,
                CostCenterName = x.CostCenter?.Name,
                Description = x.Description,
                Debit = x.Debit,
                Credit = x.Credit,
                DebitBase = x.DebitBase,
                CreditBase = x.CreditBase
            }).ToList()
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
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
