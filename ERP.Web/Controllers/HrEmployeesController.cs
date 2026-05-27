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
[Route("hr/employees")]
public sealed class HrEmployeesController(IHrApiClient hrApiClient) : Controller
{
    private const int DefaultPageSize = 20;

    [HttpGet("")]
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        int? departmentId = null,
        EmploymentStatus? employmentStatus = null,
        CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path + Request.QueryString });
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);

        var employeesTask = hrApiClient.GetEmployeesAsync(accessToken, new EmployeePagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            DepartmentId = departmentId,
            EmploymentStatus = employmentStatus
        }, ct);

        var departmentsTask = hrApiClient.GetDepartmentOptionsAsync(accessToken, ct);

        await Task.WhenAll(employeesTask, departmentsTask);

        var employees = await employeesTask;
        var departments = await departmentsTask;

        ViewData["Title"] = "Employees";
        ViewData["Breadcrumb"] = "HR / Employees";

        return View(new HrEmployeesIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            DepartmentId = departmentId,
            EmploymentStatus = employmentStatus,
            Departments = departments,
            Employees = employees ?? PagedResult<EmployeeListDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    private static int NormalizePageSize(int pageSize) => pageSize is 20 or 50 or 100 ? pageSize : DefaultPageSize;

    private string? GetAccessToken() => User.FindFirstValue("access_token");
}
