namespace ERP.Domain.Enums.Purchasing;

public enum PoStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    SentToVendor = 3,
    PartialReceived = 4,
    FullyReceived = 5,
    Closed = 6,
    Cancelled = 7
}
