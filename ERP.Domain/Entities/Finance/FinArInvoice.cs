using ERP.Domain.Entities.System;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities.Finance;

public sealed class FinArInvoice : BaseEntity
{
    public string InvoiceNo { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int PeriodId { get; set; }
    public DateOnly InvoiceDate { get; set; }
    public DateOnly DueDate { get; set; }
    public string? Description { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ReceivedAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public string CurrencyCode { get; set; } = "IDR";
    public decimal ExchangeRate { get; set; } = 1m;
    public FinanceArInvoiceStatus Status { get; set; } = FinanceArInvoiceStatus.Draft;
    public int? SentBy { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public int? JournalEntryId { get; set; }

    public FinCustomer Customer { get; set; } = null!;
    public FinPeriod Period { get; set; } = null!;
    public FinCurrency Currency { get; set; } = null!;
    public SysUser? SentByUser { get; set; }
    public FinJournalEntry? JournalEntry { get; set; }
    public ICollection<FinArInvoiceLine> Lines { get; set; } = new List<FinArInvoiceLine>();
    public ICollection<FinArReceiptApplication> ReceiptApplications { get; set; } = new List<FinArReceiptApplication>();
}
