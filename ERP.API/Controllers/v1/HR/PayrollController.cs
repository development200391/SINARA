using ERP.Application.DTOs.HR;
using ERP.Application.Services.HR;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers.v1.HR;

[Route("api/v1/hr/payroll")]
public sealed class PayrollController(IPayrollService payrollService) : HrControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetRuns([FromQuery] PayrollRunPagedRequest request, CancellationToken ct)
    {
        var runs = await payrollService.GetRunsAsync(request, ct);
        return Ok(runs);
    }

    [HttpPost("run")]
    public async Task<IActionResult> Run([FromBody] PayrollRunRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var run = await payrollService.RunPayrollAsync(request.Month, request.Year, userId.Value, ct);
            return Ok(run);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{runId:int}/details")]
    public async Task<IActionResult> GetDetails(int runId, CancellationToken ct)
    {
        var details = await payrollService.GetRunDetailsAsync(runId, ct);
        return Ok(details);
    }

    [HttpGet("{runId:int}/payslip/{employeeId:int}")]
    public async Task<IActionResult> GetPayslip(int runId, int employeeId, CancellationToken ct)
    {
        var payslip = await payrollService.GetPayslipAsync(runId, employeeId, ct);
        return payslip is null ? NotFound() : Ok(payslip);
    }
}
