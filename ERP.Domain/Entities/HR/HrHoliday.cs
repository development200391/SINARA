using ERP.Domain.Enums;

namespace ERP.Domain.Entities.HR;

public sealed class HrHoliday : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public DateOnly HolidayDate { get; set; }
    public HolidayType HolidayType { get; set; } = HolidayType.National;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public string AppliesTo { get; set; } = "all";
    public short Year { get; private set; }
}
