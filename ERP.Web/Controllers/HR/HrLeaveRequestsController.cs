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

    [HttpPost("{leaveRequestId:int}/documents")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadDocument(int leaveRequestId, IFormFile? file, int? categoryId, string? description, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        if (file is null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Please choose a file to upload.";
            return RedirectToAction(nameof(Edit), new { id = leaveRequestId });
        }

        var uploaded = await hrApiClient.UploadDocumentAsync(accessToken, file, DocumentReferenceType, leaveRequestId, categoryId, description, ct);
        TempData[uploaded.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = uploaded.IsSuccess
            ? "Document uploaded."
            : (string.IsNullOrWhiteSpace(uploaded.ErrorMessage) ? "Failed to upload document." : uploaded.ErrorMessage);

        return RedirectToAction(nameof(Edit), new { id = leaveRequestId });
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
        var categoriesTask = hrApiClient.GetDocumentCategoriesAsync(accessToken, ct);
        await Task.WhenAll(optionsTask, categoriesTask);
        var options = await optionsTask ?? new LeaveRequestOptionsDto();

        ViewData["Title"] = "Create Leave Request";
        ViewData["Breadcrumb"] = "HR / Leave / Requests / Create";

        return View(new HrLeaveRequestEditViewModel
        {
            Employees = options.Employees,
            LeaveTypes = options.LeaveTypes,
            AttachmentCategories = await categoriesTask
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
        model.AttachmentCategories = await hrApiClient.GetDocumentCategoriesAsync(accessToken, ct);

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

        if (model.AttachmentFile is not null && model.AttachmentFile.Length > 0)
        {
            var uploaded = await hrApiClient.UploadDocumentAsync(
                accessToken, model.AttachmentFile, DocumentReferenceType, created.Data!.Id, model.AttachmentCategoryId, model.AttachmentDescription, ct);

            if (uploaded.IsSuccess)
            {
                TempData["SuccessMessage"] = "Leave request submitted and attachment uploaded.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Leave request submitted, but the attachment failed to upload: {(string.IsNullOrWhiteSpace(uploaded.ErrorMessage) ? "unknown error." : uploaded.ErrorMessage)} You can retry from the Edit page.";
            }
        }
        else
        {
            TempData["SuccessMessage"] = "Leave request submitted.";
        }

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
        var categoriesTask = hrApiClient.GetDocumentCategoriesAsync(accessToken, ct);
        var documentsTask = hrApiClient.GetDocumentsAsync(accessToken, DocumentReferenceType, id, ct);
        await Task.WhenAll(optionsTask, categoriesTask, documentsTask);
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
            AttachmentCategories = await categoriesTask,
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
        model.AttachmentCategories = await hrApiClient.GetDocumentCategoriesAsync(accessToken, ct);
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
        }, ct);

        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update leave request." : updated.ErrorMessage);
            ViewData["Title"] = "Edit Leave Request";
            ViewData["Breadcrumb"] = "HR / Leave / Requests / Edit";
            return View(model);
        }

        if (model.AttachmentFile is not null && model.AttachmentFile.Length > 0)
        {
            var uploaded = await hrApiClient.UploadDocumentAsync(
                accessToken, model.AttachmentFile, DocumentReferenceType, id, model.AttachmentCategoryId, model.AttachmentDescription, ct);

            if (uploaded.IsSuccess)
            {
                TempData["SuccessMessage"] = "Leave request updated and attachment uploaded.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Leave request updated, but the attachment failed to upload: {(string.IsNullOrWhiteSpace(uploaded.ErrorMessage) ? "unknown error." : uploaded.ErrorMessage)}";
            }
        }
        else
        {
            TempData["SuccessMessage"] = "Leave request updated.";
        }

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
