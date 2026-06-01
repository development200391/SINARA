using ERP.Domain.Entities.Finance;

namespace ERP.Domain.Entities.Purchasing;

public sealed class PurVendorCategory : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<FinVendor> Vendors { get; set; } = new List<FinVendor>();
}
