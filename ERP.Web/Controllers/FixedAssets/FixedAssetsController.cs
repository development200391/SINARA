using System.Security.Claims;
using ERP.Application.DTOs.Finance;
using ERP.Application.DTOs.FixedAssets;
using ERP.Domain.Enums.FixedAssets;
using ERP.Web.Services;
using ERP.Web.ViewModels.FixedAssets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ERP.Web.Controllers;

[Authorize]
[Route("fixed-assets")]
public sealed partial class FixedAssetsController(
    IFixedAssetsApiClient fixedAssetsApiClient,
    IHrApiClient hrApiClient,
    IFinanceApiClient financeApiClient) : Controller
{
    private const int DefaultPageSize = 20;

    private IActionResult? RequireAccessToken(out string accessToken, bool includeReturnUrl = true)
    {
        accessToken = User.FindFirstValue("access_token") ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            return null;
        }

        return includeReturnUrl
            ? RedirectToAction("Login", "Auth", new { returnUrl = Request.Path + Request.QueryString })
            : RedirectToAction("Login", "Auth");
    }

    private static int NormalizePageSize(int pageSize) => pageSize is 20 or 50 or 100 ? pageSize : DefaultPageSize;

    private static string NormalizeSortDirection(string? sortDirection) =>
        string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";

    private static string NormalizeSortBy(string? sortBy, string defaultSortBy, params string[] allowedSortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return defaultSortBy;
        }

        var normalized = sortBy.Trim().ToLowerInvariant();
        return allowedSortBy.Contains(normalized, StringComparer.OrdinalIgnoreCase) ? normalized : defaultSortBy;
    }

    private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static (decimal? From, decimal? To) NormalizeDecimalRange(decimal? from, decimal? to)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
        {
            return (to, from);
        }

        return (from, to);
    }

    private static (DateOnly? From, DateOnly? To) NormalizeDateRange(DateOnly? from, DateOnly? to)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
        {
            return (to, from);
        }

        return (from, to);
    }

    private async Task PopulateAssetCategoryFormOptionsAsync(string accessToken, FixedAssetCategoryEditViewModel model, CancellationToken ct)
    {
        model.AccountOptions = await GetAccountOptionsAsync(accessToken, ct);
    }

    private async Task PopulateLocationFormOptionsAsync(string accessToken, FixedAssetLocationEditViewModel model, CancellationToken ct)
    {
        var departmentOptionsTask = GetDepartmentOptionsAsync(accessToken, ct);
        var managerOptionsTask = GetEmployeeOptionsAsync(accessToken, ct);

        await Task.WhenAll(departmentOptionsTask, managerOptionsTask);

        model.DepartmentOptions = await departmentOptionsTask;
        model.ManagerOptions = await managerOptionsTask;
    }

    private async Task PopulateAssetFormOptionsAsync(string accessToken, FixedAssetEditViewModel model, CancellationToken ct)
    {
        var categoryOptionsTask = fixedAssetsApiClient.GetAssetCategoryOptionsAsync(accessToken, ct);
        var locationOptionsTask = fixedAssetsApiClient.GetLocationOptionsAsync(accessToken, ct);
        var departmentOptionsTask = GetDepartmentOptionsAsync(accessToken, ct);

        await Task.WhenAll(categoryOptionsTask, locationOptionsTask, departmentOptionsTask);

        model.CategoryOptions = await categoryOptionsTask;
        model.LocationOptions = await locationOptionsTask;
        model.DepartmentOptions = await departmentOptionsTask;
    }

    private async Task PopulateTransferFormOptionsAsync(string accessToken, FixedAssetTransferEditViewModel model, CancellationToken ct)
    {
        var assetOptionsTask = fixedAssetsApiClient.GetAssetOptionsAsync(accessToken, ct);
        var locationOptionsTask = fixedAssetsApiClient.GetLocationOptionsAsync(accessToken, ct);
        var departmentOptionsTask = GetDepartmentOptionsAsync(accessToken, ct);

        await Task.WhenAll(assetOptionsTask, locationOptionsTask, departmentOptionsTask);

        model.AssetOptions = await assetOptionsTask;
        model.LocationOptions = await locationOptionsTask;
        model.DepartmentOptions = await departmentOptionsTask;
    }

    private async Task PopulateMaintenanceOrderFormOptionsAsync(string accessToken, FixedAssetMaintenanceOrderEditViewModel model, CancellationToken ct)
    {
        model.AssetOptions = await fixedAssetsApiClient.GetAssetOptionsAsync(accessToken, ct);
    }

    private async Task PopulateDisposalFormOptionsAsync(string accessToken, FixedAssetDisposalEditViewModel model, CancellationToken ct)
    {
        model.AssetOptions = await fixedAssetsApiClient.GetAssetOptionsAsync(accessToken, ct);
    }

    private async Task PopulateRevaluationFormOptionsAsync(string accessToken, FixedAssetRevaluationEditViewModel model, CancellationToken ct)
    {
        model.AssetOptions = await fixedAssetsApiClient.GetAssetOptionsAsync(accessToken, ct);
    }

    private async Task<IReadOnlyList<FixedAssetOptionDto>> GetDepartmentOptionsAsync(string accessToken, CancellationToken ct)
    {
        var options = await hrApiClient.GetDepartmentOptionsAsync(accessToken, ct);
        return options
            .OrderBy(x => x.Code)
            .Select(x => new FixedAssetOptionDto
            {
                Id = x.Id,
                Label = string.IsNullOrWhiteSpace(x.Code) ? x.Name : $"{x.Code} - {x.Name}"
            })
            .ToList();
    }

    private async Task<IReadOnlyList<FixedAssetOptionDto>> GetEmployeeOptionsAsync(string accessToken, CancellationToken ct)
    {
        var options = await hrApiClient.GetEmployeeOptionsAsync(accessToken, ct);
        return options
            .OrderBy(x => x.Name)
            .Select(x => new FixedAssetOptionDto
            {
                Id = x.Id,
                Label = x.Name
            })
            .ToList();
    }

    private async Task<IReadOnlyList<FixedAssetOptionDto>> GetAccountOptionsAsync(string accessToken, CancellationToken ct)
    {
        var response = await financeApiClient.GetAccountsAsync(accessToken, new AccountPagedRequest
        {
            Page = 1,
            PageSize = 500,
            SortBy = "code",
            SortDirection = "asc",
            IsActive = true
        }, ct);

        return response?.Items
            .OrderBy(x => x.Code)
            .Select(x => new FixedAssetOptionDto
            {
                Id = x.Id,
                Label = $"{x.Code} - {x.Name}"
            })
            .ToList() ?? [];
    }

    private static void ValidateAssetCategoryForm(FixedAssetCategoryEditViewModel model, ModelStateDictionary modelState)
    {
        if (model.DepreciationMethod == DepreciationMethod.DecliningBalance && !model.DepreciationRate.HasValue)
        {
            modelState.AddModelError(nameof(model.DepreciationRate), "Depreciation rate is required for declining balance method.");
        }
    }

    private static void ValidateDepreciationConfigForm(FixedAssetDepreciationConfigEditViewModel model, ModelStateDictionary modelState)
    {
        if (model.StartDate > model.EndDate)
        {
            modelState.AddModelError(nameof(model.EndDate), "End date must be on or after start date.");
        }
    }

    private static void ValidateAssetForm(FixedAssetEditViewModel model, ModelStateDictionary modelState)
    {
        if (model.SalvageValue > model.AcquisitionCost)
        {
            modelState.AddModelError(nameof(model.SalvageValue), "Salvage value cannot exceed acquisition cost.");
        }

        if (model.InServiceDate < model.AcquisitionDate)
        {
            modelState.AddModelError(nameof(model.InServiceDate), "In-service date must be on or after acquisition date.");
        }

        if (model.DepreciationMethod == DepreciationMethod.DecliningBalance && !model.DepreciationRate.HasValue)
        {
            modelState.AddModelError(nameof(model.DepreciationRate), "Depreciation rate is required for declining balance method.");
        }
    }

    private static FixedAssetCategoryDto MapAssetCategoryDto(FixedAssetCategoryEditViewModel model)
    {
        return new FixedAssetCategoryDto
        {
            Id = model.Id ?? 0,
            Code = model.Code,
            Name = model.Name,
            DepreciationMethod = model.DepreciationMethod,
            UsefulLifeMonths = model.UsefulLifeMonths,
            DepreciationRate = model.DepreciationRate,
            AssetAccountId = model.AssetAccountId,
            AccumulatedDepreciationAccountId = model.AccumulatedDepreciationAccountId,
            DepreciationExpenseAccountId = model.DepreciationExpenseAccountId,
            IsActive = model.IsActive
        };
    }

    private static FixedAssetLocationDto MapLocationDto(FixedAssetLocationEditViewModel model)
    {
        return new FixedAssetLocationDto
        {
            Id = model.Id ?? 0,
            Code = model.Code,
            Name = model.Name,
            Address = NormalizeText(model.Address),
            DepartmentId = model.DepartmentId,
            ManagerId = model.ManagerId,
            IsActive = model.IsActive
        };
    }

    private static FixedAssetDepreciationConfigDto MapDepreciationConfigDto(FixedAssetDepreciationConfigEditViewModel model)
    {
        return new FixedAssetDepreciationConfigDto
        {
            Id = model.Id ?? 0,
            Name = model.Name,
            FiscalYear = model.FiscalYear,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            RunDay = model.RunDay,
            IsAutoPostJournal = model.IsAutoPostJournal,
            IsActive = model.IsActive
        };
    }

    private static FixedAssetDto MapAssetDto(FixedAssetEditViewModel model)
    {
        return new FixedAssetDto
        {
            Id = model.Id ?? 0,
            AssetCode = NormalizeText(model.AssetCode) ?? string.Empty,
            Name = model.Name,
            CategoryId = model.CategoryId,
            LocationId = model.LocationId,
            DepartmentId = model.DepartmentId,
            AcquisitionDate = model.AcquisitionDate,
            InServiceDate = model.InServiceDate,
            AcquisitionCost = model.AcquisitionCost,
            SalvageValue = model.SalvageValue,
            UsefulLifeMonths = model.UsefulLifeMonths,
            DepreciationMethod = model.DepreciationMethod,
            DepreciationRate = model.DepreciationRate,
            Status = model.Status,
            SerialNumber = NormalizeText(model.SerialNumber),
            VendorName = NormalizeText(model.VendorName),
            Description = NormalizeText(model.Description),
            IsActive = model.IsActive
        };
    }

    private static FixedAssetTransferDto MapTransferDto(FixedAssetTransferEditViewModel model)
    {
        return new FixedAssetTransferDto
        {
            Id = model.Id ?? 0,
            TransferNo = string.Empty,
            AssetId = model.AssetId,
            AssetCode = string.Empty,
            AssetName = string.Empty,
            TransferDate = model.TransferDate,
            FromLocationId = model.FromLocationId,
            FromLocationName = string.Empty,
            ToLocationId = model.ToLocationId,
            ToLocationName = string.Empty,
            FromDepartmentId = model.FromDepartmentId,
            ToDepartmentId = model.ToDepartmentId,
            Reason = NormalizeText(model.Reason),
            Status = AssetTransferStatus.Draft
        };
    }

    private static FixedAssetMaintenanceOrderDto MapMaintenanceOrderDto(FixedAssetMaintenanceOrderEditViewModel model)
    {
        return new FixedAssetMaintenanceOrderDto
        {
            Id = model.Id ?? 0,
            WorkOrderNo = string.Empty,
            AssetId = model.AssetId,
            AssetCode = string.Empty,
            AssetName = string.Empty,
            OrderDate = model.OrderDate,
            MaintenanceType = model.MaintenanceType,
            VendorName = NormalizeText(model.VendorName),
            Cost = model.Cost,
            IsCapitalized = model.IsCapitalized,
            Status = MaintenanceStatus.Open,
            Notes = NormalizeText(model.Notes)
        };
    }

    private static FixedAssetDisposalDto MapDisposalDto(FixedAssetDisposalEditViewModel model)
    {
        return new FixedAssetDisposalDto
        {
            Id = model.Id ?? 0,
            DisposalNo = string.Empty,
            AssetId = model.AssetId,
            AssetCode = string.Empty,
            AssetName = string.Empty,
            DisposalDate = model.DisposalDate,
            DisposalType = model.DisposalType,
            SaleAmount = model.SaleAmount,
            DisposalExpense = model.DisposalExpense,
            GainLossAmount = null,
            Status = DisposalStatus.Draft,
            Notes = NormalizeText(model.Notes)
        };
    }

    private static FixedAssetRevaluationDto MapRevaluationDto(FixedAssetRevaluationEditViewModel model)
    {
        return new FixedAssetRevaluationDto
        {
            Id = model.Id ?? 0,
            RevaluationNo = string.Empty,
            AssetId = model.AssetId,
            AssetCode = string.Empty,
            AssetName = string.Empty,
            RevaluationDate = model.RevaluationDate,
            OldBookValue = model.OldBookValue,
            NewBookValue = model.NewBookValue,
            ImpairmentAmount = model.ImpairmentAmount,
            Status = RevaluationStatus.Draft,
            Notes = NormalizeText(model.Notes)
        };
    }
}
