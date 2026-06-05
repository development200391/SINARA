using System.Security.Claims;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;
using ERP.Domain.Enums;
using ERP.Web.Services;
using ERP.Web.ViewModels.HR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[Authorize]
[Route("hr/leave/requests")]
public sealed class HrLeaveRequestsController(IHrApiClient hrApiClient) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = 20,
        string? search = null,
        LeaveStatus? status = null,
        CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path + Request.QueryString });
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);

        var result = await hrApiClient.GetLeaveRequestsAsync(accessToken, new LeaveRequestPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            Status = status
        }, ct);

        ViewData["Title"] = "Leave Requests";
        ViewData["Breadcrumb"] = "HR / Leave / Requests";

        return View(new HrLeaveRequestsIndexViewModel
        {
            Search = search,
            Status = status,
            PageSize = normalizedPageSize,
            Requests = result ?? PagedResult<LeaveRequestDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("details/{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var leaveRequest = await hrApiClient.GetLeaveRequestByIdAsync(accessToken, id, ct);
        if (leaveRequest is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Leave Request Details";
        ViewData["Breadcrumb"] = "HR / Leave / Requests / Details";

        return View(leaveRequest);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var options = await hrApiClient.GetLeaveRequestOptionsAsync(accessToken, ct) ?? new LeaveRequestOptionsDto();

        ViewData["Title"] = "Create Leave Request";
        ViewData["Breadcrumb"] = "HR / Leave / Requests / Create";

        return View(new HrLeaveRequestCreateViewModel
        {
            Employees = options.Employees,
            LeaveTypes = options.LeaveTypes
        });
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HrLeaveRequestCreateViewModel model, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var options = await hrApiClient.GetLeaveRequestOptionsAsync(accessToken, ct) ?? new LeaveRequestOptionsDto();
        model.Employees = options.Employees;
        model.LeaveTypes = options.LeaveTypes;

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Leave Request";
            ViewData["Breadcrumb"] = "HR / Leave / Requests / Create";
            return View(model);
        }

        var created = await hrApiClient.SubmitLeaveRequestAsync(accessToken, new SubmitLeaveRequest
        {
            EmployeeId = model.EmployeeId,
            LeaveTypeId = model.LeaveTypeId,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            Reason = model.Reason
        }, ct);

        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to submit leave request." : created.ErrorMessage);
            ViewData["Title"] = "Create Leave Request";
            ViewData["Breadcrumb"] = "HR / Leave / Requests / Create";
            return View(model);
        }

        TempData["SuccessMessage"] = "Leave request submitted.";
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

        var leaveRequest = await hrApiClient.GetLeaveRequestByIdAsync(accessToken, id, ct);
        if (leaveRequest is null)
        {
            return NotFound();
        }

        if (leaveRequest.Status != LeaveStatus.Pending)
        {
            TempData["ErrorMessage"] = "Only pending leave request can be edited.";
            return RedirectToAction(nameof(Index));
        }

        var options = await hrApiClient.GetLeaveRequestOptionsAsync(accessToken, ct) ?? new LeaveRequestOptionsDto();

        ViewData["Title"] = "Edit Leave Request";
        ViewData["Breadcrumb"] = "HR / Leave / Requests / Edit";

        return View(new HrLeaveRequestEditViewModel
        {
            Id = leaveRequest.Id,
            EmployeeId = leaveRequest.EmployeeId,
            LeaveTypeId = leaveRequest.LeaveTypeId,
            StartDate = leaveRequest.StartDate,
            EndDate = leaveRequest.EndDate,
            Reason = leaveRequest.Reason,
            Employees = options.Employees,
            LeaveTypes = options.LeaveTypes
        });
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, HrLeaveRequestEditViewModel model, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        model.Id = id;

        var options = await hrApiClient.GetLeaveRequestOptionsAsync(accessToken, ct) ?? new LeaveRequestOptionsDto();
        model.Employees = options.Employees;
        model.LeaveTypes = options.LeaveTypes;

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Leave Request";
            ViewData["Breadcrumb"] = "HR / Leave / Requests / Edit";
            return View(model);
        }

        var updated = await hrApiClient.UpdateLeaveRequestAsync(accessToken, id, new SubmitLeaveRequest
        {
            EmployeeId = model.EmployeeId,
            LeaveTypeId = model.LeaveTypeId,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            Reason = model.Reason
        }, ct);

        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update leave request." : updated.ErrorMessage);
            ViewData["Title"] = "Edit Leave Request";
            ViewData["Breadcrumb"] = "HR / Leave / Requests / Edit";
            return View(model);
        }

        TempData["SuccessMessage"] = "Leave request updated.";
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

        var deleted = await hrApiClient.DeleteLeaveRequestAsync(accessToken, id, ct);
        TempData[deleted.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = deleted.IsSuccess ? "Leave request deleted." : (string.IsNullOrWhiteSpace(deleted.ErrorMessage) ? "Failed to delete leave request." : deleted.ErrorMessage);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:int}/approve")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var ok = await hrApiClient.ApproveLeaveRequestAsync(accessToken, id, ct);
        TempData[ok.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = ok.IsSuccess ? "Leave request approved." : (string.IsNullOrWhiteSpace(ok.ErrorMessage) ? "Failed to approve leave request." : ok.ErrorMessage);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:int}/reject")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var ok = await hrApiClient.RejectLeaveRequestAsync(accessToken, id, ct);
        TempData[ok.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = ok.IsSuccess ? "Leave request rejected." : (string.IsNullOrWhiteSpace(ok.ErrorMessage) ? "Failed to reject leave request." : ok.ErrorMessage);

        return RedirectToAction(nameof(Index));
    }

    private static int NormalizePageSize(int pageSize) => pageSize is 20 or 50 or 100 ? pageSize : 20;

    private string? GetAccessToken() => User.FindFirstValue("access_token");
}
