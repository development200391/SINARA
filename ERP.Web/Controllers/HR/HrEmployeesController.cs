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
public sealed class HrEmployeesController(
    IHrApiClient hrApiClient,
    IEmployeePhotoService employeePhotoService,
    ILogger<HrEmployeesController> logger) : Controller
{
    private const int DefaultPageSize = 20;
    private const string DefaultSortBy = "fullName";

    [HttpGet("")]
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = DefaultSortBy,
        string? sortDirection = "asc",
        string? employeeCode = null,
        string? fullName = null,
        string? email = null,
        string? phone = null,
        int? departmentId = null,
        int? positionId = null,
        EmploymentStatus? employmentStatus = null,
        DateOnly? hireDateFrom = null,
        DateOnly? hireDateTo = null,
        DateOnly? terminationDateFrom = null,
        DateOnly? terminationDateTo = null,
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
        var normalizedEmployeeCode = NormalizeTextFilter(employeeCode);
        var normalizedFullName = NormalizeTextFilter(fullName);
        var normalizedEmail = NormalizeTextFilter(email);
        var normalizedPhone = NormalizeTextFilter(phone);
        var (normalizedHireDateFrom, normalizedHireDateTo) = NormalizeDateRange(hireDateFrom, hireDateTo);
        var (normalizedTerminationDateFrom, normalizedTerminationDateTo) = NormalizeDateRange(terminationDateFrom, terminationDateTo);

        var employeesTask = hrApiClient.GetEmployeesAsync(accessToken, new EmployeePagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            EmployeeCode = normalizedEmployeeCode,
            FullName = normalizedFullName,
            Email = normalizedEmail,
            Phone = normalizedPhone,
            DepartmentId = departmentId,
            PositionId = positionId,
            EmploymentStatus = employmentStatus,
            HireDateFrom = normalizedHireDateFrom,
            HireDateTo = normalizedHireDateTo,
            TerminationDateFrom = normalizedTerminationDateFrom,
            TerminationDateTo = normalizedTerminationDateTo
        }, ct);

        var departmentsTask = hrApiClient.GetDepartmentOptionsAsync(accessToken, ct);
        var positionsTask = hrApiClient.GetPositionOptionsAsync(accessToken, ct);

        await Task.WhenAll(employeesTask, departmentsTask, positionsTask);

        var employees = await employeesTask;
        var departments = await departmentsTask;
        var positions = await positionsTask;

        ViewData["Title"] = "Employees";
        ViewData["Breadcrumb"] = "HR / Employees";

        return View(new HrEmployeesIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            EmployeeCodeFilter = normalizedEmployeeCode,
            FullNameFilter = normalizedFullName,
            EmailFilter = normalizedEmail,
            PhoneFilter = normalizedPhone,
            DepartmentIdFilter = departmentId,
            PositionIdFilter = positionId,
            EmploymentStatusFilter = employmentStatus,
            HireDateFrom = normalizedHireDateFrom,
            HireDateTo = normalizedHireDateTo,
            TerminationDateFrom = normalizedTerminationDateFrom,
            TerminationDateTo = normalizedTerminationDateTo,
            Departments = departments,
            Positions = positions,
            Employees = employees ?? PagedResult<EmployeeListDto>.Create([], 0, normalizedPage, normalizedPageSize)
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

        var employee = await hrApiClient.GetEmployeeByIdAsync(accessToken, id, ct);
        if (employee is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Employee Details";
        ViewData["Breadcrumb"] = "HR / Employees / Details";

        return View(employee);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var model = new HrEmployeeEditViewModel();
        await PopulateFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create Employee";
        ViewData["Breadcrumb"] = "HR / Employees / Create";

        return View(model);
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HrEmployeeEditViewModel model, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        await PopulateFormOptionsAsync(accessToken, model, ct);

        if (model.TerminationDate.HasValue && model.TerminationDate.Value < model.HireDate)
        {
            ModelState.AddModelError(nameof(model.TerminationDate), "Termination date cannot be earlier than hire date.");
        }

        ValidatePositionBelongsToDepartment(model);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Employee";
            ViewData["Breadcrumb"] = "HR / Employees / Create";
            return View(model);
        }

        var newPhotoPath = await SaveEmployeePhotoAsync(model, ct);
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Employee";
            ViewData["Breadcrumb"] = "HR / Employees / Create";
            return View(model);
        }

        var created = await hrApiClient.CreateEmployeeAsync(accessToken, new CreateEmployeeRequest
        {
            EmployeeCode = model.EmployeeCode,
            FullName = model.FullName,
            Email = model.Email,
            Phone = model.Phone,
            PhotoPath = newPhotoPath,
            DepartmentId = model.DepartmentId,
            PositionId = model.PositionId,
            HireDate = model.HireDate,
            EmploymentStatus = model.EmploymentStatus
        }, ct);

        if (!created.IsSuccess)
        {
            await DeleteEmployeePhotoSafelyAsync(newPhotoPath, ct);

            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to create employee." : created.ErrorMessage);
            ViewData["Title"] = "Create Employee";
            ViewData["Breadcrumb"] = "HR / Employees / Create";
            return View(model);
        }

        TempData["SuccessMessage"] = "Employee created.";
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

        var employee = await hrApiClient.GetEmployeeByIdAsync(accessToken, id, ct);
        if (employee is null)
        {
            return NotFound();
        }

        var model = new HrEmployeeEditViewModel
        {
            Id = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            FullName = employee.FullName,
            Email = employee.Email,
            Phone = employee.Phone,
            PhotoPath = employee.PhotoPath,
            DepartmentId = employee.DepartmentId,
            PositionId = employee.PositionId,
            HireDate = employee.HireDate,
            TerminationDate = employee.TerminationDate,
            EmploymentStatus = employee.EmploymentStatus
        };

        await PopulateFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit Employee";
        ViewData["Breadcrumb"] = "HR / Employees / Edit";

        return View(model);
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, HrEmployeeEditViewModel model, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var existingPhotoPath = model.PhotoPath;

        model.Id = id;
        await PopulateFormOptionsAsync(accessToken, model, ct);

        if (model.TerminationDate.HasValue && model.TerminationDate.Value < model.HireDate)
        {
            ModelState.AddModelError(nameof(model.TerminationDate), "Termination date cannot be earlier than hire date.");
        }

        ValidatePositionBelongsToDepartment(model);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Employee";
            ViewData["Breadcrumb"] = "HR / Employees / Edit";
            return View(model);
        }

        var newPhotoPath = await SaveEmployeePhotoAsync(model, ct);
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Employee";
            ViewData["Breadcrumb"] = "HR / Employees / Edit";
            return View(model);
        }

        var finalPhotoPath = newPhotoPath ?? existingPhotoPath;

        var updated = await hrApiClient.UpdateEmployeeAsync(accessToken, id, new UpdateEmployeeRequest
        {
            EmployeeCode = model.EmployeeCode,
            FullName = model.FullName,
            Email = model.Email,
            Phone = model.Phone,
            PhotoPath = finalPhotoPath,
            DepartmentId = model.DepartmentId,
            PositionId = model.PositionId,
            HireDate = model.HireDate,
            TerminationDate = model.TerminationDate,
            EmploymentStatus = model.EmploymentStatus
        }, ct);

        if (!updated.IsSuccess)
        {
            await DeleteEmployeePhotoSafelyAsync(newPhotoPath, ct);

            model.PhotoPath = existingPhotoPath;
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update employee." : updated.ErrorMessage);
            ViewData["Title"] = "Edit Employee";
            ViewData["Breadcrumb"] = "HR / Employees / Edit";
            return View(model);
        }

        if (!string.IsNullOrWhiteSpace(newPhotoPath)
            && !string.Equals(existingPhotoPath, newPhotoPath, StringComparison.OrdinalIgnoreCase))
        {
            await DeleteEmployeePhotoSafelyAsync(existingPhotoPath, ct);
        }

        TempData["SuccessMessage"] = "Employee updated.";
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

        var deleted = await hrApiClient.DeleteEmployeeAsync(accessToken, id, ct);
        TempData[deleted.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = deleted.IsSuccess ? "Employee deleted." : (string.IsNullOrWhiteSpace(deleted.ErrorMessage) ? "Failed to delete employee." : deleted.ErrorMessage);

        return RedirectToAction(nameof(Index));
    }

    private async Task<string?> SaveEmployeePhotoAsync(HrEmployeeEditViewModel model, CancellationToken ct)
    {
        if (model.PhotoFile is null || model.PhotoFile.Length <= 0)
        {
            return null;
        }

        try
        {
            return await employeePhotoService.SavePhotoAsync(
                model.PhotoFile,
                new EmployeePhotoCropData(model.PhotoCropX, model.PhotoCropY, model.PhotoCropWidth, model.PhotoCropHeight),
                ct);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.PhotoFile), ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to process employee photo.");
            ModelState.AddModelError(nameof(model.PhotoFile), "Failed to process employee photo.");
            return null;
        }
    }

    private async Task DeleteEmployeePhotoSafelyAsync(string? photoPath, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(photoPath))
        {
            return;
        }

        try
        {
            await employeePhotoService.DeletePhotoAsync(photoPath, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to delete employee photo {PhotoPath}", photoPath);
        }
    }

    private async Task PopulateFormOptionsAsync(string accessToken, HrEmployeeEditViewModel model, CancellationToken ct)
    {
        var departmentsTask = hrApiClient.GetDepartmentOptionsAsync(accessToken, ct);
        var positionsTask = hrApiClient.GetPositionOptionsAsync(accessToken, ct);

        await Task.WhenAll(departmentsTask, positionsTask);

        var departments = await departmentsTask;
        var positions = await positionsTask;

        if (model.DepartmentId <= 0)
        {
            model.DepartmentId = departments.FirstOrDefault()?.Id ?? 0;
        }

        if (model.PositionId <= 0 || positions.All(x => x.Id != model.PositionId))
        {
            var candidate = positions.FirstOrDefault(x => x.DepartmentId == model.DepartmentId) ?? positions.FirstOrDefault();
            model.PositionId = candidate?.Id ?? 0;
        }

        model.Departments = departments;
        model.Positions = positions;
    }

    private void ValidatePositionBelongsToDepartment(HrEmployeeEditViewModel model)
    {
        if (model.DepartmentId <= 0 || model.PositionId <= 0)
        {
            return;
        }

        var selectedPosition = model.Positions.FirstOrDefault(x => x.Id == model.PositionId);
        if (selectedPosition is null)
        {
            ModelState.AddModelError(nameof(model.PositionId), "Position is not valid.");
            return;
        }

        if (selectedPosition.DepartmentId != model.DepartmentId)
        {
            ModelState.AddModelError(nameof(model.PositionId), "Selected position does not belong to selected department.");
        }
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
            "employeecode" => "employeeCode",
            "fullname" => "fullName",
            "email" => "email",
            "phone" => "phone",
            "departmentname" => "departmentName",
            "positionname" => "positionName",
            "hiredate" => "hireDate",
            "terminationdate" => "terminationDate",
            "employmentstatus" => "employmentStatus",
            _ => DefaultSortBy
        };
    }

    private static string NormalizeSortDirection(string? sortDirection) =>
        string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";

    private static string? NormalizeTextFilter(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static (DateOnly? From, DateOnly? To) NormalizeDateRange(DateOnly? from, DateOnly? to)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
        {
            return (to, from);
        }

        return (from, to);
    }

    private string? GetAccessToken() => User.FindFirstValue("access_token");
}