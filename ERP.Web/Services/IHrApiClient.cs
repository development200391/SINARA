using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Document;
using ERP.Application.DTOs.HR;
using Microsoft.AspNetCore.Http;

namespace ERP.Web.Services;

public interface IHrApiClient
{
    Task<PagedResult<EmployeeListDto>?> GetEmployeesAsync(string accessToken, EmployeePagedRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<LookupDto>> GetEmployeeOptionsAsync(string accessToken, CancellationToken ct = default);
    Task<EmployeeDetailDto?> GetEmployeeByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<EmployeeDetailDto>> CreateEmployeeAsync(string accessToken, CreateEmployeeRequest request, CancellationToken ct = default);
    Task<ApiCallResult<EmployeeDetailDto>> UpdateEmployeeAsync(string accessToken, int id, UpdateEmployeeRequest request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteEmployeeAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<DepartmentDto>?> GetDepartmentsAsync(string accessToken, DepartmentPagedRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<DepartmentDto>> GetDepartmentOptionsAsync(string accessToken, CancellationToken ct = default);
    Task<DepartmentDto?> GetDepartmentByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<DepartmentDto>> CreateDepartmentAsync(string accessToken, DepartmentDto request, CancellationToken ct = default);
    Task<ApiCallResult<DepartmentDto>> UpdateDepartmentAsync(string accessToken, int id, DepartmentDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteDepartmentAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<PositionDto>?> GetPositionsAsync(string accessToken, PositionPagedRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<PositionDto>> GetPositionOptionsAsync(string accessToken, CancellationToken ct = default);
    Task<IReadOnlyList<PositionDto>> GetPositionsByDepartmentAsync(string accessToken, int departmentId, CancellationToken ct = default);
    Task<PositionDto?> GetPositionByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<PositionDto>> CreatePositionAsync(string accessToken, PositionDto request, CancellationToken ct = default);
    Task<ApiCallResult<PositionDto>> UpdatePositionAsync(string accessToken, int id, PositionDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeletePositionAsync(string accessToken, int id, CancellationToken ct = default);
    Task<PagedResult<AttendanceReportDto>?> GetAttendancesAsync(string accessToken, AttendanceReportRequest request, CancellationToken ct = default);
    Task<AttendanceDto?> GetAttendanceByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<IReadOnlyList<AttendanceDto>> GetAttendancesByEmployeeAsync(string accessToken, int employeeId, CancellationToken ct = default);
    Task<ApiCallResult<AttendanceDto>> CreateAttendanceAsync(string accessToken, AttendanceRecordRequest request, CancellationToken ct = default);
    Task<ApiCallResult<AttendanceDto>> UpdateAttendanceAsync(string accessToken, int id, AttendanceRecordRequest request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteAttendanceAsync(string accessToken, int id, CancellationToken ct = default);
    Task<AttendanceSettingDto?> GetAttendanceSettingAsync(string accessToken, CancellationToken ct = default);
    Task<ApiCallResult<AttendanceSettingDto>> UpdateAttendanceSettingAsync(string accessToken, AttendanceSettingDto request, CancellationToken ct = default);

    Task<PagedResult<HolidayDto>?> GetHolidaysAsync(string accessToken, HolidayPagedRequest request, CancellationToken ct = default);
    Task<HolidayDto?> GetHolidayByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<HolidayDto>> CreateHolidayAsync(string accessToken, HolidayDto request, CancellationToken ct = default);
    Task<ApiCallResult<HolidayDto>> UpdateHolidayAsync(string accessToken, int id, HolidayDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteHolidayAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<LeaveRequestDto>?> GetLeaveRequestsAsync(string accessToken, LeaveRequestPagedRequest request, CancellationToken ct = default);
    Task<LeaveRequestDto?> GetLeaveRequestByIdAsync(string accessToken, int id, CancellationToken ct = default);
    /// <summary>
    /// <paramref name="scopeEmployeesToCurrentUser"/> restricts the Employees list to what the
    /// current user may submit/manage leave for (see ILeaveService.GetEmployeeOptionsAsync) — pass
    /// true for the admin Create/Edit forms; leave false (default) for callers like Leave Balance
    /// that intentionally need the unrestricted list.
    /// </summary>
    Task<LeaveRequestOptionsDto?> GetLeaveRequestOptionsAsync(string accessToken, bool scopeEmployeesToCurrentUser = false, CancellationToken ct = default);
    Task<ApiCallResult<SubmitLeaveRequestResult>> SubmitLeaveRequestAsync(string accessToken, SubmitLeaveRequest request, IReadOnlyList<IFormFile>? files, IReadOnlyList<string?>? notes, CancellationToken ct = default);
    Task<ApiCallResult<SubmitLeaveRequestResult>> UpdateLeaveRequestAsync(string accessToken, int id, SubmitLeaveRequest request, IReadOnlyList<IFormFile>? files, IReadOnlyList<string?>? notes, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteLeaveRequestAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<object?>> ApproveLeaveRequestAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<object?>> RejectLeaveRequestAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<LeaveBalanceDto>?> GetLeaveBalancesAsync(string accessToken, LeaveBalanceRequest request, CancellationToken ct = default);

    Task<PagedResult<LeaveTypeDto>?> GetLeaveTypesAsync(string accessToken, LeaveTypePagedRequest request, CancellationToken ct = default);
    Task<LeaveTypeDto?> GetLeaveTypeByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<ApiCallResult<LeaveTypeDto>> CreateLeaveTypeAsync(string accessToken, LeaveTypeDto request, CancellationToken ct = default);
    Task<ApiCallResult<LeaveTypeDto>> UpdateLeaveTypeAsync(string accessToken, int id, LeaveTypeDto request, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteLeaveTypeAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<PayrollRunDto>?> GetPayrollRunsAsync(string accessToken, PayrollRunPagedRequest request, CancellationToken ct = default);
    Task<ApiCallResult<PayrollRunDto>> RunPayrollAsync(string accessToken, PayrollRunRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<PayrollRunDetailDto>> GetPayrollRunDetailsAsync(string accessToken, int runId, CancellationToken ct = default);
    Task<PayslipDto?> GetPayslipAsync(string accessToken, int runId, int employeeId, CancellationToken ct = default);

    Task<IReadOnlyList<DocumentDto>> GetDocumentsAsync(string accessToken, string referenceType, int referenceId, CancellationToken ct = default);
    Task<DocumentReferenceTypeConfigDto?> GetDocumentConfigAsync(string accessToken, string referenceType, CancellationToken ct = default);
    Task<DownloadResult?> DownloadDocumentAsync(string accessToken, int documentId, CancellationToken ct = default);
    Task<ApiCallResult<object?>> DeleteDocumentAsync(string accessToken, int documentId, CancellationToken ct = default);
}
