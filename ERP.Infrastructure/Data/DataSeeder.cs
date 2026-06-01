using ERP.Application.Services;
using ERP.Domain.Entities.Config;
using ERP.Domain.Entities.HR;
using ERP.Domain.Entities.Finance;
using ERP.Domain.Entities.Inventory;
using ERP.Domain.Entities.System;
using ERP.Domain.Enums;
using ERP.Domain.Enums.Inventory;
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
        await SeedInventoryMasterDataAsync(now, ct);
        await SeedAdminUserAsync(now, ct);
        await SeedMenusAsync(now, ct);
        await SeedSuperAdminPermissionsAsync(ct);
        await SeedInventoryRolePermissionsAsync(ct);
    }

    private async Task SeedModulesAsync(DateTimeOffset now, CancellationToken ct)
    {
        await EnsureModuleAsync("Human Resources", "HR", "bi-people", 1, now, ct);
        await EnsureModuleAsync("System Configuration", "CFG", "bi-gear", 2, now, ct);
        await EnsureModuleAsync("Finance", "FIN", "bi-cash-coin", 3, now, ct);
        await EnsureModuleAsync("Inventory", "INV", "bi-box-seam", 4, now, ct);
    }

    private async Task SeedRolesAsync(DateTimeOffset now, CancellationToken ct)
    {
        var roleNames = new[]
        {
            "Super Admin",
            "HR Manager",
            "HR Staff",
            "Inventory Manager",
            "Gudang Staff",
            "Finance Staff",
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
        await EnsureAccountAsync("1150", "Persediaan Barang", group1100.Id, FinanceAccountType.Asset, FinanceNormalBalance.Debit, false, acc1100.Id, null, false, null, null, "IDR", true, now, ct);

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
        await EnsureAccountAsync("5201", "HPP/Beban Pemakaian", group5000.Id, FinanceAccountType.Expense, FinanceNormalBalance.Debit, false, acc5000.Id, null, false, null, null, "IDR", true, now, ct);
        await EnsureAccountAsync("5202", "Penyesuaian Persediaan", group5000.Id, FinanceAccountType.Expense, FinanceNormalBalance.Debit, false, acc5000.Id, null, false, null, null, "IDR", true, now, ct);
        await EnsureAccountAsync("6001", "Beban Kerugian Persediaan", group5000.Id, FinanceAccountType.Expense, FinanceNormalBalance.Debit, false, acc5000.Id, null, false, null, null, "IDR", true, now, ct);

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

            await EnsureSampleFinanceBudgetsAsync(
                fiscalYear.Id,
                seededPeriod.Id,
                seededCostCenterId,
                acc5101.Id,
                acc5104.Id,
                acc5105.Id,
                now,
                ct);

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
    private async Task SeedInventoryMasterDataAsync(DateTimeOffset now, CancellationToken ct)
    {
        var categoryRawMaterial = await EnsureInvItemCategoryAsync("RAW", "Raw Materials", null, "Raw materials and production supplies", true, now, ct);
        var categoryFinishedGoods = await EnsureInvItemCategoryAsync("FG", "Finished Goods", null, "Products ready for sale", true, now, ct);
        var categorySparePart = await EnsureInvItemCategoryAsync("SP", "Spare Parts", null, "Maintenance and spare parts", true, now, ct);
        await EnsureInvItemCategoryAsync("ELEC", "Elektronik", null, "Kategori barang elektronik", true, now, ct);
        await EnsureInvItemCategoryAsync("CONS", "Bahan Habis Pakai", null, "Kategori consumable", true, now, ct);
        await EnsureInvItemCategoryAsync("ATK", "Alat Tulis Kantor", null, "Kategori ATK", true, now, ct);
        await EnsureInvItemCategoryAsync("TOOLS", "Peralatan", null, "Kategori peralatan", true, now, ct);
        await EnsureInvItemCategoryAsync("BAHAN", "Bahan Bangunan", null, "Kategori bahan bangunan", true, now, ct);

        var uomPcs = await EnsureInvUnitOfMeasureAsync("PCS", "Pieces", "Default unit for countable items", true, now, ct);
        var uomBox = await EnsureInvUnitOfMeasureAsync("BOX", "Box", "Packaging unit", true, now, ct);
        var uomKg = await EnsureInvUnitOfMeasureAsync("KG", "Kilogram", "Weight unit", true, now, ct);
        await EnsureInvUnitOfMeasureAsync("EA", "Each/Buah", "Default each unit", true, now, ct);
        await EnsureInvUnitOfMeasureAsync("M", "Meter", "Length unit", true, now, ct);
        await EnsureInvUnitOfMeasureAsync("M2", "Meter Persegi", "Area unit", true, now, ct);
        await EnsureInvUnitOfMeasureAsync("PCE", "Piece", "Piece unit", true, now, ct);
        await EnsureInvUnitOfMeasureAsync("SET", "Set", "Set unit", true, now, ct);
        await EnsureInvUnitOfMeasureAsync("LITER", "Liter", "Volume unit", true, now, ct);
        await EnsureInvUnitOfMeasureAsync("ROLL", "Roll", "Roll unit", true, now, ct);
        await EnsureInvUnitOfMeasureAsync("UNIT", "Unit", "Unit count", true, now, ct);

        var brandSinara = await EnsureInvBrandAsync("SINARA", "Default internal brand", true, now, ct);
        var brandGeneric = await EnsureInvBrandAsync("GENERIC", "General-purpose supplier brand", true, now, ct);

        var inventoryAccountId = await dbContext.FinAccounts
            .AsNoTracking()
            .Where(x => x.Code == "1150" && x.IsActive)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(ct);

        var cogsAccountId = await dbContext.FinAccounts
            .AsNoTracking()
            .Where(x => x.Code == "5201" && x.IsActive)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(ct);

        var adjustmentAccountId = await dbContext.FinAccounts
            .AsNoTracking()
            .Where(x => x.Code == "5202" && x.IsActive)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(ct);

        var itemSteelPlate = await EnsureInvItemAsync(
            itemCode: "ITEM-RM-001",
            sku: "RM-STEEL-A36",
            name: "Steel Plate A36",
            description: "Raw material for fabrication",
            categoryId: categoryRawMaterial.Id,
            brandId: brandGeneric.Id,
            type: ItemType.RawMaterial,
            baseUomId: uomKg.Id,
            purchaseUomId: uomKg.Id,
            status: ItemStatus.Active,
            valuationMethod: ValuationMethod.WeightedAverageCost,
            lastPurchasePrice: 18000m,
            avgCost: 18000m,
            minStock: 100m,
            maxStock: 1500m,
            reorderPoint: 250m,
            leadTimeDays: 7,
            inventoryAccountId: inventoryAccountId,
            cogsAccountId: cogsAccountId,
            adjustmentAccountId: adjustmentAccountId,
            notes: "Seeded by system",
            isActive: true,
            now,
            ct);

        var itemBolt = await EnsureInvItemAsync(
            itemCode: "ITEM-FG-001",
            sku: "FG-BOLT-M10",
            name: "Bolt M10",
            description: "Finished goods fastener",
            categoryId: categoryFinishedGoods.Id,
            brandId: brandSinara.Id,
            type: ItemType.Product,
            baseUomId: uomPcs.Id,
            purchaseUomId: uomBox.Id,
            status: ItemStatus.Active,
            valuationMethod: ValuationMethod.WeightedAverageCost,
            lastPurchasePrice: 1500m,
            avgCost: 1500m,
            minStock: 500m,
            maxStock: 5000m,
            reorderPoint: 1000m,
            leadTimeDays: 5,
            inventoryAccountId: inventoryAccountId,
            cogsAccountId: cogsAccountId,
            adjustmentAccountId: adjustmentAccountId,
            notes: "Seeded by system",
            isActive: true,
            now,
            ct);

        var itemBearing = await EnsureInvItemAsync(
            itemCode: "ITEM-SP-001",
            sku: "SP-BRG-6205",
            name: "Bearing 6205",
            description: "Spare part for machine maintenance",
            categoryId: categorySparePart.Id,
            brandId: brandGeneric.Id,
            type: ItemType.Consumable,
            baseUomId: uomPcs.Id,
            purchaseUomId: uomBox.Id,
            status: ItemStatus.Active,
            valuationMethod: ValuationMethod.WeightedAverageCost,
            lastPurchasePrice: 25000m,
            avgCost: 25000m,
            minStock: 30m,
            maxStock: 300m,
            reorderPoint: 60m,
            leadTimeDays: 10,
            inventoryAccountId: inventoryAccountId,
            cogsAccountId: cogsAccountId,
            adjustmentAccountId: adjustmentAccountId,
            notes: "Seeded by system",
            isActive: true,
            now,
            ct);

        await EnsureInvItemConversionAsync(itemBolt.Id, uomBox.Id, uomPcs.Id, 100m, true, now, ct);
        await EnsureInvItemConversionAsync(itemBearing.Id, uomBox.Id, uomPcs.Id, 20m, true, now, ct);

        var managerId = await dbContext.HrEmployees
            .AsNoTracking()
            .Where(x => x.EmploymentStatus == EmploymentStatus.Active)
            .OrderBy(x => x.EmployeeCode)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(ct);

        var costCenterId = await dbContext.FinCostCenters
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(ct);

        var warehouseMain = await EnsureInvWarehouseAsync(
            code: "GDG-UTM",
            name: "Gudang Utama",
            description: "Gudang utama operasional",
            address: "Jl. Gudang Utama No. 1",
            phone: "021-5550111",
            managerId,
            costCenterId,
            isTransit: false,
            isActive: true,
            now,
            ct);

        var warehouseTransit = await EnsureInvWarehouseAsync(
            code: "GDG-TRSIT",
            name: "Gudang Transit",
            description: "Gudang transit sementara",
            address: "Jl. Gudang Transit No. 2",
            phone: "021-5550112",
            managerId,
            costCenterId,
            isTransit: true,
            isActive: true,
            now,
            ct);

        var locationMainA1 = await EnsureInvWarehouseLocationAsync(warehouseMain.Id, "RAK-A1", "RAK-A1", "Fast moving items", true, true, now, ct);
        var locationMainA2 = await EnsureInvWarehouseLocationAsync(warehouseMain.Id, "RAK-A2", "RAK-A2", "General storage", false, true, now, ct);
        await EnsureInvWarehouseLocationAsync(warehouseMain.Id, "RAK-B1", "RAK-B1", "Storage rack B1", false, true, now, ct);
        await EnsureInvWarehouseLocationAsync(warehouseMain.Id, "RAK-B2", "RAK-B2", "Storage rack B2", false, true, now, ct);
        await EnsureInvWarehouseLocationAsync(warehouseMain.Id, "ZONA-UMUM", "ZONA-UMUM", "General zone", false, true, now, ct);
        var locationTransit01 = await EnsureInvWarehouseLocationAsync(warehouseTransit.Id, "TRS-01", "TRS-01", "Transit buffer", true, true, now, ct);

        await EnsureInvStockBalanceAsync(itemSteelPlate.Id, warehouseMain.Id, locationMainA1.Id, 500m, 50m, itemSteelPlate.AvgCost, now, ct);
        await EnsureInvStockBalanceAsync(itemBolt.Id, warehouseMain.Id, locationMainA2.Id, 1200m, 100m, itemBolt.AvgCost, now, ct);
        await EnsureInvStockBalanceAsync(itemBearing.Id, warehouseTransit.Id, locationTransit01.Id, 80m, 0m, itemBearing.AvgCost, now, ct);

        await SeedInventoryTransactionDataAsync(
            itemSteelPlate,
            itemBolt,
            itemBearing,
            uomKg,
            uomPcs,
            warehouseMain,
            warehouseTransit,
            locationMainA1,
            locationMainA2,
            locationTransit01,
            costCenterId,
            now,
            ct);
    }

    private async Task<InvItemCategory> EnsureInvItemCategoryAsync(
        string code,
        string name,
        int? parentCategoryId,
        string? description,
        bool isActive,
        DateTimeOffset now,
        CancellationToken ct)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        var normalizedName = name.Trim();

        var existing = await dbContext.InvItemCategories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Code == normalizedCode, ct);

        if (existing is null)
        {
            existing = new InvItemCategory
            {
                Code = normalizedCode,
                Name = normalizedName,
                ParentCategoryId = parentCategoryId,
                Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                IsActive = isActive,
                CreatedBy = "system",
                CreatedAt = now
            };

            dbContext.InvItemCategories.Add(existing);
        }
        else
        {
            existing.Name = normalizedName;
            existing.ParentCategoryId = parentCategoryId;
            existing.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            existing.IsActive = isActive;
            existing.IsDeleted = false;
            existing.DeletedAt = null;
            existing.UpdatedBy = "system";
            existing.UpdatedAt = now;
        }

        await dbContext.SaveChangesAsync(ct);
        return existing;
    }

    private async Task<InvUnitOfMeasure> EnsureInvUnitOfMeasureAsync(
        string code,
        string name,
        string? description,
        bool isActive,
        DateTimeOffset now,
        CancellationToken ct)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        var normalizedName = name.Trim();

        var existing = await dbContext.InvUnitsOfMeasure
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Code == normalizedCode, ct);

        if (existing is null)
        {
            existing = new InvUnitOfMeasure
            {
                Code = normalizedCode,
                Name = normalizedName,
                Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                IsActive = isActive,
                CreatedBy = "system",
                CreatedAt = now
            };

            dbContext.InvUnitsOfMeasure.Add(existing);
        }
        else
        {
            existing.Name = normalizedName;
            existing.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            existing.IsActive = isActive;
            existing.IsDeleted = false;
            existing.DeletedAt = null;
            existing.UpdatedBy = "system";
            existing.UpdatedAt = now;
        }

        await dbContext.SaveChangesAsync(ct);
        return existing;
    }

    private async Task<InvBrand> EnsureInvBrandAsync(
        string name,
        string? description,
        bool isActive,
        DateTimeOffset now,
        CancellationToken ct)
    {
        var normalizedName = name.Trim();

        var existing = await dbContext.InvBrands
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Name == normalizedName, ct);

        if (existing is null)
        {
            existing = new InvBrand
            {
                Name = normalizedName,
                Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                IsActive = isActive,
                CreatedBy = "system",
                CreatedAt = now
            };

            dbContext.InvBrands.Add(existing);
        }
        else
        {
            existing.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            existing.IsActive = isActive;
            existing.IsDeleted = false;
            existing.DeletedAt = null;
            existing.UpdatedBy = "system";
            existing.UpdatedAt = now;
        }

        await dbContext.SaveChangesAsync(ct);
        return existing;
    }

    private async Task<InvItem> EnsureInvItemAsync(
        string itemCode,
        string? sku,
        string name,
        string? description,
        int categoryId,
        int? brandId,
        ItemType type,
        int baseUomId,
        int? purchaseUomId,
        ItemStatus status,
        ValuationMethod valuationMethod,
        decimal? lastPurchasePrice,
        decimal avgCost,
        decimal minStock,
        decimal maxStock,
        decimal reorderPoint,
        int leadTimeDays,
        int? inventoryAccountId,
        int? cogsAccountId,
        int? adjustmentAccountId,
        string? notes,
        bool isActive,
        DateTimeOffset now,
        CancellationToken ct)
    {
        var normalizedCode = itemCode.Trim().ToUpperInvariant();
        var normalizedSku = string.IsNullOrWhiteSpace(sku) ? null : sku.Trim().ToUpperInvariant();
        var normalizedName = name.Trim();

        var existing = await dbContext.InvItems
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.ItemCode == normalizedCode, ct);

        if (existing is null)
        {
            existing = new InvItem
            {
                ItemCode = normalizedCode,
                CreatedBy = "system",
                CreatedAt = now
            };

            dbContext.InvItems.Add(existing);
        }
        else
        {
            existing.UpdatedBy = "system";
            existing.UpdatedAt = now;
        }

        existing.Sku = normalizedSku;
        existing.Name = normalizedName;
        existing.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        existing.CategoryId = categoryId;
        existing.BrandId = brandId;
        existing.Type = type;
        existing.BaseUomId = baseUomId;
        existing.PurchaseUomId = purchaseUomId;
        existing.Status = status;
        existing.ValuationMethod = valuationMethod;
        existing.LastPurchasePrice = lastPurchasePrice;
        existing.AvgCost = decimal.Round(avgCost, 4, MidpointRounding.AwayFromZero);
        existing.MinStock = decimal.Round(minStock, 4, MidpointRounding.AwayFromZero);
        existing.MaxStock = decimal.Round(maxStock, 4, MidpointRounding.AwayFromZero);
        existing.ReorderPoint = decimal.Round(reorderPoint, 4, MidpointRounding.AwayFromZero);
        existing.LeadTimeDays = leadTimeDays;
        existing.InventoryAccountId = inventoryAccountId;
        existing.CogsAccountId = cogsAccountId;
        existing.AdjustmentAccountId = adjustmentAccountId;
        existing.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        existing.IsActive = isActive;
        existing.IsDeleted = false;
        existing.DeletedAt = null;

        await dbContext.SaveChangesAsync(ct);
        return existing;
    }

    private async Task<InvItemUnitConversion> EnsureInvItemConversionAsync(
        int itemId,
        int fromUomId,
        int toUomId,
        decimal conversionFactor,
        bool isActive,
        DateTimeOffset now,
        CancellationToken ct)
    {
        var existing = await dbContext.InvItemUnitConversions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.ItemId == itemId && x.FromUomId == fromUomId && x.ToUomId == toUomId, ct);

        if (existing is null)
        {
            existing = new InvItemUnitConversion
            {
                ItemId = itemId,
                FromUomId = fromUomId,
                ToUomId = toUomId,
                CreatedBy = "system",
                CreatedAt = now
            };

            dbContext.InvItemUnitConversions.Add(existing);
        }
        else
        {
            existing.UpdatedBy = "system";
            existing.UpdatedAt = now;
        }

        existing.ConversionFactor = decimal.Round(conversionFactor, 6, MidpointRounding.AwayFromZero);
        existing.IsActive = isActive;
        existing.IsDeleted = false;
        existing.DeletedAt = null;

        await dbContext.SaveChangesAsync(ct);
        return existing;
    }

    private async Task<InvWarehouse> EnsureInvWarehouseAsync(
        string code,
        string name,
        string? description,
        string? address,
        string? phone,
        int? managerId,
        int? costCenterId,
        bool isTransit,
        bool isActive,
        DateTimeOffset now,
        CancellationToken ct)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        var normalizedName = name.Trim();

        var existing = await dbContext.InvWarehouses
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Code == normalizedCode, ct);

        if (existing is null)
        {
            existing = new InvWarehouse
            {
                Code = normalizedCode,
                CreatedBy = "system",
                CreatedAt = now
            };

            dbContext.InvWarehouses.Add(existing);
        }
        else
        {
            existing.UpdatedBy = "system";
            existing.UpdatedAt = now;
        }

        existing.Name = normalizedName;
        existing.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        existing.Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
        existing.Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        existing.ManagerId = managerId;
        existing.CostCenterId = costCenterId;
        existing.IsTransit = isTransit;
        existing.IsActive = isActive;
        existing.IsDeleted = false;
        existing.DeletedAt = null;

        await dbContext.SaveChangesAsync(ct);
        return existing;
    }

    private async Task<InvWarehouseLocation> EnsureInvWarehouseLocationAsync(
        int warehouseId,
        string code,
        string name,
        string? description,
        bool isDefault,
        bool isActive,
        DateTimeOffset now,
        CancellationToken ct)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        var normalizedName = name.Trim();

        var existing = await dbContext.InvWarehouseLocations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.WarehouseId == warehouseId && x.Code == normalizedCode, ct);

        if (existing is null)
        {
            existing = new InvWarehouseLocation
            {
                WarehouseId = warehouseId,
                Code = normalizedCode,
                CreatedBy = "system",
                CreatedAt = now
            };

            dbContext.InvWarehouseLocations.Add(existing);
        }
        else
        {
            existing.UpdatedBy = "system";
            existing.UpdatedAt = now;
        }

        if (isDefault)
        {
            var defaults = await dbContext.InvWarehouseLocations
                .Where(x => x.WarehouseId == warehouseId && x.Id != existing.Id && x.IsDefault)
                .ToListAsync(ct);

            foreach (var currentDefault in defaults)
            {
                currentDefault.IsDefault = false;
                currentDefault.UpdatedBy = "system";
                currentDefault.UpdatedAt = now;
            }
        }

        existing.Name = normalizedName;
        existing.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        existing.IsDefault = isDefault;
        existing.IsActive = isActive;
        existing.IsDeleted = false;
        existing.DeletedAt = null;

        await dbContext.SaveChangesAsync(ct);
        return existing;
    }

    private async Task<InvStockBalance> EnsureInvStockBalanceAsync(
        int itemId,
        int warehouseId,
        int? locationId,
        decimal qtyOnHand,
        decimal qtyReserved,
        decimal avgCost,
        DateTimeOffset now,
        CancellationToken ct)
    {
        var existing = await dbContext.InvStockBalances
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.ItemId == itemId && x.WarehouseId == warehouseId && x.LocationId == locationId, ct);

        if (existing is null)
        {
            existing = new InvStockBalance
            {
                ItemId = itemId,
                WarehouseId = warehouseId,
                LocationId = locationId,
                CreatedBy = "system",
                CreatedAt = now
            };

            dbContext.InvStockBalances.Add(existing);
        }
        else
        {
            existing.UpdatedBy = "system";
            existing.UpdatedAt = now;
        }

        existing.QtyOnHand = decimal.Round(qtyOnHand, 4, MidpointRounding.AwayFromZero);
        existing.QtyReserved = decimal.Round(qtyReserved, 4, MidpointRounding.AwayFromZero);
        existing.AvgCost = decimal.Round(avgCost, 4, MidpointRounding.AwayFromZero);
        existing.LastMovementAt = now;
        existing.IsDeleted = false;
        existing.DeletedAt = null;

        await dbContext.SaveChangesAsync(ct);
        return existing;
    }

    private async Task SeedInventoryTransactionDataAsync(
        InvItem itemSteelPlate,
        InvItem itemBolt,
        InvItem itemBearing,
        InvUnitOfMeasure uomKg,
        InvUnitOfMeasure uomPcs,
        InvWarehouse warehouseMain,
        InvWarehouse warehouseTransit,
        InvWarehouseLocation locationMainA1,
        InvWarehouseLocation locationMainA2,
        InvWarehouseLocation locationTransit01,
        int? costCenterId,
        DateTimeOffset now,
        CancellationToken ct)
    {
        var departmentId = await dbContext.HrDepartments
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(ct);

        var goodsReceipt = await dbContext.InvGoodsReceipts
            .IgnoreQueryFilters()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.ReceiptNo == "GR-2026-0001", ct);

        if (goodsReceipt is null)
        {
            goodsReceipt = new InvGoodsReceipt
            {
                ReceiptNo = "GR-2026-0001",
                CreatedBy = "system",
                CreatedAt = now
            };

            dbContext.InvGoodsReceipts.Add(goodsReceipt);
        }
        else
        {
            dbContext.InvGoodsReceiptLines.RemoveRange(goodsReceipt.Lines);
            goodsReceipt.Lines.Clear();
            goodsReceipt.UpdatedBy = "system";
            goodsReceipt.UpdatedAt = now;
        }

        goodsReceipt.ReceiptDate = new DateOnly(2026, 1, 5);
        goodsReceipt.ReceiptType = GoodsReceiptType.PurchaseReceipt;
        goodsReceipt.WarehouseId = warehouseMain.Id;
        goodsReceipt.LocationId = locationMainA1.Id;
        goodsReceipt.SupplierName = "PT Baja Nusantara";
        goodsReceipt.ReferenceNo = "PO-2026-001";
        goodsReceipt.Description = "Seeded goods receipt";
        goodsReceipt.Status = TransactionStatus.Draft;
        goodsReceipt.IsDeleted = false;
        goodsReceipt.DeletedAt = null;

        goodsReceipt.Lines.Add(new InvGoodsReceiptLine
        {
            LineNo = 1,
            ItemId = itemSteelPlate.Id,
            UomId = uomKg.Id,
            QtyReceived = 120m,
            QtyBase = 120m,
            UnitCost = 18000m,
            TotalCost = 2_160_000m,
            Notes = "Seeded line"
        });

        await dbContext.SaveChangesAsync(ct);

        var goodsIssue = await dbContext.InvGoodsIssues
            .IgnoreQueryFilters()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.IssueNo == "GI-2026-0001", ct);

        if (goodsIssue is null)
        {
            goodsIssue = new InvGoodsIssue
            {
                IssueNo = "GI-2026-0001",
                CreatedBy = "system",
                CreatedAt = now
            };

            dbContext.InvGoodsIssues.Add(goodsIssue);
        }
        else
        {
            dbContext.InvGoodsIssueLines.RemoveRange(goodsIssue.Lines);
            goodsIssue.Lines.Clear();
            goodsIssue.UpdatedBy = "system";
            goodsIssue.UpdatedAt = now;
        }

        goodsIssue.IssueDate = new DateOnly(2026, 1, 8);
        goodsIssue.IssueType = GoodsIssueType.DepartmentalUse;
        goodsIssue.WarehouseId = warehouseMain.Id;
        goodsIssue.LocationId = locationMainA2.Id;
        goodsIssue.DepartmentId = departmentId;
        goodsIssue.CostCenterId = costCenterId;
        goodsIssue.ReferenceNo = "REQ-2026-017";
        goodsIssue.Description = "Seeded goods issue";
        goodsIssue.Status = TransactionStatus.Draft;
        goodsIssue.IsDeleted = false;
        goodsIssue.DeletedAt = null;

        goodsIssue.Lines.Add(new InvGoodsIssueLine
        {
            LineNo = 1,
            ItemId = itemBolt.Id,
            UomId = uomPcs.Id,
            QtyRequested = 80m,
            QtyIssued = 80m,
            QtyBase = 80m,
            UnitCost = 1500m,
            TotalCost = 120_000m,
            Notes = "Seeded line"
        });

        await dbContext.SaveChangesAsync(ct);

        var stockTransfer = await dbContext.InvStockTransfers
            .IgnoreQueryFilters()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.TransferNo == "TRF-2026-0001", ct);

        if (stockTransfer is null)
        {
            stockTransfer = new InvStockTransfer
            {
                TransferNo = "TRF-2026-0001",
                CreatedBy = "system",
                CreatedAt = now
            };

            dbContext.InvStockTransfers.Add(stockTransfer);
        }
        else
        {
            dbContext.InvStockTransferLines.RemoveRange(stockTransfer.Lines);
            stockTransfer.Lines.Clear();
            stockTransfer.UpdatedBy = "system";
            stockTransfer.UpdatedAt = now;
        }

        stockTransfer.TransferDate = new DateOnly(2026, 1, 12);
        stockTransfer.FromWarehouseId = warehouseMain.Id;
        stockTransfer.FromLocationId = locationMainA2.Id;
        stockTransfer.ToWarehouseId = warehouseTransit.Id;
        stockTransfer.ToLocationId = locationTransit01.Id;
        stockTransfer.ReferenceNo = "TRN-2026-004";
        stockTransfer.Description = "Seeded stock transfer";
        stockTransfer.Status = TransactionStatus.Draft;
        stockTransfer.IsDeleted = false;
        stockTransfer.DeletedAt = null;

        stockTransfer.Lines.Add(new InvStockTransferLine
        {
            LineNo = 1,
            ItemId = itemBolt.Id,
            UomId = uomPcs.Id,
            QtyTransfer = 50m,
            QtyBase = 50m,
            UnitCost = 1500m,
            TotalCost = 75_000m,
            Notes = "Seeded line"
        });

        await dbContext.SaveChangesAsync(ct);

        var stockAdjustment = await dbContext.InvStockAdjustments
            .IgnoreQueryFilters()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.AdjustmentNo == "ADJ-2026-0001", ct);

        if (stockAdjustment is null)
        {
            stockAdjustment = new InvStockAdjustment
            {
                AdjustmentNo = "ADJ-2026-0001",
                CreatedBy = "system",
                CreatedAt = now
            };

            dbContext.InvStockAdjustments.Add(stockAdjustment);
        }
        else
        {
            dbContext.InvStockAdjustmentLines.RemoveRange(stockAdjustment.Lines);
            stockAdjustment.Lines.Clear();
            stockAdjustment.UpdatedBy = "system";
            stockAdjustment.UpdatedAt = now;
        }

        stockAdjustment.AdjustmentDate = new DateOnly(2026, 1, 15);
        stockAdjustment.WarehouseId = warehouseMain.Id;
        stockAdjustment.LocationId = locationMainA2.Id;
        stockAdjustment.Reason = AdjustmentReason.DataCorrection;
        stockAdjustment.ReferenceNo = "ADJ-REQ-2026-001";
        stockAdjustment.Description = "Seeded stock adjustment";
        stockAdjustment.Status = TransactionStatus.Draft;
        stockAdjustment.IsDeleted = false;
        stockAdjustment.DeletedAt = null;

        stockAdjustment.Lines.Add(new InvStockAdjustmentLine
        {
            LineNo = 1,
            ItemId = itemBolt.Id,
            UomId = uomPcs.Id,
            QtyAdjustment = -10m,
            UnitCost = 1500m,
            TotalCost = 15_000m,
            Notes = "Seeded line"
        });

        await dbContext.SaveChangesAsync(ct);

        var stockOpname = await dbContext.InvStockOpnames
            .IgnoreQueryFilters()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.OpnameNo == "OPN-2026-0001", ct);

        if (stockOpname is null)
        {
            stockOpname = new InvStockOpname
            {
                OpnameNo = "OPN-2026-0001",
                CreatedBy = "system",
                CreatedAt = now
            };

            dbContext.InvStockOpnames.Add(stockOpname);
        }
        else
        {
            dbContext.InvStockOpnameLines.RemoveRange(stockOpname.Lines);
            stockOpname.Lines.Clear();
            stockOpname.UpdatedBy = "system";
            stockOpname.UpdatedAt = now;
        }

        stockOpname.OpnameDate = new DateOnly(2026, 1, 20);
        stockOpname.WarehouseId = warehouseMain.Id;
        stockOpname.LocationId = locationMainA2.Id;
        stockOpname.Description = "Seeded stock opname";
        stockOpname.Status = OpnameStatus.Draft;
        stockOpname.IsDeleted = false;
        stockOpname.DeletedAt = null;

        stockOpname.Lines.Add(new InvStockOpnameLine
        {
            LineNo = 1,
            ItemId = itemBolt.Id,
            LocationId = locationMainA2.Id,
            QtySystem = 1200m,
            QtyCounted = 1188m,
            QtyVariance = -12m,
            UnitCost = 1500m,
            TotalVarianceValue = -18_000m,
            Notes = "Seeded line"
        });

        await dbContext.SaveChangesAsync(ct);

        var stockMainA1 = await dbContext.InvStockBalances
            .FirstOrDefaultAsync(x => x.ItemId == itemSteelPlate.Id && x.WarehouseId == warehouseMain.Id && x.LocationId == locationMainA1.Id, ct);
        if (stockMainA1 is not null)
        {
            stockMainA1.LastMovementAt = new DateTimeOffset(new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc));
        }

        var stockMainA2 = await dbContext.InvStockBalances
            .FirstOrDefaultAsync(x => x.ItemId == itemBolt.Id && x.WarehouseId == warehouseMain.Id && x.LocationId == locationMainA2.Id, ct);
        if (stockMainA2 is not null)
        {
            stockMainA2.LastMovementAt = new DateTimeOffset(new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc));
        }

        var stockTransitT1 = await dbContext.InvStockBalances
            .FirstOrDefaultAsync(x => x.ItemId == itemBearing.Id && x.WarehouseId == warehouseTransit.Id && x.LocationId == locationTransit01.Id, ct);
        if (stockTransitT1 is not null)
        {
            stockTransitT1.LastMovementAt = new DateTimeOffset(new DateTime(2025, 8, 20, 0, 0, 0, DateTimeKind.Utc));
        }

        await dbContext.SaveChangesAsync(ct);

        await EnsureInvStockMovementSeedAsync(
            movementDate: new DateOnly(2026, 1, 5),
            itemId: itemSteelPlate.Id,
            warehouseId: warehouseMain.Id,
            locationId: locationMainA1.Id,
            movementType: StockMovementType.GoodsReceipt,
            qtyIn: 120m,
            qtyOut: 0m,
            qtyBalance: 620m,
            unitCost: 18000m,
            sourceTable: "inv_goods_receipts",
            sourceId: goodsReceipt.Id,
            sourceLineId: goodsReceipt.Lines.FirstOrDefault()?.Id,
            notes: "Seeded movement",
            ct: ct);

        await EnsureInvStockMovementSeedAsync(
            movementDate: new DateOnly(2026, 1, 8),
            itemId: itemBolt.Id,
            warehouseId: warehouseMain.Id,
            locationId: locationMainA2.Id,
            movementType: StockMovementType.GoodsIssue,
            qtyIn: 0m,
            qtyOut: 80m,
            qtyBalance: 1120m,
            unitCost: 1500m,
            sourceTable: "inv_goods_issues",
            sourceId: goodsIssue.Id,
            sourceLineId: goodsIssue.Lines.FirstOrDefault()?.Id,
            notes: "Seeded movement",
            ct: ct);

        await EnsureInvStockMovementSeedAsync(
            movementDate: new DateOnly(2026, 1, 12),
            itemId: itemBolt.Id,
            warehouseId: warehouseMain.Id,
            locationId: locationMainA2.Id,
            movementType: StockMovementType.TransferOut,
            qtyIn: 0m,
            qtyOut: 50m,
            qtyBalance: 1070m,
            unitCost: 1500m,
            sourceTable: "inv_stock_transfers",
            sourceId: stockTransfer.Id,
            sourceLineId: stockTransfer.Lines.FirstOrDefault()?.Id,
            notes: "Seeded movement",
            ct: ct);

        await EnsureInvStockMovementSeedAsync(
            movementDate: new DateOnly(2026, 1, 12),
            itemId: itemBolt.Id,
            warehouseId: warehouseTransit.Id,
            locationId: locationTransit01.Id,
            movementType: StockMovementType.TransferIn,
            qtyIn: 50m,
            qtyOut: 0m,
            qtyBalance: 130m,
            unitCost: 1500m,
            sourceTable: "inv_stock_transfers",
            sourceId: stockTransfer.Id,
            sourceLineId: stockTransfer.Lines.FirstOrDefault()?.Id,
            notes: "Seeded movement",
            ct: ct);

        await EnsureInvStockMovementSeedAsync(
            movementDate: new DateOnly(2026, 1, 15),
            itemId: itemBolt.Id,
            warehouseId: warehouseMain.Id,
            locationId: locationMainA2.Id,
            movementType: StockMovementType.AdjustmentOut,
            qtyIn: 0m,
            qtyOut: 10m,
            qtyBalance: 1110m,
            unitCost: 1500m,
            sourceTable: "inv_stock_adjustments",
            sourceId: stockAdjustment.Id,
            sourceLineId: stockAdjustment.Lines.FirstOrDefault()?.Id,
            notes: "Seeded movement",
            ct: ct);
    }

    private async Task EnsureInvStockMovementSeedAsync(
        DateOnly movementDate,
        int itemId,
        int warehouseId,
        int? locationId,
        StockMovementType movementType,
        decimal qtyIn,
        decimal qtyOut,
        decimal qtyBalance,
        decimal unitCost,
        string sourceTable,
        int sourceId,
        int? sourceLineId,
        string? notes,
        CancellationToken ct)
    {
        var existing = await dbContext.InvStockMovements
            .FirstOrDefaultAsync(x => x.SourceTable == sourceTable
                && x.SourceId == sourceId
                && x.SourceLineId == sourceLineId
                && x.MovementType == movementType
                && x.ItemId == itemId
                && x.WarehouseId == warehouseId
                && x.LocationId == locationId, ct);

        if (existing is null)
        {
            existing = new InvStockMovement
            {
                CreatedBy = "system",
                CreatedAt = DateTimeOffset.UtcNow
            };

            dbContext.InvStockMovements.Add(existing);
        }

        existing.MovementDate = movementDate;
        existing.ItemId = itemId;
        existing.WarehouseId = warehouseId;
        existing.LocationId = locationId;
        existing.MovementType = movementType;
        existing.QtyIn = decimal.Round(qtyIn, 4, MidpointRounding.AwayFromZero);
        existing.QtyOut = decimal.Round(qtyOut, 4, MidpointRounding.AwayFromZero);
        existing.QtyBalance = decimal.Round(qtyBalance, 4, MidpointRounding.AwayFromZero);
        existing.UnitCost = decimal.Round(unitCost, 4, MidpointRounding.AwayFromZero);
        existing.TotalCost = decimal.Round((qtyIn - qtyOut) * unitCost, 4, MidpointRounding.AwayFromZero);
        existing.SourceTable = sourceTable;
        existing.SourceId = sourceId;
        existing.SourceLineId = sourceLineId;
        existing.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

        await dbContext.SaveChangesAsync(ct);
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

        var invModule = await dbContext.CfgModules
            .IgnoreQueryFilters()
            .FirstAsync(x => x.Code == "INV", ct);

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

        var finBudgetControl = await EnsureMenuAsync(finModule.Id, null, "Budget & Cost Control", null, "bi-calculator", 6, now, ct);
        await EnsureMenuAsync(finModule.Id, finBudgetControl.Id, "Budgets", "/finance/budgets", "bi-wallet", 1, now, ct);
        await EnsureMenuAsync(finModule.Id, finBudgetControl.Id, "Budget vs Actual", "/finance/reports/budget-vs-actual", "bi-bar-chart", 2, now, ct);

        var finReports = await EnsureMenuAsync(finModule.Id, null, "Financial Reports", null, "bi-bar-chart-line", 7, now, ct);
        await EnsureMenuAsync(finModule.Id, finReports.Id, "Trial Balance", "/finance/reports/trial-balance", "bi-table", 1, now, ct);
        await EnsureMenuAsync(finModule.Id, finReports.Id, "Balance Sheet", "/finance/reports/balance-sheet", "bi-border-all", 2, now, ct);
        await EnsureMenuAsync(finModule.Id, finReports.Id, "Profit & Loss", "/finance/reports/profit-loss", "bi-graph-up-arrow", 3, now, ct);
        await EnsureMenuAsync(finModule.Id, finReports.Id, "Cash Flow", "/finance/reports/cash-flow", "bi-water", 4, now, ct);

        var finFinalization = await EnsureMenuAsync(finModule.Id, null, "Finance Finalization", null, "bi-check2-square", 8, now, ct);
        await EnsureMenuAsync(finModule.Id, finFinalization.Id, "Period Closing", "/finance/finalization/period-closing", "bi-calendar-check", 1, now, ct);
        await EnsureMenuAsync(finModule.Id, finFinalization.Id, "Smoke Tests", "/finance/finalization/smoke-tests", "bi-clipboard-check", 2, now, ct);

        var invMaster = await EnsureMenuAsync(invModule.Id, null, "Inventory Master", null, "bi-boxes", 1, now, ct);
        await EnsureMenuAsync(invModule.Id, invMaster.Id, "Item Categories", "/inventory/categories", "bi-diagram-2", 1, now, ct);
        await EnsureMenuAsync(invModule.Id, invMaster.Id, "Units of Measure", "/inventory/units", "bi-rulers", 2, now, ct);
        await EnsureMenuAsync(invModule.Id, invMaster.Id, "Brands", "/inventory/brands", "bi-tags", 3, now, ct);
        await EnsureMenuAsync(invModule.Id, invMaster.Id, "Item Conversions", "/inventory/item-conversions", "bi-arrow-left-right", 4, now, ct);
        await EnsureMenuAsync(invModule.Id, invMaster.Id, "Items", "/inventory/items", "bi-box-seam", 5, now, ct);

        var invWarehouse = await EnsureMenuAsync(invModule.Id, null, "Warehouse & Stock", null, "bi-building", 2, now, ct);
        await EnsureMenuAsync(invModule.Id, invWarehouse.Id, "Warehouses", "/inventory/warehouses", "bi-house-gear", 1, now, ct);

        var invTransactions = await EnsureMenuAsync(invModule.Id, null, "Inventory Transactions", null, "bi-arrow-left-right", 3, now, ct);
        await EnsureMenuAsync(invModule.Id, invTransactions.Id, "Goods Receipts", "/inventory/goods-receipts", "bi-box-arrow-in-down", 1, now, ct);
        await EnsureMenuAsync(invModule.Id, invTransactions.Id, "Goods Issues", "/inventory/goods-issues", "bi-box-arrow-up", 2, now, ct);
        await EnsureMenuAsync(invModule.Id, invTransactions.Id, "Stock Transfers", "/inventory/transfers", "bi-arrow-left-right", 3, now, ct);
        await EnsureMenuAsync(invModule.Id, invTransactions.Id, "Stock Adjustments", "/inventory/adjustments", "bi-sliders2-vertical", 4, now, ct);
        await EnsureMenuAsync(invModule.Id, invTransactions.Id, "Stock Opnames", "/inventory/opnames", "bi-clipboard2-check", 5, now, ct);

        var invReports = await EnsureMenuAsync(invModule.Id, null, "Inventory Reports", null, "bi-bar-chart-line", 4, now, ct);
        await EnsureMenuAsync(invModule.Id, invReports.Id, "Stock Summary", "/inventory/reports/stock-summary", "bi-table", 1, now, ct);
        await EnsureMenuAsync(invModule.Id, invReports.Id, "Movement History", "/inventory/reports/movement-history", "bi-activity", 2, now, ct);
        await EnsureMenuAsync(invModule.Id, invReports.Id, "Low Stock Report", "/inventory/reports/low-stock", "bi-exclamation-triangle", 3, now, ct);
        await EnsureMenuAsync(invModule.Id, invReports.Id, "Inventory Valuation", "/inventory/reports/inventory-valuation", "bi-currency-dollar", 4, now, ct);
        await EnsureMenuAsync(invModule.Id, invReports.Id, "Inventory Aging", "/inventory/reports/inventory-aging", "bi-hourglass-split", 5, now, ct);
        await EnsureMenuAsync(invModule.Id, invReports.Id, "Receipt Summary", "/inventory/reports/receipt-summary", "bi-journal-text", 6, now, ct);
        await EnsureMenuAsync(invModule.Id, invReports.Id, "Issue Summary", "/inventory/reports/issue-summary", "bi-journal-minus", 7, now, ct);
        await EnsureMenuAsync(invModule.Id, invReports.Id, "Transfer Summary", "/inventory/reports/transfer-summary", "bi-arrows-move", 8, now, ct);
        await EnsureMenuAsync(invModule.Id, invReports.Id, "Adjustment Summary", "/inventory/reports/adjustment-summary", "bi-journal-check", 9, now, ct);
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

    private async Task SeedInventoryRolePermissionsAsync(CancellationToken ct)
    {
        var inventoryModule = await dbContext.CfgModules
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Code == "INV", ct);

        if (inventoryModule is null)
        {
            return;
        }

        var inventoryMenus = await dbContext.CfgMenus
            .IgnoreQueryFilters()
            .Where(x => x.ModuleId == inventoryModule.Id)
            .ToListAsync(ct);

        if (inventoryMenus.Count == 0)
        {
            return;
        }

        var allInventoryMenuIds = inventoryMenus.Select(x => x.Id).ToHashSet();
        var menuById = inventoryMenus.ToDictionary(x => x.Id);

        HashSet<int> ExpandWithAncestors(IEnumerable<int> ids)
        {
            var result = new HashSet<int>();
            foreach (var id in ids)
            {
                var currentId = id;
                while (currentId > 0 && result.Add(currentId))
                {
                    if (!menuById.TryGetValue(currentId, out var menu) || !menu.ParentId.HasValue)
                    {
                        break;
                    }

                    currentId = menu.ParentId.Value;
                }
            }

            return result;
        }

        var reportRoot = inventoryMenus.FirstOrDefault(x => x.ParentId is null && x.Name == "Inventory Reports");
        var reportMenuIds = reportRoot is null
            ? new HashSet<int>()
            : inventoryMenus
                .Where(x => x.Id == reportRoot.Id || x.ParentId == reportRoot.Id)
                .Select(x => x.Id)
                .ToHashSet();

        var goodsIssueMenuIds = inventoryMenus
            .Where(x => x.Name == "Goods Issues")
            .Select(x => x.Id)
            .ToHashSet();

        var roles = await dbContext.CfgRoles
            .IgnoreQueryFilters()
            .Where(x =>
                x.Name == "Inventory Manager" ||
                x.Name == "Gudang Staff" ||
                x.Name == "Finance Staff" ||
                x.Name == "HR Staff" ||
                x.Name == "Employee")
            .ToDictionaryAsync(x => x.Name, StringComparer.OrdinalIgnoreCase, ct);

        if (roles.TryGetValue("Inventory Manager", out var inventoryManager))
        {
            await ApplyInventoryRolePermissionsAsync(
                inventoryManager.Id,
                inventoryMenus,
                allInventoryMenuIds,
                allInventoryMenuIds,
                allInventoryMenuIds,
                allInventoryMenuIds,
                ct);
        }

        if (roles.TryGetValue("Gudang Staff", out var warehouseStaff))
        {
            var editableMenuIds = allInventoryMenuIds.Except(reportMenuIds).ToHashSet();
            await ApplyInventoryRolePermissionsAsync(
                warehouseStaff.Id,
                inventoryMenus,
                allInventoryMenuIds,
                editableMenuIds,
                editableMenuIds,
                new HashSet<int>(),
                ct);
        }

        if (roles.TryGetValue("Finance Staff", out var financeStaff))
        {
            var reportReadMenus = ExpandWithAncestors(reportMenuIds);
            await ApplyInventoryRolePermissionsAsync(
                financeStaff.Id,
                inventoryMenus,
                reportReadMenus,
                new HashSet<int>(),
                new HashSet<int>(),
                new HashSet<int>(),
                ct);
        }

        if (roles.TryGetValue("HR Staff", out var hrStaff))
        {
            var goodsIssueReadMenus = ExpandWithAncestors(goodsIssueMenuIds);
            await ApplyInventoryRolePermissionsAsync(
                hrStaff.Id,
                inventoryMenus,
                goodsIssueReadMenus,
                goodsIssueMenuIds,
                goodsIssueMenuIds,
                new HashSet<int>(),
                ct);
        }

        if (roles.TryGetValue("Employee", out var employee))
        {
            await ApplyInventoryRolePermissionsAsync(
                employee.Id,
                inventoryMenus,
                new HashSet<int>(),
                new HashSet<int>(),
                new HashSet<int>(),
                new HashSet<int>(),
                ct);
        }
    }

    private async Task ApplyInventoryRolePermissionsAsync(
        int roleId,
        IReadOnlyCollection<CfgMenu> inventoryMenus,
        ISet<int> canViewMenuIds,
        ISet<int> canCreateMenuIds,
        ISet<int> canEditMenuIds,
        ISet<int> canDeleteMenuIds,
        CancellationToken ct)
    {
        var menuIds = inventoryMenus.Select(x => x.Id).ToList();

        var existingPermissions = await dbContext.CfgRoleMenuPermissions
            .Where(x => x.RoleId == roleId && menuIds.Contains(x.MenuId))
            .ToListAsync(ct);

        var permissionByMenuId = existingPermissions.ToDictionary(x => x.MenuId);

        foreach (var menu in inventoryMenus)
        {
            if (!permissionByMenuId.TryGetValue(menu.Id, out var permission))
            {
                permission = new CfgRoleMenuPermission
                {
                    RoleId = roleId,
                    MenuId = menu.Id
                };

                dbContext.CfgRoleMenuPermissions.Add(permission);
                permissionByMenuId[menu.Id] = permission;
            }

            permission.CanView = canViewMenuIds.Contains(menu.Id);
            permission.CanCreate = canCreateMenuIds.Contains(menu.Id);
            permission.CanEdit = canEditMenuIds.Contains(menu.Id);
            permission.CanDelete = canDeleteMenuIds.Contains(menu.Id);
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

    private async Task EnsureSampleFinanceBudgetsAsync(
        int fiscalYearId,
        int fallbackPeriodId,
        int? costCenterId,
        int salaryExpenseAccountId,
        int operationalExpenseAccountId,
        int depreciationExpenseAccountId,
        DateTimeOffset now,
        CancellationToken ct)
    {
        var periodIds = await dbContext.FinPeriods
            .AsNoTracking()
            .Where(x => x.FiscalYearId == fiscalYearId)
            .OrderByDescending(x => x.StartDate)
            .Select(x => x.Id)
            .Take(3)
            .ToListAsync(ct);

        if (periodIds.Count == 0)
        {
            periodIds.Add(fallbackPeriodId);
        }

        var periodPrimary = periodIds[0];
        var periodSecondary = periodIds.Count > 1 ? periodIds[1] : periodPrimary;
        var periodTertiary = periodIds.Count > 2 ? periodIds[2] : periodPrimary;
        var budgetYear = DateTime.UtcNow.Year;

        await EnsureBudgetAsync(
            budgetNo: $"BUD-{budgetYear}-OPS01",
            name: $"Operational Budget {budgetYear}",
            fiscalYearId: fiscalYearId,
            periodId: null,
            costCenterId: costCenterId,
            accountId: operationalExpenseAccountId,
            currencyCode: "IDR",
            isActive: true,
            notes: "Budget operasional tahunan",
            lines:
            [
                new SeedBudgetLine
                {
                    PeriodId = periodPrimary,
                    AccountId = operationalExpenseAccountId,
                    CostCenterId = costCenterId,
                    Description = "Alokasi biaya operasional",
                    Amount = 9_500_000m
                },
                new SeedBudgetLine
                {
                    PeriodId = periodSecondary,
                    AccountId = operationalExpenseAccountId,
                    CostCenterId = costCenterId,
                    Description = "Alokasi biaya operasional",
                    Amount = 8_900_000m
                },
                new SeedBudgetLine
                {
                    PeriodId = periodPrimary,
                    AccountId = depreciationExpenseAccountId,
                    CostCenterId = null,
                    Description = "Alokasi biaya penyusutan",
                    Amount = 1_200_000m
                }
            ],
            now,
            ct);

        await EnsureBudgetAsync(
            budgetNo: $"BUD-{budgetYear}-PAY01",
            name: $"Payroll Budget {budgetYear}",
            fiscalYearId: fiscalYearId,
            periodId: null,
            costCenterId: costCenterId,
            accountId: salaryExpenseAccountId,
            currencyCode: "IDR",
            isActive: true,
            notes: "Budget payroll tahunan",
            lines:
            [
                new SeedBudgetLine
                {
                    PeriodId = periodPrimary,
                    AccountId = salaryExpenseAccountId,
                    CostCenterId = costCenterId,
                    Description = "Alokasi biaya payroll",
                    Amount = 16_000_000m
                },
                new SeedBudgetLine
                {
                    PeriodId = periodSecondary,
                    AccountId = salaryExpenseAccountId,
                    CostCenterId = costCenterId,
                    Description = "Alokasi biaya payroll",
                    Amount = 15_500_000m
                },
                new SeedBudgetLine
                {
                    PeriodId = periodTertiary,
                    AccountId = salaryExpenseAccountId,
                    CostCenterId = costCenterId,
                    Description = "Alokasi biaya payroll",
                    Amount = 15_800_000m
                }
            ],
            now,
            ct);
    }

    private async Task<FinBudget> EnsureBudgetAsync(
        string budgetNo,
        string name,
        int fiscalYearId,
        int? periodId,
        int? costCenterId,
        int? accountId,
        string currencyCode,
        bool isActive,
        string? notes,
        IReadOnlyList<SeedBudgetLine> lines,
        DateTimeOffset now,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(budgetNo))
        {
            throw new InvalidOperationException("Budget number is required.");
        }

        if (lines.Count == 0)
        {
            throw new InvalidOperationException("Budget lines are required.");
        }

        var normalizedBudgetNo = budgetNo.Trim().ToUpperInvariant();
        var normalizedName = string.IsNullOrWhiteSpace(name) ? normalizedBudgetNo : name.Trim();
        var normalizedCurrencyCode = string.IsNullOrWhiteSpace(currencyCode) ? "IDR" : currencyCode.Trim().ToUpperInvariant();

        var existing = await dbContext.FinBudgets
            .IgnoreQueryFilters()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.BudgetNo == normalizedBudgetNo, ct);

        if (existing is null)
        {
            existing = new FinBudget
            {
                BudgetNo = normalizedBudgetNo,
                Name = normalizedName,
                FiscalYearId = fiscalYearId,
                PeriodId = periodId,
                CostCenterId = costCenterId,
                AccountId = accountId,
                CurrencyCode = normalizedCurrencyCode,
                Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
                IsActive = isActive,
                CreatedBy = "system",
                CreatedAt = now
            };

            dbContext.FinBudgets.Add(existing);
        }
        else
        {
            dbContext.FinBudgetLines.RemoveRange(existing.Lines);
            existing.Lines.Clear();

            existing.Name = normalizedName;
            existing.FiscalYearId = fiscalYearId;
            existing.PeriodId = periodId;
            existing.CostCenterId = costCenterId;
            existing.AccountId = accountId;
            existing.CurrencyCode = normalizedCurrencyCode;
            existing.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
            existing.IsActive = isActive;
            existing.IsDeleted = false;
            existing.DeletedAt = null;
            existing.UpdatedBy = "system";
            existing.UpdatedAt = now;
        }

        for (var indexLine = 0; indexLine < lines.Count; indexLine++)
        {
            var line = lines[indexLine];

            if (line.PeriodId <= 0)
            {
                throw new InvalidOperationException($"Budget line {indexLine + 1} period is invalid.");
            }

            if (line.AccountId <= 0)
            {
                throw new InvalidOperationException($"Budget line {indexLine + 1} account is invalid.");
            }

            if (line.Amount < 0)
            {
                throw new InvalidOperationException($"Budget line {indexLine + 1} amount cannot be negative.");
            }

            existing.Lines.Add(new FinBudgetLine
            {
                LineNo = indexLine + 1,
                PeriodId = line.PeriodId,
                AccountId = line.AccountId,
                CostCenterId = line.CostCenterId,
                Description = string.IsNullOrWhiteSpace(line.Description) ? null : line.Description.Trim(),
                Amount = decimal.Round(line.Amount, 4, MidpointRounding.AwayFromZero)
            });
        }

        existing.TotalAmount = decimal.Round(existing.Lines.Sum(x => x.Amount), 4, MidpointRounding.AwayFromZero);

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

    private sealed class SeedBudgetLine
    {
        public int PeriodId { get; init; }
        public int AccountId { get; init; }
        public int? CostCenterId { get; init; }
        public string? Description { get; init; }
        public decimal Amount { get; init; }
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









































