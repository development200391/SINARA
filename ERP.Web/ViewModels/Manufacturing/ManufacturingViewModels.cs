using System.ComponentModel.DataAnnotations;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;
using ERP.Application.DTOs.Inventory;
using ERP.Application.DTOs.Manufacturing;
using ERP.Domain.Enums.Manufacturing;

namespace ERP.Web.ViewModels.Manufacturing;

public sealed class ManufacturingDashboardViewModel
{
    public ManufacturingDashboardDto Data { get; set; } = new();
}

public sealed class ManufacturingLinkedOptionViewModel
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public int? ItemId { get; set; }
}

public sealed class ManufacturingWorkOrdersIndexViewModel
{
    public string? Search { get; set; }
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "code";
    public string SortDirection { get; set; } = "asc";

    public string? CodeFilter { get; set; }
    public WorkOrderStatus? StatusFilter { get; set; }
    public ProductionType? ProductionTypeFilter { get; set; }
    public bool? IsActiveFilter { get; set; }

    public PagedResult<ManufacturingWorkOrderDto> Items { get; set; } = PagedResult<ManufacturingWorkOrderDto>.Create([], 0, 1, 20);
}

public sealed class ManufacturingMrpRunsIndexViewModel
{
    public string? Search { get; set; }
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "code";
    public string SortDirection { get; set; } = "asc";

    public string? CodeFilter { get; set; }
    public MrpStatus? StatusFilter { get; set; }
    public DateOnly? RunDateFromFilter { get; set; }
    public DateOnly? RunDateToFilter { get; set; }

    public PagedResult<ManufacturingMrpRunDto> Items { get; set; } = PagedResult<ManufacturingMrpRunDto>.Create([], 0, 1, 20);
}

public sealed class ManufacturingQcInspectionsIndexViewModel
{
    public string? Search { get; set; }
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "code";
    public string SortDirection { get; set; } = "asc";

    public string? CodeFilter { get; set; }
    public QcStatus? StatusFilter { get; set; }
    public QcResult? ResultFilter { get; set; }
    public DateTimeOffset? InspectedFromFilter { get; set; }
    public DateTimeOffset? InspectedToFilter { get; set; }

    public PagedResult<ManufacturingQcInspectionDto> Items { get; set; } = PagedResult<ManufacturingQcInspectionDto>.Create([], 0, 1, 20);
}

public sealed class ManufacturingScrapRecordsIndexViewModel
{
    public string? Search { get; set; }
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "recordedat";
    public string SortDirection { get; set; } = "desc";

    public string? CodeFilter { get; set; }
    public ScrapReason? ReasonFilter { get; set; }
    public DateTimeOffset? RecordedFromFilter { get; set; }
    public DateTimeOffset? RecordedToFilter { get; set; }

    public PagedResult<ManufacturingScrapRecordDto> Items { get; set; } = PagedResult<ManufacturingScrapRecordDto>.Create([], 0, 1, 20);
}

public sealed class ManufacturingReworkOrdersIndexViewModel
{
    public string? Search { get; set; }
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "openedat";
    public string SortDirection { get; set; } = "desc";

    public string? CodeFilter { get; set; }
    public WorkOrderStatus? StatusFilter { get; set; }
    public DateTimeOffset? OpenedFromFilter { get; set; }
    public DateTimeOffset? OpenedToFilter { get; set; }
    public DateTimeOffset? ClosedFromFilter { get; set; }
    public DateTimeOffset? ClosedToFilter { get; set; }

    public PagedResult<ManufacturingReworkOrderDto> Items { get; set; } = PagedResult<ManufacturingReworkOrderDto>.Create([], 0, 1, 20);
}

public sealed class ManufacturingBomsIndexViewModel
{
    public string? Search { get; set; }
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "code";
    public string SortDirection { get; set; } = "asc";

    public string? CodeFilter { get; set; }
    public BomStatus? StatusFilter { get; set; }
    public bool? IsActiveFilter { get; set; }
    public DateOnly? EffectiveDateFromFilter { get; set; }
    public DateOnly? EffectiveDateToFilter { get; set; }

    public PagedResult<ManufacturingBomDto> Items { get; set; } = PagedResult<ManufacturingBomDto>.Create([], 0, 1, 20);
}

public sealed class ManufacturingRoutingsIndexViewModel
{
    public string? Search { get; set; }
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "code";
    public string SortDirection { get; set; } = "asc";

    public string? CodeFilter { get; set; }
    public string? NameFilter { get; set; }
    public RoutingStatus? StatusFilter { get; set; }
    public bool? IsActiveFilter { get; set; }

    public PagedResult<ManufacturingRoutingDto> Items { get; set; } = PagedResult<ManufacturingRoutingDto>.Create([], 0, 1, 20);
}

public sealed class ManufacturingWorkCentersIndexViewModel
{
    public string? Search { get; set; }
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "code";
    public string SortDirection { get; set; } = "asc";

    public string? CodeFilter { get; set; }
    public string? NameFilter { get; set; }
    public bool? IsActiveFilter { get; set; }

    public PagedResult<ManufacturingWorkCenterDto> Items { get; set; } = PagedResult<ManufacturingWorkCenterDto>.Create([], 0, 1, 20);
}

public sealed class ManufacturingQcParametersIndexViewModel
{
    public string? Search { get; set; }
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "code";
    public string SortDirection { get; set; } = "asc";

    public string? CodeFilter { get; set; }
    public string? NameFilter { get; set; }
    public QcParameterType? ParameterTypeFilter { get; set; }
    public bool? IsCriticalFilter { get; set; }
    public bool? IsActiveFilter { get; set; }

    public PagedResult<ManufacturingQcParameterDto> Items { get; set; } = PagedResult<ManufacturingQcParameterDto>.Create([], 0, 1, 20);
}

public sealed class ManufacturingProductionOutputReportViewModel
{
    public string? Search { get; set; }
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "workordercode";
    public string SortDirection { get; set; } = "asc";

    public WorkOrderStatus? StatusFilter { get; set; }
    public DateOnly? PlannedStartFromFilter { get; set; }
    public DateOnly? PlannedStartToFilter { get; set; }

    public PagedResult<ManufacturingProductionOutputReportDto> Items { get; set; } = PagedResult<ManufacturingProductionOutputReportDto>.Create([], 0, 1, 20);
}

public sealed class ManufacturingOeeReportViewModel
{
    public string? Search { get; set; }
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "snapshotdate";
    public string SortDirection { get; set; } = "desc";

    public DateOnly? SnapshotDateFromFilter { get; set; }
    public DateOnly? SnapshotDateToFilter { get; set; }

    public PagedResult<ManufacturingOeeReportDto> Items { get; set; } = PagedResult<ManufacturingOeeReportDto>.Create([], 0, 1, 20);
}

public sealed class ManufacturingCostVarianceReportViewModel
{
    public string? Search { get; set; }
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "varianceamount";
    public string SortDirection { get; set; } = "desc";

    public WorkOrderStatus? StatusFilter { get; set; }

    public PagedResult<ManufacturingCostVarianceReportDto> Items { get; set; } = PagedResult<ManufacturingCostVarianceReportDto>.Create([], 0, 1, 20);
}

public sealed class ManufacturingScrapAnalysisReportViewModel
{
    public string? Search { get; set; }
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "totalscrapcost";
    public string SortDirection { get; set; } = "desc";

    public ScrapReason? ReasonFilter { get; set; }
    public DateTimeOffset? RecordedFromFilter { get; set; }
    public DateTimeOffset? RecordedToFilter { get; set; }

    public PagedResult<ManufacturingScrapAnalysisReportDto> Items { get; set; } = PagedResult<ManufacturingScrapAnalysisReportDto>.Create([], 0, 1, 20);
}

public sealed class ManufacturingCapacityReportViewModel
{
    public string? Search { get; set; }
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "utilizationpct";
    public string SortDirection { get; set; } = "desc";

    public DateOnly? PlannedStartFromFilter { get; set; }
    public DateOnly? PlannedStartToFilter { get; set; }

    public PagedResult<ManufacturingCapacityReportDto> Items { get; set; } = PagedResult<ManufacturingCapacityReportDto>.Create([], 0, 1, 20);
}

public sealed class ManufacturingWorkCenterEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(30)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999999999999.99")]
    public decimal CapacityHoursPerDay { get; set; }

    [Range(typeof(decimal), "0", "999999999999999.99")]
    public decimal LaborCostPerHour { get; set; }

    [Range(typeof(decimal), "0", "999999999999999.99")]
    public decimal OverheadCostPerHour { get; set; }

    public int? WipAccountId { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}

public sealed class ManufacturingRoutingEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(30)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int? ItemId { get; set; }
    public int? WorkCenterId { get; set; }

    [Range(1, int.MaxValue)]
    public int Version { get; set; } = 1;

    public RoutingStatus Status { get; set; } = RoutingStatus.Draft;

    [Range(typeof(decimal), "0", "999999999999999.99")]
    public decimal TotalLeadTimeHours { get; set; }

    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public IReadOnlyList<InventoryOptionDto> ItemOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> WorkCenterOptions { get; set; } = [];
}

public sealed class ManufacturingBomEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(30)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue)]
    public int ItemId { get; set; }

    public int? RoutingId { get; set; }

    [Range(1, int.MaxValue)]
    public int Version { get; set; } = 1;

    public BomStatus Status { get; set; } = BomStatus.Draft;

    [Range(typeof(decimal), "0.0001", "999999999999999.9999")]
    public decimal QtyProduced { get; set; } = 1m;

    [Range(typeof(decimal), "0", "999999999999999.9999")]
    public decimal StandardCost { get; set; }

    [Required]
    public DateOnly EffectiveDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public IReadOnlyList<InventoryOptionDto> ItemOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> RoutingOptions { get; set; } = [];
}

public sealed class ManufacturingQcParameterEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(30)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int? ItemId { get; set; }
    public QcParameterType ParameterType { get; set; } = QcParameterType.Numeric;

    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }

    public bool IsCritical { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public IReadOnlyList<InventoryOptionDto> ItemOptions { get; set; } = [];
}

public sealed class ManufacturingWorkOrderEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(30)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue)]
    public int ItemId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int BomId { get; set; }

    public int? RoutingId { get; set; }
    public int? WorkCenterId { get; set; }
    public int? MrpRunId { get; set; }

    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Draft;
    public ProductionType ProductionType { get; set; } = ProductionType.MakeToStock;

    [Range(typeof(decimal), "0.0001", "999999999999999.9999")]
    public decimal QtyPlanned { get; set; }

    [Range(typeof(decimal), "0", "999999999999999.9999")]
    public decimal QtyGood { get; set; }

    [Range(typeof(decimal), "0", "999999999999999.9999")]
    public decimal QtyScrap { get; set; }

    [Required]
    public DateOnly PlannedStartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required]
    public DateOnly PlannedEndDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public DateTimeOffset? ActualStartAt { get; set; }
    public DateTimeOffset? ActualEndAt { get; set; }

    [Range(typeof(decimal), "0", "999999999999999.9999")]
    public decimal StandardCostTotal { get; set; }

    [Range(typeof(decimal), "0", "999999999999999.9999")]
    public decimal ActualCostTotal { get; set; }

    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public IReadOnlyList<InventoryOptionDto> ItemOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> BomOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> RoutingOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> WorkCenterOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> MrpRunOptions { get; set; } = [];
    public IReadOnlyList<ManufacturingLinkedOptionViewModel> BomItemMappings { get; set; } = [];
    public IReadOnlyList<ManufacturingLinkedOptionViewModel> RoutingItemMappings { get; set; } = [];
}

public sealed class ManufacturingMrpRunEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(30)]
    public string Code { get; set; } = string.Empty;

    [Required]
    public DateOnly RunDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public MrpStatus Status { get; set; } = MrpStatus.Draft;

    [Range(1, int.MaxValue)]
    public int HorizonDays { get; set; } = 30;

    [Range(0, int.MaxValue)]
    public int TotalDemandItems { get; set; }

    [Range(0, int.MaxValue)]
    public int RecommendedWoCount { get; set; }

    [Range(0, int.MaxValue)]
    public int RecommendedPrCount { get; set; }

    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? Notes { get; set; }
}

public sealed class ManufacturingQcInspectionEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(30)]
    public string Code { get; set; } = string.Empty;

    public int? WorkOrderId { get; set; }
    public int? ItemId { get; set; }
    public int? InspectorEmployeeId { get; set; }

    public DateTimeOffset InspectedAt { get; set; } = DateTimeOffset.Now;

    public QcStatus Status { get; set; } = QcStatus.Pending;
    public QcResult Result { get; set; } = QcResult.Pass;
    public string? Notes { get; set; }

    public IReadOnlyList<InventoryOptionDto> WorkOrderOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> ItemOptions { get; set; } = [];
    public IReadOnlyList<LookupDto> InspectorEmployeeOptions { get; set; } = [];
    public IReadOnlyList<ManufacturingLinkedOptionViewModel> WorkOrderItemMappings { get; set; } = [];
}

public sealed class ManufacturingScrapRecordEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(30)]
    public string Code { get; set; } = string.Empty;

    public int? WorkOrderId { get; set; }
    public int? ItemId { get; set; }
    public int? WorkCenterId { get; set; }

    public ScrapReason Reason { get; set; } = ScrapReason.Other;

    [Range(typeof(decimal), "0.0001", "999999999999999.9999")]
    public decimal QtyScrap { get; set; }

    [Range(typeof(decimal), "0", "999999999999999.9999")]
    public decimal UnitCost { get; set; }

    public DateTimeOffset RecordedAt { get; set; } = DateTimeOffset.Now;
    public string? Notes { get; set; }

    public IReadOnlyList<InventoryOptionDto> WorkOrderOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> ItemOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> WorkCenterOptions { get; set; } = [];
    public IReadOnlyList<ManufacturingLinkedOptionViewModel> WorkOrderItemMappings { get; set; } = [];
}

public sealed class ManufacturingReworkOrderEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(30)]
    public string Code { get; set; } = string.Empty;

    public int? SourceWorkOrderId { get; set; }
    public int? WorkOrderId { get; set; }
    public int? ItemId { get; set; }

    [Range(typeof(decimal), "0.0001", "999999999999999.9999")]
    public decimal QtyRework { get; set; }

    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Draft;
    public DateTimeOffset OpenedAt { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset? ClosedAt { get; set; }
    public string? Notes { get; set; }

    public IReadOnlyList<InventoryOptionDto> SourceWorkOrderOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> WorkOrderOptions { get; set; } = [];
    public IReadOnlyList<InventoryOptionDto> ItemOptions { get; set; } = [];
    public IReadOnlyList<ManufacturingLinkedOptionViewModel> WorkOrderItemMappings { get; set; } = [];
}

