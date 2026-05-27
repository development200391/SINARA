using System.Globalization;
using System.Security.Claims;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;
using ERP.Domain.Enums;
using ERP.Web.Services;
using ERP.Web.ViewModels.HR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ERP.Web.Controllers;

[Authorize]
[Route("hr/attendance")]
public sealed class HrAttendanceController(IHrApiClient hrApiClient) : Controller
{
    private const int DefaultPageSize = 20;
    private const string DefaultSortBy = "date";
    private static readonly string[] DateTimeInputFormats = ["yyyy-MM-ddTHH:mm", "yyyy-MM-ddTHH:mm:ss"];
    private static readonly string[] TimeInputFormats = ["HH:mm", "HH:mm:ss"];

    [HttpGet("")]
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = DefaultSortBy,
        string? sortDirection = "desc",
        string? employeeCode = null,
        string? employeeName = null,
        int? employeeId = null,
        int? departmentId = null,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        DateOnly? checkInFrom = null,
        DateOnly? checkInTo = null,
        DateOnly? checkOutFrom = null,
        DateOnly? checkOutTo = null,
        AttendanceStatus? status = null,
        string? notes = null,
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
        var normalizedEmployeeName = NormalizeTextFilter(employeeName);
        var normalizedNotes = NormalizeTextFilter(notes);
        var (normalizedDateFrom, normalizedDateTo) = NormalizeDateRange(dateFrom, dateTo);
        var (normalizedCheckInFrom, normalizedCheckInTo) = NormalizeDateRange(checkInFrom, checkInTo);
        var (normalizedCheckOutFrom, normalizedCheckOutTo) = NormalizeDateRange(checkOutFrom, checkOutTo);

        var attendancesTask = hrApiClient.GetAttendancesAsync(accessToken, new AttendanceReportRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            EmployeeCode = normalizedEmployeeCode,
            EmployeeName = normalizedEmployeeName,
            EmployeeId = employeeId,
            DepartmentId = departmentId,
            DateFrom = normalizedDateFrom,
            DateTo = normalizedDateTo,
            CheckInFrom = normalizedCheckInFrom,
            CheckInTo = normalizedCheckInTo,
            CheckOutFrom = normalizedCheckOutFrom,
            CheckOutTo = normalizedCheckOutTo,
            Status = status,
            Notes = normalizedNotes
        }, ct);

        var departmentsTask = hrApiClient.GetDepartmentOptionsAsync(accessToken, ct);

        await Task.WhenAll(attendancesTask, departmentsTask);

        var attendances = await attendancesTask;
        var departments = await departmentsTask;

        ViewData["Title"] = "Attendance";
        ViewData["Breadcrumb"] = "HR / Attendance";

        return View(new HrAttendanceIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            EmployeeCodeFilter = normalizedEmployeeCode,
            EmployeeNameFilter = normalizedEmployeeName,
            EmployeeIdFilter = employeeId,
            DepartmentIdFilter = departmentId,
            DateFromFilter = normalizedDateFrom,
            DateToFilter = normalizedDateTo,
            CheckInFromFilter = normalizedCheckInFrom,
            CheckInToFilter = normalizedCheckInTo,
            CheckOutFromFilter = normalizedCheckOutFrom,
            CheckOutToFilter = normalizedCheckOutTo,
            StatusFilter = status,
            NotesFilter = normalizedNotes,
            Departments = departments,
            Attendances = attendances ?? PagedResult<AttendanceReportDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("setting")]
    public async Task<IActionResult> Setting(CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path + Request.QueryString });
        }

        var settings = await hrApiClient.GetAttendanceSettingAsync(accessToken, ct) ?? new AttendanceSettingDto();
        var model = MapSettingViewModel(settings);
        model.CurrentPeriodPreview = BuildAttendancePeriodPreview(
            model.AttendancePeriodStartDay,
            model.AttendancePeriodEndDay,
            DateOnly.FromDateTime(DateTime.Today));

        ViewData["Title"] = "Attendance Setting";
        ViewData["Breadcrumb"] = "HR / Attendance / Setting";

        return View(model);
    }

    [HttpPost("setting")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Setting(HrAttendanceSettingViewModel model, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path + Request.QueryString });
        }

        model.CurrentPeriodPreview = BuildAttendancePeriodPreview(
            model.AttendancePeriodStartDay,
            model.AttendancePeriodEndDay,
            DateOnly.FromDateTime(DateTime.Today));

        var workStart = ParseTimeOnly(model.WorkStart, nameof(model.WorkStart), ModelState);
        var workEnd = ParseTimeOnly(model.WorkEnd, nameof(model.WorkEnd), ModelState);
        var breakStart = ParseTimeOnly(model.BreakStart, nameof(model.BreakStart), ModelState);
        var breakEnd = ParseTimeOnly(model.BreakEnd, nameof(model.BreakEnd), ModelState);

        if (workStart.HasValue && workEnd.HasValue && workEnd.Value <= workStart.Value)
        {
            ModelState.AddModelError(nameof(model.WorkEnd), "Work end time must be later than work start time.");
        }

        if (breakStart.HasValue && breakEnd.HasValue && breakEnd.Value <= breakStart.Value)
        {
            ModelState.AddModelError(nameof(model.BreakEnd), "Break end time must be later than break start time.");
        }

        if (workStart.HasValue && workEnd.HasValue && breakStart.HasValue && breakEnd.HasValue)
        {
            if (breakStart.Value < workStart.Value || breakEnd.Value > workEnd.Value)
            {
                ModelState.AddModelError(nameof(model.BreakStart), "Break time must be within work schedule.");
            }
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Attendance Setting";
            ViewData["Breadcrumb"] = "HR / Attendance / Setting";
            return View(model);
        }

        var updated = await hrApiClient.UpdateAttendanceSettingAsync(accessToken, new AttendanceSettingDto
        {
            AttendancePeriodStartDay = model.AttendancePeriodStartDay,
            AttendancePeriodEndDay = model.AttendancePeriodEndDay,
            CheckInToleranceMinutes = model.CheckInToleranceMinutes,
            WorkStart = workStart!.Value,
            WorkEnd = workEnd!.Value,
            BreakStart = breakStart!.Value,
            BreakEnd = breakEnd!.Value,
            MinimumOtMinutes = model.MinimumOtMinutes
        }, ct);

        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to save attendance setting.");
            ViewData["Title"] = "Attendance Setting";
            ViewData["Breadcrumb"] = "HR / Attendance / Setting";
            return View(model);
        }

        TempData["SuccessMessage"] = "Attendance setting saved.";
        return RedirectToAction(nameof(Setting));
    }

    [HttpGet("details/{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path + Request.QueryString });
        }

        var attendance = await hrApiClient.GetAttendanceByIdAsync(accessToken, id, ct);
        if (attendance is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Attendance Details";
        ViewData["Breadcrumb"] = "HR / Attendance / Details";

        return View(attendance);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var model = new HrAttendanceEditViewModel();
        await PopulateFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Create Attendance";
        ViewData["Breadcrumb"] = "HR / Attendance / Create";

        return View(model);
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HrAttendanceEditViewModel model, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        await PopulateFormOptionsAsync(accessToken, model, ct);

        var checkIn = ParseLocalDateTime(model.CheckInLocal, nameof(model.CheckInLocal), ModelState);
        var checkOut = ParseLocalDateTime(model.CheckOutLocal, nameof(model.CheckOutLocal), ModelState);

        if (checkIn.HasValue && checkOut.HasValue && checkOut.Value < checkIn.Value)
        {
            ModelState.AddModelError(nameof(model.CheckOutLocal), "Check-out cannot be earlier than check-in.");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Attendance";
            ViewData["Breadcrumb"] = "HR / Attendance / Create";
            return View(model);
        }

        var created = await hrApiClient.CreateAttendanceAsync(accessToken, new AttendanceRecordRequest
        {
            EmployeeId = model.EmployeeId,
            Date = model.Date,
            CheckIn = checkIn,
            CheckOut = checkOut,
            Status = model.Status,
            Notes = NormalizeTextFilter(model.Notes)
        }, ct);

        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create attendance record.");
            ViewData["Title"] = "Create Attendance";
            ViewData["Breadcrumb"] = "HR / Attendance / Create";
            return View(model);
        }

        TempData["SuccessMessage"] = "Attendance record created.";
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

        var attendance = await hrApiClient.GetAttendanceByIdAsync(accessToken, id, ct);
        if (attendance is null)
        {
            return NotFound();
        }

        var model = new HrAttendanceEditViewModel
        {
            Id = attendance.Id,
            EmployeeId = attendance.EmployeeId,
            Date = attendance.Date,
            CheckInLocal = FormatDateTimeLocal(attendance.CheckIn),
            CheckOutLocal = FormatDateTimeLocal(attendance.CheckOut),
            Status = attendance.Status,
            Notes = attendance.Notes
        };

        await PopulateFormOptionsAsync(accessToken, model, ct);

        ViewData["Title"] = "Edit Attendance";
        ViewData["Breadcrumb"] = "HR / Attendance / Edit";

        return View(model);
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, HrAttendanceEditViewModel model, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        model.Id = id;
        await PopulateFormOptionsAsync(accessToken, model, ct);

        var checkIn = ParseLocalDateTime(model.CheckInLocal, nameof(model.CheckInLocal), ModelState);
        var checkOut = ParseLocalDateTime(model.CheckOutLocal, nameof(model.CheckOutLocal), ModelState);

        if (checkIn.HasValue && checkOut.HasValue && checkOut.Value < checkIn.Value)
        {
            ModelState.AddModelError(nameof(model.CheckOutLocal), "Check-out cannot be earlier than check-in.");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Attendance";
            ViewData["Breadcrumb"] = "HR / Attendance / Edit";
            return View(model);
        }

        var updated = await hrApiClient.UpdateAttendanceAsync(accessToken, id, new AttendanceRecordRequest
        {
            EmployeeId = model.EmployeeId,
            Date = model.Date,
            CheckIn = checkIn,
            CheckOut = checkOut,
            Status = model.Status,
            Notes = NormalizeTextFilter(model.Notes)
        }, ct);

        if (updated is null)
        {
            ModelState.AddModelError(string.Empty, "Failed to update attendance record.");
            ViewData["Title"] = "Edit Attendance";
            ViewData["Breadcrumb"] = "HR / Attendance / Edit";
            return View(model);
        }

        TempData["SuccessMessage"] = "Attendance record updated.";
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

        var deleted = await hrApiClient.DeleteAttendanceAsync(accessToken, id, ct);
        TempData[deleted ? "SuccessMessage" : "ErrorMessage"] = deleted
            ? "Attendance record deleted."
            : "Failed to delete attendance record.";

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateFormOptionsAsync(string accessToken, HrAttendanceEditViewModel model, CancellationToken ct)
    {
        var employees = await hrApiClient.GetEmployeeOptionsAsync(accessToken, ct);

        if (model.EmployeeId <= 0 || employees.All(x => x.Id != model.EmployeeId))
        {
            model.EmployeeId = employees.FirstOrDefault()?.Id ?? 0;
        }

        model.Employees = employees;
    }

    private static HrAttendanceSettingViewModel MapSettingViewModel(AttendanceSettingDto dto)
    {
        return new HrAttendanceSettingViewModel
        {
            AttendancePeriodStartDay = dto.AttendancePeriodStartDay,
            AttendancePeriodEndDay = dto.AttendancePeriodEndDay,
            CheckInToleranceMinutes = dto.CheckInToleranceMinutes,
            WorkStart = dto.WorkStart.ToString("HH:mm", CultureInfo.InvariantCulture),
            WorkEnd = dto.WorkEnd.ToString("HH:mm", CultureInfo.InvariantCulture),
            BreakStart = dto.BreakStart.ToString("HH:mm", CultureInfo.InvariantCulture),
            BreakEnd = dto.BreakEnd.ToString("HH:mm", CultureInfo.InvariantCulture),
            MinimumOtMinutes = dto.MinimumOtMinutes
        };
    }

    private static string BuildAttendancePeriodPreview(int startDay, int endDay, DateOnly referenceDate)
    {
        var normalizedStartDay = Math.Clamp(startDay, 1, 31);
        var normalizedEndDay = Math.Clamp(endDay, 1, 31);

        var endDate = CreateSafeDate(referenceDate.Year, referenceDate.Month, normalizedEndDay);
        DateOnly startDate;

        if (normalizedStartDay > normalizedEndDay)
        {
            var previousMonth = endDate.AddMonths(-1);
            startDate = CreateSafeDate(previousMonth.Year, previousMonth.Month, normalizedStartDay);
        }
        else
        {
            startDate = CreateSafeDate(endDate.Year, endDate.Month, normalizedStartDay);
        }

        return $"{startDate:dd MMM} - {endDate:dd MMM}";
    }

    private static DateOnly CreateSafeDate(int year, int month, int day)
    {
        var maxDay = DateTime.DaysInMonth(year, month);
        return new DateOnly(year, month, Math.Min(day, maxDay));
    }

    private static TimeOnly? ParseTimeOnly(string? value, string fieldName, ModelStateDictionary modelState)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            modelState.AddModelError(fieldName, "Time is required.");
            return null;
        }

        var normalized = value.Trim();
        if (!TimeOnly.TryParseExact(normalized, TimeInputFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            modelState.AddModelError(fieldName, "Invalid time format. Use HH:mm.");
            return null;
        }

        return parsed;
    }

    private static string? FormatDateTimeLocal(DateTimeOffset? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        return value.Value.ToLocalTime().ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);
    }

    private static DateTimeOffset? ParseLocalDateTime(string? value, string fieldName, ModelStateDictionary modelState)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (!DateTime.TryParseExact(normalized, DateTimeInputFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsed))
        {
            modelState.AddModelError(fieldName, "Invalid date-time format.");
            return null;
        }

        if (parsed.Kind == DateTimeKind.Unspecified)
        {
            parsed = DateTime.SpecifyKind(parsed, DateTimeKind.Local);
        }

        return new DateTimeOffset(parsed);
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
            "id" => "id",
            "employeecode" => "employeeCode",
            "employeename" => "employeeName",
            "departmentname" => "departmentName",
            "date" => "date",
            "checkin" => "checkIn",
            "checkout" => "checkOut",
            "status" => "status",
            "notes" => "notes",
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


