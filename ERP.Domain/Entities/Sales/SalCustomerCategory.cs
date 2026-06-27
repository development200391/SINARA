using ERP.Domain.Entities.Finance;

namespace ERP.Domain.Entities.Sales;

public sealed class SalCustomerCategory : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? DefaultPriceListId { get; set; }
    public int DefaultPaymentTerms { get; set; } = 2;
    public decimal DefaultCreditLimit { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public SalPriceList? DefaultPriceList { get; set; }
    public ICollection<FinCustomer> Customers { get; set; } = new List<FinCustomer>();
}
