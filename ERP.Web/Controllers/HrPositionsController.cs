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
    private const string DefaultSortBy = "name";

    [HttpGet("")]
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = DefaultSortBy,
        string? sortDirection = "asc",
        string? code = null,
        string? name = null,
        int? departmentId = null,
        int? levelFrom = null,
        int? levelTo = null,
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
        var normalizedSortBy = NormalizeSortBy(sortBy);
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedCode = NormalizeTextFilter(code);
        var normalizedName = NormalizeTextFilter(name);
        var (normalizedLevelFrom, normalizedLevelTo) = NormalizeNumberRange(levelFrom, levelTo);

        var positionsTask = hrApiClient.GetPositionsAsync(accessToken, new PositionPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = normalizedCode,
            Name = normalizedName,
            DepartmentId = departmentId,
            LevelFrom = normalizedLevelFrom,
            LevelTo = normalizedLevelTo,
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
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = normalizedCode,
            NameFilter = normalizedName,
            DepartmentIdFilter = departmentId,
            LevelFrom = normalizedLevelFrom,
            LevelTo = normalizedLevelTo,
            IsActiveFilter = isActive,
            Departments = departments,
            Positions = positions ?? PagedResult<PositionDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("details/{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path + Request.QueryString });
        }

        var position = await hrApiClient.GetPositionByIdAsync(accessToken, id, ct);
        if (position is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Position Details";
        ViewData["Breadcrumb"] = "HR / Positions / Details";

        return View(position);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var model = new HrPositionEditViewModel();
        await PopulateFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create Position";
        ViewData["Breadcrumb"] = "HR / Positions / Create";

        return View(model);
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HrPositionEditViewModel model, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        await PopulateFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Position";
            ViewData["Breadcrumb"] = "HR / Positions / Create";
            return View(model);
        }

        var created = await hrApiClient.CreatePositionAsync(accessToken, new PositionDto
        {
            Name = model.Name,
            Code = model.Code,
            DepartmentId = model.DepartmentId,
            Level = model.Level,
            IsActive = model.IsActive
        }, ct);

        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create position.");
            ViewData["Title"] = "Create Position";
            ViewData["Breadcrumb"] = "HR / Positions / Create";
            return View(model);
        }

        TempData["SuccessMessage"] = "Position created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var position = await hrApiClient.GetPositionByIdAsync(accessToken, id, ct);
        if (position is null)
        {
            return NotFound();
        }

        var model = new HrPositionEditViewModel
        {
            Id = position.Id,
            Name = position.Name,
            Code = position.Code,
            DepartmentId = position.DepartmentId,
            Level = position.Level,
            IsActive = position.IsActive
        };

        await PopulateFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit Position";
        ViewData["Breadcrumb"] = "HR / Positions / Edit";

        return View(model);
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, HrPositionEditViewModel model, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        model.Id = id;
        await PopulateFormOptionsAsync(accessToken, model, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Position";
            ViewData["Breadcrumb"] = "HR / Positions / Edit";
            return View(model);
        }

        var updated = await hrApiClient.UpdatePositionAsync(accessToken, id, new PositionDto
        {
            Id = id,
            Name = model.Name,
            Code = model.Code,
            DepartmentId = model.DepartmentId,
            Level = model.Level,
            IsActive = model.IsActive
        }, ct);

        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to update position.");
            ViewData["Title"] = "Edit Position";
            ViewData["Breadcrumb"] = "HR / Positions / Edit";
            return View(model);
        }

        TempData["SuccessMessage"] = "Position updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var deleted = await hrApiClient.DeletePositionAsync(accessToken, id, ct);
        TempData[deleted ? "SuccessMessage" : "ErrorMessage"] = deleted
            ? "Position deleted."
            : "Failed to delete position.";

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateFormOptionsAsync(string accessToken, HrPositionEditViewModel model, CancellationToken ct)
    {
        var departments = await hrApiClient.GetDepartmentOptionsAsync(accessToken, ct);

        if (model.DepartmentId <= 0)
        {
            model.DepartmentId = departments.FirstOrDefault()?.Id ?? 0;
        }

        model.Departments = departments;
    }

    private static int NormalizePageSize(int pageSize) => pageSize is 20 or 50 or 100 ? pageSize : DefaultPageSize;

    private static string NormalizeSortBy(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return DefaultSortBy;
        }

        return sortBy.Trim().ToLowerInvariant() switch
        {
            "code" => "code",
            "name" => "name",
            "departmentname" => "departmentName",
            "level" => "level",
            "isactive" => "isActive",
            _ => DefaultSortBy
        };
    }

    private static string NormalizeSortDirection(string? sortDirection) =>
        string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";

    private static string? NormalizeTextFilter(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static (int? From, int? To) NormalizeNumberRange(int? from, int? to)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
        {
            return (to, from);
        }

        return (from, to);
    }

    private string? GetAccessToken() => User.FindFirstValue("access_token");
}
