namespace ERP.Domain.Interfaces.Inventory;

public interface IItemRepository
{
    Task<InventoryItemReadModel?> GetByCodeAsync(string itemCode, CancellationToken ct = default);
    Task<InventoryItemReadModel?> GetBySkuAsync(string sku, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryItemReadModel>> SearchAsync(string keyword, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryItemReadModel>> GetLowStockAsync(CancellationToken ct = default);
}
