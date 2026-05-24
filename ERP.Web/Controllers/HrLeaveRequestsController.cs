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
    public async Task<IActionResult> Index(int page = 1, string? search = null, LeaveStatus? status = null, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path + Request.QueryString });
        }

        var result = await hrApiClient.GetLeaveRequestsAsync(accessToken, new LeaveRequestPagedRequest
        {
            Page = page,
            PageSize = 20,
            Search = search,
            Status = status
        }, ct);

        ViewData["Title"] = "Leave Requests";
        ViewData["Breadcrumb"] = "HR / Leave / Requests";

        return View(new HrLeaveRequestsIndexViewModel
        {
            Search = search,
            Status = status,
            Requests = result ?? PagedResult<LeaveRequestDto>.Create([], 0, page, 20)
        });
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

        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to submit leave request.");
            ViewData["Title"] = "Create Leave Request";
            ViewData["Breadcrumb"] = "HR / Leave / Requests / Create";
            return View(model);
        }

        TempData["SuccessMessage"] = "Leave request submitted.";
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
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Leave request approved." : "Failed to approve leave request.";

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
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Leave request rejected." : "Failed to reject leave request.";

        return RedirectToAction(nameof(Index));
    }

    private string? GetAccessToken() => User.FindFirstValue("access_token");
}
