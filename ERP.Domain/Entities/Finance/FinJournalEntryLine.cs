namespace ERP.Domain.Entities.Finance;

public sealed class FinJournalEntryLine
{
    public int Id { get; set; }
    public int JournalEntryId { get; set; }
    public int LineNo { get; set; }
    public int AccountId { get; set; }
    public int? CostCenterId { get; set; }
    public string? Description { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal DebitBase { get; set; }
    public decimal CreditBase { get; set; }

    public FinJournalEntry JournalEntry { get; set; } = null!;
    public FinAccount Account { get; set; } = null!;
    public FinCostCenter? CostCenter { get; set; }
}
