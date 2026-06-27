namespace ERP.Web.Controllers;

public sealed partial class SalesController
{
    [HttpGet("customer-categories")]
    public async Task<IActionResult> CustomerCategories(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "code",
        string? sortDirection = "asc",
        string? code = null,
        string? name = null,
        int? defaultPriceListId = null,
        int? defaultPaymentTermsFrom = null,
        int? defaultPaymentTermsTo = null,
        decimal? defaultCreditLimitFrom = null,
        decimal? defaultCreditLimitTo = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "code", "code", "name", "defaultpricelistname", "defaultpaymentterms", "defaultcreditlimit", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedCode = NormalizeText(code);
        var normalizedName = NormalizeText(name);
        var (normalizedTermsFrom, normalizedTermsTo) = NormalizeIntRange(defaultPaymentTermsFrom, defaultPaymentTermsTo);
        var (normalizedCreditFrom, normalizedCreditTo) = NormalizeDecimalRange(defaultCreditLimitFrom, defaultCreditLimitTo);

        var priceListOptionsTask = salesApiClient.GetPriceListOptionsAsync(accessToken, ct);
        var itemsTask = salesApiClient.GetCustomerCategoriesAsync(accessToken, new SalesCustomerCategoryPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = normalizedCode,
            Name = normalizedName,
            DefaultPriceListId = defaultPriceListId,
            DefaultPaymentTermsFrom = normalizedTermsFrom,
            DefaultPaymentTermsTo = normalizedTermsTo,
            DefaultCreditLimitFrom = normalizedCreditFrom,
            DefaultCreditLimitTo = normalizedCreditTo,
            IsActive = isActive
        }, ct);

        await Task.WhenAll(priceListOptionsTask, itemsTask);

        ViewData["Title"] = "Customer Categories";
        ViewData["Breadcrumb"] = "Sales / Customer Categories";

        return View("CustomerCategories/Index", new SalesCustomerCategoriesIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = normalizedCode,
            NameFilter = normalizedName,
            DefaultPriceListIdFilter = defaultPriceListId,
            DefaultPaymentTermsFromFilter = normalizedTermsFrom,
            DefaultPaymentTermsToFilter = normalizedTermsTo,
            DefaultCreditLimitFromFilter = normalizedCreditFrom,
            DefaultCreditLimitToFilter = normalizedCreditTo,
            IsActiveFilter = isActive,
            PriceListOptions = await priceListOptionsTask,
            Items = await itemsTask ?? PagedResult<SalesCustomerCategoryDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("customer-categories/create")]
    public async Task<IActionResult> CreateCustomerCategory(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new SalesCustomerCategoryEditViewModel
        {
            IsActive = true,
            PriceListOptions = await salesApiClient.GetPriceListOptionsAsync(accessToken, ct)
        };

        ViewData["Title"] = "Create Customer Category";
        ViewData["Breadcrumb"] = "Sales / Customer Categories / Create";

        return View("CustomerCategories/Create", model);
    }

    [HttpPost("customer-categories/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCustomerCategory(SalesCustomerCategoryEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        NormalizeCustomerCategoryForm(model);
        model.PriceListOptions = await salesApiClient.GetPriceListOptionsAsync(accessToken, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Customer Category";
            ViewData["Breadcrumb"] = "Sales / Customer Categories / Create";
            return View("CustomerCategories/Create", model);
        }

        var created = await salesApiClient.CreateCustomerCategoryAsync(accessToken, MapCustomerCategoryRequest(model), ct);
        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to create customer category." : created.ErrorMessage);
            ViewData["Title"] = "Create Customer Category";
            ViewData["Breadcrumb"] = "Sales / Customer Categories / Create";
            return View("CustomerCategories/Create", model);
        }

        TempData["SuccessMessage"] = "Customer category created.";
        return RedirectToAction(nameof(CustomerCategories));
    }

    [HttpGet("customer-categories/edit/{id:int}")]
    public async Task<IActionResult> EditCustomerCategory(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var category = await salesApiClient.GetCustomerCategoryByIdAsync(accessToken, id, ct);
        if (category is null)
        {
            return NotFound();
        }

        var model = new SalesCustomerCategoryEditViewModel
        {
            Id = category.Id,
            Code = category.Code,
            Name = category.Name,
            DefaultPriceListId = category.DefaultPriceListId,
            DefaultPaymentTerms = category.DefaultPaymentTerms,
            DefaultCreditLimit = category.DefaultCreditLimit,
            Description = category.Description,
            IsActive = category.IsActive,
            PriceListOptions = await salesApiClient.GetPriceListOptionsAsync(accessToken, ct)
        };

        ViewData["Title"] = "Edit Customer Category";
        ViewData["Breadcrumb"] = "Sales / Customer Categories / Edit";

        return View("CustomerCategories/Edit", model);
    }

    [HttpPost("customer-categories/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCustomerCategory(int id, SalesCustomerCategoryEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        NormalizeCustomerCategoryForm(model);
        model.PriceListOptions = await salesApiClient.GetPriceListOptionsAsync(accessToken, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Customer Category";
            ViewData["Breadcrumb"] = "Sales / Customer Categories / Edit";
            return View("CustomerCategories/Edit", model);
        }

        var updated = await salesApiClient.UpdateCustomerCategoryAsync(accessToken, id, MapCustomerCategoryRequest(model), ct);
        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update customer category." : updated.ErrorMessage);
            ViewData["Title"] = "Edit Customer Category";
            ViewData["Breadcrumb"] = "Sales / Customer Categories / Edit";
            return View("CustomerCategories/Edit", model);
        }

        TempData["SuccessMessage"] = "Customer category updated.";
        return RedirectToAction(nameof(CustomerCategories));
    }

    [HttpPost("customer-categories/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCustomerCategory(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var deleted = await salesApiClient.DeleteCustomerCategoryAsync(accessToken, id, ct);
        TempData[deleted.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = deleted.IsSuccess
            ? "Customer category deleted."
            : (string.IsNullOrWhiteSpace(deleted.ErrorMessage) ? "Failed to delete customer category." : deleted.ErrorMessage);

        return RedirectToAction(nameof(CustomerCategories));
    }
}
