using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Inventory;
using ERP.Domain.Enums.Inventory;
using ERP.Web.ViewModels.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class InventoryController
{
    [HttpGet("opnames")]
    public async Task<IActionResult> Opnames(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = "opnamedate",
        string? sortDirection = "desc",
        string? opnameNo = null,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        int? warehouseId = null,
        OpnameStatus? status = null,
        CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy, "opnamedate", "opnameno", "opnamedate", "warehousecode", "status");
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);

        var itemsTask = inventoryApiClient.GetOpnamesAsync(accessToken, new StockOpnamePagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            OpnameNo = NormalizeText(opnameNo),
            DateFrom = dateFrom,
            DateTo = dateTo,
            WarehouseId = warehouseId,
            Status = status
        }, ct);

        var warehouseOptionsTask = inventoryApiClient.GetWarehouseOptionsAsync(accessToken, ct);

        await Task.WhenAll(itemsTask, warehouseOptionsTask);

        ViewData["Title"] = "Stock Opnames";
        ViewData["Breadcrumb"] = "Inventory / Stock Opnames";

        return View("Opnames/Index", new InventoryOpnamesIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            OpnameNoFilter = NormalizeText(opnameNo),
            DateFromFilter = dateFrom,
            DateToFilter = dateTo,
            WarehouseIdFilter = warehouseId,
            StatusFilter = status,
            WarehouseOptions = await warehouseOptionsTask,
            Items = await itemsTask ?? PagedResult<StockOpnameDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("opnames/create")]
    public async Task<IActionResult> CreateOpname(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var model = new InventoryOpnameEditViewModel();
        await PopulateOpnameOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create Stock Opname";
        ViewData["Breadcrumb"] = "Inventory / Stock Opnames / Create";

        return View("Opnames/Create", model);
    }

    [HttpPost("opnames/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOpname(InventoryOpnameEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        await PopulateOpnameOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Stock Opname";
            ViewData["Breadcrumb"] = "Inventory / Stock Opnames / Create";
            return View("Opnames/Create", model);
        }

        var created = await inventoryApiClient.CreateOpnameAsync(accessToken, new StockOpnameDto
        {
            OpnameDate = model.OpnameDate,
            WarehouseId = model.WarehouseId,
            LocationId = model.LocationId,
            Description = NormalizeText(model.Description)
        }, ct);

        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to create stock opname." : created.ErrorMessage);
            ViewData["Title"] = "Create Stock Opname";
            ViewData["Breadcrumb"] = "Inventory / Stock Opnames / Create";
            return View("Opnames/Create", model);
        }

        TempData["SuccessMessage"] = "Stock opname created.";
        return RedirectToAction(nameof(Opnames));
    }

    [HttpGet("opnames/edit/{id:int}")]
    public async Task<IActionResult> EditOpname(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var dto = await inventoryApiClient.GetOpnameByIdAsync(accessToken, id, ct);
        if (dto is null)
        {
            return NotFound();
        }

        var model = new InventoryOpnameEditViewModel
        {
            Id = dto.Id,
            OpnameDate = dto.OpnameDate,
            WarehouseId = dto.WarehouseId,
            LocationId = dto.LocationId,
            Description = dto.Description,
            Status = dto.Status
        };

        await PopulateOpnameOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit Stock Opname";
        ViewData["Breadcrumb"] = "Inventory / Stock Opnames / Edit";

        return View("Opnames/Edit", model);
    }

    [HttpPost("opnames/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditOpname(int id, InventoryOpnameEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        await PopulateOpnameOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Stock Opname";
            ViewData["Breadcrumb"] = "Inventory / Stock Opnames / Edit";
            return View("Opnames/Edit", model);
        }

        var updated = await inventoryApiClient.UpdateOpnameAsync(accessToken, id, new StockOpnameDto
        {
            Id = id,
            OpnameDate = model.OpnameDate,
            WarehouseId = model.WarehouseId,
            LocationId = model.LocationId,
            Description = NormalizeText(model.Description)
        }, ct);

        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update stock opname." : updated.ErrorMessage);
            ViewData["Title"] = "Edit Stock Opname";
            ViewData["Breadcrumb"] = "Inventory / Stock Opnames / Edit";
            return View("Opnames/Edit", model);
        }

        TempData["SuccessMessage"] = "Stock opname updated.";
        return RedirectToAction(nameof(Opnames));
    }

    [HttpPost("opnames/start/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartOpname(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await inventoryApiClient.StartOpnameAsync(accessToken, id, ct);
        TempData[ok.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = ok.IsSuccess ? "Stock opname started." : (string.IsNullOrWhiteSpace(ok.ErrorMessage) ? "Failed to start stock opname." : ok.ErrorMessage);
        return RedirectToAction(nameof(Opnames));
    }

    [HttpPost("opnames/complete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteOpname(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await inventoryApiClient.CompleteOpnameAsync(accessToken, id, ct);
        TempData[ok.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = ok.IsSuccess ? "Stock opname completed." : (string.IsNullOrWhiteSpace(ok.ErrorMessage) ? "Failed to complete stock opname." : ok.ErrorMessage);
        return RedirectToAction(nameof(Opnames));
    }

    [HttpPost("opnames/approve/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveOpname(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await inventoryApiClient.ApproveOpnameAsync(accessToken, id, ct);
        TempData[ok.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = ok.IsSuccess ? "Stock opname approved." : (string.IsNullOrWhiteSpace(ok.ErrorMessage) ? "Failed to approve stock opname." : ok.ErrorMessage);
        return RedirectToAction(nameof(Opnames));
    }

    [HttpPost("opnames/cancel/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelOpname(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var ok = await inventoryApiClient.CancelOpnameAsync(accessToken, id, ct);
        TempData[ok.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = ok.IsSuccess ? "Stock opname cancelled." : (string.IsNullOrWhiteSpace(ok.ErrorMessage) ? "Failed to cancel stock opname." : ok.ErrorMessage);
        return RedirectToAction(nameof(Opnames));
    }

    private async Task PopulateOpnameOptionsAsync(string accessToken, InventoryOpnameEditViewModel model, CancellationToken ct)
    {
        var warehouseTask = inventoryApiClient.GetWarehouseOptionsAsync(accessToken, ct);
        var locationTask = GetWarehouseLocationOptionsAsync(accessToken, model.WarehouseId > 0 ? model.WarehouseId : null, ct);

        await Task.WhenAll(warehouseTask, locationTask);

        model.WarehouseOptions = await warehouseTask;
        model.LocationOptions = await locationTask;
    }
}
