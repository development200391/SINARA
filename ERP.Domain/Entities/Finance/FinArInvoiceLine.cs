namespace ERP.Domain.Entities.Finance;

public sealed class FinArInvoiceLine
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public int LineNo { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1m;
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
    public int? TaxCodeId { get; set; }
    public decimal TaxAmount { get; set; }
    public int AccountId { get; set; }
    public int? CostCenterId { get; set; }

    public FinArInvoice Invoice { get; set; } = null!;
    public FinTaxCode? TaxCode { get; set; }
    public FinAccount Account { get; set; } = null!;
    public FinCostCenter? CostCenter { get; set; }
}
