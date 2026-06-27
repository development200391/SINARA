namespace ERP.Web.Controllers;

public sealed partial class SalesController
{
    [HttpGet("customers")]
    public async Task<IActionResult> Customers(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "code",
        string? sortDirection = "asc",
        string? code = null,
        string? name = null,
        int? customerCategoryId = null,
        int? priceListId = null,
        int? salesEmployeeId = null,
        int? salesTeamId = null,
        decimal? creditLimitFrom = null,
        decimal? creditLimitTo = null,
        decimal? creditUsedFrom = null,
        decimal? creditUsedTo = null,
        DateOnly? lastOrderDateFrom = null,
        DateOnly? lastOrderDateTo = null,
        decimal? totalYtdSalesFrom = null,
        decimal? totalYtdSalesTo = null,
        bool? isOverCreditLimit = null,
        bool? isActive = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "code", "code", "name", "customercategoryname", "pricelistname", "salesemployeename", "salesteamname", "creditlimit", "creditused", "lastorderdate", "totalytdsales", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedCode = NormalizeText(code);
        var normalizedName = NormalizeText(name);
        var (normalizedCreditLimitFrom, normalizedCreditLimitTo) = NormalizeDecimalRange(creditLimitFrom, creditLimitTo);
        var (normalizedCreditUsedFrom, normalizedCreditUsedTo) = NormalizeDecimalRange(creditUsedFrom, creditUsedTo);
        var (normalizedLastOrderFrom, normalizedLastOrderTo) = NormalizeDateRange(lastOrderDateFrom, lastOrderDateTo);
        var (normalizedYtdFrom, normalizedYtdTo) = NormalizeDecimalRange(totalYtdSalesFrom, totalYtdSalesTo);

        var categoryOptionsTask = salesApiClient.GetCustomerCategoryOptionsAsync(accessToken, ct);
        var priceListOptionsTask = salesApiClient.GetPriceListOptionsAsync(accessToken, ct);
        var teamOptionsTask = salesApiClient.GetTeamOptionsAsync(accessToken, ct);
        var employeeOptionsTask = hrApiClient.GetEmployeeOptionsAsync(accessToken, ct);
        var itemsTask = salesApiClient.GetCustomersAsync(accessToken, new SalesCustomerPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = normalizedCode,
            Name = normalizedName,
            CustomerCategoryId = customerCategoryId,
            PriceListId = priceListId,
            SalesEmployeeId = salesEmployeeId,
            SalesTeamId = salesTeamId,
            CreditLimitFrom = normalizedCreditLimitFrom,
            CreditLimitTo = normalizedCreditLimitTo,
            CreditUsedFrom = normalizedCreditUsedFrom,
            CreditUsedTo = normalizedCreditUsedTo,
            LastOrderDateFrom = normalizedLastOrderFrom,
            LastOrderDateTo = normalizedLastOrderTo,
            TotalYtdSalesFrom = normalizedYtdFrom,
            TotalYtdSalesTo = normalizedYtdTo,
            IsOverCreditLimit = isOverCreditLimit,
            IsActive = isActive
        }, ct);

        await Task.WhenAll(categoryOptionsTask, priceListOptionsTask, teamOptionsTask, employeeOptionsTask, itemsTask);

        ViewData["Title"] = "Sales Customers";
        ViewData["Breadcrumb"] = "Sales / Customers";

        return View("Customers/Index", new SalesCustomersIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = normalizedCode,
            NameFilter = normalizedName,
            CustomerCategoryIdFilter = customerCategoryId,
            PriceListIdFilter = priceListId,
            SalesEmployeeIdFilter = salesEmployeeId,
            SalesTeamIdFilter = salesTeamId,
            CreditLimitFromFilter = normalizedCreditLimitFrom,
            CreditLimitToFilter = normalizedCreditLimitTo,
            CreditUsedFromFilter = normalizedCreditUsedFrom,
            CreditUsedToFilter = normalizedCreditUsedTo,
            LastOrderDateFromFilter = normalizedLastOrderFrom,
            LastOrderDateToFilter = normalizedLastOrderTo,
            TotalYtdSalesFromFilter = normalizedYtdFrom,
            TotalYtdSalesToFilter = normalizedYtdTo,
            IsOverCreditLimitFilter = isOverCreditLimit,
            IsActiveFilter = isActive,
            CustomerCategoryOptions = await categoryOptionsTask,
            PriceListOptions = await priceListOptionsTask,
            SalesTeamOptions = await teamOptionsTask,
            SalesEmployeeOptions = (await employeeOptionsTask).OrderBy(x => x.Name).ToList(),
            Items = await itemsTask ?? PagedResult<SalesCustomerDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("customers/{id:int}")]
    public async Task<IActionResult> CustomerDetail(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var customer = await salesApiClient.GetCustomerByIdAsync(accessToken, id, ct);
        if (customer is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Sales Customer Detail";
        ViewData["Breadcrumb"] = "Sales / Customers / Detail";

        return View("Customers/Detail", new SalesCustomerDetailViewModel
        {
            Customer = customer
        });
    }
}
