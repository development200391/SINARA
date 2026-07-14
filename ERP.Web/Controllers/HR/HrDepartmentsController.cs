using System.Security.Claims;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;
using ERP.Web.Filters;
using ERP.Web.Services;
using ERP.Web.ViewModels.HR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[Authorize]
[Route("hr/departments")]
public sealed class HrDepartmentsController(IHrApiClient hrApiClient, IMenuPermissionService menuPermissionService) : Controller
{
    private const int DefaultPageSize = 20;
    private const string DefaultSortBy = "name";
    private const string DepartmentsMenuUrl = "/hr/departments";
    private const string DepartmentsMenuKey = "1";

    [HttpGet("")]
    [RequireMenuPermission(DepartmentsMenuUrl, MenuPermissionAction.View, DepartmentsMenuKey)]
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = DefaultSortBy,
        string? sortDirection = "asc",
        string? code = null,
        string? name = null,
        int? managerId = null,
        int? parentDepartmentId = null,
        bool? isActive = null,
        CancellationToken ct = default)
    {
        var accessToken = GetAccessToken() ?? string.Empty;
        var permission = await GetMenuPermissionAsync(accessToken, ct);

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy);
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedCode = NormalizeTextFilter(code);
        var normalizedName = NormalizeTextFilter(name);

        var departmentsTask = hrApiClient.GetDepartmentsAsync(accessToken, new DepartmentPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Code = normalizedCode,
            Name = normalizedName,
            ManagerId = managerId,
            ParentDepartmentId = parentDepartmentId,
            IsActive = isActive
        }, ct);

        var managersTask = hrApiClient.GetEmployeeOptionsAsync(accessToken, ct);
        var departmentOptionsTask = hrApiClient.GetDepartmentOptionsAsync(accessToken, ct);

        await Task.WhenAll(departmentsTask, managersTask, departmentOptionsTask);

        var departments = await departmentsTask;
        var managers = await managersTask;
        var departmentOptions = await departmentOptionsTask;

        ViewData["Title"] = "Departments";
        ViewData["Breadcrumb"] = "HR / Departments";

        return View(new HrDepartmentsIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            CodeFilter = normalizedCode,
            NameFilter = normalizedName,
            ManagerIdFilter = managerId,
            ParentDepartmentIdFilter = parentDepartmentId,
            IsActiveFilter = isActive,
            CanView = permission.CanView,
            CanCreate = permission.CanCreate,
            CanEdit = permission.CanEdit,
            CanDelete = permission.CanDelete,
            Managers = managers,
            DepartmentOptions = departmentOptions,
            Departments = departments ?? PagedResult<DepartmentDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("details/{id:int}")]
    [RequireMenuPermission(DepartmentsMenuUrl, MenuPermissionAction.View, DepartmentsMenuKey)]
    public async Task<IActionResult> Details(int id, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken() ?? string.Empty;

        var department = await hrApiClient.GetDepartmentByIdAsync(accessToken, id, ct);
        if (department is null)
        {
            return NotFound();
        }

        var model = new HrDepartmentEditViewModel
        {
            Id = department.Id,
            Name = department.Name,
            Code = department.Code,
            ManagerId = department.ManagerId,
            ParentDepartmentId = department.ParentDepartmentId,
            IsActive = department.IsActive,
            IsReadOnly = true
        };

        await PopulateReadOnlyOptionsAsync(accessToken, model, department, ct);

        ViewData["Title"] = "Department Details";
        ViewData["Breadcrumb"] = "HR / Departments / Details";

        return View(model);
    }

    [HttpGet("create")]
    [RequireMenuPermission(DepartmentsMenuUrl, MenuPermissionAction.Create, DepartmentsMenuKey)]
    public async Task<IActionResult> Create(CancellationToken ct = default)
    {
        var accessToken = GetAccessToken() ?? string.Empty;

        var model = new HrDepartmentEditViewModel();
        await PopulateFormOptionsAsync(accessToken, model, null, ct);

        ViewData["Title"] = "Create Department";
        ViewData["Breadcrumb"] = "HR / Departments / Create";

        return View(model);
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    [RequireMenuPermission(DepartmentsMenuUrl, MenuPermissionAction.Create, DepartmentsMenuKey)]
    public async Task<IActionResult> Create(HrDepartmentEditViewModel model, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken() ?? string.Empty;

        await PopulateFormOptionsAsync(accessToken, model, null, ct);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Department";
            ViewData["Breadcrumb"] = "HR / Departments / Create";
            return View(model);
        }

        var created = await hrApiClient.CreateDepartmentAsync(accessToken, new DepartmentDto
        {
            Name = model.Name,
            Code = model.Code,
            ManagerId = model.ManagerId,
            ParentDepartmentId = model.ParentDepartmentId,
            IsActive = model.IsActive
        }, ct);

        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to create department." : created.ErrorMessage);
            ViewData["Title"] = "Create Department";
            ViewData["Breadcrumb"] = "HR / Departments / Create";
            return View(model);
        }

        TempData["SuccessMessage"] = "Department created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    [RequireMenuPermission(DepartmentsMenuUrl, MenuPermissionAction.Edit, DepartmentsMenuKey)]
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken() ?? string.Empty;

        var department = await hrApiClient.GetDepartmentByIdAsync(accessToken, id, ct);
        if (department is null)
        {
            return NotFound();
        }

        var model = new HrDepartmentEditViewModel
        {
            Id = department.Id,
            Name = department.Name,
            Code = department.Code,
            ManagerId = department.ManagerId,
            ParentDepartmentId = department.ParentDepartmentId,
            IsActive = department.IsActive
        };

        await PopulateFormOptionsAsync(accessToken, model, id, ct);

        ViewData["Title"] = "Edit Department";
        ViewData["Breadcrumb"] = "HR / Departments / Edit";

        return View(model);
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    [RequireMenuPermission(DepartmentsMenuUrl, MenuPermissionAction.Edit, DepartmentsMenuKey)]
    public async Task<IActionResult> Edit(int id, HrDepartmentEditViewModel model, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken() ?? string.Empty;

        model.Id = id;
        await PopulateFormOptionsAsync(accessToken, model, id, ct);

        if (model.ParentDepartmentId == id)
        {
            ModelState.AddModelError(nameof(model.ParentDepartmentId), "Department cannot be its own parent.");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Department";
            ViewData["Breadcrumb"] = "HR / Departments / Edit";
            return View(model);
        }

        var updated = await hrApiClient.UpdateDepartmentAsync(accessToken, id, new DepartmentDto
        {
            Id = id,
            Name = model.Name,
            Code = model.Code,
            ManagerId = model.ManagerId,
            ParentDepartmentId = model.ParentDepartmentId,
            IsActive = model.IsActive
        }, ct);

        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update department." : updated.ErrorMessage);
            ViewData["Title"] = "Edit Department";
            ViewData["Breadcrumb"] = "HR / Departments / Edit";
            return View(model);
        }

        TempData["SuccessMessage"] = "Department updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    [RequireMenuPermission(DepartmentsMenuUrl, MenuPermissionAction.Delete, DepartmentsMenuKey)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken() ?? string.Empty;

        var deleted = await hrApiClient.DeleteDepartmentAsync(accessToken, id, ct);
        TempData[deleted.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = deleted.IsSuccess ? "Department deleted." : (string.IsNullOrWhiteSpace(deleted.ErrorMessage) ? "Failed to delete department." : deleted.ErrorMessage);

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateFormOptionsAsync(string accessToken, HrDepartmentEditViewModel model, int? currentDepartmentId, CancellationToken ct)
    {
        var managersTask = hrApiClient.GetEmployeeOptionsAsync(accessToken, ct);
        var departmentsTask = hrApiClient.GetDepartmentOptionsAsync(accessToken, ct);

        await Task.WhenAll(managersTask, departmentsTask);

        var managers = await managersTask;
        var parentDepartments = await departmentsTask;

        if (currentDepartmentId.HasValue)
        {
            parentDepartments = parentDepartments.Where(x => x.Id != currentDepartmentId.Value).ToList();
        }

        if (model.ParentDepartmentId.HasValue && parentDepartments.All(x => x.Id != model.ParentDepartmentId.Value))
        {
            model.ParentDepartmentId = null;
        }

        if (model.ManagerId.HasValue && managers.All(x => x.Id != model.ManagerId.Value))
        {
            model.ManagerId = null;
        }

        model.Managers = managers;
        model.ParentDepartments = parentDepartments;
    }

    /// <summary>
    /// Unlike <see cref="PopulateFormOptionsAsync"/> (used by Create/Edit), this never clears a stale
    /// ManagerId/ParentDepartmentId that fell out of the active options list — a Details page must keep
    /// showing the department's actual data (e.g. a manager who's since gone inactive), not silently
    /// blank it out. Missing options are appended using the names already returned on the DTO.
    /// </summary>
    private async Task PopulateReadOnlyOptionsAsync(string accessToken, HrDepartmentEditViewModel model, DepartmentDto department, CancellationToken ct)
    {
        var managersTask = hrApiClient.GetEmployeeOptionsAsync(accessToken, ct);
        var departmentsTask = hrApiClient.GetDepartmentOptionsAsync(accessToken, ct);

        await Task.WhenAll(managersTask, departmentsTask);

        var managers = await managersTask;
        if (department.ManagerId.HasValue && managers.All(x => x.Id != department.ManagerId.Value))
        {
            managers = [.. managers, new LookupDto { Id = department.ManagerId.Value, Name = department.ManagerName ?? $"#{department.ManagerId.Value}" }];
        }

        var parentDepartments = await departmentsTask;
        if (department.ParentDepartmentId.HasValue && parentDepartments.All(x => x.Id != department.ParentDepartmentId.Value))
        {
            parentDepartments = [.. parentDepartments, new DepartmentDto { Id = department.ParentDepartmentId.Value, Code = string.Empty, Name = department.ParentDepartmentName ?? $"#{department.ParentDepartmentId.Value}" }];
        }

        model.Managers = managers;
        model.ParentDepartments = parentDepartments;
    }

    private Task<MenuPermissionFlags> GetMenuPermissionAsync(string accessToken, CancellationToken ct) =>
        menuPermissionService.GetMenuPermissionAsync(User, accessToken, DepartmentsMenuUrl, DepartmentsMenuKey, ct);

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
            "managername" => "managerName",
            "parentdepartmentname" => "parentDepartmentName",
            "isactive" => "isActive",
            _ => DefaultSortBy
        };
    }

    private static string NormalizeSortDirection(string? sortDirection) =>
        string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";

    private static string? NormalizeTextFilter(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private string? GetAccessToken() => User.FindFirstValue("access_token");
}
