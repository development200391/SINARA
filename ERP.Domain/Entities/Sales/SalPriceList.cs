using ERP.Domain.Entities.Finance;
using ERP.Domain.Enums.Sales;

namespace ERP.Domain.Entities.Sales;

public sealed class SalPriceList : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public PriceListType Type { get; set; } = PriceListType.Standard;
    public string CurrencyCode { get; set; } = "IDR";
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public FinCurrency Currency { get; set; } = null!;
    public ICollection<SalPriceListItem> Items { get; set; } = new List<SalPriceListItem>();
    public ICollection<SalCustomerCategory> CustomerCategories { get; set; } = new List<SalCustomerCategory>();
    public ICollection<FinCustomer> Customers { get; set; } = new List<FinCustomer>();
}
