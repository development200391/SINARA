using ERP.Application.DTOs.Common;
using ERP.Domain.Enums.Manufacturing;

namespace ERP.Application.DTOs.Manufacturing;

public sealed class ManufacturingDashboardDto
{
    public int ActiveWorkOrderCount { get; set; }
    public int OpenMrpRunCount { get; set; }
    public int PendingQcCount { get; set; }
    public decimal TotalScrapCost { get; set; }
    public decimal AverageOeePct { get; set; }
}

public sealed class ManufacturingWorkOrderDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int? ItemId { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public int? BomId { get; set; }
    public string? BomCode { get; set; }
    public int? RoutingId { get; set; }
    public string? RoutingCode { get; set; }
    public int? WorkCenterId { get; set; }
    public string? WorkCenterCode { get; set; }
    public string? WorkCenterName { get; set; }
    public int? MrpRunId { get; set; }
    public string? MrpRunCode { get; set; }
    public WorkOrderStatus Status { get; set; }
    public ProductionType ProductionType { get; set; }
    public decimal QtyPlanned { get; set; }
    public decimal QtyGood { get; set; }
    public decimal QtyScrap { get; set; }
    public DateOnly PlannedStartDate { get; set; }
    public DateOnly PlannedEndDate { get; set; }
    public DateTimeOffset? ActualStartAt { get; set; }
    public DateTimeOffset? ActualEndAt { get; set; }
    public decimal StandardCostTotal { get; set; }
    public decimal ActualCostTotal { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

public sealed class ManufacturingWorkOrderPagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public int? ItemId { get; set; }
    public int? WorkCenterId { get; set; }
    public WorkOrderStatus? Status { get; set; }
    public ProductionType? ProductionType { get; set; }
    public DateOnly? PlannedStartFrom { get; set; }
    public DateOnly? PlannedStartTo { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class ManufacturingMrpRunDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateOnly RunDate { get; set; }
    public MrpStatus Status { get; set; }
    public int HorizonDays { get; set; }
    public int TotalDemandItems { get; set; }
    public int RecommendedWoCount { get; set; }
    public int RecommendedPrCount { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? Notes { get; set; }
}

public sealed class ManufacturingMrpRunPagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public MrpStatus? Status { get; set; }
    public DateOnly? RunDateFrom { get; set; }
    public DateOnly? RunDateTo { get; set; }
}

public sealed class ManufacturingQcInspectionDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int? WorkOrderId { get; set; }
    public string? WorkOrderCode { get; set; }
    public int? ItemId { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public int? InspectorEmployeeId { get; set; }
    public string? InspectorEmployeeCode { get; set; }
    public string? InspectorEmployeeName { get; set; }
    public DateTimeOffset InspectedAt { get; set; }
    public QcStatus Status { get; set; }
    public QcResult Result { get; set; }
    public string? Notes { get; set; }
}

public sealed class ManufacturingQcInspectionPagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public int? WorkOrderId { get; set; }
    public int? ItemId { get; set; }
    public int? InspectorEmployeeId { get; set; }
    public QcStatus? Status { get; set; }
    public QcResult? Result { get; set; }
    public DateTimeOffset? InspectedFrom { get; set; }
    public DateTimeOffset? InspectedTo { get; set; }
}

public sealed class ManufacturingScrapRecordDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int? WorkOrderId { get; set; }
    public string? WorkOrderCode { get; set; }
    public int? ItemId { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public int? WorkCenterId { get; set; }
    public string? WorkCenterCode { get; set; }
    public string? WorkCenterName { get; set; }
    public ScrapReason Reason { get; set; }
    public decimal QtyScrap { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalScrapCost { get; set; }
    public DateTimeOffset RecordedAt { get; set; }
    public string? Notes { get; set; }
}

public sealed class ManufacturingScrapRecordPagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public int? WorkOrderId { get; set; }
    public int? ItemId { get; set; }
    public int? WorkCenterId { get; set; }
    public ScrapReason? Reason { get; set; }
    public DateTimeOffset? RecordedFrom { get; set; }
    public DateTimeOffset? RecordedTo { get; set; }
}

public sealed class ManufacturingReworkOrderDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int? SourceWorkOrderId { get; set; }
    public string? SourceWorkOrderCode { get; set; }
    public int? WorkOrderId { get; set; }
    public string? WorkOrderCode { get; set; }
    public int? ItemId { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public decimal QtyRework { get; set; }
    public WorkOrderStatus Status { get; set; }
    public DateTimeOffset OpenedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public string? Notes { get; set; }
}

public sealed class ManufacturingReworkOrderPagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public int? ItemId { get; set; }
    public WorkOrderStatus? Status { get; set; }
    public DateTimeOffset? OpenedFrom { get; set; }
    public DateTimeOffset? OpenedTo { get; set; }
    public DateTimeOffset? ClosedFrom { get; set; }
    public DateTimeOffset? ClosedTo { get; set; }
}

public sealed class ManufacturingBomDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int? ItemId { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public int? RoutingId { get; set; }
    public string? RoutingCode { get; set; }
    public int Version { get; set; }
    public BomStatus Status { get; set; }
    public decimal QtyProduced { get; set; }
    public decimal StandardCost { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

public sealed class ManufacturingBomPagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public int? ItemId { get; set; }
    public int? RoutingId { get; set; }
    public BomStatus? Status { get; set; }
    public DateOnly? EffectiveDateFrom { get; set; }
    public DateOnly? EffectiveDateTo { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class ManufacturingRoutingDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? ItemId { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public int? WorkCenterId { get; set; }
    public string? WorkCenterCode { get; set; }
    public string? WorkCenterName { get; set; }
    public int Version { get; set; }
    public RoutingStatus Status { get; set; }
    public decimal TotalLeadTimeHours { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

public sealed class ManufacturingRoutingPagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public int? ItemId { get; set; }
    public int? WorkCenterId { get; set; }
    public RoutingStatus? Status { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class ManufacturingWorkCenterDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal CapacityHoursPerDay { get; set; }
    public decimal LaborCostPerHour { get; set; }
    public decimal OverheadCostPerHour { get; set; }
    public int? WipAccountId { get; set; }
    public string? WipAccountCode { get; set; }
    public string? WipAccountName { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

public sealed class ManufacturingWorkCenterPagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class ManufacturingQcParameterDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? ItemId { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public QcParameterType ParameterType { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public bool IsCritical { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

public sealed class ManufacturingQcParameterPagedRequest : PagedRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public int? ItemId { get; set; }
    public QcParameterType? ParameterType { get; set; }
    public bool? IsCritical { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class ManufacturingProductionOutputReportDto
{
    public string WorkOrderCode { get; set; } = string.Empty;
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public decimal QtyPlanned { get; set; }
    public decimal QtyGood { get; set; }
    public decimal QtyScrap { get; set; }
    public decimal CompletionRatePct { get; set; }
}

public sealed class ManufacturingOeeReportDto
{
    public string WorkCenterCode { get; set; } = string.Empty;
    public string WorkCenterName { get; set; } = string.Empty;
    public DateOnly SnapshotDate { get; set; }
    public decimal AvailabilityPct { get; set; }
    public decimal PerformancePct { get; set; }
    public decimal QualityPct { get; set; }
    public decimal OeePct { get; set; }
}

public sealed class ManufacturingCostVarianceReportDto
{
    public string WorkOrderCode { get; set; } = string.Empty;
    public decimal StandardCostTotal { get; set; }
    public decimal ActualCostTotal { get; set; }
    public decimal VarianceAmount { get; set; }
    public decimal VariancePct { get; set; }
}

public sealed class ManufacturingScrapAnalysisReportDto
{
    public ScrapReason Reason { get; set; }
    public decimal TotalQtyScrap { get; set; }
    public decimal TotalScrapCost { get; set; }
}

public sealed class ManufacturingCapacityReportDto
{
    public string WorkCenterCode { get; set; } = string.Empty;
    public string WorkCenterName { get; set; } = string.Empty;
    public decimal CapacityHoursPerDay { get; set; }
    public decimal PlannedQtyTotal { get; set; }
    public decimal GoodQtyTotal { get; set; }
    public decimal UtilizationPct { get; set; }
}

public sealed class ManufacturingProductionOutputReportRequest : PagedRequest
{
    public WorkOrderStatus? Status { get; set; }
    public int? WorkCenterId { get; set; }
    public DateOnly? PlannedStartFrom { get; set; }
    public DateOnly? PlannedStartTo { get; set; }
}

public sealed class ManufacturingOeeReportRequest : PagedRequest
{
    public int? WorkCenterId { get; set; }
    public DateOnly? SnapshotDateFrom { get; set; }
    public DateOnly? SnapshotDateTo { get; set; }
}

public sealed class ManufacturingCostVarianceReportRequest : PagedRequest
{
    public WorkOrderStatus? Status { get; set; }
    public int? WorkCenterId { get; set; }
}

public sealed class ManufacturingScrapAnalysisReportRequest : PagedRequest
{
    public ScrapReason? Reason { get; set; }
    public DateTimeOffset? RecordedFrom { get; set; }
    public DateTimeOffset? RecordedTo { get; set; }
}

public sealed class ManufacturingCapacityReportRequest : PagedRequest
{
    public int? WorkCenterId { get; set; }
    public DateOnly? PlannedStartFrom { get; set; }
    public DateOnly? PlannedStartTo { get; set; }
}

public sealed class ManufacturingWorkOrderCompleteRequest
{
    public decimal QtyGood { get; set; }
    public decimal QtyScrap { get; set; }
    public string? Notes { get; set; }
}

public sealed class ManufacturingQcCompleteRequest
{
    public QcResult Result { get; set; } = QcResult.Pass;
    public string? Notes { get; set; }
}
