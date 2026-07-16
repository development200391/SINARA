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
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = 20,
        string? search = null,
        string? sortBy = "employee",
        string? sortDirection = "asc",
        int? year = null,
        int? employeeId = null,
        int? leaveTypeId = null,
        CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path + Request.QueryString });
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy);
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);

        var options = await hrApiClient.GetLeaveRequestOptionsAsync(accessToken, ct: ct) ?? new LeaveRequestOptionsDto();

        var balances = await hrApiClient.GetLeaveBalancesAsync(accessToken, new LeaveBalanceRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Year = year,
            EmployeeId = employeeId,
            LeaveTypeId = leaveTypeId
        }, ct);

        ViewData["Title"] = "Leave Balance";
        ViewData["Breadcrumb"] = "HR / Leave / Balance";

        return View(new HrLeaveBalanceIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Year = year,
            EmployeeId = employeeId,
            LeaveTypeId = leaveTypeId,
            Employees = options.Employees,
            LeaveTypes = options.LeaveTypes,
            Balances = balances ?? ERP.Application.DTOs.Common.PagedResult<LeaveBalanceDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    private static int NormalizePageSize(int pageSize) => pageSize is 20 or 50 or 100 ? pageSize : 20;

    private static string NormalizeSortBy(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return "employee";
        }

        return sortBy.Trim().ToLowerInvariant() switch
        {
            "employee" => "employee",
            "leavetype" => "leaveType",
            "year" => "year",
            "maxdays" => "maxDays",
            "useddays" => "usedDays",
            "remainingdays" => "remainingDays",
            _ => "employee"
        };
    }

    private static string NormalizeSortDirection(string? sortDirection) =>
        string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";

    private string? GetAccessToken() => User.FindFirstValue("access_token");
}
