namespace ERP.Domain.Entities.FixedAssets;

public sealed class FaDepreciationConfig : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public short FiscalYear { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public byte RunDay { get; set; } = 28;
    public bool IsAutoPostJournal { get; set; }
    public bool IsActive { get; set; } = true;
}
