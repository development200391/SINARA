using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;
using ERP.Web.ViewModels.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class InventoryController
{
    [HttpGet("warehouses")]
    public async Task<IActionResult> Warehouses(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "code",
        string? sortDirection = "asc",
        string? code = null,
        string? name = null,
        int? managerId = null,
        int? costCenterId = null,
        bool? isTransit = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "code", "code", "name", "managername", "costcentercode", "istransit", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);

        var itemsTask = inventoryApiClient.GetWarehousesAsync(accessToken, new WarehousePagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = NormalizeText(code),
            Name = NormalizeText(name),
            ManagerId = managerId,
            CostCenterId = costCenterId,
            IsTransit = isTransit,
            IsActive = isActive
        }, ct);

        var managerOptionsTask = GetManagerOptionsAsync(accessToken, ct);
        var costCenterOptionsTask = GetCostCenterOptionsAsync(accessToken, ct);

        await Task.WhenAll(itemsTask, managerOptionsTask, costCenterOptionsTask);

        ViewData["Title"] = "Warehouses";
        ViewData["Breadcrumb"] = "Inventory / Warehouses";

        return View("Warehouses/Index", new InventoryWarehousesIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = NormalizeText(code),
            NameFilter = NormalizeText(name),
            ManagerIdFilter = managerId,
            CostCenterIdFilter = costCenterId,
            IsTransitFilter = isTransit,
            IsActiveFilter = isActive,
            ManagerOptions = await managerOptionsTask,
            CostCenterOptions = await costCenterOptionsTask,
            Items = await itemsTask ?? PagedResult<WarehouseDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("warehouses/create")]
    public async Task<IActionResult> CreateWarehouse(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new InventoryWarehouseEditViewModel();
        await PopulateWarehouseFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create Warehouse";
        ViewData["Breadcrumb"] = "Inventory / Warehouses / Create";

        return View("Warehouses/Create", model);
    }

    [HttpPost("warehouses/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateWarehouse(InventoryWarehouseEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        await PopulateWarehouseFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Warehouse";
            ViewData["Breadcrumb"] = "Inventory / Warehouses / Create";
            return View("Warehouses/Create", model);
        }

        var created = await inventoryApiClient.CreateWarehouseAsync(accessToken, new WarehouseDto
        {
            Code = model.Code,
            Name = model.Name,
            Description = model.Description,
            Address = model.Address,
            Phone = model.Phone,
            ManagerId = model.ManagerId,
            CostCenterId = model.CostCenterId,
            IsTransit = model.IsTransit,
            IsActive = model.IsActive
        }, ct);

        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create warehouse.");
            ViewData["Title"] = "Create Warehouse";
            ViewData["Breadcrumb"] = "Inventory / Warehouses / Create";
            return View("Warehouses/Create", model);
        }

        TempData["SuccessMessage"] = "Warehouse created.";
        return RedirectToAction(nameof(Warehouses));
    }

    [HttpGet("warehouses/edit/{id:int}")]
    public async Task<IActionResult> EditWarehouse(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await inventoryApiClient.GetWarehouseByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        var model = new InventoryWarehouseEditViewModel
        {
            Id = item.Id,
            Code = item.Code,
            Name = item.Name,
            Description = item.Description,
            Address = item.Address,
            Phone = item.Phone,
            ManagerId = item.ManagerId,
            CostCenterId = item.CostCenterId,
            IsTransit = item.IsTransit,
            IsActive = item.IsActive
        };

        await PopulateWarehouseFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit Warehouse";
        ViewData["Breadcrumb"] = "Inventory / Warehouses / Edit";

        return View("Warehouses/Edit", model);
    }

    [HttpPost("warehouses/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditWarehouse(int id, InventoryWarehouseEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        await PopulateWarehouseFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Warehouse";
            ViewData["Breadcrumb"] = "Inventory / Warehouses / Edit";
            return View("Warehouses/Edit", model);
        }

        var updated = await inventoryApiClient.UpdateWarehouseAsync(accessToken, id, new WarehouseDto
        {
            Id = id,
            Code = model.Code,
            Name = model.Name,
            Description = model.Description,
            Address = model.Address,
            Phone = model.Phone,
            ManagerId = model.ManagerId,
            CostCenterId = model.CostCenterId,
            IsTransit = model.IsTransit,
            IsActive = model.IsActive
        }, ct);

        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to update warehouse.");
            ViewData["Title"] = "Edit Warehouse";
            ViewData["Breadcrumb"] = "Inventory / Warehouses / Edit";
            return View("Warehouses/Edit", model);
        }

        TempData["SuccessMessage"] = "Warehouse updated.";
        return RedirectToAction(nameof(Warehouses));
    }

    [HttpPost("warehouses/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteWarehouse(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var deleted = await inventoryApiClient.DeleteWarehouseAsync(accessToken, id, ct);
        TempData[deleted ? "SuccessMessage" : "ErrorMessage"] = deleted ? "Warehouse deleted." : "Failed to delete warehouse.";
        return RedirectToAction(nameof(Warehouses));
    }
}
