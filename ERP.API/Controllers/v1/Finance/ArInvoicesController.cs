using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Entities.Finance;
using ERP.Domain.Enums;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Finance;

[Route("api/v1/finance/ar/invoices")]
public sealed class ArInvoicesController(AppDbContext dbContext) : FinanceControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ArInvoicePagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        var asOfDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var query = dbContext.FinArInvoices
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Period)
            .Include(x => x.SentByUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.InvoiceNo.ToLower().Contains(search) ||
                x.Customer.Code.ToLower().Contains(search) ||
                x.Customer.Name.ToLower().Contains(search) ||
                (x.Description != null && x.Description.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.InvoiceNo))
        {
            var invoiceNo = request.InvoiceNo.Trim().ToLowerInvariant();
            query = query.Where(x => x.InvoiceNo.ToLower().Contains(invoiceNo));
        }

        if (request.CustomerId.HasValue)
        {
            query = query.Where(x => x.CustomerId == request.CustomerId.Value);
        }

        if (request.PeriodId.HasValue)
        {
            query = query.Where(x => x.PeriodId == request.PeriodId.Value);
        }

        if (request.InvoiceDateFrom.HasValue)
        {
            query = query.Where(x => x.InvoiceDate >= request.InvoiceDateFrom.Value);
        }

        if (request.InvoiceDateTo.HasValue)
        {
            query = query.Where(x => x.InvoiceDate <= request.InvoiceDateTo.Value);
        }

        if (request.DueDateFrom.HasValue)
        {
            query = query.Where(x => x.DueDate >= request.DueDateFrom.Value);
        }

        if (request.DueDateTo.HasValue)
        {
            query = query.Where(x => x.DueDate <= request.DueDateTo.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (request.OutstandingFrom.HasValue)
        {
            query = query.Where(x => x.OutstandingAmount >= request.OutstandingFrom.Value);
        }

        if (request.OutstandingTo.HasValue)
        {
            query = query.Where(x => x.OutstandingAmount <= request.OutstandingTo.Value);
        }

        if (request.IsOverdue.HasValue)
        {
            if (request.IsOverdue.Value)
            {
                query = query.Where(x => x.OutstandingAmount > 0 && x.DueDate < asOfDate && x.Status != FinanceArInvoiceStatus.Cancelled);
            }
            else
            {
                query = query.Where(x => !(x.OutstandingAmount > 0 && x.DueDate < asOfDate && x.Status != FinanceArInvoiceStatus.Cancelled));
            }
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "invoiceno" => isDesc ? query.OrderByDescending(x => x.InvoiceNo) : query.OrderBy(x => x.InvoiceNo),
            "customername" => isDesc ? query.OrderByDescending(x => x.Customer.Name).ThenByDescending(x => x.InvoiceDate) : query.OrderBy(x => x.Customer.Name).ThenBy(x => x.InvoiceDate),
            "invoicedate" => isDesc ? query.OrderByDescending(x => x.InvoiceDate).ThenByDescending(x => x.InvoiceNo) : query.OrderBy(x => x.InvoiceDate).ThenBy(x => x.InvoiceNo),
            "duedate" => isDesc ? query.OrderByDescending(x => x.DueDate).ThenByDescending(x => x.InvoiceNo) : query.OrderBy(x => x.DueDate).ThenBy(x => x.InvoiceNo),
            "totalamount" => isDesc ? query.OrderByDescending(x => x.TotalAmount) : query.OrderBy(x => x.TotalAmount),
            "outstandingamount" => isDesc ? query.OrderByDescending(x => x.OutstandingAmount) : query.OrderBy(x => x.OutstandingAmount),
            "status" => isDesc ? query.OrderByDescending(x => x.Status).ThenByDescending(x => x.InvoiceDate) : query.OrderBy(x => x.Status).ThenBy(x => x.InvoiceDate),
            "sentat" => isDesc ? query.OrderByDescending(x => x.SentAt).ThenByDescending(x => x.InvoiceDate) : query.OrderBy(x => x.SentAt).ThenBy(x => x.InvoiceDate),
            "createdat" => isDesc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            _ => isDesc ? query.OrderByDescending(x => x.InvoiceDate).ThenByDescending(x => x.InvoiceNo) : query.OrderBy(x => x.InvoiceDate).ThenBy(x => x.InvoiceNo)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ArInvoiceDto
            {
                Id = x.Id,
                InvoiceNo = x.InvoiceNo,
                CustomerId = x.CustomerId,
                CustomerCode = x.Customer.Code,
                CustomerName = x.Customer.Name,
                PeriodId = x.PeriodId,
                PeriodName = x.Period.Name,
                InvoiceDate = x.InvoiceDate,
                DueDate = x.DueDate,
                Description = x.Description,
                Subtotal = x.Subtotal,
                TaxAmount = x.TaxAmount,
                TotalAmount = x.TotalAmount,
                ReceivedAmount = x.ReceivedAmount,
                OutstandingAmount = x.OutstandingAmount,
                CurrencyCode = x.CurrencyCode,
                ExchangeRate = x.ExchangeRate,
                Status = x.Status,
                SentBy = x.SentBy,
                SentByName = x.SentByUser != null ? x.SentByUser.FullName : null,
                SentAt = x.SentAt,
                JournalEntryId = x.JournalEntryId,
                IsOverdue = x.OutstandingAmount > 0 && x.DueDate < asOfDate && x.Status != FinanceArInvoiceStatus.Cancelled,
                Lines = new List<ArInvoiceLineDto>()
            })
            .ToListAsync(ct);

        return Ok(PagedResult<ArInvoiceDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var asOfDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var entity = await dbContext.FinArInvoices
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Period)
            .Include(x => x.SentByUser)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Account)
            .Include(x => x.Lines)
                .ThenInclude(x => x.CostCenter)
            .Include(x => x.Lines)
                .ThenInclude(x => x.TaxCode)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return entity is null ? NotFound() : Ok(MapDto(entity, asOfDate));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ArInvoiceDto request, CancellationToken ct)
    {
        try
        {
            if (request.CustomerId <= 0)
            {
                return BadRequest(new { message = "Customer is required." });
            }

            if (request.PeriodId <= 0)
            {
                return BadRequest(new { message = "Period is required." });
            }

            if (request.DueDate < request.InvoiceDate)
            {
                return BadRequest(new { message = "Due date must be greater than or equal to invoice date." });
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

            var customer = await dbContext.FinCustomers.FirstOrDefaultAsync(x => x.Id == request.CustomerId, ct);
            if (customer is null)
            {
                return BadRequest(new { message = "Customer not found." });
            }

            var normalizedCurrencyCode = NormalizeRequired(request.CurrencyCode, "Currency is required.").ToUpperInvariant();
            var currencyExists = await dbContext.FinCurrencies.AnyAsync(x => x.Code == normalizedCurrencyCode, ct);
            if (!currencyExists)
            {
                return BadRequest(new { message = "Currency not found." });
            }

            var normalizedLines = await NormalizeInvoiceLinesAsync(request.Lines, ct);
            var subtotal = normalizedLines.Sum(x => x.Amount);
            var taxAmount = normalizedLines.Sum(x => x.TaxAmount);
            var totalAmount = subtotal + taxAmount;

            var entity = new FinArInvoice
            {
                InvoiceNo = await GenerateInvoiceNoAsync(request.InvoiceDate, ct),
                CustomerId = request.CustomerId,
                PeriodId = request.PeriodId,
                InvoiceDate = request.InvoiceDate,
                DueDate = request.DueDate,
                Description = NormalizeOptional(request.Description),
                Subtotal = subtotal,
                TaxAmount = taxAmount,
                TotalAmount = totalAmount,
                ReceivedAmount = 0m,
                OutstandingAmount = totalAmount,
                CurrencyCode = normalizedCurrencyCode,
                ExchangeRate = request.ExchangeRate <= 0 ? 1m : request.ExchangeRate,
                Status = FinanceArInvoiceStatus.Draft,
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system",
                Lines = normalizedLines
            };

            dbContext.FinArInvoices.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var result = await dbContext.FinArInvoices
                .AsNoTracking()
                .Include(x => x.Customer)
                .Include(x => x.Period)
                .Include(x => x.SentByUser)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.Account)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.CostCenter)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.TaxCode)
                .FirstAsync(x => x.Id == entity.Id, ct);

            return Ok(MapDto(result, DateOnly.FromDateTime(DateTime.UtcNow.Date)));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ArInvoiceDto request, CancellationToken ct)
    {
        try
        {
            var entity = await dbContext.FinArInvoices
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (entity is null)
            {
                return NotFound();
            }

            if (entity.Status != FinanceArInvoiceStatus.Draft)
            {
                return BadRequest(new { message = "Only draft invoice can be edited." });
            }

            if (request.CustomerId <= 0)
            {
                return BadRequest(new { message = "Customer is required." });
            }

            if (request.PeriodId <= 0)
            {
                return BadRequest(new { message = "Period is required." });
            }

            if (request.DueDate < request.InvoiceDate)
            {
                return BadRequest(new { message = "Due date must be greater than or equal to invoice date." });
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

            var customer = await dbContext.FinCustomers.FirstOrDefaultAsync(x => x.Id == request.CustomerId, ct);
            if (customer is null)
            {
                return BadRequest(new { message = "Customer not found." });
            }

            var normalizedCurrencyCode = NormalizeRequired(request.CurrencyCode, "Currency is required.").ToUpperInvariant();
            var currencyExists = await dbContext.FinCurrencies.AnyAsync(x => x.Code == normalizedCurrencyCode, ct);
            if (!currencyExists)
            {
                return BadRequest(new { message = "Currency not found." });
            }

            var normalizedLines = await NormalizeInvoiceLinesAsync(request.Lines, ct);
            var subtotal = normalizedLines.Sum(x => x.Amount);
            var taxAmount = normalizedLines.Sum(x => x.TaxAmount);
            var totalAmount = subtotal + taxAmount;

            dbContext.FinArInvoiceLines.RemoveRange(entity.Lines);

            entity.CustomerId = request.CustomerId;
            entity.PeriodId = request.PeriodId;
            entity.InvoiceDate = request.InvoiceDate;
            entity.DueDate = request.DueDate;
            entity.Description = NormalizeOptional(request.Description);
            entity.Subtotal = subtotal;
            entity.TaxAmount = taxAmount;
            entity.TotalAmount = totalAmount;
            entity.ReceivedAmount = 0m;
            entity.OutstandingAmount = totalAmount;
            entity.CurrencyCode = normalizedCurrencyCode;
            entity.ExchangeRate = request.ExchangeRate <= 0 ? 1m : request.ExchangeRate;
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            foreach (var line in normalizedLines)
            {
                entity.Lines.Add(line);
            }

            await dbContext.SaveChangesAsync(ct);

            var result = await dbContext.FinArInvoices
                .AsNoTracking()
                .Include(x => x.Customer)
                .Include(x => x.Period)
                .Include(x => x.SentByUser)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.Account)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.CostCenter)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.TaxCode)
                .FirstAsync(x => x.Id == id, ct);

            return Ok(MapDto(result, DateOnly.FromDateTime(DateTime.UtcNow.Date)));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/send")]
    public async Task<IActionResult> Send(int id, CancellationToken ct)
    {
        var invoice = await dbContext.FinArInvoices
            .Include(x => x.Customer)
            .Include(x => x.Lines)
                .ThenInclude(x => x.TaxCode)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (invoice is null)
        {
            return NotFound();
        }

        if (invoice.Status != FinanceArInvoiceStatus.Draft)
        {
            return BadRequest(new { message = "Only draft invoice can be sent." });
        }

        if (invoice.Lines.Count == 0)
        {
            return BadRequest(new { message = "Invoice lines are required before send." });
        }

        var period = await dbContext.FinPeriods.FirstOrDefaultAsync(x => x.Id == invoice.PeriodId, ct);
        if (period is null)
        {
            return BadRequest(new { message = "Period not found." });
        }

        if (period.Status != FinancePeriodStatus.Open)
        {
            return BadRequest(new { message = "Period is not open." });
        }

        var arAccountId = invoice.Customer.DefaultAccountId;
        if (!arAccountId.HasValue)
        {
            arAccountId = await dbContext.FinAccounts
                .AsNoTracking()
                .Where(x => x.Code == "1110")
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync(ct);
        }

        if (!arAccountId.HasValue)
        {
            return BadRequest(new { message = "AR account not found. Please set customer default account or account 1110." });
        }

        var exchangeRate = invoice.ExchangeRate <= 0 ? 1m : invoice.ExchangeRate;
        var journalLines = new List<FinJournalEntryLine>();
        var lineNo = 1;

        foreach (var line in invoice.Lines.OrderBy(x => x.LineNo))
        {
            if (line.Amount > 0)
            {
                journalLines.Add(new FinJournalEntryLine
                {
                    LineNo = lineNo++,
                    AccountId = line.AccountId,
                    CostCenterId = line.CostCenterId,
                    Description = string.IsNullOrWhiteSpace(line.Description) ? null : line.Description,
                    Debit = 0m,
                    Credit = line.Amount,
                    DebitBase = 0m,
                    CreditBase = decimal.Round(line.Amount * exchangeRate, 4, MidpointRounding.AwayFromZero)
                });
            }

            if (line.TaxAmount > 0)
            {
                if (line.TaxCodeId is null || line.TaxCode is null)
                {
                    return BadRequest(new { message = "Tax amount exists but tax code is missing in one of invoice lines." });
                }

                journalLines.Add(new FinJournalEntryLine
                {
                    LineNo = lineNo++,
                    AccountId = line.TaxCode.AccountId,
                    CostCenterId = line.CostCenterId,
                    Description = $"Tax {line.TaxCode.Code} - {line.Description}",
                    Debit = 0m,
                    Credit = line.TaxAmount,
                    DebitBase = 0m,
                    CreditBase = decimal.Round(line.TaxAmount * exchangeRate, 4, MidpointRounding.AwayFromZero)
                });
            }
        }

        journalLines.Add(new FinJournalEntryLine
        {
            LineNo = lineNo,
            AccountId = arAccountId.Value,
            CostCenterId = null,
            Description = $"AR Invoice {invoice.InvoiceNo}",
            Debit = invoice.TotalAmount,
            Credit = 0m,
            DebitBase = decimal.Round(invoice.TotalAmount * exchangeRate, 4, MidpointRounding.AwayFromZero),
            CreditBase = 0m
        });

        var totalDebitBase = journalLines.Sum(x => x.DebitBase);
        var totalCreditBase = journalLines.Sum(x => x.CreditBase);
        if (totalDebitBase != totalCreditBase)
        {
            return BadRequest(new { message = "Generated journal is not balanced." });
        }

        var now = DateTimeOffset.UtcNow;
        var journal = new FinJournalEntry
        {
            JournalNo = await GenerateJournalNoAsync(invoice.InvoiceDate, ct),
            PeriodId = invoice.PeriodId,
            Date = invoice.InvoiceDate,
            Description = $"AR Invoice {invoice.InvoiceNo} - {invoice.Customer.Name}",
            Source = FinanceJournalSource.Ar,
            SourceRefId = invoice.Id,
            SourceRefType = "fin_ar_invoices",
            Status = FinanceJournalStatus.Posted,
            PostedBy = GetCurrentUserId(),
            PostedAt = now,
            CurrencyCode = invoice.CurrencyCode,
            ExchangeRate = exchangeRate,
            CreatedBy = GetCurrentUserId()?.ToString() ?? "system",
            CreatedAt = now,
            Lines = journalLines
        };

        dbContext.FinJournalEntries.Add(journal);
        await dbContext.SaveChangesAsync(ct);

        invoice.Status = FinanceArInvoiceStatus.Sent;
        invoice.SentBy = GetCurrentUserId();
        invoice.SentAt = now;
        invoice.JournalEntryId = journal.Id;
        invoice.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
        invoice.UpdatedAt = now;

        await dbContext.SaveChangesAsync(ct);

        var result = await dbContext.FinArInvoices
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Period)
            .Include(x => x.SentByUser)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Account)
            .Include(x => x.Lines)
                .ThenInclude(x => x.CostCenter)
            .Include(x => x.Lines)
                .ThenInclude(x => x.TaxCode)
            .FirstAsync(x => x.Id == id, ct);

        return Ok(MapDto(result, DateOnly.FromDateTime(DateTime.UtcNow.Date)));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var entity = await dbContext.FinArInvoices
            .Include(x => x.ReceiptApplications)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.Status != FinanceArInvoiceStatus.Draft)
        {
            return BadRequest(new { message = "Only draft invoice can be deleted." });
        }

        if (entity.ReceiptApplications.Count > 0)
        {
            return BadRequest(new { message = "Invoice already has receipt applications and cannot be deleted." });
        }

        dbContext.FinArInvoices.Remove(entity);
        await dbContext.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task<List<FinArInvoiceLine>> NormalizeInvoiceLinesAsync(IReadOnlyList<ArInvoiceLineDto> lines, CancellationToken ct)
    {
        if (lines.Count == 0)
        {
            throw new InvalidOperationException("Invoice lines are required.");
        }

        var accountIds = new HashSet<int>();
        var costCenterIds = new HashSet<int>();
        var taxCodeIds = new HashSet<int>();

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];

            if (line.AccountId <= 0)
            {
                throw new InvalidOperationException($"Account is required on line {i + 1}.");
            }

            if (string.IsNullOrWhiteSpace(line.Description))
            {
                throw new InvalidOperationException($"Description is required on line {i + 1}.");
            }

            if (line.Quantity <= 0)
            {
                throw new InvalidOperationException($"Quantity must be greater than 0 on line {i + 1}.");
            }

            if (line.UnitPrice < 0)
            {
                throw new InvalidOperationException($"Unit price cannot be negative on line {i + 1}.");
            }

            if (line.TaxAmount < 0)
            {
                throw new InvalidOperationException($"Tax amount cannot be negative on line {i + 1}.");
            }

            accountIds.Add(line.AccountId);
            if (line.CostCenterId is > 0)
            {
                costCenterIds.Add(line.CostCenterId.Value);
            }

            if (line.TaxCodeId is > 0)
            {
                taxCodeIds.Add(line.TaxCodeId.Value);
            }
        }

        var existingAccounts = await dbContext.FinAccounts
            .AsNoTracking()
            .Where(x => accountIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(ct);

        if (existingAccounts.Count != accountIds.Count)
        {
            throw new InvalidOperationException("One or more accounts are invalid.");
        }

        if (costCenterIds.Count > 0)
        {
            var existingCostCenters = await dbContext.FinCostCenters
                .AsNoTracking()
                .Where(x => costCenterIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync(ct);

            if (existingCostCenters.Count != costCenterIds.Count)
            {
                throw new InvalidOperationException("One or more cost centers are invalid.");
            }
        }

        var taxRates = new Dictionary<int, decimal>();
        if (taxCodeIds.Count > 0)
        {
            var taxCodeData = await dbContext.FinTaxCodes
                .AsNoTracking()
                .Where(x => taxCodeIds.Contains(x.Id))
                .Select(x => new { x.Id, x.Rate })
                .ToListAsync(ct);

            if (taxCodeData.Count != taxCodeIds.Count)
            {
                throw new InvalidOperationException("One or more tax codes are invalid.");
            }

            foreach (var item in taxCodeData)
            {
                taxRates[item.Id] = item.Rate;
            }
        }

        var normalized = new List<FinArInvoiceLine>(lines.Count);
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var amount = decimal.Round(line.Quantity * line.UnitPrice, 4, MidpointRounding.AwayFromZero);

            var taxCodeId = line.TaxCodeId is > 0 ? line.TaxCodeId : null;
            decimal taxAmount;
            if (!taxCodeId.HasValue)
            {
                taxAmount = 0m;
            }
            else
            {
                taxAmount = line.TaxAmount > 0
                    ? decimal.Round(line.TaxAmount, 4, MidpointRounding.AwayFromZero)
                    : decimal.Round(amount * taxRates[taxCodeId.Value] / 100m, 4, MidpointRounding.AwayFromZero);
            }

            normalized.Add(new FinArInvoiceLine
            {
                LineNo = i + 1,
                Description = line.Description.Trim(),
                Quantity = decimal.Round(line.Quantity, 4, MidpointRounding.AwayFromZero),
                UnitPrice = decimal.Round(line.UnitPrice, 4, MidpointRounding.AwayFromZero),
                Amount = amount,
                TaxCodeId = taxCodeId,
                TaxAmount = taxAmount,
                AccountId = line.AccountId,
                CostCenterId = line.CostCenterId is > 0 ? line.CostCenterId : null
            });
        }

        return normalized;
    }

    private async Task<string> GenerateInvoiceNoAsync(DateOnly date, CancellationToken ct)
    {
        var prefix = $"AR-{date.Year}-";

        var existingNos = await dbContext.FinArInvoices
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.InvoiceNo.StartsWith(prefix))
            .Select(x => x.InvoiceNo)
            .ToListAsync(ct);

        var maxSequence = 0;
        foreach (var invoiceNo in existingNos)
        {
            if (invoiceNo.Length <= prefix.Length)
            {
                continue;
            }

            var suffix = invoiceNo[prefix.Length..];
            if (int.TryParse(suffix, out var parsed) && parsed > maxSequence)
            {
                maxSequence = parsed;
            }
        }

        return $"{prefix}{maxSequence + 1:D6}";
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

    private static ArInvoiceDto MapDto(FinArInvoice entity, DateOnly asOfDate)
    {
        return new ArInvoiceDto
        {
            Id = entity.Id,
            InvoiceNo = entity.InvoiceNo,
            CustomerId = entity.CustomerId,
            CustomerCode = entity.Customer.Code,
            CustomerName = entity.Customer.Name,
            PeriodId = entity.PeriodId,
            PeriodName = entity.Period.Name,
            InvoiceDate = entity.InvoiceDate,
            DueDate = entity.DueDate,
            Description = entity.Description,
            Subtotal = entity.Subtotal,
            TaxAmount = entity.TaxAmount,
            TotalAmount = entity.TotalAmount,
            ReceivedAmount = entity.ReceivedAmount,
            OutstandingAmount = entity.OutstandingAmount,
            CurrencyCode = entity.CurrencyCode,
            ExchangeRate = entity.ExchangeRate,
            Status = entity.Status,
            SentBy = entity.SentBy,
            SentByName = entity.SentByUser != null ? entity.SentByUser.FullName : null,
            SentAt = entity.SentAt,
            JournalEntryId = entity.JournalEntryId,
            IsOverdue = entity.OutstandingAmount > 0 && entity.DueDate < asOfDate && entity.Status != FinanceArInvoiceStatus.Cancelled,
            Lines = entity.Lines
                .OrderBy(x => x.LineNo)
                .Select(x => new ArInvoiceLineDto
                {
                    Id = x.Id,
                    LineNo = x.LineNo,
                    Description = x.Description,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    Amount = x.Amount,
                    TaxCodeId = x.TaxCodeId,
                    TaxCodeCode = x.TaxCode != null ? x.TaxCode.Code : null,
                    TaxCodeName = x.TaxCode != null ? x.TaxCode.Name : null,
                    TaxAmount = x.TaxAmount,
                    AccountId = x.AccountId,
                    AccountCode = x.Account != null ? x.Account.Code : string.Empty,
                    AccountName = x.Account != null ? x.Account.Name : string.Empty,
                    CostCenterId = x.CostCenterId,
                    CostCenterCode = x.CostCenter != null ? x.CostCenter.Code : null,
                    CostCenterName = x.CostCenter != null ? x.CostCenter.Name : null
                })
                .ToList()
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
