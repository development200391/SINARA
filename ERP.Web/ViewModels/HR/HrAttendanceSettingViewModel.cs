using System.ComponentModel.DataAnnotations;

namespace ERP.Web.ViewModels.HR;

public sealed class HrAttendanceSettingViewModel
{
    [Range(1, 31)]
    public int AttendancePeriodStartDay { get; set; } = 26;

    [Range(1, 31)]
    public int AttendancePeriodEndDay { get; set; } = 25;

    public string CurrentPeriodPreview { get; set; } = string.Empty;

    [Range(0, 600)]
    public int CheckInToleranceMinutes { get; set; } = 10;

    [Required]
    public string WorkStart { get; set; } = "08:00";

    [Required]
    public string WorkEnd { get; set; } = "17:00";

    [Required]
    public string BreakStart { get; set; } = "12:00";

    [Required]
    public string BreakEnd { get; set; } = "13:00";

    [Range(0, 1440)]
    public int MinimumOtMinutes { get; set; } = 60;
}
