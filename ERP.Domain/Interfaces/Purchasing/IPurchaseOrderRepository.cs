namespace ERP.Domain.Interfaces.Purchasing;

public interface IPurchaseOrderRepository
{
    Task<PurchaseOrderReadModel?> GetByCodeAsync(string poNo, CancellationToken ct = default);
    Task<IReadOnlyList<PurchaseOrderReadModel>> GetPendingReceiptAsync(CancellationToken ct = default);
    Task<IReadOnlyList<PurchaseOrderReadModel>> GetByVendorAsync(int vendorId, CancellationToken ct = default);
    Task<IReadOnlyList<PurchaseOrderReadModel>> GetOverdueAsync(DateOnly asOfDate, CancellationToken ct = default);
}
