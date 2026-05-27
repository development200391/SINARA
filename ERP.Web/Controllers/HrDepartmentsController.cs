using System.Security.Claims;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;
using ERP.Web.Services;
using ERP.Web.ViewModels.HR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[Authorize]
[Route("hr/departments")]
public sealed class HrDepartmentsController(IHrApiClient hrApiClient) : Controller
{
    private const int DefaultPageSize = 20;

    [HttpGet("")]
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
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

        var departments = await hrApiClient.GetDepartmentsAsync(accessToken, new DepartmentPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            IsActive = isActive
        }, ct);

        ViewData["Title"] = "Departments";
        ViewData["Breadcrumb"] = "HR / Departments";

        return View(new HrDepartmentsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            IsActive = isActive,
            Departments = departments ?? PagedResult<DepartmentDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    private static int NormalizePageSize(int pageSize) => pageSize is 20 or 50 or 100 ? pageSize : DefaultPageSize;

    private string? GetAccessToken() => User.FindFirstValue("access_token");
}
