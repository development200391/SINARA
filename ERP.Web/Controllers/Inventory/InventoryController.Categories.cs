using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;
using ERP.Web.ViewModels.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class InventoryController
{
    [HttpGet("categories")]
    public async Task<IActionResult> Categories(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "code",
        string? sortDirection = "asc",
        string? code = null,
        string? name = null,
        int? parentCategoryId = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "code", "code", "name", "parentcategoryname", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);

        var categoriesTask = inventoryApiClient.GetCategoriesAsync(accessToken, new ItemCategoryPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = NormalizeText(code),
            Name = NormalizeText(name),
            ParentCategoryId = parentCategoryId,
            IsActive = isActive
        }, ct);

        var parentOptionsTask = inventoryApiClient.GetCategoryOptionsAsync(accessToken, ct);
        await Task.WhenAll(categoriesTask, parentOptionsTask);

        ViewData["Title"] = "Item Categories";
        ViewData["Breadcrumb"] = "Inventory / Item Categories";

        return View("Categories/Index", new InventoryCategoriesIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = NormalizeText(code),
            NameFilter = NormalizeText(name),
            ParentCategoryIdFilter = parentCategoryId,
            IsActiveFilter = isActive,
            ParentCategories = await parentOptionsTask,
            Items = await categoriesTask ?? PagedResult<ItemCategoryDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("categories/create")]
    public async Task<IActionResult> CreateCategory(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new InventoryCategoryEditViewModel
        {
            ParentCategories = await inventoryApiClient.GetCategoryOptionsAsync(accessToken, ct)
        };

        ViewData["Title"] = "Create Category";
        ViewData["Breadcrumb"] = "Inventory / Item Categories / Create";

        return View("Categories/Create", model);
    }

    [HttpPost("categories/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(InventoryCategoryEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.ParentCategories = await inventoryApiClient.GetCategoryOptionsAsync(accessToken, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Category";
            ViewData["Breadcrumb"] = "Inventory / Item Categories / Create";
            return View("Categories/Create", model);
        }

        var createResult = await inventoryApiClient.CreateCategoryAsync(accessToken, new ItemCategoryDto
        {
            Code = model.Code,
            Name = model.Name,
            ParentCategoryId = model.ParentCategoryId,
            Description = model.Description,
            IsActive = model.IsActive
        }, ct);

        if (!createResult.IsSuccess)
        {
            AddApiModelError(createResult, "Failed to create category.");
            ViewData["Title"] = "Create Category";
            ViewData["Breadcrumb"] = "Inventory / Item Categories / Create";
            return View("Categories/Create", model);
        }

        TempData["SuccessMessage"] = "Category created.";
        return RedirectToAction(nameof(Categories));
    }

    [HttpGet("categories/edit/{id:int}")]
    public async Task<IActionResult> EditCategory(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var category = await inventoryApiClient.GetCategoryByIdAsync(accessToken, id, ct);
        if (category is null)
        {
            return NotFound();
        }

        var parentOptions = (await inventoryApiClient.GetCategoryOptionsAsync(accessToken, ct))
            .Where(x => x.Id != id)
            .ToList();

        var model = new InventoryCategoryEditViewModel
        {
            Id = category.Id,
            Code = category.Code,
            Name = category.Name,
            ParentCategoryId = category.ParentCategoryId,
            Description = category.Description,
            IsActive = category.IsActive,
            ParentCategories = parentOptions
        };

        ViewData["Title"] = "Edit Category";
        ViewData["Breadcrumb"] = "Inventory / Item Categories / Edit";

        return View("Categories/Edit", model);
    }

    [HttpPost("categories/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(int id, InventoryCategoryEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        model.ParentCategories = (await inventoryApiClient.GetCategoryOptionsAsync(accessToken, ct))
            .Where(x => x.Id != id)
            .ToList();

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Category";
            ViewData["Breadcrumb"] = "Inventory / Item Categories / Edit";
            return View("Categories/Edit", model);
        }

        var updateResult = await inventoryApiClient.UpdateCategoryAsync(accessToken, id, new ItemCategoryDto
        {
            Id = id,
            Code = model.Code,
            Name = model.Name,
            ParentCategoryId = model.ParentCategoryId,
            Description = model.Description,
            IsActive = model.IsActive
        }, ct);

        if (!updateResult.IsSuccess)
        {
            AddApiModelError(updateResult, "Failed to update category.");
            ViewData["Title"] = "Edit Category";
            ViewData["Breadcrumb"] = "Inventory / Item Categories / Edit";
            return View("Categories/Edit", model);
        }

        TempData["SuccessMessage"] = "Category updated.";
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost("categories/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var deleteResult = await inventoryApiClient.DeleteCategoryAsync(accessToken, id, ct);
        TempData[deleteResult.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = deleteResult.IsSuccess
            ? "Category deleted."
            : ResolveApiErrorMessage(deleteResult, "Failed to delete category.");
        return RedirectToAction(nameof(Categories));
    }
}

