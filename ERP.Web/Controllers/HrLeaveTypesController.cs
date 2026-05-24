using System.Security.Claims;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;
using ERP.Web.Services;
using ERP.Web.ViewModels.HR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[Authorize]
[Route("hr/leave/types")]
public sealed class HrLeaveTypesController(IHrApiClient hrApiClient) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1, string? search = null, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path + Request.QueryString });
        }

        var result = await hrApiClient.GetLeaveTypesAsync(accessToken, new PagedRequest
        {
            Page = page,
            PageSize = 20,
            Search = search
        }, ct);

        ViewData["Title"] = "Leave Types";
        ViewData["Breadcrumb"] = "HR / Leave / Types";

        return View(new HrLeaveTypesIndexViewModel
        {
            Search = search,
            LeaveTypes = result ?? PagedResult<LeaveTypeDto>.Create([], 0, page, 20)
        });
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        ViewData["Title"] = "Create Leave Type";
        ViewData["Breadcrumb"] = "HR / Leave / Types / Create";
        return View(new HrLeaveTypeEditViewModel());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HrLeaveTypeEditViewModel model, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Leave Type";
            ViewData["Breadcrumb"] = "HR / Leave / Types / Create";
            return View(model);
        }

        var created = await hrApiClient.CreateLeaveTypeAsync(accessToken, new LeaveTypeDto
        {
            Name = model.Name,
            Code = model.Code,
            MaxDaysPerYear = model.MaxDaysPerYear,
            IsCarryOver = model.IsCarryOver,
            IsActive = model.IsActive
        }, ct);

        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create leave type.");
            ViewData["Title"] = "Create Leave Type";
            ViewData["Breadcrumb"] = "HR / Leave / Types / Create";
            return View(model);
        }

        TempData["SuccessMessage"] = "Leave type created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var leaveType = await hrApiClient.GetLeaveTypeByIdAsync(accessToken, id, ct);
        if (leaveType is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Edit Leave Type";
        ViewData["Breadcrumb"] = "HR / Leave / Types / Edit";

        return View(new HrLeaveTypeEditViewModel
        {
            Id = leaveType.Id,
            Name = leaveType.Name,
            Code = leaveType.Code,
            MaxDaysPerYear = leaveType.MaxDaysPerYear,
            IsCarryOver = leaveType.IsCarryOver,
            IsActive = leaveType.IsActive
        });
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, HrLeaveTypeEditViewModel model, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Leave Type";
            ViewData["Breadcrumb"] = "HR / Leave / Types / Edit";
            return View(model);
        }

        var updated = await hrApiClient.UpdateLeaveTypeAsync(accessToken, id, new LeaveTypeDto
        {
            Id = id,
            Name = model.Name,
            Code = model.Code,
            MaxDaysPerYear = model.MaxDaysPerYear,
            IsCarryOver = model.IsCarryOver,
            IsActive = model.IsActive
        }, ct);

        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to update leave type.");
            ViewData["Title"] = "Edit Leave Type";
            ViewData["Breadcrumb"] = "HR / Leave / Types / Edit";
            return View(model);
        }

        TempData["SuccessMessage"] = "Leave type updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var ok = await hrApiClient.DeleteLeaveTypeAsync(accessToken, id, ct);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Leave type deleted." : "Failed to delete leave type.";

        return RedirectToAction(nameof(Index));
    }

    private string? GetAccessToken() => User.FindFirstValue("access_token");
}
