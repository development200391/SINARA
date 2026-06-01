using ERP.Application.DTOs.Common;
using ERP.Domain.Enums.FixedAssets;

namespace ERP.Application.DTOs.FixedAssets;

public sealed class FixedAssetOptionDto
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
}

public sealed class FixedAssetDashboardDto
{
    public int ActiveAssetCount { get; set; }
    public decimal TotalAcquisitionCost { get; set; }
    public decimal TotalBookValue { get; set; }
    public decimal MonthlyDepreciationAmount { get; set; }
    public int AssetsInMaintenance { get; set; }
    public int DisposedAssetCount { get; set; }
}

public sealed class FixedAssetCategoryDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DepreciationMethod DepreciationMethod { get; set; } = DepreciationMethod.StraightLine;
    public int UsefulLifeMonths { get; set; }
    public decimal? DepreciationRate { get; set; }
    public int? AssetAccountId { get; set; }
    public int? AccumulatedDepreciationAccountId { get; set; }
    public int? DepreciationExpenseAccountId { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class FixedAssetCategoryPagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public DepreciationMethod? DepreciationMethod { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class FixedAssetLocationDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class FixedAssetLocationPagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public int? DepartmentId { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class FixedAssetDepreciationConfigDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public short FiscalYear { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public byte RunDay { get; set; } = 28;
    public bool IsAutoPostJournal { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class FixedAssetDepreciationConfigPagedRequest : PagedRequest
{
    public short? FiscalYear { get; set; }
    public bool? IsAutoPostJournal { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class FixedAssetDto
{
    public int Id { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int LocationId { get; set; }
    public string LocationCode { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public DateOnly AcquisitionDate { get; set; }
    public DateOnly InServiceDate { get; set; }
    public decimal AcquisitionCost { get; set; }
    public decimal SalvageValue { get; set; }
    public int UsefulLifeMonths { get; set; }
    public DepreciationMethod DepreciationMethod { get; set; } = DepreciationMethod.StraightLine;
    public decimal? DepreciationRate { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public decimal BookValue { get; set; }
    public AssetStatus Status { get; set; } = AssetStatus.Active;
    public string? SerialNumber { get; set; }
    public string? VendorName { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int DocumentCount { get; set; }
}

public sealed class FixedAssetPagedRequest : PagedRequest
{
    public string? AssetCode { get; set; }
    public string? Name { get; set; }
    public int? CategoryId { get; set; }
    public int? LocationId { get; set; }
    public int? DepartmentId { get; set; }
    public AssetStatus? Status { get; set; }
    public decimal? BookValueFrom { get; set; }
    public decimal? BookValueTo { get; set; }
    public DateOnly? AcquisitionDateFrom { get; set; }
    public DateOnly? AcquisitionDateTo { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class FixedAssetScheduleDto
{
    public int Id { get; set; }
    public short PeriodYear { get; set; }
    public byte PeriodMonth { get; set; }
    public DateOnly DepreciationDate { get; set; }
    public decimal DepreciationAmount { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public decimal BookValue { get; set; }
    public DepreciationScheduleStatus Status { get; set; } = DepreciationScheduleStatus.Pending;
}

public sealed class FixedAssetTransferDto
{
    public int Id { get; set; }
    public string TransferNo { get; set; } = string.Empty;
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public DateOnly TransferDate { get; set; }
    public int FromLocationId { get; set; }
    public string FromLocationName { get; set; } = string.Empty;
    public int ToLocationId { get; set; }
    public string ToLocationName { get; set; } = string.Empty;
    public int? FromDepartmentId { get; set; }
    public string? FromDepartmentName { get; set; }
    public int? ToDepartmentId { get; set; }
    public string? ToDepartmentName { get; set; }
    public string? Reason { get; set; }
    public AssetTransferStatus Status { get; set; } = AssetTransferStatus.Draft;
}

public sealed class FixedAssetTransferPagedRequest : PagedRequest
{
    public int? AssetId { get; set; }
    public int? FromLocationId { get; set; }
    public int? ToLocationId { get; set; }
    public AssetTransferStatus? Status { get; set; }
    public DateOnly? TransferDateFrom { get; set; }
    public DateOnly? TransferDateTo { get; set; }
}

public sealed class FixedAssetMaintenanceOrderDto
{
    public int Id { get; set; }
    public string WorkOrderNo { get; set; } = string.Empty;
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public DateOnly OrderDate { get; set; }
    public MaintenanceType MaintenanceType { get; set; } = MaintenanceType.Preventive;
    public string? VendorName { get; set; }
    public decimal Cost { get; set; }
    public bool IsCapitalized { get; set; }
    public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Open;
    public string? Notes { get; set; }
}

public sealed class FixedAssetMaintenanceOrderPagedRequest : PagedRequest
{
    public int? AssetId { get; set; }
    public MaintenanceType? MaintenanceType { get; set; }
    public MaintenanceStatus? Status { get; set; }
    public DateOnly? OrderDateFrom { get; set; }
    public DateOnly? OrderDateTo { get; set; }
}

public sealed class FixedAssetDisposalDto
{
    public int Id { get; set; }
    public string DisposalNo { get; set; } = string.Empty;
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public DateOnly DisposalDate { get; set; }
    public DisposalType DisposalType { get; set; } = DisposalType.Sale;
    public decimal? SaleAmount { get; set; }
    public decimal DisposalExpense { get; set; }
    public decimal? GainLossAmount { get; set; }
    public DisposalStatus Status { get; set; } = DisposalStatus.Draft;
    public string? Notes { get; set; }
}

public sealed class FixedAssetDisposalPagedRequest : PagedRequest
{
    public int? AssetId { get; set; }
    public DisposalType? DisposalType { get; set; }
    public DisposalStatus? Status { get; set; }
    public DateOnly? DisposalDateFrom { get; set; }
    public DateOnly? DisposalDateTo { get; set; }
}

public sealed class FixedAssetRevaluationDto
{
    public int Id { get; set; }
    public string RevaluationNo { get; set; } = string.Empty;
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public DateOnly RevaluationDate { get; set; }
    public decimal OldBookValue { get; set; }
    public decimal NewBookValue { get; set; }
    public decimal ImpairmentAmount { get; set; }
    public RevaluationStatus Status { get; set; } = RevaluationStatus.Draft;
    public string? Notes { get; set; }
}

public sealed class FixedAssetRevaluationPagedRequest : PagedRequest
{
    public int? AssetId { get; set; }
    public RevaluationStatus? Status { get; set; }
    public DateOnly? RevaluationDateFrom { get; set; }
    public DateOnly? RevaluationDateTo { get; set; }
}

public sealed class FixedAssetDepreciationRunDto
{
    public int Id { get; set; }
    public string RunNo { get; set; } = string.Empty;
    public short PeriodYear { get; set; }
    public byte PeriodMonth { get; set; }
    public DateOnly RunDate { get; set; }
    public int TotalAssetCount { get; set; }
    public decimal TotalDepreciationAmount { get; set; }
    public DepreciationRunStatus Status { get; set; } = DepreciationRunStatus.Draft;
    public string? ApprovedByName { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public int? JournalEntryId { get; set; }
}

public sealed class FixedAssetDepreciationRunPagedRequest : PagedRequest
{
    public short? PeriodYear { get; set; }
    public byte? PeriodMonth { get; set; }
    public DepreciationRunStatus? Status { get; set; }
}

public sealed class RunDepreciationRequest
{
    public short PeriodYear { get; set; }
    public byte PeriodMonth { get; set; }
    public bool ApproveImmediately { get; set; }
}

public sealed class FixedAssetHistoryDto
{
    public int Id { get; set; }
    public DateOnly EventDate { get; set; }
    public AssetHistoryType EventType { get; set; } = AssetHistoryType.Registration;
    public string? ReferenceNo { get; set; }
    public string? Description { get; set; }
    public decimal? AmountChange { get; set; }
}

public sealed class FixedAssetDetailDto
{
    public FixedAssetDto Asset { get; set; } = new();
    public IReadOnlyList<FixedAssetScheduleDto> DepreciationSchedules { get; set; } = [];
    public IReadOnlyList<FixedAssetTransferDto> Transfers { get; set; } = [];
    public IReadOnlyList<FixedAssetMaintenanceOrderDto> MaintenanceOrders { get; set; } = [];
    public IReadOnlyList<FixedAssetDisposalDto> Disposals { get; set; } = [];
    public IReadOnlyList<FixedAssetRevaluationDto> Revaluations { get; set; } = [];
    public IReadOnlyList<FixedAssetHistoryDto> Histories { get; set; } = [];
}
