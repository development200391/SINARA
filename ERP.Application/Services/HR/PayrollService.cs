using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.HR;
using ERP.Domain.Entities.HR;
using ERP.Domain.Enums;
using ERP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Services.HR;

public sealed class PayrollService(IUnitOfWork unitOfWork) : IPayrollService
{
    private const decimal BaseSalaryFloor = 3_000_000m;
    private const decimal LevelSalaryStep = 1_000_000m;
    private const decimal AllowanceRate = 0.20m;
    private const decimal TaxRate = 0.05m;
    private const decimal AbsentPenalty = 50_000m;
    private const decimal LatePenalty = 20_000m;

    public async Task<PagedResult<PayrollRunDto>> GetRunsAsync(PayrollRunPagedRequest request, CancellationToken ct = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var query = unitOfWork.Repository<HrPayrollRun>()
            .Query()
            .AsNoTracking()
            .Include(x => x.ProcessedByUser)
            .Include(x => x.PayrollDetails)
            .AsQueryable();

        if (request.Month.HasValue)
        {
            query = query.Where(x => x.PeriodMonth == request.Month.Value);
        }

        if (request.Year.HasValue)
        {
            query = query.Where(x => x.PeriodYear == request.Year.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.PeriodYear.ToString().Contains(search) ||
                x.PeriodMonth.ToString().Contains(search));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.PeriodYear)
            .ThenByDescending(x => x.PeriodMonth)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new PayrollRunDto
            {
                Id = x.Id,
                PeriodMonth = x.PeriodMonth,
                PeriodYear = x.PeriodYear,
                Status = x.Status,
                ProcessedBy = x.ProcessedBy,
                ProcessedByName = x.ProcessedByUser != null ? x.ProcessedByUser.FullName : null,
                ProcessedAt = x.ProcessedAt,
                TotalEmployees = x.PayrollDetails.Count,
                TotalNetSalary = x.PayrollDetails.Sum(d => d.NetSalary)
            })
            .ToListAsync(ct);

        return PagedResult<PayrollRunDto>.Create(items, total, page, pageSize);
    }

    public async Task<PayrollRunDto> RunPayrollAsync(int month, int year, int processedByUserId, CancellationToken ct = default)
    {
        if (month is < 1 or > 12)
        {
            throw new InvalidOperationException("Month must be between 1 and 12.");
        }

        if (year is < 2000 or > 9999)
        {
            throw new InvalidOperationException("Year is invalid.");
        }

        var run = await unitOfWork.Repository<HrPayrollRun>()
            .Query()
            .Include(x => x.PayrollDetails)
            .FirstOrDefaultAsync(x => x.PeriodMonth == month && x.PeriodYear == year, ct);

        //if (run is not null && run.Status == PayrollStatus.Completed)
        //{
        //    throw new InvalidOperationException("Payroll for selected period has already been completed.");
        //}

        if (run is null)
        {
            run = new HrPayrollRun
            {
                PeriodMonth = month,
                PeriodYear = year,
                Status = PayrollStatus.Processing,
                ProcessedBy = processedByUserId,
                ProcessedAt = DateTimeOffset.UtcNow,
                CreatedBy = "system"
            };

            await unitOfWork.Repository<HrPayrollRun>().AddAsync(run, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }
        else
        {
            run.Status = PayrollStatus.Processing;
            run.ProcessedBy = processedByUserId;
            run.ProcessedAt = DateTimeOffset.UtcNow;
            run.UpdatedBy = "system";
            run.UpdatedAt = DateTimeOffset.UtcNow;
            unitOfWork.Repository<HrPayrollRun>().Update(run);
            await unitOfWork.SaveChangesAsync(ct);
        }

        var employees = await unitOfWork.Repository<HrEmployee>()
            .Query()
            .AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.Position)
            .Where(x => x.EmploymentStatus == EmploymentStatus.Active)
            .ToListAsync(ct);

        var monthAttendances = await unitOfWork.Repository<HrAttendanceRecord>()
            .Query()
            .AsNoTracking()
            .Where(x => x.Date.Year == year && x.Date.Month == month)
            .GroupBy(x => x.EmployeeId)
            .Select(g => new
            {
                EmployeeId = g.Key,
                Absent = g.Count(x => x.Status == AttendanceStatus.Absent),
                Late = g.Count(x => x.Status == AttendanceStatus.Late)
            })
            .ToListAsync(ct);

        var attendanceLookup = monthAttendances.ToDictionary(x => x.EmployeeId, x => (x.Absent, x.Late));

        var existingDetailMap = run.PayrollDetails.ToDictionary(x => x.EmployeeId, x => x);

        foreach (var employee in employees)
        {
            var attendance = attendanceLookup.GetValueOrDefault(employee.Id);

            var level = employee.Position?.Level ?? 1;
            var basicSalary = BaseSalaryFloor + (level * LevelSalaryStep);
            var allowances = decimal.Round(basicSalary * AllowanceRate, 2, MidpointRounding.AwayFromZero);
            var deductions = (attendance.Absent * AbsentPenalty) + (attendance.Late * LatePenalty);
            var grossSalary = basicSalary + allowances;
            var tax = decimal.Round(grossSalary * TaxRate, 2, MidpointRounding.AwayFromZero);
            var netSalary = grossSalary - deductions - tax;
            if (netSalary < 0)
            {
                netSalary = 0;
            }

            if (existingDetailMap.TryGetValue(employee.Id, out var detail))
            {
                detail.BasicSalary = basicSalary;
                detail.Allowances = allowances;
                detail.Deductions = deductions;
                detail.GrossSalary = grossSalary;
                detail.TaxAmount = tax;
                detail.NetSalary = netSalary;
                detail.UpdatedBy = "system";
                detail.UpdatedAt = DateTimeOffset.UtcNow;

                unitOfWork.Repository<HrPayrollDetail>().Update(detail);
                continue;
            }

            await unitOfWork.Repository<HrPayrollDetail>().AddAsync(new HrPayrollDetail
            {
                PayrollRunId = run.Id,
                EmployeeId = employee.Id,
                BasicSalary = basicSalary,
                Allowances = allowances,
                Deductions = deductions,
                GrossSalary = grossSalary,
                TaxAmount = tax,
                NetSalary = netSalary,
                CreatedBy = "system"
            }, ct);
        }

        run.Status = PayrollStatus.Completed;
        run.ProcessedBy = processedByUserId;
        run.ProcessedAt = DateTimeOffset.UtcNow;
        run.UpdatedBy = "system";
        run.UpdatedAt = DateTimeOffset.UtcNow;

        unitOfWork.Repository<HrPayrollRun>().Update(run);
        await unitOfWork.SaveChangesAsync(ct);

        var runDto = await unitOfWork.Repository<HrPayrollRun>()
            .Query()
            .AsNoTracking()
            .Include(x => x.ProcessedByUser)
            .Include(x => x.PayrollDetails)
            .Where(x => x.Id == run.Id)
            .Select(x => new PayrollRunDto
            {
                Id = x.Id,
                PeriodMonth = x.PeriodMonth,
                PeriodYear = x.PeriodYear,
                Status = x.Status,
                ProcessedBy = x.ProcessedBy,
                ProcessedByName = x.ProcessedByUser != null ? x.ProcessedByUser.FullName : null,
                ProcessedAt = x.ProcessedAt,
                TotalEmployees = x.PayrollDetails.Count,
                TotalNetSalary = x.PayrollDetails.Sum(d => d.NetSalary)
            })
            .FirstAsync(ct);

        return runDto;
    }

    public async Task<IReadOnlyList<PayrollRunDetailDto>> GetRunDetailsAsync(int runId, CancellationToken ct = default)
    {
        return await unitOfWork.Repository<HrPayrollDetail>()
            .Query()
            .AsNoTracking()
            .Include(x => x.Employee)
            .ThenInclude(x => x.Department)
            .Include(x => x.Employee)
            .ThenInclude(x => x.Position)
            .Where(x => x.PayrollRunId == runId)
            .OrderBy(x => x.Employee.FullName)
            .Select(x => new PayrollRunDetailDto
            {
                EmployeeId = x.EmployeeId,
                EmployeeCode = x.Employee.EmployeeCode,
                EmployeeName = x.Employee.FullName,
                DepartmentName = x.Employee.Department.Name,
                PositionName = x.Employee.Position.Name,
                BasicSalary = x.BasicSalary,
                Allowances = x.Allowances,
                Deductions = x.Deductions,
                GrossSalary = x.GrossSalary,
                TaxAmount = x.TaxAmount,
                NetSalary = x.NetSalary
            })
            .ToListAsync(ct);
    }

    public async Task<PayslipDto?> GetPayslipAsync(int runId, int employeeId, CancellationToken ct = default)
    {
        return await unitOfWork.Repository<HrPayrollDetail>()
            .Query()
            .AsNoTracking()
            .Include(x => x.PayrollRun)
            .Include(x => x.Employee)
            .ThenInclude(x => x.Department)
            .Include(x => x.Employee)
            .ThenInclude(x => x.Position)
            .Where(x => x.PayrollRunId == runId && x.EmployeeId == employeeId)
            .Select(x => new PayslipDto
            {
                PayrollRunId = x.PayrollRunId,
                EmployeeId = x.EmployeeId,
                EmployeeCode = x.Employee.EmployeeCode,
                EmployeeName = x.Employee.FullName,
                DepartmentName = x.Employee.Department.Name,
                PositionName = x.Employee.Position.Name,
                PeriodMonth = x.PayrollRun.PeriodMonth,
                PeriodYear = x.PayrollRun.PeriodYear,
                BasicSalary = x.BasicSalary,
                Allowances = x.Allowances,
                Deductions = x.Deductions,
                GrossSalary = x.GrossSalary,
                TaxAmount = x.TaxAmount,
                NetSalary = x.NetSalary
            })
            .FirstOrDefaultAsync(ct);
    }
}
