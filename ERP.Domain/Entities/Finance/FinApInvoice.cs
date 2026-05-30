using ERP.Domain.Entities.System;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities.Finance;

public sealed class FinApInvoice : BaseEntity
{
    public string InvoiceNo { get; set; } = string.Empty;
    public string? VendorInvoiceNo { get; set; }
    public int VendorId { get; set; }
    public int PeriodId { get; set; }
    public DateOnly InvoiceDate { get; set; }
    public DateOnly DueDate { get; set; }
    public string? Description { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public string CurrencyCode { get; set; } = "IDR";
    public decimal ExchangeRate { get; set; } = 1m;
    public FinanceApInvoiceStatus Status { get; set; } = FinanceApInvoiceStatus.Draft;
    public int? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public int? JournalEntryId { get; set; }

    public FinVendor Vendor { get; set; } = null!;
    public FinPeriod Period { get; set; } = null!;
    public FinCurrency Currency { get; set; } = null!;
    public SysUser? ApprovedByUser { get; set; }
    public FinJournalEntry? JournalEntry { get; set; }
    public ICollection<FinApInvoiceLine> Lines { get; set; } = new List<FinApInvoiceLine>();
    public ICollection<FinApPaymentApplication> PaymentApplications { get; set; } = new List<FinApPaymentApplication>();
}
