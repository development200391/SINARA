namespace ERP.Domain.Interfaces.Purchasing;

public interface IPurchasingIntegrationService
{
    Task<int?> CreateApInvoiceFromReceiptAsync(int poReceiptId, CancellationToken ct = default);
    Task<int?> CreateGoodsReceiptFromPoReceiptAsync(int poReceiptId, CancellationToken ct = default);
    Task<bool> ReverseApInvoiceAsync(int apInvoiceId, string reason, CancellationToken ct = default);
}
