namespace ERP.Domain.Enums.Sales;

public enum QuotationStatus
{
    Draft = 0,
    SentToCustomer = 1,
    Negotiation = 2,
    Accepted = 3,
    Rejected = 4,
    Expired = 5,
    ConvertedToSo = 6,
    Cancelled = 7
}
