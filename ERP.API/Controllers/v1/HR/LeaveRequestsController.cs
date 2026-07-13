using ERP.Application.DTOs.Document;
using ERP.Application.DTOs.HR;
using ERP.Application.Services.Document;
using ERP.Application.Services.HR;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.v1.HR;

[Route("api/v1/hr/leave-requests")]
public sealed class LeaveRequestsController(ILeaveService leaveService, IEmployeeService employeeService, IDocumentService documentService) : HrControllerBase
{
    private const string DocumentReferenceType = "hr_leave_requests";

    [HttpGet("self/leave-types")]
    public async Task<IActionResult> GetSelfLeaveTypes(CancellationToken ct)
    {
        var leaveTypes = await leaveService.GetLeaveTypeOptionsAsync(ct);
        return Ok(leaveTypes);
    }

    [HttpGet("self")]
    public async Task<IActionResult> GetSelf([FromQuery] LeaveRequestPagedRequest request, CancellationToken ct)
    {
        var employeeId = await ResolveEmployeeIdAsync(employeeService, ct);
        if (employeeId is null)
        {
            return BadRequest(new { message = "No employee profile is linked to this account." });
        }

        request.EmployeeId = employeeId.Value;
        var result = await leaveService.GetRequestsAsync(request, ct);
        return Ok(result);
    }

    [HttpPost("self")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(30 * 1024 * 1024)]
    public async Task<IActionResult> SubmitSelf([FromForm] SubmitSelfLeaveRequestForm form, CancellationToken ct)
    {
        var employeeId = await ResolveEmployeeIdAsync(employeeService, ct);
        if (employeeId is null)
        {
            return BadRequest(new { message = "No employee profile is linked to this account." });
        }

        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var validationError = await ValidateAttachmentRequirementAsync(form.Files, ct);
        if (validationError is not null)
        {
            return validationError;
        }

        try
        {
            var created = await leaveService.SubmitAsync(new SubmitLeaveRequest
            {
                EmployeeId = employeeId.Value,
                LeaveTypeId = form.LeaveTypeId,
                StartDate = form.StartDate,
                EndDate = form.EndDate,
                Reason = form.Reason
            }, ct);

            var warnings = await UploadAttachmentsAsync(form.Files, form.Notes, created.Id, userId.Value, ct);
            return Ok(new SubmitLeaveRequestResult { LeaveRequest = created, AttachmentWarnings = warnings });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] LeaveRequestPagedRequest request, CancellationToken ct)
    {
        var result = await leaveService.GetRequestsAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await leaveService.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("options")]
    public async Task<IActionResult> GetOptions(CancellationToken ct)
    {
        var employees = await leaveService.GetEmployeeOptionsAsync(ct);
        var leaveTypes = await leaveService.GetLeaveTypeOptionsAsync(ct);

        return Ok(new LeaveRequestOptionsDto
        {
            Employees = employees,
            LeaveTypes = leaveTypes
        });
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(30 * 1024 * 1024)]
    public async Task<IActionResult> Submit([FromForm] SubmitLeaveRequestForm form, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var validationError = await ValidateAttachmentRequirementAsync(form.Files, ct);
        if (validationError is not null)
        {
            return validationError;
        }

        try
        {
            var created = await leaveService.SubmitAsync(new SubmitLeaveRequest
            {
                EmployeeId = form.EmployeeId,
                LeaveTypeId = form.LeaveTypeId,
                StartDate = form.StartDate,
                EndDate = form.EndDate,
                Reason = form.Reason
            }, ct);

            var warnings = await UploadAttachmentsAsync(form.Files, form.Notes, created.Id, userId.Value, ct);
            return Ok(new SubmitLeaveRequestResult { LeaveRequest = created, AttachmentWarnings = warnings });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(30 * 1024 * 1024)]
    public async Task<IActionResult> Update(int id, [FromForm] SubmitLeaveRequestForm form, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var updated = await leaveService.UpdateAsync(id, new SubmitLeaveRequest
            {
                EmployeeId = form.EmployeeId,
                LeaveTypeId = form.LeaveTypeId,
                StartDate = form.StartDate,
                EndDate = form.EndDate,
                Reason = form.Reason
            }, ct);

            if (updated is null)
            {
                return NotFound();
            }

            var warnings = await ProcessAttachmentsAsync(form.Files, form.Notes, id, userId.Value, ct);
            return Ok(new SubmitLeaveRequestResult { LeaveRequest = updated, AttachmentWarnings = warnings });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            var deleted = await leaveService.DeleteAsync(id, ct);
            return deleted ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var approved = await leaveService.ApproveAsync(id, userId.Value, ct);
        return approved ? NoContent() : NotFound();
    }

    [HttpPut("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var rejected = await leaveService.RejectAsync(id, userId.Value, ct);
        return rejected ? NoContent() : NotFound();
    }

    private async Task<IActionResult?> ValidateAttachmentRequirementAsync(List<IFormFile>? files, CancellationToken ct)
    {
        var config = await documentService.GetConfigAsync(DocumentReferenceType, ct);
        if (config is null)
        {
            return null;
        }

        var fileCount = files?.Count(f => f.Length > 0) ?? 0;

        if (config.IsRequired && fileCount == 0)
        {
            return BadRequest(new { message = $"At least one attachment is required for {config.DisplayName}." });
        }

        if (fileCount > config.MaxFileCount)
        {
            return BadRequest(new { message = $"Maximum of {config.MaxFileCount} attachment(s) allowed." });
        }

        return null;
    }

    /// <summary>
    /// Create-time upload: the record was just made, so there are no existing
    /// documents to reconcile against yet — every non-empty file is a new
    /// upload. Notes are matched to files positionally (same slot order the
    /// Web form renders them in).
    /// </summary>
    private async Task<List<string>> UploadAttachmentsAsync(List<IFormFile>? files, List<string?>? notes, int leaveRequestId, int currentUserId, CancellationToken ct)
    {
        var warnings = new List<string>();
        if (files is null || files.Count == 0)
        {
            return warnings;
        }

        for (var i = 0; i < files.Count; i++)
        {
            var file = files[i];
            if (file.Length == 0)
            {
                continue;
            }

            try
            {
                await using var stream = file.OpenReadStream();
                await documentService.UploadAsync(new UploadDocumentRequest
                {
                    Content = stream,
                    OriginalFileName = file.FileName,
                    ContentType = file.ContentType,
                    FileSizeBytes = file.Length,
                    ReferenceType = DocumentReferenceType,
                    ReferenceId = leaveRequestId,
                    Description = notes?.ElementAtOrDefault(i)
                }, currentUserId, ct);
            }
            catch (Exception ex) when (ex is InvalidOperationException or UnauthorizedAccessException)
            {
                warnings.Add($"{file.FileName}: {ex.Message}");
            }
        }

        return warnings;
    }

    /// <summary>
    /// Edit-time upload: files/notes are positionally matched to slots the same
    /// way the Web form renders them. A slot with a newly-picked file uploads
    /// it as a new document; a slot with no new file but an existing document
    /// (matched by upload order — DocDocument has no slot column yet, see
    /// ReadMeDocumentGeneral.md) gets its note updated in place instead.
    /// </summary>
    private async Task<List<string>> ProcessAttachmentsAsync(List<IFormFile>? files, List<string?>? notes, int leaveRequestId, int currentUserId, CancellationToken ct)
    {
        var warnings = new List<string>();
        var slotCount = Math.Max(files?.Count ?? 0, notes?.Count ?? 0);
        if (slotCount == 0)
        {
            return warnings;
        }

        var existingBySlot = (await documentService.GetByReferenceAsync(DocumentReferenceType, leaveRequestId, currentUserId, ct))
            .OrderBy(d => d.UploadedAt)
            .ToList();

        for (var i = 0; i < slotCount; i++)
        {
            var file = files?.ElementAtOrDefault(i);
            var note = notes?.ElementAtOrDefault(i);

            if (file is not null && file.Length > 0)
            {
                try
                {
                    await using var stream = file.OpenReadStream();
                    await documentService.UploadAsync(new UploadDocumentRequest
                    {
                        Content = stream,
                        OriginalFileName = file.FileName,
                        ContentType = file.ContentType,
                        FileSizeBytes = file.Length,
                        ReferenceType = DocumentReferenceType,
                        ReferenceId = leaveRequestId,
                        Description = note
                    }, currentUserId, ct);
                }
                catch (Exception ex) when (ex is InvalidOperationException or UnauthorizedAccessException)
                {
                    warnings.Add($"{file.FileName}: {ex.Message}");
                }
                continue;
            }

            if (i >= existingBySlot.Count)
            {
                continue;
            }

            var existing = existingBySlot[i];
            if (string.Equals(existing.Description, note, StringComparison.Ordinal))
            {
                continue;
            }

            try
            {
                await documentService.UpdateDescriptionAsync(existing.Id, note, currentUserId, ct);
            }
            catch (Exception ex) when (ex is InvalidOperationException or UnauthorizedAccessException)
            {
                warnings.Add($"{existing.OriginalFileName}: {ex.Message}");
            }
        }

        return warnings;
    }

    public sealed class SubmitLeaveRequestForm
    {
        public int EmployeeId { get; set; }
        public int LeaveTypeId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string? Reason { get; set; }
        public List<string?>? Notes { get; set; }
        public List<IFormFile>? Files { get; set; }
    }

    public sealed class SubmitSelfLeaveRequestForm
    {
        public int LeaveTypeId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string? Reason { get; set; }
        public List<string?>? Notes { get; set; }
        public List<IFormFile>? Files { get; set; }
    }
}
