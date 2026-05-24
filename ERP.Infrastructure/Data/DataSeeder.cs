using ERP.Application.Services;
using ERP.Domain.Entities.Config;
using ERP.Domain.Entities.HR;
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
        await SeedAdminUserAsync(now, ct);
        await SeedMenusAsync(now, ct);
        await SeedSuperAdminPermissionsAsync(ct);
    }

    private async Task SeedModulesAsync(DateTimeOffset now, CancellationToken ct)
    {
        await EnsureModuleAsync("Human Resources", "HR", "bi-people", 1, now, ct);
        await EnsureModuleAsync("System Configuration", "CFG", "bi-gear", 2, now, ct);
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

        var hrEmployees = await EnsureMenuAsync(hrModule.Id, null, "Employees", null, "bi-people", 1, now, ct);
        await EnsureMenuAsync(hrModule.Id, hrEmployees.Id, "All Employees", "/hr/employees", "bi-list", 1, now, ct);
        await EnsureMenuAsync(hrModule.Id, hrEmployees.Id, "Add Employee", "/hr/employees/create", "bi-plus-circle", 2, now, ct);
        await EnsureMenuAsync(hrModule.Id, hrEmployees.Id, "Departments", "/hr/departments", "bi-diagram-3", 3, now, ct);
        await EnsureMenuAsync(hrModule.Id, hrEmployees.Id, "Positions", "/hr/positions", "bi-person-badge", 4, now, ct);

        var hrAttendance = await EnsureMenuAsync(hrModule.Id, null, "Attendance", null, "bi-calendar-check", 2, now, ct);
        await EnsureMenuAsync(hrModule.Id, hrAttendance.Id, "Daily Attendance", "/hr/attendance", "bi-clock", 1, now, ct);
        await EnsureMenuAsync(hrModule.Id, hrAttendance.Id, "Attendance Report", "/hr/attendance/report", "bi-file-earmark-text", 2, now, ct);

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
