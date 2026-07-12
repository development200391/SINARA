using ERP.Application.DTOs.Document;
using ERP.Application.Services.Document;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.v1.Document;

[Route("api/v1/documents")]
public sealed class DocumentsController(IDocumentService documentService) : DocumentControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetByReference([FromQuery] string referenceType, [FromQuery] int referenceId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var documents = await documentService.GetByReferenceAsync(referenceType, referenceId, userId.Value, ct);
            return Ok(documents);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("config")]
    public async Task<IActionResult> GetConfig([FromQuery] string referenceType, CancellationToken ct)
    {
        var config = await documentService.GetConfigAsync(referenceType, ct);
        return config is null ? NotFound() : Ok(config);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile file,
        [FromForm] string referenceType,
        [FromForm] int referenceId,
        [FromForm] string? description,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await documentService.UploadAsync(new UploadDocumentRequest
            {
                Content = stream,
                OriginalFileName = file.FileName,
                ContentType = file.ContentType,
                FileSizeBytes = file.Length,
                ReferenceType = referenceType,
                ReferenceId = referenceId,
                Description = description
            }, userId.Value, ct);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:int}/download")]
    public async Task<IActionResult> Download(int id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var result = await documentService.DownloadAsync(id, userId.Value, ct);
            return File(result.Content, result.ContentType, result.FileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { message = "Stored document file was not found." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var deleted = await documentService.DeleteAsync(id, userId.Value, ct);
            return deleted ? NoContent() : NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
