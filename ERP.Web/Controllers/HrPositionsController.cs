using System.Security.Claims;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;
using ERP.Web.Services;
using ERP.Web.ViewModels.HR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[Authorize]
[Route("hr/positions")]
[Route("hr/position")]
public sealed class HrPositionsController(IHrApiClient hrApiClient) : Controller
{
    private const int DefaultPageSize = 20;

    [HttpGet("")]
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        int? departmentId = null,
        bool? isActive = null,
        CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path + Request.QueryString });
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);

        var positionsTask = hrApiClient.GetPositionsAsync(accessToken, new PositionPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            DepartmentId = departmentId,
            IsActive = isActive
        }, ct);

        var departmentsTask = hrApiClient.GetDepartmentOptionsAsync(accessToken, ct);

        await Task.WhenAll(positionsTask, departmentsTask);

        var positions = await positionsTask;
        var departments = await departmentsTask;

        ViewData["Title"] = "Positions";
        ViewData["Breadcrumb"] = "HR / Positions";

        return View(new HrPositionsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            DepartmentId = departmentId,
            IsActive = isActive,
            Departments = departments,
            Positions = positions ?? PagedResult<PositionDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    private static int NormalizePageSize(int pageSize) => pageSize is 20 or 50 or 100 ? pageSize : DefaultPageSize;

    private string? GetAccessToken() => User.FindFirstValue("access_token");
}
