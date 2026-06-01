namespace ERP.Domain.Enums.Inventory;

public enum StockMovementType
{
    GoodsReceipt = 0,
    GoodsIssue = 1,
    TransferIn = 2,
    TransferOut = 3,
    AdjustmentIn = 4,
    AdjustmentOut = 5,
    OpnameIn = 6,
    OpnameOut = 7,
    OpeningBalance = 8
}
