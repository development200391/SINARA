using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;
using ERP.Web.ViewModels.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class InventoryController
{
    [HttpGet("units")]
    public async Task<IActionResult> Units(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "code",
        string? sortDirection = "asc",
        string? code = null,
        string? name = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "code", "code", "name", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);

        var items = await inventoryApiClient.GetUnitsAsync(accessToken, new UnitOfMeasurePagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = NormalizeText(code),
            Name = NormalizeText(name),
            IsActive = isActive
        }, ct);

        ViewData["Title"] = "Units of Measure";
        ViewData["Breadcrumb"] = "Inventory / Units of Measure";

        return View("Units/Index", new InventoryUnitsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = NormalizeText(code),
            NameFilter = NormalizeText(name),
            IsActiveFilter = isActive,
            Items = items ?? PagedResult<UnitOfMeasureDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("units/create")]
    public IActionResult CreateUnit()
    {
        var unauthorized = RequireAccessToken(out _, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        ViewData["Title"] = "Create Unit";
        ViewData["Breadcrumb"] = "Inventory / Units / Create";
        return View("Units/Create", new InventoryUnitEditViewModel());
    }

    [HttpPost("units/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUnit(InventoryUnitEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Unit";
            ViewData["Breadcrumb"] = "Inventory / Units / Create";
            return View("Units/Create", model);
        }

        var createResult = await inventoryApiClient.CreateUnitAsync(accessToken, new UnitOfMeasureDto
        {
            Code = model.Code,
            Name = model.Name,
            Description = model.Description,
            IsActive = model.IsActive
        }, ct);

        if (!createResult.IsSuccess)
        {
            AddApiModelError(createResult, "Failed to create unit.");
            ViewData["Title"] = "Create Unit";
            ViewData["Breadcrumb"] = "Inventory / Units / Create";
            return View("Units/Create", model);
        }

        TempData["SuccessMessage"] = "Unit created.";
        return RedirectToAction(nameof(Units));
    }

    [HttpGet("units/edit/{id:int}")]
    public async Task<IActionResult> EditUnit(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await inventoryApiClient.GetUnitByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        var model = new InventoryUnitEditViewModel
        {
            Id = item.Id,
            Code = item.Code,
            Name = item.Name,
            Description = item.Description,
            IsActive = item.IsActive
        };

        ViewData["Title"] = "Edit Unit";
        ViewData["Breadcrumb"] = "Inventory / Units / Edit";
        return View("Units/Edit", model);
    }

    [HttpPost("units/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUnit(int id, InventoryUnitEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Unit";
            ViewData["Breadcrumb"] = "Inventory / Units / Edit";
            return View("Units/Edit", model);
        }

        var updateResult = await inventoryApiClient.UpdateUnitAsync(accessToken, id, new UnitOfMeasureDto
        {
            Id = id,
            Code = model.Code,
            Name = model.Name,
            Description = model.Description,
            IsActive = model.IsActive
        }, ct);

        if (!updateResult.IsSuccess)
        {
            AddApiModelError(updateResult, "Failed to update unit.");
            ViewData["Title"] = "Edit Unit";
            ViewData["Breadcrumb"] = "Inventory / Units / Edit";
            return View("Units/Edit", model);
        }

        TempData["SuccessMessage"] = "Unit updated.";
        return RedirectToAction(nameof(Units));
    }

    [HttpPost("units/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUnit(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var deleteResult = await inventoryApiClient.DeleteUnitAsync(accessToken, id, ct);
        TempData[deleteResult.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = deleteResult.IsSuccess
            ? "Unit deleted."
            : ResolveApiErrorMessage(deleteResult, "Failed to delete unit.");
        return RedirectToAction(nameof(Units));
    }
}

