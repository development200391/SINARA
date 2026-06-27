using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Manufacturing;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Manufacturing;

[Route("api/v1/manufacturing/reports")]
public sealed class ReportsController(AppDbContext dbContext) : ManufacturingControllerBase
{
    [HttpGet("production-output")]
    public async Task<IActionResult> ProductionOutput([FromQuery] ManufacturingProductionOutputReportRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.MfgWorkOrders
            .AsNoTracking()
            .Include(x => x.Item)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(search)
                || (x.Item != null && x.Item.ItemCode.ToLower().Contains(search))
                || (x.Item != null && x.Item.Name.ToLower().Contains(search))
                || (x.Notes != null && x.Notes.ToLower().Contains(search)));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (request.WorkCenterId.HasValue)
        {
            query = query.Where(x => x.WorkCenterId == request.WorkCenterId.Value);
        }

        if (request.PlannedStartFrom.HasValue)
        {
            query = query.Where(x => x.PlannedStartDate >= request.PlannedStartFrom.Value);
        }

        if (request.PlannedStartTo.HasValue)
        {
            query = query.Where(x => x.PlannedStartDate <= request.PlannedStartTo.Value);
        }

        var projected = query.Select(x => new
        {
            WorkOrderCode = x.Code,
            ItemCode = x.Item != null ? x.Item.ItemCode : null,
            ItemName = x.Item != null ? x.Item.Name : null,
            QtyPlanned = x.QtyPlanned,
            QtyGood = x.QtyGood,
            QtyScrap = x.QtyScrap,
            CompletionRatePct = x.QtyPlanned <= 0m ? 0m : (x.QtyGood * 100m / x.QtyPlanned)
        });

        projected = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "workordercode" => isDesc ? projected.OrderByDescending(x => x.WorkOrderCode) : projected.OrderBy(x => x.WorkOrderCode),
            "itemcode" => isDesc ? projected.OrderByDescending(x => x.ItemCode).ThenByDescending(x => x.WorkOrderCode) : projected.OrderBy(x => x.ItemCode).ThenBy(x => x.WorkOrderCode),
            "qtyplanned" => isDesc ? projected.OrderByDescending(x => x.QtyPlanned).ThenByDescending(x => x.WorkOrderCode) : projected.OrderBy(x => x.QtyPlanned).ThenBy(x => x.WorkOrderCode),
            "qtygood" => isDesc ? projected.OrderByDescending(x => x.QtyGood).ThenByDescending(x => x.WorkOrderCode) : projected.OrderBy(x => x.QtyGood).ThenBy(x => x.WorkOrderCode),
            "qtyscrap" => isDesc ? projected.OrderByDescending(x => x.QtyScrap).ThenByDescending(x => x.WorkOrderCode) : projected.OrderBy(x => x.QtyScrap).ThenBy(x => x.WorkOrderCode),
            "completionratepct" => isDesc ? projected.OrderByDescending(x => x.CompletionRatePct).ThenByDescending(x => x.WorkOrderCode) : projected.OrderBy(x => x.CompletionRatePct).ThenBy(x => x.WorkOrderCode),
            _ => isDesc ? projected.OrderByDescending(x => x.WorkOrderCode) : projected.OrderBy(x => x.WorkOrderCode)
        };

        var totalCount = await projected.CountAsync(ct);
        var rows = await projected
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = rows
            .Select(x => new ManufacturingProductionOutputReportDto
            {
                WorkOrderCode = x.WorkOrderCode,
                ItemCode = x.ItemCode,
                ItemName = x.ItemName,
                QtyPlanned = x.QtyPlanned,
                QtyGood = x.QtyGood,
                QtyScrap = x.QtyScrap,
                CompletionRatePct = decimal.Round(x.CompletionRatePct, 2, MidpointRounding.AwayFromZero)
            })
            .ToList();

        return Ok(PagedResult<ManufacturingProductionOutputReportDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("oee")]
    public async Task<IActionResult> Oee([FromQuery] ManufacturingOeeReportRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.MfgOeeSnapshots
            .AsNoTracking()
            .Include(x => x.WorkCenter)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.WorkCenter.Code.ToLower().Contains(search)
                || x.WorkCenter.Name.ToLower().Contains(search));
        }

        if (request.WorkCenterId.HasValue)
        {
            query = query.Where(x => x.WorkCenterId == request.WorkCenterId.Value);
        }

        if (request.SnapshotDateFrom.HasValue)
        {
            query = query.Where(x => x.SnapshotDate >= request.SnapshotDateFrom.Value);
        }

        if (request.SnapshotDateTo.HasValue)
        {
            query = query.Where(x => x.SnapshotDate <= request.SnapshotDateTo.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "workcentercode" => isDesc ? query.OrderByDescending(x => x.WorkCenter.Code).ThenByDescending(x => x.SnapshotDate) : query.OrderBy(x => x.WorkCenter.Code).ThenBy(x => x.SnapshotDate),
            "snapshotdate" => isDesc ? query.OrderByDescending(x => x.SnapshotDate).ThenByDescending(x => x.WorkCenter.Code) : query.OrderBy(x => x.SnapshotDate).ThenBy(x => x.WorkCenter.Code),
            "availabilitypct" => isDesc ? query.OrderByDescending(x => x.AvailabilityPct).ThenByDescending(x => x.SnapshotDate) : query.OrderBy(x => x.AvailabilityPct).ThenBy(x => x.SnapshotDate),
            "performancepct" => isDesc ? query.OrderByDescending(x => x.PerformancePct).ThenByDescending(x => x.SnapshotDate) : query.OrderBy(x => x.PerformancePct).ThenBy(x => x.SnapshotDate),
            "qualitypct" => isDesc ? query.OrderByDescending(x => x.QualityPct).ThenByDescending(x => x.SnapshotDate) : query.OrderBy(x => x.QualityPct).ThenBy(x => x.SnapshotDate),
            "oeepct" => isDesc ? query.OrderByDescending(x => x.OeePct).ThenByDescending(x => x.SnapshotDate) : query.OrderBy(x => x.OeePct).ThenBy(x => x.SnapshotDate),
            _ => isDesc ? query.OrderByDescending(x => x.SnapshotDate).ThenByDescending(x => x.WorkCenter.Code) : query.OrderBy(x => x.SnapshotDate).ThenBy(x => x.WorkCenter.Code)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ManufacturingOeeReportDto
            {
                WorkCenterCode = x.WorkCenter.Code,
                WorkCenterName = x.WorkCenter.Name,
                SnapshotDate = x.SnapshotDate,
                AvailabilityPct = x.AvailabilityPct,
                PerformancePct = x.PerformancePct,
                QualityPct = x.QualityPct,
                OeePct = x.OeePct
            })
            .ToListAsync(ct);

        return Ok(PagedResult<ManufacturingOeeReportDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("cost-variance")]
    public async Task<IActionResult> CostVariance([FromQuery] ManufacturingCostVarianceReportRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.MfgWorkOrders
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x => x.Code.ToLower().Contains(search));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (request.WorkCenterId.HasValue)
        {
            query = query.Where(x => x.WorkCenterId == request.WorkCenterId.Value);
        }

        var projected = query.Select(x => new
        {
            WorkOrderCode = x.Code,
            StandardCostTotal = x.StandardCostTotal,
            ActualCostTotal = x.ActualCostTotal,
            VarianceAmount = x.ActualCostTotal - x.StandardCostTotal,
            VariancePct = x.StandardCostTotal == 0m ? 0m : ((x.ActualCostTotal - x.StandardCostTotal) * 100m / x.StandardCostTotal)
        });

        projected = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "workordercode" => isDesc ? projected.OrderByDescending(x => x.WorkOrderCode) : projected.OrderBy(x => x.WorkOrderCode),
            "standardcosttotal" => isDesc ? projected.OrderByDescending(x => x.StandardCostTotal).ThenByDescending(x => x.WorkOrderCode) : projected.OrderBy(x => x.StandardCostTotal).ThenBy(x => x.WorkOrderCode),
            "actualcosttotal" => isDesc ? projected.OrderByDescending(x => x.ActualCostTotal).ThenByDescending(x => x.WorkOrderCode) : projected.OrderBy(x => x.ActualCostTotal).ThenBy(x => x.WorkOrderCode),
            "varianceamount" => isDesc ? projected.OrderByDescending(x => x.VarianceAmount).ThenByDescending(x => x.WorkOrderCode) : projected.OrderBy(x => x.VarianceAmount).ThenBy(x => x.WorkOrderCode),
            "variancepct" => isDesc ? projected.OrderByDescending(x => x.VariancePct).ThenByDescending(x => x.WorkOrderCode) : projected.OrderBy(x => x.VariancePct).ThenBy(x => x.WorkOrderCode),
            _ => isDesc ? projected.OrderByDescending(x => x.WorkOrderCode) : projected.OrderBy(x => x.WorkOrderCode)
        };

        var totalCount = await projected.CountAsync(ct);
        var rows = await projected
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = rows
            .Select(x => new ManufacturingCostVarianceReportDto
            {
                WorkOrderCode = x.WorkOrderCode,
                StandardCostTotal = x.StandardCostTotal,
                ActualCostTotal = x.ActualCostTotal,
                VarianceAmount = x.VarianceAmount,
                VariancePct = decimal.Round(x.VariancePct, 2, MidpointRounding.AwayFromZero)
            })
            .ToList();

        return Ok(PagedResult<ManufacturingCostVarianceReportDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("scrap-analysis")]
    public async Task<IActionResult> ScrapAnalysis([FromQuery] ManufacturingScrapAnalysisReportRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.MfgScrapRecords
            .AsNoTracking()
            .AsQueryable();

        if (request.Reason.HasValue)
        {
            query = query.Where(x => x.Reason == request.Reason.Value);
        }

        if (request.RecordedFrom.HasValue)
        {
            query = query.Where(x => x.RecordedAt >= request.RecordedFrom.Value);
        }

        if (request.RecordedTo.HasValue)
        {
            query = query.Where(x => x.RecordedAt <= request.RecordedTo.Value);
        }

        var grouped = query
            .GroupBy(x => x.Reason)
            .Select(x => new ManufacturingScrapAnalysisReportDto
            {
                Reason = x.Key,
                TotalQtyScrap = x.Sum(v => v.QtyScrap),
                TotalScrapCost = x.Sum(v => v.TotalScrapCost)
            });

        grouped = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "reason" => isDesc ? grouped.OrderByDescending(x => x.Reason) : grouped.OrderBy(x => x.Reason),
            "totalqtyscrap" => isDesc ? grouped.OrderByDescending(x => x.TotalQtyScrap).ThenByDescending(x => x.Reason) : grouped.OrderBy(x => x.TotalQtyScrap).ThenBy(x => x.Reason),
            "totalscrapcost" => isDesc ? grouped.OrderByDescending(x => x.TotalScrapCost).ThenByDescending(x => x.Reason) : grouped.OrderBy(x => x.TotalScrapCost).ThenBy(x => x.Reason),
            _ => isDesc ? grouped.OrderByDescending(x => x.TotalScrapCost).ThenByDescending(x => x.Reason) : grouped.OrderBy(x => x.TotalScrapCost).ThenBy(x => x.Reason)
        };

        var totalCount = await grouped.CountAsync(ct);
        var items = await grouped
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return Ok(PagedResult<ManufacturingScrapAnalysisReportDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("capacity")]
    public async Task<IActionResult> Capacity([FromQuery] ManufacturingCapacityReportRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.MfgWorkCenters
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(search)
                || x.Name.ToLower().Contains(search));
        }

        if (request.WorkCenterId.HasValue)
        {
            query = query.Where(x => x.Id == request.WorkCenterId.Value);
        }

        var projected = query.Select(x => new ManufacturingCapacityReportDto
        {
            WorkCenterCode = x.Code,
            WorkCenterName = x.Name,
            CapacityHoursPerDay = x.CapacityHoursPerDay,
            PlannedQtyTotal = x.WorkOrders
                .Where(wo =>
                    (!request.PlannedStartFrom.HasValue || wo.PlannedStartDate >= request.PlannedStartFrom.Value)
                    && (!request.PlannedStartTo.HasValue || wo.PlannedStartDate <= request.PlannedStartTo.Value))
                .Sum(wo => (decimal?)wo.QtyPlanned) ?? 0m,
            GoodQtyTotal = x.WorkOrders
                .Where(wo =>
                    (!request.PlannedStartFrom.HasValue || wo.PlannedStartDate >= request.PlannedStartFrom.Value)
                    && (!request.PlannedStartTo.HasValue || wo.PlannedStartDate <= request.PlannedStartTo.Value))
                .Sum(wo => (decimal?)wo.QtyGood) ?? 0m,
            UtilizationPct = 0m
        });

        projected = projected.Select(x => new ManufacturingCapacityReportDto
        {
            WorkCenterCode = x.WorkCenterCode,
            WorkCenterName = x.WorkCenterName,
            CapacityHoursPerDay = x.CapacityHoursPerDay,
            PlannedQtyTotal = x.PlannedQtyTotal,
            GoodQtyTotal = x.GoodQtyTotal,
            UtilizationPct = x.CapacityHoursPerDay <= 0m ? 0m : (x.GoodQtyTotal * 100m / x.CapacityHoursPerDay)
        });

        projected = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "workcentercode" => isDesc ? projected.OrderByDescending(x => x.WorkCenterCode) : projected.OrderBy(x => x.WorkCenterCode),
            "capacityhoursperday" => isDesc ? projected.OrderByDescending(x => x.CapacityHoursPerDay).ThenByDescending(x => x.WorkCenterCode) : projected.OrderBy(x => x.CapacityHoursPerDay).ThenBy(x => x.WorkCenterCode),
            "plannedqtytotal" => isDesc ? projected.OrderByDescending(x => x.PlannedQtyTotal).ThenByDescending(x => x.WorkCenterCode) : projected.OrderBy(x => x.PlannedQtyTotal).ThenBy(x => x.WorkCenterCode),
            "goodqtytotal" => isDesc ? projected.OrderByDescending(x => x.GoodQtyTotal).ThenByDescending(x => x.WorkCenterCode) : projected.OrderBy(x => x.GoodQtyTotal).ThenBy(x => x.WorkCenterCode),
            "utilizationpct" => isDesc ? projected.OrderByDescending(x => x.UtilizationPct).ThenByDescending(x => x.WorkCenterCode) : projected.OrderBy(x => x.UtilizationPct).ThenBy(x => x.WorkCenterCode),
            _ => isDesc ? projected.OrderByDescending(x => x.UtilizationPct).ThenByDescending(x => x.WorkCenterCode) : projected.OrderBy(x => x.UtilizationPct).ThenBy(x => x.WorkCenterCode)
        };

        var totalCount = await projected.CountAsync(ct);
        var rows = await projected
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = rows
            .Select(x => new ManufacturingCapacityReportDto
            {
                WorkCenterCode = x.WorkCenterCode,
                WorkCenterName = x.WorkCenterName,
                CapacityHoursPerDay = x.CapacityHoursPerDay,
                PlannedQtyTotal = x.PlannedQtyTotal,
                GoodQtyTotal = x.GoodQtyTotal,
                UtilizationPct = decimal.Round(x.UtilizationPct, 2, MidpointRounding.AwayFromZero)
            })
            .ToList();

        return Ok(PagedResult<ManufacturingCapacityReportDto>.Create(items, totalCount, page, pageSize));
    }
}
