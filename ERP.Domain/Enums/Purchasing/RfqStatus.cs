namespace ERP.Domain.Enums.Purchasing;

public enum RfqStatus
{
    Draft = 0,
    SentToVendor = 1,
    QuotationReceived = 2,
    Evaluated = 3,
    Awarded = 4,
    Cancelled = 5
}
