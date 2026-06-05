using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;
using ERP.Domain.Enums.Inventory;
using ERP.Web.ViewModels.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class InventoryController
{
    [HttpGet("items")]
    public async Task<IActionResult> Items(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "itemcode",
        string? sortDirection = "asc",
        string? itemCode = null,
        string? sku = null,
        string? name = null,
        int? categoryId = null,
        int? brandId = null,
        ItemType? type = null,
        ItemStatus? status = null,
        bool? isActive = null,
        decimal? minStockFrom = null,
        decimal? minStockTo = null,
        decimal? reorderPointFrom = null,
        decimal? reorderPointTo = null,
        bool lowStockOnly = false,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "itemcode", "itemcode", "sku", "name", "categoryname", "status", "type", "minstock", "reorderpoint", "qtyavailable", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedMinStockFrom, normalizedMinStockTo) = NormalizeDecimalRange(minStockFrom, minStockTo);
        var (normalizedReorderFrom, normalizedReorderTo) = NormalizeDecimalRange(reorderPointFrom, reorderPointTo);

        var request = new ItemPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            ItemCode = NormalizeText(itemCode),
            Sku = NormalizeText(sku),
            Name = NormalizeText(name),
            CategoryId = categoryId,
            BrandId = brandId,
            Type = type,
            Status = status,
            IsActive = isActive,
            MinStockFrom = normalizedMinStockFrom,
            MinStockTo = normalizedMinStockTo,
            ReorderPointFrom = normalizedReorderFrom,
            ReorderPointTo = normalizedReorderTo
        };

        var itemsTask = lowStockOnly
            ? inventoryApiClient.GetLowStockItemsAsync(accessToken, request, ct)
            : inventoryApiClient.GetItemsAsync(accessToken, request, ct);

        var categoriesTask = inventoryApiClient.GetCategoryOptionsAsync(accessToken, ct);
        var brandsTask = inventoryApiClient.GetBrandOptionsAsync(accessToken, ct);

        await Task.WhenAll(itemsTask, categoriesTask, brandsTask);

        ViewData["Title"] = lowStockOnly ? "Low Stock Items" : "Items";
        ViewData["Breadcrumb"] = lowStockOnly ? "Inventory / Items / Low Stock" : "Inventory / Items";

        return View("Items/Index", new InventoryItemsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            ItemCodeFilter = NormalizeText(itemCode),
            SkuFilter = NormalizeText(sku),
            NameFilter = NormalizeText(name),
            CategoryIdFilter = categoryId,
            BrandIdFilter = brandId,
            TypeFilter = type,
            StatusFilter = status,
            IsActiveFilter = isActive,
            MinStockFromFilter = normalizedMinStockFrom,
            MinStockToFilter = normalizedMinStockTo,
            ReorderPointFromFilter = normalizedReorderFrom,
            ReorderPointToFilter = normalizedReorderTo,
            LowStockOnly = lowStockOnly,
            Categories = await categoriesTask,
            Brands = await brandsTask,
            Items = await itemsTask ?? PagedResult<ItemDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("items/create")]
    public async Task<IActionResult> CreateItem(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new InventoryItemEditViewModel();
        await PopulateItemFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create Item";
        ViewData["Breadcrumb"] = "Inventory / Items / Create";

        return View("Items/Create", model);
    }

    [HttpPost("items/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateItem(InventoryItemEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        await PopulateItemFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Item";
            ViewData["Breadcrumb"] = "Inventory / Items / Create";
            return View("Items/Create", model);
        }

        var createResult = await inventoryApiClient.CreateItemAsync(accessToken, MapItemDto(model), ct);
        if (!createResult.IsSuccess)
        {
            AddApiModelError(createResult, "Failed to create item.");
            ViewData["Title"] = "Create Item";
            ViewData["Breadcrumb"] = "Inventory / Items / Create";
            return View("Items/Create", model);
        }

        TempData["SuccessMessage"] = "Item created.";
        return RedirectToAction(nameof(Items));
    }

    [HttpGet("items/edit/{id:int}")]
    public async Task<IActionResult> EditItem(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await inventoryApiClient.GetItemByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        var model = new InventoryItemEditViewModel
        {
            Id = item.Id,
            ItemCode = item.ItemCode,
            Sku = item.Sku,
            Name = item.Name,
            Description = item.Description,
            CategoryId = item.CategoryId,
            BrandId = item.BrandId,
            Type = item.Type,
            BaseUomId = item.BaseUomId,
            PurchaseUomId = item.PurchaseUomId,
            Status = item.Status,
            ValuationMethod = item.ValuationMethod,
            LastPurchasePrice = item.LastPurchasePrice,
            AvgCost = item.AvgCost,
            MinStock = item.MinStock,
            MaxStock = item.MaxStock,
            ReorderPoint = item.ReorderPoint,
            LeadTimeDays = item.LeadTimeDays,
            InventoryAccountId = item.InventoryAccountId,
            CogsAccountId = item.CogsAccountId,
            AdjustmentAccountId = item.AdjustmentAccountId,
            Notes = item.Notes,
            IsActive = item.IsActive
        };

        await PopulateItemFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit Item";
        ViewData["Breadcrumb"] = "Inventory / Items / Edit";

        return View("Items/Edit", model);
    }

    [HttpPost("items/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditItem(int id, InventoryItemEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        await PopulateItemFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Item";
            ViewData["Breadcrumb"] = "Inventory / Items / Edit";
            return View("Items/Edit", model);
        }

        var updateResult = await inventoryApiClient.UpdateItemAsync(accessToken, id, MapItemDto(model), ct);
        if (!updateResult.IsSuccess)
        {
            AddApiModelError(updateResult, "Failed to update item.");
            ViewData["Title"] = "Edit Item";
            ViewData["Breadcrumb"] = "Inventory / Items / Edit";
            return View("Items/Edit", model);
        }

        TempData["SuccessMessage"] = "Item updated.";
        return RedirectToAction(nameof(Items));
    }

    [HttpPost("items/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteItem(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var deleteResult = await inventoryApiClient.DeleteItemAsync(accessToken, id, ct);
        TempData[deleteResult.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = deleteResult.IsSuccess
            ? "Item deleted."
            : ResolveApiErrorMessage(deleteResult, "Failed to delete item.");
        return RedirectToAction(nameof(Items));
    }
}

