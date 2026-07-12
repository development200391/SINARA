using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Document;
using ERP.Application.Services.Document;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.v1.Document;

[Route("api/v1/document-categories")]
public sealed class DocumentCategoriesController(IDocumentService documentService) : DocumentControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] PagedRequest request, CancellationToken ct)
    {
        var result = await documentService.GetCategoriesPagedAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await documentService.GetCategoryByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DocumentCategoryDto request, CancellationToken ct)
    {
        try
        {
            var created = await documentService.CreateCategoryAsync(request, ct);
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] DocumentCategoryDto request, CancellationToken ct)
    {
        try
        {
            var updated = await documentService.UpdateCategoryAsync(id, request, ct);
            return updated is null ? NotFound() : Ok(updated);
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
            var deleted = await documentService.DeleteCategoryAsync(id, ct);
            return deleted ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
