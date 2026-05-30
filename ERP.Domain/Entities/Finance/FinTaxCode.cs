using ERP.Domain.Enums;

namespace ERP.Domain.Entities.Finance;

public sealed class FinTaxCode : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public FinanceTaxType Type { get; set; }
    public decimal Rate { get; set; }
    public bool IsInclusive { get; set; }
    public int AccountId { get; set; }
    public bool IsActive { get; set; } = true;

    public FinAccount Account { get; set; } = null!;
    public ICollection<FinVendor> VendorDefaults { get; set; } = new List<FinVendor>();
    public ICollection<FinApInvoiceLine> ApInvoiceLines { get; set; } = new List<FinApInvoiceLine>();
}
