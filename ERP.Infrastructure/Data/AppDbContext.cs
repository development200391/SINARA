using System.Linq.Expressions;
using ERP.Domain.Entities;
using ERP.Domain.Entities.Config;
using ERP.Domain.Entities.HR;
using ERP.Domain.Entities.Finance;
using ERP.Domain.Entities.System;
using ERP.Domain.Entities.Inventory;
using ERP.Domain.Entities.Purchasing;
using ERP.Domain.Entities.Sales;
using ERP.Domain.Entities.FixedAssets;
using ERP.Domain.Entities.Manufacturing;
using ERP.Domain.Interfaces;
using ERP.Domain.Enums;
using ERP.Domain.Enums.Inventory;
using ERP.Domain.Enums.Purchasing;
using ERP.Domain.Enums.Sales;
using ERP.Domain.Enums.FixedAssets;
using ERP.Domain.Enums.Manufacturing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL;

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
    public DbSet<HrHoliday> HrHolidays => Set<HrHoliday>();
    public DbSet<HrLeaveType> HrLeaveTypes => Set<HrLeaveType>();
    public DbSet<HrLeaveRequest> HrLeaveRequests => Set<HrLeaveRequest>();
    public DbSet<HrPayrollRun> HrPayrollRuns => Set<HrPayrollRun>();
    public DbSet<HrPayrollDetail> HrPayrollDetails => Set<HrPayrollDetail>();

    public DbSet<FinAccountGroup> FinAccountGroups => Set<FinAccountGroup>();
    public DbSet<FinAccount> FinAccounts => Set<FinAccount>();
    public DbSet<FinCostCenter> FinCostCenters => Set<FinCostCenter>();
    public DbSet<FinCurrency> FinCurrencies => Set<FinCurrency>();
    public DbSet<FinExchangeRate> FinExchangeRates => Set<FinExchangeRate>();
    public DbSet<FinFiscalYear> FinFiscalYears => Set<FinFiscalYear>();
    public DbSet<FinPeriod> FinPeriods => Set<FinPeriod>();
    public DbSet<FinTaxCode> FinTaxCodes => Set<FinTaxCode>();
    public DbSet<FinJournalEntry> FinJournalEntries => Set<FinJournalEntry>();
    public DbSet<FinJournalEntryLine> FinJournalEntryLines => Set<FinJournalEntryLine>();
    public DbSet<FinVendor> FinVendors => Set<FinVendor>();
    public DbSet<FinApInvoice> FinApInvoices => Set<FinApInvoice>();
    public DbSet<FinApInvoiceLine> FinApInvoiceLines => Set<FinApInvoiceLine>();
    public DbSet<FinApPayment> FinApPayments => Set<FinApPayment>();
    public DbSet<FinApPaymentApplication> FinApPaymentApplications => Set<FinApPaymentApplication>();
    public DbSet<FinCustomer> FinCustomers => Set<FinCustomer>();
    public DbSet<FinArInvoice> FinArInvoices => Set<FinArInvoice>();
    public DbSet<FinArInvoiceLine> FinArInvoiceLines => Set<FinArInvoiceLine>();
    public DbSet<FinArReceipt> FinArReceipts => Set<FinArReceipt>();
    public DbSet<FinArReceiptApplication> FinArReceiptApplications => Set<FinArReceiptApplication>();
    public DbSet<FinBudget> FinBudgets => Set<FinBudget>();
    public DbSet<FinBudgetLine> FinBudgetLines => Set<FinBudgetLine>();
    

    public DbSet<InvItemCategory> InvItemCategories => Set<InvItemCategory>();

    
    public DbSet<InvUnitOfMeasure> InvUnitsOfMeasure => Set<InvUnitOfMeasure>();
    
    public DbSet<InvBrand> InvBrands => Set<InvBrand>();
    
    public DbSet<InvItem> InvItems => Set<InvItem>();
    
    public DbSet<InvItemUnitConversion> InvItemUnitConversions => Set<InvItemUnitConversion>();
    
    public DbSet<InvWarehouse> InvWarehouses => Set<InvWarehouse>();
    
    public DbSet<InvWarehouseLocation> InvWarehouseLocations => Set<InvWarehouseLocation>();
    
    public DbSet<InvStockBalance> InvStockBalances => Set<InvStockBalance>();
    
    public DbSet<InvGoodsReceipt> InvGoodsReceipts => Set<InvGoodsReceipt>();
    public DbSet<InvGoodsReceiptLine> InvGoodsReceiptLines => Set<InvGoodsReceiptLine>();
    public DbSet<InvGoodsIssue> InvGoodsIssues => Set<InvGoodsIssue>();
    public DbSet<InvGoodsIssueLine> InvGoodsIssueLines => Set<InvGoodsIssueLine>();
    public DbSet<InvStockTransfer> InvStockTransfers => Set<InvStockTransfer>();
    public DbSet<InvStockTransferLine> InvStockTransferLines => Set<InvStockTransferLine>();
    public DbSet<InvStockAdjustment> InvStockAdjustments => Set<InvStockAdjustment>();
    public DbSet<InvStockAdjustmentLine> InvStockAdjustmentLines => Set<InvStockAdjustmentLine>();
    public DbSet<InvStockOpname> InvStockOpnames => Set<InvStockOpname>();
    public DbSet<InvStockOpnameLine> InvStockOpnameLines => Set<InvStockOpnameLine>();
    public DbSet<InvStockMovement> InvStockMovements => Set<InvStockMovement>();

    public DbSet<PurVendorCategory> PurVendorCategories => Set<PurVendorCategory>();
    public DbSet<PurApprovalConfig> PurApprovalConfigs => Set<PurApprovalConfig>();
    public DbSet<PurBuyerGroup> PurBuyerGroups => Set<PurBuyerGroup>();
    public DbSet<PurBuyerGroupCategory> PurBuyerGroupCategories => Set<PurBuyerGroupCategory>();

    public DbSet<SalCustomerCategory> SalCustomerCategories => Set<SalCustomerCategory>();
    public DbSet<SalPriceList> SalPriceLists => Set<SalPriceList>();
    public DbSet<SalPriceListItem> SalPriceListItems => Set<SalPriceListItem>();
    public DbSet<SalApprovalConfig> SalApprovalConfigs => Set<SalApprovalConfig>();
    public DbSet<SalSalesTeam> SalSalesTeams => Set<SalSalesTeam>();
    public DbSet<SalSalesTeamMember> SalSalesTeamMembers => Set<SalSalesTeamMember>();
    public DbSet<MfgWorkCenter> MfgWorkCenters => Set<MfgWorkCenter>();
    public DbSet<MfgRouting> MfgRoutings => Set<MfgRouting>();
    public DbSet<MfgBom> MfgBoms => Set<MfgBom>();
    public DbSet<MfgWorkOrder> MfgWorkOrders => Set<MfgWorkOrder>();
    public DbSet<MfgMrpRun> MfgMrpRuns => Set<MfgMrpRun>();
    public DbSet<MfgQcParameter> MfgQcParameters => Set<MfgQcParameter>();
    public DbSet<MfgQcInspection> MfgQcInspections => Set<MfgQcInspection>();
    public DbSet<MfgScrapRecord> MfgScrapRecords => Set<MfgScrapRecord>();
    public DbSet<MfgReworkOrder> MfgReworkOrders => Set<MfgReworkOrder>();
    public DbSet<MfgOeeSnapshot> MfgOeeSnapshots => Set<MfgOeeSnapshot>();
    public DbSet<FaAssetCategory> FaAssetCategories => Set<FaAssetCategory>();
    public DbSet<FaLocation> FaLocations => Set<FaLocation>();
    public DbSet<FaDepreciationConfig> FaDepreciationConfigs => Set<FaDepreciationConfig>();
    public DbSet<FaAsset> FaAssets => Set<FaAsset>();
    public DbSet<FaAssetDocument> FaAssetDocuments => Set<FaAssetDocument>();
    public DbSet<FaDepreciationRun> FaDepreciationRuns => Set<FaDepreciationRun>();
    public DbSet<FaDepreciationSchedule> FaDepreciationSchedules => Set<FaDepreciationSchedule>();
    public DbSet<FaAssetTransfer> FaAssetTransfers => Set<FaAssetTransfer>();
    public DbSet<FaMaintenanceOrder> FaMaintenanceOrders => Set<FaMaintenanceOrder>();
    public DbSet<FaDisposal> FaDisposals => Set<FaDisposal>();
    public DbSet<FaRevaluation> FaRevaluations => Set<FaRevaluation>();
    public DbSet<FaAssetHistory> FaAssetHistories => Set<FaAssetHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.HasPostgresEnum<HolidayType>("public", "holiday_type_enum");

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
        ConfigureHrHoliday(modelBuilder.Entity<HrHoliday>());
        ConfigureHrLeaveType(modelBuilder.Entity<HrLeaveType>());
        ConfigureHrLeaveRequest(modelBuilder.Entity<HrLeaveRequest>());
        ConfigureHrPayrollRun(modelBuilder.Entity<HrPayrollRun>());
        ConfigureHrPayrollDetail(modelBuilder.Entity<HrPayrollDetail>());

        ConfigureFinAccountGroup(modelBuilder.Entity<FinAccountGroup>());
        ConfigureFinAccount(modelBuilder.Entity<FinAccount>());
        ConfigureFinCostCenter(modelBuilder.Entity<FinCostCenter>());
        ConfigureFinCurrency(modelBuilder.Entity<FinCurrency>());
        ConfigureFinExchangeRate(modelBuilder.Entity<FinExchangeRate>());
        ConfigureFinFiscalYear(modelBuilder.Entity<FinFiscalYear>());
        ConfigureFinPeriod(modelBuilder.Entity<FinPeriod>());
        ConfigureFinTaxCode(modelBuilder.Entity<FinTaxCode>());
        ConfigureFinJournalEntry(modelBuilder.Entity<FinJournalEntry>());
        ConfigureFinJournalEntryLine(modelBuilder.Entity<FinJournalEntryLine>());
        ConfigureFinVendor(modelBuilder.Entity<FinVendor>());
        ConfigureFinApInvoice(modelBuilder.Entity<FinApInvoice>());
        ConfigureFinApInvoiceLine(modelBuilder.Entity<FinApInvoiceLine>());
        ConfigureFinApPayment(modelBuilder.Entity<FinApPayment>());
        ConfigureFinApPaymentApplication(modelBuilder.Entity<FinApPaymentApplication>());
        ConfigureFinCustomer(modelBuilder.Entity<FinCustomer>());
        ConfigureFinArInvoice(modelBuilder.Entity<FinArInvoice>());
        ConfigureFinArInvoiceLine(modelBuilder.Entity<FinArInvoiceLine>());
        ConfigureFinArReceipt(modelBuilder.Entity<FinArReceipt>());
        ConfigureFinArReceiptApplication(modelBuilder.Entity<FinArReceiptApplication>());
        ConfigureFinBudget(modelBuilder.Entity<FinBudget>());
        ConfigureFinBudgetLine(modelBuilder.Entity<FinBudgetLine>());
        

        ConfigureInvItemCategory(modelBuilder.Entity<InvItemCategory>());

        
        ConfigureInvUnitOfMeasure(modelBuilder.Entity<InvUnitOfMeasure>());
        
        ConfigureInvBrand(modelBuilder.Entity<InvBrand>());
        
        ConfigureInvItem(modelBuilder.Entity<InvItem>());
        
        ConfigureInvItemUnitConversion(modelBuilder.Entity<InvItemUnitConversion>());
        
        ConfigureInvWarehouse(modelBuilder.Entity<InvWarehouse>());
        
        ConfigureInvWarehouseLocation(modelBuilder.Entity<InvWarehouseLocation>());
        
        ConfigureInvStockBalance(modelBuilder.Entity<InvStockBalance>());
        
        ConfigureInvGoodsReceipt(modelBuilder.Entity<InvGoodsReceipt>());
        ConfigureInvGoodsReceiptLine(modelBuilder.Entity<InvGoodsReceiptLine>());
        ConfigureInvGoodsIssue(modelBuilder.Entity<InvGoodsIssue>());
        ConfigureInvGoodsIssueLine(modelBuilder.Entity<InvGoodsIssueLine>());
        ConfigureInvStockTransfer(modelBuilder.Entity<InvStockTransfer>());
        ConfigureInvStockTransferLine(modelBuilder.Entity<InvStockTransferLine>());
        ConfigureInvStockAdjustment(modelBuilder.Entity<InvStockAdjustment>());
        ConfigureInvStockAdjustmentLine(modelBuilder.Entity<InvStockAdjustmentLine>());
        ConfigureInvStockOpname(modelBuilder.Entity<InvStockOpname>());
        ConfigureInvStockOpnameLine(modelBuilder.Entity<InvStockOpnameLine>());
        ConfigureInvStockMovement(modelBuilder.Entity<InvStockMovement>());

        ConfigurePurVendorCategory(modelBuilder.Entity<PurVendorCategory>());
        ConfigurePurApprovalConfig(modelBuilder.Entity<PurApprovalConfig>());
        ConfigurePurBuyerGroup(modelBuilder.Entity<PurBuyerGroup>());
        ConfigurePurBuyerGroupCategory(modelBuilder.Entity<PurBuyerGroupCategory>());

        ConfigureSalCustomerCategory(modelBuilder.Entity<SalCustomerCategory>());
        ConfigureSalPriceList(modelBuilder.Entity<SalPriceList>());
        ConfigureSalPriceListItem(modelBuilder.Entity<SalPriceListItem>());
        ConfigureSalApprovalConfig(modelBuilder.Entity<SalApprovalConfig>());
        ConfigureSalSalesTeam(modelBuilder.Entity<SalSalesTeam>());
        ConfigureSalSalesTeamMember(modelBuilder.Entity<SalSalesTeamMember>());
        ConfigureMfgWorkCenter(modelBuilder.Entity<MfgWorkCenter>());
        ConfigureMfgRouting(modelBuilder.Entity<MfgRouting>());
        ConfigureMfgBom(modelBuilder.Entity<MfgBom>());
        ConfigureMfgWorkOrder(modelBuilder.Entity<MfgWorkOrder>());
        ConfigureMfgMrpRun(modelBuilder.Entity<MfgMrpRun>());
        ConfigureMfgQcParameter(modelBuilder.Entity<MfgQcParameter>());
        ConfigureMfgQcInspection(modelBuilder.Entity<MfgQcInspection>());
        ConfigureMfgScrapRecord(modelBuilder.Entity<MfgScrapRecord>());
        ConfigureMfgReworkOrder(modelBuilder.Entity<MfgReworkOrder>());
        ConfigureMfgOeeSnapshot(modelBuilder.Entity<MfgOeeSnapshot>());
        ConfigureFaAssetCategory(modelBuilder.Entity<FaAssetCategory>());
        ConfigureFaLocation(modelBuilder.Entity<FaLocation>());
        ConfigureFaDepreciationConfig(modelBuilder.Entity<FaDepreciationConfig>());
        ConfigureFaAsset(modelBuilder.Entity<FaAsset>());
        ConfigureFaAssetDocument(modelBuilder.Entity<FaAssetDocument>());
        ConfigureFaDepreciationRun(modelBuilder.Entity<FaDepreciationRun>());
        ConfigureFaDepreciationSchedule(modelBuilder.Entity<FaDepreciationSchedule>());
        ConfigureFaAssetTransfer(modelBuilder.Entity<FaAssetTransfer>());
        ConfigureFaMaintenanceOrder(modelBuilder.Entity<FaMaintenanceOrder>());
        ConfigureFaDisposal(modelBuilder.Entity<FaDisposal>());
        ConfigureFaRevaluation(modelBuilder.Entity<FaRevaluation>());
        ConfigureFaAssetHistory(modelBuilder.Entity<FaAssetHistory>());
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
        builder.Property(x => x.PhotoPath).HasMaxLength(500);
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
        builder.Property(x => x.CheckInLatitude).HasColumnType("numeric(9,6)");
        builder.Property(x => x.CheckInLongitude).HasColumnType("numeric(9,6)");
        builder.Property(x => x.CheckOutLatitude).HasColumnType("numeric(9,6)");
        builder.Property(x => x.CheckOutLongitude).HasColumnType("numeric(9,6)");
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
        builder.Property(x => x.OfficeLatitude).HasColumnType("numeric(9,6)");
        builder.Property(x => x.OfficeLongitude).HasColumnType("numeric(9,6)");
        builder.Property(x => x.RadiusMeters).HasDefaultValue(100).IsRequired();

        builder.HasIndex(x => x.SingletonKey).IsUnique();
    }
    private static void ConfigureHrHoliday(EntityTypeBuilder<HrHoliday> builder)
    {
        builder.ToTable("hr_holiday");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.HolidayDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.HolidayType)
            .HasColumnType("holiday_type_enum")
            .HasDefaultValueSql("'national'::holiday_type_enum")
            .HasSentinel((HolidayType)(-1))
            .IsRequired();

        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();
        builder.Property(x => x.AppliesTo).HasMaxLength(100).HasDefaultValue("all");
        builder.Property(x => x.Year)
            .HasColumnType("smallint")
            .HasComputedColumnSql("EXTRACT(YEAR FROM holiday_date)::smallint", stored: true);

        builder.HasIndex(x => x.Year);
        builder.HasIndex(x => x.HolidayDate);
        builder.HasIndex(x => x.HolidayType);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => new { x.Name, x.HolidayDate }).IsUnique();
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

    private static void ConfigureFinAccountGroup(EntityTypeBuilder<FinAccountGroup> builder)
    {
        builder.ToTable("fin_account_groups");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Type).HasConversion<int>().IsRequired();
        builder.Property(x => x.NormalBalance).HasConversion<int>().IsRequired();
        builder.Property(x => x.SortOrder).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasOne(x => x.ParentGroup)
            .WithMany(x => x.ChildGroups)
            .HasForeignKey(x => x.ParentGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.ParentGroupId);
        builder.HasIndex(x => x.SortOrder);
        builder.HasIndex(x => x.IsActive);
    }

    private static void ConfigureFinAccount(EntityTypeBuilder<FinAccount> builder)
    {
        builder.ToTable("fin_accounts");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Type).HasConversion<int>().IsRequired();
        builder.Property(x => x.NormalBalance).HasConversion<int>().IsRequired();
        builder.Property(x => x.IsHeader).HasDefaultValue(false).IsRequired();
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.IsBankAccount).HasDefaultValue(false).IsRequired();
        builder.Property(x => x.BankName).HasMaxLength(100);
        builder.Property(x => x.BankAccountNo).HasMaxLength(50);
        builder.Property(x => x.CurrencyCode).HasMaxLength(10).HasDefaultValue("IDR").IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasOne(x => x.Group)
            .WithMany(x => x.Accounts)
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ParentAccount)
            .WithMany(x => x.ChildAccounts)
            .HasForeignKey(x => x.ParentAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Currency)
            .WithMany(x => x.Accounts)
            .HasForeignKey(x => x.CurrencyCode)
            .HasPrincipalKey(x => x.Code)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.GroupId);
        builder.HasIndex(x => x.ParentAccountId);
        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.CurrencyCode);
        builder.HasIndex(x => x.IsBankAccount);
        builder.HasIndex(x => x.IsActive);
    }

    private static void ConfigureFinCostCenter(EntityTypeBuilder<FinCostCenter> builder)
    {
        builder.ToTable("fin_cost_centers");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Manager)
            .WithMany()
            .HasForeignKey(x => x.ManagerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.BudgetAccount)
            .WithMany(x => x.BudgetCostCenters)
            .HasForeignKey(x => x.BudgetAccountId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.DepartmentId);
        builder.HasIndex(x => x.ManagerId);
        builder.HasIndex(x => x.BudgetAccountId);
        builder.HasIndex(x => x.IsActive);
    }

    private static void ConfigureFinCurrency(EntityTypeBuilder<FinCurrency> builder)
    {
        builder.ToTable("fin_currencies");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.Code).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Symbol).HasMaxLength(10).IsRequired();
        builder.Property(x => x.IsBaseCurrency).HasDefaultValue(false).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasAlternateKey(x => x.Code);
        builder.HasIndex(x => x.IsBaseCurrency);
        builder.HasIndex(x => x.IsActive);
    }

    private static void ConfigureFinExchangeRate(EntityTypeBuilder<FinExchangeRate> builder)
    {
        builder.ToTable("fin_exchange_rates");

        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.FromCurrencyCode).HasMaxLength(10).IsRequired();
        builder.Property(x => x.ToCurrencyCode).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Rate).HasColumnType("numeric(18,6)").IsRequired();
        builder.Property(x => x.EffectiveDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.Source).HasMaxLength(50);
        builder.Property(x => x.CreatedBy).HasMaxLength(100).HasDefaultValue("system").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz").HasDefaultValueSql("NOW()").IsRequired();

        builder.HasOne(x => x.FromCurrency)
            .WithMany(x => x.ExchangeRatesFrom)
            .HasForeignKey(x => x.FromCurrencyCode)
            .HasPrincipalKey(x => x.Code)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ToCurrency)
            .WithMany(x => x.ExchangeRatesTo)
            .HasForeignKey(x => x.ToCurrencyCode)
            .HasPrincipalKey(x => x.Code)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.FromCurrencyCode, x.EffectiveDate });
        builder.HasIndex(x => new { x.ToCurrencyCode, x.EffectiveDate });
        builder.HasIndex(x => new { x.FromCurrencyCode, x.ToCurrencyCode, x.EffectiveDate }).IsUnique();
    }

    private static void ConfigureFinFiscalYear(EntityTypeBuilder<FinFiscalYear> builder)
    {
        builder.ToTable("fin_fiscal_years");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.Property(x => x.StartDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.EndDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().HasDefaultValue(FinancePeriodStatus.Open).IsRequired();

        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasIndex(x => x.StartDate);
        builder.HasIndex(x => x.EndDate);
        builder.HasIndex(x => x.Status);
    }

    private static void ConfigureFinPeriod(EntityTypeBuilder<FinPeriod> builder)
    {
        builder.ToTable("fin_periods");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.PeriodNumber).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(30).IsRequired();
        builder.Property(x => x.StartDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.EndDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().HasDefaultValue(FinancePeriodStatus.Open).IsRequired();

        builder.HasOne(x => x.FiscalYear)
            .WithMany(x => x.Periods)
            .HasForeignKey(x => x.FiscalYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.FiscalYearId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.FiscalYearId, x.PeriodNumber }).IsUnique();
    }

    private static void ConfigureFinTaxCode(EntityTypeBuilder<FinTaxCode> builder)
    {
        builder.ToTable("fin_tax_codes");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Type).HasConversion<int>().IsRequired();
        builder.Property(x => x.Rate).HasColumnType("numeric(5,2)").IsRequired();
        builder.Property(x => x.IsInclusive).HasDefaultValue(false).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasOne(x => x.Account)
            .WithMany(x => x.TaxCodes)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.AccountId);
        builder.HasIndex(x => x.IsActive);
    }
    private static void ConfigureFinJournalEntry(EntityTypeBuilder<FinJournalEntry> builder)
    {
        builder.ToTable("fin_journal_entries");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.JournalNo).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Date).HasColumnType("date").IsRequired();
        builder.Property(x => x.Description).HasColumnType("text").IsRequired();
        builder.Property(x => x.Source).HasConversion<int>().IsRequired();
        builder.Property(x => x.SourceRefType).HasMaxLength(50);
        builder.Property(x => x.Status).HasConversion<int>().HasDefaultValue(FinanceJournalStatus.Draft).IsRequired();
        builder.Property(x => x.PostedAt).HasColumnType("timestamptz");
        builder.Property(x => x.CurrencyCode).HasMaxLength(10).HasDefaultValue("IDR").IsRequired();
        builder.Property(x => x.ExchangeRate).HasColumnType("numeric(18,6)").HasDefaultValue(1m).IsRequired();

        builder.HasOne(x => x.Period)
            .WithMany(x => x.JournalEntries)
            .HasForeignKey(x => x.PeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PostedByUser)
            .WithMany(x => x.PostedFinanceJournals)
            .HasForeignKey(x => x.PostedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.ReversedJournal)
            .WithMany(x => x.ReversalJournals)
            .HasForeignKey(x => x.ReversedJournalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Currency)
            .WithMany(x => x.JournalEntries)
            .HasForeignKey(x => x.CurrencyCode)
            .HasPrincipalKey(x => x.Code)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.JournalNo).IsUnique();
        builder.HasIndex(x => x.PeriodId);
        builder.HasIndex(x => x.Date);
        builder.HasIndex(x => x.Source);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.PostedBy);
        builder.HasIndex(x => x.ReversedJournalId);
        builder.HasIndex(x => x.CurrencyCode);
        builder.HasIndex(x => new { x.Source, x.SourceRefId, x.SourceRefType });
    }

    private static void ConfigureFinJournalEntryLine(EntityTypeBuilder<FinJournalEntryLine> builder)
    {
        builder.ToTable("fin_journal_entry_lines", t =>
        {
            t.HasCheckConstraint("ck_fin_journal_entry_lines_non_negative", "debit >= 0 AND credit >= 0");
            t.HasCheckConstraint("ck_fin_journal_entry_lines_single_side", "NOT (debit > 0 AND credit > 0)");
        });

        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.LineNo).IsRequired();
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.Debit).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.Credit).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.DebitBase).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.CreditBase).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();

        builder.HasOne(x => x.JournalEntry)
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.JournalEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Account)
            .WithMany(x => x.JournalLines)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CostCenter)
            .WithMany(x => x.JournalLines)
            .HasForeignKey(x => x.CostCenterId)
            .OnDelete(DeleteBehavior.SetNull);


        builder.HasIndex(x => x.JournalEntryId);
        builder.HasIndex(x => new { x.JournalEntryId, x.LineNo }).IsUnique();
        builder.HasIndex(x => x.AccountId);
        builder.HasIndex(x => x.CostCenterId);
    }
    
    private static void ConfigureFinVendor(EntityTypeBuilder<FinVendor> builder)
    {
        builder.ToTable("fin_vendors", t =>
        {
            t.HasCheckConstraint("ck_fin_vendors_lead_time_days_non_negative", "lead_time_days IS NULL OR lead_time_days >= 0");
            t.HasCheckConstraint("ck_fin_vendors_performance_score_range", "performance_score IS NULL OR (performance_score >= 0 AND performance_score <= 100)");
        });
        ConfigureAuditEntity(builder);

        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TaxId).HasMaxLength(30);
        builder.Property(x => x.Address).HasColumnType("text");
        builder.Property(x => x.Phone).HasMaxLength(30);
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.ContactPerson).HasMaxLength(100);
        builder.Property(x => x.PaymentTermsDays).HasDefaultValue(30).IsRequired();
        builder.Property(x => x.BankName).HasMaxLength(100);
        builder.Property(x => x.BankAccountNo).HasMaxLength(50);
        builder.Property(x => x.IsApprovedVendor).HasDefaultValue(false).IsRequired();
        builder.Property(x => x.ApprovedDate).HasColumnType("date");
        builder.Property(x => x.LeadTimeDays);
        builder.Property(x => x.PerformanceScore).HasColumnType("numeric(5,2)");
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasOne(x => x.DefaultAccount)
            .WithMany(x => x.VendorDefaultAccounts)
            .HasForeignKey(x => x.DefaultAccountId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.DefaultTaxCode)
            .WithMany(x => x.VendorDefaults)
            .HasForeignKey(x => x.DefaultTaxCodeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.VendorCategory)
            .WithMany(x => x.Vendors)
            .HasForeignKey(x => x.VendorCategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.BuyerGroup)
            .WithMany(x => x.Vendors)
            .HasForeignKey(x => x.BuyerGroupId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.DefaultAccountId);
        builder.HasIndex(x => x.DefaultTaxCodeId);
        builder.HasIndex(x => x.VendorCategoryId);
        builder.HasIndex(x => x.BuyerGroupId);
        builder.HasIndex(x => x.IsApprovedVendor);
        builder.HasIndex(x => x.PerformanceScore);
    }

    private static void ConfigurePurVendorCategory(EntityTypeBuilder<PurVendorCategory> builder)
    {
        builder.ToTable("pur_vendor_categories");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.IsActive);
    }

    private static void ConfigurePurApprovalConfig(EntityTypeBuilder<PurApprovalConfig> builder)
    {
        builder.ToTable("pur_approval_configs", t =>
        {
            t.HasCheckConstraint("ck_pur_approval_configs_level_positive", "level > 0");
            t.HasCheckConstraint("ck_pur_approval_configs_min_amount_non_negative", "min_amount >= 0");
            t.HasCheckConstraint("ck_pur_approval_configs_max_amount_non_negative", "max_amount IS NULL OR max_amount >= 0");
            t.HasCheckConstraint("ck_pur_approval_configs_amount_range", "max_amount IS NULL OR max_amount >= min_amount");
        });
        ConfigureAuditEntity(builder);

        builder.Property(x => x.DocumentType).HasConversion<int>().IsRequired();
        builder.Property(x => x.Level).IsRequired();
        builder.Property(x => x.MinAmount).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.MaxAmount).HasColumnType("numeric(18,4)");
        builder.Property(x => x.Notes).HasColumnType("text");
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasOne(x => x.ApproverEmployee)
            .WithMany()
            .HasForeignKey(x => x.ApproverEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.DocumentType);
        builder.HasIndex(x => x.Level);
        builder.HasIndex(x => x.ApproverEmployeeId);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => new { x.DocumentType, x.Level, x.MinAmount, x.MaxAmount }).IsUnique();
    }

    private static void ConfigurePurBuyerGroup(EntityTypeBuilder<PurBuyerGroup> builder)
    {
        builder.ToTable("pur_buyer_groups");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasOne(x => x.BuyerEmployee)
            .WithMany()
            .HasForeignKey(x => x.BuyerEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.BuyerEmployeeId);
        builder.HasIndex(x => x.IsActive);
    }

    private static void ConfigurePurBuyerGroupCategory(EntityTypeBuilder<PurBuyerGroupCategory> builder)
    {
        builder.ToTable("pur_buyer_group_categories");
        ConfigureAuditEntity(builder);

        builder.HasOne(x => x.BuyerGroup)
            .WithMany(x => x.CategoryMappings)
            .HasForeignKey(x => x.BuyerGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ItemCategory)
            .WithMany()
            .HasForeignKey(x => x.ItemCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.BuyerGroupId);
        builder.HasIndex(x => x.ItemCategoryId);
        builder.HasIndex(x => new { x.BuyerGroupId, x.ItemCategoryId }).IsUnique();
    }

    private static void ConfigureSalCustomerCategory(EntityTypeBuilder<SalCustomerCategory> builder)
    {
        builder.ToTable("sal_customer_categories", t =>
        {
            t.HasCheckConstraint("ck_sal_customer_categories_default_payment_terms_non_negative", "default_payment_terms >= 0");
            t.HasCheckConstraint("ck_sal_customer_categories_default_credit_limit_non_negative", "default_credit_limit >= 0");
        });
        ConfigureAuditEntity(builder);

        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DefaultPaymentTerms).HasDefaultValue(2).IsRequired();
        builder.Property(x => x.DefaultCreditLimit).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasOne(x => x.DefaultPriceList)
            .WithMany(x => x.CustomerCategories)
            .HasForeignKey(x => x.DefaultPriceListId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.DefaultPriceListId);
        builder.HasIndex(x => x.IsActive);
    }

    private static void ConfigureSalPriceList(EntityTypeBuilder<SalPriceList> builder)
    {
        builder.ToTable("sal_price_lists", t =>
        {
            t.HasCheckConstraint("ck_sal_price_lists_valid_range", "valid_to IS NULL OR valid_to >= valid_from");
        });
        ConfigureAuditEntity(builder);

        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Type).HasConversion<int>().HasDefaultValue(PriceListType.Standard).IsRequired();
        builder.Property(x => x.CurrencyCode).HasMaxLength(10).HasDefaultValue("IDR").IsRequired();
        builder.Property(x => x.ValidFrom).HasColumnType("date").IsRequired();
        builder.Property(x => x.ValidTo).HasColumnType("date");
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();
        builder.Property(x => x.Notes).HasColumnType("text");

        builder.HasOne(x => x.Currency)
            .WithMany()
            .HasForeignKey(x => x.CurrencyCode)
            .HasPrincipalKey(x => x.Code)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.CurrencyCode);
        builder.HasIndex(x => x.ValidFrom);
        builder.HasIndex(x => x.ValidTo);
        builder.HasIndex(x => x.IsActive);
    }

    private static void ConfigureSalPriceListItem(EntityTypeBuilder<SalPriceListItem> builder)
    {
        builder.ToTable("sal_price_list_items", t =>
        {
            t.HasCheckConstraint("ck_sal_price_list_items_min_qty_positive", "min_qty > 0");
            t.HasCheckConstraint("ck_sal_price_list_items_unit_price_non_negative", "unit_price >= 0");
            t.HasCheckConstraint("ck_sal_price_list_items_discount_range", "discount_pct >= 0 AND discount_pct <= 100");
        });
        ConfigureAuditEntity(builder);

        builder.Property(x => x.MinQty).HasColumnType("numeric(18,4)").HasDefaultValue(1m).IsRequired();
        builder.Property(x => x.UnitPrice).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.DiscountPct).HasColumnType("numeric(5,2)").HasDefaultValue(0m).IsRequired();

        builder.HasOne(x => x.PriceList)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.PriceListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Item)
            .WithMany()
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Uom)
            .WithMany()
            .HasForeignKey(x => x.UomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.PriceListId);
        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => x.UomId);
        builder.HasIndex(x => new { x.PriceListId, x.ItemId, x.UomId, x.MinQty }).IsUnique();
    }

    private static void ConfigureSalApprovalConfig(EntityTypeBuilder<SalApprovalConfig> builder)
    {
        builder.ToTable("sal_approval_configs", t =>
        {
            t.HasCheckConstraint("ck_sal_approval_configs_level_positive", "level > 0");
            t.HasCheckConstraint("ck_sal_approval_configs_min_amount_non_negative", "min_amount >= 0");
            t.HasCheckConstraint("ck_sal_approval_configs_max_amount_non_negative", "max_amount IS NULL OR max_amount >= 0");
            t.HasCheckConstraint("ck_sal_approval_configs_amount_range", "max_amount IS NULL OR max_amount >= min_amount");
            t.HasCheckConstraint("ck_sal_approval_configs_max_discount_pct_range", "max_discount_pct IS NULL OR (max_discount_pct >= 0 AND max_discount_pct <= 100)");
            t.HasCheckConstraint("ck_sal_approval_configs_timeout_hours_positive", "timeout_hours > 0");
            t.HasCheckConstraint("ck_sal_approval_configs_has_approver", "approver_role_id IS NOT NULL OR approver_employee_id IS NOT NULL");
        });
        ConfigureAuditEntity(builder);

        builder.Property(x => x.DocumentType).HasConversion<int>().IsRequired();
        builder.Property(x => x.Level).IsRequired();
        builder.Property(x => x.MinAmount).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.MaxAmount).HasColumnType("numeric(18,4)");
        builder.Property(x => x.MaxDiscountPct).HasColumnType("numeric(5,2)");
        builder.Property(x => x.TimeoutHours).HasDefaultValue(48).IsRequired();
        builder.Property(x => x.AutoApproveIfTimeout).HasDefaultValue(false).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasOne(x => x.ApproverRole)
            .WithMany()
            .HasForeignKey(x => x.ApproverRoleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.ApproverEmployee)
            .WithMany()
            .HasForeignKey(x => x.ApproverEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.DocumentType);
        builder.HasIndex(x => x.Level);
        builder.HasIndex(x => x.ApproverRoleId);
        builder.HasIndex(x => x.ApproverEmployeeId);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => new { x.DocumentType, x.Level }).IsUnique();
    }

    private static void ConfigureSalSalesTeam(EntityTypeBuilder<SalSalesTeam> builder)
    {
        builder.ToTable("sal_sales_teams");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasOne(x => x.TeamLeader)
            .WithMany()
            .HasForeignKey(x => x.TeamLeaderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.TeamLeaderId);
        builder.HasIndex(x => x.IsActive);
    }

    private static void ConfigureSalSalesTeamMember(EntityTypeBuilder<SalSalesTeamMember> builder)
    {
        builder.ToTable("sal_sales_team_members");
        ConfigureAuditEntity(builder);

        builder.HasOne(x => x.SalesTeam)
            .WithMany(x => x.Members)
            .HasForeignKey(x => x.SalesTeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.SalesTeamId);
        builder.HasIndex(x => x.EmployeeId);
        builder.HasIndex(x => new { x.SalesTeamId, x.EmployeeId }).IsUnique();
    }

    private static void ConfigureMfgWorkCenter(EntityTypeBuilder<MfgWorkCenter> builder)
    {
        builder.ToTable("mfg_work_centers", t =>
        {
            t.HasCheckConstraint("ck_mfg_work_centers_capacity_hours_positive", "capacity_hours_per_day > 0");
            t.HasCheckConstraint("ck_mfg_work_centers_labor_cost_non_negative", "labor_cost_per_hour >= 0");
            t.HasCheckConstraint("ck_mfg_work_centers_overhead_cost_non_negative", "overhead_cost_per_hour >= 0");
        });
        ConfigureAuditEntity(builder);
        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CapacityHoursPerDay).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(x => x.LaborCostPerHour).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.OverheadCostPerHour).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();
        builder.Property(x => x.Notes).HasColumnType("text");
        builder.HasOne(x => x.WipAccount)
            .WithMany()
            .HasForeignKey(x => x.WipAccountId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.WipAccountId);
        builder.HasIndex(x => x.IsActive);
    }
    private static void ConfigureMfgRouting(EntityTypeBuilder<MfgRouting> builder)
    {
        builder.ToTable("mfg_routings", t =>
        {
            t.HasCheckConstraint("ck_mfg_routings_version_positive", "version > 0");
            t.HasCheckConstraint("ck_mfg_routings_lead_time_non_negative", "total_lead_time_hours >= 0");
        });
        ConfigureAuditEntity(builder);
        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Version).HasDefaultValue(1).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().HasDefaultValue(RoutingStatus.Draft).IsRequired();
        builder.Property(x => x.TotalLeadTimeHours).HasColumnType("numeric(18,2)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();
        builder.Property(x => x.Notes).HasColumnType("text");
        builder.HasOne(x => x.Item)
            .WithMany()
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.WorkCenter)
            .WithMany(x => x.Routings)
            .HasForeignKey(x => x.WorkCenterId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => x.WorkCenterId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.IsActive);
    }
    private static void ConfigureMfgBom(EntityTypeBuilder<MfgBom> builder)
    {
        builder.ToTable("mfg_boms", t =>
        {
            t.HasCheckConstraint("ck_mfg_boms_version_positive", "version > 0");
            t.HasCheckConstraint("ck_mfg_boms_qty_produced_positive", "qty_produced > 0");
            t.HasCheckConstraint("ck_mfg_boms_standard_cost_non_negative", "standard_cost >= 0");
        });
        ConfigureAuditEntity(builder);
        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Version).HasDefaultValue(1).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().HasDefaultValue(BomStatus.Draft).IsRequired();
        builder.Property(x => x.QtyProduced).HasColumnType("numeric(18,4)").HasDefaultValue(1m).IsRequired();
        builder.Property(x => x.StandardCost).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.EffectiveDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();
        builder.Property(x => x.Notes).HasColumnType("text");
        builder.HasOne(x => x.Item)
            .WithMany()
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Routing)
            .WithMany(x => x.Boms)
            .HasForeignKey(x => x.RoutingId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => x.RoutingId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.EffectiveDate);
        builder.HasIndex(x => x.IsActive);
    }
    private static void ConfigureMfgMrpRun(EntityTypeBuilder<MfgMrpRun> builder)
    {
        builder.ToTable("mfg_mrp_runs", t =>
        {
            t.HasCheckConstraint("ck_mfg_mrp_runs_horizon_days_positive", "horizon_days > 0");
            t.HasCheckConstraint("ck_mfg_mrp_runs_total_demand_items_non_negative", "total_demand_items >= 0");
            t.HasCheckConstraint("ck_mfg_mrp_runs_recommended_wo_non_negative", "recommended_wo_count >= 0");
            t.HasCheckConstraint("ck_mfg_mrp_runs_recommended_pr_non_negative", "recommended_pr_count >= 0");
            t.HasCheckConstraint("ck_mfg_mrp_runs_completed_after_started", "completed_at IS NULL OR started_at IS NULL OR completed_at >= started_at");
        });
        ConfigureAuditEntity(builder);
        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
        builder.Property(x => x.RunDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().HasDefaultValue(MrpStatus.Draft).IsRequired();
        builder.Property(x => x.HorizonDays).HasDefaultValue(30).IsRequired();
        builder.Property(x => x.TotalDemandItems).HasDefaultValue(0).IsRequired();
        builder.Property(x => x.RecommendedWoCount).HasDefaultValue(0).IsRequired();
        builder.Property(x => x.RecommendedPrCount).HasDefaultValue(0).IsRequired();
        builder.Property(x => x.StartedAt).HasColumnType("timestamptz");
        builder.Property(x => x.CompletedAt).HasColumnType("timestamptz");
        builder.Property(x => x.Notes).HasColumnType("text");
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.RunDate);
        builder.HasIndex(x => x.Status);
    }
    private static void ConfigureMfgWorkOrder(EntityTypeBuilder<MfgWorkOrder> builder)
    {
        builder.ToTable("mfg_work_orders", t =>
        {
            t.HasCheckConstraint("ck_mfg_work_orders_qty_planned_positive", "qty_planned > 0");
            t.HasCheckConstraint("ck_mfg_work_orders_qty_good_non_negative", "qty_good >= 0");
            t.HasCheckConstraint("ck_mfg_work_orders_qty_scrap_non_negative", "qty_scrap >= 0");
            t.HasCheckConstraint("ck_mfg_work_orders_plan_date_range", "planned_end_date >= planned_start_date");
            t.HasCheckConstraint("ck_mfg_work_orders_standard_cost_non_negative", "standard_cost_total >= 0");
            t.HasCheckConstraint("ck_mfg_work_orders_actual_cost_non_negative", "actual_cost_total >= 0");
        });
        ConfigureAuditEntity(builder);
        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().HasDefaultValue(WorkOrderStatus.Draft).IsRequired();
        builder.Property(x => x.ProductionType).HasConversion<int>().HasDefaultValue(ProductionType.MakeToStock).IsRequired();
        builder.Property(x => x.QtyPlanned).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.QtyGood).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.QtyScrap).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.PlannedStartDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.PlannedEndDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.ActualStartAt).HasColumnType("timestamptz");
        builder.Property(x => x.ActualEndAt).HasColumnType("timestamptz");
        builder.Property(x => x.StandardCostTotal).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.ActualCostTotal).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();
        builder.Property(x => x.Notes).HasColumnType("text");
        builder.HasOne(x => x.Item)
            .WithMany()
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Bom)
            .WithMany(x => x.WorkOrders)
            .HasForeignKey(x => x.BomId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Routing)
            .WithMany(x => x.WorkOrders)
            .HasForeignKey(x => x.RoutingId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.WorkCenter)
            .WithMany(x => x.WorkOrders)
            .HasForeignKey(x => x.WorkCenterId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.MrpRun)
            .WithMany(x => x.WorkOrders)
            .HasForeignKey(x => x.MrpRunId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => x.BomId);
        builder.HasIndex(x => x.RoutingId);
        builder.HasIndex(x => x.WorkCenterId);
        builder.HasIndex(x => x.MrpRunId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.PlannedStartDate);
        builder.HasIndex(x => x.IsActive);
    }
    private static void ConfigureMfgQcParameter(EntityTypeBuilder<MfgQcParameter> builder)
    {
        builder.ToTable("mfg_qc_parameters", t =>
        {
            t.HasCheckConstraint("ck_mfg_qc_parameters_min_max", "min_value IS NULL OR max_value IS NULL OR min_value <= max_value");
        });
        ConfigureAuditEntity(builder);
        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ParameterType).HasConversion<int>().HasDefaultValue(QcParameterType.Numeric).IsRequired();
        builder.Property(x => x.MinValue).HasColumnType("numeric(18,4)");
        builder.Property(x => x.MaxValue).HasColumnType("numeric(18,4)");
        builder.Property(x => x.IsCritical).HasDefaultValue(false).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();
        builder.Property(x => x.Notes).HasColumnType("text");
        builder.HasOne(x => x.Item)
            .WithMany()
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => x.ParameterType);
        builder.HasIndex(x => x.IsCritical);
        builder.HasIndex(x => x.IsActive);
    }
    private static void ConfigureMfgQcInspection(EntityTypeBuilder<MfgQcInspection> builder)
    {
        builder.ToTable("mfg_qc_inspections");
        ConfigureAuditEntity(builder);
        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
        builder.Property(x => x.InspectedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().HasDefaultValue(QcStatus.Pending).IsRequired();
        builder.Property(x => x.Result).HasConversion<int>().HasDefaultValue(QcResult.Pass).IsRequired();
        builder.Property(x => x.Notes).HasColumnType("text");
        builder.HasOne(x => x.WorkOrder)
            .WithMany(x => x.QcInspections)
            .HasForeignKey(x => x.WorkOrderId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Item)
            .WithMany()
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.InspectorEmployee)
            .WithMany()
            .HasForeignKey(x => x.InspectorEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.WorkOrderId);
        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => x.InspectorEmployeeId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Result);
        builder.HasIndex(x => x.InspectedAt);
    }
    private static void ConfigureMfgScrapRecord(EntityTypeBuilder<MfgScrapRecord> builder)
    {
        builder.ToTable("mfg_scrap_records", t =>
        {
            t.HasCheckConstraint("ck_mfg_scrap_records_qty_positive", "qty_scrap > 0");
            t.HasCheckConstraint("ck_mfg_scrap_records_unit_cost_non_negative", "unit_cost >= 0");
            t.HasCheckConstraint("ck_mfg_scrap_records_total_cost_non_negative", "total_scrap_cost >= 0");
        });
        ConfigureAuditEntity(builder);
        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Reason).HasConversion<int>().HasDefaultValue(ScrapReason.Other).IsRequired();
        builder.Property(x => x.QtyScrap).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.UnitCost).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.TotalScrapCost).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.RecordedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.Notes).HasColumnType("text");
        builder.HasOne(x => x.WorkOrder)
            .WithMany(x => x.ScrapRecords)
            .HasForeignKey(x => x.WorkOrderId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Item)
            .WithMany()
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.WorkCenter)
            .WithMany(x => x.ScrapRecords)
            .HasForeignKey(x => x.WorkCenterId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.WorkOrderId);
        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => x.WorkCenterId);
        builder.HasIndex(x => x.Reason);
        builder.HasIndex(x => x.RecordedAt);
    }
    private static void ConfigureMfgReworkOrder(EntityTypeBuilder<MfgReworkOrder> builder)
    {
        builder.ToTable("mfg_rework_orders", t =>
        {
            t.HasCheckConstraint("ck_mfg_rework_orders_qty_positive", "qty_rework > 0");
            t.HasCheckConstraint("ck_mfg_rework_orders_closed_after_opened", "closed_at IS NULL OR closed_at >= opened_at");
        });
        ConfigureAuditEntity(builder);
        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
        builder.Property(x => x.QtyRework).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().HasDefaultValue(WorkOrderStatus.Draft).IsRequired();
        builder.Property(x => x.OpenedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.ClosedAt).HasColumnType("timestamptz");
        builder.Property(x => x.Notes).HasColumnType("text");
        builder.HasOne(x => x.SourceWorkOrder)
            .WithMany()
            .HasForeignKey(x => x.SourceWorkOrderId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.WorkOrder)
            .WithMany()
            .HasForeignKey(x => x.WorkOrderId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Item)
            .WithMany()
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.SourceWorkOrderId);
        builder.HasIndex(x => x.WorkOrderId);
        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.OpenedAt);
        builder.HasIndex(x => x.ClosedAt);
    }
    private static void ConfigureMfgOeeSnapshot(EntityTypeBuilder<MfgOeeSnapshot> builder)
    {
        builder.ToTable("mfg_oee_snapshots", t =>
        {
            t.HasCheckConstraint("ck_mfg_oee_snapshots_availability_range", "availability_pct >= 0 AND availability_pct <= 100");
            t.HasCheckConstraint("ck_mfg_oee_snapshots_performance_range", "performance_pct >= 0 AND performance_pct <= 100");
            t.HasCheckConstraint("ck_mfg_oee_snapshots_quality_range", "quality_pct >= 0 AND quality_pct <= 100");
            t.HasCheckConstraint("ck_mfg_oee_snapshots_oee_range", "oee_pct >= 0 AND oee_pct <= 100");
        });
        ConfigureAuditEntity(builder);
        builder.Property(x => x.SnapshotDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.AvailabilityPct).HasColumnType("numeric(5,2)").IsRequired();
        builder.Property(x => x.PerformancePct).HasColumnType("numeric(5,2)").IsRequired();
        builder.Property(x => x.QualityPct).HasColumnType("numeric(5,2)").IsRequired();
        builder.Property(x => x.OeePct).HasColumnType("numeric(5,2)").IsRequired();
        builder.HasOne(x => x.WorkCenter)
            .WithMany(x => x.OeeSnapshots)
            .HasForeignKey(x => x.WorkCenterId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.WorkCenterId);
        builder.HasIndex(x => x.SnapshotDate);
        builder.HasIndex(x => new { x.WorkCenterId, x.SnapshotDate }).IsUnique();
    }

    private static void ConfigureFaAssetCategory(EntityTypeBuilder<FaAssetCategory> builder)
    {
        builder.ToTable("fa_asset_categories", t =>
        {
            t.HasCheckConstraint("ck_fa_asset_categories_useful_life_positive", "useful_life_months > 0");
            t.HasCheckConstraint("ck_fa_asset_categories_depreciation_rate_range", "depreciation_rate IS NULL OR (depreciation_rate > 0 AND depreciation_rate <= 100)");
        });
        ConfigureAuditEntity(builder);

        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DepreciationMethod).HasConversion<int>().IsRequired();
        builder.Property(x => x.UsefulLifeMonths).IsRequired();
        builder.Property(x => x.DepreciationRate).HasColumnType("numeric(7,4)");
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasOne(x => x.AssetAccount)
            .WithMany()
            .HasForeignKey(x => x.AssetAccountId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.AccumulatedDepreciationAccount)
            .WithMany()
            .HasForeignKey(x => x.AccumulatedDepreciationAccountId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.DepreciationExpenseAccount)
            .WithMany()
            .HasForeignKey(x => x.DepreciationExpenseAccountId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.DepreciationMethod);
        builder.HasIndex(x => x.IsActive);
    }

    private static void ConfigureFaLocation(EntityTypeBuilder<FaLocation> builder)
    {
        builder.ToTable("fa_locations");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Manager)
            .WithMany()
            .HasForeignKey(x => x.ManagerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.DepartmentId);
        builder.HasIndex(x => x.IsActive);
    }

    private static void ConfigureFaDepreciationConfig(EntityTypeBuilder<FaDepreciationConfig> builder)
    {
        builder.ToTable("fa_depreciation_configs", t =>
        {
            t.HasCheckConstraint("ck_fa_depreciation_configs_period", "start_date <= end_date");
            t.HasCheckConstraint("ck_fa_depreciation_configs_run_day_range", "run_day >= 1 AND run_day <= 31");
        });
        ConfigureAuditEntity(builder);

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.FiscalYear).IsRequired();
        builder.Property(x => x.StartDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.EndDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.RunDay).HasDefaultValue((byte)28).IsRequired();
        builder.Property(x => x.IsAutoPostJournal).HasDefaultValue(false).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasIndex(x => x.FiscalYear);
        builder.HasIndex(x => x.IsActive);
    }

    private static void ConfigureFaAsset(EntityTypeBuilder<FaAsset> builder)
    {
        builder.ToTable("fa_assets", t =>
        {
            t.HasCheckConstraint("ck_fa_assets_acquisition_cost_non_negative", "acquisition_cost >= 0");
            t.HasCheckConstraint("ck_fa_assets_salvage_value_non_negative", "salvage_value >= 0");
            t.HasCheckConstraint("ck_fa_assets_salvage_not_exceed_cost", "salvage_value <= acquisition_cost");
            t.HasCheckConstraint("ck_fa_assets_useful_life_positive", "useful_life_months > 0");
            t.HasCheckConstraint("ck_fa_assets_depreciation_rate_range", "depreciation_rate IS NULL OR (depreciation_rate > 0 AND depreciation_rate <= 100)");
            t.HasCheckConstraint("ck_fa_assets_accumulated_depreciation_non_negative", "accumulated_depreciation >= 0");
            t.HasCheckConstraint("ck_fa_assets_book_value_non_negative", "book_value >= 0");
        });
        ConfigureAuditEntity(builder);

        builder.Property(x => x.AssetCode).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.AcquisitionDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.InServiceDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.AcquisitionCost).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(x => x.SalvageValue).HasColumnType("numeric(18,2)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.UsefulLifeMonths).IsRequired();
        builder.Property(x => x.DepreciationMethod).HasConversion<int>().IsRequired();
        builder.Property(x => x.DepreciationRate).HasColumnType("numeric(7,4)");
        builder.Property(x => x.AccumulatedDepreciation).HasColumnType("numeric(18,2)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.BookValue).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.SerialNumber).HasMaxLength(100);
        builder.Property(x => x.VendorName).HasMaxLength(200);
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Assets)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Location)
            .WithMany(x => x.Assets)
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.AssetCode).IsUnique();
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.LocationId);
        builder.HasIndex(x => x.DepartmentId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.AcquisitionDate);
        builder.HasIndex(x => x.InServiceDate);
    }

    private static void ConfigureFaAssetDocument(EntityTypeBuilder<FaAssetDocument> builder)
    {
        builder.ToTable("fa_asset_documents");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.DocumentType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.FilePath).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Notes).HasColumnType("text");

        builder.HasOne(x => x.Asset)
            .WithMany(x => x.Documents)
            .HasForeignKey(x => x.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.AssetId);
        builder.HasIndex(x => x.DocumentType);
    }

    private static void ConfigureFaDepreciationRun(EntityTypeBuilder<FaDepreciationRun> builder)
    {
        builder.ToTable("fa_depreciation_runs", t =>
        {
            t.HasCheckConstraint("ck_fa_depreciation_runs_period_month", "period_month >= 1 AND period_month <= 12");
            t.HasCheckConstraint("ck_fa_depreciation_runs_total_asset_count", "total_asset_count >= 0");
            t.HasCheckConstraint("ck_fa_depreciation_runs_total_depreciation_non_negative", "total_depreciation_amount >= 0");
        });
        ConfigureAuditEntity(builder);

        builder.Property(x => x.RunNo).HasMaxLength(30).IsRequired();
        builder.Property(x => x.PeriodYear).IsRequired();
        builder.Property(x => x.PeriodMonth).IsRequired();
        builder.Property(x => x.RunDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.TotalAssetCount).IsRequired();
        builder.Property(x => x.TotalDepreciationAmount).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();

        builder.HasOne(x => x.ApprovedByUser)
            .WithMany()
            .HasForeignKey(x => x.ApprovedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.JournalEntry)
            .WithMany()
            .HasForeignKey(x => x.JournalEntryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.RunNo).IsUnique();
        builder.HasIndex(x => new { x.PeriodYear, x.PeriodMonth });
        builder.HasIndex(x => x.Status);
    }

    private static void ConfigureFaDepreciationSchedule(EntityTypeBuilder<FaDepreciationSchedule> builder)
    {
        builder.ToTable("fa_depreciation_schedules", t =>
        {
            t.HasCheckConstraint("ck_fa_depreciation_schedules_period_month", "period_month >= 1 AND period_month <= 12");
            t.HasCheckConstraint("ck_fa_depreciation_schedules_amount_non_negative", "depreciation_amount >= 0");
            t.HasCheckConstraint("ck_fa_depreciation_schedules_accumulated_non_negative", "accumulated_depreciation >= 0");
            t.HasCheckConstraint("ck_fa_depreciation_schedules_book_value_non_negative", "book_value >= 0");
        });
        ConfigureAuditEntity(builder);

        builder.Property(x => x.PeriodYear).IsRequired();
        builder.Property(x => x.PeriodMonth).IsRequired();
        builder.Property(x => x.DepreciationDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.DepreciationAmount).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(x => x.AccumulatedDepreciation).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(x => x.BookValue).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();

        builder.HasOne(x => x.Asset)
            .WithMany(x => x.DepreciationSchedules)
            .HasForeignKey(x => x.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Run)
            .WithMany(x => x.Schedules)
            .HasForeignKey(x => x.RunId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.AssetId, x.PeriodYear, x.PeriodMonth }).IsUnique();
        builder.HasIndex(x => x.RunId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.DepreciationDate);
    }

    private static void ConfigureFaAssetTransfer(EntityTypeBuilder<FaAssetTransfer> builder)
    {
        builder.ToTable("fa_asset_transfers", t =>
        {
            t.HasCheckConstraint("ck_fa_asset_transfers_locations_not_same", "from_location_id <> to_location_id");
        });
        ConfigureAuditEntity(builder);

        builder.Property(x => x.TransferNo).HasMaxLength(30).IsRequired();
        builder.Property(x => x.TransferDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.Reason).HasColumnType("text");
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();

        builder.HasOne(x => x.Asset)
            .WithMany(x => x.Transfers)
            .HasForeignKey(x => x.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FromLocation)
            .WithMany()
            .HasForeignKey(x => x.FromLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ToLocation)
            .WithMany()
            .HasForeignKey(x => x.ToLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FromDepartment)
            .WithMany()
            .HasForeignKey(x => x.FromDepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.ToDepartment)
            .WithMany()
            .HasForeignKey(x => x.ToDepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.ApprovedByEmployee)
            .WithMany()
            .HasForeignKey(x => x.ApprovedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.TransferNo).IsUnique();
        builder.HasIndex(x => x.AssetId);
        builder.HasIndex(x => x.TransferDate);
        builder.HasIndex(x => x.Status);
    }

    private static void ConfigureFaMaintenanceOrder(EntityTypeBuilder<FaMaintenanceOrder> builder)
    {
        builder.ToTable("fa_maintenance_orders", t =>
        {
            t.HasCheckConstraint("ck_fa_maintenance_orders_cost_non_negative", "cost >= 0");
        });
        ConfigureAuditEntity(builder);

        builder.Property(x => x.WorkOrderNo).HasMaxLength(30).IsRequired();
        builder.Property(x => x.OrderDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.MaintenanceType).HasConversion<int>().IsRequired();
        builder.Property(x => x.VendorName).HasMaxLength(200);
        builder.Property(x => x.Cost).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.Notes).HasColumnType("text");

        builder.HasOne(x => x.Asset)
            .WithMany(x => x.MaintenanceOrders)
            .HasForeignKey(x => x.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.WorkOrderNo).IsUnique();
        builder.HasIndex(x => x.AssetId);
        builder.HasIndex(x => x.OrderDate);
        builder.HasIndex(x => x.Status);
    }

    private static void ConfigureFaDisposal(EntityTypeBuilder<FaDisposal> builder)
    {
        builder.ToTable("fa_disposals", t =>
        {
            t.HasCheckConstraint("ck_fa_disposals_sale_amount_non_negative", "sale_amount IS NULL OR sale_amount >= 0");
            t.HasCheckConstraint("ck_fa_disposals_expense_non_negative", "disposal_expense >= 0");
        });
        ConfigureAuditEntity(builder);

        builder.Property(x => x.DisposalNo).HasMaxLength(30).IsRequired();
        builder.Property(x => x.DisposalDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.DisposalType).HasConversion<int>().IsRequired();
        builder.Property(x => x.SaleAmount).HasColumnType("numeric(18,2)");
        builder.Property(x => x.DisposalExpense).HasColumnType("numeric(18,2)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.GainLossAmount).HasColumnType("numeric(18,2)");
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.Notes).HasColumnType("text");

        builder.HasOne(x => x.Asset)
            .WithMany(x => x.Disposals)
            .HasForeignKey(x => x.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.DisposalNo).IsUnique();
        builder.HasIndex(x => x.AssetId);
        builder.HasIndex(x => x.DisposalDate);
        builder.HasIndex(x => x.Status);
    }

    private static void ConfigureFaRevaluation(EntityTypeBuilder<FaRevaluation> builder)
    {
        builder.ToTable("fa_revaluations", t =>
        {
            t.HasCheckConstraint("ck_fa_revaluations_values_non_negative", "old_book_value >= 0 AND new_book_value >= 0 AND impairment_amount >= 0");
        });
        ConfigureAuditEntity(builder);

        builder.Property(x => x.RevaluationNo).HasMaxLength(30).IsRequired();
        builder.Property(x => x.RevaluationDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.OldBookValue).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(x => x.NewBookValue).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(x => x.ImpairmentAmount).HasColumnType("numeric(18,2)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.Notes).HasColumnType("text");

        builder.HasOne(x => x.Asset)
            .WithMany(x => x.Revaluations)
            .HasForeignKey(x => x.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.RevaluationNo).IsUnique();
        builder.HasIndex(x => x.AssetId);
        builder.HasIndex(x => x.RevaluationDate);
        builder.HasIndex(x => x.Status);
    }

    private static void ConfigureFaAssetHistory(EntityTypeBuilder<FaAssetHistory> builder)
    {
        builder.ToTable("fa_asset_histories");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.EventDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.EventType).HasConversion<int>().IsRequired();
        builder.Property(x => x.ReferenceNo).HasMaxLength(50);
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.AmountChange).HasColumnType("numeric(18,2)");

        builder.HasOne(x => x.Asset)
            .WithMany(x => x.Histories)
            .HasForeignKey(x => x.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.AssetId);
        builder.HasIndex(x => x.EventDate);
        builder.HasIndex(x => x.EventType);
    }

    private static void ConfigureFinApInvoice(EntityTypeBuilder<FinApInvoice> builder)
    {
        builder.ToTable("fin_ap_invoices", t =>
        {
            t.HasCheckConstraint("ck_fin_ap_invoices_non_negative", "subtotal >= 0 AND tax_amount >= 0 AND total_amount >= 0 AND paid_amount >= 0 AND outstanding_amount >= 0");
        });
        ConfigureAuditEntity(builder);

        builder.Property(x => x.InvoiceNo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.VendorInvoiceNo).HasMaxLength(100);
        builder.Property(x => x.InvoiceDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.DueDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.Subtotal).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.TaxAmount).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.TotalAmount).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.PaidAmount).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.OutstandingAmount).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.CurrencyCode).HasMaxLength(10).HasDefaultValue("IDR").IsRequired();
        builder.Property(x => x.ExchangeRate).HasColumnType("numeric(18,6)").HasDefaultValue(1m).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().HasDefaultValue(FinanceApInvoiceStatus.Draft).IsRequired();
        builder.Property(x => x.ApprovedAt).HasColumnType("timestamptz");

        builder.HasOne(x => x.Vendor)
            .WithMany(x => x.ApInvoices)
            .HasForeignKey(x => x.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Period)
            .WithMany(x => x.ApInvoices)
            .HasForeignKey(x => x.PeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Currency)
            .WithMany(x => x.ApInvoices)
            .HasForeignKey(x => x.CurrencyCode)
            .HasPrincipalKey(x => x.Code)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ApprovedByUser)
            .WithMany()
            .HasForeignKey(x => x.ApprovedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.JournalEntry)
            .WithMany()
            .HasForeignKey(x => x.JournalEntryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.InvoiceNo).IsUnique();
        builder.HasIndex(x => x.VendorInvoiceNo);
        builder.HasIndex(x => x.VendorId);
        builder.HasIndex(x => x.PeriodId);
        builder.HasIndex(x => x.InvoiceDate);
        builder.HasIndex(x => x.DueDate);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.OutstandingAmount);
        builder.HasIndex(x => x.CurrencyCode);
        builder.HasIndex(x => x.ApprovedBy);
        builder.HasIndex(x => x.JournalEntryId);
    }

    private static void ConfigureFinApInvoiceLine(EntityTypeBuilder<FinApInvoiceLine> builder)
    {
        builder.ToTable("fin_ap_invoice_lines", t =>
        {
            t.HasCheckConstraint("ck_fin_ap_invoice_lines_positive_qty", "quantity > 0");
            t.HasCheckConstraint("ck_fin_ap_invoice_lines_non_negative", "unit_price >= 0 AND amount >= 0 AND tax_amount >= 0");
        });

        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.LineNo).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Quantity).HasColumnType("numeric(18,4)").HasDefaultValue(1m).IsRequired();
        builder.Property(x => x.UnitPrice).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.Amount).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.TaxAmount).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();

        builder.HasOne(x => x.Invoice)
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.TaxCode)
            .WithMany(x => x.ApInvoiceLines)
            .HasForeignKey(x => x.TaxCodeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Account)
            .WithMany(x => x.ApInvoiceLines)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CostCenter)
            .WithMany(x => x.ApInvoiceLines)
            .HasForeignKey(x => x.CostCenterId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.InvoiceId);
        builder.HasIndex(x => new { x.InvoiceId, x.LineNo }).IsUnique();
        builder.HasIndex(x => x.AccountId);
        builder.HasIndex(x => x.TaxCodeId);
        builder.HasIndex(x => x.CostCenterId);
    }

    private static void ConfigureFinApPayment(EntityTypeBuilder<FinApPayment> builder)
    {
        builder.ToTable("fin_ap_payments", t =>
        {
            t.HasCheckConstraint("ck_fin_ap_payments_positive_amount", "amount > 0");
        });
        ConfigureAuditEntity(builder);

        builder.Property(x => x.PaymentNo).HasMaxLength(30).IsRequired();
        builder.Property(x => x.PaymentDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.Amount).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.PaymentMethod).HasConversion<int>().IsRequired();
        builder.Property(x => x.ReferenceNo).HasMaxLength(100);
        builder.Property(x => x.Notes).HasColumnType("text");

        builder.HasOne(x => x.Vendor)
            .WithMany(x => x.ApPayments)
            .HasForeignKey(x => x.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.BankAccount)
            .WithMany(x => x.ApPayments)
            .HasForeignKey(x => x.BankAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.JournalEntry)
            .WithMany()
            .HasForeignKey(x => x.JournalEntryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.PaymentNo).IsUnique();
        builder.HasIndex(x => x.VendorId);
        builder.HasIndex(x => x.PaymentDate);
        builder.HasIndex(x => x.PaymentMethod);
        builder.HasIndex(x => x.BankAccountId);
        builder.HasIndex(x => x.JournalEntryId);
    }

    private static void ConfigureFinApPaymentApplication(EntityTypeBuilder<FinApPaymentApplication> builder)
    {
        builder.ToTable("fin_ap_payment_applications", t =>
        {
            t.HasCheckConstraint("ck_fin_ap_payment_apps_positive", "applied_amount > 0");
        });

        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.AppliedAmount).HasColumnType("numeric(18,4)").IsRequired();

        builder.HasOne(x => x.Payment)
            .WithMany(x => x.Applications)
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Invoice)
            .WithMany(x => x.PaymentApplications)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.PaymentId);
        builder.HasIndex(x => x.InvoiceId);
        builder.HasIndex(x => new { x.PaymentId, x.InvoiceId }).IsUnique();
    }

    private static void ConfigureFinCustomer(EntityTypeBuilder<FinCustomer> builder)
    {
        builder.ToTable("fin_customers", t =>
        {
            t.HasCheckConstraint("ck_fin_customers_non_negative_credit_limit", "credit_limit >= 0");
            t.HasCheckConstraint("ck_fin_customers_non_negative_terms", "payment_terms_days >= 0");
            t.HasCheckConstraint("ck_fin_customers_non_negative_credit_used", "credit_used >= 0");
            t.HasCheckConstraint("ck_fin_customers_non_negative_total_ytd_sales", "total_ytd_sales >= 0");
        });
        ConfigureAuditEntity(builder);

        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TaxId).HasMaxLength(30);
        builder.Property(x => x.Address).HasColumnType("text");
        builder.Property(x => x.Phone).HasMaxLength(30);
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.ContactPerson).HasMaxLength(100);
        builder.Property(x => x.CreditLimit).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.PaymentTermsDays).HasDefaultValue(30).IsRequired();
        builder.Property(x => x.CreditUsed).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.LastOrderDate).HasColumnType("date");
        builder.Property(x => x.TotalYtdSales).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.Notes).HasColumnType("text");
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasOne(x => x.DefaultAccount)
            .WithMany(x => x.CustomerDefaultAccounts)
            .HasForeignKey(x => x.DefaultAccountId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.DefaultTaxCode)
            .WithMany(x => x.CustomerDefaults)
            .HasForeignKey(x => x.DefaultTaxCodeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.CustomerCategory)
            .WithMany(x => x.Customers)
            .HasForeignKey(x => x.CustomerCategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.PriceList)
            .WithMany(x => x.Customers)
            .HasForeignKey(x => x.PriceListId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.SalesEmployee)
            .WithMany()
            .HasForeignKey(x => x.SalesEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.SalesTeam)
            .WithMany(x => x.Customers)
            .HasForeignKey(x => x.SalesTeamId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.DefaultAccountId);
        builder.HasIndex(x => x.DefaultTaxCodeId);
        builder.HasIndex(x => x.CustomerCategoryId);
        builder.HasIndex(x => x.PriceListId);
        builder.HasIndex(x => x.SalesEmployeeId);
        builder.HasIndex(x => x.SalesTeamId);
        builder.HasIndex(x => x.CreditUsed);
        builder.HasIndex(x => x.LastOrderDate);
        builder.HasIndex(x => x.TotalYtdSales);
    }

    private static void ConfigureFinArInvoice(EntityTypeBuilder<FinArInvoice> builder)
    {
        builder.ToTable("fin_ar_invoices", t =>
        {
            t.HasCheckConstraint("ck_fin_ar_invoices_non_negative", "subtotal >= 0 AND tax_amount >= 0 AND total_amount >= 0 AND received_amount >= 0 AND outstanding_amount >= 0");
        });
        ConfigureAuditEntity(builder);

        builder.Property(x => x.InvoiceNo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.InvoiceDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.DueDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.Subtotal).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.TaxAmount).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.TotalAmount).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.ReceivedAmount).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.OutstandingAmount).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.CurrencyCode).HasMaxLength(10).HasDefaultValue("IDR").IsRequired();
        builder.Property(x => x.ExchangeRate).HasColumnType("numeric(18,6)").HasDefaultValue(1m).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().HasDefaultValue(FinanceArInvoiceStatus.Draft).IsRequired();
        builder.Property(x => x.SentAt).HasColumnType("timestamptz");

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.ArInvoices)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Period)
            .WithMany(x => x.ArInvoices)
            .HasForeignKey(x => x.PeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Currency)
            .WithMany(x => x.ArInvoices)
            .HasForeignKey(x => x.CurrencyCode)
            .HasPrincipalKey(x => x.Code)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SentByUser)
            .WithMany()
            .HasForeignKey(x => x.SentBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.JournalEntry)
            .WithMany()
            .HasForeignKey(x => x.JournalEntryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.InvoiceNo).IsUnique();
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.PeriodId);
        builder.HasIndex(x => x.InvoiceDate);
        builder.HasIndex(x => x.DueDate);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.OutstandingAmount);
        builder.HasIndex(x => x.CurrencyCode);
        builder.HasIndex(x => x.SentBy);
        builder.HasIndex(x => x.JournalEntryId);
    }

    private static void ConfigureFinArInvoiceLine(EntityTypeBuilder<FinArInvoiceLine> builder)
    {
        builder.ToTable("fin_ar_invoice_lines", t =>
        {
            t.HasCheckConstraint("ck_fin_ar_invoice_lines_positive_qty", "quantity > 0");
            t.HasCheckConstraint("ck_fin_ar_invoice_lines_non_negative", "unit_price >= 0 AND amount >= 0 AND tax_amount >= 0");
        });

        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.LineNo).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Quantity).HasColumnType("numeric(18,4)").HasDefaultValue(1m).IsRequired();
        builder.Property(x => x.UnitPrice).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.Amount).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.TaxAmount).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();

        builder.HasOne(x => x.Invoice)
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.TaxCode)
            .WithMany(x => x.ArInvoiceLines)
            .HasForeignKey(x => x.TaxCodeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Account)
            .WithMany(x => x.ArInvoiceLines)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CostCenter)
            .WithMany(x => x.ArInvoiceLines)
            .HasForeignKey(x => x.CostCenterId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.InvoiceId);
        builder.HasIndex(x => new { x.InvoiceId, x.LineNo }).IsUnique();
        builder.HasIndex(x => x.AccountId);
        builder.HasIndex(x => x.TaxCodeId);
        builder.HasIndex(x => x.CostCenterId);
    }

    private static void ConfigureFinArReceipt(EntityTypeBuilder<FinArReceipt> builder)
    {
        builder.ToTable("fin_ar_receipts", t =>
        {
            t.HasCheckConstraint("ck_fin_ar_receipts_positive_amount", "amount > 0");
        });
        ConfigureAuditEntity(builder);

        builder.Property(x => x.ReceiptNo).HasMaxLength(30).IsRequired();
        builder.Property(x => x.ReceiptDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.Amount).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.PaymentMethod).HasConversion<int>().IsRequired();
        builder.Property(x => x.ReferenceNo).HasMaxLength(100);
        builder.Property(x => x.Notes).HasColumnType("text");

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.ArReceipts)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.BankAccount)
            .WithMany(x => x.ArReceipts)
            .HasForeignKey(x => x.BankAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.JournalEntry)
            .WithMany()
            .HasForeignKey(x => x.JournalEntryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.ReceiptNo).IsUnique();
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.ReceiptDate);
        builder.HasIndex(x => x.PaymentMethod);
        builder.HasIndex(x => x.BankAccountId);
        builder.HasIndex(x => x.JournalEntryId);
    }

    private static void ConfigureFinArReceiptApplication(EntityTypeBuilder<FinArReceiptApplication> builder)
    {
        builder.ToTable("fin_ar_receipt_applications", t =>
        {
            t.HasCheckConstraint("ck_fin_ar_receipt_apps_positive", "applied_amount > 0");
        });

        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.AppliedAmount).HasColumnType("numeric(18,4)").IsRequired();

        builder.HasOne(x => x.Receipt)
            .WithMany(x => x.Applications)
            .HasForeignKey(x => x.ReceiptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Invoice)
            .WithMany(x => x.ReceiptApplications)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ReceiptId);
        builder.HasIndex(x => x.InvoiceId);
        builder.HasIndex(x => new { x.ReceiptId, x.InvoiceId }).IsUnique();
    }


    private static void ConfigureFinBudget(EntityTypeBuilder<FinBudget> builder)
    {
        builder.ToTable("fin_budgets", t =>
        {
            t.HasCheckConstraint("ck_fin_budgets_non_negative_total", "total_amount >= 0");
        });
        ConfigureAuditEntity(builder);

        builder.Property(x => x.BudgetNo).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CurrencyCode).HasMaxLength(10).HasDefaultValue("IDR").IsRequired();
        builder.Property(x => x.TotalAmount).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.Notes).HasColumnType("text");
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasOne(x => x.FiscalYear)
            .WithMany(x => x.Budgets)
            .HasForeignKey(x => x.FiscalYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Period)
            .WithMany(x => x.Budgets)
            .HasForeignKey(x => x.PeriodId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.CostCenter)
            .WithMany(x => x.Budgets)
            .HasForeignKey(x => x.CostCenterId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Account)
            .WithMany(x => x.Budgets)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Currency)
            .WithMany(x => x.Budgets)
            .HasForeignKey(x => x.CurrencyCode)
            .HasPrincipalKey(x => x.Code)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.BudgetNo).IsUnique();
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.FiscalYearId);
        builder.HasIndex(x => x.PeriodId);
        builder.HasIndex(x => x.CostCenterId);
        builder.HasIndex(x => x.AccountId);
        builder.HasIndex(x => x.CurrencyCode);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.TotalAmount);
    }

    private static void ConfigureFinBudgetLine(EntityTypeBuilder<FinBudgetLine> builder)
    {
        builder.ToTable("fin_budget_lines", t =>
        {
            t.HasCheckConstraint("ck_fin_budget_lines_non_negative_amount", "amount >= 0");
        });

        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.LineNo).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(200);
        builder.Property(x => x.Amount).HasColumnType("numeric(18,4)").IsRequired();

        builder.HasOne(x => x.Budget)
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.BudgetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Period)
            .WithMany(x => x.BudgetLines)
            .HasForeignKey(x => x.PeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Account)
            .WithMany(x => x.BudgetLines)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CostCenter)
            .WithMany(x => x.BudgetLines)
            .HasForeignKey(x => x.CostCenterId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.BudgetId);
        builder.HasIndex(x => new { x.BudgetId, x.LineNo }).IsUnique();
        builder.HasIndex(x => x.PeriodId);
        builder.HasIndex(x => x.AccountId);
        builder.HasIndex(x => x.CostCenterId);
    }


    private static void ConfigureInvItemCategory(EntityTypeBuilder<InvItemCategory> builder)
    {
        builder.ToTable("inv_item_categories");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasOne(x => x.ParentCategory)
            .WithMany(x => x.ChildCategories)
            .HasForeignKey(x => x.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.ParentCategoryId);
        builder.HasIndex(x => x.IsActive);
    }

    private static void ConfigureInvUnitOfMeasure(EntityTypeBuilder<InvUnitOfMeasure> builder)
    {
        builder.ToTable("inv_units_of_measure");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.IsActive);
    }

    private static void ConfigureInvBrand(EntityTypeBuilder<InvBrand> builder)
    {
        builder.ToTable("inv_brands");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasIndex(x => x.IsActive);
    }

    private static void ConfigureInvItem(EntityTypeBuilder<InvItem> builder)
    {
        builder.ToTable("inv_items", t =>
        {
            t.HasCheckConstraint("ck_inv_items_last_purchase_price_non_negative", "last_purchase_price IS NULL OR last_purchase_price >= 0");
            t.HasCheckConstraint("ck_inv_items_avg_cost_non_negative", "avg_cost >= 0");
            t.HasCheckConstraint("ck_inv_items_min_stock_non_negative", "min_stock >= 0");
            t.HasCheckConstraint("ck_inv_items_max_stock_non_negative", "max_stock >= 0");
            t.HasCheckConstraint("ck_inv_items_reorder_point_non_negative", "reorder_point >= 0");
            t.HasCheckConstraint("ck_inv_items_lead_time_days_non_negative", "lead_time_days >= 0");
        });
        ConfigureAuditEntity(builder);

        builder.Property(x => x.ItemCode).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Sku).HasMaxLength(100);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.Type).HasConversion<int>().HasDefaultValue(ItemType.Product).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().HasDefaultValue(ItemStatus.Active).IsRequired();
        builder.Property(x => x.ValuationMethod).HasConversion<int>().HasDefaultValue(ValuationMethod.WeightedAverageCost).IsRequired();
        builder.Property(x => x.LastPurchasePrice).HasColumnType("numeric(18,4)");
        builder.Property(x => x.AvgCost).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.MinStock).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.MaxStock).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.ReorderPoint).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.LeadTimeDays).HasDefaultValue(0).IsRequired();
        builder.Property(x => x.InventoryAccountId).HasColumnName("account_inventory_id");
        builder.Property(x => x.CogsAccountId).HasColumnName("account_cogs_id");
        builder.Property(x => x.AdjustmentAccountId).HasColumnName("account_adjustment_id");
        builder.Property(x => x.Notes).HasColumnType("text");
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Brand)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.BrandId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.BaseUom)
            .WithMany(x => x.BaseItems)
            .HasForeignKey(x => x.BaseUomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PurchaseUom)
            .WithMany(x => x.PurchaseItems)
            .HasForeignKey(x => x.PurchaseUomId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.InventoryAccount)
            .WithMany()
            .HasForeignKey(x => x.InventoryAccountId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.CogsAccount)
            .WithMany()
            .HasForeignKey(x => x.CogsAccountId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.AdjustmentAccount)
            .WithMany()
            .HasForeignKey(x => x.AdjustmentAccountId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.ItemCode).IsUnique();
        builder.HasIndex(x => x.Sku).IsUnique();
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.BrandId);
        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.BaseUomId);
        builder.HasIndex(x => x.PurchaseUomId);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.MinStock);
        builder.HasIndex(x => x.ReorderPoint);
    }

    private static void ConfigureInvItemUnitConversion(EntityTypeBuilder<InvItemUnitConversion> builder)
    {
        builder.ToTable("inv_item_unit_conversions", t =>
        {
            t.HasCheckConstraint("ck_inv_item_unit_conversions_factor_positive", "conversion_factor > 0");
        });
        ConfigureAuditEntity(builder);

        builder.Property(x => x.ConversionFactor).HasColumnType("numeric(18,6)").IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasOne(x => x.Item)
            .WithMany(x => x.UnitConversions)
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.FromUom)
            .WithMany(x => x.FromConversions)
            .HasForeignKey(x => x.FromUomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ToUom)
            .WithMany(x => x.ToConversions)
            .HasForeignKey(x => x.ToUomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => x.FromUomId);
        builder.HasIndex(x => x.ToUomId);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => new { x.ItemId, x.FromUomId, x.ToUomId }).IsUnique();
    }

    private static void ConfigureInvWarehouse(EntityTypeBuilder<InvWarehouse> builder)
    {
        builder.ToTable("inv_warehouses");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.Address).HasColumnType("text");
        builder.Property(x => x.Phone).HasMaxLength(30);
        builder.Property(x => x.ManagerId).HasColumnName("manager_id");
        builder.Property(x => x.IsTransit).HasDefaultValue(false).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasOne(x => x.Manager)
            .WithMany()
            .HasForeignKey(x => x.ManagerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.CostCenter)
            .WithMany()
            .HasForeignKey(x => x.CostCenterId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.ManagerId);
        builder.HasIndex(x => x.CostCenterId);
        builder.HasIndex(x => x.IsTransit);
        builder.HasIndex(x => x.IsActive);
    }

    private static void ConfigureInvWarehouseLocation(EntityTypeBuilder<InvWarehouseLocation> builder)
    {
        builder.ToTable("inv_warehouse_locations");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.IsDefault).HasDefaultValue(false).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasOne(x => x.Warehouse)
            .WithMany(x => x.Locations)
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.WarehouseId);
        builder.HasIndex(x => x.Code);
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.IsDefault);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => new { x.WarehouseId, x.Code }).IsUnique();
    }

    private static void ConfigureInvStockBalance(EntityTypeBuilder<InvStockBalance> builder)
    {
        builder.ToTable("inv_stock_balances", t =>
        {
            t.HasCheckConstraint("ck_inv_stock_balances_qty_on_hand_non_negative", "qty_on_hand >= 0");
            t.HasCheckConstraint("ck_inv_stock_balances_qty_reserved_non_negative", "qty_reserved >= 0");
            t.HasCheckConstraint("ck_inv_stock_balances_qty_available_non_negative", "qty_on_hand - qty_reserved >= 0");
            t.HasCheckConstraint("ck_inv_stock_balances_avg_cost_non_negative", "avg_cost >= 0");
        });
        ConfigureAuditEntity(builder);

        builder.Property(x => x.QtyOnHand).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.QtyReserved).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.QtyAvailable).HasColumnType("numeric(18,4)").HasComputedColumnSql("qty_on_hand - qty_reserved", stored: true);
        builder.Property(x => x.AvgCost).HasColumnType("numeric(18,4)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.TotalValue).HasColumnType("numeric(18,4)").HasComputedColumnSql("qty_on_hand * avg_cost", stored: true);
        builder.Property(x => x.LastMovementAt).HasColumnType("timestamptz");

        builder.HasOne(x => x.Item)
            .WithMany(x => x.StockBalances)
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Warehouse)
            .WithMany(x => x.StockBalances)
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Location)
            .WithMany(x => x.StockBalances)
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => x.WarehouseId);
        builder.HasIndex(x => x.LocationId);
        builder.HasIndex(x => x.QtyAvailable);
        builder.HasIndex(x => x.TotalValue);
        builder.HasIndex(x => new { x.ItemId, x.WarehouseId, x.LocationId }).IsUnique();
        builder.HasIndex(x => new { x.ItemId, x.WarehouseId }).IsUnique().HasFilter("\"location_id\" IS NULL");
    }

    private static void ConfigureInvGoodsReceipt(EntityTypeBuilder<InvGoodsReceipt> builder)
    {
        builder.ToTable("inv_goods_receipts");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.ReceiptNo).HasMaxLength(30).IsRequired();
        builder.Property(x => x.ReceiptDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.ReceiptType).HasConversion<int>().IsRequired();
        builder.Property(x => x.SupplierName).HasMaxLength(200);
        builder.Property(x => x.ReferenceNo).HasMaxLength(100);
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.ConfirmedAt).HasColumnType("timestamptz");

        builder.HasOne(x => x.Warehouse)
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Location)
            .WithMany()
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.ReceivedByUser)
            .WithMany()
            .HasForeignKey(x => x.ReceivedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.ConfirmedByUser)
            .WithMany()
            .HasForeignKey(x => x.ConfirmedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.JournalEntry)
            .WithMany()
            .HasForeignKey(x => x.JournalEntryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.ReceiptNo).IsUnique();
        builder.HasIndex(x => x.ReceiptDate);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.WarehouseId);
        builder.HasIndex(x => x.LocationId);
        builder.HasIndex(x => x.ReceiptType);
    }

    private static void ConfigureInvGoodsReceiptLine(EntityTypeBuilder<InvGoodsReceiptLine> builder)
    {
        builder.ToTable("inv_goods_receipt_lines", t =>
        {
            t.HasCheckConstraint("ck_inv_goods_receipt_lines_qty_received_positive", "qty_received > 0");
            t.HasCheckConstraint("ck_inv_goods_receipt_lines_qty_base_positive", "qty_base > 0");
            t.HasCheckConstraint("ck_inv_goods_receipt_lines_unit_cost_non_negative", "unit_cost >= 0");
        });

        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.LineNo).IsRequired();
        builder.Property(x => x.QtyReceived).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.QtyBase).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.UnitCost).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.TotalCost).HasColumnType("numeric(18,4)").HasComputedColumnSql("qty_base * unit_cost", stored: true);
        builder.Property(x => x.Notes).HasColumnType("text");

        builder.HasOne(x => x.GoodsReceipt)
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.GoodsReceiptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Item)
            .WithMany()
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Uom)
            .WithMany()
            .HasForeignKey(x => x.UomId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.GoodsReceiptId);
        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => x.UomId);
        builder.HasIndex(x => new { x.GoodsReceiptId, x.LineNo }).IsUnique();
    }

    private static void ConfigureInvGoodsIssue(EntityTypeBuilder<InvGoodsIssue> builder)
    {
        builder.ToTable("inv_goods_issues");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.IssueNo).HasMaxLength(30).IsRequired();
        builder.Property(x => x.IssueDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.IssueType).HasConversion<int>().IsRequired();
        builder.Property(x => x.ReferenceNo).HasMaxLength(100);
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.ConfirmedAt).HasColumnType("timestamptz");

        builder.HasOne(x => x.Warehouse)
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Location)
            .WithMany()
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.CostCenter)
            .WithMany()
            .HasForeignKey(x => x.CostCenterId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.RequestedByUser)
            .WithMany()
            .HasForeignKey(x => x.RequestedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.IssuedByUser)
            .WithMany()
            .HasForeignKey(x => x.IssuedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.ConfirmedByUser)
            .WithMany()
            .HasForeignKey(x => x.ConfirmedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.JournalEntry)
            .WithMany()
            .HasForeignKey(x => x.JournalEntryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.IssueNo).IsUnique();
        builder.HasIndex(x => x.IssueDate);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.WarehouseId);
        builder.HasIndex(x => x.LocationId);
        builder.HasIndex(x => x.DepartmentId);
        builder.HasIndex(x => x.CostCenterId);
        builder.HasIndex(x => x.IssueType);
    }

    private static void ConfigureInvGoodsIssueLine(EntityTypeBuilder<InvGoodsIssueLine> builder)
    {
        builder.ToTable("inv_goods_issue_lines", t =>
        {
            t.HasCheckConstraint("ck_inv_goods_issue_lines_qty_requested_positive", "qty_requested > 0");
            t.HasCheckConstraint("ck_inv_goods_issue_lines_qty_issued_positive", "qty_issued > 0");
            t.HasCheckConstraint("ck_inv_goods_issue_lines_qty_base_positive", "qty_base > 0");
            t.HasCheckConstraint("ck_inv_goods_issue_lines_unit_cost_non_negative", "unit_cost >= 0");
            t.HasCheckConstraint("ck_inv_goods_issue_lines_qty_not_exceed_requested", "qty_issued <= qty_requested");
        });

        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.LineNo).IsRequired();
        builder.Property(x => x.QtyRequested).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.QtyIssued).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.QtyBase).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.UnitCost).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.TotalCost).HasColumnType("numeric(18,4)").HasComputedColumnSql("qty_base * unit_cost", stored: true);
        builder.Property(x => x.Notes).HasColumnType("text");

        builder.HasOne(x => x.GoodsIssue)
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.GoodsIssueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Item)
            .WithMany()
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Uom)
            .WithMany()
            .HasForeignKey(x => x.UomId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.GoodsIssueId);
        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => x.UomId);
        builder.HasIndex(x => new { x.GoodsIssueId, x.LineNo }).IsUnique();
    }

    private static void ConfigureInvStockTransfer(EntityTypeBuilder<InvStockTransfer> builder)
    {
        builder.ToTable("inv_stock_transfers", t =>
        {
            t.HasCheckConstraint("ck_inv_stock_transfers_warehouse_not_same", "from_warehouse_id <> to_warehouse_id");
        });
        ConfigureAuditEntity(builder);

        builder.Property(x => x.TransferNo).HasMaxLength(30).IsRequired();
        builder.Property(x => x.TransferDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.ReferenceNo).HasMaxLength(100);
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.ConfirmedAt).HasColumnType("timestamptz");

        builder.HasOne(x => x.FromWarehouse)
            .WithMany()
            .HasForeignKey(x => x.FromWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ToWarehouse)
            .WithMany()
            .HasForeignKey(x => x.ToWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FromLocation)
            .WithMany()
            .HasForeignKey(x => x.FromLocationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.ToLocation)
            .WithMany()
            .HasForeignKey(x => x.ToLocationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.TransferredByUser)
            .WithMany()
            .HasForeignKey(x => x.TransferredBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.ConfirmedByUser)
            .WithMany()
            .HasForeignKey(x => x.ConfirmedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.JournalEntry)
            .WithMany()
            .HasForeignKey(x => x.JournalEntryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.TransferNo).IsUnique();
        builder.HasIndex(x => x.TransferDate);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.FromWarehouseId);
        builder.HasIndex(x => x.ToWarehouseId);
    }

    private static void ConfigureInvStockTransferLine(EntityTypeBuilder<InvStockTransferLine> builder)
    {
        builder.ToTable("inv_stock_transfer_lines", t =>
        {
            t.HasCheckConstraint("ck_inv_stock_transfer_lines_qty_transfer_positive", "qty_transfer > 0");
            t.HasCheckConstraint("ck_inv_stock_transfer_lines_qty_base_positive", "qty_base > 0");
            t.HasCheckConstraint("ck_inv_stock_transfer_lines_unit_cost_non_negative", "unit_cost >= 0");
        });

        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.LineNo).IsRequired();
        builder.Property(x => x.QtyTransfer).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.QtyBase).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.UnitCost).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.TotalCost).HasColumnType("numeric(18,4)").HasComputedColumnSql("qty_base * unit_cost", stored: true);
        builder.Property(x => x.Notes).HasColumnType("text");

        builder.HasOne(x => x.StockTransfer)
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.StockTransferId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Item)
            .WithMany()
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Uom)
            .WithMany()
            .HasForeignKey(x => x.UomId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.StockTransferId);
        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => x.UomId);
        builder.HasIndex(x => new { x.StockTransferId, x.LineNo }).IsUnique();
    }

    private static void ConfigureInvStockAdjustment(EntityTypeBuilder<InvStockAdjustment> builder)
    {
        builder.ToTable("inv_stock_adjustments");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.AdjustmentNo).HasMaxLength(30).IsRequired();
        builder.Property(x => x.AdjustmentDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.Reason).HasConversion<int>().IsRequired();
        builder.Property(x => x.ReferenceNo).HasMaxLength(100);
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.ApprovedAt).HasColumnType("timestamptz");
        builder.Property(x => x.ConfirmedAt).HasColumnType("timestamptz");

        builder.HasOne(x => x.Warehouse)
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Location)
            .WithMany()
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.RequestedByUser)
            .WithMany()
            .HasForeignKey(x => x.RequestedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.ApprovedByUser)
            .WithMany()
            .HasForeignKey(x => x.ApprovedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.ConfirmedByUser)
            .WithMany()
            .HasForeignKey(x => x.ConfirmedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.JournalEntry)
            .WithMany()
            .HasForeignKey(x => x.JournalEntryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.AdjustmentNo).IsUnique();
        builder.HasIndex(x => x.AdjustmentDate);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Reason);
        builder.HasIndex(x => x.WarehouseId);
    }

    private static void ConfigureInvStockAdjustmentLine(EntityTypeBuilder<InvStockAdjustmentLine> builder)
    {
        builder.ToTable("inv_stock_adjustment_lines", t =>
        {
            t.HasCheckConstraint("ck_inv_stock_adjustment_lines_qty_adjustment_not_zero", "qty_adjustment <> 0");
            t.HasCheckConstraint("ck_inv_stock_adjustment_lines_unit_cost_non_negative", "unit_cost >= 0");
        });

        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.LineNo).IsRequired();
        builder.Property(x => x.QtyAdjustment).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.UnitCost).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.TotalCost).HasColumnType("numeric(18,4)").HasComputedColumnSql("qty_adjustment * unit_cost", stored: true);
        builder.Property(x => x.Notes).HasColumnType("text");

        builder.HasOne(x => x.StockAdjustment)
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.StockAdjustmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Item)
            .WithMany()
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Uom)
            .WithMany()
            .HasForeignKey(x => x.UomId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.StockAdjustmentId);
        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => x.UomId);
        builder.HasIndex(x => new { x.StockAdjustmentId, x.LineNo }).IsUnique();
    }

    private static void ConfigureInvStockOpname(EntityTypeBuilder<InvStockOpname> builder)
    {
        builder.ToTable("inv_stock_opnames");
        ConfigureAuditEntity(builder);

        builder.Property(x => x.OpnameNo).HasMaxLength(30).IsRequired();
        builder.Property(x => x.OpnameDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.ApprovedAt).HasColumnType("timestamptz");

        builder.HasOne(x => x.Warehouse)
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Location)
            .WithMany()
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.CountedByUser)
            .WithMany()
            .HasForeignKey(x => x.CountedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.ApprovedByUser)
            .WithMany()
            .HasForeignKey(x => x.ApprovedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Adjustment)
            .WithMany()
            .HasForeignKey(x => x.AdjustmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.OpnameNo).IsUnique();
        builder.HasIndex(x => x.OpnameDate);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.WarehouseId);
    }

    private static void ConfigureInvStockOpnameLine(EntityTypeBuilder<InvStockOpnameLine> builder)
    {
        builder.ToTable("inv_opname_lines", t =>
        {
            t.HasCheckConstraint("ck_inv_opname_lines_qty_system_non_negative", "qty_system >= 0");
            t.HasCheckConstraint("ck_inv_opname_lines_qty_counted_non_negative", "qty_counted >= 0");
            t.HasCheckConstraint("ck_inv_opname_lines_unit_cost_non_negative", "unit_cost >= 0");
        });

        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.LineNo).IsRequired();
        builder.Property(x => x.QtySystem).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.QtyCounted).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.QtyVariance).HasColumnType("numeric(18,4)").HasComputedColumnSql("qty_counted - qty_system", stored: true);
        builder.Property(x => x.UnitCost).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.TotalVarianceValue).HasColumnType("numeric(18,4)").HasComputedColumnSql("(qty_counted - qty_system) * unit_cost", stored: true);
        builder.Property(x => x.Notes).HasColumnType("text");

        builder.HasOne(x => x.StockOpname)
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.StockOpnameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Item)
            .WithMany()
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Location)
            .WithMany()
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.StockOpnameId);
        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => x.LocationId);
        builder.HasIndex(x => new { x.StockOpnameId, x.LineNo }).IsUnique();
        builder.HasIndex(x => new { x.StockOpnameId, x.ItemId, x.LocationId }).IsUnique();
    }

    private static void ConfigureInvStockMovement(EntityTypeBuilder<InvStockMovement> builder)
    {
        builder.ToTable("inv_stock_movements", t =>
        {
            t.HasCheckConstraint("ck_inv_stock_movements_qty_non_negative", "qty_in >= 0 AND qty_out >= 0");
            t.HasCheckConstraint("ck_inv_stock_movements_unit_cost_non_negative", "unit_cost >= 0");
        });

        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.MovementDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.MovementType).HasConversion<int>().IsRequired();
        builder.Property(x => x.QtyIn).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.QtyOut).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.QtyBalance).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.UnitCost).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.TotalCost).HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.SourceTable).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Notes).HasColumnType("text");
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz").IsRequired();

        builder.HasOne(x => x.Item)
            .WithMany()
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Warehouse)
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Location)
            .WithMany()
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.MovementDate);
        builder.HasIndex(x => x.ItemId);
        builder.HasIndex(x => x.WarehouseId);
        builder.HasIndex(x => x.LocationId);
        builder.HasIndex(x => x.MovementType);
        builder.HasIndex(x => new { x.ItemId, x.WarehouseId, x.LocationId, x.MovementDate });
        builder.HasIndex(x => new { x.SourceTable, x.SourceId });
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






