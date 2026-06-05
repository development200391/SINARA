using System.Security.Claims;
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
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = 20,
        string? search = null,
        string? sortBy = "name",
        string? sortDirection = "asc",
        string? name = null,
        string? code = null,
        int? maxDaysPerYearFrom = null,
        int? maxDaysPerYearTo = null,
        bool? isCarryOver = null,
        bool? isActive = null,
        CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path + Request.QueryString });
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy);
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedName = NormalizeText(name);
        var normalizedCode = NormalizeText(code);
        var (normalizedMaxDaysFrom, normalizedMaxDaysTo) = NormalizeIntRange(maxDaysPerYearFrom, maxDaysPerYearTo);

        var result = await hrApiClient.GetLeaveTypesAsync(accessToken, new LeaveTypePagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Name = normalizedName,
            Code = normalizedCode,
            MaxDaysPerYearFrom = normalizedMaxDaysFrom,
            MaxDaysPerYearTo = normalizedMaxDaysTo,
            IsCarryOver = isCarryOver,
            IsActive = isActive
        }, ct);

        ViewData["Title"] = "Leave Types";
        ViewData["Breadcrumb"] = "HR / Leave / Types";

        return View(new HrLeaveTypesIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            NameFilter = normalizedName,
            CodeFilter = normalizedCode,
            MaxDaysPerYearFromFilter = normalizedMaxDaysFrom,
            MaxDaysPerYearToFilter = normalizedMaxDaysTo,
            IsCarryOverFilter = isCarryOver,
            IsActiveFilter = isActive,
            LeaveTypes = result ?? ERP.Application.DTOs.Common.PagedResult<LeaveTypeDto>.Create([], 0, normalizedPage, normalizedPageSize)
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

        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to create leave type." : created.ErrorMessage);
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

        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update leave type." : updated.ErrorMessage);
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
        TempData[ok.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = ok.IsSuccess ? "Leave type deleted." : (string.IsNullOrWhiteSpace(ok.ErrorMessage) ? "Failed to delete leave type." : ok.ErrorMessage);

        return RedirectToAction(nameof(Index));
    }

    private static int NormalizePageSize(int pageSize) => pageSize is 20 or 50 or 100 ? pageSize : 20;

    private static string NormalizeSortBy(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return "name";
        }

        return sortBy.Trim().ToLowerInvariant() switch
        {
            "name" => "name",
            "code" => "code",
            "maxdays" => "maxDays",
            "iscarryover" => "isCarryOver",
            "isactive" => "isActive",
            _ => "name"
        };
    }

    private static string NormalizeSortDirection(string? sortDirection) =>
        string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";

    private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static (int? From, int? To) NormalizeIntRange(int? from, int? to)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
        {
            return (to, from);
        }

        return (from, to);
    }

    private string? GetAccessToken() => User.FindFirstValue("access_token");
}
