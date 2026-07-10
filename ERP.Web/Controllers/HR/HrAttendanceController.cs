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
    private const string DefaultHolidaySortBy = "holidayDate";
    private const string DefaultReportSortBy = "employeeName";
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

    [HttpGet("holiday")]
    public async Task<IActionResult> Holiday(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? sortBy = DefaultHolidaySortBy,
        string? sortDirection = "desc",
        string? name = null,
        DateOnly? holidayDateFrom = null,
        DateOnly? holidayDateTo = null,
        HolidayType? holidayType = null,
        string? description = null,
        bool? isActive = null,
        string? appliesTo = null,
        short? yearFrom = null,
        short? yearTo = null,
        CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path + Request.QueryString });
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeHolidaySortBy(sortBy);
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedName = NormalizeTextFilter(name);
        var normalizedDescription = NormalizeTextFilter(description);
        var normalizedAppliesTo = NormalizeTextFilter(appliesTo);
        var (normalizedHolidayDateFrom, normalizedHolidayDateTo) = NormalizeDateRange(holidayDateFrom, holidayDateTo);
        var (normalizedYearFrom, normalizedYearTo) = NormalizeShortRange(yearFrom, yearTo);

        var holidays = await hrApiClient.GetHolidaysAsync(accessToken, new HolidayPagedRequest
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Search = search,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Name = normalizedName,
            HolidayDateFrom = normalizedHolidayDateFrom,
            HolidayDateTo = normalizedHolidayDateTo,
            HolidayType = holidayType,
            Description = normalizedDescription,
            IsActive = isActive,
            AppliesTo = normalizedAppliesTo,
            YearFrom = normalizedYearFrom,
            YearTo = normalizedYearTo
        }, ct);

        ViewData["Title"] = "Holiday Master";
        ViewData["Breadcrumb"] = "HR / Attendance / Holiday";

        return View(new HrHolidayIndexViewModel
        {
            Search = search,
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            NameFilter = normalizedName,
            HolidayDateFromFilter = normalizedHolidayDateFrom,
            HolidayDateToFilter = normalizedHolidayDateTo,
            HolidayTypeFilter = holidayType,
            DescriptionFilter = normalizedDescription,
            IsActiveFilter = isActive,
            AppliesToFilter = normalizedAppliesTo,
            YearFromFilter = normalizedYearFrom,
            YearToFilter = normalizedYearTo,
            Holidays = holidays ?? PagedResult<HolidayDto>.Create([], 0, normalizedPage, normalizedPageSize)
        });
    }

    [HttpGet("holiday/details/{id:int}")]
    public async Task<IActionResult> HolidayDetails(int id, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path + Request.QueryString });
        }

        var holiday = await hrApiClient.GetHolidayByIdAsync(accessToken, id, ct);
        if (holiday is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Holiday Details";
        ViewData["Breadcrumb"] = "HR / Attendance / Holiday / Details";

        return View("HolidayDetails", holiday);
    }

    [HttpGet("holiday/create")]
    public IActionResult HolidayCreate()
    {
        ViewData["Title"] = "Create Holiday";
        ViewData["Breadcrumb"] = "HR / Attendance / Holiday / Create";

        return View("HolidayCreate", new HrHolidayEditViewModel
        {
            IsActive = true,
            AppliesTo = "all"
        });
    }

    [HttpPost("holiday/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HolidayCreate(HrHolidayEditViewModel model, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Holiday";
            ViewData["Breadcrumb"] = "HR / Attendance / Holiday / Create";
            return View("HolidayCreate", model);
        }

        var created = await hrApiClient.CreateHolidayAsync(accessToken, MapHolidayDto(model), ct);
        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to create holiday." : created.ErrorMessage);
            ViewData["Title"] = "Create Holiday";
            ViewData["Breadcrumb"] = "HR / Attendance / Holiday / Create";
            return View("HolidayCreate", model);
        }

        TempData["SuccessMessage"] = "Holiday created.";
        return RedirectToAction(nameof(Holiday));
    }

    [HttpGet("holiday/edit/{id:int}")]
    public async Task<IActionResult> HolidayEdit(int id, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var holiday = await hrApiClient.GetHolidayByIdAsync(accessToken, id, ct);
        if (holiday is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Edit Holiday";
        ViewData["Breadcrumb"] = "HR / Attendance / Holiday / Edit";

        return View("HolidayEdit", new HrHolidayEditViewModel
        {
            Id = holiday.Id,
            Name = holiday.Name,
            HolidayDate = holiday.HolidayDate,
            HolidayType = holiday.HolidayType,
            Description = holiday.Description,
            IsActive = holiday.IsActive,
            AppliesTo = holiday.AppliesTo
        });
    }

    [HttpPost("holiday/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HolidayEdit(int id, HrHolidayEditViewModel model, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        model.Id = id;

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Edit Holiday";
            ViewData["Breadcrumb"] = "HR / Attendance / Holiday / Edit";
            return View("HolidayEdit", model);
        }

        var updated = await hrApiClient.UpdateHolidayAsync(accessToken, id, MapHolidayDto(model), ct);
        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update holiday." : updated.ErrorMessage);
            ViewData["Title"] = "Edit Holiday";
            ViewData["Breadcrumb"] = "HR / Attendance / Holiday / Edit";
            return View("HolidayEdit", model);
        }

        TempData["SuccessMessage"] = "Holiday updated.";
        return RedirectToAction(nameof(Holiday));
    }

    [HttpPost("holiday/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HolidayDelete(int id, CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth");
        }

        var deleted = await hrApiClient.DeleteHolidayAsync(accessToken, id, ct);
        TempData[deleted.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = deleted.IsSuccess ? "Holiday deleted." : (string.IsNullOrWhiteSpace(deleted.ErrorMessage) ? "Failed to delete holiday." : deleted.ErrorMessage);

        return RedirectToAction(nameof(Holiday));
    }
    [HttpGet("report")]
    public async Task<IActionResult> Report(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? sortBy = DefaultReportSortBy,
        string? sortDirection = "asc",
        string? period = null,
        int? departmentId = null,
        int? employeeId = null,
        string? status = null,
        CancellationToken ct = default)
    {
        var accessToken = GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path + Request.QueryString });
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = NormalizePageSize(pageSize);
        var normalizedSortBy = NormalizeAttendanceReportSortBy(sortBy);
        var normalizedSortDirection = NormalizeSortDirection(sortDirection);
        var normalizedStatus = NormalizeAttendanceReportStatus(status);
        var (periodYear, periodMonth, normalizedPeriod) = NormalizeAttendanceReportPeriod(period);

        var settingsTask = hrApiClient.GetAttendanceSettingAsync(accessToken, ct);
        var departmentsTask = hrApiClient.GetDepartmentOptionsAsync(accessToken, ct);
        var employeesTask = hrApiClient.GetEmployeeOptionsAsync(accessToken, ct);

        await Task.WhenAll(settingsTask, departmentsTask, employeesTask);

        var attendanceSetting = await settingsTask ?? new AttendanceSettingDto();
        var (periodStart, periodEnd, periodDisplay) = BuildAttendanceReportPeriodRange(periodYear, periodMonth, attendanceSetting);

        var attendanceItems = await GetAttendanceReportItemsAsync(
            accessToken,
            periodStart,
            periodEnd,
            departmentId,
            employeeId,
            ct);

        var lateThresholdTime = attendanceSetting.WorkStart.AddMinutes(Math.Max(0, attendanceSetting.CheckInToleranceMinutes));

        var groupedRows = attendanceItems
            .GroupBy(x => new { x.EmployeeId, x.EmployeeName, x.DepartmentName })
            .Select(group =>
            {
                var presentCount = 0;
                var lateCount = 0;
                var absentCount = 0;
                var leaveCount = 0;

                foreach (var item in group)
                {
                    if (item.Status == AttendanceStatus.Absent)
                    {
                        absentCount++;
                        continue;
                    }

                    if (IsAttendanceReportLeave(item))
                    {
                        leaveCount++;
                        continue;
                    }

                    if (IsAttendanceReportLate(item, lateThresholdTime))
                    {
                        lateCount++;
                        continue;
                    }

                    presentCount++;
                }

                return new HrAttendanceReportRowViewModel
                {
                    EmployeeId = group.Key.EmployeeId,
                    EmployeeName = group.Key.EmployeeName,
                    DepartmentName = group.Key.DepartmentName,
                    PresentCount = presentCount,
                    LateCount = lateCount,
                    AbsentCount = absentCount,
                    LeaveCount = leaveCount,
                    DominantStatus = DetermineAttendanceReportDominantStatus(presentCount, lateCount, absentCount, leaveCount)
                };
            })
            .ToList();

        IEnumerable<HrAttendanceReportRowViewModel> filteredRows = groupedRows;

        if (!string.IsNullOrWhiteSpace(normalizedStatus))
        {
            filteredRows = filteredRows.Where(x => string.Equals(x.DominantStatus, normalizedStatus, StringComparison.OrdinalIgnoreCase));
        }

        var sortedRows = ApplyAttendanceReportSorting(filteredRows, normalizedSortBy, normalizedSortDirection).ToList();

        var totalEmployees = sortedRows.Count;
        var totalPresenceDays = sortedRows.Sum(x => x.PresentCount + x.LateCount);
        var totalWorkDays = sortedRows.Sum(x => x.PresentCount + x.LateCount + x.AbsentCount + x.LeaveCount);
        var totalLateCount = sortedRows.Sum(x => x.LateCount);
        var totalAbsentCount = sortedRows.Sum(x => x.AbsentCount);
        var attendancePercentage = totalWorkDays <= 0
            ? 0m
            : Math.Round((decimal)totalPresenceDays * 100m / totalWorkDays, 1);

        var pagedRows = sortedRows
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToList();

        ViewData["Title"] = "Attendance Report";
        ViewData["Breadcrumb"] = "HR / Attendance / Report";

        return View(new HrAttendanceReportViewModel
        {
            PageSize = normalizedPageSize,
            SortBy = normalizedSortBy,
            SortDirection = normalizedSortDirection,
            Period = normalizedPeriod,
            PeriodDisplay = periodDisplay,
            DepartmentIdFilter = departmentId,
            EmployeeIdFilter = employeeId,
            StatusFilter = normalizedStatus,
            TotalEmployees = totalEmployees,
            TotalPresenceDays = totalPresenceDays,
            TotalWorkDays = totalWorkDays,
            TotalLateCount = totalLateCount,
            TotalAbsentCount = totalAbsentCount,
            AttendancePercentage = attendancePercentage,
            Departments = (await departmentsTask)
                .OrderBy(x => x.Name)
                .ToList(),
            Employees = (await employeesTask)
                .OrderBy(x => x.Name)
                .ToList(),
            Reports = PagedResult<HrAttendanceReportRowViewModel>.Create(pagedRows, totalEmployees, normalizedPage, normalizedPageSize)
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
        model.AttendancePeriodEndDay = ComputeAttendancePeriodEndDay(model.AttendancePeriodStartDay);
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

        model.AttendancePeriodEndDay = ComputeAttendancePeriodEndDay(model.AttendancePeriodStartDay);
        ModelState.Remove(nameof(model.AttendancePeriodEndDay));
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
            MinimumOtMinutes = model.MinimumOtMinutes,
            OfficeLatitude = model.OfficeLatitude,
            OfficeLongitude = model.OfficeLongitude,
            RadiusMeters = model.RadiusMeters
        }, ct);

        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to save attendance setting." : updated.ErrorMessage);
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

        var checkIn = ToOffset(model.CheckInLocal);
        var checkOut = ToOffset(model.CheckOutLocal);

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

        if (!created.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(created.ErrorMessage) ? "Failed to create attendance record." : created.ErrorMessage);
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
            CheckInLocal = ToLocalDateTime(attendance.CheckIn),
            CheckOutLocal = ToLocalDateTime(attendance.CheckOut),
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

        var checkIn = ToOffset(model.CheckInLocal);
        var checkOut = ToOffset(model.CheckOutLocal);

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

        if (!updated.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(updated.ErrorMessage) ? "Failed to update attendance record." : updated.ErrorMessage);
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
        TempData[deleted.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = deleted.IsSuccess ? "Attendance record deleted." : (string.IsNullOrWhiteSpace(deleted.ErrorMessage) ? "Failed to delete attendance record." : deleted.ErrorMessage);

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

    private async Task<IReadOnlyList<AttendanceReportDto>> GetAttendanceReportItemsAsync(
        string accessToken,
        DateOnly periodStart,
        DateOnly periodEnd,
        int? departmentId,
        int? employeeId,
        CancellationToken ct)
    {
        var records = new List<AttendanceReportDto>();
        var page = 1;
        const int pageSize = 500;

        while (true)
        {
            var response = await hrApiClient.GetAttendancesAsync(accessToken, new AttendanceReportRequest
            {
                Page = page,
                PageSize = pageSize,
                SortBy = "date",
                SortDirection = "asc",
                DepartmentId = departmentId,
                EmployeeId = employeeId,
                DateFrom = periodStart,
                DateTo = periodEnd
            }, ct);

            if (response is null || response.Items.Count == 0)
            {
                break;
            }

            records.AddRange(response.Items);

            if (page >= response.TotalPages)
            {
                break;
            }

            page++;
        }

        return records;
    }

    private static string DetermineAttendanceReportDominantStatus(int presentCount, int lateCount, int absentCount, int leaveCount)
    {
        if (absentCount >= leaveCount && absentCount >= lateCount && absentCount > 0)
        {
            return "Absent";
        }

        if (leaveCount >= lateCount && leaveCount > 0)
        {
            return "Cuti";
        }

        if (lateCount > 0)
        {
            return "Terlambat";
        }

        return "Hadir";
    }

    private static bool IsAttendanceReportLeave(AttendanceReportDto item)
    {
        if (item.Status == AttendanceStatus.HalfDay)
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(item.Notes)
            && item.Notes.Contains("cuti", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAttendanceReportLate(AttendanceReportDto item, TimeOnly lateThreshold)
    {
        if (item.Status == AttendanceStatus.Late)
        {
            return true;
        }

        if (!item.CheckIn.HasValue)
        {
            return false;
        }

        var localCheckIn = item.CheckIn.Value.ToLocalTime().TimeOfDay;
        var thresholdTime = lateThreshold.ToTimeSpan();
        return localCheckIn > thresholdTime;
    }

    private static IEnumerable<HrAttendanceReportRowViewModel> ApplyAttendanceReportSorting(
        IEnumerable<HrAttendanceReportRowViewModel> rows,
        string sortBy,
        string sortDirection)
    {
        var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy switch
        {
            "departmentName" => isDesc
                ? rows.OrderByDescending(x => x.DepartmentName).ThenBy(x => x.EmployeeName)
                : rows.OrderBy(x => x.DepartmentName).ThenBy(x => x.EmployeeName),

            "hadir" => isDesc
                ? rows.OrderByDescending(x => x.PresentCount).ThenBy(x => x.EmployeeName)
                : rows.OrderBy(x => x.PresentCount).ThenBy(x => x.EmployeeName),

            "terlambat" => isDesc
                ? rows.OrderByDescending(x => x.LateCount).ThenBy(x => x.EmployeeName)
                : rows.OrderBy(x => x.LateCount).ThenBy(x => x.EmployeeName),

            "absent" => isDesc
                ? rows.OrderByDescending(x => x.AbsentCount).ThenBy(x => x.EmployeeName)
                : rows.OrderBy(x => x.AbsentCount).ThenBy(x => x.EmployeeName),

            "cuti" => isDesc
                ? rows.OrderByDescending(x => x.LeaveCount).ThenBy(x => x.EmployeeName)
                : rows.OrderBy(x => x.LeaveCount).ThenBy(x => x.EmployeeName),

            "status" => isDesc
                ? rows.OrderByDescending(x => GetAttendanceReportStatusRank(x.DominantStatus)).ThenBy(x => x.EmployeeName)
                : rows.OrderBy(x => GetAttendanceReportStatusRank(x.DominantStatus)).ThenBy(x => x.EmployeeName),

            _ => isDesc
                ? rows.OrderByDescending(x => x.EmployeeName)
                : rows.OrderBy(x => x.EmployeeName)
        };
    }

    private static int GetAttendanceReportStatusRank(string? status)
    {
        return status?.Trim().ToLowerInvariant() switch
        {
            "hadir" => 0,
            "terlambat" => 1,
            "cuti" => 2,
            "absent" => 3,
            _ => 4
        };
    }

    private static string NormalizeAttendanceReportSortBy(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return DefaultReportSortBy;
        }

        return sortBy.Trim().ToLowerInvariant() switch
        {
            "employeename" => "employeeName",
            "departmentname" => "departmentName",
            "hadir" => "hadir",
            "terlambat" => "terlambat",
            "absent" => "absent",
            "cuti" => "cuti",
            "status" => "status",
            _ => DefaultReportSortBy
        };
    }

    private static string? NormalizeAttendanceReportStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        return status.Trim().ToLowerInvariant() switch
        {
            "hadir" => "Hadir",
            "terlambat" => "Terlambat",
            "absent" => "Absent",
            "cuti" => "Cuti",
            _ => null
        };
    }

    private static (int Year, int Month, string Period) NormalizeAttendanceReportPeriod(string? period)
    {
        if (!string.IsNullOrWhiteSpace(period)
            && DateTime.TryParseExact(period.Trim(), "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedPeriod))
        {
            return (parsedPeriod.Year, parsedPeriod.Month, parsedPeriod.ToString("yyyy-MM", CultureInfo.InvariantCulture));
        }

        var now = DateTime.Today;
        return (now.Year, now.Month, now.ToString("yyyy-MM", CultureInfo.InvariantCulture));
    }

    private static (DateOnly Start, DateOnly End, string Display) BuildAttendanceReportPeriodRange(
        int year,
        int month,
        AttendanceSettingDto attendanceSetting)
    {
        var normalizedStartDay = Math.Clamp(attendanceSetting.AttendancePeriodStartDay, 1, 31);
        var normalizedEndDay = Math.Clamp(attendanceSetting.AttendancePeriodEndDay, 1, 31);

        var periodEnd = CreateSafeDate(year, month, normalizedEndDay);
        DateOnly periodStart;

        if (normalizedStartDay > normalizedEndDay)
        {
            var previousMonth = periodEnd.AddMonths(-1);
            periodStart = CreateSafeDate(previousMonth.Year, previousMonth.Month, normalizedStartDay);
        }
        else
        {
            periodStart = CreateSafeDate(periodEnd.Year, periodEnd.Month, normalizedStartDay);
        }

        var periodDisplay = $"{periodStart:dd MMM yyyy} - {periodEnd:dd MMM yyyy}";
        return (periodStart, periodEnd, periodDisplay);
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
            MinimumOtMinutes = dto.MinimumOtMinutes,
            OfficeLatitude = dto.OfficeLatitude,
            OfficeLongitude = dto.OfficeLongitude,
            RadiusMeters = dto.RadiusMeters
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

    private static int ComputeAttendancePeriodEndDay(int startDay)
    {
        var normalizedStartDay = Math.Clamp(startDay, 1, 31);
        return normalizedStartDay == 1 ? 31 : normalizedStartDay - 1;
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

    // This app has no per-user timezone setting, so "local" here means the server's local
    // timezone (TimeZoneInfo.Local) - the same assumption FormDateTimeViewComponent documents.
    private static DateTime? ToLocalDateTime(DateTimeOffset? value)
    {
        return value?.LocalDateTime;
    }

    private static DateTimeOffset? ToOffset(DateTime? localValue)
    {
        if (!localValue.HasValue)
        {
            return null;
        }

        var local = localValue.Value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(localValue.Value, DateTimeKind.Local)
            : localValue.Value;

        return new DateTimeOffset(local);
    }

    private static HolidayDto MapHolidayDto(HrHolidayEditViewModel model)
    {
        return new HolidayDto
        {
            Id = model.Id,
            Name = NormalizeTextFilter(model.Name) ?? string.Empty,
            HolidayDate = model.HolidayDate,
            HolidayType = model.HolidayType,
            Description = NormalizeTextFilter(model.Description),
            IsActive = model.IsActive,
            AppliesTo = NormalizeTextFilter(model.AppliesTo) ?? "all"
        };
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

    private static string NormalizeHolidaySortBy(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return DefaultHolidaySortBy;
        }

        return sortBy.Trim().ToLowerInvariant() switch
        {
            "id" => "id",
            "name" => "name",
            "holidaydate" => "holidayDate",
            "holidaytype" => "holidayType",
            "description" => "description",
            "isactive" => "isActive",
            "appliesto" => "appliesTo",
            "year" => "year",
            _ => DefaultHolidaySortBy
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

    private static (short? From, short? To) NormalizeShortRange(short? from, short? to)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
        {
            return (to, from);
        }

        return (from, to);
    }

    private string? GetAccessToken() => User.FindFirstValue("access_token");
}







