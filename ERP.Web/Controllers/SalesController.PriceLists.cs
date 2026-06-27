namespace ERP.Web.Controllers;

public sealed partial class SalesController
{
    [HttpGet("price-lists")]
    public async Task<IActionResult> PriceLists(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "code",
        string? sortDirection = "asc",
        string? code = null,
        string? name = null,
        PriceListType? type = null,
        string? currencyCode = null,
        DateOnly? validFromFrom = null,
        DateOnly? validFromTo = null,
        DateOnly? validToFrom = null,
        DateOnly? validToTo = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "code", "code", "name", "type", "currencycode", "validfrom", "validto", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedCode = NormalizeText(code);
        var normalizedName = NormalizeText(name);
        var normalizedCurrencyCode = NormalizeText(currencyCode)?.ToUpperInvariant();
        var (normalizedValidFromFrom, normalizedValidFromTo) = NormalizeDateRange(validFromFrom, validFromTo);
        var (normalizedValidToFrom, normalizedValidToTo) = NormalizeDateRange(validToFrom, validToTo);

        var currencyOptionsTask = LoadCurrencyCodesAsync(accessToken, ct);
        var itemsTask = salesApiClient.GetPriceListsAsync(accessToken, new SalesPriceListPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = normalizedCode,
            Name = normalizedName,
            Type = type,
            CurrencyCode = normalizedCurrencyCode,
            ValidFromFrom = normalizedValidFromFrom,
            ValidFromTo = normalizedValidFromTo,
            ValidToFrom = normalizedValidToFrom,
            ValidToTo = normalizedValidToTo,
            IsActive = isActive
        }, ct);

        await Task.WhenAll(currencyOptionsTask, itemsTask);

        ViewData["Title"] = "Price Lists";
        ViewData["Breadcrumb"] = "Sales / Price Lists";

        return View("PriceLists/Index", new SalesPriceListsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = normalizedCode,
            NameFilter = normalizedName,
            TypeFilter = type,
            CurrencyCodeFilter = normalizedCurrencyCode,
            ValidFromFromFilter = normalizedValidFromFrom,
            ValidFromToFilter = normalizedValidFromTo,
            ValidToFromFilter = normalizedValidToFrom,
            ValidToToFilter = normalizedValidToTo,
            IsActiveFilter = isActive,
            CurrencyOptions = await currencyOptionsTask,
            Items = await itemsTask ?? PagedResult<SalesPriceListDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("price-lists/create")]
    public async Task<IActionResult> CreatePriceList(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new SalesPriceListEditViewModel
        {
            ValidFrom = DateOnly.FromDateTime(DateTime.Today),
            IsActive = true,
            CurrencyOptions = await LoadCurrencyCodesAsync(accessToken, ct)
        };

        ViewData["Title"] = "Create Price List";
        ViewData["Breadcrumb"] = "Sales / Price Lists / Create";

        return View("PriceLists/Create", model);
    }

    [HttpPost("price-lists/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePriceList(SalesPriceListEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        NormalizePriceListForm(model);
        model.CurrencyOptions = await LoadCurrencyCodesAsync(accessToken, ct);

        if (model.ValidTo.HasValue && model.ValidTo.Value < model.ValidFrom)
        {
            ModelState.AddModelError(nameof(model.ValidTo), "Valid-to date must be greater than or equal to valid-from date.");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Price List";
            ViewData["Breadcrumb"] = "Sales / Price Lists / Create";
            return View("PriceLists/Create", model);
        }

        var created = await salesApiClient.CreatePriceListAsync(accessToken, MapPriceListRequest(model), ct);
        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to create price list." : created.ErrorMessage);
            ViewData["Title"] = "Create Price List";
            ViewData["Breadcrumb"] = "Sales / Price Lists / Create";
            return View("PriceLists/Create", model);
        }

        TempData["SuccessMessage"] = "Price list created.";
        return RedirectToAction(nameof(PriceLists));
    }

    [HttpGet("price-lists/edit/{id:int}")]
    public async Task<IActionResult> EditPriceList(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var priceList = await salesApiClient.GetPriceListByIdAsync(accessToken, id, ct);
        if (priceList is null)
        {
            return NotFound();
        }

        var model = new SalesPriceListEditViewModel
        {
            Id = priceList.Id,
            Code = priceList.Code,
            Name = priceList.Name,
            Type = priceList.Type,
            CurrencyCode = priceList.CurrencyCode,
            ValidFrom = priceList.ValidFrom,
            ValidTo = priceList.ValidTo,
            IsActive = priceList.IsActive,
            Notes = priceList.Notes,
            CurrencyOptions = await LoadCurrencyCodesAsync(accessToken, ct)
        };

        ViewData["Title"] = "Edit Price List";
        ViewData["Breadcrumb"] = "Sales / Price Lists / Edit";

        return View("PriceLists/Edit", model);
    }

    [HttpPost("price-lists/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPriceList(int id, SalesPriceListEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        NormalizePriceListForm(model);
        model.CurrencyOptions = await LoadCurrencyCodesAsync(accessToken, ct);

        if (model.ValidTo.HasValue && model.ValidTo.Value < model.ValidFrom)
        {
            ModelState.AddModelError(nameof(model.ValidTo), "Valid-to date must be greater than or equal to valid-from date.");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Price List";
            ViewData["Breadcrumb"] = "Sales / Price Lists / Edit";
            return View("PriceLists/Edit", model);
        }

        var updated = await salesApiClient.UpdatePriceListAsync(accessToken, id, MapPriceListRequest(model), ct);
        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update price list." : updated.ErrorMessage);
            ViewData["Title"] = "Edit Price List";
            ViewData["Breadcrumb"] = "Sales / Price Lists / Edit";
            return View("PriceLists/Edit", model);
        }

        TempData["SuccessMessage"] = "Price list updated.";
        return RedirectToAction(nameof(PriceLists));
    }

    [HttpPost("price-lists/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePriceList(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var deleted = await salesApiClient.DeletePriceListAsync(accessToken, id, ct);
        TempData[deleted.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = deleted.IsSuccess
            ? "Price list deleted."
            : (string.IsNullOrWhiteSpace(deleted.ErrorMessage) ? "Failed to delete price list." : deleted.ErrorMessage);

        return RedirectToAction(nameof(PriceLists));
    }
}
