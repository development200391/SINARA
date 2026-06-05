using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.FixedAssets;
using ERP.Domain.Enums.FixedAssets;
using ERP.Web.Services;
using ERP.Web.ViewModels.FixedAssets;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class FixedAssetsController
{
    [HttpGet("disposals")]
    public async Task<IActionResult> Disposals(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "disposaldate",
        string? sortDirection = "desc",
        int? assetId = null,
        DisposalType? disposalType = null,
        DisposalStatus? status = null,
        DateOnly? disposalDateFrom = null,
        DateOnly? disposalDateTo = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "disposaldate", "disposalno", "assetcode", "disposaldate", "disposaltype", "saleamount", "gainlossamount", "status");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedDateFrom, normalizedDateTo) = NormalizeDateRange(disposalDateFrom, disposalDateTo);

        var assetOptionsTask = fixedAssetsApiClient.GetAssetOptionsAsync(accessToken, ct);
        var itemsTask = fixedAssetsApiClient.GetDisposalsAsync(accessToken, new FixedAssetDisposalPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            AssetId = assetId,
            DisposalType = disposalType,
            Status = status,
            DisposalDateFrom = normalizedDateFrom,
            DisposalDateTo = normalizedDateTo
        }, ct);

        await Task.WhenAll(assetOptionsTask, itemsTask);

        ViewData["Title"] = "Disposals";
        ViewData["Breadcrumb"] = "Fixed Assets / Disposals";

        return View("Disposals/Index", new FixedAssetDisposalsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            AssetIdFilter = assetId,
            DisposalTypeFilter = disposalType,
            StatusFilter = status,
            DisposalDateFromFilter = normalizedDateFrom,
            DisposalDateToFilter = normalizedDateTo,
            AssetOptions = await assetOptionsTask,
            Items = await itemsTask ?? PagedResult<FixedAssetDisposalDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("disposals/create")]
    public async Task<IActionResult> CreateDisposal(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new FixedAssetDisposalEditViewModel();
        await PopulateDisposalFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create Disposal";
        ViewData["Breadcrumb"] = "Fixed Assets / Disposals / Create";

        return View("Disposals/Create", model);
    }

    [HttpPost("disposals/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDisposal(FixedAssetDisposalEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        await PopulateDisposalFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Disposal";
            ViewData["Breadcrumb"] = "Fixed Assets / Disposals / Create";
            return View("Disposals/Create", model);
        }

        var created = await fixedAssetsApiClient.CreateDisposalAsync(accessToken, MapDisposalDto(model), ct);
        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to create disposal." : created.ErrorMessage);
            ViewData["Title"] = "Create Disposal";
            ViewData["Breadcrumb"] = "Fixed Assets / Disposals / Create";
            return View("Disposals/Create", model);
        }

        TempData["SuccessMessage"] = "Disposal created.";
        return RedirectToAction(nameof(Disposals));
    }
    [HttpGet("disposals/edit/{id:int}")]
    public async Task<IActionResult> EditDisposal(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await fixedAssetsApiClient.GetDisposalByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        var model = new FixedAssetDisposalEditViewModel
        {
            Id = item.Id,
            AssetId = item.AssetId,
            DisposalDate = item.DisposalDate,
            DisposalType = item.DisposalType,
            SaleAmount = item.SaleAmount,
            DisposalExpense = item.DisposalExpense,
            Notes = item.Notes
        };

        await PopulateDisposalFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit Disposal";
        ViewData["Breadcrumb"] = "Fixed Assets / Disposals / Edit";

        return View("Disposals/Edit", model);
    }

    [HttpPost("disposals/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDisposal(int id, FixedAssetDisposalEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        await PopulateDisposalFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Disposal";
            ViewData["Breadcrumb"] = "Fixed Assets / Disposals / Edit";
            return View("Disposals/Edit", model);
        }

        var updated = await fixedAssetsApiClient.UpdateDisposalAsync(accessToken, id, MapDisposalDto(model), ct);
        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update disposal." : updated.ErrorMessage);
            ViewData["Title"] = "Edit Disposal";
            ViewData["Breadcrumb"] = "Fixed Assets / Disposals / Edit";
            return View("Disposals/Edit", model);
        }

        TempData["SuccessMessage"] = "Disposal updated.";
        return RedirectToAction(nameof(Disposals));
    }

    [HttpPost("disposals/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDisposal(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var deleted = await fixedAssetsApiClient.DeleteDisposalAsync(accessToken, id, ct);
        TempData[deleted.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = deleted.IsSuccess ? "Disposal deleted." : (string.IsNullOrWhiteSpace(deleted.ErrorMessage) ? "Failed to delete disposal." : deleted.ErrorMessage);

        return RedirectToAction(nameof(Disposals));
    }

    [HttpPost("disposals/process/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessDisposal(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await fixedAssetsApiClient.GetDisposalByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            TempData["ErrorMessage"] = "Disposal not found.";
            return RedirectToAction(nameof(Disposals));
        }

        var ok = item.Status switch
        {
            DisposalStatus.Draft => await fixedAssetsApiClient.ApproveDisposalAsync(accessToken, id, ct),
            DisposalStatus.Approved => await fixedAssetsApiClient.PostDisposalAsync(accessToken, id, ct),
            _ => ApiCallResult<object?>.Failure("No process action available for the current status.")
        };

        TempData[ok.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = ok.IsSuccess ? "Disposal processed." : (string.IsNullOrWhiteSpace(ok.ErrorMessage) ? "Failed to process disposal." : ok.ErrorMessage);

        return RedirectToAction(nameof(Disposals));
    }

    [HttpPost("disposals/cancel/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelDisposal(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await fixedAssetsApiClient.CancelDisposalAsync(accessToken, id, ct);
        TempData[ok.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = ok.IsSuccess ? "Disposal cancelled." : (string.IsNullOrWhiteSpace(ok.ErrorMessage) ? "Failed to cancel disposal." : ok.ErrorMessage);

        return RedirectToAction(nameof(Disposals));
    }
    [HttpGet("revaluations")]
    public async Task<IActionResult> Revaluations(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "revaluationdate",
        string? sortDirection = "desc",
        int? assetId = null,
        RevaluationStatus? status = null,
        DateOnly? revaluationDateFrom = null,
        DateOnly? revaluationDateTo = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "revaluationdate", "revaluationno", "assetcode", "revaluationdate", "oldbookvalue", "newbookvalue", "impairmentamount", "status");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var (normalizedDateFrom, normalizedDateTo) = NormalizeDateRange(revaluationDateFrom, revaluationDateTo);

        var assetOptionsTask = fixedAssetsApiClient.GetAssetOptionsAsync(accessToken, ct);
        var itemsTask = fixedAssetsApiClient.GetRevaluationsAsync(accessToken, new FixedAssetRevaluationPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            AssetId = assetId,
            Status = status,
            RevaluationDateFrom = normalizedDateFrom,
            RevaluationDateTo = normalizedDateTo
        }, ct);

        await Task.WhenAll(assetOptionsTask, itemsTask);

        ViewData["Title"] = "Revaluations";
        ViewData["Breadcrumb"] = "Fixed Assets / Revaluations";

        return View("Revaluations/Index", new FixedAssetRevaluationsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            AssetIdFilter = assetId,
            StatusFilter = status,
            RevaluationDateFromFilter = normalizedDateFrom,
            RevaluationDateToFilter = normalizedDateTo,
            AssetOptions = await assetOptionsTask,
            Items = await itemsTask ?? PagedResult<FixedAssetRevaluationDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("revaluations/create")]
    public async Task<IActionResult> CreateRevaluation(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new FixedAssetRevaluationEditViewModel();
        await PopulateRevaluationFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create Revaluation";
        ViewData["Breadcrumb"] = "Fixed Assets / Revaluations / Create";

        return View("Revaluations/Create", model);
    }

    [HttpPost("revaluations/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRevaluation(FixedAssetRevaluationEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        await PopulateRevaluationFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Revaluation";
            ViewData["Breadcrumb"] = "Fixed Assets / Revaluations / Create";
            return View("Revaluations/Create", model);
        }

        var created = await fixedAssetsApiClient.CreateRevaluationAsync(accessToken, MapRevaluationDto(model), ct);
        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to create revaluation." : created.ErrorMessage);
            ViewData["Title"] = "Create Revaluation";
            ViewData["Breadcrumb"] = "Fixed Assets / Revaluations / Create";
            return View("Revaluations/Create", model);
        }

        TempData["SuccessMessage"] = "Revaluation created.";
        return RedirectToAction(nameof(Revaluations));
    }
    [HttpGet("revaluations/edit/{id:int}")]
    public async Task<IActionResult> EditRevaluation(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await fixedAssetsApiClient.GetRevaluationByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        var model = new FixedAssetRevaluationEditViewModel
        {
            Id = item.Id,
            AssetId = item.AssetId,
            RevaluationDate = item.RevaluationDate,
            OldBookValue = item.OldBookValue,
            NewBookValue = item.NewBookValue,
            ImpairmentAmount = item.ImpairmentAmount,
            Notes = item.Notes
        };

        await PopulateRevaluationFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit Revaluation";
        ViewData["Breadcrumb"] = "Fixed Assets / Revaluations / Edit";

        return View("Revaluations/Edit", model);
    }

    [HttpPost("revaluations/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRevaluation(int id, FixedAssetRevaluationEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        await PopulateRevaluationFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Revaluation";
            ViewData["Breadcrumb"] = "Fixed Assets / Revaluations / Edit";
            return View("Revaluations/Edit", model);
        }

        var updated = await fixedAssetsApiClient.UpdateRevaluationAsync(accessToken, id, MapRevaluationDto(model), ct);
        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update revaluation." : updated.ErrorMessage);
            ViewData["Title"] = "Edit Revaluation";
            ViewData["Breadcrumb"] = "Fixed Assets / Revaluations / Edit";
            return View("Revaluations/Edit", model);
        }

        TempData["SuccessMessage"] = "Revaluation updated.";
        return RedirectToAction(nameof(Revaluations));
    }

    [HttpPost("revaluations/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRevaluation(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var deleted = await fixedAssetsApiClient.DeleteRevaluationAsync(accessToken, id, ct);
        TempData[deleted.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = deleted.IsSuccess ? "Revaluation deleted." : (string.IsNullOrWhiteSpace(deleted.ErrorMessage) ? "Failed to delete revaluation." : deleted.ErrorMessage);

        return RedirectToAction(nameof(Revaluations));
    }

    [HttpPost("revaluations/process/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessRevaluation(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await fixedAssetsApiClient.GetRevaluationByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            TempData["ErrorMessage"] = "Revaluation not found.";
            return RedirectToAction(nameof(Revaluations));
        }

        var ok = item.Status switch
        {
            RevaluationStatus.Draft => await fixedAssetsApiClient.ApproveRevaluationAsync(accessToken, id, ct),
            RevaluationStatus.Approved => await fixedAssetsApiClient.PostRevaluationAsync(accessToken, id, ct),
            _ => ApiCallResult<object?>.Failure("No process action available for the current status.")
        };

        TempData[ok.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = ok.IsSuccess ? "Revaluation processed." : (string.IsNullOrWhiteSpace(ok.ErrorMessage) ? "Failed to process revaluation." : ok.ErrorMessage);

        return RedirectToAction(nameof(Revaluations));
    }
}
