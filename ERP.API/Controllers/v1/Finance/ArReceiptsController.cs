using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Entities.Finance;
using ERP.Domain.Enums;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Finance;

[Route("api/v1/finance/ar/receipts")]
public sealed class ArReceiptsController(AppDbContext dbContext) : FinanceControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ArReceiptPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.FinArReceipts
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.BankAccount)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.ReceiptNo.ToLower().Contains(search) ||
                x.Customer.Code.ToLower().Contains(search) ||
                x.Customer.Name.ToLower().Contains(search) ||
                (x.ReferenceNo != null && x.ReferenceNo.ToLower().Contains(search)) ||
                (x.Notes != null && x.Notes.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.ReceiptNo))
        {
            var receiptNo = request.ReceiptNo.Trim().ToLowerInvariant();
            query = query.Where(x => x.ReceiptNo.ToLower().Contains(receiptNo));
        }

        if (request.CustomerId.HasValue)
        {
            query = query.Where(x => x.CustomerId == request.CustomerId.Value);
        }

        if (request.ReceiptDateFrom.HasValue)
        {
            query = query.Where(x => x.ReceiptDate >= request.ReceiptDateFrom.Value);
        }

        if (request.ReceiptDateTo.HasValue)
        {
            query = query.Where(x => x.ReceiptDate <= request.ReceiptDateTo.Value);
        }

        if (request.PaymentMethod.HasValue)
        {
            query = query.Where(x => x.PaymentMethod == request.PaymentMethod.Value);
        }

        if (request.AmountFrom.HasValue)
        {
            query = query.Where(x => x.Amount >= request.AmountFrom.Value);
        }

        if (request.AmountTo.HasValue)
        {
            query = query.Where(x => x.Amount <= request.AmountTo.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "receiptno" => isDesc ? query.OrderByDescending(x => x.ReceiptNo) : query.OrderBy(x => x.ReceiptNo),
            "customername" => isDesc ? query.OrderByDescending(x => x.Customer.Name).ThenByDescending(x => x.ReceiptDate) : query.OrderBy(x => x.Customer.Name).ThenBy(x => x.ReceiptDate),
            "receiptdate" => isDesc ? query.OrderByDescending(x => x.ReceiptDate).ThenByDescending(x => x.ReceiptNo) : query.OrderBy(x => x.ReceiptDate).ThenBy(x => x.ReceiptNo),
            "amount" => isDesc ? query.OrderByDescending(x => x.Amount) : query.OrderBy(x => x.Amount),
            "paymentmethod" => isDesc ? query.OrderByDescending(x => x.PaymentMethod).ThenByDescending(x => x.ReceiptDate) : query.OrderBy(x => x.PaymentMethod).ThenBy(x => x.ReceiptDate),
            "createdat" => isDesc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            _ => isDesc ? query.OrderByDescending(x => x.ReceiptDate).ThenByDescending(x => x.ReceiptNo) : query.OrderBy(x => x.ReceiptDate).ThenBy(x => x.ReceiptNo)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ArReceiptDto
            {
                Id = x.Id,
                ReceiptNo = x.ReceiptNo,
                CustomerId = x.CustomerId,
                CustomerCode = x.Customer.Code,
                CustomerName = x.Customer.Name,
                ReceiptDate = x.ReceiptDate,
                Amount = x.Amount,
                PaymentMethod = x.PaymentMethod,
                BankAccountId = x.BankAccountId,
                BankAccountCode = x.BankAccount.Code,
                BankAccountName = x.BankAccount.Name,
                ReferenceNo = x.ReferenceNo,
                Notes = x.Notes,
                JournalEntryId = x.JournalEntryId,
                Applications = new List<ArReceiptApplicationDto>()
            })
            .ToListAsync(ct);

        return Ok(PagedResult<ArReceiptDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var entity = await dbContext.FinArReceipts
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.BankAccount)
            .Include(x => x.Applications)
                .ThenInclude(x => x.Invoice)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return entity is null ? NotFound() : Ok(MapDto(entity));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ArReceiptDto request, CancellationToken ct)
    {
        try
        {
            if (request.CustomerId <= 0)
            {
                return BadRequest(new { message = "Customer is required." });
            }

            if (request.BankAccountId <= 0)
            {
                return BadRequest(new { message = "Bank account is required." });
            }

            if (request.Applications.Count == 0)
            {
                return BadRequest(new { message = "Receipt applications are required." });
            }

            var customer = await dbContext.FinCustomers.FirstOrDefaultAsync(x => x.Id == request.CustomerId, ct);
            if (customer is null)
            {
                return BadRequest(new { message = "Customer not found." });
            }

            var bankAccount = await dbContext.FinAccounts.FirstOrDefaultAsync(x => x.Id == request.BankAccountId, ct);
            if (bankAccount is null)
            {
                return BadRequest(new { message = "Bank account not found." });
            }

            var invoiceIds = request.Applications
                .Select(x => x.InvoiceId)
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            if (invoiceIds.Count != request.Applications.Count)
            {
                return BadRequest(new { message = "Each receipt application must have unique invoice." });
            }

            if (request.Applications.Any(x => x.AppliedAmount <= 0))
            {
                return BadRequest(new { message = "Applied amount must be greater than 0." });
            }

            var invoices = await dbContext.FinArInvoices
                .Where(x => invoiceIds.Contains(x.Id) && x.CustomerId == request.CustomerId)
                .ToListAsync(ct);

            if (invoices.Count != invoiceIds.Count)
            {
                return BadRequest(new { message = "One or more invoices are invalid for selected customer." });
            }

            foreach (var invoice in invoices)
            {
                if (invoice.Status is FinanceArInvoiceStatus.Draft or FinanceArInvoiceStatus.Cancelled)
                {
                    return BadRequest(new { message = $"Invoice '{invoice.InvoiceNo}' is not receivable." });
                }

                if (invoice.OutstandingAmount <= 0)
                {
                    return BadRequest(new { message = $"Invoice '{invoice.InvoiceNo}' has no outstanding amount." });
                }
            }

            var appliedByInvoice = request.Applications.ToDictionary(x => x.InvoiceId, x => decimal.Round(x.AppliedAmount, 4, MidpointRounding.AwayFromZero));
            foreach (var invoice in invoices)
            {
                var applied = appliedByInvoice[invoice.Id];
                if (applied > invoice.OutstandingAmount)
                {
                    return BadRequest(new { message = $"Applied amount for invoice '{invoice.InvoiceNo}' exceeds outstanding amount." });
                }
            }

            var totalApplied = appliedByInvoice.Values.Sum();
            var receiptAmount = request.Amount <= 0 ? totalApplied : decimal.Round(request.Amount, 4, MidpointRounding.AwayFromZero);
            if (receiptAmount != totalApplied)
            {
                return BadRequest(new { message = "Receipt amount must equal total applied amount." });
            }

            if (receiptAmount <= 0)
            {
                return BadRequest(new { message = "Receipt amount must be greater than 0." });
            }

            var receiptDate = request.ReceiptDate;
            var period = await dbContext.FinPeriods
                .AsNoTracking()
                .Where(x => x.Status == FinancePeriodStatus.Open && x.StartDate <= receiptDate && x.EndDate >= receiptDate)
                .OrderByDescending(x => x.StartDate)
                .FirstOrDefaultAsync(ct);

            if (period is null)
            {
                return BadRequest(new { message = "No open period found for receipt date." });
            }

            var arAccountId = customer.DefaultAccountId;
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

            var now = DateTimeOffset.UtcNow;
            var entity = new FinArReceipt
            {
                ReceiptNo = await GenerateReceiptNoAsync(receiptDate, ct),
                CustomerId = request.CustomerId,
                ReceiptDate = receiptDate,
                Amount = receiptAmount,
                PaymentMethod = request.PaymentMethod,
                BankAccountId = request.BankAccountId,
                ReferenceNo = NormalizeOptional(request.ReferenceNo),
                Notes = NormalizeOptional(request.Notes),
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system",
                CreatedAt = now,
                Applications = request.Applications
                    .Select(x => new FinArReceiptApplication
                    {
                        InvoiceId = x.InvoiceId,
                        AppliedAmount = decimal.Round(x.AppliedAmount, 4, MidpointRounding.AwayFromZero)
                    })
                    .ToList()
            };

            dbContext.FinArReceipts.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var journal = new FinJournalEntry
            {
                JournalNo = await GenerateJournalNoAsync(receiptDate, ct),
                PeriodId = period.Id,
                Date = receiptDate,
                Description = $"AR Receipt {entity.ReceiptNo} - {customer.Name}",
                Source = FinanceJournalSource.Ar,
                SourceRefId = entity.Id,
                SourceRefType = "fin_ar_receipts",
                Status = FinanceJournalStatus.Posted,
                PostedBy = GetCurrentUserId(),
                PostedAt = now,
                CurrencyCode = bankAccount.CurrencyCode,
                ExchangeRate = 1m,
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system",
                CreatedAt = now,
                Lines =
                [
                    new FinJournalEntryLine
                    {
                        LineNo = 1,
                        AccountId = request.BankAccountId,
                        Description = $"Bank receipt {entity.ReceiptNo}",
                        Debit = receiptAmount,
                        Credit = 0m,
                        DebitBase = receiptAmount,
                        CreditBase = 0m
                    },
                    new FinJournalEntryLine
                    {
                        LineNo = 2,
                        AccountId = arAccountId.Value,
                        Description = $"AR settlement {entity.ReceiptNo}",
                        Debit = 0m,
                        Credit = receiptAmount,
                        DebitBase = 0m,
                        CreditBase = receiptAmount
                    }
                ]
            };

            dbContext.FinJournalEntries.Add(journal);
            await dbContext.SaveChangesAsync(ct);

            entity.JournalEntryId = journal.Id;
            entity.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
            entity.UpdatedAt = now;

            foreach (var invoice in invoices)
            {
                var applied = appliedByInvoice[invoice.Id];
                invoice.ReceivedAmount = decimal.Round(invoice.ReceivedAmount + applied, 4, MidpointRounding.AwayFromZero);
                invoice.OutstandingAmount = decimal.Round(invoice.TotalAmount - invoice.ReceivedAmount, 4, MidpointRounding.AwayFromZero);
                invoice.Status = invoice.OutstandingAmount <= 0
                    ? FinanceArInvoiceStatus.Paid
                    : FinanceArInvoiceStatus.PartiallyPaid;
                invoice.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
                invoice.UpdatedAt = now;
            }

            await dbContext.SaveChangesAsync(ct);

            var result = await dbContext.FinArReceipts
                .AsNoTracking()
                .Include(x => x.Customer)
                .Include(x => x.BankAccount)
                .Include(x => x.Applications)
                    .ThenInclude(x => x.Invoice)
                .FirstAsync(x => x.Id == entity.Id, ct);

            return Ok(MapDto(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private async Task<string> GenerateReceiptNoAsync(DateOnly date, CancellationToken ct)
    {
        var prefix = $"RCV-{date.Year}-";

        var existingNos = await dbContext.FinArReceipts
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.ReceiptNo.StartsWith(prefix))
            .Select(x => x.ReceiptNo)
            .ToListAsync(ct);

        var maxSequence = 0;
        foreach (var receiptNo in existingNos)
        {
            if (receiptNo.Length <= prefix.Length)
            {
                continue;
            }

            var suffix = receiptNo[prefix.Length..];
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

    private static ArReceiptDto MapDto(FinArReceipt entity)
    {
        return new ArReceiptDto
        {
            Id = entity.Id,
            ReceiptNo = entity.ReceiptNo,
            CustomerId = entity.CustomerId,
            CustomerCode = entity.Customer.Code,
            CustomerName = entity.Customer.Name,
            ReceiptDate = entity.ReceiptDate,
            Amount = entity.Amount,
            PaymentMethod = entity.PaymentMethod,
            BankAccountId = entity.BankAccountId,
            BankAccountCode = entity.BankAccount.Code,
            BankAccountName = entity.BankAccount.Name,
            ReferenceNo = entity.ReferenceNo,
            Notes = entity.Notes,
            JournalEntryId = entity.JournalEntryId,
            Applications = entity.Applications
                .OrderBy(x => x.Id)
                .Select(x => new ArReceiptApplicationDto
                {
                    Id = x.Id,
                    InvoiceId = x.InvoiceId,
                    InvoiceNo = x.Invoice.InvoiceNo,
                    InvoiceDate = x.Invoice.InvoiceDate,
                    DueDate = x.Invoice.DueDate,
                    InvoiceTotalAmount = x.Invoice.TotalAmount,
                    InvoiceOutstandingAmount = x.Invoice.OutstandingAmount,
                    AppliedAmount = x.AppliedAmount
                })
                .ToList()
        };
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
