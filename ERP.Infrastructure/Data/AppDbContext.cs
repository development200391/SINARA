using System.Linq.Expressions;
using ERP.Domain.Entities;
using ERP.Domain.Entities.Config;
using ERP.Domain.Entities.HR;
using ERP.Domain.Entities.System;
using ERP.Domain.Interfaces;
using ERP.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<CfgModule> CfgModules => Set<CfgModule>();
    public DbSet<CfgMenu> CfgMenus => Set<CfgMenu>();
    public DbSet<CfgRole> CfgRoles => Set<CfgRole>();
    public DbSet<CfgRoleMenuPermission> CfgRoleMenuPermissions => Set<CfgRoleMenuPermission>();

    public DbSet<SysUser> SysUsers => Set<SysUser>();
    public DbSet<SysUserRole> SysUserRoles => Set<SysUserRole>();
    public DbSet<SysRefreshToken> SysRefreshTokens => Set<SysRefreshToken>();
    public DbSet<SysAuditLog> SysAuditLogs => Set<SysAuditLog>();

    public DbSet<HrDepartment> HrDepartments => Set<HrDepartment>();
    public DbSet<HrPosition> HrPositions => Set<HrPosition>();
    public DbSet<HrEmployee> HrEmployees => Set<HrEmployee>();
    public DbSet<HrAttendanceRecord> HrAttendanceRecords => Set<HrAttendanceRecord>();
    public DbSet<HrAttendanceSetting> HrAttendanceSettings => Set<HrAttendanceSetting>();
    public DbSet<HrLeaveType> HrLeaveTypes => Set<HrLeaveType>();
    public DbSet<HrLeaveRequest> HrLeaveRequests => Set<HrLeaveRequest>();
    public DbSet<HrPayrollRun> HrPayrollRuns => Set<HrPayrollRun>();
    public DbSet<HrPayrollDetail> HrPayrollDetails => Set<HrPayrollDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");

        ConfigureCfgModule(modelBuilder.Entity<CfgModule>());
        ConfigureCfgMenu(modelBuilder.Entity<CfgMenu>());
        ConfigureCfgRole(modelBuilder.Entity<CfgRole>());
        ConfigureCfgRoleMenuPermission(modelBuilder.Entity<CfgRoleMenuPermission>());

        ConfigureSysUser(modelBuilder.Entity<SysUser>());
        ConfigureSysUserRole(modelBuilder.Entity<SysUserRole>());
        ConfigureSysRefreshToken(modelBuilder.Entity<SysRefreshToken>());
        ConfigureSysAuditLog(modelBuilder.Entity<SysAuditLog>());

        ConfigureHrDepartment(modelBuilder.Entity<HrDepartment>());
        ConfigureHrPosition(modelBuilder.Entity<HrPosition>());
        ConfigureHrEmployee(modelBuilder.Entity<HrEmployee>());
        ConfigureHrAttendanceRecord(modelBuilder.Entity<HrAttendanceRecord>());
        ConfigureHrAttendanceSetting(modelBuilder.Entity<HrAttendanceSetting>());
        ConfigureHrLeaveType(modelBuilder.Entity<HrLeaveType>());
        ConfigureHrLeaveRequest(modelBuilder.Entity<HrLeaveRequest>());
        ConfigureHrPayrollRun(modelBuilder.Entity<HrPayrollRun>());
        ConfigureHrPayrollDetail(modelBuilder.Entity<HrPayrollDetail>());

        ApplySoftDeleteQueryFilters(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditAndSoftDeleteRules();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditAndSoftDeleteRules();
        return base.SaveChanges();
    }

    private void ApplyAuditAndSoftDeleteRules()
    {
        var utcNow = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = utcNow;
                }

                if (string.IsNullOrWhiteSpace(entry.Entity.CreatedBy))
                {
                    entry.Entity.CreatedBy = "system";
                }
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = utcNow;
                if (string.IsNullOrWhiteSpace(entry.Entity.UpdatedBy))
                {
                    entry.Entity.UpdatedBy = "system";
                }
            }

            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = utcNow;
                entry.Entity.UpdatedAt = utcNow;
                entry.Entity.UpdatedBy = "system";
            }
        }
    }

    private static void ConfigureAuditEntity<TEntity>(EntityTypeBuilder<TEntity> builder) where TEntity : BaseEntity
    {
        builder.Property(e => e.Id).UseIdentityAlwaysColumn();
        builder.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);
        builder.Property(e => e.UpdatedAt).HasColumnType("timestamptz");
        builder.Property(e => e.IsDeleted).HasDefaultValue(false).IsRequired();
        builder.Property(e => e.DeletedAt).HasColumnType("timestamptz");

        builder.HasIndex(e => e.IsDeleted);
        builder.HasIndex(e => e.CreatedAt);
    }

    private static void ConfigureCfgModule(EntityTypeBuilder<CfgModule> builder)
    {
        builder.ToTable("cfg_modules");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Icon).HasMaxLength(50);
        builder.Property(x => x.SortOrder).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();
    }

    private static void ConfigureCfgMenu(EntityTypeBuilder<CfgMenu> builder)
    {
        builder.ToTable("cfg_menus");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.ModuleId).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Url).HasMaxLength(200);
        builder.Property(x => x.Icon).HasMaxLength(50);
        builder.Property(x => x.SortOrder).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasOne(x => x.Module)
            .WithMany(x => x.Menus)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ModuleId);
        builder.HasIndex(x => x.ParentId);
    }

    private static void ConfigureCfgRole(EntityTypeBuilder<CfgRole> builder)
    {
        builder.ToTable("cfg_roles");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.IsSystem).HasDefaultValue(false).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasIndex(x => x.Name);
    }

    private static void ConfigureCfgRoleMenuPermission(EntityTypeBuilder<CfgRoleMenuPermission> builder)
    {
        builder.ToTable("cfg_role_menu_permissions");

        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.CanView).HasDefaultValue(false).IsRequired();
        builder.Property(x => x.CanCreate).HasDefaultValue(false).IsRequired();
        builder.Property(x => x.CanEdit).HasDefaultValue(false).IsRequired();
        builder.Property(x => x.CanDelete).HasDefaultValue(false).IsRequired();

        builder.HasOne(x => x.Role)
            .WithMany(x => x.RoleMenuPermissions)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Menu)
            .WithMany(x => x.RoleMenuPermissions)
            .HasForeignKey(x => x.MenuId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.RoleId);
        builder.HasIndex(x => x.MenuId);
        builder.HasIndex(x => new { x.RoleId, x.MenuId }).IsUnique();
    }

    private static void ConfigureSysUser(EntityTypeBuilder<SysUser> builder)
    {
        builder.ToTable("sys_users");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.Username).HasMaxLength(100).IsRequired();
        builder.Property(x => x.PasswordHash).HasColumnType("text").IsRequired();
        builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(30);
        builder.Property(x => x.AvatarUrl).HasColumnType("text");
        builder.Property(x => x.LanguagePreference).HasMaxLength(10).HasDefaultValue("en").IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasIndex(x => x.Username).IsUnique();
        builder.HasIndex(x => x.Email).IsUnique();
    }

    private static void ConfigureSysUserRole(EntityTypeBuilder<SysUserRole> builder)
    {
        builder.ToTable("sys_user_roles");

        builder.Property(x => x.Id).UseIdentityAlwaysColumn();

        builder.HasOne(x => x.User)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Role)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.RoleId);
        builder.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();
    }

    private static void ConfigureSysRefreshToken(EntityTypeBuilder<SysRefreshToken> builder)
    {
        builder.ToTable("sys_refresh_tokens");

        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.Token).HasColumnType("text").IsRequired();
        builder.Property(x => x.ExpiresAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.RevokedAt).HasColumnType("timestamptz");
        builder.Property(x => x.CreatedByIp).HasMaxLength(50);

        builder.HasOne(x => x.User)
            .WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Token).IsUnique();
    }

    private static void ConfigureSysAuditLog(EntityTypeBuilder<SysAuditLog> builder)
    {
        builder.ToTable("sys_audit_logs");

        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.Username).HasMaxLength(100);
        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EntityName).HasMaxLength(100);
        builder.Property(x => x.EntityId).HasMaxLength(50);
        builder.Property(x => x.OldValues).HasColumnType("text");
        builder.Property(x => x.NewValues).HasColumnType("text");
        builder.Property(x => x.IpAddress).HasMaxLength(50);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz").IsRequired();

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Action);
        builder.HasIndex(x => x.CreatedAt);
    }

    private static void ConfigureHrDepartment(EntityTypeBuilder<HrDepartment> builder)
    {
        builder.ToTable("hr_departments");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasOne(x => x.Manager)
            .WithMany(x => x.DepartmentsAsManager)
            .HasForeignKey(x => x.ManagerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.ParentDepartment)
            .WithMany(x => x.ChildDepartments)
            .HasForeignKey(x => x.ParentDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.ManagerId);
        builder.HasIndex(x => x.ParentDepartmentId);
    }

    private static void ConfigureHrPosition(EntityTypeBuilder<HrPosition> builder)
    {
        builder.ToTable("hr_positions");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Level).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasOne(x => x.Department)
            .WithMany(x => x.Positions)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.DepartmentId);
    }

    private static void ConfigureHrEmployee(EntityTypeBuilder<HrEmployee> builder)
    {
        builder.ToTable("hr_employees");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.EmployeeCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.Phone).HasMaxLength(30);
        builder.Property(x => x.HireDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.TerminationDate).HasColumnType("date");
        builder.Property(x => x.EmploymentStatus).HasConversion<int>().IsRequired();

        builder.HasOne(x => x.Department)
            .WithMany(x => x.Employees)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Position)
            .WithMany(x => x.Employees)
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany(x => x.Employees)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.EmployeeCode).IsUnique();
        builder.HasIndex(x => x.DepartmentId);
        builder.HasIndex(x => x.PositionId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.EmploymentStatus);
    }

    private static void ConfigureHrAttendanceRecord(EntityTypeBuilder<HrAttendanceRecord> builder)
    {
        builder.ToTable("hr_attendance_records");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.Date).HasColumnType("date").IsRequired();
        builder.Property(x => x.CheckIn).HasColumnType("timestamptz");
        builder.Property(x => x.CheckOut).HasColumnType("timestamptz");
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.Notes).HasColumnType("text");

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.AttendanceRecords)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.EmployeeId);
        builder.HasIndex(x => x.Date);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.EmployeeId, x.Date }).IsUnique();
    }

    private static void ConfigureHrAttendanceSetting(EntityTypeBuilder<HrAttendanceSetting> builder)
    {
        builder.ToTable("hr_attendance_settings");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.SingletonKey).HasMaxLength(20).HasDefaultValue("default").IsRequired();
        builder.Property(x => x.AttendancePeriodStartDay).HasDefaultValue(26).IsRequired();
        builder.Property(x => x.AttendancePeriodEndDay).HasDefaultValue(25).IsRequired();
        builder.Property(x => x.CheckInToleranceMinutes).HasDefaultValue(10).IsRequired();
        builder.Property(x => x.WorkStart).HasColumnType("time").IsRequired();
        builder.Property(x => x.WorkEnd).HasColumnType("time").IsRequired();
        builder.Property(x => x.BreakStart).HasColumnType("time").IsRequired();
        builder.Property(x => x.BreakEnd).HasColumnType("time").IsRequired();
        builder.Property(x => x.MinimumOtMinutes).HasDefaultValue(60).IsRequired();

        builder.HasIndex(x => x.SingletonKey).IsUnique();
    }

    private static void ConfigureHrLeaveType(EntityTypeBuilder<HrLeaveType> builder)
    {
        builder.ToTable("hr_leave_types");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.MaxDaysPerYear).IsRequired();
        builder.Property(x => x.IsCarryOver).HasDefaultValue(false).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();
    }

    private static void ConfigureHrLeaveRequest(EntityTypeBuilder<HrLeaveRequest> builder)
    {
        builder.ToTable("hr_leave_requests");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.StartDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.EndDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.TotalDays).IsRequired();
        builder.Property(x => x.Reason).HasColumnType("text");
        builder.Property(x => x.Status).HasConversion<int>().HasDefaultValue(LeaveStatus.Pending).IsRequired();
        builder.Property(x => x.ApprovedAt).HasColumnType("timestamptz");

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.LeaveRequests)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LeaveType)
            .WithMany(x => x.LeaveRequests)
            .HasForeignKey(x => x.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ApprovedByUser)
            .WithMany(x => x.ApprovedLeaveRequests)
            .HasForeignKey(x => x.ApprovedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.EmployeeId);
        builder.HasIndex(x => x.LeaveTypeId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.ApprovedBy);
    }

    private static void ConfigureHrPayrollRun(EntityTypeBuilder<HrPayrollRun> builder)
    {
        builder.ToTable("hr_payroll_runs");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.PeriodMonth).IsRequired();
        builder.Property(x => x.PeriodYear).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().HasDefaultValue(PayrollStatus.Draft).IsRequired();
        builder.Property(x => x.ProcessedAt).HasColumnType("timestamptz");

        builder.HasOne(x => x.ProcessedByUser)
            .WithMany(x => x.ProcessedPayrollRuns)
            .HasForeignKey(x => x.ProcessedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.ProcessedBy);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.PeriodMonth, x.PeriodYear }).IsUnique();
    }

    private static void ConfigureHrPayrollDetail(EntityTypeBuilder<HrPayrollDetail> builder)
    {
        builder.ToTable("hr_payroll_details");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.BasicSalary).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.Allowances).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.Deductions).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.GrossSalary).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.TaxAmount).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.NetSalary).HasColumnType("numeric(18,4)").IsRequired();

        builder.HasOne(x => x.PayrollRun)
            .WithMany(x => x.PayrollDetails)
            .HasForeignKey(x => x.PayrollRunId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.PayrollDetails)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.PayrollRunId);
        builder.HasIndex(x => x.EmployeeId);
        builder.HasIndex(x => new { x.PayrollRunId, x.EmployeeId }).IsUnique();
    }

    private static void ApplySoftDeleteQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var propertyMethod = typeof(EF)
                .GetMethod(nameof(EF.Property), [typeof(object), typeof(string)])!
                .MakeGenericMethod(typeof(bool));
            var isDeletedProperty = Expression.Call(propertyMethod, parameter, Expression.Constant(nameof(ISoftDelete.IsDeleted)));
            var body = Expression.Equal(isDeletedProperty, Expression.Constant(false));
            var lambda = Expression.Lambda(body, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }
}

