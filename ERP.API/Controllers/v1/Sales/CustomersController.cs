using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Sales;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers.v1.Sales;

[Route("api/v1/sales/customers")]
public sealed class CustomersController(AppDbContext dbContext) : SalesControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] SalesCustomerPagedRequest request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var isDesc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = dbContext.FinCustomers
            .AsNoTracking()
            .Include(x => x.CustomerCategory)
            .Include(x => x.PriceList)
            .Include(x => x.SalesEmployee)
            .Include(x => x.SalesTeam)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(search) ||
                x.Name.ToLower().Contains(search) ||
                (x.ContactPerson != null && x.ContactPerson.ToLower().Contains(search)) ||
                (x.TaxId != null && x.TaxId.ToLower().Contains(search)) ||
                (x.CustomerCategory != null && x.CustomerCategory.Name.ToLower().Contains(search)) ||
                (x.PriceList != null && x.PriceList.Name.ToLower().Contains(search)) ||
                (x.SalesEmployee != null && x.SalesEmployee.FullName.ToLower().Contains(search)) ||
                (x.SalesTeam != null && x.SalesTeam.Name.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var code = request.Code.Trim().ToLowerInvariant();
            query = query.Where(x => x.Code.ToLower().Contains(code));
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var name = request.Name.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(name));
        }

        if (request.CustomerCategoryId.HasValue)
        {
            query = query.Where(x => x.CustomerCategoryId == request.CustomerCategoryId.Value);
        }

        if (request.PriceListId.HasValue)
        {
            query = query.Where(x => x.PriceListId == request.PriceListId.Value);
        }

        if (request.SalesEmployeeId.HasValue)
        {
            query = query.Where(x => x.SalesEmployeeId == request.SalesEmployeeId.Value);
        }

        if (request.SalesTeamId.HasValue)
        {
            query = query.Where(x => x.SalesTeamId == request.SalesTeamId.Value);
        }

        if (request.CreditLimitFrom.HasValue)
        {
            query = query.Where(x => x.CreditLimit >= request.CreditLimitFrom.Value);
        }

        if (request.CreditLimitTo.HasValue)
        {
            query = query.Where(x => x.CreditLimit <= request.CreditLimitTo.Value);
        }

        if (request.CreditUsedFrom.HasValue)
        {
            query = query.Where(x => x.CreditUsed >= request.CreditUsedFrom.Value);
        }

        if (request.CreditUsedTo.HasValue)
        {
            query = query.Where(x => x.CreditUsed <= request.CreditUsedTo.Value);
        }

        if (request.LastOrderDateFrom.HasValue)
        {
            query = query.Where(x => x.LastOrderDate.HasValue && x.LastOrderDate.Value >= request.LastOrderDateFrom.Value);
        }

        if (request.LastOrderDateTo.HasValue)
        {
            query = query.Where(x => x.LastOrderDate.HasValue && x.LastOrderDate.Value <= request.LastOrderDateTo.Value);
        }

        if (request.TotalYtdSalesFrom.HasValue)
        {
            query = query.Where(x => x.TotalYtdSales >= request.TotalYtdSalesFrom.Value);
        }

        if (request.TotalYtdSalesTo.HasValue)
        {
            query = query.Where(x => x.TotalYtdSales <= request.TotalYtdSalesTo.Value);
        }

        if (request.IsOverCreditLimit.HasValue)
        {
            query = request.IsOverCreditLimit.Value
                ? query.Where(x => x.CreditUsed > x.CreditLimit)
                : query.Where(x => x.CreditUsed <= x.CreditLimit);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "name" => isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "customercategoryname" => isDesc ? query.OrderByDescending(x => x.CustomerCategory != null ? x.CustomerCategory.Name : string.Empty).ThenByDescending(x => x.Code) : query.OrderBy(x => x.CustomerCategory != null ? x.CustomerCategory.Name : string.Empty).ThenBy(x => x.Code),
            "pricelistname" => isDesc ? query.OrderByDescending(x => x.PriceList != null ? x.PriceList.Name : string.Empty).ThenByDescending(x => x.Code) : query.OrderBy(x => x.PriceList != null ? x.PriceList.Name : string.Empty).ThenBy(x => x.Code),
            "salesemployeename" => isDesc ? query.OrderByDescending(x => x.SalesEmployee != null ? x.SalesEmployee.FullName : string.Empty).ThenByDescending(x => x.Code) : query.OrderBy(x => x.SalesEmployee != null ? x.SalesEmployee.FullName : string.Empty).ThenBy(x => x.Code),
            "salesteamname" => isDesc ? query.OrderByDescending(x => x.SalesTeam != null ? x.SalesTeam.Name : string.Empty).ThenByDescending(x => x.Code) : query.OrderBy(x => x.SalesTeam != null ? x.SalesTeam.Name : string.Empty).ThenBy(x => x.Code),
            "creditlimit" => isDesc ? query.OrderByDescending(x => x.CreditLimit).ThenByDescending(x => x.Code) : query.OrderBy(x => x.CreditLimit).ThenBy(x => x.Code),
            "creditused" => isDesc ? query.OrderByDescending(x => x.CreditUsed).ThenByDescending(x => x.Code) : query.OrderBy(x => x.CreditUsed).ThenBy(x => x.Code),
            "lastorderdate" => isDesc ? query.OrderByDescending(x => x.LastOrderDate).ThenByDescending(x => x.Code) : query.OrderBy(x => x.LastOrderDate).ThenBy(x => x.Code),
            "totalytdsales" => isDesc ? query.OrderByDescending(x => x.TotalYtdSales).ThenByDescending(x => x.Code) : query.OrderBy(x => x.TotalYtdSales).ThenBy(x => x.Code),
            "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Code) : query.OrderBy(x => x.IsActive).ThenBy(x => x.Code),
            _ => isDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new SalesCustomerDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                TaxId = x.TaxId,
                ContactPerson = x.ContactPerson,
                CustomerCategoryId = x.CustomerCategoryId,
                CustomerCategoryCode = x.CustomerCategory != null ? x.CustomerCategory.Code : null,
                CustomerCategoryName = x.CustomerCategory != null ? x.CustomerCategory.Name : null,
                PriceListId = x.PriceListId,
                PriceListCode = x.PriceList != null ? x.PriceList.Code : null,
                PriceListName = x.PriceList != null ? x.PriceList.Name : null,
                SalesEmployeeId = x.SalesEmployeeId,
                SalesEmployeeName = x.SalesEmployee != null ? x.SalesEmployee.FullName : null,
                SalesTeamId = x.SalesTeamId,
                SalesTeamCode = x.SalesTeam != null ? x.SalesTeam.Code : null,
                SalesTeamName = x.SalesTeam != null ? x.SalesTeam.Name : null,
                CreditLimit = x.CreditLimit,
                CreditUsed = x.CreditUsed,
                LastOrderDate = x.LastOrderDate,
                TotalYtdSales = x.TotalYtdSales,
                IsOverCreditLimit = x.CreditUsed > x.CreditLimit,
                IsActive = x.IsActive
            })
            .ToListAsync(ct);

        return Ok(PagedResult<SalesCustomerDto>.Create(items, totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await dbContext.FinCustomers
            .AsNoTracking()
            .Include(x => x.CustomerCategory)
            .Include(x => x.PriceList)
            .Include(x => x.SalesEmployee)
            .Include(x => x.SalesTeam)
            .Where(x => x.Id == id)
            .Select(x => new SalesCustomerDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                TaxId = x.TaxId,
                ContactPerson = x.ContactPerson,
                CustomerCategoryId = x.CustomerCategoryId,
                CustomerCategoryCode = x.CustomerCategory != null ? x.CustomerCategory.Code : null,
                CustomerCategoryName = x.CustomerCategory != null ? x.CustomerCategory.Name : null,
                PriceListId = x.PriceListId,
                PriceListCode = x.PriceList != null ? x.PriceList.Code : null,
                PriceListName = x.PriceList != null ? x.PriceList.Name : null,
                SalesEmployeeId = x.SalesEmployeeId,
                SalesEmployeeName = x.SalesEmployee != null ? x.SalesEmployee.FullName : null,
                SalesTeamId = x.SalesTeamId,
                SalesTeamCode = x.SalesTeam != null ? x.SalesTeam.Code : null,
                SalesTeamName = x.SalesTeam != null ? x.SalesTeam.Name : null,
                CreditLimit = x.CreditLimit,
                CreditUsed = x.CreditUsed,
                LastOrderDate = x.LastOrderDate,
                TotalYtdSales = x.TotalYtdSales,
                IsOverCreditLimit = x.CreditUsed > x.CreditLimit,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }
}
