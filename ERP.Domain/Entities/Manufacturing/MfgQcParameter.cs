using ERP.Domain.Entities.Inventory;
using ERP.Domain.Enums.Manufacturing;

namespace ERP.Domain.Entities.Manufacturing;

public sealed class MfgQcParameter : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? ItemId { get; set; }
    public QcParameterType ParameterType { get; set; } = QcParameterType.Numeric;
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public bool IsCritical { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public InvItem? Item { get; set; }
}
