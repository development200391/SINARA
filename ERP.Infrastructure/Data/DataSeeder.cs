using ERP.Application.Services;
using ERP.Domain.Entities.Config;
using ERP.Domain.Entities.HR;
using ERP.Domain.Entities.Finance;
using ERP.Domain.Entities.System;
using ERP.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Data;

public sealed class DataSeeder(AppDbContext dbContext) : IDataSeeder
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        await SeedModulesAsync(now, ct);
        await SeedRolesAsync(now, ct);
        await SeedHrMasterDataAsync(now, ct);
        await SeedLeaveTypesAsync(now, ct);
        await SeedAttendanceSettingAsync(now, ct);
        await SeedFinanceMasterDataAsync(now, ct);
        await SeedAdminUserAsync(now, ct);
        await SeedMenusAsync(now, ct);
        await SeedSuperAdminPermissionsAsync(ct);
    }

    private async Task SeedModulesAsync(DateTimeOffset now, CancellationToken ct)
    {
        await EnsureModuleAsync("Human Resources", "HR", "bi-people", 1, now, ct);
        await EnsureModuleAsync("System Configuration", "CFG", "bi-gear", 2, now, ct);
        await EnsureModuleAsync("Finance", "FIN", "bi-cash-coin", 3, now, ct);
    }

    private async Task SeedRolesAsync(DateTimeOffset now, CancellationToken ct)
    {
        var roleNames = new[]
        {
            "Super Admin",
            "HR Manager",
            "HR Staff",
            "Employee",
            "Viewer"
        };

        foreach (var roleName in roleNames)
        {
            var exists = await dbContext.CfgRoles
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Name == roleName, ct);

            if (exists)
            {
                continue;
            }

            dbContext.CfgRoles.Add(new CfgRole
            {
                Name = roleName,
                Description = $"Default role {roleName}",
                IsSystem = true,
                IsActive = true,
                CreatedBy = "system",
                CreatedAt = now
            });
        }

        await dbContext.SaveChangesAsync(ct);
    }

    private async Task SeedHrMasterDataAsync(DateTimeOffset now, CancellationToken ct)
    {
        var itDepartment = await EnsureDepartmentAsync("Information Technology", "IT", true, now, ct);
        var hrDepartment = await EnsureDepartmentAsync("Human Resources", "HRD", true, now, ct);

        var itStaff = await EnsurePositionAsync("IT Staff", "IT_STAFF", itDepartment.Id, 1, true, now, ct);
        var hrStaff = await EnsurePositionAsync("HR Staff", "HR_STAFF", hrDepartment.Id, 1, true, now, ct);

        await EnsureEmployeeAsync(
            employeeCode: "EMP001",
            fullName: "Andi Saputra",
            email: "andi.saputra@sinara.local",
            phone: "081200000001",
            departmentId: itDepartment.Id,
            positionId: itStaff.Id,
            hireDate: new DateOnly(2024, 1, 10),
            employmentStatus: EmploymentStatus.Active,
            now,
            ct);

        await EnsureEmployeeAsync(
            employeeCode: "EMP002",
            fullName: "Bunga Lestari",
            email: "bunga.lestari@sinara.local",
            phone: "081200000002",
            departmentId: hrDepartment.Id,
            positionId: hrStaff.Id,
            hireDate: new DateOnly(2024, 2, 5),
            employmentStatus: EmploymentStatus.Active,
            now,
            ct);
    }

    private async Task SeedLeaveTypesAsync(DateTimeOffset now, CancellationToken ct)
    {
        await EnsureLeaveTypeAsync("Cuti Tahunan", "ANNUAL", 12, true, true, now, ct);
        await EnsureLeaveTypeAsync("Cuti Sakit", "SICK", 12, false, true, now, ct);
        await EnsureLeaveTypeAsync("Cuti Tanpa Bayar", "UNPAID", 30, false, true, now, ct);
    }

    private async Task SeedAttendanceSettingAsync(DateTimeOffset now, CancellationToken ct)
    {
        var existing = await dbContext.HrAttendanceSettings
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.SingletonKey == "default", ct);

        if (existing is not null)
        {
            if (existing.IsDeleted)
            {
                existing.IsDeleted = false;
                existing.DeletedAt = null;
                existing.UpdatedBy = "system";
                existing.UpdatedAt = now;
                await dbContext.SaveChangesAsync(ct);
            }

            return;
        }

        dbContext.HrAttendanceSettings.Add(new HrAttendanceSetting
        {
            SingletonKey = "default",
            AttendancePeriodStartDay = 26,
            AttendancePeriodEndDay = 25,
            CheckInToleranceMinutes = 10,
            WorkStart = new TimeOnly(8, 0),
            WorkEnd = new TimeOnly(17, 0),
            BreakStart = new TimeOnly(12, 0),
            BreakEnd = new TimeOnly(13, 0),
            MinimumOtMinutes = 60,
            CreatedBy = "system",
            CreatedAt = now
        });

        await dbContext.SaveChangesAsync(ct);
    }

    private async Task SeedFinanceMasterDataAsync(DateTimeOffset now, CancellationToken ct)
    {
        await EnsureCurrencyAsync("IDR", "Indonesian Rupiah", "Rp", true, true, now, ct);
        await EnsureCurrencyAsync("USD", "US Dollar", "$", false, true, now, ct);
        await EnsureCurrencyAsync("EUR", "Euro", "EUR", false, true, now, ct);
        await EnsureCurrencyAsync("SGD", "Singapore Dollar", "S$", false, true, now, ct);

        var group1000 = await EnsureAccountGroupAsync("ASET", "1000", FinanceAccountType.Asset, FinanceNormalBalance.Debit, null, 1, true, now, ct);
        var group1100 = await EnsureAccountGroupAsync("Aset Lancar", "1100", FinanceAccountType.Asset, FinanceNormalBalance.Debit, group1000.Id, 2, true, now, ct);
        var group1200 = await EnsureAccountGroupAsync("Aset Tidak Lancar", "1200", FinanceAccountType.Asset, FinanceNormalBalance.Debit, group1000.Id, 3, true, now, ct);

        var group2000 = await EnsureAccountGroupAsync("KEWAJIBAN", "2000", FinanceAccountType.Liability, FinanceNormalBalance.Credit, null, 4, true, now, ct);
        var group2100 = await EnsureAccountGroupAsync("Kewajiban Jangka Pendek", "2100", FinanceAccountType.Liability, FinanceNormalBalance.Credit, group2000.Id, 5, true, now, ct);

        var group3000 = await EnsureAccountGroupAsync("EKUITAS", "3000", FinanceAccountType.Equity, FinanceNormalBalance.Credit, null, 6, true, now, ct);
        var group4000 = await EnsureAccountGroupAsync("PENDAPATAN", "4000", FinanceAccountType.Revenue, FinanceNormalBalance.Credit, null, 7, true, now, ct);
        var group5000 = await EnsureAccountGroupAsync("BEBAN", "5000", FinanceAccountType.Expense, FinanceNormalBalance.Debit, null, 8, true, now, ct);

        var acc1000 = await EnsureAccountAsync("1000", "ASET", group1000.Id, FinanceAccountType.Asset, FinanceNormalBalance.Debit, true, null, null, false, null, null, "IDR", true, now, ct);
        var acc1100 = await EnsureAccountAsync("1100", "Aset Lancar", group1100.Id, FinanceAccountType.Asset, FinanceNormalBalance.Debit, true, acc1000.Id, null, false, null, null, "IDR", true, now, ct);
        await EnsureAccountAsync("1101", "Kas & Setara Kas", group1100.Id, FinanceAccountType.Asset, FinanceNormalBalance.Debit, false, acc1100.Id, null, false, null, null, "IDR", true, now, ct);
        var acc1102 = await EnsureAccountAsync("1102", "Bank BCA", group1100.Id, FinanceAccountType.Asset, FinanceNormalBalance.Debit, false, acc1100.Id, null, true, "BCA", null, "IDR", true, now, ct);
        await EnsureAccountAsync("1103", "Bank Mandiri", group1100.Id, FinanceAccountType.Asset, FinanceNormalBalance.Debit, false, acc1100.Id, null, true, "Mandiri", null, "IDR", true, now, ct);
        await EnsureAccountAsync("1110", "Piutang Usaha", group1100.Id, FinanceAccountType.Asset, FinanceNormalBalance.Debit, false, acc1100.Id, null, false, null, null, "IDR", true, now, ct);
        await EnsureAccountAsync("1120", "Piutang Lain-lain", group1100.Id, FinanceAccountType.Asset, FinanceNormalBalance.Debit, false, acc1100.Id, null, false, null, null, "IDR", true, now, ct);
        await EnsureAccountAsync("1130", "Persekot/Uang Muka", group1100.Id, FinanceAccountType.Asset, FinanceNormalBalance.Debit, false, acc1100.Id, null, false, null, null, "IDR", true, now, ct);
        await EnsureAccountAsync("1140", "PPN Masukan", group1100.Id, FinanceAccountType.Asset, FinanceNormalBalance.Debit, false, acc1100.Id, null, false, null, null, "IDR", true, now, ct);

        var acc1200 = await EnsureAccountAsync("1200", "Aset Tidak Lancar", group1200.Id, FinanceAccountType.Asset, FinanceNormalBalance.Debit, true, acc1000.Id, null, false, null, null, "IDR", true, now, ct);
        await EnsureAccountAsync("1201", "Aset Tetap", group1200.Id, FinanceAccountType.Asset, FinanceNormalBalance.Debit, false, acc1200.Id, null, false, null, null, "IDR", true, now, ct);
        var acc1202 = await EnsureAccountAsync("1202", "Akumulasi Penyusutan", group1200.Id, FinanceAccountType.Asset, FinanceNormalBalance.Credit, false, acc1200.Id, null, false, null, null, "IDR", true, now, ct);

        var acc2000 = await EnsureAccountAsync("2000", "KEWAJIBAN", group2000.Id, FinanceAccountType.Liability, FinanceNormalBalance.Credit, true, null, null, false, null, null, "IDR", true, now, ct);
        var acc2100 = await EnsureAccountAsync("2100", "Kewajiban Jangka Pendek", group2100.Id, FinanceAccountType.Liability, FinanceNormalBalance.Credit, true, acc2000.Id, null, false, null, null, "IDR", true, now, ct);
        await EnsureAccountAsync("2101", "Utang Usaha", group2100.Id, FinanceAccountType.Liability, FinanceNormalBalance.Credit, false, acc2100.Id, null, false, null, null, "IDR", true, now, ct);
        var acc2102 = await EnsureAccountAsync("2102", "Utang Gaji", group2100.Id, FinanceAccountType.Liability, FinanceNormalBalance.Credit, false, acc2100.Id, null, false, null, null, "IDR", true, now, ct);
        var acc2103 = await EnsureAccountAsync("2103", "Utang PPh 21", group2100.Id, FinanceAccountType.Liability, FinanceNormalBalance.Credit, false, acc2100.Id, null, false, null, null, "IDR", true, now, ct);
        var acc2104 = await EnsureAccountAsync("2104", "Utang BPJS", group2100.Id, FinanceAccountType.Liability, FinanceNormalBalance.Credit, false, acc2100.Id, null, false, null, null, "IDR", true, now, ct);
        var acc2105 = await EnsureAccountAsync("2105", "PPN Keluaran", group2100.Id, FinanceAccountType.Liability, FinanceNormalBalance.Credit, false, acc2100.Id, null, false, null, null, "IDR", true, now, ct);
        await EnsureAccountAsync("2106", "Pendapatan Diterima Dimuka", group2100.Id, FinanceAccountType.Liability, FinanceNormalBalance.Credit, false, acc2100.Id, null, false, null, null, "IDR", true, now, ct);

        var acc3000 = await EnsureAccountAsync("3000", "EKUITAS", group3000.Id, FinanceAccountType.Equity, FinanceNormalBalance.Credit, true, null, null, false, null, null, "IDR", true, now, ct);
        var acc3101 = await EnsureAccountAsync("3101", "Modal Disetor", group3000.Id, FinanceAccountType.Equity, FinanceNormalBalance.Credit, false, acc3000.Id, null, false, null, null, "IDR", true, now, ct);
        await EnsureAccountAsync("3102", "Laba Ditahan", group3000.Id, FinanceAccountType.Equity, FinanceNormalBalance.Credit, false, acc3000.Id, null, false, null, null, "IDR", true, now, ct);
        await EnsureAccountAsync("3103", "Laba Tahun Berjalan", group3000.Id, FinanceAccountType.Equity, FinanceNormalBalance.Credit, false, acc3000.Id, null, false, null, null, "IDR", true, now, ct);

        var acc4000 = await EnsureAccountAsync("4000", "PENDAPATAN", group4000.Id, FinanceAccountType.Revenue, FinanceNormalBalance.Credit, true, null, null, false, null, null, "IDR", true, now, ct);
        await EnsureAccountAsync("4101", "Pendapatan Usaha", group4000.Id, FinanceAccountType.Revenue, FinanceNormalBalance.Credit, false, acc4000.Id, null, false, null, null, "IDR", true, now, ct);
        var acc4102 = await EnsureAccountAsync("4102", "Pendapatan Jasa", group4000.Id, FinanceAccountType.Revenue, FinanceNormalBalance.Credit, false, acc4000.Id, null, false, null, null, "IDR", true, now, ct);
        await EnsureAccountAsync("4103", "Pendapatan Lain-lain", group4000.Id, FinanceAccountType.Revenue, FinanceNormalBalance.Credit, false, acc4000.Id, null, false, null, null, "IDR", true, now, ct);

        var acc5000 = await EnsureAccountAsync("5000", "BEBAN", group5000.Id, FinanceAccountType.Expense, FinanceNormalBalance.Debit, true, null, null, false, null, null, "IDR", true, now, ct);
        var acc5101 = await EnsureAccountAsync("5101", "Beban Gaji & Tunjangan", group5000.Id, FinanceAccountType.Expense, FinanceNormalBalance.Debit, false, acc5000.Id, null, false, null, null, "IDR", true, now, ct);
        var acc5102 = await EnsureAccountAsync("5102", "Beban PPh 21", group5000.Id, FinanceAccountType.Expense, FinanceNormalBalance.Debit, false, acc5000.Id, null, false, null, null, "IDR", true, now, ct);
        var acc5103 = await EnsureAccountAsync("5103", "Beban BPJS Perusahaan", group5000.Id, FinanceAccountType.Expense, FinanceNormalBalance.Debit, false, acc5000.Id, null, false, null, null, "IDR", true, now, ct);
        var acc5104 = await EnsureAccountAsync("5104", "Beban Operasional", group5000.Id, FinanceAccountType.Expense, FinanceNormalBalance.Debit, false, acc5000.Id, null, false, null, null, "IDR", true, now, ct);
        var acc5105 = await EnsureAccountAsync("5105", "Beban Penyusutan", group5000.Id, FinanceAccountType.Expense, FinanceNormalBalance.Debit, false, acc5000.Id, null, false, null, null, "IDR", true, now, ct);
        await EnsureAccountAsync("5106", "Beban Pajak Lain", group5000.Id, FinanceAccountType.Expense, FinanceNormalBalance.Debit, false, acc5000.Id, null, false, null, null, "IDR", true, now, ct);

        var currentYear = DateTime.UtcNow.Year;
        var fiscalYear = await EnsureFiscalYearAsync($"FY {currentYear}", new DateOnly(currentYear, 1, 1), new DateOnly(currentYear, 12, 31), FinancePeriodStatus.Open, now, ct);
        for (var month = 1; month <= 12; month++)
        {
            var periodStart = new DateOnly(currentYear, month, 1);
            var periodEnd = new DateOnly(currentYear, month, DateTime.DaysInMonth(currentYear, month));
            await EnsurePeriodAsync(fiscalYear.Id, month, periodStart.ToString("MMMM yyyy"), periodStart, periodEnd, FinancePeriodStatus.Open, now, ct);
        }

        var departments = await dbContext.HrDepartments
            .AsNoTracking()
            .Where(x => x.IsActive)
            .ToListAsync(ct);

        foreach (var department in departments)
        {
            var code = BuildCostCenterCode(department.Code);
            await EnsureCostCenterAsync(code, department.Name, department.Id, department.ManagerId, acc5104.Id, true, now, ct);
        }

        await EnsureTaxCodeAsync("PPN11", "PPN 11%", FinanceTaxType.Ppn, 11m, false, acc2105.Id, true, now, ct);
        await EnsureTaxCodeAsync("PPH21", "PPh 21 Karyawan", FinanceTaxType.Pph21, 5m, false, acc2103.Id, true, now, ct);
        await EnsureTaxCodeAsync("PPH23", "PPh 23", FinanceTaxType.Pph23, 2m, false, acc2103.Id, true, now, ct);
        await EnsureTaxCodeAsync("PPH4_2", "PPh 4(2)", FinanceTaxType.Pph4Ayat2, 10m, false, acc2103.Id, true, now, ct);

        var currentMonthStart = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        await EnsureExchangeRateAsync("USD", "IDR", 16250m, currentMonthStart, "BI", now, ct);
        await EnsureExchangeRateAsync("EUR", "IDR", 17680m, currentMonthStart, "BI", now, ct);
        await EnsureExchangeRateAsync("SGD", "IDR", 11980m, currentMonthStart, "BI", now, ct);

        var seededPeriod = await dbContext.FinPeriods
            .AsNoTracking()
            .Where(x => x.Status == FinancePeriodStatus.Open)
            .OrderByDescending(x => x.StartDate)
            .FirstOrDefaultAsync(ct);

        if (seededPeriod is not null)
        {
            var seededCostCenterId = await dbContext.FinCostCenters
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Code)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync(ct);

            var seededPostedBy = await dbContext.SysUsers
                .AsNoTracking()
                .Where(x => x.Username == "admin")
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync(ct);

            var seededPayrollRunId = await dbContext.HrPayrollRuns
                .AsNoTracking()
                .OrderByDescending(x => x.PeriodYear)
                .ThenByDescending(x => x.PeriodMonth)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync(ct);

            await EnsureSampleFinanceJournalsAsync(
                seededPeriod.Id,
                seededCostCenterId,
                seededPostedBy,
                seededPayrollRunId,
                acc1102.Id,
                acc1202.Id,
                acc2102.Id,
                acc2103.Id,
                acc2104.Id,
                acc4102.Id,
                acc5101.Id,
                acc5102.Id,
                acc5103.Id,
                acc5104.Id,
                acc5105.Id,
                acc3101.Id,
                now,
                ct);
        }
    }
    private async Task SeedAdminUserAsync(DateTimeOffset now, CancellationToken ct)
    {
        var adminUser = await dbContext.SysUsers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Username == "admin", ct);

        if (adminUser is null)
        {
            adminUser = new SysUser
            {
                Username = "admin",
                FullName = "Super Administrator",
                Email = "admin@sinara.local",
                LanguagePreference = "en",
                IsActive = true,
                CreatedBy = "system",
                CreatedAt = now
            };

            var passwordHasher = new PasswordHasher<SysUser>();
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Admin@123!");
            dbContext.SysUsers.Add(adminUser);
            await dbContext.SaveChangesAsync(ct);
        }

        var superAdminRole = await dbContext.CfgRoles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Name == "Super Admin", ct);

        if (superAdminRole is null)
        {
            return;
        }

        var hasRole = await dbContext.SysUserRoles
            .AnyAsync(x => x.UserId == adminUser.Id && x.RoleId == superAdminRole.Id, ct);

        if (!hasRole)
        {
            dbContext.SysUserRoles.Add(new SysUserRole
            {
                UserId = adminUser.Id,
                RoleId = superAdminRole.Id
            });

            await dbContext.SaveChangesAsync(ct);
        }
    }

    private async Task SeedMenusAsync(DateTimeOffset now, CancellationToken ct)
    {
        var hrModule = await dbContext.CfgModules
            .IgnoreQueryFilters()
            .FirstAsync(x => x.Code == "HR", ct);

        var cfgModule = await dbContext.CfgModules
            .IgnoreQueryFilters()
            .FirstAsync(x => x.Code == "CFG", ct);

        var finModule = await dbContext.CfgModules
            .IgnoreQueryFilters()
            .FirstAsync(x => x.Code == "FIN", ct);

        var hrEmployees = await EnsureMenuAsync(hrModule.Id, null, "Employees", null, "bi-people", 1, now, ct);
        await EnsureMenuAsync(hrModule.Id, hrEmployees.Id, "All Employees", "/hr/employees", "bi-list", 1, now, ct);
        await EnsureMenuAsync(hrModule.Id, hrEmployees.Id, "Add Employee", "/hr/employees/create", "bi-plus-circle", 2, now, ct);
        await EnsureMenuAsync(hrModule.Id, hrEmployees.Id, "Departments", "/hr/departments", "bi-diagram-3", 3, now, ct);
        await EnsureMenuAsync(hrModule.Id, hrEmployees.Id, "Positions", "/hr/positions", "bi-person-badge", 4, now, ct);

        var hrAttendance = await EnsureMenuAsync(hrModule.Id, null, "Attendance", null, "bi-calendar-check", 2, now, ct);
        await EnsureMenuAsync(hrModule.Id, hrAttendance.Id, "Daily Attendance", "/hr/attendance", "bi-clock", 1, now, ct);
        await EnsureMenuAsync(hrModule.Id, hrAttendance.Id, "Holiday Master", "/hr/attendance/holiday", "bi-calendar-event", 2, now, ct);
        await EnsureMenuAsync(hrModule.Id, hrAttendance.Id, "Attendance Report", "/hr/attendance/report", "bi-file-earmark-text", 3, now, ct);
        await EnsureMenuAsync(hrModule.Id, hrAttendance.Id, "Attendance Setting", "/hr/attendance/setting", "bi-sliders2", 4, now, ct);

        var hrPayroll = await EnsureMenuAsync(hrModule.Id, null, "Payroll", null, "bi-cash-stack", 3, now, ct);
        await EnsureMenuAsync(hrModule.Id, hrPayroll.Id, "Payroll Run", "/hr/payroll", "bi-gear-wide-connected", 1, now, ct);
        await EnsureMenuAsync(hrModule.Id, hrPayroll.Id, "Salary Setup", "/hr/payroll/setup", "bi-sliders", 2, now, ct);
        await EnsureMenuAsync(hrModule.Id, hrPayroll.Id, "Payslips", "/hr/payroll/payslips", "bi-receipt", 3, now, ct);

        var hrLeave = await EnsureMenuAsync(hrModule.Id, null, "Leave", null, "bi-calendar2-heart", 4, now, ct);
        await EnsureMenuAsync(hrModule.Id, hrLeave.Id, "Leave Requests", "/hr/leave/requests", "bi-envelope-check", 1, now, ct);
        await EnsureMenuAsync(hrModule.Id, hrLeave.Id, "Leave Balance", "/hr/leave/balance", "bi-clipboard-data", 2, now, ct);
        await EnsureMenuAsync(hrModule.Id, hrLeave.Id, "Leave Types", "/hr/leave/types", "bi-tags", 3, now, ct);

        var hrReports = await EnsureMenuAsync(hrModule.Id, null, "Reports", null, "bi-bar-chart", 5, now, ct);
        await EnsureMenuAsync(hrModule.Id, hrReports.Id, "Headcount Report", "/hr/reports/headcount", "bi-graph-up", 1, now, ct);
        await EnsureMenuAsync(hrModule.Id, hrReports.Id, "Turnover Report", "/hr/reports/turnover", "bi-graph-down", 2, now, ct);

        var cfgUsers = await EnsureMenuAsync(cfgModule.Id, null, "Users", null, "bi-people-fill", 1, now, ct);
        await EnsureMenuAsync(cfgModule.Id, cfgUsers.Id, "User List", "/config/users", "bi-list-ul", 1, now, ct);
        await EnsureMenuAsync(cfgModule.Id, cfgUsers.Id, "Add User", "/config/users/create", "bi-person-plus", 2, now, ct);

        var cfgRoles = await EnsureMenuAsync(cfgModule.Id, null, "Roles & Permissions", null, "bi-shield-lock", 2, now, ct);
        await EnsureMenuAsync(cfgModule.Id, cfgRoles.Id, "Role List", "/config/roles", "bi-card-list", 1, now, ct);
        await EnsureMenuAsync(cfgModule.Id, cfgRoles.Id, "Permission Matrix", "/config/roles/permissions", "bi-grid-3x3-gap", 2, now, ct);

        var cfgMenuConfig = await EnsureMenuAsync(cfgModule.Id, null, "Menu Config", null, "bi-menu-button", 3, now, ct);
        await EnsureMenuAsync(cfgModule.Id, cfgMenuConfig.Id, "Menu Structure", "/config/menus", "bi-diagram-2", 1, now, ct);
        await EnsureMenuAsync(cfgModule.Id, cfgMenuConfig.Id, "Module Settings", "/config/modules", "bi-boxes", 2, now, ct);

        var cfgSystem = await EnsureMenuAsync(cfgModule.Id, null, "System", null, "bi-gear-wide-connected", 4, now, ct);
        await EnsureMenuAsync(cfgModule.Id, cfgSystem.Id, "App Settings", "/config/settings", "bi-tools", 1, now, ct);
        await EnsureMenuAsync(cfgModule.Id, cfgSystem.Id, "Language Setup", "/config/languages", "bi-translate", 2, now, ct);
        await EnsureMenuAsync(cfgModule.Id, cfgSystem.Id, "Audit Log", "/config/audit", "bi-journal-text", 3, now, ct);

        var finCoa = await EnsureMenuAsync(finModule.Id, null, "Chart of Accounts", null, "bi-diagram-2", 1, now, ct);
        await EnsureMenuAsync(finModule.Id, finCoa.Id, "Accounts", "/finance/coa", "bi-list-columns", 1, now, ct);
        await EnsureMenuAsync(finModule.Id, finCoa.Id, "Account Groups", "/finance/coa/groups", "bi-folder2-open", 2, now, ct);

        var finMasters = await EnsureMenuAsync(finModule.Id, null, "Finance Masters", null, "bi-sliders", 2, now, ct);
        await EnsureMenuAsync(finModule.Id, finMasters.Id, "Cost Centers", "/finance/cost-centers", "bi-diagram-3", 1, now, ct);
        await EnsureMenuAsync(finModule.Id, finMasters.Id, "Currencies", "/finance/currencies", "bi-currency-exchange", 2, now, ct);
        await EnsureMenuAsync(finModule.Id, finMasters.Id, "Exchange Rates", "/finance/exchange-rates", "bi-graph-up-arrow", 3, now, ct);
        await EnsureMenuAsync(finModule.Id, finMasters.Id, "Fiscal Years", "/finance/fiscal-years", "bi-calendar3", 4, now, ct);
        await EnsureMenuAsync(finModule.Id, finMasters.Id, "Periods", "/finance/periods", "bi-calendar-week", 5, now, ct);
        await EnsureMenuAsync(finModule.Id, finMasters.Id, "Tax Codes", "/finance/tax-codes", "bi-receipt-cutoff", 6, now, ct);

        var finJournalLedger = await EnsureMenuAsync(finModule.Id, null, "Journal & Ledger", null, "bi-journal-bookmark", 3, now, ct);
        await EnsureMenuAsync(finModule.Id, finJournalLedger.Id, "Journals", "/finance/journals", "bi-journal-check", 1, now, ct);
        await EnsureMenuAsync(finModule.Id, finJournalLedger.Id, "Payroll Journals", "/finance/journals?source=Payroll", "bi-journal-text", 2, now, ct);
        await EnsureMenuAsync(finModule.Id, finJournalLedger.Id, "General Ledger", "/finance/ledger", "bi-book", 3, now, ct);

        var finAp = await EnsureMenuAsync(finModule.Id, null, "Accounts Payable", null, "bi-wallet2", 4, now, ct);
        await EnsureMenuAsync(finModule.Id, finAp.Id, "Vendors", "/finance/vendors", "bi-building", 1, now, ct);
        await EnsureMenuAsync(finModule.Id, finAp.Id, "AP Invoices", "/finance/ap/invoices", "bi-receipt", 2, now, ct);
        await EnsureMenuAsync(finModule.Id, finAp.Id, "AP Payments", "/finance/ap/payments", "bi-cash-coin", 3, now, ct);
        await EnsureMenuAsync(finModule.Id, finAp.Id, "AP Aging", "/finance/ap/aging", "bi-hourglass-split", 4, now, ct);

        var finAr = await EnsureMenuAsync(finModule.Id, null, "Accounts Receivable", null, "bi-cash-stack", 5, now, ct);
        await EnsureMenuAsync(finModule.Id, finAr.Id, "Customers", "/finance/customers", "bi-people", 1, now, ct);
        await EnsureMenuAsync(finModule.Id, finAr.Id, "AR Invoices", "/finance/ar/invoices", "bi-receipt", 2, now, ct);
        await EnsureMenuAsync(finModule.Id, finAr.Id, "AR Receipts", "/finance/ar/receipts", "bi-cash-coin", 3, now, ct);
        await EnsureMenuAsync(finModule.Id, finAr.Id, "AR Aging", "/finance/ar/aging", "bi-hourglass-split", 4, now, ct);

        var finReports = await EnsureMenuAsync(finModule.Id, null, "Financial Reports", null, "bi-bar-chart-line", 6, now, ct);
        await EnsureMenuAsync(finModule.Id, finReports.Id, "Trial Balance", "/finance/reports/trial-balance", "bi-table", 1, now, ct);
        await EnsureMenuAsync(finModule.Id, finReports.Id, "Balance Sheet", "/finance/reports/balance-sheet", "bi-border-all", 2, now, ct);
        await EnsureMenuAsync(finModule.Id, finReports.Id, "Profit & Loss", "/finance/reports/profit-loss", "bi-graph-up-arrow", 3, now, ct);
        await EnsureMenuAsync(finModule.Id, finReports.Id, "Cash Flow", "/finance/reports/cash-flow", "bi-water", 4, now, ct);
    }

    private async Task SeedSuperAdminPermissionsAsync(CancellationToken ct)
    {
        var superAdminRole = await dbContext.CfgRoles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Name == "Super Admin", ct);

        if (superAdminRole is null)
        {
            return;
        }

        var menus = await dbContext.CfgMenus
            .IgnoreQueryFilters()
            .ToListAsync(ct);

        var existingPermissions = await dbContext.CfgRoleMenuPermissions
            .Where(x => x.RoleId == superAdminRole.Id)
            .ToListAsync(ct);

        foreach (var menu in menus)
        {
            var permission = existingPermissions.FirstOrDefault(x => x.MenuId == menu.Id);
            if (permission is null)
            {
                dbContext.CfgRoleMenuPermissions.Add(new CfgRoleMenuPermission
                {
                    RoleId = superAdminRole.Id,
                    MenuId = menu.Id,
                    CanView = true,
                    CanCreate = true,
                    CanEdit = true,
                    CanDelete = true
                });
            }
            else
            {
                permission.CanView = true;
                permission.CanCreate = true;
                permission.CanEdit = true;
                permission.CanDelete = true;
            }
        }

        await dbContext.SaveChangesAsync(ct);
    }

    private async Task EnsureModuleAsync(string name, string code, string icon, int sortOrder, DateTimeOffset now, CancellationToken ct)
    {
        var existing = await dbContext.CfgModules
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Code == code, ct);

        if (existing is not null)
        {
            existing.Name = name;
            existing.Icon = icon;
            existing.SortOrder = sortOrder;
            existing.IsActive = true;
            existing.UpdatedBy = "system";
            existing.UpdatedAt = now;
            await dbContext.SaveChangesAsync(ct);
            return;
        }

        dbContext.CfgModules.Add(new CfgModule
        {
            Name = name,
            Code = code,
            Icon = icon,
            SortOrder = sortOrder,
            IsActive = true,
            CreatedBy = "system",
            CreatedAt = now
        });

        await dbContext.SaveChangesAsync(ct);
    }

    private async Task<HrDepartment> EnsureDepartmentAsync(string name, string code, bool isActive, DateTimeOffset now, CancellationToken ct)
    {
        var existing = await dbContext.HrDepartments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Code == code, ct);

        if (existing is not null)
        {
            existing.Name = name;
            existing.IsActive = isActive;
            existing.UpdatedBy = "system";
            existing.UpdatedAt = now;
            await dbContext.SaveChangesAsync(ct);
            return existing;
        }

        var entity = new HrDepartment
        {
            Name = name,
            Code = code,
            IsActive = isActive,
            CreatedBy = "system",
            CreatedAt = now
        };

        dbContext.HrDepartments.Add(entity);
        await dbContext.SaveChangesAsync(ct);
        return entity;
    }

    private async Task<HrPosition> EnsurePositionAsync(
        string name,
        string code,
        int departmentId,
        int level,
        bool isActive,
        DateTimeOffset now,
        CancellationToken ct)
    {
        var existing = await dbContext.HrPositions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Code == code, ct);

        if (existing is not null)
        {
            existing.Name = name;
            existing.DepartmentId = departmentId;
            existing.Level = level;
            existing.IsActive = isActive;
            existing.UpdatedBy = "system";
            existing.UpdatedAt = now;
            await dbContext.SaveChangesAsync(ct);
            return existing;
        }

        var entity = new HrPosition
        {
            Name = name,
            Code = code,
            DepartmentId = departmentId,
            Level = level,
            IsActive = isActive,
            CreatedBy = "system",
            CreatedAt = now
        };

        dbContext.HrPositions.Add(entity);
        await dbContext.SaveChangesAsync(ct);
        return entity;
    }

    private async Task<HrEmployee> EnsureEmployeeAsync(
        string employeeCode,
        string fullName,
        string? email,
        string? phone,
        int departmentId,
        int positionId,
        DateOnly hireDate,
        EmploymentStatus employmentStatus,
        DateTimeOffset now,
        CancellationToken ct)
    {
        var existing = await dbContext.HrEmployees
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.EmployeeCode == employeeCode, ct);

        if (existing is not null)
        {
            existing.FullName = fullName;
            existing.Email = email;
            existing.Phone = phone;
            existing.DepartmentId = departmentId;
            existing.PositionId = positionId;
            existing.HireDate = hireDate;
            existing.EmploymentStatus = employmentStatus;
            existing.UpdatedBy = "system";
            existing.UpdatedAt = now;
            await dbContext.SaveChangesAsync(ct);
            return existing;
        }

        var entity = new HrEmployee
        {
            EmployeeCode = employeeCode,
            FullName = fullName,
            Email = email,
            Phone = phone,
            DepartmentId = departmentId,
            PositionId = positionId,
            HireDate = hireDate,
            EmploymentStatus = employmentStatus,
            CreatedBy = "system",
            CreatedAt = now
        };

        dbContext.HrEmployees.Add(entity);
        await dbContext.SaveChangesAsync(ct);
        return entity;
    }

    private async Task EnsureLeaveTypeAsync(
        string name,
        string code,
        int maxDaysPerYear,
        bool isCarryOver,
        bool isActive,
        DateTimeOffset now,
        CancellationToken ct)
    {
        var existing = await dbContext.HrLeaveTypes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Code == code, ct);

        if (existing is not null)
        {
            existing.Name = name;
            existing.MaxDaysPerYear = maxDaysPerYear;
            existing.IsCarryOver = isCarryOver;
            existing.IsActive = isActive;
            existing.UpdatedBy = "system";
            existing.UpdatedAt = now;
            await dbContext.SaveChangesAsync(ct);
            return;
        }

        dbContext.HrLeaveTypes.Add(new HrLeaveType
        {
            Name = name,
            Code = code,
            MaxDaysPerYear = maxDaysPerYear,
            IsCarryOver = isCarryOver,
            IsActive = isActive,
            CreatedBy = "system",
            CreatedAt = now
        });

        await dbContext.SaveChangesAsync(ct);
    }


    private async Task<FinCurrency> EnsureCurrencyAsync(
        string code,
        string name,
        string symbol,
        bool isBaseCurrency,
        bool isActive,
        DateTimeOffset now,
        CancellationToken ct)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        var existing = await dbContext.FinCurrencies
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Code == normalizedCode, ct);

        if (existing is null)
        {
            existing = new FinCurrency
            {
                Code = normalizedCode,
                Name = name,
                Symbol = symbol,
                IsBaseCurrency = isBaseCurrency,
                IsActive = isActive,
                CreatedBy = "system",
                CreatedAt = now
            };

            dbContext.FinCurrencies.Add(existing);
        }
        else
        {
            existing.Name = name;
            existing.Symbol = symbol;
            existing.IsBaseCurrency = isBaseCurrency;
            existing.IsActive = isActive;
            existing.IsDeleted = false;
            existing.DeletedAt = null;
            existing.UpdatedBy = "system";
            existing.UpdatedAt = now;
        }

        if (isBaseCurrency)
        {
            var others = await dbContext.FinCurrencies
                .IgnoreQueryFilters()
                .Where(x => x.Code != normalizedCode && x.IsBaseCurrency)
                .ToListAsync(ct);

            foreach (var other in others)
            {
                other.IsBaseCurrency = false;
                other.UpdatedBy = "system";
                other.UpdatedAt = now;
            }
        }

        await dbContext.SaveChangesAsync(ct);
        return existing;
    }

    private async Task<FinAccountGroup> EnsureAccountGroupAsync(
        string name,
        string code,
        FinanceAccountType type,
        FinanceNormalBalance normalBalance,
        int? parentGroupId,
        int sortOrder,
        bool isActive,
        DateTimeOffset now,
        CancellationToken ct)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        var existing = await dbContext.FinAccountGroups
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Code == normalizedCode, ct);

        if (existing is null)
        {
            existing = new FinAccountGroup
            {
                Name = name,
                Code = normalizedCode,
                Type = type,
                NormalBalance = normalBalance,
                ParentGroupId = parentGroupId,
                SortOrder = sortOrder,
                IsActive = isActive,
                CreatedBy = "system",
                CreatedAt = now
            };

            dbContext.FinAccountGroups.Add(existing);
        }
        else
        {
            existing.Name = name;
            existing.Type = type;
            existing.NormalBalance = normalBalance;
            existing.ParentGroupId = parentGroupId;
            existing.SortOrder = sortOrder;
            existing.IsActive = isActive;
            existing.IsDeleted = false;
            existing.DeletedAt = null;
            existing.UpdatedBy = "system";
            existing.UpdatedAt = now;
        }

        await dbContext.SaveChangesAsync(ct);
        return existing;
    }

    private async Task<FinAccount> EnsureAccountAsync(
        string code,
        string name,
        int groupId,
        FinanceAccountType type,
        FinanceNormalBalance normalBalance,
        bool isHeader,
        int? parentAccountId,
        string? description,
        bool isBankAccount,
        string? bankName,
        string? bankAccountNo,
        string currencyCode,
        bool isActive,
        DateTimeOffset now,
        CancellationToken ct)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        var normalizedCurrencyCode = currencyCode.Trim().ToUpperInvariant();

        var existing = await dbContext.FinAccounts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Code == normalizedCode, ct);

        if (existing is null)
        {
            existing = new FinAccount
            {
                Code = normalizedCode,
                Name = name,
                GroupId = groupId,
                Type = type,
                NormalBalance = normalBalance,
                IsHeader = isHeader,
                ParentAccountId = parentAccountId,
                Description = description,
                IsBankAccount = isBankAccount,
                BankName = bankName,
                BankAccountNo = bankAccountNo,
                CurrencyCode = normalizedCurrencyCode,
                IsActive = isActive,
                CreatedBy = "system",
                CreatedAt = now
            };

            dbContext.FinAccounts.Add(existing);
        }
        else
        {
            existing.Name = name;
            existing.GroupId = groupId;
            existing.Type = type;
            existing.NormalBalance = normalBalance;
            existing.IsHeader = isHeader;
            existing.ParentAccountId = parentAccountId;
            existing.Description = description;
            existing.IsBankAccount = isBankAccount;
            existing.BankName = bankName;
            existing.BankAccountNo = bankAccountNo;
            existing.CurrencyCode = normalizedCurrencyCode;
            existing.IsActive = isActive;
            existing.IsDeleted = false;
            existing.DeletedAt = null;
            existing.UpdatedBy = "system";
            existing.UpdatedAt = now;
        }

        await dbContext.SaveChangesAsync(ct);
        return existing;
    }

    private async Task<FinCostCenter> EnsureCostCenterAsync(
        string code,
        string name,
        int? departmentId,
        int? managerId,
        int? budgetAccountId,
        bool isActive,
        DateTimeOffset now,
        CancellationToken ct)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        var existing = await dbContext.FinCostCenters
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Code == normalizedCode, ct);

        if (existing is null)
        {
            existing = new FinCostCenter
            {
                Code = normalizedCode,
                Name = name,
                DepartmentId = departmentId,
                ManagerId = managerId,
                BudgetAccountId = budgetAccountId,
                IsActive = isActive,
                CreatedBy = "system",
                CreatedAt = now
            };

            dbContext.FinCostCenters.Add(existing);
        }
        else
        {
            existing.Name = name;
            existing.DepartmentId = departmentId;
            existing.ManagerId = managerId;
            existing.BudgetAccountId = budgetAccountId;
            existing.IsActive = isActive;
            existing.IsDeleted = false;
            existing.DeletedAt = null;
            existing.UpdatedBy = "system";
            existing.UpdatedAt = now;
        }

        await dbContext.SaveChangesAsync(ct);
        return existing;
    }

    private async Task<FinExchangeRate> EnsureExchangeRateAsync(
        string fromCurrencyCode,
        string toCurrencyCode,
        decimal rate,
        DateOnly effectiveDate,
        string? source,
        DateTimeOffset now,
        CancellationToken ct)
    {
        var normalizedFromCurrencyCode = fromCurrencyCode.Trim().ToUpperInvariant();
        var normalizedToCurrencyCode = toCurrencyCode.Trim().ToUpperInvariant();

        var existing = await dbContext.FinExchangeRates
            .FirstOrDefaultAsync(x =>
                x.FromCurrencyCode == normalizedFromCurrencyCode &&
                x.ToCurrencyCode == normalizedToCurrencyCode &&
                x.EffectiveDate == effectiveDate,
                ct);

        if (existing is null)
        {
            existing = new FinExchangeRate
            {
                FromCurrencyCode = normalizedFromCurrencyCode,
                ToCurrencyCode = normalizedToCurrencyCode,
                Rate = rate,
                EffectiveDate = effectiveDate,
                Source = string.IsNullOrWhiteSpace(source) ? null : source.Trim(),
                CreatedBy = "system",
                CreatedAt = now
            };

            dbContext.FinExchangeRates.Add(existing);
        }
        else
        {
            existing.Rate = rate;
            existing.Source = string.IsNullOrWhiteSpace(source) ? null : source.Trim();
        }

        await dbContext.SaveChangesAsync(ct);
        return existing;
    }
    private async Task<FinFiscalYear> EnsureFiscalYearAsync(
        string name,
        DateOnly startDate,
        DateOnly endDate,
        FinancePeriodStatus status,
        DateTimeOffset now,
        CancellationToken ct)
    {
        var existing = await dbContext.FinFiscalYears
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Name == name, ct);

        if (existing is null)
        {
            existing = new FinFiscalYear
            {
                Name = name,
                StartDate = startDate,
                EndDate = endDate,
                Status = status,
                CreatedBy = "system",
                CreatedAt = now
            };

            dbContext.FinFiscalYears.Add(existing);
        }
        else
        {
            existing.StartDate = startDate;
            existing.EndDate = endDate;
            existing.Status = status;
            existing.IsDeleted = false;
            existing.DeletedAt = null;
            existing.UpdatedBy = "system";
            existing.UpdatedAt = now;
        }

        await dbContext.SaveChangesAsync(ct);
        return existing;
    }

    private async Task<FinPeriod> EnsurePeriodAsync(
        int fiscalYearId,
        int periodNumber,
        string name,
        DateOnly startDate,
        DateOnly endDate,
        FinancePeriodStatus status,
        DateTimeOffset now,
        CancellationToken ct)
    {
        var existing = await dbContext.FinPeriods
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.FiscalYearId == fiscalYearId && x.PeriodNumber == periodNumber, ct);

        if (existing is null)
        {
            existing = new FinPeriod
            {
                FiscalYearId = fiscalYearId,
                PeriodNumber = periodNumber,
                Name = name,
                StartDate = startDate,
                EndDate = endDate,
                Status = status,
                CreatedBy = "system",
                CreatedAt = now
            };

            dbContext.FinPeriods.Add(existing);
        }
        else
        {
            existing.Name = name;
            existing.StartDate = startDate;
            existing.EndDate = endDate;
            existing.Status = status;
            existing.IsDeleted = false;
            existing.DeletedAt = null;
            existing.UpdatedBy = "system";
            existing.UpdatedAt = now;
        }

        await dbContext.SaveChangesAsync(ct);
        return existing;
    }

    private async Task<FinTaxCode> EnsureTaxCodeAsync(
        string code,
        string name,
        FinanceTaxType type,
        decimal rate,
        bool isInclusive,
        int accountId,
        bool isActive,
        DateTimeOffset now,
        CancellationToken ct)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        var existing = await dbContext.FinTaxCodes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Code == normalizedCode, ct);

        if (existing is null)
        {
            existing = new FinTaxCode
            {
                Code = normalizedCode,
                Name = name,
                Type = type,
                Rate = rate,
                IsInclusive = isInclusive,
                AccountId = accountId,
                IsActive = isActive,
                CreatedBy = "system",
                CreatedAt = now
            };

            dbContext.FinTaxCodes.Add(existing);
        }
        else
        {
            existing.Name = name;
            existing.Type = type;
            existing.Rate = rate;
            existing.IsInclusive = isInclusive;
            existing.AccountId = accountId;
            existing.IsActive = isActive;
            existing.IsDeleted = false;
            existing.DeletedAt = null;
            existing.UpdatedBy = "system";
            existing.UpdatedAt = now;
        }

        await dbContext.SaveChangesAsync(ct);
        return existing;
    }

    private async Task EnsureSampleFinanceJournalsAsync(
        int periodId,
        int? costCenterId,
        int? postedByUserId,
        int? payrollRunId,
        int bankAccountId,
        int accumulatedDepreciationAccountId,
        int payrollPayableAccountId,
        int taxPayableAccountId,
        int bpjsPayableAccountId,
        int serviceRevenueAccountId,
        int salaryExpenseAccountId,
        int pphExpenseAccountId,
        int bpjsExpenseAccountId,
        int operationalExpenseAccountId,
        int depreciationExpenseAccountId,
        int paidInCapitalAccountId,
        DateTimeOffset now,
        CancellationToken ct)
    {
        var postingDate = DateOnly.FromDateTime(DateTime.UtcNow);

        await EnsureJournalAsync(
            journalNo: $"JE-{postingDate.Year}-900001",
            periodId: periodId,
            date: postingDate,
            description: "Pembelian perlengkapan kantor (tunai)",
            source: FinanceJournalSource.Manual,
            sourceRefId: null,
            sourceRefType: null,
            status: FinanceJournalStatus.Posted,
            postedBy: postedByUserId,
            postedAt: now,
            currencyCode: "IDR",
            exchangeRate: 1m,
            lines:
            [
                new SeedJournalLine
                {
                    AccountId = operationalExpenseAccountId,
                    CostCenterId = costCenterId,
                    Description = "Beban operasional kantor",
                    Debit = 2_500_000m,
                    Credit = 0m
                },
                new SeedJournalLine
                {
                    AccountId = bankAccountId,
                    CostCenterId = null,
                    Description = "Pembayaran melalui Bank BCA",
                    Debit = 0m,
                    Credit = 2_500_000m
                }
            ],
            now,
            ct);

        await EnsureJournalAsync(
            journalNo: $"JE-{postingDate.Year}-900002",
            periodId: periodId,
            date: postingDate,
            description: "Penerimaan pendapatan jasa",
            source: FinanceJournalSource.Manual,
            sourceRefId: null,
            sourceRefType: null,
            status: FinanceJournalStatus.Posted,
            postedBy: postedByUserId,
            postedAt: now,
            currencyCode: "IDR",
            exchangeRate: 1m,
            lines:
            [
                new SeedJournalLine
                {
                    AccountId = bankAccountId,
                    CostCenterId = null,
                    Description = "Penerimaan kas via bank",
                    Debit = 7_500_000m,
                    Credit = 0m
                },
                new SeedJournalLine
                {
                    AccountId = serviceRevenueAccountId,
                    CostCenterId = costCenterId,
                    Description = "Pendapatan jasa bulan berjalan",
                    Debit = 0m,
                    Credit = 7_500_000m
                }
            ],
            now,
            ct);

        await EnsureJournalAsync(
            journalNo: $"JE-{postingDate.Year}-900003",
            periodId: periodId,
            date: postingDate,
            description: "Pengakuan beban penyusutan",
            source: FinanceJournalSource.Manual,
            sourceRefId: null,
            sourceRefType: null,
            status: FinanceJournalStatus.Posted,
            postedBy: postedByUserId,
            postedAt: now,
            currencyCode: "IDR",
            exchangeRate: 1m,
            lines:
            [
                new SeedJournalLine
                {
                    AccountId = depreciationExpenseAccountId,
                    CostCenterId = costCenterId,
                    Description = "Beban penyusutan aset tetap",
                    Debit = 1_200_000m,
                    Credit = 0m
                },
                new SeedJournalLine
                {
                    AccountId = accumulatedDepreciationAccountId,
                    CostCenterId = null,
                    Description = "Akumulasi penyusutan",
                    Debit = 0m,
                    Credit = 1_200_000m
                }
            ],
            now,
            ct);

        await EnsureJournalAsync(
            journalNo: $"JE-{postingDate.Year}-900004",
            periodId: periodId,
            date: postingDate,
            description: "Setoran modal pemilik",
            source: FinanceJournalSource.Manual,
            sourceRefId: null,
            sourceRefType: null,
            status: FinanceJournalStatus.Posted,
            postedBy: postedByUserId,
            postedAt: now,
            currencyCode: "IDR",
            exchangeRate: 1m,
            lines:
            [
                new SeedJournalLine
                {
                    AccountId = bankAccountId,
                    CostCenterId = null,
                    Description = "Setoran modal via bank",
                    Debit = 20_000_000m,
                    Credit = 0m
                },
                new SeedJournalLine
                {
                    AccountId = paidInCapitalAccountId,
                    CostCenterId = null,
                    Description = "Modal disetor",
                    Debit = 0m,
                    Credit = 20_000_000m
                }
            ],
            now,
            ct);

        await EnsureJournalAsync(
            journalNo: $"JE-{postingDate.Year}-900101",
            periodId: periodId,
            date: postingDate,
            description: "Jurnal payroll otomatis",
            source: FinanceJournalSource.Payroll,
            sourceRefId: payrollRunId,
            sourceRefType: "hr_payroll_runs",
            status: FinanceJournalStatus.Posted,
            postedBy: postedByUserId,
            postedAt: now,
            currencyCode: "IDR",
            exchangeRate: 1m,
            lines:
            [
                new SeedJournalLine
                {
                    AccountId = salaryExpenseAccountId,
                    CostCenterId = costCenterId,
                    Description = "Beban gaji dan tunjangan",
                    Debit = 15_000_000m,
                    Credit = 0m
                },
                new SeedJournalLine
                {
                    AccountId = pphExpenseAccountId,
                    CostCenterId = costCenterId,
                    Description = "Beban PPh 21 perusahaan",
                    Debit = 750_000m,
                    Credit = 0m
                },
                new SeedJournalLine
                {
                    AccountId = bpjsExpenseAccountId,
                    CostCenterId = costCenterId,
                    Description = "Beban BPJS perusahaan",
                    Debit = 1_250_000m,
                    Credit = 0m
                },
                new SeedJournalLine
                {
                    AccountId = payrollPayableAccountId,
                    CostCenterId = null,
                    Description = "Utang gaji",
                    Debit = 0m,
                    Credit = 14_000_000m
                },
                new SeedJournalLine
                {
                    AccountId = taxPayableAccountId,
                    CostCenterId = null,
                    Description = "Utang PPh 21",
                    Debit = 0m,
                    Credit = 750_000m
                },
                new SeedJournalLine
                {
                    AccountId = bpjsPayableAccountId,
                    CostCenterId = null,
                    Description = "Utang BPJS",
                    Debit = 0m,
                    Credit = 2_250_000m
                }
            ],
            now,
            ct);
    }

    private async Task<FinJournalEntry> EnsureJournalAsync(
        string journalNo,
        int periodId,
        DateOnly date,
        string description,
        FinanceJournalSource source,
        int? sourceRefId,
        string? sourceRefType,
        FinanceJournalStatus status,
        int? postedBy,
        DateTimeOffset? postedAt,
        string currencyCode,
        decimal exchangeRate,
        IReadOnlyList<SeedJournalLine> lines,
        DateTimeOffset now,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(journalNo))
        {
            throw new InvalidOperationException("Journal number is required.");
        }

        if (lines.Count == 0)
        {
            throw new InvalidOperationException("Journal lines are required.");
        }

        var normalizedJournalNo = journalNo.Trim().ToUpperInvariant();
        var normalizedCurrencyCode = string.IsNullOrWhiteSpace(currencyCode) ? "IDR" : currencyCode.Trim().ToUpperInvariant();
        var normalizedDescription = description.Trim();
        var normalizedSourceRefType = string.IsNullOrWhiteSpace(sourceRefType) ? null : sourceRefType.Trim();
        var normalizedExchangeRate = exchangeRate <= 0 ? 1m : exchangeRate;

        var existing = await dbContext.FinJournalEntries
            .IgnoreQueryFilters()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.JournalNo == normalizedJournalNo, ct);

        if (existing is null)
        {
            existing = new FinJournalEntry
            {
                JournalNo = normalizedJournalNo,
                PeriodId = periodId,
                Date = date,
                Description = normalizedDescription,
                Source = source,
                SourceRefId = sourceRefId,
                SourceRefType = normalizedSourceRefType,
                Status = status,
                PostedBy = status == FinanceJournalStatus.Posted ? postedBy : null,
                PostedAt = status == FinanceJournalStatus.Posted ? postedAt : null,
                CurrencyCode = normalizedCurrencyCode,
                ExchangeRate = normalizedExchangeRate,
                CreatedBy = "system",
                CreatedAt = now
            };

            dbContext.FinJournalEntries.Add(existing);
        }
        else
        {
            dbContext.FinJournalEntryLines.RemoveRange(existing.Lines);
            existing.Lines.Clear();

            existing.PeriodId = periodId;
            existing.Date = date;
            existing.Description = normalizedDescription;
            existing.Source = source;
            existing.SourceRefId = sourceRefId;
            existing.SourceRefType = normalizedSourceRefType;
            existing.Status = status;
            existing.PostedBy = status == FinanceJournalStatus.Posted ? postedBy : null;
            existing.PostedAt = status == FinanceJournalStatus.Posted ? postedAt : null;
            existing.CurrencyCode = normalizedCurrencyCode;
            existing.ExchangeRate = normalizedExchangeRate;
            existing.IsDeleted = false;
            existing.DeletedAt = null;
            existing.UpdatedBy = "system";
            existing.UpdatedAt = now;
        }

        for (var index = 0; index < lines.Count; index++)
        {
            var line = lines[index];

            if (line.Debit < 0 || line.Credit < 0)
            {
                throw new InvalidOperationException($"Journal line {index + 1} cannot be negative.");
            }

            if ((line.Debit > 0m && line.Credit > 0m) || (line.Debit == 0m && line.Credit == 0m))
            {
                throw new InvalidOperationException($"Journal line {index + 1} must contain only debit or credit amount.");
            }

            var debitBase = decimal.Round(line.Debit * normalizedExchangeRate, 4, MidpointRounding.AwayFromZero);
            var creditBase = decimal.Round(line.Credit * normalizedExchangeRate, 4, MidpointRounding.AwayFromZero);

            existing.Lines.Add(new FinJournalEntryLine
            {
                LineNo = index + 1,
                AccountId = line.AccountId,
                CostCenterId = line.CostCenterId,
                Description = string.IsNullOrWhiteSpace(line.Description) ? null : line.Description.Trim(),
                Debit = line.Debit,
                Credit = line.Credit,
                DebitBase = debitBase,
                CreditBase = creditBase
            });
        }

        if (status == FinanceJournalStatus.Posted)
        {
            var totalDebitBase = existing.Lines.Sum(x => x.DebitBase);
            var totalCreditBase = existing.Lines.Sum(x => x.CreditBase);

            if (totalDebitBase != totalCreditBase)
            {
                throw new InvalidOperationException($"Journal '{normalizedJournalNo}' is not balanced.");
            }
        }

        await dbContext.SaveChangesAsync(ct);
        return existing;
    }

    private sealed class SeedJournalLine
    {
        public int AccountId { get; init; }
        public int? CostCenterId { get; init; }
        public string? Description { get; init; }
        public decimal Debit { get; init; }
        public decimal Credit { get; init; }
    }
    private static string BuildCostCenterCode(string departmentCode)
    {
        var raw = string.IsNullOrWhiteSpace(departmentCode)
            ? "GEN"
            : new string(departmentCode.Trim().Where(char.IsLetterOrDigit).ToArray());

        if (string.IsNullOrWhiteSpace(raw))
        {
            raw = "GEN";
        }

        var code = $"CC-{raw.ToUpperInvariant()}";
        return code.Length <= 20 ? code : code[..20];
    }
    private async Task<CfgMenu> EnsureMenuAsync(
        int moduleId,
        int? parentId,
        string name,
        string? url,
        string? icon,
        int sortOrder,
        DateTimeOffset now,
        CancellationToken ct)
    {
        var existing = await dbContext.CfgMenus
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x =>
                x.ModuleId == moduleId &&
                x.ParentId == parentId &&
                x.Name == name,
                ct);

        if (existing is not null)
        {
            existing.Url = url;
            existing.Icon = icon;
            existing.SortOrder = sortOrder;
            existing.IsActive = true;
            existing.UpdatedBy = "system";
            existing.UpdatedAt = now;
            await dbContext.SaveChangesAsync(ct);
            return existing;
        }

        var menu = new CfgMenu
        {
            ModuleId = moduleId,
            ParentId = parentId,
            Name = name,
            Url = url,
            Icon = icon,
            SortOrder = sortOrder,
            IsActive = true,
            CreatedBy = "system",
            CreatedAt = now
        };

        dbContext.CfgMenus.Add(menu);
        await dbContext.SaveChangesAsync(ct);
        return menu;
    }
}















