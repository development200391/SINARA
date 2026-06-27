namespace ERP.Domain.Enums.Sales;

public enum SoStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    ConfirmedToCustomer = 3,
    PartialDelivered = 4,
    FullyDelivered = 5,
    Invoiced = 6,
    Closed = 7,
    Cancelled = 8
}
