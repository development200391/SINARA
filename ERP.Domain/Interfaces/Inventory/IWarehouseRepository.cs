namespace ERP.Domain.Interfaces.Inventory;

public interface IWarehouseRepository
{
    Task<IReadOnlyList<InventoryWarehouseReadModel>> GetWithLocationsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<InventoryWarehouseReadModel>> GetActiveAsync(CancellationToken ct = default);
}
