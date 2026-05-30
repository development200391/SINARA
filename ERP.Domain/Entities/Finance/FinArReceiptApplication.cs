namespace ERP.Domain.Entities.Finance;

public sealed class FinArReceiptApplication
{
    public int Id { get; set; }
    public int ReceiptId { get; set; }
    public int InvoiceId { get; set; }
    public decimal AppliedAmount { get; set; }

    public FinArReceipt Receipt { get; set; } = null!;
    public FinArInvoice Invoice { get; set; } = null!;
}
