using ERP.Application.DTOs.Sales;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Sales;

[Route("api/v1/sales/dashboard")]
public sealed class DashboardController(AppDbContext dbContext) : SalesControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var activeCategoryCountTask = dbContext.SalCustomerCategories
            .AsNoTracking()
            .CountAsync(x => x.IsActive, ct);

        var activePriceListCountTask = dbContext.SalPriceLists
            .AsNoTracking()
            .CountAsync(x => x.IsActive, ct);

        var activeSalesTeamCountTask = dbContext.SalSalesTeams
            .AsNoTracking()
            .CountAsync(x => x.IsActive, ct);

        var overCreditLimitCustomerCountTask = dbContext.FinCustomers
            .AsNoTracking()
            .CountAsync(x => x.IsActive && x.CreditUsed > x.CreditLimit, ct);

        await Task.WhenAll(
            activeCategoryCountTask,
            activePriceListCountTask,
            activeSalesTeamCountTask,
            overCreditLimitCustomerCountTask);

        return Ok(new SalesDashboardDto
        {
            ActiveCustomerCategoryCount = activeCategoryCountTask.Result,
            ActivePriceListCount = activePriceListCountTask.Result,
            ActiveSalesTeamCount = activeSalesTeamCountTask.Result,
            OverCreditLimitCustomerCount = overCreditLimitCustomerCountTask.Result
        });
    }
}
