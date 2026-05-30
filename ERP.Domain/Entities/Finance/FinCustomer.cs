namespace ERP.Domain.Entities.Finance;

public sealed class FinCustomer : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ContactPerson { get; set; }
    public decimal CreditLimit { get; set; }
    public int PaymentTermsDays { get; set; } = 30;
    public int? DefaultAccountId { get; set; }
    public int? DefaultTaxCodeId { get; set; }
    public bool IsActive { get; set; } = true;

    public FinAccount? DefaultAccount { get; set; }
    public FinTaxCode? DefaultTaxCode { get; set; }
    public ICollection<FinArInvoice> ArInvoices { get; set; } = new List<FinArInvoice>();
    public ICollection<FinArReceipt> ArReceipts { get; set; } = new List<FinArReceipt>();
}
