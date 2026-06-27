namespace ERP.Domain.Interfaces.Sales;

public interface ISalesIntegrationService
{
    Task<int?> CreateArInvoiceFromDeliveryAsync(int salesDeliveryId, CancellationToken ct = default);
    Task<int?> CreateDeliveryOrderFromSoAsync(int salesOrderId, CancellationToken ct = default);
    Task<bool> ReverseArInvoiceAsync(int arInvoiceId, string reason, CancellationToken ct = default);
}
