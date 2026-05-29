using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;

namespace ERP.Web.Services;

public sealed class HrApiClient(HttpClient httpClient, ILogger<HrApiClient> logger) : IHrApiClient
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
        return SendAsync<PagedResult<EmployeeListDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public async Task<IReadOnlyList<LookupDto>> GetEmployeeOptionsAsync(string accessToken, CancellationToken ct = default)
    {
        return await SendAsync<IReadOnlyList<LookupDto>>(HttpMethod.Get, "api/v1/hr/employees/options", accessToken, null, ct)
            ?? [];
    }

    public Task<EmployeeDetailDto?> GetEmployeeByIdAsync(string accessToken, int id, CancellationToken ct = default)
    {
        return SendAsync<EmployeeDetailDto>(HttpMethod.Get, $"api/v1/hr/employees/{id}", accessToken, null, ct);
    }

    public Task<EmployeeDetailDto?> CreateEmployeeAsync(string accessToken, CreateEmployeeRequest request, CancellationToken ct = default)
    {
        return SendAsync<EmployeeDetailDto>(HttpMethod.Post, "api/v1/hr/employees", accessToken, request, ct);
    }

    public Task<EmployeeDetailDto?> UpdateEmployeeAsync(string accessToken, int id, UpdateEmployeeRequest request, CancellationToken ct = default)
    {
        return SendAsync<EmployeeDetailDto>(HttpMethod.Put, $"api/v1/hr/employees/{id}", accessToken, request, ct);
    }

    public async Task<bool> DeleteEmployeeAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/hr/employees/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

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
        return SendAsync<PagedResult<DepartmentDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public async Task<IReadOnlyList<DepartmentDto>> GetDepartmentOptionsAsync(string accessToken, CancellationToken ct = default)
    {
        return await SendAsync<IReadOnlyList<DepartmentDto>>(HttpMethod.Get, "api/v1/hr/departments/all", accessToken, null, ct)
            ?? [];
    }

    public Task<DepartmentDto?> GetDepartmentByIdAsync(string accessToken, int id, CancellationToken ct = default)
    {
        return SendAsync<DepartmentDto>(HttpMethod.Get, $"api/v1/hr/departments/{id}", accessToken, null, ct);
    }

    public Task<DepartmentDto?> CreateDepartmentAsync(string accessToken, DepartmentDto request, CancellationToken ct = default)
    {
        return SendAsync<DepartmentDto>(HttpMethod.Post, "api/v1/hr/departments", accessToken, request, ct);
    }

    public Task<DepartmentDto?> UpdateDepartmentAsync(string accessToken, int id, DepartmentDto request, CancellationToken ct = default)
    {
        return SendAsync<DepartmentDto>(HttpMethod.Put, $"api/v1/hr/departments/{id}", accessToken, request, ct);
    }

    public async Task<bool> DeleteDepartmentAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/hr/departments/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

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
        return SendAsync<PagedResult<PositionDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public async Task<IReadOnlyList<PositionDto>> GetPositionOptionsAsync(string accessToken, CancellationToken ct = default)
    {
        return await SendAsync<IReadOnlyList<PositionDto>>(HttpMethod.Get, "api/v1/hr/positions/all", accessToken, null, ct)
            ?? [];
    }

    public async Task<IReadOnlyList<PositionDto>> GetPositionsByDepartmentAsync(string accessToken, int departmentId, CancellationToken ct = default)
    {
        if (departmentId <= 0)
        {
            return [];
        }

        return await SendAsync<IReadOnlyList<PositionDto>>(HttpMethod.Get, $"api/v1/hr/positions/by-department/{departmentId}", accessToken, null, ct)
            ?? [];
    }

    public Task<PositionDto?> GetPositionByIdAsync(string accessToken, int id, CancellationToken ct = default)
    {
        return SendAsync<PositionDto>(HttpMethod.Get, $"api/v1/hr/positions/{id}", accessToken, null, ct);
    }

    public Task<PositionDto?> CreatePositionAsync(string accessToken, PositionDto request, CancellationToken ct = default)
    {
        return SendAsync<PositionDto>(HttpMethod.Post, "api/v1/hr/positions", accessToken, request, ct);
    }

    public Task<PositionDto?> UpdatePositionAsync(string accessToken, int id, PositionDto request, CancellationToken ct = default)
    {
        return SendAsync<PositionDto>(HttpMethod.Put, $"api/v1/hr/positions/{id}", accessToken, request, ct);
    }

    public async Task<bool> DeletePositionAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/hr/positions/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

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
        return SendAsync<PagedResult<AttendanceReportDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<AttendanceDto?> GetAttendanceByIdAsync(string accessToken, int id, CancellationToken ct = default)
    {
        return SendAsync<AttendanceDto>(HttpMethod.Get, $"api/v1/hr/attendance/{id}", accessToken, null, ct);
    }

    public async Task<IReadOnlyList<AttendanceDto>> GetAttendancesByEmployeeAsync(string accessToken, int employeeId, CancellationToken ct = default)
    {
        if (employeeId <= 0)
        {
            return [];
        }

        return await SendAsync<IReadOnlyList<AttendanceDto>>(HttpMethod.Get, $"api/v1/hr/attendance/by-employee/{employeeId}", accessToken, null, ct)
            ?? [];
    }

    public Task<AttendanceDto?> CreateAttendanceAsync(string accessToken, AttendanceRecordRequest request, CancellationToken ct = default)
    {
        return SendAsync<AttendanceDto>(HttpMethod.Post, "api/v1/hr/attendance", accessToken, request, ct);
    }

    public Task<AttendanceDto?> UpdateAttendanceAsync(string accessToken, int id, AttendanceRecordRequest request, CancellationToken ct = default)
    {
        return SendAsync<AttendanceDto>(HttpMethod.Put, $"api/v1/hr/attendance/{id}", accessToken, request, ct);
    }

    public async Task<bool> DeleteAttendanceAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/hr/attendance/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<AttendanceSettingDto?> GetAttendanceSettingAsync(string accessToken, CancellationToken ct = default)
    {
        return SendAsync<AttendanceSettingDto>(HttpMethod.Get, "api/v1/hr/attendance/settings", accessToken, null, ct);
    }

    public Task<AttendanceSettingDto?> UpdateAttendanceSettingAsync(string accessToken, AttendanceSettingDto request, CancellationToken ct = default)
    {
        return SendAsync<AttendanceSettingDto>(HttpMethod.Put, "api/v1/hr/attendance/settings", accessToken, request, ct);
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
        return SendAsync<PagedResult<HolidayDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<HolidayDto?> GetHolidayByIdAsync(string accessToken, int id, CancellationToken ct = default)
    {
        return SendAsync<HolidayDto>(HttpMethod.Get, $"api/v1/hr/holidays/{id}", accessToken, null, ct);
    }

    public Task<HolidayDto?> CreateHolidayAsync(string accessToken, HolidayDto request, CancellationToken ct = default)
    {
        return SendAsync<HolidayDto>(HttpMethod.Post, "api/v1/hr/holidays", accessToken, request, ct);
    }

    public Task<HolidayDto?> UpdateHolidayAsync(string accessToken, int id, HolidayDto request, CancellationToken ct = default)
    {
        return SendAsync<HolidayDto>(HttpMethod.Put, $"api/v1/hr/holidays/{id}", accessToken, request, ct);
    }

    public async Task<bool> DeleteHolidayAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/hr/holidays/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<LeaveRequestDto>?> GetLeaveRequestsAsync(string accessToken, LeaveRequestPagedRequest request, CancellationToken ct = default)
    {
        var query =
            $"api/v1/hr/leave-requests?page={request.Page}&pageSize={request.PageSize}" +
            $"&search={Uri.EscapeDataString(request.Search ?? string.Empty)}" +
            $"&status={(request.Status.HasValue ? ((int)request.Status.Value).ToString() : string.Empty)}" +
            $"&employeeId={(request.EmployeeId.HasValue ? request.EmployeeId.Value.ToString() : string.Empty)}";

        return SendAsync<PagedResult<LeaveRequestDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<LeaveRequestDto?> GetLeaveRequestByIdAsync(string accessToken, int id, CancellationToken ct = default)
    {
        return SendAsync<LeaveRequestDto>(HttpMethod.Get, $"api/v1/hr/leave-requests/{id}", accessToken, null, ct);
    }

    public Task<LeaveRequestOptionsDto?> GetLeaveRequestOptionsAsync(string accessToken, CancellationToken ct = default)
    {
        return SendAsync<LeaveRequestOptionsDto>(HttpMethod.Get, "api/v1/hr/leave-requests/options", accessToken, null, ct);
    }

    public Task<LeaveRequestDto?> SubmitLeaveRequestAsync(string accessToken, SubmitLeaveRequest request, CancellationToken ct = default)
    {
        return SendAsync<LeaveRequestDto>(HttpMethod.Post, "api/v1/hr/leave-requests", accessToken, request, ct);
    }

    public Task<LeaveRequestDto?> UpdateLeaveRequestAsync(string accessToken, int id, SubmitLeaveRequest request, CancellationToken ct = default)
    {
        return SendAsync<LeaveRequestDto>(HttpMethod.Put, $"api/v1/hr/leave-requests/{id}", accessToken, request, ct);
    }

    public async Task<bool> DeleteLeaveRequestAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/hr/leave-requests/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public async Task<bool> ApproveLeaveRequestAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Put, $"api/v1/hr/leave-requests/{id}/approve", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public async Task<bool> RejectLeaveRequestAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Put, $"api/v1/hr/leave-requests/{id}/reject", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

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

        return SendAsync<PagedResult<LeaveBalanceDto>>(HttpMethod.Get, query, accessToken, null, ct);
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

        return SendAsync<PagedResult<LeaveTypeDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<LeaveTypeDto?> GetLeaveTypeByIdAsync(string accessToken, int id, CancellationToken ct = default)
    {
        return SendAsync<LeaveTypeDto>(HttpMethod.Get, $"api/v1/hr/leave-types/{id}", accessToken, null, ct);
    }

    public Task<LeaveTypeDto?> CreateLeaveTypeAsync(string accessToken, LeaveTypeDto request, CancellationToken ct = default)
    {
        return SendAsync<LeaveTypeDto>(HttpMethod.Post, "api/v1/hr/leave-types", accessToken, request, ct);
    }

    public Task<LeaveTypeDto?> UpdateLeaveTypeAsync(string accessToken, int id, LeaveTypeDto request, CancellationToken ct = default)
    {
        return SendAsync<LeaveTypeDto>(HttpMethod.Put, $"api/v1/hr/leave-types/{id}", accessToken, request, ct);
    }

    public async Task<bool> DeleteLeaveTypeAsync(string accessToken, int id, CancellationToken ct = default)
    {
        var response = await SendRawAsync(HttpMethod.Delete, $"api/v1/hr/leave-types/{id}", accessToken, null, ct);
        return response?.IsSuccessStatusCode == true;
    }

    public Task<PagedResult<PayrollRunDto>?> GetPayrollRunsAsync(string accessToken, PayrollRunPagedRequest request, CancellationToken ct = default)
    {
        var query =
            $"api/v1/hr/payroll?page={request.Page}&pageSize={request.PageSize}" +
            $"&search={Uri.EscapeDataString(request.Search ?? string.Empty)}" +
            $"&month={(request.Month.HasValue ? request.Month.Value.ToString() : string.Empty)}" +
            $"&year={(request.Year.HasValue ? request.Year.Value.ToString() : string.Empty)}" +
            $"&status={(request.Status.HasValue ? ((int)request.Status.Value).ToString() : string.Empty)}";

        return SendAsync<PagedResult<PayrollRunDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<PayrollRunDto?> RunPayrollAsync(string accessToken, PayrollRunRequest request, CancellationToken ct = default)
    {
        return SendAsync<PayrollRunDto>(HttpMethod.Post, "api/v1/hr/payroll/run", accessToken, request, ct);
    }

    public async Task<IReadOnlyList<PayrollRunDetailDto>> GetPayrollRunDetailsAsync(string accessToken, int runId, CancellationToken ct = default)
    {
        return await SendAsync<IReadOnlyList<PayrollRunDetailDto>>(HttpMethod.Get, $"api/v1/hr/payroll/{runId}/details", accessToken, null, ct)
            ?? [];
    }

    public Task<PayslipDto?> GetPayslipAsync(string accessToken, int runId, int employeeId, CancellationToken ct = default)
    {
        return SendAsync<PayslipDto>(HttpMethod.Get, $"api/v1/hr/payroll/{runId}/payslip/{employeeId}", accessToken, null, ct);
    }

    private static void AddPagedParameters(List<string> parameters, PagedRequest request)
    {
        parameters.Add($"page={request.Page}");
        parameters.Add($"pageSize={request.PageSize}");
        parameters.Add($"search={Uri.EscapeDataString(request.Search ?? string.Empty)}");
        parameters.Add($"sortBy={Uri.EscapeDataString(request.SortBy ?? string.Empty)}");
        parameters.Add($"sortDirection={Uri.EscapeDataString(request.SortDirection ?? string.Empty)}");
    }

    private async Task<T?> SendAsync<T>(HttpMethod method, string uri, string accessToken, object? body, CancellationToken ct)
    {
        var response = await SendRawAsync(method, uri, accessToken, body, ct);
        if (response is null || !response.IsSuccessStatusCode)
        {
            return default;
        }

        try
        {
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to deserialize HR API response from {Uri}", uri);
            return default;
        }
    }

    private async Task<HttpResponseMessage?> SendRawAsync(HttpMethod method, string uri, string accessToken, object? body, CancellationToken ct)
    {
        try
        {
            using var request = new HttpRequestMessage(method, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            if (body is not null)
            {
                request.Content = JsonContent.Create(body);
            }

            var response = await httpClient.SendAsync(request, ct);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new ApiUnauthorizedException(uri);
            }

            return response;
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Failed to call HR API endpoint {Uri}", uri);
            return null;
        }
    }
}


