using ERP.Application.DTOs.Purchasing;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Purchasing;

[Route("api/v1/purchasing/dashboard")]
public sealed class DashboardController(AppDbContext dbContext) : PurchasingControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var approvedVendorCount = await dbContext.FinVendors
            .AsNoTracking()
            .CountAsync(x => x.IsApprovedVendor, ct);

        var dto = new PurchasingDashboardDto
        {
            PendingPrApprovalCount = 0,
            OverduePoCount = 0,
            CurrentMonthPoAmount = 0m,
            ApprovedVendorCount = approvedVendorCount
        };

        return Ok(dto);
    }
}
