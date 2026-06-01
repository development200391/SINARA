using ERP.Domain.Entities.Inventory;
using ERP.Domain.Enums.Inventory;
using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Inventory;

internal static class InventoryStockMutationHelper
{
    public static async Task<decimal> GetAvailableAsync(
        AppDbContext dbContext,
        int itemId,
        int warehouseId,
        int? locationId,
        CancellationToken ct)
    {
        var balance = await dbContext.InvStockBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.ItemId == itemId &&
                x.WarehouseId == warehouseId &&
                x.LocationId == locationId,
                ct);

        return balance?.QtyAvailable ?? 0m;
    }

    public static async Task<StockMutationResult> ApplyMovementAsync(
        AppDbContext dbContext,
        int itemId,
        int warehouseId,
        int? locationId,
        decimal qtyDelta,
        decimal unitCost,
        DateOnly movementDate,
        StockMovementType movementType,
        string sourceTable,
        int sourceId,
        int? sourceLineId,
        string createdBy,
        string? notes,
        CancellationToken ct)
    {
        if (qtyDelta == 0)
        {
            throw new InvalidOperationException("Stock movement quantity cannot be zero.");
        }

        var now = DateTimeOffset.UtcNow;
        var normalizedCreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "system" : createdBy.Trim();

        var balance = await dbContext.InvStockBalances
            .FirstOrDefaultAsync(x =>
                x.ItemId == itemId &&
                x.WarehouseId == warehouseId &&
                x.LocationId == locationId,
                ct);

        if (balance is null)
        {
            balance = new InvStockBalance
            {
                ItemId = itemId,
                WarehouseId = warehouseId,
                LocationId = locationId,
                QtyOnHand = 0m,
                QtyReserved = 0m,
                AvgCost = 0m,
                CreatedBy = normalizedCreatedBy,
                CreatedAt = now
            };

            dbContext.InvStockBalances.Add(balance);
        }

        var currentQty = decimal.Round(balance.QtyOnHand, 4, MidpointRounding.AwayFromZero);
        var reservedQty = decimal.Round(balance.QtyReserved, 4, MidpointRounding.AwayFromZero);
        var currentAvailable = currentQty - reservedQty;
        var requestQty = decimal.Round(Math.Abs(qtyDelta), 4, MidpointRounding.AwayFromZero);

        if (qtyDelta < 0 && currentAvailable < requestQty)
        {
            throw new InvalidOperationException($"Insufficient stock. Available: {currentAvailable:N4}, Requested: {requestQty:N4}.");
        }

        var costForMovement = unitCost > 0
            ? decimal.Round(unitCost, 4, MidpointRounding.AwayFromZero)
            : decimal.Round(balance.AvgCost, 4, MidpointRounding.AwayFromZero);

        var newQty = decimal.Round(currentQty + qtyDelta, 4, MidpointRounding.AwayFromZero);
        if (newQty < 0)
        {
            throw new InvalidOperationException("Stock cannot go below zero.");
        }

        if (qtyDelta > 0)
        {
            if (newQty == 0)
            {
                balance.AvgCost = 0m;
            }
            else
            {
                var weightedTotal = (currentQty * balance.AvgCost) + (qtyDelta * costForMovement);
                balance.AvgCost = decimal.Round(weightedTotal / newQty, 4, MidpointRounding.AwayFromZero);
            }
        }

        balance.QtyOnHand = newQty;
        balance.LastMovementAt = now;
        balance.UpdatedBy = normalizedCreatedBy;
        balance.UpdatedAt = now;

        dbContext.InvStockMovements.Add(new InvStockMovement
        {
            MovementDate = movementDate,
            ItemId = itemId,
            WarehouseId = warehouseId,
            LocationId = locationId,
            MovementType = movementType,
            QtyIn = qtyDelta > 0 ? requestQty : 0m,
            QtyOut = qtyDelta < 0 ? requestQty : 0m,
            QtyBalance = newQty,
            UnitCost = costForMovement,
            TotalCost = decimal.Round(requestQty * costForMovement, 4, MidpointRounding.AwayFromZero),
            SourceTable = sourceTable,
            SourceId = sourceId,
            SourceLineId = sourceLineId,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            CreatedBy = normalizedCreatedBy,
            CreatedAt = now
        });

        return new StockMutationResult(newQty, decimal.Round(balance.AvgCost, 4, MidpointRounding.AwayFromZero), costForMovement);
    }
}

internal readonly record struct StockMutationResult(decimal QtyOnHand, decimal AvgCost, decimal UnitCost);
