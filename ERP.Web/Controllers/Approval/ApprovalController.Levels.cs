using ERP.Web.ViewModels.Approval;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class ApprovalController
{
    [HttpGet("templates/{templateId:int}/levels")]
    public async Task<IActionResult> TemplateLevels(int templateId, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var templateTask = approvalApiClient.GetTemplateByIdAsync(accessToken, templateId, ct);
        var levelsTask = approvalApiClient.GetLevelsAsync(accessToken, templateId, ct);

        await Task.WhenAll(templateTask, levelsTask);

        var template = await templateTask;
        if (template is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Approval Levels";
        ViewData["Breadcrumb"] = "Approval / Templates / Levels";

        return View("Levels/Index", new ApprovalLevelsIndexViewModel
        {
            TemplateId = templateId,
            TemplateCode = template.Code,
            TemplateName = template.Name,
            Items = await levelsTask
        });
    }

    [HttpGet("templates/{templateId:int}/levels/create")]
    public async Task<IActionResult> CreateLevel(int templateId, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var template = await approvalApiClient.GetTemplateByIdAsync(accessToken, templateId, ct);
        if (template is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Create Approval Level";
        ViewData["Breadcrumb"] = "Approval / Templates / Levels / Create";

        return View("Levels/Create", new ApprovalLevelEditViewModel
        {
            TemplateId = templateId,
            TemplateCode = template.Code,
            TemplateName = template.Name
        });
    }

    [HttpPost("templates/{templateId:int}/levels/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLevel(int templateId, ApprovalLevelEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var template = await approvalApiClient.GetTemplateByIdAsync(accessToken, templateId, ct);
        if (template is null)
        {
            return NotFound();
        }

        model.TemplateId = templateId;
        model.TemplateCode = template.Code;
        model.TemplateName = template.Name;

        ValidateLevelForm(model, ModelState);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Approval Level";
            ViewData["Breadcrumb"] = "Approval / Templates / Levels / Create";
            return View("Levels/Create", model);
        }

        var result = await approvalApiClient.CreateLevelAsync(accessToken, templateId, MapLevelDto(model), ct);
        if (!result.IsSuccess)
        {
            AddApiModelError(result, "Failed to create level.");
            ViewData["Title"] = "Create Approval Level";
            ViewData["Breadcrumb"] = "Approval / Templates / Levels / Create";
            return View("Levels/Create", model);
        }

        TempData["SuccessMessage"] = "Level created.";
        return RedirectToAction(nameof(TemplateLevels), new { templateId });
    }

    [HttpGet("templates/{templateId:int}/levels/edit/{id:int}")]
    public async Task<IActionResult> EditLevel(int templateId, int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var templateTask = approvalApiClient.GetTemplateByIdAsync(accessToken, templateId, ct);
        var levelTask = approvalApiClient.GetLevelByIdAsync(accessToken, templateId, id, ct);

        await Task.WhenAll(templateTask, levelTask);

        var template = await templateTask;
        var level = await levelTask;

        if (template is null || level is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Edit Approval Level";
        ViewData["Breadcrumb"] = "Approval / Templates / Levels / Edit";

        return View("Levels/Edit", new ApprovalLevelEditViewModel
        {
            Id = level.Id,
            TemplateId = templateId,
            TemplateCode = template.Code,
            TemplateName = template.Name,
            LevelOrder = level.LevelOrder,
            LevelName = level.LevelName,
            ApproverType = level.ApproverType,
            ApproverRoleId = level.ApproverRoleId,
            ApproverPositionId = level.ApproverPositionId,
            ApproverUserId = level.ApproverUserId,
            MinApproversRequired = level.MinApproversRequired,
            EscalationHours = level.EscalationHours,
            EscalateToLevelId = level.EscalateToLevelId,
            IsActive = level.IsActive
        });
    }

    [HttpPost("templates/{templateId:int}/levels/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditLevel(int templateId, int id, ApprovalLevelEditViewModel model, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var template = await approvalApiClient.GetTemplateByIdAsync(accessToken, templateId, ct);
        if (template is null)
        {
            return NotFound();
        }

        model.Id = id;
        model.TemplateId = templateId;
        model.TemplateCode = template.Code;
        model.TemplateName = template.Name;

        ValidateLevelForm(model, ModelState);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Approval Level";
            ViewData["Breadcrumb"] = "Approval / Templates / Levels / Edit";
            return View("Levels/Edit", model);
        }

        var result = await approvalApiClient.UpdateLevelAsync(accessToken, templateId, id, MapLevelDto(model), ct);
        if (!result.IsSuccess)
        {
            AddApiModelError(result, "Failed to update level.");
            ViewData["Title"] = "Edit Approval Level";
            ViewData["Breadcrumb"] = "Approval / Templates / Levels / Edit";
            return View("Levels/Edit", model);
        }

        TempData["SuccessMessage"] = "Level updated.";
        return RedirectToAction(nameof(TemplateLevels), new { templateId });
    }

    [HttpPost("templates/{templateId:int}/levels/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLevel(int templateId, int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var result = await approvalApiClient.DeleteLevelAsync(accessToken, templateId, id, ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? "Level deleted."
            : ResolveApiErrorMessage(result, "Failed to delete level.");

        return RedirectToAction(nameof(TemplateLevels), new { templateId });
    }
}
