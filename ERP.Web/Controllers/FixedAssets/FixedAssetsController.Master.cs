using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.FixedAssets;
using ERP.Domain.Enums.FixedAssets;
using ERP.Web.ViewModels.FixedAssets;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class FixedAssetsController
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var dashboard = await fixedAssetsApiClient.GetDashboardAsync(accessToken, ct) ?? new FixedAssetDashboardDto();

        ViewData["Title"] = "Fixed Assets Dashboard";
        ViewData["Breadcrumb"] = "Fixed Assets / Dashboard";

        return View("Index", new FixedAssetsDashboardViewModel
        {
            Data = dashboard
        });
    }

    [HttpGet("asset-categories")]
    public async Task<IActionResult> AssetCategories(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "code",
        string? sortDirection = "asc",
        string? code = null,
        string? name = null,
        DepreciationMethod? depreciationMethod = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "code", "code", "name", "depreciationmethod", "usefullifemonths", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);

        var items = await fixedAssetsApiClient.GetAssetCategoriesAsync(accessToken, new FixedAssetCategoryPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = NormalizeText(code),
            Name = NormalizeText(name),
            DepreciationMethod = depreciationMethod,
            IsActive = isActive
        }, ct);

        ViewData["Title"] = "Asset Categories";
        ViewData["Breadcrumb"] = "Fixed Assets / Asset Categories";

        return View("AssetCategories/Index", new FixedAssetCategoriesIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = NormalizeText(code),
            NameFilter = NormalizeText(name),
            DepreciationMethodFilter = depreciationMethod,
            IsActiveFilter = isActive,
            Items = items ?? PagedResult<FixedAssetCategoryDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("asset-categories/create")]
    public async Task<IActionResult> CreateAssetCategory(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new FixedAssetCategoryEditViewModel();
        await PopulateAssetCategoryFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create Asset Category";
        ViewData["Breadcrumb"] = "Fixed Assets / Asset Categories / Create";

        return View("AssetCategories/Create", model);
    }

    [HttpPost("asset-categories/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAssetCategory(FixedAssetCategoryEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        await PopulateAssetCategoryFormOptionsAsync(accessToken, model, ct);
        ValidateAssetCategoryForm(model, ModelState);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Asset Category";
            ViewData["Breadcrumb"] = "Fixed Assets / Asset Categories / Create";
            return View("AssetCategories/Create", model);
        }

        var created = await fixedAssetsApiClient.CreateAssetCategoryAsync(accessToken, MapAssetCategoryDto(model), ct);
        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create asset category.");
            ViewData["Title"] = "Create Asset Category";
            ViewData["Breadcrumb"] = "Fixed Assets / Asset Categories / Create";
            return View("AssetCategories/Create", model);
        }

        TempData["SuccessMessage"] = "Asset category created.";
        return RedirectToAction(nameof(AssetCategories));
    }

    [HttpGet("asset-categories/edit/{id:int}")]
    public async Task<IActionResult> EditAssetCategory(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await fixedAssetsApiClient.GetAssetCategoryByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        var model = new FixedAssetCategoryEditViewModel
        {
            Id = item.Id,
            Code = item.Code,
            Name = item.Name,
            DepreciationMethod = item.DepreciationMethod,
            UsefulLifeMonths = item.UsefulLifeMonths,
            DepreciationRate = item.DepreciationRate,
            AssetAccountId = item.AssetAccountId,
            AccumulatedDepreciationAccountId = item.AccumulatedDepreciationAccountId,
            DepreciationExpenseAccountId = item.DepreciationExpenseAccountId,
            IsActive = item.IsActive
        };

        await PopulateAssetCategoryFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit Asset Category";
        ViewData["Breadcrumb"] = "Fixed Assets / Asset Categories / Edit";

        return View("AssetCategories/Edit", model);
    }

    [HttpPost("asset-categories/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAssetCategory(int id, FixedAssetCategoryEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        await PopulateAssetCategoryFormOptionsAsync(accessToken, model, ct);
        ValidateAssetCategoryForm(model, ModelState);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Asset Category";
            ViewData["Breadcrumb"] = "Fixed Assets / Asset Categories / Edit";
            return View("AssetCategories/Edit", model);
        }

        var updated = await fixedAssetsApiClient.UpdateAssetCategoryAsync(accessToken, id, MapAssetCategoryDto(model), ct);
        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to update asset category.");
            ViewData["Title"] = "Edit Asset Category";
            ViewData["Breadcrumb"] = "Fixed Assets / Asset Categories / Edit";
            return View("AssetCategories/Edit", model);
        }

        TempData["SuccessMessage"] = "Asset category updated.";
        return RedirectToAction(nameof(AssetCategories));
    }

    [HttpPost("asset-categories/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAssetCategory(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var deleted = await fixedAssetsApiClient.DeleteAssetCategoryAsync(accessToken, id, ct);
        TempData[deleted ? "SuccessMessage" : "ErrorMessage"] = deleted
            ? "Asset category deleted."
            : "Failed to delete asset category.";

        return RedirectToAction(nameof(AssetCategories));
    }

    [HttpGet("locations")]
    public async Task<IActionResult> Locations(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "code",
        string? sortDirection = "asc",
        string? code = null,
        string? name = null,
        int? departmentId = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "code", "code", "name", "departmentname", "managername", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);

        var departmentOptionsTask = GetDepartmentOptionsAsync(accessToken, ct);
        var itemsTask = fixedAssetsApiClient.GetLocationsAsync(accessToken, new FixedAssetLocationPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = NormalizeText(code),
            Name = NormalizeText(name),
            DepartmentId = departmentId,
            IsActive = isActive
        }, ct);

        await Task.WhenAll(departmentOptionsTask, itemsTask);

        ViewData["Title"] = "Asset Locations";
        ViewData["Breadcrumb"] = "Fixed Assets / Asset Locations";

        return View("Locations/Index", new FixedAssetLocationsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = NormalizeText(code),
            NameFilter = NormalizeText(name),
            DepartmentIdFilter = departmentId,
            IsActiveFilter = isActive,
            DepartmentOptions = await departmentOptionsTask,
            Items = await itemsTask ?? PagedResult<FixedAssetLocationDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("locations/create")]
    public async Task<IActionResult> CreateLocation(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new FixedAssetLocationEditViewModel();
        await PopulateLocationFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create Asset Location";
        ViewData["Breadcrumb"] = "Fixed Assets / Asset Locations / Create";

        return View("Locations/Create", model);
    }

    [HttpPost("locations/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLocation(FixedAssetLocationEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        await PopulateLocationFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Asset Location";
            ViewData["Breadcrumb"] = "Fixed Assets / Asset Locations / Create";
            return View("Locations/Create", model);
        }

        var created = await fixedAssetsApiClient.CreateLocationAsync(accessToken, MapLocationDto(model), ct);
        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create asset location.");
            ViewData["Title"] = "Create Asset Location";
            ViewData["Breadcrumb"] = "Fixed Assets / Asset Locations / Create";
            return View("Locations/Create", model);
        }

        TempData["SuccessMessage"] = "Asset location created.";
        return RedirectToAction(nameof(Locations));
    }

    [HttpGet("locations/edit/{id:int}")]
    public async Task<IActionResult> EditLocation(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await fixedAssetsApiClient.GetLocationByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        var model = new FixedAssetLocationEditViewModel
        {
            Id = item.Id,
            Code = item.Code,
            Name = item.Name,
            Address = item.Address,
            DepartmentId = item.DepartmentId,
            ManagerId = item.ManagerId,
            IsActive = item.IsActive
        };

        await PopulateLocationFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit Asset Location";
        ViewData["Breadcrumb"] = "Fixed Assets / Asset Locations / Edit";

        return View("Locations/Edit", model);
    }

    [HttpPost("locations/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditLocation(int id, FixedAssetLocationEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        await PopulateLocationFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Asset Location";
            ViewData["Breadcrumb"] = "Fixed Assets / Asset Locations / Edit";
            return View("Locations/Edit", model);
        }

        var updated = await fixedAssetsApiClient.UpdateLocationAsync(accessToken, id, MapLocationDto(model), ct);
        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to update asset location.");
            ViewData["Title"] = "Edit Asset Location";
            ViewData["Breadcrumb"] = "Fixed Assets / Asset Locations / Edit";
            return View("Locations/Edit", model);
        }

        TempData["SuccessMessage"] = "Asset location updated.";
        return RedirectToAction(nameof(Locations));
    }

    [HttpPost("locations/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLocation(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var deleted = await fixedAssetsApiClient.DeleteLocationAsync(accessToken, id, ct);
        TempData[deleted ? "SuccessMessage" : "ErrorMessage"] = deleted
            ? "Asset location deleted."
            : "Failed to delete asset location.";

        return RedirectToAction(nameof(Locations));
    }

    [HttpGet("depreciation-configs")]
    public async Task<IActionResult> DepreciationConfigs(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "fiscalyear",
        string? sortDirection = "desc",
        short? fiscalYear = null,
        bool? isAutoPostJournal = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "fiscalyear", "name", "fiscalyear", "startdate", "enddate", "runday", "isautopostjournal", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);

        var items = await fixedAssetsApiClient.GetDepreciationConfigsAsync(accessToken, new FixedAssetDepreciationConfigPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            FiscalYear = fiscalYear,
            IsAutoPostJournal = isAutoPostJournal,
            IsActive = isActive
        }, ct);

        ViewData["Title"] = "Depreciation Configs";
        ViewData["Breadcrumb"] = "Fixed Assets / Depreciation Configs";

        return View("DepreciationConfigs/Index", new FixedAssetDepreciationConfigsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            FiscalYearFilter = fiscalYear,
            IsAutoPostJournalFilter = isAutoPostJournal,
            IsActiveFilter = isActive,
            Items = items ?? PagedResult<FixedAssetDepreciationConfigDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("depreciation-configs/create")]
    public IActionResult CreateDepreciationConfig()
    {
        var unauthorized = RequireAccessToken(out _, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        ViewData["Title"] = "Create Depreciation Config";
        ViewData["Breadcrumb"] = "Fixed Assets / Depreciation Configs / Create";

        return View("DepreciationConfigs/Create", new FixedAssetDepreciationConfigEditViewModel());
    }

    [HttpPost("depreciation-configs/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDepreciationConfig(FixedAssetDepreciationConfigEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        ValidateDepreciationConfigForm(model, ModelState);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Depreciation Config";
            ViewData["Breadcrumb"] = "Fixed Assets / Depreciation Configs / Create";
            return View("DepreciationConfigs/Create", model);
        }

        var created = await fixedAssetsApiClient.CreateDepreciationConfigAsync(accessToken, MapDepreciationConfigDto(model), ct);
        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create depreciation config.");
            ViewData["Title"] = "Create Depreciation Config";
            ViewData["Breadcrumb"] = "Fixed Assets / Depreciation Configs / Create";
            return View("DepreciationConfigs/Create", model);
        }

        TempData["SuccessMessage"] = "Depreciation config created.";
        return RedirectToAction(nameof(DepreciationConfigs));
    }

    [HttpGet("depreciation-configs/edit/{id:int}")]
    public async Task<IActionResult> EditDepreciationConfig(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await fixedAssetsApiClient.GetDepreciationConfigByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        var model = new FixedAssetDepreciationConfigEditViewModel
        {
            Id = item.Id,
            Name = item.Name,
            FiscalYear = item.FiscalYear,
            StartDate = item.StartDate,
            EndDate = item.EndDate,
            RunDay = item.RunDay,
            IsAutoPostJournal = item.IsAutoPostJournal,
            IsActive = item.IsActive
        };

        ViewData["Title"] = "Edit Depreciation Config";
        ViewData["Breadcrumb"] = "Fixed Assets / Depreciation Configs / Edit";

        return View("DepreciationConfigs/Edit", model);
    }

    [HttpPost("depreciation-configs/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDepreciationConfig(int id, FixedAssetDepreciationConfigEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        ValidateDepreciationConfigForm(model, ModelState);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Depreciation Config";
            ViewData["Breadcrumb"] = "Fixed Assets / Depreciation Configs / Edit";
            return View("DepreciationConfigs/Edit", model);
        }

        var updated = await fixedAssetsApiClient.UpdateDepreciationConfigAsync(accessToken, id, MapDepreciationConfigDto(model), ct);
        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to update depreciation config.");
            ViewData["Title"] = "Edit Depreciation Config";
            ViewData["Breadcrumb"] = "Fixed Assets / Depreciation Configs / Edit";
            return View("DepreciationConfigs/Edit", model);
        }

        TempData["SuccessMessage"] = "Depreciation config updated.";
        return RedirectToAction(nameof(DepreciationConfigs));
    }

    [HttpPost("depreciation-configs/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDepreciationConfig(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var deleted = await fixedAssetsApiClient.DeleteDepreciationConfigAsync(accessToken, id, ct);
        TempData[deleted ? "SuccessMessage" : "ErrorMessage"] = deleted
            ? "Depreciation config deleted."
            : "Failed to delete depreciation config.";

        return RedirectToAction(nameof(DepreciationConfigs));
    }
}
