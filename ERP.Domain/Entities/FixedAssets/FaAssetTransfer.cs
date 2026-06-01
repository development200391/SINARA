using ERP.Domain.Entities.HR;
using ERP.Domain.Enums.FixedAssets;

namespace ERP.Domain.Entities.FixedAssets;

public sealed class FaAssetTransfer : BaseEntity
{
    public string TransferNo { get; set; } = string.Empty;
    public int AssetId { get; set; }
    public DateOnly TransferDate { get; set; }
    public int FromLocationId { get; set; }
    public int ToLocationId { get; set; }
    public int? FromDepartmentId { get; set; }
    public int? ToDepartmentId { get; set; }
    public string? Reason { get; set; }
    public AssetTransferStatus Status { get; set; } = AssetTransferStatus.Draft;
    public int? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }

    public FaAsset Asset { get; set; } = null!;
    public FaLocation FromLocation { get; set; } = null!;
    public FaLocation ToLocation { get; set; } = null!;
    public HrDepartment? FromDepartment { get; set; }
    public HrDepartment? ToDepartment { get; set; }
    public HrEmployee? ApprovedByEmployee { get; set; }
}
