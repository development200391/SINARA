using System.Security.Claims;
using ERP.Application.DTOs.HR;
using ERP.Web.Services;
using ERP.Web.ViewModels.HR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[Authorize]
[Route("hr/leave/balance")]
public sealed class HrLeaveBalanceController(IHrApiClient hrApiClient) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1, string? search = null, int? year = null, int? employeeId = null, int? leaveTypeId = null, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path + Request.QueryString });
        }

        var options = await hrApiClient.GetLeaveRequestOptionsAsync(accessToken, ct) ?? new LeaveRequestOptionsDto();

        var balances = await hrApiClient.GetLeaveBalancesAsync(accessToken, new LeaveBalanceRequest
        {
            Page = page,
            PageSize = 20,
            Search = search,
            Year = year,
            EmployeeId = employeeId,
            LeaveTypeId = leaveTypeId
        }, ct);

        ViewData["Title"] = "Leave Balance";
        ViewData["Breadcrumb"] = "HR / Leave / Balance";

        return View(new HrLeaveBalanceIndexViewModel
        {
            Search = search,
            Year = year,
            EmployeeId = employeeId,
            LeaveTypeId = leaveTypeId,
            Employees = options.Employees,
            LeaveTypes = options.LeaveTypes,
            Balances = balances ?? ERP.Application.DTOs.Common.PagedResult<LeaveBalanceDto>.Create([], 0, page, 20)
        });
    }

    private string? GetAccessToken() => User.FindFirstValue("access_token");
}
