namespace ERP.Web.Controllers;

public sealed partial class SalesController
{
    [HttpGet("price-lists/{id:int}")]
    public async Task<IActionResult> PriceListDetail(
        int id,
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "itemcode",
        string? sortDirection = "asc",
        int? itemId = null,
        int? uomId = null,
        decimal? minQtyFrom = null,
        decimal? minQtyTo = null,
        decimal? unitPriceFrom = null,
        decimal? unitPriceTo = null,
        decimal? discountPctFrom = null,
        decimal? discountPctTo = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var priceListTask = salesApiClient.GetPriceListByIdAsync(accessToken, id, ct);
        var itemOptionsTask = inventoryApiClient.GetItemOptionsAsync(accessToken, ct);
        var uomOptionsTask = inventoryApiClient.GetUnitOptionsAsync(accessToken, ct);

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "itemcode", "itemcode", "itemname", "uomcode", "minqty", "unitprice", "discountpct");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedMinQtyFrom, normalizedMinQtyTo) = NormalizeDecimalRange(minQtyFrom, minQtyTo);
        var (normalizedUnitPriceFrom, normalizedUnitPriceTo) = NormalizeDecimalRange(unitPriceFrom, unitPriceTo);
        var (normalizedDiscountFrom, normalizedDiscountTo) = NormalizeDecimalRange(discountPctFrom, discountPctTo);

        var itemsTask = salesApiClient.GetPriceListItemsAsync(accessToken, id, new SalesPriceListItemPagedRequest
        {
            PriceListId = id,
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            ItemId = itemId,
            UomId = uomId,
            MinQtyFrom = normalizedMinQtyFrom,
            MinQtyTo = normalizedMinQtyTo,
            UnitPriceFrom = normalizedUnitPriceFrom,
            UnitPriceTo = normalizedUnitPriceTo,
            DiscountPctFrom = normalizedDiscountFrom,
            DiscountPctTo = normalizedDiscountTo
        }, ct);

        await Task.WhenAll(priceListTask, itemOptionsTask, uomOptionsTask, itemsTask);

        var priceList = await priceListTask;
        if (priceList is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Price List Detail";
        ViewData["Breadcrumb"] = "Sales / Price Lists / Detail";

        return View("PriceLists/Detail", new SalesPriceListDetailViewModel
        {
            PriceList = priceList,
            ItemSearch = search,
            ItemPageSize = normalizedPageSize,
            ItemSortBy = normalizedSortBy,
            ItemSortDirection = normalizedSortDirection,
            ItemIdFilter = itemId,
            UomIdFilter = uomId,
            MinQtyFromFilter = normalizedMinQtyFrom,
            MinQtyToFilter = normalizedMinQtyTo,
            UnitPriceFromFilter = normalizedUnitPriceFrom,
            UnitPriceToFilter = normalizedUnitPriceTo,
            DiscountPctFromFilter = normalizedDiscountFrom,
            DiscountPctToFilter = normalizedDiscountTo,
            ItemOptions = await itemOptionsTask,
            UomOptions = await uomOptionsTask,
            Items = await itemsTask ?? PagedResult<SalesPriceListItemDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("price-lists/{priceListId:int}/items/create")]
    public async Task<IActionResult> CreatePriceListItem(int priceListId, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var priceList = await salesApiClient.GetPriceListByIdAsync(accessToken, priceListId, ct);
        if (priceList is null)
        {
            return NotFound();
        }

        var model = new SalesPriceListItemEditViewModel
        {
            PriceListId = priceListId,
            MinQty = 1m
        };

        await PopulatePriceListItemOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create Price List Item";
        ViewData["Breadcrumb"] = "Sales / Price Lists / Detail / Add Item";
        ViewData["PriceListCode"] = priceList.Code;

        return View("PriceLists/Items/Create", model);
    }

    [HttpPost("price-lists/{priceListId:int}/items/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePriceListItem(int priceListId, SalesPriceListItemEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.PriceListId = priceListId;
        NormalizePriceListItemForm(model);
        await PopulatePriceListItemOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Price List Item";
            ViewData["Breadcrumb"] = "Sales / Price Lists / Detail / Add Item";
            return View("PriceLists/Items/Create", model);
        }

        var created = await salesApiClient.CreatePriceListItemAsync(accessToken, priceListId, MapPriceListItemRequest(model), ct);
        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to create price list item." : created.ErrorMessage);
            ViewData["Title"] = "Create Price List Item";
            ViewData["Breadcrumb"] = "Sales / Price Lists / Detail / Add Item";
            return View("PriceLists/Items/Create", model);
        }

        TempData["SuccessMessage"] = "Price list item created.";
        return RedirectToAction(nameof(PriceListDetail), new { id = priceListId });
    }

    [HttpGet("price-lists/{priceListId:int}/items/edit/{id:int}")]
    public async Task<IActionResult> EditPriceListItem(int priceListId, int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await salesApiClient.GetPriceListItemByIdAsync(accessToken, priceListId, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        var model = new SalesPriceListItemEditViewModel
        {
            Id = item.Id,
            PriceListId = priceListId,
            ItemId = item.ItemId,
            UomId = item.UomId,
            MinQty = item.MinQty,
            UnitPrice = item.UnitPrice,
            DiscountPct = item.DiscountPct
        };

        await PopulatePriceListItemOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit Price List Item";
        ViewData["Breadcrumb"] = "Sales / Price Lists / Detail / Edit Item";

        return View("PriceLists/Items/Edit", model);
    }

    [HttpPost("price-lists/{priceListId:int}/items/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPriceListItem(int priceListId, int id, SalesPriceListItemEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        model.PriceListId = priceListId;
        NormalizePriceListItemForm(model);
        await PopulatePriceListItemOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Price List Item";
            ViewData["Breadcrumb"] = "Sales / Price Lists / Detail / Edit Item";
            return View("PriceLists/Items/Edit", model);
        }

        var updated = await salesApiClient.UpdatePriceListItemAsync(accessToken, priceListId, id, MapPriceListItemRequest(model), ct);
        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update price list item." : updated.ErrorMessage);
            ViewData["Title"] = "Edit Price List Item";
            ViewData["Breadcrumb"] = "Sales / Price Lists / Detail / Edit Item";
            return View("PriceLists/Items/Edit", model);
        }

        TempData["SuccessMessage"] = "Price list item updated.";
        return RedirectToAction(nameof(PriceListDetail), new { id = priceListId });
    }

    [HttpPost("price-lists/{priceListId:int}/items/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePriceListItem(int priceListId, int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var deleted = await salesApiClient.DeletePriceListItemAsync(accessToken, priceListId, id, ct);
        TempData[deleted.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = deleted.IsSuccess
            ? "Price list item deleted."
            : (string.IsNullOrWhiteSpace(deleted.ErrorMessage) ? "Failed to delete price list item." : deleted.ErrorMessage);

        return RedirectToAction(nameof(PriceListDetail), new { id = priceListId });
    }
}
