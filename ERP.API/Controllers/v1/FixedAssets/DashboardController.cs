using ERP.Application.DTOs.FixedAssets;
using ERP.Domain.Enums.FixedAssets;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.FixedAssets;

[Route("api/v1/fixed-assets/dashboard")]
public sealed class DashboardController(AppDbContext dbContext) : FixedAssetsControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var year = (short)now.Year;
        var month = (byte)now.Month;

        var activeAssets = dbContext.FaAssets
            .AsNoTracking()
            .Where(x => x.IsActive && x.Status != AssetStatus.Disposed);

        var dashboard = new FixedAssetDashboardDto
        {
            ActiveAssetCount = await activeAssets.CountAsync(ct),
            TotalAcquisitionCost = await activeAssets.SumAsync(x => x.AcquisitionCost, ct),
            TotalBookValue = await activeAssets.SumAsync(x => x.BookValue, ct),
            MonthlyDepreciationAmount = await dbContext.FaDepreciationSchedules
                .AsNoTracking()
                .Where(x => x.PeriodYear == year && x.PeriodMonth == month)
                .SumAsync(x => x.DepreciationAmount, ct),
            AssetsInMaintenance = await dbContext.FaAssets
                .AsNoTracking()
                .CountAsync(x => x.Status == AssetStatus.InMaintenance, ct),
            DisposedAssetCount = await dbContext.FaAssets
                .AsNoTracking()
                .CountAsync(x => x.Status == AssetStatus.Disposed, ct)
        };

        return Ok(dashboard);
    }
}
