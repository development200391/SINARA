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
    private const string DocumentReferenceType = "hr_leave_requests";

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

        return View(new HrLeaveRequestDetailsViewModel
        {
            Request = leaveRequest,
            Documents = await hrApiClient.GetDocumentsAsync(accessToken, DocumentReferenceType, id, ct)
        });
    }

    [HttpGet("documents/{id:int}/download")]
    public async Task<IActionResult> DownloadDocument(int id, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var download = await hrApiClient.DownloadDocumentAsync(accessToken, id, ct);
        if (download is null)
        {
            return NotFound();
        }

        return File(download.Content, download.ContentType, download.FileName);
    }

    [HttpPost("{leaveRequestId:int}/documents/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDocument(int leaveRequestId, int id, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var deleted = await hrApiClient.DeleteDocumentAsync(accessToken, id, ct);
        TempData[deleted.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = deleted.IsSuccess
            ? "Document deleted."
            : (string.IsNullOrWhiteSpace(deleted.ErrorMessage) ? "Failed to delete document." : deleted.ErrorMessage);

        return RedirectToAction(nameof(Edit), new { id = leaveRequestId });
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var optionsTask = hrApiClient.GetLeaveRequestOptionsAsync(accessToken, ct);
        var configTask = hrApiClient.GetDocumentConfigAsync(accessToken, DocumentReferenceType, ct);
        await Task.WhenAll(optionsTask, configTask);
        var options = await optionsTask ?? new LeaveRequestOptionsDto();

        ViewData["Title"] = "Create Leave Request";
        ViewData["Breadcrumb"] = "HR / Leave / Requests / Create";

        return View(new HrLeaveRequestEditViewModel
        {
            Employees = options.Employees,
            LeaveTypes = options.LeaveTypes,
            AttachmentConfig = await configTask
        });
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HrLeaveRequestEditViewModel model, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var options = await hrApiClient.GetLeaveRequestOptionsAsync(accessToken, ct) ?? new LeaveRequestOptionsDto();
        model.Employees = options.Employees;
        model.LeaveTypes = options.LeaveTypes;
        model.AttachmentConfig = await hrApiClient.GetDocumentConfigAsync(accessToken, DocumentReferenceType, ct);

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
        }, model.AttachmentFiles, model.AttachmentNote, ct);

        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to submit leave request." : created.ErrorMessage);
            ViewData["Title"] = "Create Leave Request";
            ViewData["Breadcrumb"] = "HR / Leave / Requests / Create";
            return View(model);
        }

        SetSubmitResultMessage(created.Data, "Leave request submitted.");
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

        var optionsTask = hrApiClient.GetLeaveRequestOptionsAsync(accessToken, ct);
        var configTask = hrApiClient.GetDocumentConfigAsync(accessToken, DocumentReferenceType, ct);
        var documentsTask = hrApiClient.GetDocumentsAsync(accessToken, DocumentReferenceType, id, ct);
        await Task.WhenAll(optionsTask, configTask, documentsTask);
        var options = await optionsTask ?? new LeaveRequestOptionsDto();

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
            LeaveTypes = options.LeaveTypes,
            AttachmentConfig = await configTask,
            Documents = await documentsTask
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
        model.AttachmentConfig = await hrApiClient.GetDocumentConfigAsync(accessToken, DocumentReferenceType, ct);
        model.Documents = await hrApiClient.GetDocumentsAsync(accessToken, DocumentReferenceType, id, ct);

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
        }, model.AttachmentFiles, model.AttachmentNote, ct);

        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update leave request." : updated.ErrorMessage);
            ViewData["Title"] = "Edit Leave Request";
            ViewData["Breadcrumb"] = "HR / Leave / Requests / Edit";
            return View(model);
        }

        SetSubmitResultMessage(updated.Data, "Leave request updated.");
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

    private void SetSubmitResultMessage(SubmitLeaveRequestResult? result, string baseMessage)
    {
        if (result is null || result.AttachmentWarnings.Count == 0)
        {
            TempData["SuccessMessage"] = baseMessage;
            return;
        }

        TempData["ErrorMessage"] = $"{baseMessage} Some attachments failed: {string.Join(" | ", result.AttachmentWarnings)}";
    }

    private static int NormalizePageSize(int pageSize) => pageSize is 20 or 50 or 100 ? pageSize : 20;

    private string? GetAccessToken() => User.FindFirstValue("access_token");
}
