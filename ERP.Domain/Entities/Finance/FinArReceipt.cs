using ERP.Domain.Enums;

namespace ERP.Domain.Entities.Finance;

public sealed class FinArReceipt : BaseEntity
{
    public string ReceiptNo { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public DateOnly ReceiptDate { get; set; }
    public decimal Amount { get; set; }
    public FinanceArReceiptMethod PaymentMethod { get; set; } = FinanceArReceiptMethod.Transfer;
    public int BankAccountId { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Notes { get; set; }
    public int? JournalEntryId { get; set; }

    public FinCustomer Customer { get; set; } = null!;
    public FinAccount BankAccount { get; set; } = null!;
    public FinJournalEntry? JournalEntry { get; set; }
    public ICollection<FinArReceiptApplication> Applications { get; set; } = new List<FinArReceiptApplication>();
}
