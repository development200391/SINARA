namespace ERP.Application.DTOs.HR;

public sealed class AttendanceSettingDto
{
    public int AttendancePeriodStartDay { get; set; } = 26;
    public int AttendancePeriodEndDay { get; set; } = 25;
    public int CheckInToleranceMinutes { get; set; } = 10;
    public TimeOnly WorkStart { get; set; } = new(8, 0);
    public TimeOnly WorkEnd { get; set; } = new(17, 0);
    public TimeOnly BreakStart { get; set; } = new(12, 0);
    public TimeOnly BreakEnd { get; set; } = new(13, 0);
    public int MinimumOtMinutes { get; set; } = 60;
}

