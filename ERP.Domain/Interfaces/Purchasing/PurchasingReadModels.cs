using ERP.Domain.Enums.Purchasing;

namespace ERP.Domain.Interfaces.Purchasing;

public sealed record PurchaseRequisitionReadModel(
    int Id,
    string PrNo,
    DateOnly PrDate,
    PrStatus Status,
    int DepartmentId,
    int RequestedBy,
    decimal TotalAmount);

public sealed record PurchaseOrderReadModel(
    int Id,
    string PoNo,
    DateOnly PoDate,
    PoStatus Status,
    int VendorId,
    int BuyerEmployeeId,
    DateOnly ExpectedDeliveryDate,
    decimal TotalAmount);

public sealed record RfqReadModel(
    int Id,
    string RfqNo,
    DateOnly RfqDate,
    RfqStatus Status,
    DateOnly DeadlineDate,
    int BuyerEmployeeId);
