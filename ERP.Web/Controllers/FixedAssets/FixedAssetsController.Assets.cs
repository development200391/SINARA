using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.FixedAssets;
using ERP.Domain.Enums.FixedAssets;
using ERP.Web.ViewModels.FixedAssets;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class FixedAssetsController
{
    [HttpGet("assets")]
    public async Task<IActionResult> Assets(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "assetcode",
        string? sortDirection = "asc",
        string? assetCode = null,
        string? name = null,
        int? categoryId = null,
        int? locationId = null,
        int? departmentId = null,
        AssetStatus? status = null,
        decimal? bookValueFrom = null,
        decimal? bookValueTo = null,
        DateOnly? acquisitionDateFrom = null,
        DateOnly? acquisitionDateTo = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy, "assetcode", "assetcode", "name", "categoryname", "locationname", "acquisitiondate", "acquisitioncost", "bookvalue", "status", "isactive");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedBookFrom, normalizedBookTo) = NormalizeDecimalRange(bookValueFrom, bookValueTo);
        var (normalizedDateFrom, normalizedDateTo) = NormalizeDateRange(acquisitionDateFrom, acquisitionDateTo);

        var categoryOptionsTask = fixedAssetsApiClient.GetAssetCategoryOptionsAsync(accessToken, ct);
        var locationOptionsTask = fixedAssetsApiClient.GetLocationOptionsAsync(accessToken, ct);
        var departmentOptionsTask = GetDepartmentOptionsAsync(accessToken, ct);
        var itemsTask = fixedAssetsApiClient.GetAssetsAsync(accessToken, new FixedAssetPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            AssetCode = NormalizeText(assetCode),
            Name = NormalizeText(name),
            CategoryId = categoryId,
            LocationId = locationId,
            DepartmentId = departmentId,
            Status = status,
            BookValueFrom = normalizedBookFrom,
            BookValueTo = normalizedBookTo,
            AcquisitionDateFrom = normalizedDateFrom,
            AcquisitionDateTo = normalizedDateTo,
            IsActive = isActive
        }, ct);

        await Task.WhenAll(categoryOptionsTask, locationOptionsTask, departmentOptionsTask, itemsTask);

        ViewData["Title"] = "Assets";
        ViewData["Breadcrumb"] = "Fixed Assets / Assets";

        return View("Assets/Index", new FixedAssetsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            AssetCodeFilter = NormalizeText(assetCode),
            NameFilter = NormalizeText(name),
            CategoryIdFilter = categoryId,
            LocationIdFilter = locationId,
            DepartmentIdFilter = departmentId,
            StatusFilter = status,
            BookValueFromFilter = normalizedBookFrom,
            BookValueToFilter = normalizedBookTo,
            AcquisitionDateFromFilter = normalizedDateFrom,
            AcquisitionDateToFilter = normalizedDateTo,
            IsActiveFilter = isActive,
            CategoryOptions = await categoryOptionsTask,
            LocationOptions = await locationOptionsTask,
            DepartmentOptions = await departmentOptionsTask,
            Items = await itemsTask ?? PagedResult<FixedAssetDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("assets/create")]
    public async Task<IActionResult> CreateAsset(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new FixedAssetEditViewModel();
        await PopulateAssetFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create Asset";
        ViewData["Breadcrumb"] = "Fixed Assets / Assets / Create";

        return View("Assets/Create", model);
    }

    [HttpPost("assets/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAsset(FixedAssetEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        await PopulateAssetFormOptionsAsync(accessToken, model, ct);
        ValidateAssetForm(model, ModelState);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Asset";
            ViewData["Breadcrumb"] = "Fixed Assets / Assets / Create";
            return View("Assets/Create", model);
        }

        var created = await fixedAssetsApiClient.CreateAssetAsync(accessToken, MapAssetDto(model), ct);
        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create asset.");
            ViewData["Title"] = "Create Asset";
            ViewData["Breadcrumb"] = "Fixed Assets / Assets / Create";
            return View("Assets/Create", model);
        }

        TempData["SuccessMessage"] = "Asset created.";
        return RedirectToAction(nameof(Assets));
    }

    [HttpGet("assets/edit/{id:int}")]
    public async Task<IActionResult> EditAsset(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var detail = await fixedAssetsApiClient.GetAssetByIdAsync(accessToken, id, ct);
        if (detail is null)
        {
            return NotFound();
        }

        var item = detail.Asset;
        var model = new FixedAssetEditViewModel
        {
            Id = item.Id,
            AssetCode = item.AssetCode,
            Name = item.Name,
            CategoryId = item.CategoryId,
            LocationId = item.LocationId,
            DepartmentId = item.DepartmentId,
            AcquisitionDate = item.AcquisitionDate,
            InServiceDate = item.InServiceDate,
            AcquisitionCost = item.AcquisitionCost,
            SalvageValue = item.SalvageValue,
            UsefulLifeMonths = item.UsefulLifeMonths,
            DepreciationMethod = item.DepreciationMethod,
            DepreciationRate = item.DepreciationRate,
            Status = item.Status,
            SerialNumber = item.SerialNumber,
            VendorName = item.VendorName,
            Description = item.Description,
            IsActive = item.IsActive
        };

        await PopulateAssetFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit Asset";
        ViewData["Breadcrumb"] = "Fixed Assets / Assets / Edit";

        return View("Assets/Edit", model);
    }

    [HttpPost("assets/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAsset(int id, FixedAssetEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        await PopulateAssetFormOptionsAsync(accessToken, model, ct);
        ValidateAssetForm(model, ModelState);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Asset";
            ViewData["Breadcrumb"] = "Fixed Assets / Assets / Edit";
            return View("Assets/Edit", model);
        }

        var updated = await fixedAssetsApiClient.UpdateAssetAsync(accessToken, id, MapAssetDto(model), ct);
        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to update asset.");
            ViewData["Title"] = "Edit Asset";
            ViewData["Breadcrumb"] = "Fixed Assets / Assets / Edit";
            return View("Assets/Edit", model);
        }

        TempData["SuccessMessage"] = "Asset updated.";
        return RedirectToAction(nameof(Assets));
    }

    [HttpPost("assets/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAsset(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var deleted = await fixedAssetsApiClient.DeleteAssetAsync(accessToken, id, ct);
        TempData[deleted ? "SuccessMessage" : "ErrorMessage"] = deleted
            ? "Asset deleted."
            : "Failed to delete asset.";

        return RedirectToAction(nameof(Assets));
    }

    [HttpGet("assets/{id:int}")]
    public async Task<IActionResult> AssetDetail(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var detail = await fixedAssetsApiClient.GetAssetByIdAsync(accessToken, id, ct);
        if (detail is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Asset Detail";
        ViewData["Breadcrumb"] = "Fixed Assets / Assets / Detail";

        return View("Assets/Detail", new FixedAssetDetailViewModel
        {
            Data = detail
        });
    }

    [HttpGet("depreciation-runs")]
    public async Task<IActionResult> DepreciationRuns(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "periodyear",
        string? sortDirection = "desc",
        short? periodYear = null,
        byte? periodMonth = null,
        DepreciationRunStatus? status = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "periodyear", "runno", "periodyear", "periodmonth", "rundate", "totalassetcount", "totaldepreciationamount", "status");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);

        var items = await fixedAssetsApiClient.GetDepreciationRunsAsync(accessToken, new FixedAssetDepreciationRunPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            PeriodYear = periodYear,
            PeriodMonth = periodMonth,
            Status = status
        }, ct);

        ViewData["Title"] = "Depreciation Runs";
        ViewData["Breadcrumb"] = "Fixed Assets / Depreciation Runs";

        return View("DepreciationRuns/Index", new FixedAssetDepreciationRunsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            PeriodYearFilter = periodYear,
            PeriodMonthFilter = periodMonth,
            StatusFilter = status,
            Items = items ?? PagedResult<FixedAssetDepreciationRunDto>.Create([], 0, normalizedPage, normalizedPageSize),
            RunForm = new FixedAssetRunDepreciationFormViewModel
            {
                PeriodYear = periodYear ?? (short)DateTime.UtcNow.Year,
                PeriodMonth = periodMonth ?? (byte)DateTime.UtcNow.Month,
                ApproveImmediately = false
            }
        });
    }

    [HttpPost("depreciation-runs/run")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RunDepreciation(FixedAssetRunDepreciationFormViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Invalid depreciation period.";
            return RedirectToAction(nameof(DepreciationRuns));
        }

        var result = await fixedAssetsApiClient.RunDepreciationAsync(accessToken, new RunDepreciationRequest
        {
            PeriodYear = model.PeriodYear,
            PeriodMonth = model.PeriodMonth,
            ApproveImmediately = model.ApproveImmediately
        }, ct);

        TempData[result is not null ? "SuccessMessage" : "ErrorMessage"] = result is not null
            ? "Depreciation run processed."
            : "Failed to process depreciation run.";

        return RedirectToAction(nameof(DepreciationRuns), new
        {
            periodYear = model.PeriodYear,
            periodMonth = model.PeriodMonth
        });
    }

    [HttpPost("depreciation-runs/approve/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveDepreciationRun(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await fixedAssetsApiClient.ApproveDepreciationRunAsync(accessToken, id, ct);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
            ? "Depreciation run approved."
            : "Failed to approve depreciation run.";

        return RedirectToAction(nameof(DepreciationRuns));
    }
}
