using ERP.Application.DTOs.Manufacturing;
using ERP.Domain.Enums.Manufacturing;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Manufacturing;

[Route("api/v1/manufacturing/dashboard")]
public sealed class DashboardController(AppDbContext dbContext) : ManufacturingControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var activeWorkOrderCountTask = dbContext.MfgWorkOrders
            .AsNoTracking()
            .CountAsync(x => x.IsActive && x.Status != WorkOrderStatus.Cancelled, ct);

        var openMrpRunCountTask = dbContext.MfgMrpRuns
            .AsNoTracking()
            .CountAsync(x => x.Status == MrpStatus.Draft || x.Status == MrpStatus.Running, ct);

        var pendingQcCountTask = dbContext.MfgQcInspections
            .AsNoTracking()
            .CountAsync(x => x.Status == QcStatus.Pending || x.Status == QcStatus.InProgress, ct);

        var totalScrapCostTask = dbContext.MfgScrapRecords
            .AsNoTracking()
            .SumAsync(x => (decimal?)x.TotalScrapCost, ct);

        var averageOeeTask = dbContext.MfgOeeSnapshots
            .AsNoTracking()
            .AverageAsync(x => (decimal?)x.OeePct, ct);

        await Task.WhenAll(activeWorkOrderCountTask, openMrpRunCountTask, pendingQcCountTask, totalScrapCostTask, averageOeeTask);

        return Ok(new ManufacturingDashboardDto
        {
            ActiveWorkOrderCount = activeWorkOrderCountTask.Result,
            OpenMrpRunCount = openMrpRunCountTask.Result,
            PendingQcCount = pendingQcCountTask.Result,
            TotalScrapCost = totalScrapCostTask.Result ?? 0m,
            AverageOeePct = averageOeeTask.Result ?? 0m
        });
    }
}
