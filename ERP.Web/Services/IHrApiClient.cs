using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;

namespace ERP.Web.Services;

public interface IHrApiClient
{
    Task<PagedResult<EmployeeListDto>?> GetEmployeesAsync(string accessToken, EmployeePagedRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<LookupDto>> GetEmployeeOptionsAsync(string accessToken, CancellationToken ct = default);
    Task<EmployeeDetailDto?> GetEmployeeByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<EmployeeDetailDto?> CreateEmployeeAsync(string accessToken, CreateEmployeeRequest request, CancellationToken ct = default);
    Task<EmployeeDetailDto?> UpdateEmployeeAsync(string accessToken, int id, UpdateEmployeeRequest request, CancellationToken ct = default);
    Task<bool> DeleteEmployeeAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<DepartmentDto>?> GetDepartmentsAsync(string accessToken, DepartmentPagedRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<DepartmentDto>> GetDepartmentOptionsAsync(string accessToken, CancellationToken ct = default);
    Task<DepartmentDto?> GetDepartmentByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<DepartmentDto?> CreateDepartmentAsync(string accessToken, DepartmentDto request, CancellationToken ct = default);
    Task<DepartmentDto?> UpdateDepartmentAsync(string accessToken, int id, DepartmentDto request, CancellationToken ct = default);
    Task<bool> DeleteDepartmentAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<PositionDto>?> GetPositionsAsync(string accessToken, PositionPagedRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<PositionDto>> GetPositionOptionsAsync(string accessToken, CancellationToken ct = default);
    Task<IReadOnlyList<PositionDto>> GetPositionsByDepartmentAsync(string accessToken, int departmentId, CancellationToken ct = default);
    Task<PositionDto?> GetPositionByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<PositionDto?> CreatePositionAsync(string accessToken, PositionDto request, CancellationToken ct = default);
    Task<PositionDto?> UpdatePositionAsync(string accessToken, int id, PositionDto request, CancellationToken ct = default);
    Task<bool> DeletePositionAsync(string accessToken, int id, CancellationToken ct = default);
    Task<PagedResult<AttendanceReportDto>?> GetAttendancesAsync(string accessToken, AttendanceReportRequest request, CancellationToken ct = default);
    Task<AttendanceDto?> GetAttendanceByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<IReadOnlyList<AttendanceDto>> GetAttendancesByEmployeeAsync(string accessToken, int employeeId, CancellationToken ct = default);
    Task<AttendanceDto?> CreateAttendanceAsync(string accessToken, AttendanceRecordRequest request, CancellationToken ct = default);
    Task<AttendanceDto?> UpdateAttendanceAsync(string accessToken, int id, AttendanceRecordRequest request, CancellationToken ct = default);
    Task<bool> DeleteAttendanceAsync(string accessToken, int id, CancellationToken ct = default);
    Task<AttendanceSettingDto?> GetAttendanceSettingAsync(string accessToken, CancellationToken ct = default);
    Task<AttendanceSettingDto?> UpdateAttendanceSettingAsync(string accessToken, AttendanceSettingDto request, CancellationToken ct = default);

    Task<PagedResult<HolidayDto>?> GetHolidaysAsync(string accessToken, HolidayPagedRequest request, CancellationToken ct = default);
    Task<HolidayDto?> GetHolidayByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<HolidayDto?> CreateHolidayAsync(string accessToken, HolidayDto request, CancellationToken ct = default);
    Task<HolidayDto?> UpdateHolidayAsync(string accessToken, int id, HolidayDto request, CancellationToken ct = default);
    Task<bool> DeleteHolidayAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<LeaveRequestDto>?> GetLeaveRequestsAsync(string accessToken, LeaveRequestPagedRequest request, CancellationToken ct = default);
    Task<LeaveRequestDto?> GetLeaveRequestByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<LeaveRequestOptionsDto?> GetLeaveRequestOptionsAsync(string accessToken, CancellationToken ct = default);
    Task<LeaveRequestDto?> SubmitLeaveRequestAsync(string accessToken, SubmitLeaveRequest request, CancellationToken ct = default);
    Task<LeaveRequestDto?> UpdateLeaveRequestAsync(string accessToken, int id, SubmitLeaveRequest request, CancellationToken ct = default);
    Task<bool> DeleteLeaveRequestAsync(string accessToken, int id, CancellationToken ct = default);
    Task<bool> ApproveLeaveRequestAsync(string accessToken, int id, CancellationToken ct = default);
    Task<bool> RejectLeaveRequestAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<LeaveBalanceDto>?> GetLeaveBalancesAsync(string accessToken, LeaveBalanceRequest request, CancellationToken ct = default);

    Task<PagedResult<LeaveTypeDto>?> GetLeaveTypesAsync(string accessToken, LeaveTypePagedRequest request, CancellationToken ct = default);
    Task<LeaveTypeDto?> GetLeaveTypeByIdAsync(string accessToken, int id, CancellationToken ct = default);
    Task<LeaveTypeDto?> CreateLeaveTypeAsync(string accessToken, LeaveTypeDto request, CancellationToken ct = default);
    Task<LeaveTypeDto?> UpdateLeaveTypeAsync(string accessToken, int id, LeaveTypeDto request, CancellationToken ct = default);
    Task<bool> DeleteLeaveTypeAsync(string accessToken, int id, CancellationToken ct = default);

    Task<PagedResult<PayrollRunDto>?> GetPayrollRunsAsync(string accessToken, PayrollRunPagedRequest request, CancellationToken ct = default);
    Task<PayrollRunDto?> RunPayrollAsync(string accessToken, PayrollRunRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<PayrollRunDetailDto>> GetPayrollRunDetailsAsync(string accessToken, int runId, CancellationToken ct = default);
    Task<PayslipDto?> GetPayslipAsync(string accessToken, int runId, int employeeId, CancellationToken ct = default);
}


