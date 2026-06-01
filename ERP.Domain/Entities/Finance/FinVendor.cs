using ERP.Domain.Entities.Purchasing;

namespace ERP.Domain.Entities.Finance;

public sealed class FinVendor : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ContactPerson { get; set; }
    public int PaymentTermsDays { get; set; } = 30;
    public int? DefaultAccountId { get; set; }
    public int? DefaultTaxCodeId { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNo { get; set; }
    public int? VendorCategoryId { get; set; }
    public int? BuyerGroupId { get; set; }
    public bool IsApprovedVendor { get; set; }
    public DateOnly? ApprovedDate { get; set; }
    public int? LeadTimeDays { get; set; }
    public decimal? PerformanceScore { get; set; }
    public bool IsActive { get; set; } = true;

    public FinAccount? DefaultAccount { get; set; }
    public FinTaxCode? DefaultTaxCode { get; set; }
    public PurVendorCategory? VendorCategory { get; set; }
    public PurBuyerGroup? BuyerGroup { get; set; }
    public ICollection<FinApInvoice> ApInvoices { get; set; } = new List<FinApInvoice>();
    public ICollection<FinApPayment> ApPayments { get; set; } = new List<FinApPayment>();
}
