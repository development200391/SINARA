namespace ERP.Domain.Interfaces.Sales;

public interface ISalesDeliveryRepository
{
    Task<SalesDeliveryReadModel?> GetByCodeAsync(string deliveryNo, CancellationToken ct = default);
    Task<IReadOnlyList<SalesDeliveryReadModel>> GetBySoAsync(int salesOrderId, CancellationToken ct = default);
    Task<IReadOnlyList<SalesDeliveryReadModel>> GetReadyToShipAsync(CancellationToken ct = default);
}
