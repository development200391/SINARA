namespace ERP.Domain.Enums.Purchasing;

public enum PrStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Rejected = 3,
    ConvertedToPo = 4,
    Cancelled = 5
}
