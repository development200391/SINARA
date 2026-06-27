using ERP.Application.DTOs.Manufacturing;
using ERP.Web.ViewModels.Manufacturing;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class ManufacturingController
{
    [HttpGet("work-centers/create")]
    public IActionResult CreateWorkCenter()
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        ViewData["Title"] = "Create Work Center";
        ViewData["Breadcrumb"] = "Manufacturing / Work Centers / Create";

        return View("WorkCenters/Create", new ManufacturingWorkCenterEditViewModel
        {
            IsActive = true
        });
    }

    [HttpPost("work-centers/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateWorkCenter(ManufacturingWorkCenterEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        NormalizeWorkCenterForm(model);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Work Center";
            ViewData["Breadcrumb"] = "Manufacturing / Work Centers / Create";
            return View("WorkCenters/Create", model);
        }

        var created = await manufacturingApiClient.CreateWorkCenterAsync(accessToken, MapWorkCenterRequest(model), ct);
        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to create work center." : created.ErrorMessage);
            ViewData["Title"] = "Create Work Center";
            ViewData["Breadcrumb"] = "Manufacturing / Work Centers / Create";
            return View("WorkCenters/Create", model);
        }

        TempData["SuccessMessage"] = "Work center created.";
        return RedirectToAction(nameof(WorkCenters));
    }

    [HttpGet("work-centers/edit/{id:int}")]
    public async Task<IActionResult> EditWorkCenter(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await manufacturingApiClient.GetWorkCenterByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Edit Work Center";
        ViewData["Breadcrumb"] = "Manufacturing / Work Centers / Edit";

        return View("WorkCenters/Edit", new ManufacturingWorkCenterEditViewModel
        {
            Id = item.Id,
            Code = item.Code,
            Name = item.Name,
            CapacityHoursPerDay = item.CapacityHoursPerDay,
            LaborCostPerHour = item.LaborCostPerHour,
            OverheadCostPerHour = item.OverheadCostPerHour,
            WipAccountId = item.WipAccountId,
            IsActive = item.IsActive,
            Notes = item.Notes
        });
    }

    [HttpPost("work-centers/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditWorkCenter(int id, ManufacturingWorkCenterEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        NormalizeWorkCenterForm(model);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Work Center";
            ViewData["Breadcrumb"] = "Manufacturing / Work Centers / Edit";
            return View("WorkCenters/Edit", model);
        }

        var updated = await manufacturingApiClient.UpdateWorkCenterAsync(accessToken, id, MapWorkCenterRequest(model), ct);
        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update work center." : updated.ErrorMessage);
            ViewData["Title"] = "Edit Work Center";
            ViewData["Breadcrumb"] = "Manufacturing / Work Centers / Edit";
            return View("WorkCenters/Edit", model);
        }

        TempData["SuccessMessage"] = "Work center updated.";
        return RedirectToAction(nameof(WorkCenters));
    }

    [HttpPost("work-centers/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteWorkCenter(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var deleted = await manufacturingApiClient.DeleteWorkCenterAsync(accessToken, id, ct);
        TempData[deleted.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = deleted.IsSuccess
            ? "Work center deleted."
            : (string.IsNullOrWhiteSpace(deleted.ErrorMessage) ? "Failed to delete work center." : deleted.ErrorMessage);

        return RedirectToAction(nameof(WorkCenters));
    }

    [HttpGet("routings/create")]
    public async Task<IActionResult> CreateRouting(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        ViewData["Title"] = "Create Routing";
        ViewData["Breadcrumb"] = "Manufacturing / Routings / Create";

        var model = new ManufacturingRoutingEditViewModel
        {
            Version = 1,
            IsActive = true
        };

        await PopulateRoutingFormOptionsAsync(accessToken, model, ct);
        return View("Routings/Create", model);
    }

    [HttpPost("routings/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRouting(ManufacturingRoutingEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        NormalizeRoutingForm(model);
        await PopulateRoutingFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Routing";
            ViewData["Breadcrumb"] = "Manufacturing / Routings / Create";
            return View("Routings/Create", model);
        }

        var created = await manufacturingApiClient.CreateRoutingAsync(accessToken, MapRoutingRequest(model), ct);
        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to create routing." : created.ErrorMessage);
            ViewData["Title"] = "Create Routing";
            ViewData["Breadcrumb"] = "Manufacturing / Routings / Create";
            return View("Routings/Create", model);
        }

        TempData["SuccessMessage"] = "Routing created.";
        return RedirectToAction(nameof(Routings));
    }

    [HttpGet("routings/edit/{id:int}")]
    public async Task<IActionResult> EditRouting(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await manufacturingApiClient.GetRoutingByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Edit Routing";
        ViewData["Breadcrumb"] = "Manufacturing / Routings / Edit";

        var model = new ManufacturingRoutingEditViewModel
        {
            Id = item.Id,
            Code = item.Code,
            Name = item.Name,
            ItemId = item.ItemId,
            WorkCenterId = item.WorkCenterId,
            Version = item.Version,
            Status = item.Status,
            TotalLeadTimeHours = item.TotalLeadTimeHours,
            IsActive = item.IsActive,
            Notes = item.Notes
        };

        await PopulateRoutingFormOptionsAsync(accessToken, model, ct);
        return View("Routings/Edit", model);
    }

    [HttpPost("routings/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRouting(int id, ManufacturingRoutingEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        NormalizeRoutingForm(model);
        await PopulateRoutingFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Routing";
            ViewData["Breadcrumb"] = "Manufacturing / Routings / Edit";
            return View("Routings/Edit", model);
        }

        var updated = await manufacturingApiClient.UpdateRoutingAsync(accessToken, id, MapRoutingRequest(model), ct);
        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update routing." : updated.ErrorMessage);
            ViewData["Title"] = "Edit Routing";
            ViewData["Breadcrumb"] = "Manufacturing / Routings / Edit";
            return View("Routings/Edit", model);
        }

        TempData["SuccessMessage"] = "Routing updated.";
        return RedirectToAction(nameof(Routings));
    }

    [HttpPost("routings/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRouting(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var deleted = await manufacturingApiClient.DeleteRoutingAsync(accessToken, id, ct);
        TempData[deleted.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = deleted.IsSuccess
            ? "Routing deleted."
            : (string.IsNullOrWhiteSpace(deleted.ErrorMessage) ? "Failed to delete routing." : deleted.ErrorMessage);

        return RedirectToAction(nameof(Routings));
    }

    [HttpGet("boms/create")]
    public async Task<IActionResult> CreateBom(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        ViewData["Title"] = "Create BOM";
        ViewData["Breadcrumb"] = "Manufacturing / BOMs / Create";

        var model = new ManufacturingBomEditViewModel
        {
            Version = 1,
            QtyProduced = 1m,
            EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
            IsActive = true
        };

        await PopulateBomFormOptionsAsync(accessToken, model, ct);
        return View("Boms/Create", model);
    }

    [HttpPost("boms/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBom(ManufacturingBomEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        NormalizeBomForm(model);
        await PopulateBomFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create BOM";
            ViewData["Breadcrumb"] = "Manufacturing / BOMs / Create";
            return View("Boms/Create", model);
        }

        var created = await manufacturingApiClient.CreateBomAsync(accessToken, MapBomRequest(model), ct);
        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to create BOM." : created.ErrorMessage);
            ViewData["Title"] = "Create BOM";
            ViewData["Breadcrumb"] = "Manufacturing / BOMs / Create";
            return View("Boms/Create", model);
        }

        TempData["SuccessMessage"] = "BOM created.";
        return RedirectToAction(nameof(Boms));
    }

    [HttpGet("boms/edit/{id:int}")]
    public async Task<IActionResult> EditBom(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await manufacturingApiClient.GetBomByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Edit BOM";
        ViewData["Breadcrumb"] = "Manufacturing / BOMs / Edit";

        var model = new ManufacturingBomEditViewModel
        {
            Id = item.Id,
            Code = item.Code,
            ItemId = item.ItemId ?? 0,
            RoutingId = item.RoutingId,
            Version = item.Version,
            Status = item.Status,
            QtyProduced = item.QtyProduced,
            StandardCost = item.StandardCost,
            EffectiveDate = item.EffectiveDate,
            IsActive = item.IsActive,
            Notes = item.Notes
        };

        await PopulateBomFormOptionsAsync(accessToken, model, ct);
        return View("Boms/Edit", model);
    }

    [HttpPost("boms/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditBom(int id, ManufacturingBomEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        NormalizeBomForm(model);
        await PopulateBomFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit BOM";
            ViewData["Breadcrumb"] = "Manufacturing / BOMs / Edit";
            return View("Boms/Edit", model);
        }

        var updated = await manufacturingApiClient.UpdateBomAsync(accessToken, id, MapBomRequest(model), ct);
        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update BOM." : updated.ErrorMessage);
            ViewData["Title"] = "Edit BOM";
            ViewData["Breadcrumb"] = "Manufacturing / BOMs / Edit";
            return View("Boms/Edit", model);
        }

        TempData["SuccessMessage"] = "BOM updated.";
        return RedirectToAction(nameof(Boms));
    }

    [HttpPost("boms/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBom(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var deleted = await manufacturingApiClient.DeleteBomAsync(accessToken, id, ct);
        TempData[deleted.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = deleted.IsSuccess
            ? "BOM deleted."
            : (string.IsNullOrWhiteSpace(deleted.ErrorMessage) ? "Failed to delete BOM." : deleted.ErrorMessage);

        return RedirectToAction(nameof(Boms));
    }

    [HttpGet("qc/parameters/create")]
    public async Task<IActionResult> CreateQcParameter(CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        ViewData["Title"] = "Create QC Parameter";
        ViewData["Breadcrumb"] = "Manufacturing / QC Parameters / Create";

        var model = new ManufacturingQcParameterEditViewModel
        {
            IsActive = true
        };

        await PopulateQcParameterFormOptionsAsync(accessToken, model, ct);
        return View("QcParameters/Create", model);
    }

    [HttpPost("qc/parameters/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateQcParameter(ManufacturingQcParameterEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        NormalizeQcParameterForm(model);
        await PopulateQcParameterFormOptionsAsync(accessToken, model, ct);

        if (model.MinValue.HasValue && model.MaxValue.HasValue && model.MinValue.Value > model.MaxValue.Value)
        {
            ModelState.AddModelError(nameof(model.MinValue), "Min value must be less than or equal to max value.");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create QC Parameter";
            ViewData["Breadcrumb"] = "Manufacturing / QC Parameters / Create";
            return View("QcParameters/Create", model);
        }

        var created = await manufacturingApiClient.CreateQcParameterAsync(accessToken, MapQcParameterRequest(model), ct);
        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to create QC parameter." : created.ErrorMessage);
            ViewData["Title"] = "Create QC Parameter";
            ViewData["Breadcrumb"] = "Manufacturing / QC Parameters / Create";
            return View("QcParameters/Create", model);
        }

        TempData["SuccessMessage"] = "QC parameter created.";
        return RedirectToAction(nameof(QcParameters));
    }

    [HttpGet("qc/parameters/edit/{id:int}")]
    public async Task<IActionResult> EditQcParameter(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var item = await manufacturingApiClient.GetQcParameterByIdAsync(accessToken, id, ct);
        if (item is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Edit QC Parameter";
        ViewData["Breadcrumb"] = "Manufacturing / QC Parameters / Edit";

        var model = new ManufacturingQcParameterEditViewModel
        {
            Id = item.Id,
            Code = item.Code,
            Name = item.Name,
            ItemId = item.ItemId,
            ParameterType = item.ParameterType,
            MinValue = item.MinValue,
            MaxValue = item.MaxValue,
            IsCritical = item.IsCritical,
            IsActive = item.IsActive,
            Notes = item.Notes
        };

        await PopulateQcParameterFormOptionsAsync(accessToken, model, ct);
        return View("QcParameters/Edit", model);
    }

    [HttpPost("qc/parameters/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditQcParameter(int id, ManufacturingQcParameterEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        model.Id = id;
        NormalizeQcParameterForm(model);
        await PopulateQcParameterFormOptionsAsync(accessToken, model, ct);

        if (model.MinValue.HasValue && model.MaxValue.HasValue && model.MinValue.Value > model.MaxValue.Value)
        {
            ModelState.AddModelError(nameof(model.MinValue), "Min value must be less than or equal to max value.");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit QC Parameter";
            ViewData["Breadcrumb"] = "Manufacturing / QC Parameters / Edit";
            return View("QcParameters/Edit", model);
        }

        var updated = await manufacturingApiClient.UpdateQcParameterAsync(accessToken, id, MapQcParameterRequest(model), ct);
        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update QC parameter." : updated.ErrorMessage);
            ViewData["Title"] = "Edit QC Parameter";
            ViewData["Breadcrumb"] = "Manufacturing / QC Parameters / Edit";
            return View("QcParameters/Edit", model);
        }

        TempData["SuccessMessage"] = "QC parameter updated.";
        return RedirectToAction(nameof(QcParameters));
    }

    [HttpPost("qc/parameters/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQcParameter(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var deleted = await manufacturingApiClient.DeleteQcParameterAsync(accessToken, id, ct);
        TempData[deleted.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = deleted.IsSuccess
            ? "QC parameter deleted."
            : (string.IsNullOrWhiteSpace(deleted.ErrorMessage) ? "Failed to delete QC parameter." : deleted.ErrorMessage);

        return RedirectToAction(nameof(QcParameters));
    }

    private static void NormalizeWorkCenterForm(ManufacturingWorkCenterEditViewModel model)
    {
        model.Code = NormalizeText(model.Code) ?? string.Empty;
        model.Name = NormalizeText(model.Name) ?? string.Empty;
        model.Notes = NormalizeText(model.Notes);
        if (model.WipAccountId.GetValueOrDefault() <= 0)
        {
            model.WipAccountId = null;
        }
    }

    private static void NormalizeRoutingForm(ManufacturingRoutingEditViewModel model)
    {
        model.Code = NormalizeText(model.Code) ?? string.Empty;
        model.Name = NormalizeText(model.Name) ?? string.Empty;
        model.Notes = NormalizeText(model.Notes);

        if (model.ItemId.GetValueOrDefault() <= 0)
        {
            model.ItemId = null;
        }

        if (model.WorkCenterId.GetValueOrDefault() <= 0)
        {
            model.WorkCenterId = null;
        }
    }

    private static void NormalizeBomForm(ManufacturingBomEditViewModel model)
    {
        model.Code = NormalizeText(model.Code) ?? string.Empty;
        model.Notes = NormalizeText(model.Notes);

        if (model.RoutingId.GetValueOrDefault() <= 0)
        {
            model.RoutingId = null;
        }
    }

    private static void NormalizeQcParameterForm(ManufacturingQcParameterEditViewModel model)
    {
        model.Code = NormalizeText(model.Code) ?? string.Empty;
        model.Name = NormalizeText(model.Name) ?? string.Empty;
        model.Notes = NormalizeText(model.Notes);

        if (model.ItemId.GetValueOrDefault() <= 0)
        {
            model.ItemId = null;
        }
    }

    private static ManufacturingWorkCenterDto MapWorkCenterRequest(ManufacturingWorkCenterEditViewModel model)
    {
        return new ManufacturingWorkCenterDto
        {
            Id = model.Id ?? 0,
            Code = model.Code,
            Name = model.Name,
            CapacityHoursPerDay = model.CapacityHoursPerDay,
            LaborCostPerHour = model.LaborCostPerHour,
            OverheadCostPerHour = model.OverheadCostPerHour,
            WipAccountId = model.WipAccountId,
            IsActive = model.IsActive,
            Notes = model.Notes
        };
    }

    private static ManufacturingRoutingDto MapRoutingRequest(ManufacturingRoutingEditViewModel model)
    {
        return new ManufacturingRoutingDto
        {
            Id = model.Id ?? 0,
            Code = model.Code,
            Name = model.Name,
            ItemId = model.ItemId,
            WorkCenterId = model.WorkCenterId,
            Version = model.Version,
            Status = model.Status,
            TotalLeadTimeHours = model.TotalLeadTimeHours,
            IsActive = model.IsActive,
            Notes = model.Notes
        };
    }

    private static ManufacturingBomDto MapBomRequest(ManufacturingBomEditViewModel model)
    {
        return new ManufacturingBomDto
        {
            Id = model.Id ?? 0,
            Code = model.Code,
            ItemId = model.ItemId,
            RoutingId = model.RoutingId,
            Version = model.Version,
            Status = model.Status,
            QtyProduced = model.QtyProduced,
            StandardCost = model.StandardCost,
            EffectiveDate = model.EffectiveDate,
            IsActive = model.IsActive,
            Notes = model.Notes
        };
    }

    private static ManufacturingQcParameterDto MapQcParameterRequest(ManufacturingQcParameterEditViewModel model)
    {
        return new ManufacturingQcParameterDto
        {
            Id = model.Id ?? 0,
            Code = model.Code,
            Name = model.Name,
            ItemId = model.ItemId,
            ParameterType = model.ParameterType,
            MinValue = model.MinValue,
            MaxValue = model.MaxValue,
            IsCritical = model.IsCritical,
            IsActive = model.IsActive,
            Notes = model.Notes
        };
    }
}


