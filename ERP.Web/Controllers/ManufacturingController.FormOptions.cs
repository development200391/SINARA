using System.Globalization;
using ERP.Application.DTOs.HR;
using ERP.Application.DTOs.Inventory;
using ERP.Application.DTOs.Manufacturing;
using ERP.Web.ViewModels.Manufacturing;

namespace ERP.Web.Controllers;

public sealed partial class ManufacturingController
{
    private async Task PopulateRoutingFormOptionsAsync(string accessToken, ManufacturingRoutingEditViewModel model, CancellationToken ct)
    {
        var itemOptionsTask = LoadItemOptionsAsync(accessToken, ct);
        var workCenterOptionsTask = LoadWorkCenterOptionsAsync(accessToken, ct);

        await Task.WhenAll(itemOptionsTask, workCenterOptionsTask);

        model.ItemOptions = await itemOptionsTask;
        model.WorkCenterOptions = await workCenterOptionsTask;
    }

    private async Task PopulateBomFormOptionsAsync(string accessToken, ManufacturingBomEditViewModel model, CancellationToken ct)
    {
        var itemOptionsTask = LoadItemOptionsAsync(accessToken, ct);
        var routingOptionsTask = LoadRoutingOptionsAsync(accessToken, ct);

        await Task.WhenAll(itemOptionsTask, routingOptionsTask);

        model.ItemOptions = await itemOptionsTask;
        model.RoutingOptions = await routingOptionsTask;
    }

    private async Task PopulateQcParameterFormOptionsAsync(string accessToken, ManufacturingQcParameterEditViewModel model, CancellationToken ct)
    {
        model.ItemOptions = await LoadItemOptionsAsync(accessToken, ct);
    }

    private async Task PopulateWorkOrderFormOptionsAsync(string accessToken, ManufacturingWorkOrderEditViewModel model, CancellationToken ct)
    {
        var itemOptionsTask = LoadItemOptionsAsync(accessToken, ct);
        var bomMappingsTask = LoadBomLinkedOptionsAsync(accessToken, ct);
        var routingMappingsTask = LoadRoutingLinkedOptionsAsync(accessToken, ct);
        var workCenterOptionsTask = LoadWorkCenterOptionsAsync(accessToken, ct);
        var mrpRunOptionsTask = LoadMrpRunOptionsAsync(accessToken, ct);

        await Task.WhenAll(itemOptionsTask, bomMappingsTask, routingMappingsTask, workCenterOptionsTask, mrpRunOptionsTask);

        var bomMappings = await bomMappingsTask;
        var routingMappings = await routingMappingsTask;

        model.ItemOptions = await itemOptionsTask;
        model.BomItemMappings = bomMappings;
        model.RoutingItemMappings = routingMappings;
        model.BomOptions = bomMappings.Select(ToInventoryOption).ToList();
        model.RoutingOptions = routingMappings.Select(ToInventoryOption).ToList();
        model.WorkCenterOptions = await workCenterOptionsTask;
        model.MrpRunOptions = await mrpRunOptionsTask;
    }

    private async Task PopulateQcInspectionFormOptionsAsync(string accessToken, ManufacturingQcInspectionEditViewModel model, CancellationToken ct)
    {
        var workOrderMappingsTask = LoadWorkOrderLinkedOptionsAsync(accessToken, ct);
        var itemOptionsTask = LoadItemOptionsAsync(accessToken, ct);
        var inspectorOptionsTask = LoadEmployeeOptionsAsync(accessToken, ct);

        await Task.WhenAll(workOrderMappingsTask, itemOptionsTask, inspectorOptionsTask);

        var workOrderMappings = await workOrderMappingsTask;

        model.WorkOrderItemMappings = workOrderMappings;
        model.WorkOrderOptions = workOrderMappings.Select(ToInventoryOption).ToList();
        model.ItemOptions = await itemOptionsTask;
        model.InspectorEmployeeOptions = await inspectorOptionsTask;
    }

    private async Task PopulateScrapFormOptionsAsync(string accessToken, ManufacturingScrapRecordEditViewModel model, CancellationToken ct)
    {
        var workOrderMappingsTask = LoadWorkOrderLinkedOptionsAsync(accessToken, ct);
        var itemOptionsTask = LoadItemOptionsAsync(accessToken, ct);
        var workCenterOptionsTask = LoadWorkCenterOptionsAsync(accessToken, ct);

        await Task.WhenAll(workOrderMappingsTask, itemOptionsTask, workCenterOptionsTask);

        var workOrderMappings = await workOrderMappingsTask;

        model.WorkOrderItemMappings = workOrderMappings;
        model.WorkOrderOptions = workOrderMappings.Select(ToInventoryOption).ToList();
        model.ItemOptions = await itemOptionsTask;
        model.WorkCenterOptions = await workCenterOptionsTask;
    }

    private async Task PopulateReworkFormOptionsAsync(string accessToken, ManufacturingReworkOrderEditViewModel model, CancellationToken ct)
    {
        var workOrderMappingsTask = LoadWorkOrderLinkedOptionsAsync(accessToken, ct);
        var itemOptionsTask = LoadItemOptionsAsync(accessToken, ct);

        await Task.WhenAll(workOrderMappingsTask, itemOptionsTask);

        var workOrderMappings = await workOrderMappingsTask;

        model.WorkOrderItemMappings = workOrderMappings;
        model.SourceWorkOrderOptions = workOrderMappings.Select(ToInventoryOption).ToList();
        model.WorkOrderOptions = workOrderMappings.Select(ToInventoryOption).ToList();
        model.ItemOptions = await itemOptionsTask;
    }

    private async Task<IReadOnlyList<InventoryOptionDto>> LoadItemOptionsAsync(string accessToken, CancellationToken ct)
    {
        var items = await inventoryApiClient.GetItemOptionsAsync(accessToken, ct);
        return items
            .OrderBy(x => x.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task<IReadOnlyList<LookupDto>> LoadEmployeeOptionsAsync(string accessToken, CancellationToken ct)
    {
        var employees = await hrApiClient.GetEmployeeOptionsAsync(accessToken, ct);
        return employees
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task<IReadOnlyList<InventoryOptionDto>> LoadWorkCenterOptionsAsync(string accessToken, CancellationToken ct)
    {
        var result = await manufacturingApiClient.GetWorkCentersAsync(accessToken, new ManufacturingWorkCenterPagedRequest
        {
            Page = 1,
            PageSize = 1000,
            SortBy = "code",
            SortDirection = "asc"
        }, ct);

        return (result?.Items ?? [])
            .Select(x => new InventoryOptionDto
            {
                Id = x.Id,
                Label = string.IsNullOrWhiteSpace(x.Name)
                    ? x.Code
                    : $"{x.Code} - {x.Name}"
            })
            .OrderBy(x => x.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task<IReadOnlyList<InventoryOptionDto>> LoadRoutingOptionsAsync(string accessToken, CancellationToken ct)
    {
        var options = await LoadRoutingLinkedOptionsAsync(accessToken, ct);
        return options.Select(ToInventoryOption).ToList();
    }

    private async Task<IReadOnlyList<InventoryOptionDto>> LoadBomOptionsAsync(string accessToken, CancellationToken ct)
    {
        var options = await LoadBomLinkedOptionsAsync(accessToken, ct);
        return options.Select(ToInventoryOption).ToList();
    }

    private async Task<IReadOnlyList<InventoryOptionDto>> LoadMrpRunOptionsAsync(string accessToken, CancellationToken ct)
    {
        var result = await manufacturingApiClient.GetMrpRunsAsync(accessToken, new ManufacturingMrpRunPagedRequest
        {
            Page = 1,
            PageSize = 1000,
            SortBy = "rundate",
            SortDirection = "desc"
        }, ct);

        return (result?.Items ?? [])
            .Select(x => new InventoryOptionDto
            {
                Id = x.Id,
                Label = $"{x.Code} ({x.RunDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)})"
            })
            .OrderByDescending(x => x.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task<IReadOnlyList<InventoryOptionDto>> LoadWorkOrderOptionsAsync(string accessToken, CancellationToken ct)
    {
        var options = await LoadWorkOrderLinkedOptionsAsync(accessToken, ct);
        return options.Select(ToInventoryOption).ToList();
    }

    private async Task<IReadOnlyList<ManufacturingLinkedOptionViewModel>> LoadRoutingLinkedOptionsAsync(string accessToken, CancellationToken ct)
    {
        var result = await manufacturingApiClient.GetRoutingsAsync(accessToken, new ManufacturingRoutingPagedRequest
        {
            Page = 1,
            PageSize = 1000,
            SortBy = "code",
            SortDirection = "asc"
        }, ct);

        return (result?.Items ?? [])
            .Select(x => new ManufacturingLinkedOptionViewModel
            {
                Id = x.Id,
                ItemId = x.ItemId,
                Label = string.IsNullOrWhiteSpace(x.Name)
                    ? x.Code
                    : $"{x.Code} - {x.Name}"
            })
            .OrderBy(x => x.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task<IReadOnlyList<ManufacturingLinkedOptionViewModel>> LoadBomLinkedOptionsAsync(string accessToken, CancellationToken ct)
    {
        var result = await manufacturingApiClient.GetBomsAsync(accessToken, new ManufacturingBomPagedRequest
        {
            Page = 1,
            PageSize = 1000,
            SortBy = "code",
            SortDirection = "asc"
        }, ct);

        return (result?.Items ?? [])
            .Select(x => new ManufacturingLinkedOptionViewModel
            {
                Id = x.Id,
                ItemId = x.ItemId,
                Label = string.IsNullOrWhiteSpace(x.ItemCode)
                    ? x.Code
                    : $"{x.Code} ({x.ItemCode})"
            })
            .OrderBy(x => x.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task<IReadOnlyList<ManufacturingLinkedOptionViewModel>> LoadWorkOrderLinkedOptionsAsync(string accessToken, CancellationToken ct)
    {
        var result = await manufacturingApiClient.GetWorkOrdersAsync(accessToken, new ManufacturingWorkOrderPagedRequest
        {
            Page = 1,
            PageSize = 1000,
            SortBy = "code",
            SortDirection = "asc"
        }, ct);

        return (result?.Items ?? [])
            .Select(x => new ManufacturingLinkedOptionViewModel
            {
                Id = x.Id,
                ItemId = x.ItemId,
                Label = string.IsNullOrWhiteSpace(x.ItemCode)
                    ? x.Code
                    : $"{x.Code} ({x.ItemCode})"
            })
            .OrderBy(x => x.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static InventoryOptionDto ToInventoryOption(ManufacturingLinkedOptionViewModel option)
    {
        return new InventoryOptionDto
        {
            Id = option.Id,
            Label = option.Label
        };
    }
}
