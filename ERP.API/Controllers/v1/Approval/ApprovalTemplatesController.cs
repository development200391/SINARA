using ERP.Application.DTOs.Approval;
using ERP.Application.Services.Approval;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.v1.Approval;

[Route("api/v1/approval/templates")]
public sealed class ApprovalTemplatesController(IApprovalTemplateService templateService) : ApprovalControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ApprovalTemplatePagedRequest request, CancellationToken ct)
    {
        var result = await templateService.GetTemplatesPagedAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("options")]
    public async Task<IActionResult> GetOptions(CancellationToken ct)
    {
        var result = await templateService.GetTemplateOptionsAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var template = await templateService.GetTemplateByIdAsync(id, ct);
        return template is null ? NotFound() : Ok(template);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ApprovalTemplateDto request, CancellationToken ct)
    {
        try
        {
            var result = await templateService.CreateTemplateAsync(request, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ApprovalTemplateDto request, CancellationToken ct)
    {
        try
        {
            var result = await templateService.UpdateTemplateAsync(id, request, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/set-active")]
    public async Task<IActionResult> SetActive(int id, [FromBody] SetActiveRequestBody request, CancellationToken ct)
    {
        try
        {
            await templateService.SetTemplateActiveAsync(id, request.IsActive, ct);
            return Ok();
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
            await templateService.DeleteTemplateAsync(id, ct);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{templateId:int}/levels")]
    public async Task<IActionResult> GetLevels(int templateId, CancellationToken ct)
    {
        var result = await templateService.GetLevelsAsync(templateId, ct);
        return Ok(result);
    }

    [HttpGet("{templateId:int}/levels/{levelId:int}")]
    public async Task<IActionResult> GetLevelById(int templateId, int levelId, CancellationToken ct)
    {
        var level = await templateService.GetLevelByIdAsync(templateId, levelId, ct);
        return level is null ? NotFound() : Ok(level);
    }

    [HttpPost("{templateId:int}/levels")]
    public async Task<IActionResult> CreateLevel(int templateId, [FromBody] ApprovalLevelDto request, CancellationToken ct)
    {
        try
        {
            var result = await templateService.CreateLevelAsync(templateId, request, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{templateId:int}/levels/{levelId:int}")]
    public async Task<IActionResult> UpdateLevel(int templateId, int levelId, [FromBody] ApprovalLevelDto request, CancellationToken ct)
    {
        try
        {
            var result = await templateService.UpdateLevelAsync(templateId, levelId, request, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{templateId:int}/levels/{levelId:int}")]
    public async Task<IActionResult> DeleteLevel(int templateId, int levelId, CancellationToken ct)
    {
        try
        {
            await templateService.DeleteLevelAsync(templateId, levelId, ct);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    public sealed class SetActiveRequestBody
    {
        public bool IsActive { get; set; }
    }
}
