namespace ERP.Domain.Entities.HR;

public sealed class HrAttendanceSetting : BaseEntity
{
    public string SingletonKey { get; set; } = "default";
    public int AttendancePeriodStartDay { get; set; } = 26;
    public int AttendancePeriodEndDay { get; set; } = 25;
    public int CheckInToleranceMinutes { get; set; } = 10;
    public TimeOnly WorkStart { get; set; } = new(8, 0);
    public TimeOnly WorkEnd { get; set; } = new(17, 0);
    public TimeOnly BreakStart { get; set; } = new(12, 0);
    public TimeOnly BreakEnd { get; set; } = new(13, 0);
    public int MinimumOtMinutes { get; set; } = 60;
    public decimal? OfficeLatitude { get; set; }
    public decimal? OfficeLongitude { get; set; }
    public int RadiusMeters { get; set; } = 100;
}

