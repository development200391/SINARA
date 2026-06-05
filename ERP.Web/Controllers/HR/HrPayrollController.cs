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
[Route("hr/payroll")]
public sealed class HrPayrollController(IHrApiClient hrApiClient) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1, string? search = null, int? month = null, int? year = null, PayrollStatus? status = null, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path + Request.QueryString });
        }

        var runs = await hrApiClient.GetPayrollRunsAsync(accessToken, new PayrollRunPagedRequest
        {
            Page = page,
            PageSize = 20,
            Search = search,
            Month = month,
            Year = year,
            Status = status
        }, ct);

        ViewData["Title"] = "Payroll Runs";
        ViewData["Breadcrumb"] = "HR / Payroll";

        return View(new HrPayrollIndexViewModel
        {
            Search = search,
            Month = month,
            Year = year,
            Status = status,
            Runs = runs ?? PagedResult<PayrollRunDto>.Create([], 0, page, 20)
        });
    }

    [HttpGet("run")]
    public IActionResult Run()
    {
        ViewData["Title"] = "Run Payroll";
        ViewData["Breadcrumb"] = "HR / Payroll / Run";
        return View(new HrPayrollRunViewModel());
    }

    [HttpPost("run")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Run(HrPayrollRunViewModel model, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Run Payroll";
            ViewData["Breadcrumb"] = "HR / Payroll / Run";
            return View(model);
        }

        var run = await hrApiClient.RunPayrollAsync(accessToken, new PayrollRunRequest
        {
            Month = model.Month,
            Year = model.Year
        }, ct);

        if (!run.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(run.ErrorMessage) ? "Failed to run payroll." : run.ErrorMessage);
            ViewData["Title"] = "Run Payroll";
            ViewData["Breadcrumb"] = "HR / Payroll / Run";
            return View(model);
        }

        if (run.Data is null)
        {
            TempData["ErrorMessage"] = "Payroll completed, but run detail was not returned by API.";
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = "Payroll completed.";
        return RedirectToAction(nameof(Details), new { runId = run.Data.Id });
    }

    [HttpGet("{runId:int}/details")]
    public async Task<IActionResult> Details(int runId, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var details = await hrApiClient.GetPayrollRunDetailsAsync(accessToken, runId, ct);

        ViewData["Title"] = "Payroll Details";
        ViewData["Breadcrumb"] = "HR / Payroll / Details";

        return View(new HrPayrollDetailsViewModel
        {
            RunId = runId,
            Details = details
        });
    }

    [HttpGet("{runId:int}/payslip/{employeeId:int}")]
    public async Task<IActionResult> Payslip(int runId, int employeeId, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var payslip = await hrApiClient.GetPayslipAsync(accessToken, runId, employeeId, ct);
        if (payslip is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Payslip";
        ViewData["Breadcrumb"] = "HR / Payroll / Payslip";

        return View(new HrPayslipViewModel { Payslip = payslip });
    }

    private string? GetAccessToken() => User.FindFirstValue("access_token");
}
