using ERP.Domain.Enums.Sales;

namespace ERP.Domain.Interfaces.Sales;

public sealed record SalesQuotationReadModel(
    int Id,
    string QuotationNo,
    DateOnly QuotationDate,
    DateOnly ValidUntil,
    QuotationStatus Status,
    int CustomerId,
    int SalesEmployeeId,
    decimal TotalAmount);

public sealed record SalesOrderReadModel(
    int Id,
    string SoNo,
    DateOnly SoDate,
    SoStatus Status,
    int CustomerId,
    int SalesEmployeeId,
    decimal TotalAmount);

public sealed record SalesDeliveryReadModel(
    int Id,
    string DeliveryNo,
    DateOnly DeliveryDate,
    DeliveryStatus Status,
    int SalesOrderId,
    decimal TotalQty);
