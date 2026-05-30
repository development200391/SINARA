using ERP.Domain.Entities.System;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities.Finance;

public sealed class FinJournalEntry : BaseEntity
{
    public string JournalNo { get; set; } = string.Empty;
    public int PeriodId { get; set; }
    public DateOnly Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public FinanceJournalSource Source { get; set; } = FinanceJournalSource.Manual;
    public int? SourceRefId { get; set; }
    public string? SourceRefType { get; set; }
    public FinanceJournalStatus Status { get; set; } = FinanceJournalStatus.Draft;
    public int? PostedBy { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public int? ReversedJournalId { get; set; }
    public string CurrencyCode { get; set; } = "IDR";
    public decimal ExchangeRate { get; set; } = 1m;

    public FinPeriod Period { get; set; } = null!;
    public SysUser? PostedByUser { get; set; }
    public FinJournalEntry? ReversedJournal { get; set; }
    public ICollection<FinJournalEntry> ReversalJournals { get; set; } = new List<FinJournalEntry>();
    public FinCurrency Currency { get; set; } = null!;
    public ICollection<FinJournalEntryLine> Lines { get; set; } = new List<FinJournalEntryLine>();
}
