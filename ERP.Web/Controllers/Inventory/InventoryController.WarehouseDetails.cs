using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;
using ERP.Web.ViewModels.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class InventoryController
{
    [HttpGet("warehouses/{warehouseId:int}/locations")]
    public async Task<IActionResult> WarehouseLocations(
        int warehouseId,
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "code",
        string? sortDirection = "asc",
        string? code = null,
        string? name = null,
        bool? isDefault = null,
        bool? isActive = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var warehouse = await inventoryApiClient.GetWarehouseByIdAsync(accessToken, warehouseId, ct);
        if (warehouse is null)
        {
            return NotFound();
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "code", "code", "name", "isdefault", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);

        var items = await inventoryApiClient.GetWarehouseLocationsAsync(accessToken, warehouseId, new WarehouseLocationPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = NormalizeText(code),
            Name = NormalizeText(name),
            IsDefault = isDefault,
            IsActive = isActive
        }, ct);

        ViewData["Title"] = "Warehouse Locations";
        ViewData["Breadcrumb"] = $"Inventory / Warehouses / {warehouse.Code} / Detail / Locations";

        return View("Warehouses/Locations", new InventoryWarehouseLocationsIndexViewModel
        {
            WarehouseId = warehouseId,
            WarehouseCode = warehouse.Code,
            WarehouseName = warehouse.Name,
            WarehouseDescription = warehouse.Description,
            WarehouseAddress = warehouse.Address,
            WarehousePhone = warehouse.Phone,
            WarehouseManagerName = warehouse.ManagerName,
            WarehouseCostCenterCode = warehouse.CostCenterCode,
            WarehouseIsTransit = warehouse.IsTransit,
            WarehouseIsActive = warehouse.IsActive,
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = NormalizeText(code),
            NameFilter = NormalizeText(name),
            IsDefaultFilter = isDefault,
            IsActiveFilter = isActive,
            Items = items ?? PagedResult<WarehouseLocationDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("warehouses/{warehouseId:int}/locations/create")]
    public IActionResult CreateWarehouseLocation(int warehouseId)
    {
        var unauthorized = RequireAccessToken(out _, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        ViewData["Title"] = "Create Warehouse Location";
        ViewData["Breadcrumb"] = "Inventory / Warehouse Locations / Create";

        return View("Warehouses/CreateLocation", new InventoryWarehouseLocationEditViewModel
        {
            WarehouseId = warehouseId,
            IsActive = true
        });
    }

    [HttpPost("warehouses/{warehouseId:int}/locations/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateWarehouseLocation(int warehouseId, InventoryWarehouseLocationEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.WarehouseId = warehouseId;

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Warehouse Location";
            ViewData["Breadcrumb"] = "Inventory / Warehouse Locations / Create";
            return View("Warehouses/CreateLocation", model);
        }

        var createResult = await inventoryApiClient.CreateWarehouseLocationAsync(accessToken, warehouseId, new WarehouseLocationDto
        {
            WarehouseId = warehouseId,
            Code = model.Code,
            Name = model.Name,
            Description = model.Description,
            IsDefault = model.IsDefault,
            IsActive = model.IsActive
        }, ct);

        if (!createResult.IsSuccess)
        {
            AddApiModelError(createResult, "Failed to create warehouse location.");
            ViewData["Title"] = "Create Warehouse Location";
            ViewData["Breadcrumb"] = "Inventory / Warehouse Locations / Create";
            return View("Warehouses/CreateLocation", model);
        }

        TempData["SuccessMessage"] = "Warehouse location created.";
        return RedirectToAction(nameof(WarehouseLocations), new { warehouseId });
    }

    [HttpGet("warehouses/{warehouseId:int}/locations/edit/{id:int}")]
    public async Task<IActionResult> EditWarehouseLocation(int warehouseId, int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await inventoryApiClient.GetWarehouseLocationByIdAsync(accessToken, warehouseId, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        var model = new InventoryWarehouseLocationEditViewModel
        {
            WarehouseId = warehouseId,
            Id = id,
            Code = item.Code,
            Name = item.Name,
            Description = item.Description,
            IsDefault = item.IsDefault,
            IsActive = item.IsActive
        };

        ViewData["Title"] = "Edit Warehouse Location";
        ViewData["Breadcrumb"] = "Inventory / Warehouse Locations / Edit";

        return View("Warehouses/EditLocation", model);
    }

    [HttpPost("warehouses/{warehouseId:int}/locations/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditWarehouseLocation(int warehouseId, int id, InventoryWarehouseLocationEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.WarehouseId = warehouseId;
        model.Id = id;

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Warehouse Location";
            ViewData["Breadcrumb"] = "Inventory / Warehouse Locations / Edit";
            return View("Warehouses/EditLocation", model);
        }

        var updateResult = await inventoryApiClient.UpdateWarehouseLocationAsync(accessToken, warehouseId, id, new WarehouseLocationDto
        {
            Id = id,
            WarehouseId = warehouseId,
            Code = model.Code,
            Name = model.Name,
            Description = model.Description,
            IsDefault = model.IsDefault,
            IsActive = model.IsActive
        }, ct);

        if (!updateResult.IsSuccess)
        {
            AddApiModelError(updateResult, "Failed to update warehouse location.");
            ViewData["Title"] = "Edit Warehouse Location";
            ViewData["Breadcrumb"] = "Inventory / Warehouse Locations / Edit";
            return View("Warehouses/EditLocation", model);
        }

        TempData["SuccessMessage"] = "Warehouse location updated.";
        return RedirectToAction(nameof(WarehouseLocations), new { warehouseId });
    }

    [HttpPost("warehouses/{warehouseId:int}/locations/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteWarehouseLocation(int warehouseId, int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var deleteResult = await inventoryApiClient.DeleteWarehouseLocationAsync(accessToken, warehouseId, id, ct);
        TempData[deleteResult.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = deleteResult.IsSuccess
            ? "Warehouse location deleted."
            : ResolveApiErrorMessage(deleteResult, "Failed to delete warehouse location.");
        return RedirectToAction(nameof(WarehouseLocations), new { warehouseId });
    }

    [HttpGet("warehouses/{warehouseId:int}/stock")]
    public async Task<IActionResult> WarehouseStock(
        int warehouseId,
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "itemcode",
        string? sortDirection = "asc",
        int? itemId = null,
        int? locationId = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var warehouse = await inventoryApiClient.GetWarehouseByIdAsync(accessToken, warehouseId, ct);
        if (warehouse is null)
        {
            return NotFound();
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "itemcode", "itemcode", "locationcode", "qtyonhand", "qtyreserved", "qtyavailable", "avgcost", "totalvalue");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);

        var stockTask = inventoryApiClient.GetWarehouseStockAsync(accessToken, warehouseId, new StockBalancePagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            ItemId = itemId,
            LocationId = locationId
        }, ct);

        var itemOptionsTask = inventoryApiClient.GetItemOptionsAsync(accessToken, ct);
        var locationOptionsTask = inventoryApiClient.GetWarehouseLocationOptionsAsync(accessToken, warehouseId, ct);

        await Task.WhenAll(stockTask, itemOptionsTask, locationOptionsTask);

        ViewData["Title"] = "Warehouse Stock";
        ViewData["Breadcrumb"] = $"Inventory / Warehouses / {warehouse.Code} / Detail / Stock";

        return View("Warehouses/Stock", new InventoryWarehouseStockIndexViewModel
        {
            WarehouseId = warehouseId,
            WarehouseCode = warehouse.Code,
            WarehouseName = warehouse.Name,
            WarehouseDescription = warehouse.Description,
            WarehouseAddress = warehouse.Address,
            WarehousePhone = warehouse.Phone,
            WarehouseManagerName = warehouse.ManagerName,
            WarehouseCostCenterCode = warehouse.CostCenterCode,
            WarehouseIsTransit = warehouse.IsTransit,
            WarehouseIsActive = warehouse.IsActive,
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            ItemIdFilter = itemId,
            LocationIdFilter = locationId,
            ItemOptions = await itemOptionsTask,
            LocationOptions = await locationOptionsTask,
            Items = await stockTask ?? PagedResult<StockBalanceDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }
}


