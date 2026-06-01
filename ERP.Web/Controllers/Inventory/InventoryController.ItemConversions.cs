using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;
using ERP.Web.ViewModels.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class InventoryController
{
    [HttpGet("item-conversions")]
    public async Task<IActionResult> ItemConversions(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "itemcode",
        string? sortDirection = "asc",
        int? itemId = null,
        int? fromUomId = null,
        int? toUomId = null,
        bool? isActive = null,
        decimal? factorFrom = null,
        decimal? factorTo = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "itemcode", "itemcode", "fromuomcode", "touomcode", "conversionfactor", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedFactorFrom, normalizedFactorTo) = NormalizeDecimalRange(factorFrom, factorTo);

        var itemsTask = inventoryApiClient.GetItemConversionsAsync(accessToken, new ItemUnitConversionPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            ItemId = itemId,
            FromUomId = fromUomId,
            ToUomId = toUomId,
            IsActive = isActive,
            FactorFrom = normalizedFactorFrom,
            FactorTo = normalizedFactorTo
        }, ct);

        var itemOptionsTask = inventoryApiClient.GetItemOptionsAsync(accessToken, ct);
        var uomOptionsTask = inventoryApiClient.GetUnitOptionsAsync(accessToken, ct);

        await Task.WhenAll(itemsTask, itemOptionsTask, uomOptionsTask);

        ViewData["Title"] = "Item Conversions";
        ViewData["Breadcrumb"] = "Inventory / Item Conversions";

        return View("ItemConversions/Index", new InventoryItemConversionsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            ItemIdFilter = itemId,
            FromUomIdFilter = fromUomId,
            ToUomIdFilter = toUomId,
            IsActiveFilter = isActive,
            FactorFromFilter = normalizedFactorFrom,
            FactorToFilter = normalizedFactorTo,
            ItemOptions = await itemOptionsTask,
            UomOptions = await uomOptionsTask,
            Items = await itemsTask ?? PagedResult<ItemUnitConversionDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("item-conversions/create")]
    public async Task<IActionResult> CreateItemConversion(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new InventoryItemConversionEditViewModel();
        await PopulateItemConversionOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create Item Conversion";
        ViewData["Breadcrumb"] = "Inventory / Item Conversions / Create";

        return View("ItemConversions/Create", model);
    }

    [HttpPost("item-conversions/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateItemConversion(InventoryItemConversionEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        await PopulateItemConversionOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Item Conversion";
            ViewData["Breadcrumb"] = "Inventory / Item Conversions / Create";
            return View("ItemConversions/Create", model);
        }

        var created = await inventoryApiClient.CreateItemConversionAsync(accessToken, new ItemUnitConversionDto
        {
            ItemId = model.ItemId,
            FromUomId = model.FromUomId,
            ToUomId = model.ToUomId,
            ConversionFactor = model.ConversionFactor,
            IsActive = model.IsActive
        }, ct);

        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create item conversion.");
            ViewData["Title"] = "Create Item Conversion";
            ViewData["Breadcrumb"] = "Inventory / Item Conversions / Create";
            return View("ItemConversions/Create", model);
        }

        TempData["SuccessMessage"] = "Item conversion created.";
        return RedirectToAction(nameof(ItemConversions));
    }

    [HttpGet("item-conversions/edit/{id:int}")]
    public async Task<IActionResult> EditItemConversion(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await inventoryApiClient.GetItemConversionByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        var model = new InventoryItemConversionEditViewModel
        {
            Id = item.Id,
            ItemId = item.ItemId,
            FromUomId = item.FromUomId,
            ToUomId = item.ToUomId,
            ConversionFactor = item.ConversionFactor,
            IsActive = item.IsActive
        };

        await PopulateItemConversionOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit Item Conversion";
        ViewData["Breadcrumb"] = "Inventory / Item Conversions / Edit";

        return View("ItemConversions/Edit", model);
    }

    [HttpPost("item-conversions/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditItemConversion(int id, InventoryItemConversionEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        await PopulateItemConversionOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Item Conversion";
            ViewData["Breadcrumb"] = "Inventory / Item Conversions / Edit";
            return View("ItemConversions/Edit", model);
        }

        var updated = await inventoryApiClient.UpdateItemConversionAsync(accessToken, id, new ItemUnitConversionDto
        {
            Id = id,
            ItemId = model.ItemId,
            FromUomId = model.FromUomId,
            ToUomId = model.ToUomId,
            ConversionFactor = model.ConversionFactor,
            IsActive = model.IsActive
        }, ct);

        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to update item conversion.");
            ViewData["Title"] = "Edit Item Conversion";
            ViewData["Breadcrumb"] = "Inventory / Item Conversions / Edit";
            return View("ItemConversions/Edit", model);
        }

        TempData["SuccessMessage"] = "Item conversion updated.";
        return RedirectToAction(nameof(ItemConversions));
    }

    [HttpPost("item-conversions/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteItemConversion(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var deleted = await inventoryApiClient.DeleteItemConversionAsync(accessToken, id, ct);
        TempData[deleted ? "SuccessMessage" : "ErrorMessage"] = deleted ? "Item conversion deleted." : "Failed to delete item conversion.";
        return RedirectToAction(nameof(ItemConversions));
    }
}
