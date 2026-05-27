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
        var parameters = new List<string>
        {
            $"page={request.Page}",
            $"pageSize={request.PageSize}",
            $"search={Uri.EscapeDataString(request.Search ?? string.Empty)}"
        };

        if (request.DepartmentId.HasValue)
        {
            parameters.Add($"departmentId={request.DepartmentId.Value}");
        }

        if (request.EmploymentStatus.HasValue)
        {
            parameters.Add($"employmentStatus={(int)request.EmploymentStatus.Value}");
        }

        var query = $"api/v1/hr/employees?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<EmployeeListDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<PagedResult<DepartmentDto>?> GetDepartmentsAsync(string accessToken, DepartmentPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>
        {
            $"page={request.Page}",
            $"pageSize={request.PageSize}",
            $"search={Uri.EscapeDataString(request.Search ?? string.Empty)}"
        };

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

    public Task<PagedResult<PositionDto>?> GetPositionsAsync(string accessToken, PositionPagedRequest request, CancellationToken ct = default)
    {
        var parameters = new List<string>
        {
            $"page={request.Page}",
            $"pageSize={request.PageSize}",
            $"search={Uri.EscapeDataString(request.Search ?? string.Empty)}"
        };

        if (request.DepartmentId.HasValue)
        {
            parameters.Add($"departmentId={request.DepartmentId.Value}");
        }

        if (request.IsActive.HasValue)
        {
            parameters.Add($"isActive={(request.IsActive.Value ? "true" : "false")}");
        }

        var query = $"api/v1/hr/positions?{string.Join("&", parameters)}";
        return SendAsync<PagedResult<PositionDto>>(HttpMethod.Get, query, accessToken, null, ct);
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

    public Task<LeaveRequestOptionsDto?> GetLeaveRequestOptionsAsync(string accessToken, CancellationToken ct = default)
    {
        return SendAsync<LeaveRequestOptionsDto>(HttpMethod.Get, "api/v1/hr/leave-requests/options", accessToken, null, ct);
    }

    public Task<LeaveRequestDto?> SubmitLeaveRequestAsync(string accessToken, SubmitLeaveRequest request, CancellationToken ct = default)
    {
        return SendAsync<LeaveRequestDto>(HttpMethod.Post, "api/v1/hr/leave-requests", accessToken, request, ct);
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
            $"&year={(request.Year.HasValue ? request.Year.Value.ToString() : string.Empty)}" +
            $"&employeeId={(request.EmployeeId.HasValue ? request.EmployeeId.Value.ToString() : string.Empty)}" +
            $"&leaveTypeId={(request.LeaveTypeId.HasValue ? request.LeaveTypeId.Value.ToString() : string.Empty)}";

        return SendAsync<PagedResult<LeaveBalanceDto>>(HttpMethod.Get, query, accessToken, null, ct);
    }

    public Task<PagedResult<LeaveTypeDto>?> GetLeaveTypesAsync(string accessToken, PagedRequest request, CancellationToken ct = default)
    {
        var query =
            $"api/v1/hr/leave-types?page={request.Page}&pageSize={request.PageSize}" +
            $"&search={Uri.EscapeDataString(request.Search ?? string.Empty)}";

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
