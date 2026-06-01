namespace ERP.Domain.Interfaces.Inventory;

public interface IStockRepository
{
    Task<InventoryStockBalanceReadModel?> GetCurrentStockAsync(int itemId, int warehouseId, int? locationId, CancellationToken ct = default);

    Task<IReadOnlyList<InventoryStockMovementReadModel>> GetMovementsAsync(
        int itemId,
        int? warehouseId = null,
        int? locationId = null,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<InventoryStockCardLineReadModel>> GetCardStokAsync(
        int itemId,
        int warehouseId,
        int? locationId,
        DateOnly dateFrom,
        DateOnly dateTo,
        CancellationToken ct = default);
}
