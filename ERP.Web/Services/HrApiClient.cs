using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Document;
using ERP.Application.DTOs.HR;
using Microsoft.AspNetCore.Http;

namespace ERP.Web.Services;

public sealed class HrApiClient(HttpClient httpClient, ILogger<HrApiClient> logger) : ApiClientBase(httpClient, logger, "HR"), IHrApiClient
{
    public Task<PagedResult<EmployeeListDto>?> GetEmployeesAsync(string accessToken, EmployeePagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.EmployeeCode))
        {
            parameters.Add($"employeeCode={Uri.EscapeDataString(request.EmployeeCode.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            parameters.Add($"fullName={Uri.EscapeDataString(request.FullName.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            parameters.Add($"email={Uri.EscapeDataString(request.Email.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            parameters.Add($"phone={Uri.EscapeDataString(request.Phone.Trim())}");
        }

        if (request.DepartmentId.HasValue)
        {
            parameters.Add($"departmentId={request.DepartmentId.Value}");
        }

        if (request.PositionId.HasValue)
        {
            parameters.Add($"positionId={request.PositionId.Value}");
        }

        if (request.EmploymentStatus.HasValue)
        {
            parameters.Add($"employmentStatus={(int)request.EmploymentStatus.Value}");
        }

        if (request.HireDateFrom.HasValue)
        {
            parameters.Add($"hireDateFrom={request.HireDateFrom.Value:yyyy-MM-dd}");
        }

        if (request.HireDateTo.HasValue)
        {
            parameters.Add($"hireDateTo={request.HireDateTo.Value:yyyy-MM-dd}");
        }

        if (request.TerminationDateFrom.HasValue)
        {
            parameters.Add($"terminationDateFrom={request.TerminationDateFrom.Value:yyyy-MM-dd}");
        }

        if (request.TerminationDateTo.HasValue)
        {
            parameters.Add($"terminationDateTo={request.TerminationDateTo.Value:yyyy-MM-dd}");
        }

        var query = $"api/v1/hr/employees?{string.Join("&", parameters)}";
        return SendWithResultAsync<PagedResult<EmployeeListDto>>(HttpMethod.Get, query, accessToken, null, ct).ToDataAsync();
    }

    public async Task<IReadOnlyList<LookupDto>> GetEmployeeOptionsAsync(string accessToken, CancellationToken ct = default)
    {
        return await SendWithResultAsync<IReadOnlyList<LookupDto>>(HttpMethod.Get, "api/v1/hr/employees/options", accessToken, null, ct).ToDataAsync() ?? [];
    }

    public Task<EmployeeDetailDto?> GetEmployeeByIdAsync(string accessToken, int id, CancellationToken ct = default)
    {
        return SendWithResultAsync<EmployeeDetailDto>(HttpMethod.Get, $"api/v1/hr/employees/{id}", accessToken, null, ct).ToDataAsync();
    }

    public Task<ApiCallResult<EmployeeDetailDto>> CreateEmployeeAsync(string accessToken, CreateEmployeeRequest request, CancellationToken ct = default)
    {
        return SendWithResultAsync<EmployeeDetailDto>(HttpMethod.Post, "api/v1/hr/employees", accessToken, request, ct);
    }

    public Task<ApiCallResult<EmployeeDetailDto>> UpdateEmployeeAsync(string accessToken, int id, UpdateEmployeeRequest request, CancellationToken ct = default)
    {
        return SendWithResultAsync<EmployeeDetailDto>(HttpMethod.Put, $"api/v1/hr/employees/{id}", accessToken, request, ct);
    }

    public Task<ApiCallResult<object?>> DeleteEmployeeAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Delete, $"api/v1/hr/employees/{id}", accessToken, null, ct);

    public Task<PagedResult<DepartmentDto>?> GetDepartmentsAsync(string accessToken, DepartmentPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            parameters.Add($"code={Uri.EscapeDataString(request.Code.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            parameters.Add($"name={Uri.EscapeDataString(request.Name.Trim())}");
        }

        if (request.ManagerId.HasValue)
        {
            parameters.Add($"managerId={request.ManagerId.Value}");
        }

        if (request.ParentDepartmentId.HasValue)
        {
            parameters.Add($"parentDepartmentId={request.ParentDepartmentId.Value}");
        }

        if (request.IsActive.HasValue)
        {
            parameters.Add($"isActive={(request.IsActive.Value ? "true" : "false")}");
        }

        var query = $"api/v1/hr/departments?{string.Join("&", parameters)}";
        return SendWithResultAsync<PagedResult<DepartmentDto>>(HttpMethod.Get, query, accessToken, null, ct).ToDataAsync();
    }

    public async Task<IReadOnlyList<DepartmentDto>> GetDepartmentOptionsAsync(string accessToken, CancellationToken ct = default)
    {
        return await SendWithResultAsync<IReadOnlyList<DepartmentDto>>(HttpMethod.Get, "api/v1/hr/departments/all", accessToken, null, ct).ToDataAsync() ?? [];
    }

    public Task<DepartmentDto?> GetDepartmentByIdAsync(string accessToken, int id, CancellationToken ct = default)
    {
        return SendWithResultAsync<DepartmentDto>(HttpMethod.Get, $"api/v1/hr/departments/{id}", accessToken, null, ct).ToDataAsync();
    }

    public Task<ApiCallResult<DepartmentDto>> CreateDepartmentAsync(string accessToken, DepartmentDto request, CancellationToken ct = default)
    {
        return SendWithResultAsync<DepartmentDto>(HttpMethod.Post, "api/v1/hr/departments", accessToken, request, ct);
    }

    public Task<ApiCallResult<DepartmentDto>> UpdateDepartmentAsync(string accessToken, int id, DepartmentDto request, CancellationToken ct = default)
    {
        return SendWithResultAsync<DepartmentDto>(HttpMethod.Put, $"api/v1/hr/departments/{id}", accessToken, request, ct);
    }

    public Task<ApiCallResult<object?>> DeleteDepartmentAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Delete, $"api/v1/hr/departments/{id}", accessToken, null, ct);

    public Task<PagedResult<PositionDto>?> GetPositionsAsync(string accessToken, PositionPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            parameters.Add($"code={Uri.EscapeDataString(request.Code.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            parameters.Add($"name={Uri.EscapeDataString(request.Name.Trim())}");
        }

        if (request.DepartmentId.HasValue)
        {
            parameters.Add($"departmentId={request.DepartmentId.Value}");
        }

        if (request.LevelFrom.HasValue)
        {
            parameters.Add($"levelFrom={request.LevelFrom.Value}");
        }

        if (request.LevelTo.HasValue)
        {
            parameters.Add($"levelTo={request.LevelTo.Value}");
        }

        if (request.IsActive.HasValue)
        {
            parameters.Add($"isActive={(request.IsActive.Value ? "true" : "false")}");
        }

        var query = $"api/v1/hr/positions?{string.Join("&", parameters)}";
        return SendWithResultAsync<PagedResult<PositionDto>>(HttpMethod.Get, query, accessToken, null, ct).ToDataAsync();
    }

    public async Task<IReadOnlyList<PositionDto>> GetPositionOptionsAsync(string accessToken, CancellationToken ct = default)
    {
        return await SendWithResultAsync<IReadOnlyList<PositionDto>>(HttpMethod.Get, "api/v1/hr/positions/all", accessToken, null, ct).ToDataAsync() ?? [];
    }

    public async Task<IReadOnlyList<PositionDto>> GetPositionsByDepartmentAsync(string accessToken, int departmentId, CancellationToken ct = default)
    {
        if (departmentId <= 0)
        {
            return [];
        }

        return await SendWithResultAsync<IReadOnlyList<PositionDto>>(HttpMethod.Get, $"api/v1/hr/positions/by-department/{departmentId}", accessToken, null, ct).ToDataAsync() ?? [];
    }

    public Task<PositionDto?> GetPositionByIdAsync(string accessToken, int id, CancellationToken ct = default)
    {
        return SendWithResultAsync<PositionDto>(HttpMethod.Get, $"api/v1/hr/positions/{id}", accessToken, null, ct).ToDataAsync();
    }

    public Task<ApiCallResult<PositionDto>> CreatePositionAsync(string accessToken, PositionDto request, CancellationToken ct = default)
    {
        return SendWithResultAsync<PositionDto>(HttpMethod.Post, "api/v1/hr/positions", accessToken, request, ct);
    }

    public Task<ApiCallResult<PositionDto>> UpdatePositionAsync(string accessToken, int id, PositionDto request, CancellationToken ct = default)
    {
        return SendWithResultAsync<PositionDto>(HttpMethod.Put, $"api/v1/hr/positions/{id}", accessToken, request, ct);
    }

    public Task<ApiCallResult<object?>> DeletePositionAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Delete, $"api/v1/hr/positions/{id}", accessToken, null, ct);

    public Task<PagedResult<AttendanceReportDto>?> GetAttendancesAsync(string accessToken, AttendanceReportRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.EmployeeCode))
        {
            parameters.Add($"employeeCode={Uri.EscapeDataString(request.EmployeeCode.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(request.EmployeeName))
        {
            parameters.Add($"employeeName={Uri.EscapeDataString(request.EmployeeName.Trim())}");
        }

        if (request.EmployeeId.HasValue)
        {
            parameters.Add($"employeeId={request.EmployeeId.Value}");
        }

        if (request.DepartmentId.HasValue)
        {
            parameters.Add($"departmentId={request.DepartmentId.Value}");
        }

        if (request.DateFrom.HasValue)
        {
            parameters.Add($"dateFrom={request.DateFrom.Value:yyyy-MM-dd}");
        }

        if (request.DateTo.HasValue)
        {
            parameters.Add($"dateTo={request.DateTo.Value:yyyy-MM-dd}");
        }

        if (request.CheckInFrom.HasValue)
        {
            parameters.Add($"checkInFrom={request.CheckInFrom.Value:yyyy-MM-dd}");
        }

        if (request.CheckInTo.HasValue)
        {
            parameters.Add($"checkInTo={request.CheckInTo.Value:yyyy-MM-dd}");
        }

        if (request.CheckOutFrom.HasValue)
        {
            parameters.Add($"checkOutFrom={request.CheckOutFrom.Value:yyyy-MM-dd}");
        }

        if (request.CheckOutTo.HasValue)
        {
            parameters.Add($"checkOutTo={request.CheckOutTo.Value:yyyy-MM-dd}");
        }

        if (request.Status.HasValue)
        {
            parameters.Add($"status={(int)request.Status.Value}");
        }

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            parameters.Add($"notes={Uri.EscapeDataString(request.Notes.Trim())}");
        }

        var query = $"api/v1/hr/attendance?{string.Join("&", parameters)}";
        return SendWithResultAsync<PagedResult<AttendanceReportDto>>(HttpMethod.Get, query, accessToken, null, ct).ToDataAsync();
    }

    public Task<AttendanceDto?> GetAttendanceByIdAsync(string accessToken, int id, CancellationToken ct = default)
    {
        return SendWithResultAsync<AttendanceDto>(HttpMethod.Get, $"api/v1/hr/attendance/{id}", accessToken, null, ct).ToDataAsync();
    }

    public async Task<IReadOnlyList<AttendanceDto>> GetAttendancesByEmployeeAsync(string accessToken, int employeeId, CancellationToken ct = default)
    {
        if (employeeId <= 0)
        {
            return [];
        }

        return await SendWithResultAsync<IReadOnlyList<AttendanceDto>>(HttpMethod.Get, $"api/v1/hr/attendance/by-employee/{employeeId}", accessToken, null, ct).ToDataAsync() ?? [];
    }

    public Task<ApiCallResult<AttendanceDto>> CreateAttendanceAsync(string accessToken, AttendanceRecordRequest request, CancellationToken ct = default)
    {
        return SendWithResultAsync<AttendanceDto>(HttpMethod.Post, "api/v1/hr/attendance", accessToken, request, ct);
    }

    public Task<ApiCallResult<AttendanceDto>> UpdateAttendanceAsync(string accessToken, int id, AttendanceRecordRequest request, CancellationToken ct = default)
    {
        return SendWithResultAsync<AttendanceDto>(HttpMethod.Put, $"api/v1/hr/attendance/{id}", accessToken, request, ct);
    }

    public Task<ApiCallResult<object?>> DeleteAttendanceAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Delete, $"api/v1/hr/attendance/{id}", accessToken, null, ct);

    public Task<AttendanceSettingDto?> GetAttendanceSettingAsync(string accessToken, CancellationToken ct = default)
    {
        return SendWithResultAsync<AttendanceSettingDto>(HttpMethod.Get, "api/v1/hr/attendance/settings", accessToken, null, ct).ToDataAsync();
    }

    public Task<ApiCallResult<AttendanceSettingDto>> UpdateAttendanceSettingAsync(string accessToken, AttendanceSettingDto request, CancellationToken ct = default)
    {
        return SendWithResultAsync<AttendanceSettingDto>(HttpMethod.Put, "api/v1/hr/attendance/settings", accessToken, request, ct);
    }

    public Task<PagedResult<HolidayDto>?> GetHolidaysAsync(string accessToken, HolidayPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>();
        AddPagedParameters(parameters, request);

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            parameters.Add($"name={Uri.EscapeDataString(request.Name.Trim())}");
        }

        if (request.HolidayDateFrom.HasValue)
        {
            parameters.Add($"holidayDateFrom={request.HolidayDateFrom.Value:yyyy-MM-dd}");
        }

        if (request.HolidayDateTo.HasValue)
        {
            parameters.Add($"holidayDateTo={request.HolidayDateTo.Value:yyyy-MM-dd}");
        }

        if (request.HolidayType.HasValue)
        {
            parameters.Add($"holidayType={(int)request.HolidayType.Value}");
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            parameters.Add($"description={Uri.EscapeDataString(request.Description.Trim())}");
        }

        if (request.IsActive.HasValue)
        {
            parameters.Add($"isActive={(request.IsActive.Value ? "true" : "false")}");
        }

        if (!string.IsNullOrWhiteSpace(request.AppliesTo))
        {
            parameters.Add($"appliesTo={Uri.EscapeDataString(request.AppliesTo.Trim())}");
        }

        if (request.YearFrom.HasValue)
        {
            parameters.Add($"yearFrom={request.YearFrom.Value}");
        }

        if (request.YearTo.HasValue)
        {
            parameters.Add($"yearTo={request.YearTo.Value}");
        }

        var query = $"api/v1/hr/holidays?{string.Join("&", parameters)}";
        return SendWithResultAsync<PagedResult<HolidayDto>>(HttpMethod.Get, query, accessToken, null, ct).ToDataAsync();
    }

    public Task<HolidayDto?> GetHolidayByIdAsync(string accessToken, int id, CancellationToken ct = default)
    {
        return SendWithResultAsync<HolidayDto>(HttpMethod.Get, $"api/v1/hr/holidays/{id}", accessToken, null, ct).ToDataAsync();
    }

    public Task<ApiCallResult<HolidayDto>> CreateHolidayAsync(string accessToken, HolidayDto request, CancellationToken ct = default)
    {
        return SendWithResultAsync<HolidayDto>(HttpMethod.Post, "api/v1/hr/holidays", accessToken, request, ct);
    }

    public Task<ApiCallResult<HolidayDto>> UpdateHolidayAsync(string accessToken, int id, HolidayDto request, CancellationToken ct = default)
    {
        return SendWithResultAsync<HolidayDto>(HttpMethod.Put, $"api/v1/hr/holidays/{id}", accessToken, request, ct);
    }

    public Task<ApiCallResult<object?>> DeleteHolidayAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Delete, $"api/v1/hr/holidays/{id}", accessToken, null, ct);

    public Task<PagedResult<LeaveRequestDto>?> GetLeaveRequestsAsync(string accessToken, LeaveRequestPagedRequest request, CancellationToken ct = default)
    {
        var query =
            $"api/v1/hr/leave-requests?page={request.Page}&pageSize={request.PageSize}" +
            $"&search={Uri.EscapeDataString(request.Search ?? string.Empty)}" +
            $"&status={(request.Status.HasValue ? ((int)request.Status.Value).ToString() : string.Empty)}" +
            $"&employeeId={(request.EmployeeId.HasValue ? request.EmployeeId.Value.ToString() : string.Empty)}";

        return SendWithResultAsync<PagedResult<LeaveRequestDto>>(HttpMethod.Get, query, accessToken, null, ct).ToDataAsync();
    }

    public Task<LeaveRequestDto?> GetLeaveRequestByIdAsync(string accessToken, int id, CancellationToken ct = default)
    {
        return SendWithResultAsync<LeaveRequestDto>(HttpMethod.Get, $"api/v1/hr/leave-requests/{id}", accessToken, null, ct).ToDataAsync();
    }

    public Task<LeaveRequestOptionsDto?> GetLeaveRequestOptionsAsync(string accessToken, CancellationToken ct = default)
    {
        return SendWithResultAsync<LeaveRequestOptionsDto>(HttpMethod.Get, "api/v1/hr/leave-requests/options", accessToken, null, ct).ToDataAsync();
    }

    public async Task<ApiCallResult<SubmitLeaveRequestResult>> SubmitLeaveRequestAsync(string accessToken, SubmitLeaveRequest request, IReadOnlyList<IFormFile>? files, string? note, CancellationToken ct = default)
    {
        using var content = BuildLeaveRequestForm(request, files, note);
        return await SendMultipartAsync<SubmitLeaveRequestResult>(HttpMethod.Post, "api/v1/hr/leave-requests", accessToken, content, ct);
    }

    public async Task<ApiCallResult<SubmitLeaveRequestResult>> UpdateLeaveRequestAsync(string accessToken, int id, SubmitLeaveRequest request, IReadOnlyList<IFormFile>? files, string? note, CancellationToken ct = default)
    {
        using var content = BuildLeaveRequestForm(request, files, note);
        return await SendMultipartAsync<SubmitLeaveRequestResult>(HttpMethod.Put, $"api/v1/hr/leave-requests/{id}", accessToken, content, ct);
    }

    private static MultipartFormDataContent BuildLeaveRequestForm(SubmitLeaveRequest request, IReadOnlyList<IFormFile>? files, string? note)
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent(request.EmployeeId.ToString()), "EmployeeId" },
            { new StringContent(request.LeaveTypeId.ToString()), "LeaveTypeId" },
            { new StringContent(request.StartDate.ToString("yyyy-MM-dd")), "StartDate" },
            { new StringContent(request.EndDate.ToString("yyyy-MM-dd")), "EndDate" }
        };

        if (!string.IsNullOrWhiteSpace(request.Reason))
        {
            content.Add(new StringContent(request.Reason), "Reason");
        }

        if (!string.IsNullOrWhiteSpace(note))
        {
            content.Add(new StringContent(note), "Note");
        }

        if (files is not null)
        {
            foreach (var file in files.Where(f => f.Length > 0))
            {
                var streamContent = new StreamContent(file.OpenReadStream());
                streamContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(
                    string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);
                content.Add(streamContent, "Files", file.FileName);
            }
        }

        return content;
    }

    public Task<ApiCallResult<object?>> DeleteLeaveRequestAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Delete, $"api/v1/hr/leave-requests/{id}", accessToken, null, ct);

    public Task<ApiCallResult<object?>> ApproveLeaveRequestAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Put, $"api/v1/hr/leave-requests/{id}/approve", accessToken, null, ct);

    public Task<ApiCallResult<object?>> RejectLeaveRequestAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Put, $"api/v1/hr/leave-requests/{id}/reject", accessToken, null, ct);

    public Task<PagedResult<LeaveBalanceDto>?> GetLeaveBalancesAsync(string accessToken, LeaveBalanceRequest request, CancellationToken ct = default)
    {
        var query =
            $"api/v1/hr/leave-balance?page={request.Page}&pageSize={request.PageSize}" +
            $"&search={Uri.EscapeDataString(request.Search ?? string.Empty)}" +
            $"&sortBy={Uri.EscapeDataString(request.SortBy ?? string.Empty)}" +
            $"&sortDirection={Uri.EscapeDataString(request.SortDirection ?? string.Empty)}" +
            $"&year={(request.Year.HasValue ? request.Year.Value.ToString() : string.Empty)}" +
            $"&employeeId={(request.EmployeeId.HasValue ? request.EmployeeId.Value.ToString() : string.Empty)}" +
            $"&leaveTypeId={(request.LeaveTypeId.HasValue ? request.LeaveTypeId.Value.ToString() : string.Empty)}";

        return SendWithResultAsync<PagedResult<LeaveBalanceDto>>(HttpMethod.Get, query, accessToken, null, ct).ToDataAsync();
    }

    public Task<PagedResult<LeaveTypeDto>?> GetLeaveTypesAsync(string accessToken, LeaveTypePagedRequest request, CancellationToken ct = default)
    {
        var query =
            $"api/v1/hr/leave-types?page={request.Page}&pageSize={request.PageSize}" +
            $"&search={Uri.EscapeDataString(request.Search ?? string.Empty)}" +
            $"&sortBy={Uri.EscapeDataString(request.SortBy ?? string.Empty)}" +
            $"&sortDirection={Uri.EscapeDataString(request.SortDirection ?? string.Empty)}" +
            $"&name={Uri.EscapeDataString(request.Name ?? string.Empty)}" +
            $"&code={Uri.EscapeDataString(request.Code ?? string.Empty)}" +
            $"&maxDaysPerYearFrom={(request.MaxDaysPerYearFrom.HasValue ? request.MaxDaysPerYearFrom.Value.ToString() : string.Empty)}" +
            $"&maxDaysPerYearTo={(request.MaxDaysPerYearTo.HasValue ? request.MaxDaysPerYearTo.Value.ToString() : string.Empty)}" +
            $"&isCarryOver={(request.IsCarryOver.HasValue ? (request.IsCarryOver.Value ? "true" : "false") : string.Empty)}" +
            $"&isActive={(request.IsActive.HasValue ? (request.IsActive.Value ? "true" : "false") : string.Empty)}";

        return SendWithResultAsync<PagedResult<LeaveTypeDto>>(HttpMethod.Get, query, accessToken, null, ct).ToDataAsync();
    }

    public Task<LeaveTypeDto?> GetLeaveTypeByIdAsync(string accessToken, int id, CancellationToken ct = default)
    {
        return SendWithResultAsync<LeaveTypeDto>(HttpMethod.Get, $"api/v1/hr/leave-types/{id}", accessToken, null, ct).ToDataAsync();
    }

    public Task<ApiCallResult<LeaveTypeDto>> CreateLeaveTypeAsync(string accessToken, LeaveTypeDto request, CancellationToken ct = default)
    {
        return SendWithResultAsync<LeaveTypeDto>(HttpMethod.Post, "api/v1/hr/leave-types", accessToken, request, ct);
    }

    public Task<ApiCallResult<LeaveTypeDto>> UpdateLeaveTypeAsync(string accessToken, int id, LeaveTypeDto request, CancellationToken ct = default)
    {
        return SendWithResultAsync<LeaveTypeDto>(HttpMethod.Put, $"api/v1/hr/leave-types/{id}", accessToken, request, ct);
    }

    public Task<ApiCallResult<object?>> DeleteLeaveTypeAsync(string accessToken, int id, CancellationToken ct = default)
        => SendWithResultAsync<object?>(HttpMethod.Delete, $"api/v1/hr/leave-types/{id}", accessToken, null, ct);

    public Task<PagedResult<PayrollRunDto>?> GetPayrollRunsAsync(string accessToken, PayrollRunPagedRequest request, CancellationToken ct = default)
    {
        var query =
            $"api/v1/hr/payroll?page={request.Page}&pageSize={request.PageSize}" +
            $"&search={Uri.EscapeDataString(request.Search ?? string.Empty)}" +
            $"&month={(request.Month.HasValue ? request.Month.Value.ToString() : string.Empty)}" +
            $"&year={(request.Year.HasValue ? request.Year.Value.ToString() : string.Empty)}" +
            $"&status={(request.Status.HasValue ? ((int)request.Status.Value).ToString() : string.Empty)}";

        return SendWithResultAsync<PagedResult<PayrollRunDto>>(HttpMethod.Get, query, accessToken, null, ct).ToDataAsync();
    }

    public Task<ApiCallResult<PayrollRunDto>> RunPayrollAsync(string accessToken, PayrollRunRequest request, CancellationToken ct = default)
    {
        return SendWithResultAsync<PayrollRunDto>(HttpMethod.Post, "api/v1/hr/payroll/run", accessToken, request, ct);
    }

    public async Task<IReadOnlyList<PayrollRunDetailDto>> GetPayrollRunDetailsAsync(string accessToken, int runId, CancellationToken ct = default)
    {
        return await SendWithResultAsync<IReadOnlyList<PayrollRunDetailDto>>(HttpMethod.Get, $"api/v1/hr/payroll/{runId}/details", accessToken, null, ct).ToDataAsync() ?? [];
    }

    public Task<PayslipDto?> GetPayslipAsync(string accessToken, int runId, int employeeId, CancellationToken ct = default)
    {
        return SendWithResultAsync<PayslipDto>(HttpMethod.Get, $"api/v1/hr/payroll/{runId}/payslip/{employeeId}", accessToken, null, ct).ToDataAsync();
    }

    public async Task<IReadOnlyList<DocumentDto>> GetDocumentsAsync(string accessToken, string referenceType, int referenceId, CancellationToken ct = default)
    {
        var query = $"api/v1/documents?referenceType={Uri.EscapeDataString(referenceType)}&referenceId={referenceId}";
        return await SendWithResultAsync<IReadOnlyList<DocumentDto>>(HttpMethod.Get, query, accessToken, null, ct).ToDataAsync() ?? [];
    }

    public Task<DocumentReferenceTypeConfigDto?> GetDocumentConfigAsync(string accessToken, string referenceType, CancellationToken ct = default)
    {
        var query = $"api/v1/documents/config?referenceType={Uri.EscapeDataString(referenceType)}";
        return SendWithResultAsync<DocumentReferenceTypeConfigDto>(HttpMethod.Get, query, accessToken, null, ct).ToDataAsync();
    }

    public Task<DownloadResult?> DownloadDocumentAsync(string accessToken, int documentId, CancellationToken ct = default)
    {
        return DownloadAsync($"api/v1/documents/{documentId}/download", accessToken, ct);
    }

    public Task<ApiCallResult<object?>> DeleteDocumentAsync(string accessToken, int documentId, CancellationToken ct = default)
    {
        return SendWithResultAsync<object?>(HttpMethod.Delete, $"api/v1/documents/{documentId}", accessToken, null, ct);
    }

    private static void AddPagedParameters(List<string> parameters, PagedRequest request)
    {
        parameters.Add($"page={request.Page}");
        parameters.Add($"pageSize={request.PageSize}");
        parameters.Add($"search={Uri.EscapeDataString(request.Search ?? string.Empty)}");
        parameters.Add($"sortBy={Uri.EscapeDataString(request.SortBy ?? string.Empty)}");
        parameters.Add($"sortDirection={Uri.EscapeDataString(request.SortDirection ?? string.Empty)}");
    }
}


