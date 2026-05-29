using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;
using ERP.Domain.Enums;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.HR;

public sealed class HrHolidayIndexViewModel : PagedGridStateViewModel
{
    public HrHolidayIndexViewModel()
    {
        SortBy = "holidayDate";
        SortDirection = "desc";
    }

    public string? NameFilter { get; set; }
    public DateOnly? HolidayDateFromFilter { get; set; }
    public DateOnly? HolidayDateToFilter { get; set; }
    public HolidayType? HolidayTypeFilter { get; set; }
    public string? DescriptionFilter { get; set; }
    public bool? IsActiveFilter { get; set; }
    public string? AppliesToFilter { get; set; }
    public short? YearFromFilter { get; set; }
    public short? YearToFilter { get; set; }

    public PagedResult<HolidayDto> Holidays { get; set; } = PagedResult<HolidayDto>.Create([], 0, 1, 20);
}
