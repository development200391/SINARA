namespace ERP.Domain.Interfaces.Sales;

public interface ISalesOrderRepository
{
    Task<SalesOrderReadModel?> GetByCodeAsync(string soNo, CancellationToken ct = default);
    Task<IReadOnlyList<SalesOrderReadModel>> GetPendingDeliveryAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SalesOrderReadModel>> GetByCustomerAsync(int customerId, CancellationToken ct = default);
    Task<IReadOnlyList<SalesOrderReadModel>> GetOverdueAsync(DateOnly asOfDate, CancellationToken ct = default);
}
