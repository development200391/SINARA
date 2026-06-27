namespace ERP.Domain.Enums.Manufacturing;

public enum QcStatus
{
    Pending = 0,
    InProgress = 1,
    Passed = 2,
    Failed = 3,
    ConditionalPass = 4,
    Cancelled = 5
}
