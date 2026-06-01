using System.Security.Claims;
using ERP.Application.DTOs.Finance;
using ERP.Application.DTOs.Inventory;
using ERP.Web.Services;
using ERP.Web.ViewModels.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[Authorize]
[Route("inventory")]
public sealed partial class InventoryController(
    IInventoryApiClient inventoryApiClient,
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

    private async Task PopulateItemFormOptionsAsync(string accessToken, InventoryItemEditViewModel model, CancellationToken ct)
    {
        var categoryTask = inventoryApiClient.GetCategoryOptionsAsync(accessToken, ct);
        var brandTask = inventoryApiClient.GetBrandOptionsAsync(accessToken, ct);
        var uomTask = inventoryApiClient.GetUnitOptionsAsync(accessToken, ct);
        var accountTask = GetAccountOptionsAsync(accessToken, ct);

        await Task.WhenAll(categoryTask, brandTask, uomTask, accountTask);

        model.CategoryOptions = await categoryTask;
        model.BrandOptions = await brandTask;
        model.UomOptions = await uomTask;
        model.AccountOptions = await accountTask;
    }

    private async Task PopulateItemConversionOptionsAsync(string accessToken, InventoryItemConversionEditViewModel model, CancellationToken ct)
    {
        var itemTask = inventoryApiClient.GetItemOptionsAsync(accessToken, ct);
        var uomTask = inventoryApiClient.GetUnitOptionsAsync(accessToken, ct);

        await Task.WhenAll(itemTask, uomTask);

        model.ItemOptions = await itemTask;
        model.UomOptions = await uomTask;
    }

    private async Task PopulateWarehouseFormOptionsAsync(string accessToken, InventoryWarehouseEditViewModel model, CancellationToken ct)
    {
        var managerTask = GetManagerOptionsAsync(accessToken, ct);
        var costCenterTask = GetCostCenterOptionsAsync(accessToken, ct);

        await Task.WhenAll(managerTask, costCenterTask);

        model.ManagerOptions = await managerTask;
        model.CostCenterOptions = await costCenterTask;
    }

    private async Task<IReadOnlyList<InventoryOptionDto>> GetManagerOptionsAsync(string accessToken, CancellationToken ct)
    {
        var employees = await hrApiClient.GetEmployeeOptionsAsync(accessToken, ct);
        return employees
            .OrderBy(x => x.Name)
            .Select(x => new InventoryOptionDto
            {
                Id = x.Id,
                Label = x.Name
            })
            .ToList();
    }

    private async Task<IReadOnlyList<InventoryOptionDto>> GetDepartmentOptionsAsync(string accessToken, CancellationToken ct)
    {
        var departments = await hrApiClient.GetDepartmentOptionsAsync(accessToken, ct);
        return departments
            .OrderBy(x => x.Code)
            .Select(x => new InventoryOptionDto
            {
                Id = x.Id,
                Label = $"{x.Code} - {x.Name}"
            })
            .ToList();
    }

    private async Task<IReadOnlyList<InventoryOptionDto>> GetWarehouseLocationOptionsAsync(string accessToken, int? warehouseId, CancellationToken ct)
    {
        if (!warehouseId.HasValue || warehouseId.Value <= 0)
        {
            return [];
        }

        return await inventoryApiClient.GetWarehouseLocationOptionsAsync(accessToken, warehouseId.Value, ct);
    }

    private async Task<IReadOnlyList<InventoryOptionDto>> GetCostCenterOptionsAsync(string accessToken, CancellationToken ct)
    {
        var result = await financeApiClient.GetCostCentersAsync(accessToken, new CostCenterPagedRequest
        {
            Page = 1,
            PageSize = 200,
            SortBy = "code",
            SortDirection = "asc",
            IsActive = true
        }, ct);

        return result?.Items
            .OrderBy(x => x.Code)
            .Select(x => new InventoryOptionDto
            {
                Id = x.Id,
                Label = $"{x.Code} - {x.Name}"
            })
            .ToList() ?? [];
    }

    private async Task<IReadOnlyList<InventoryOptionDto>> GetAccountOptionsAsync(string accessToken, CancellationToken ct)
    {
        var result = await financeApiClient.GetAccountsAsync(accessToken, new AccountPagedRequest
        {
            Page = 1,
            PageSize = 200,
            SortBy = "code",
            SortDirection = "asc",
            IsActive = true
        }, ct);

        return result?.Items
            .OrderBy(x => x.Code)
            .Select(x => new InventoryOptionDto
            {
                Id = x.Id,
                Label = $"{x.Code} - {x.Name}"
            })
            .ToList() ?? [];
    }

    private static ItemDto MapItemDto(InventoryItemEditViewModel model)
    {
        return new ItemDto
        {
            Id = model.Id ?? 0,
            ItemCode = model.ItemCode,
            Sku = model.Sku,
            Name = model.Name,
            Description = model.Description,
            CategoryId = model.CategoryId,
            BrandId = model.BrandId,
            Type = model.Type,
            BaseUomId = model.BaseUomId,
            PurchaseUomId = model.PurchaseUomId,
            Status = model.Status,
            ValuationMethod = model.ValuationMethod,
            LastPurchasePrice = model.LastPurchasePrice,
            AvgCost = model.AvgCost,
            MinStock = model.MinStock,
            MaxStock = model.MaxStock,
            ReorderPoint = model.ReorderPoint,
            LeadTimeDays = model.LeadTimeDays,
            InventoryAccountId = model.InventoryAccountId,
            CogsAccountId = model.CogsAccountId,
            AdjustmentAccountId = model.AdjustmentAccountId,
            Notes = model.Notes,
            IsActive = model.IsActive
        };
    }
}
