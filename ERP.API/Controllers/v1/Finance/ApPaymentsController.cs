using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Finance;
using ERP.Domain.Entities.Finance;
using ERP.Domain.Enums;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Finance;

[Route("api/v1/finance/ap/payments")]
public sealed class ApPaymentsController(AppDbContext dbContext) : FinanceControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ApPaymentPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.FinApPayments
            .AsNoTracking()
            .Include(x => x.Vendor)
            .Include(x => x.BankAccount)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.PaymentNo.ToLower().Contains(search) ||
                x.Vendor.Code.ToLower().Contains(search) ||
                x.Vendor.Name.ToLower().Contains(search) ||
                (x.ReferenceNo != null && x.ReferenceNo.ToLower().Contains(search)) ||
                (x.Notes != null && x.Notes.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.PaymentNo))
        {
            var paymentNo = request.PaymentNo.Trim().ToLowerInvariant();
            query = query.Where(x => x.PaymentNo.ToLower().Contains(paymentNo));
        }

        if (request.VendorId.HasValue)
        {
            query = query.Where(x => x.VendorId == request.VendorId.Value);
        }

        if (request.PaymentDateFrom.HasValue)
        {
            query = query.Where(x => x.PaymentDate >= request.PaymentDateFrom.Value);
        }

        if (request.PaymentDateTo.HasValue)
        {
            query = query.Where(x => x.PaymentDate <= request.PaymentDateTo.Value);
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
            "paymentno" => isDesc ? query.OrderByDescending(x => x.PaymentNo) : query.OrderBy(x => x.PaymentNo),
            "vendorname" => isDesc ? query.OrderByDescending(x => x.Vendor.Name).ThenByDescending(x => x.PaymentDate) : query.OrderBy(x => x.Vendor.Name).ThenBy(x => x.PaymentDate),
            "paymentdate" => isDesc ? query.OrderByDescending(x => x.PaymentDate).ThenByDescending(x => x.PaymentNo) : query.OrderBy(x => x.PaymentDate).ThenBy(x => x.PaymentNo),
            "amount" => isDesc ? query.OrderByDescending(x => x.Amount) : query.OrderBy(x => x.Amount),
            "paymentmethod" => isDesc ? query.OrderByDescending(x => x.PaymentMethod).ThenByDescending(x => x.PaymentDate) : query.OrderBy(x => x.PaymentMethod).ThenBy(x => x.PaymentDate),
            "createdat" => isDesc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            _ => isDesc ? query.OrderByDescending(x => x.PaymentDate).ThenByDescending(x => x.PaymentNo) : query.OrderBy(x => x.PaymentDate).ThenBy(x => x.PaymentNo)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ApPaymentDto
            {
                Id = x.Id,
                PaymentNo = x.PaymentNo,
                VendorId = x.VendorId,
                VendorCode = x.Vendor.Code,
                VendorName = x.Vendor.Name,
                PaymentDate = x.PaymentDate,
                Amount = x.Amount,
                PaymentMethod = x.PaymentMethod,
                BankAccountId = x.BankAccountId,
                BankAccountCode = x.BankAccount.Code,
                BankAccountName = x.BankAccount.Name,
                ReferenceNo = x.ReferenceNo,
                Notes = x.Notes,
                JournalEntryId = x.JournalEntryId,
                Applications = new List<ApPaymentApplicationDto>()
            })
            .ToListAsync(ct);

        return Ok(PagedResult<ApPaymentDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var entity = await dbContext.FinApPayments
            .AsNoTracking()
            .Include(x => x.Vendor)
            .Include(x => x.BankAccount)
            .Include(x => x.Applications)
                .ThenInclude(x => x.Invoice)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return entity is null ? NotFound() : Ok(MapDto(entity));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ApPaymentDto request, CancellationToken ct)
    {
        try
        {
            if (request.VendorId <= 0)
            {
                return BadRequest(new { message = "Vendor is required." });
            }

            if (request.BankAccountId <= 0)
            {
                return BadRequest(new { message = "Bank account is required." });
            }

            if (request.Applications.Count == 0)
            {
                return BadRequest(new { message = "Payment applications are required." });
            }

            var vendor = await dbContext.FinVendors.FirstOrDefaultAsync(x => x.Id == request.VendorId, ct);
            if (vendor is null)
            {
                return BadRequest(new { message = "Vendor not found." });
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
                return BadRequest(new { message = "Each payment application must have unique invoice." });
            }

            if (request.Applications.Any(x => x.AppliedAmount <= 0))
            {
                return BadRequest(new { message = "Applied amount must be greater than 0." });
            }

            var invoices = await dbContext.FinApInvoices
                .Where(x => invoiceIds.Contains(x.Id) && x.VendorId == request.VendorId)
                .ToListAsync(ct);

            if (invoices.Count != invoiceIds.Count)
            {
                return BadRequest(new { message = "One or more invoices are invalid for selected vendor." });
            }

            foreach (var invoice in invoices)
            {
                if (invoice.Status is FinanceApInvoiceStatus.Draft or FinanceApInvoiceStatus.Cancelled)
                {
                    return BadRequest(new { message = $"Invoice '{invoice.InvoiceNo}' is not payable." });
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
            var paymentAmount = request.Amount <= 0 ? totalApplied : decimal.Round(request.Amount, 4, MidpointRounding.AwayFromZero);
            if (paymentAmount != totalApplied)
            {
                return BadRequest(new { message = "Payment amount must equal total applied amount." });
            }

            if (paymentAmount <= 0)
            {
                return BadRequest(new { message = "Payment amount must be greater than 0." });
            }

            var paymentDate = request.PaymentDate;
            var period = await dbContext.FinPeriods
                .AsNoTracking()
                .Where(x => x.Status == FinancePeriodStatus.Open && x.StartDate <= paymentDate && x.EndDate >= paymentDate)
                .OrderByDescending(x => x.StartDate)
                .FirstOrDefaultAsync(ct);

            if (period is null)
            {
                return BadRequest(new { message = "No open period found for payment date." });
            }

            var apAccountId = vendor.DefaultAccountId;
            if (!apAccountId.HasValue)
            {
                apAccountId = await dbContext.FinAccounts
                    .AsNoTracking()
                    .Where(x => x.Code == "2101")
                    .Select(x => (int?)x.Id)
                    .FirstOrDefaultAsync(ct);
            }

            if (!apAccountId.HasValue)
            {
                return BadRequest(new { message = "AP account not found. Please set vendor default account or account 2101." });
            }

            var now = DateTimeOffset.UtcNow;
            var entity = new FinApPayment
            {
                PaymentNo = await GeneratePaymentNoAsync(paymentDate, ct),
                VendorId = request.VendorId,
                PaymentDate = paymentDate,
                Amount = paymentAmount,
                PaymentMethod = request.PaymentMethod,
                BankAccountId = request.BankAccountId,
                ReferenceNo = NormalizeOptional(request.ReferenceNo),
                Notes = NormalizeOptional(request.Notes),
                CreatedBy = GetCurrentUserId()?.ToString() ?? "system",
                CreatedAt = now,
                Applications = request.Applications
                    .Select(x => new FinApPaymentApplication
                    {
                        InvoiceId = x.InvoiceId,
                        AppliedAmount = decimal.Round(x.AppliedAmount, 4, MidpointRounding.AwayFromZero)
                    })
                    .ToList()
            };

            dbContext.FinApPayments.Add(entity);
            await dbContext.SaveChangesAsync(ct);

            var journal = new FinJournalEntry
            {
                JournalNo = await GenerateJournalNoAsync(paymentDate, ct),
                PeriodId = period.Id,
                Date = paymentDate,
                Description = $"AP Payment {entity.PaymentNo} - {vendor.Name}",
                Source = FinanceJournalSource.Ap,
                SourceRefId = entity.Id,
                SourceRefType = "fin_ap_payments",
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
                        AccountId = apAccountId.Value,
                        Description = $"AP settlement {entity.PaymentNo}",
                        Debit = paymentAmount,
                        Credit = 0m,
                        DebitBase = paymentAmount,
                        CreditBase = 0m
                    },
                    new FinJournalEntryLine
                    {
                        LineNo = 2,
                        AccountId = request.BankAccountId,
                        Description = $"Bank payment {entity.PaymentNo}",
                        Debit = 0m,
                        Credit = paymentAmount,
                        DebitBase = 0m,
                        CreditBase = paymentAmount
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
                invoice.PaidAmount = decimal.Round(invoice.PaidAmount + applied, 4, MidpointRounding.AwayFromZero);
                invoice.OutstandingAmount = decimal.Round(invoice.TotalAmount - invoice.PaidAmount, 4, MidpointRounding.AwayFromZero);
                invoice.Status = invoice.OutstandingAmount <= 0
                    ? FinanceApInvoiceStatus.Paid
                    : FinanceApInvoiceStatus.PartiallyPaid;
                invoice.UpdatedBy = GetCurrentUserId()?.ToString() ?? "system";
                invoice.UpdatedAt = now;
            }

            await dbContext.SaveChangesAsync(ct);

            var result = await dbContext.FinApPayments
                .AsNoTracking()
                .Include(x => x.Vendor)
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

    private async Task<string> GeneratePaymentNoAsync(DateOnly date, CancellationToken ct)
    {
        var prefix = $"PAY-{date.Year}-";

        var existingNos = await dbContext.FinApPayments
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.PaymentNo.StartsWith(prefix))
            .Select(x => x.PaymentNo)
            .ToListAsync(ct);

        var maxSequence = 0;
        foreach (var paymentNo in existingNos)
        {
            if (paymentNo.Length <= prefix.Length)
            {
                continue;
            }

            var suffix = paymentNo[prefix.Length..];
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

    private static ApPaymentDto MapDto(FinApPayment entity)
    {
        return new ApPaymentDto
        {
            Id = entity.Id,
            PaymentNo = entity.PaymentNo,
            VendorId = entity.VendorId,
            VendorCode = entity.Vendor.Code,
            VendorName = entity.Vendor.Name,
            PaymentDate = entity.PaymentDate,
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
                .Select(x => new ApPaymentApplicationDto
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

