namespace ERP.Domain.Interfaces.Inventory;

public interface IInventoryIntegrationService
{
    Task<int?> CreateGoodsReceiptJournalAsync(int goodsReceiptId, CancellationToken ct = default);
    Task<int?> CreateGoodsIssueJournalAsync(int goodsIssueId, CancellationToken ct = default);
    Task<int?> CreateAdjustmentJournalAsync(int adjustmentId, CancellationToken ct = default);
}
