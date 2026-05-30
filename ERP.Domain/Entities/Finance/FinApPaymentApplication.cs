namespace ERP.Domain.Entities.Finance;

public sealed class FinApPaymentApplication
{
    public int Id { get; set; }
    public int PaymentId { get; set; }
    public int InvoiceId { get; set; }
    public decimal AppliedAmount { get; set; }

    public FinApPayment Payment { get; set; } = null!;
    public FinApInvoice Invoice { get; set; } = null!;
}
