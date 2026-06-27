using ERP.Application.DTOs.Manufacturing;
using ERP.Web.ViewModels.Manufacturing;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class ManufacturingController
{
    [HttpGet("work-orders/create")]
    public async Task<IActionResult> CreateWorkOrder(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var today = DateOnly.FromDateTime(DateTime.Today);

        ViewData["Title"] = "Create Work Order";
        ViewData["Breadcrumb"] = "Manufacturing / Work Orders / Create";

        var model = new ManufacturingWorkOrderEditViewModel
        {
            Status = ERP.Domain.Enums.Manufacturing.WorkOrderStatus.Draft,
            ProductionType = ERP.Domain.Enums.Manufacturing.ProductionType.MakeToStock,
            PlannedStartDate = today,
            PlannedEndDate = today,
            IsActive = true
        };

        await PopulateWorkOrderFormOptionsAsync(accessToken, model, ct);
        return View("WorkOrders/Create", model);
    }

    [HttpPost("work-orders/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateWorkOrder(ManufacturingWorkOrderEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        NormalizeWorkOrderForm(model);
        ValidateWorkOrderForm(model);

        await PopulateWorkOrderFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Work Order";
            ViewData["Breadcrumb"] = "Manufacturing / Work Orders / Create";
            return View("WorkOrders/Create", model);
        }

        var created = await manufacturingApiClient.CreateWorkOrderAsync(accessToken, MapWorkOrderRequest(model), ct);
        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to create work order." : created.ErrorMessage);
            ViewData["Title"] = "Create Work Order";
            ViewData["Breadcrumb"] = "Manufacturing / Work Orders / Create";
            return View("WorkOrders/Create", model);
        }

        TempData["SuccessMessage"] = "Work order created.";
        return RedirectToAction(nameof(WorkOrders));
    }

    [HttpGet("work-orders/edit/{id:int}")]
    public async Task<IActionResult> EditWorkOrder(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await manufacturingApiClient.GetWorkOrderByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Edit Work Order";
        ViewData["Breadcrumb"] = "Manufacturing / Work Orders / Edit";

        var model = new ManufacturingWorkOrderEditViewModel
        {
            Id = item.Id,
            Code = item.Code,
            ItemId = item.ItemId ?? 0,
            BomId = item.BomId ?? 0,
            RoutingId = item.RoutingId,
            WorkCenterId = item.WorkCenterId,
            MrpRunId = item.MrpRunId,
            Status = item.Status,
            ProductionType = item.ProductionType,
            QtyPlanned = item.QtyPlanned,
            QtyGood = item.QtyGood,
            QtyScrap = item.QtyScrap,
            PlannedStartDate = item.PlannedStartDate,
            PlannedEndDate = item.PlannedEndDate,
            ActualStartAt = item.ActualStartAt,
            ActualEndAt = item.ActualEndAt,
            StandardCostTotal = item.StandardCostTotal,
            ActualCostTotal = item.ActualCostTotal,
            IsActive = item.IsActive,
            Notes = item.Notes
        };

        await PopulateWorkOrderFormOptionsAsync(accessToken, model, ct);
        return View("WorkOrders/Edit", model);
    }

    [HttpPost("work-orders/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditWorkOrder(int id, ManufacturingWorkOrderEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        NormalizeWorkOrderForm(model);
        ValidateWorkOrderForm(model);

        await PopulateWorkOrderFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Work Order";
            ViewData["Breadcrumb"] = "Manufacturing / Work Orders / Edit";
            return View("WorkOrders/Edit", model);
        }

        var updated = await manufacturingApiClient.UpdateWorkOrderAsync(accessToken, id, MapWorkOrderRequest(model), ct);
        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update work order." : updated.ErrorMessage);
            ViewData["Title"] = "Edit Work Order";
            ViewData["Breadcrumb"] = "Manufacturing / Work Orders / Edit";
            return View("WorkOrders/Edit", model);
        }

        TempData["SuccessMessage"] = "Work order updated.";
        return RedirectToAction(nameof(WorkOrders));
    }

    [HttpGet("mrp/create")]
    public IActionResult CreateMrpRun()
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        ViewData["Title"] = "Create MRP Run";
        ViewData["Breadcrumb"] = "Manufacturing / MRP / Create";

        return View("Mrp/Create", new ManufacturingMrpRunEditViewModel
        {
            Status = ERP.Domain.Enums.Manufacturing.MrpStatus.Draft,
            RunDate = DateOnly.FromDateTime(DateTime.Today),
            HorizonDays = 30
        });
    }

    [HttpPost("mrp/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateMrpRun(ManufacturingMrpRunEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        NormalizeMrpRunForm(model);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create MRP Run";
            ViewData["Breadcrumb"] = "Manufacturing / MRP / Create";
            return View("Mrp/Create", model);
        }

        var created = await manufacturingApiClient.CreateMrpRunAsync(accessToken, MapMrpRunRequest(model), ct);
        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to create MRP run." : created.ErrorMessage);
            ViewData["Title"] = "Create MRP Run";
            ViewData["Breadcrumb"] = "Manufacturing / MRP / Create";
            return View("Mrp/Create", model);
        }

        TempData["SuccessMessage"] = "MRP run created.";
        return RedirectToAction(nameof(Mrp));
    }

    [HttpGet("mrp/edit/{id:int}")]
    public async Task<IActionResult> EditMrpRun(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await manufacturingApiClient.GetMrpRunByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Edit MRP Run";
        ViewData["Breadcrumb"] = "Manufacturing / MRP / Edit";

        return View("Mrp/Edit", new ManufacturingMrpRunEditViewModel
        {
            Id = item.Id,
            Code = item.Code,
            RunDate = item.RunDate,
            Status = item.Status,
            HorizonDays = item.HorizonDays,
            TotalDemandItems = item.TotalDemandItems,
            RecommendedWoCount = item.RecommendedWoCount,
            RecommendedPrCount = item.RecommendedPrCount,
            StartedAt = item.StartedAt,
            CompletedAt = item.CompletedAt,
            Notes = item.Notes
        });
    }

    [HttpPost("mrp/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditMrpRun(int id, ManufacturingMrpRunEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        NormalizeMrpRunForm(model);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit MRP Run";
            ViewData["Breadcrumb"] = "Manufacturing / MRP / Edit";
            return View("Mrp/Edit", model);
        }

        var updated = await manufacturingApiClient.UpdateMrpRunAsync(accessToken, id, MapMrpRunRequest(model), ct);
        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update MRP run." : updated.ErrorMessage);
            ViewData["Title"] = "Edit MRP Run";
            ViewData["Breadcrumb"] = "Manufacturing / MRP / Edit";
            return View("Mrp/Edit", model);
        }

        TempData["SuccessMessage"] = "MRP run updated.";
        return RedirectToAction(nameof(Mrp));
    }

    [HttpGet("qc/create")]
    public async Task<IActionResult> CreateQcInspection(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        ViewData["Title"] = "Create QC Inspection";
        ViewData["Breadcrumb"] = "Manufacturing / QC / Create";

        var model = new ManufacturingQcInspectionEditViewModel
        {
            Status = ERP.Domain.Enums.Manufacturing.QcStatus.Pending,
            Result = ERP.Domain.Enums.Manufacturing.QcResult.Pass,
            InspectedAt = DateTimeOffset.Now
        };

        await PopulateQcInspectionFormOptionsAsync(accessToken, model, ct);
        return View("Qc/Create", model);
    }

    [HttpPost("qc/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateQcInspection(ManufacturingQcInspectionEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        NormalizeQcInspectionForm(model);

        await PopulateQcInspectionFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create QC Inspection";
            ViewData["Breadcrumb"] = "Manufacturing / QC / Create";
            return View("Qc/Create", model);
        }

        var created = await manufacturingApiClient.CreateQcInspectionAsync(accessToken, MapQcInspectionRequest(model), ct);
        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to create QC inspection." : created.ErrorMessage);
            ViewData["Title"] = "Create QC Inspection";
            ViewData["Breadcrumb"] = "Manufacturing / QC / Create";
            return View("Qc/Create", model);
        }

        TempData["SuccessMessage"] = "QC inspection created.";
        return RedirectToAction(nameof(Qc));
    }

    [HttpGet("qc/edit/{id:int}")]
    public async Task<IActionResult> EditQcInspection(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await manufacturingApiClient.GetQcInspectionByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Edit QC Inspection";
        ViewData["Breadcrumb"] = "Manufacturing / QC / Edit";

        var model = new ManufacturingQcInspectionEditViewModel
        {
            Id = item.Id,
            Code = item.Code,
            WorkOrderId = item.WorkOrderId,
            ItemId = item.ItemId,
            InspectorEmployeeId = item.InspectorEmployeeId,
            InspectedAt = item.InspectedAt,
            Status = item.Status,
            Result = item.Result,
            Notes = item.Notes
        };

        await PopulateQcInspectionFormOptionsAsync(accessToken, model, ct);
        return View("Qc/Edit", model);
    }

    [HttpPost("qc/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditQcInspection(int id, ManufacturingQcInspectionEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        NormalizeQcInspectionForm(model);

        await PopulateQcInspectionFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit QC Inspection";
            ViewData["Breadcrumb"] = "Manufacturing / QC / Edit";
            return View("Qc/Edit", model);
        }

        var updated = await manufacturingApiClient.UpdateQcInspectionAsync(accessToken, id, MapQcInspectionRequest(model), ct);
        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update QC inspection." : updated.ErrorMessage);
            ViewData["Title"] = "Edit QC Inspection";
            ViewData["Breadcrumb"] = "Manufacturing / QC / Edit";
            return View("Qc/Edit", model);
        }

        TempData["SuccessMessage"] = "QC inspection updated.";
        return RedirectToAction(nameof(Qc));
    }

    [HttpGet("scrap/create")]
    public async Task<IActionResult> CreateScrap(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        ViewData["Title"] = "Create Scrap Record";
        ViewData["Breadcrumb"] = "Manufacturing / Scrap / Create";

        var model = new ManufacturingScrapRecordEditViewModel
        {
            Reason = ERP.Domain.Enums.Manufacturing.ScrapReason.Other,
            RecordedAt = DateTimeOffset.Now
        };

        await PopulateScrapFormOptionsAsync(accessToken, model, ct);
        return View("Scrap/Create", model);
    }

    [HttpPost("scrap/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateScrap(ManufacturingScrapRecordEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        NormalizeScrapForm(model);

        await PopulateScrapFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Scrap Record";
            ViewData["Breadcrumb"] = "Manufacturing / Scrap / Create";
            return View("Scrap/Create", model);
        }

        var created = await manufacturingApiClient.CreateScrapRecordAsync(accessToken, MapScrapRequest(model), ct);
        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to create scrap record." : created.ErrorMessage);
            ViewData["Title"] = "Create Scrap Record";
            ViewData["Breadcrumb"] = "Manufacturing / Scrap / Create";
            return View("Scrap/Create", model);
        }

        TempData["SuccessMessage"] = "Scrap record created.";
        return RedirectToAction(nameof(Scrap));
    }

    [HttpGet("scrap/edit/{id:int}")]
    public async Task<IActionResult> EditScrap(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await manufacturingApiClient.GetScrapRecordByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Edit Scrap Record";
        ViewData["Breadcrumb"] = "Manufacturing / Scrap / Edit";

        var model = new ManufacturingScrapRecordEditViewModel
        {
            Id = item.Id,
            Code = item.Code,
            WorkOrderId = item.WorkOrderId,
            ItemId = item.ItemId,
            WorkCenterId = item.WorkCenterId,
            Reason = item.Reason,
            QtyScrap = item.QtyScrap,
            UnitCost = item.UnitCost,
            RecordedAt = item.RecordedAt,
            Notes = item.Notes
        };

        await PopulateScrapFormOptionsAsync(accessToken, model, ct);
        return View("Scrap/Edit", model);
    }

    [HttpPost("scrap/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditScrap(int id, ManufacturingScrapRecordEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        NormalizeScrapForm(model);

        await PopulateScrapFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Scrap Record";
            ViewData["Breadcrumb"] = "Manufacturing / Scrap / Edit";
            return View("Scrap/Edit", model);
        }

        var updated = await manufacturingApiClient.UpdateScrapRecordAsync(accessToken, id, MapScrapRequest(model), ct);
        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update scrap record." : updated.ErrorMessage);
            ViewData["Title"] = "Edit Scrap Record";
            ViewData["Breadcrumb"] = "Manufacturing / Scrap / Edit";
            return View("Scrap/Edit", model);
        }

        TempData["SuccessMessage"] = "Scrap record updated.";
        return RedirectToAction(nameof(Scrap));
    }

    [HttpGet("rework/create")]
    public async Task<IActionResult> CreateRework(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        ViewData["Title"] = "Create Rework";
        ViewData["Breadcrumb"] = "Manufacturing / Rework / Create";

        var model = new ManufacturingReworkOrderEditViewModel
        {
            Status = ERP.Domain.Enums.Manufacturing.WorkOrderStatus.Draft,
            OpenedAt = DateTimeOffset.Now
        };

        await PopulateReworkFormOptionsAsync(accessToken, model, ct);
        return View("Rework/Create", model);
    }

    [HttpPost("rework/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRework(ManufacturingReworkOrderEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        NormalizeReworkForm(model);

        await PopulateReworkFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Rework";
            ViewData["Breadcrumb"] = "Manufacturing / Rework / Create";
            return View("Rework/Create", model);
        }

        var created = await manufacturingApiClient.CreateReworkOrderAsync(accessToken, MapReworkRequest(model), ct);
        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to create rework." : created.ErrorMessage);
            ViewData["Title"] = "Create Rework";
            ViewData["Breadcrumb"] = "Manufacturing / Rework / Create";
            return View("Rework/Create", model);
        }

        TempData["SuccessMessage"] = "Rework created.";
        return RedirectToAction(nameof(Rework));
    }

    [HttpGet("rework/edit/{id:int}")]
    public async Task<IActionResult> EditRework(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await manufacturingApiClient.GetReworkOrderByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Edit Rework";
        ViewData["Breadcrumb"] = "Manufacturing / Rework / Edit";

        var model = new ManufacturingReworkOrderEditViewModel
        {
            Id = item.Id,
            Code = item.Code,
            SourceWorkOrderId = item.SourceWorkOrderId,
            WorkOrderId = item.WorkOrderId,
            ItemId = item.ItemId,
            QtyRework = item.QtyRework,
            Status = item.Status,
            OpenedAt = item.OpenedAt,
            ClosedAt = item.ClosedAt,
            Notes = item.Notes
        };

        await PopulateReworkFormOptionsAsync(accessToken, model, ct);
        return View("Rework/Edit", model);
    }

    [HttpPost("rework/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRework(int id, ManufacturingReworkOrderEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        NormalizeReworkForm(model);

        await PopulateReworkFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Rework";
            ViewData["Breadcrumb"] = "Manufacturing / Rework / Edit";
            return View("Rework/Edit", model);
        }

        var updated = await manufacturingApiClient.UpdateReworkOrderAsync(accessToken, id, MapReworkRequest(model), ct);
        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update rework." : updated.ErrorMessage);
            ViewData["Title"] = "Edit Rework";
            ViewData["Breadcrumb"] = "Manufacturing / Rework / Edit";
            return View("Rework/Edit", model);
        }

        TempData["SuccessMessage"] = "Rework updated.";
        return RedirectToAction(nameof(Rework));
    }

    private void ValidateWorkOrderForm(ManufacturingWorkOrderEditViewModel model)
    {
        if (model.PlannedEndDate < model.PlannedStartDate)
        {
            ModelState.AddModelError(nameof(model.PlannedEndDate), "Planned end date must be greater than or equal to planned start date.");
        }
    }

    private static void NormalizeWorkOrderForm(ManufacturingWorkOrderEditViewModel model)
    {
        model.Code = NormalizeText(model.Code) ?? string.Empty;
        model.Notes = NormalizeText(model.Notes);

        if (model.RoutingId.GetValueOrDefault() <= 0)
        {
            model.RoutingId = null;
        }

        if (model.WorkCenterId.GetValueOrDefault() <= 0)
        {
            model.WorkCenterId = null;
        }

        if (model.MrpRunId.GetValueOrDefault() <= 0)
        {
            model.MrpRunId = null;
        }
    }

    private static void NormalizeMrpRunForm(ManufacturingMrpRunEditViewModel model)
    {
        model.Code = NormalizeText(model.Code) ?? string.Empty;
        model.Notes = NormalizeText(model.Notes);
    }

    private static void NormalizeQcInspectionForm(ManufacturingQcInspectionEditViewModel model)
    {
        model.Code = NormalizeText(model.Code) ?? string.Empty;
        model.Notes = NormalizeText(model.Notes);

        if (model.WorkOrderId.GetValueOrDefault() <= 0)
        {
            model.WorkOrderId = null;
        }

        if (model.ItemId.GetValueOrDefault() <= 0)
        {
            model.ItemId = null;
        }

        if (model.InspectorEmployeeId.GetValueOrDefault() <= 0)
        {
            model.InspectorEmployeeId = null;
        }
    }

    private static void NormalizeScrapForm(ManufacturingScrapRecordEditViewModel model)
    {
        model.Code = NormalizeText(model.Code) ?? string.Empty;
        model.Notes = NormalizeText(model.Notes);

        if (model.WorkOrderId.GetValueOrDefault() <= 0)
        {
            model.WorkOrderId = null;
        }

        if (model.ItemId.GetValueOrDefault() <= 0)
        {
            model.ItemId = null;
        }

        if (model.WorkCenterId.GetValueOrDefault() <= 0)
        {
            model.WorkCenterId = null;
        }
    }

    private static void NormalizeReworkForm(ManufacturingReworkOrderEditViewModel model)
    {
        model.Code = NormalizeText(model.Code) ?? string.Empty;
        model.Notes = NormalizeText(model.Notes);

        if (model.SourceWorkOrderId.GetValueOrDefault() <= 0)
        {
            model.SourceWorkOrderId = null;
        }

        if (model.WorkOrderId.GetValueOrDefault() <= 0)
        {
            model.WorkOrderId = null;
        }

        if (model.ItemId.GetValueOrDefault() <= 0)
        {
            model.ItemId = null;
        }
    }

    private static ManufacturingWorkOrderDto MapWorkOrderRequest(ManufacturingWorkOrderEditViewModel model)
    {
        return new ManufacturingWorkOrderDto
        {
            Id = model.Id ?? 0,
            Code = model.Code,
            ItemId = model.ItemId,
            BomId = model.BomId,
            RoutingId = model.RoutingId,
            WorkCenterId = model.WorkCenterId,
            MrpRunId = model.MrpRunId,
            Status = model.Status,
            ProductionType = model.ProductionType,
            QtyPlanned = model.QtyPlanned,
            QtyGood = model.QtyGood,
            QtyScrap = model.QtyScrap,
            PlannedStartDate = model.PlannedStartDate,
            PlannedEndDate = model.PlannedEndDate,
            ActualStartAt = model.ActualStartAt,
            ActualEndAt = model.ActualEndAt,
            StandardCostTotal = model.StandardCostTotal,
            ActualCostTotal = model.ActualCostTotal,
            IsActive = model.IsActive,
            Notes = model.Notes
        };
    }

    private static ManufacturingMrpRunDto MapMrpRunRequest(ManufacturingMrpRunEditViewModel model)
    {
        return new ManufacturingMrpRunDto
        {
            Id = model.Id ?? 0,
            Code = model.Code,
            RunDate = model.RunDate,
            Status = model.Status,
            HorizonDays = model.HorizonDays,
            TotalDemandItems = model.TotalDemandItems,
            RecommendedWoCount = model.RecommendedWoCount,
            RecommendedPrCount = model.RecommendedPrCount,
            StartedAt = model.StartedAt,
            CompletedAt = model.CompletedAt,
            Notes = model.Notes
        };
    }

    private static ManufacturingQcInspectionDto MapQcInspectionRequest(ManufacturingQcInspectionEditViewModel model)
    {
        return new ManufacturingQcInspectionDto
        {
            Id = model.Id ?? 0,
            Code = model.Code,
            WorkOrderId = model.WorkOrderId,
            ItemId = model.ItemId,
            InspectorEmployeeId = model.InspectorEmployeeId,
            InspectedAt = model.InspectedAt,
            Status = model.Status,
            Result = model.Result,
            Notes = model.Notes
        };
    }

    private static ManufacturingScrapRecordDto MapScrapRequest(ManufacturingScrapRecordEditViewModel model)
    {
        return new ManufacturingScrapRecordDto
        {
            Id = model.Id ?? 0,
            Code = model.Code,
            WorkOrderId = model.WorkOrderId,
            ItemId = model.ItemId,
            WorkCenterId = model.WorkCenterId,
            Reason = model.Reason,
            QtyScrap = model.QtyScrap,
            UnitCost = model.UnitCost,
            RecordedAt = model.RecordedAt,
            Notes = model.Notes
        };
    }

    private static ManufacturingReworkOrderDto MapReworkRequest(ManufacturingReworkOrderEditViewModel model)
    {
        return new ManufacturingReworkOrderDto
        {
            Id = model.Id ?? 0,
            Code = model.Code,
            SourceWorkOrderId = model.SourceWorkOrderId,
            WorkOrderId = model.WorkOrderId,
            ItemId = model.ItemId,
            QtyRework = model.QtyRework,
            Status = model.Status,
            OpenedAt = model.OpenedAt,
            ClosedAt = model.ClosedAt,
            Notes = model.Notes
        };
    }
}


