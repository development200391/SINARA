using ERP.Application.DTOs.Common;
using ERP.Domain.Enums;

namespace ERP.Application.DTOs.HR;

public sealed class HolidayDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly HolidayDate { get; set; }
    public HolidayType HolidayType { get; set; } = HolidayType.National;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public string AppliesTo { get; set; } = "all";
    public short Year { get; set; }
}

public sealed class HolidayPagedRequest : PagedRequest
{
    public string? Name { get; set; }
    public DateOnly? HolidayDateFrom { get; set; }
    public DateOnly? HolidayDateTo { get; set; }
    public HolidayType? HolidayType { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public string? AppliesTo { get; set; }
    public short? YearFrom { get; set; }
    public short? YearTo { get; set; }
}
