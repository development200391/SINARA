using ERP.Domain.Enums;

namespace ERP.Domain.Entities.Finance;

public sealed class FinApPayment : BaseEntity
{
    public string PaymentNo { get; set; } = string.Empty;
    public int VendorId { get; set; }
    public DateOnly PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public FinanceApPaymentMethod PaymentMethod { get; set; } = FinanceApPaymentMethod.Transfer;
    public int BankAccountId { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Notes { get; set; }
    public int? JournalEntryId { get; set; }

    public FinVendor Vendor { get; set; } = null!;
    public FinAccount BankAccount { get; set; } = null!;
    public FinJournalEntry? JournalEntry { get; set; }
    public ICollection<FinApPaymentApplication> Applications { get; set; } = new List<FinApPaymentApplication>();
}
