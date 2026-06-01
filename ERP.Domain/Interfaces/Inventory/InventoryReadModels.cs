using ERP.Domain.Enums.Inventory;

namespace ERP.Domain.Interfaces.Inventory;

public sealed record InventoryItemReadModel(
    int Id,
    string ItemCode,
    string Name,
    string? Sku,
    ItemType Type,
    ItemStatus Status,
    decimal MinimumStockLevel);

public sealed record InventoryStockBalanceReadModel(
    int ItemId,
    int WarehouseId,
    int? LocationId,
    decimal QuantityOnHand,
    decimal AverageCost,
    DateTimeOffset? LastMovementAt);

public sealed record InventoryStockMovementReadModel(
    long Id,
    int ItemId,
    int WarehouseId,
    int? LocationId,
    StockMovementType MovementType,
    decimal QuantityIn,
    decimal QuantityOut,
    decimal UnitCost,
    DateTimeOffset MovementAt,
    string ReferenceNo,
    string? Notes);

public sealed record InventoryStockCardLineReadModel(
    DateTimeOffset MovementAt,
    string ReferenceNo,
    StockMovementType MovementType,
    decimal QuantityIn,
    decimal QuantityOut,
    decimal RunningBalance,
    decimal UnitCost,
    decimal BalanceValue,
    string? Notes);

public sealed record InventoryWarehouseLocationReadModel(
    int Id,
    int WarehouseId,
    string Code,
    string Name,
    bool IsActive);

public sealed record InventoryWarehouseReadModel(
    int Id,
    string Code,
    string Name,
    bool IsActive,
    IReadOnlyList<InventoryWarehouseLocationReadModel> Locations);
