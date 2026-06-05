using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;
using ERP.Web.ViewModels.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class InventoryController
{
    [HttpGet("brands")]
    public async Task<IActionResult> Brands(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "name",
        string? sortDirection = "asc",
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "name", "name", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);

        var items = await inventoryApiClient.GetBrandsAsync(accessToken, new BrandPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Name = NormalizeText(name),
            IsActive = isActive
        }, ct);

        ViewData["Title"] = "Brands";
        ViewData["Breadcrumb"] = "Inventory / Brands";

        return View("Brands/Index", new InventoryBrandsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            NameFilter = NormalizeText(name),
            IsActiveFilter = isActive,
            Items = items ?? PagedResult<BrandDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("brands/create")]
    public IActionResult CreateBrand()
    {
        var unauthorized = RequireAccessToken(out _, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        ViewData["Title"] = "Create Brand";
        ViewData["Breadcrumb"] = "Inventory / Brands / Create";
        return View("Brands/Create", new InventoryBrandEditViewModel());
    }

    [HttpPost("brands/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBrand(InventoryBrandEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Brand";
            ViewData["Breadcrumb"] = "Inventory / Brands / Create";
            return View("Brands/Create", model);
        }

        var createResult = await inventoryApiClient.CreateBrandAsync(accessToken, new BrandDto
        {
            Name = model.Name,
            Description = model.Description,
            IsActive = model.IsActive
        }, ct);

        if (!createResult.IsSuccess)
        {
            AddApiModelError(createResult, "Failed to create brand.");
            ViewData["Title"] = "Create Brand";
            ViewData["Breadcrumb"] = "Inventory / Brands / Create";
            return View("Brands/Create", model);
        }

        TempData["SuccessMessage"] = "Brand created.";
        return RedirectToAction(nameof(Brands));
    }

    [HttpGet("brands/edit/{id:int}")]
    public async Task<IActionResult> EditBrand(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await inventoryApiClient.GetBrandByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        var model = new InventoryBrandEditViewModel
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            IsActive = item.IsActive
        };

        ViewData["Title"] = "Edit Brand";
        ViewData["Breadcrumb"] = "Inventory / Brands / Edit";

        return View("Brands/Edit", model);
    }

    [HttpPost("brands/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditBrand(int id, InventoryBrandEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Brand";
            ViewData["Breadcrumb"] = "Inventory / Brands / Edit";
            return View("Brands/Edit", model);
        }

        var updateResult = await inventoryApiClient.UpdateBrandAsync(accessToken, id, new BrandDto
        {
            Id = id,
            Name = model.Name,
            Description = model.Description,
            IsActive = model.IsActive
        }, ct);

        if (!updateResult.IsSuccess)
        {
            AddApiModelError(updateResult, "Failed to update brand.");
            ViewData["Title"] = "Edit Brand";
            ViewData["Breadcrumb"] = "Inventory / Brands / Edit";
            return View("Brands/Edit", model);
        }

        TempData["SuccessMessage"] = "Brand updated.";
        return RedirectToAction(nameof(Brands));
    }

    [HttpPost("brands/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBrand(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var deleteResult = await inventoryApiClient.DeleteBrandAsync(accessToken, id, ct);
        TempData[deleteResult.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = deleteResult.IsSuccess
            ? "Brand deleted."
            : ResolveApiErrorMessage(deleteResult, "Failed to delete brand.");
        return RedirectToAction(nameof(Brands));
    }
}

