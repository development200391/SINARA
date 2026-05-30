using ERP.Infrastructure.Data;
using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260530130000_AddFinancePhase8Foundation")]
public partial class AddFinancePhase8Foundation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "fin_account_groups",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                type = table.Column<int>(type: "integer", nullable: false),
                normal_balance = table.Column<int>(type: "integer", nullable: false),
                parent_group_id = table.Column<int>(type: "integer", nullable: true),
                sort_order = table.Column<int>(type: "integer", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_fin_account_groups", x => x.id);
                table.ForeignKey(
                    name: "fk_fin_account_groups_fin_account_groups_parent_group_id",
                    column: x => x.parent_group_id,
                    principalSchema: "public",
                    principalTable: "fin_account_groups",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "fin_currencies",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                symbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                is_base_currency = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_fin_currencies", x => x.id);
                table.UniqueConstraint("ak_fin_currencies_code", x => x.code);
            });

        migrationBuilder.CreateTable(
            name: "fin_fiscal_years",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                start_date = table.Column<DateOnly>(type: "date", nullable: false),
                end_date = table.Column<DateOnly>(type: "date", nullable: false),
                status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_fin_fiscal_years", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "fin_exchange_rates",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                from_currency_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                to_currency_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                rate = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                effective_date = table.Column<DateOnly>(type: "date", nullable: false),
                source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "system"),
                created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_fin_exchange_rates", x => x.id);
                table.ForeignKey(
                    name: "fk_fin_exchange_rates_fin_currencies_from_currency_code",
                    column: x => x.from_currency_code,
                    principalSchema: "public",
                    principalTable: "fin_currencies",
                    principalColumn: "code",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_fin_exchange_rates_fin_currencies_to_currency_code",
                    column: x => x.to_currency_code,
                    principalSchema: "public",
                    principalTable: "fin_currencies",
                    principalColumn: "code",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "fin_accounts",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                group_id = table.Column<int>(type: "integer", nullable: false),
                type = table.Column<int>(type: "integer", nullable: false),
                normal_balance = table.Column<int>(type: "integer", nullable: false),
                is_header = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                parent_account_id = table.Column<int>(type: "integer", nullable: true),
                description = table.Column<string>(type: "text", nullable: true),
                is_bank_account = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                bank_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                bank_account_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                currency_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "IDR"),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_fin_accounts", x => x.id);
                table.ForeignKey(
                    name: "fk_fin_accounts_fin_account_groups_group_id",
                    column: x => x.group_id,
                    principalSchema: "public",
                    principalTable: "fin_account_groups",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_fin_accounts_fin_accounts_parent_account_id",
                    column: x => x.parent_account_id,
                    principalSchema: "public",
                    principalTable: "fin_accounts",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_fin_accounts_fin_currencies_currency_code",
                    column: x => x.currency_code,
                    principalSchema: "public",
                    principalTable: "fin_currencies",
                    principalColumn: "code",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "fin_periods",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                fiscal_year_id = table.Column<int>(type: "integer", nullable: false),
                period_number = table.Column<int>(type: "integer", nullable: false),
                name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                start_date = table.Column<DateOnly>(type: "date", nullable: false),
                end_date = table.Column<DateOnly>(type: "date", nullable: false),
                status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_fin_periods", x => x.id);
                table.ForeignKey(
                    name: "fk_fin_periods_fin_fiscal_years_fiscal_year_id",
                    column: x => x.fiscal_year_id,
                    principalSchema: "public",
                    principalTable: "fin_fiscal_years",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "fin_cost_centers",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                department_id = table.Column<int>(type: "integer", nullable: true),
                manager_id = table.Column<int>(type: "integer", nullable: true),
                budget_account_id = table.Column<int>(type: "integer", nullable: true),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_fin_cost_centers", x => x.id);
                table.ForeignKey(
                    name: "fk_fin_cost_centers_fin_accounts_budget_account_id",
                    column: x => x.budget_account_id,
                    principalSchema: "public",
                    principalTable: "fin_accounts",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "fk_fin_cost_centers_hr_departments_department_id",
                    column: x => x.department_id,
                    principalSchema: "public",
                    principalTable: "hr_departments",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "fk_fin_cost_centers_hr_employees_manager_id",
                    column: x => x.manager_id,
                    principalSchema: "public",
                    principalTable: "hr_employees",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "fin_tax_codes",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                type = table.Column<int>(type: "integer", nullable: false),
                rate = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                is_inclusive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                account_id = table.Column<int>(type: "integer", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_fin_tax_codes", x => x.id);
                table.ForeignKey(
                    name: "fk_fin_tax_codes_fin_accounts_account_id",
                    column: x => x.account_id,
                    principalSchema: "public",
                    principalTable: "fin_accounts",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_fin_account_groups_code",
            schema: "public",
            table: "fin_account_groups",
            column: "code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_fin_account_groups_created_at",
            schema: "public",
            table: "fin_account_groups",
            column: "created_at");

        migrationBuilder.CreateIndex(
            name: "ix_fin_account_groups_is_active",
            schema: "public",
            table: "fin_account_groups",
            column: "is_active");

        migrationBuilder.CreateIndex(
            name: "ix_fin_account_groups_is_deleted",
            schema: "public",
            table: "fin_account_groups",
            column: "is_deleted");

        migrationBuilder.CreateIndex(
            name: "ix_fin_account_groups_parent_group_id",
            schema: "public",
            table: "fin_account_groups",
            column: "parent_group_id");

        migrationBuilder.CreateIndex(
            name: "ix_fin_account_groups_sort_order",
            schema: "public",
            table: "fin_account_groups",
            column: "sort_order");

        migrationBuilder.CreateIndex(
            name: "ix_fin_account_groups_type",
            schema: "public",
            table: "fin_account_groups",
            column: "type");

        migrationBuilder.CreateIndex(
            name: "ix_fin_accounts_code",
            schema: "public",
            table: "fin_accounts",
            column: "code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_fin_accounts_created_at",
            schema: "public",
            table: "fin_accounts",
            column: "created_at");

        migrationBuilder.CreateIndex(
            name: "ix_fin_accounts_currency_code",
            schema: "public",
            table: "fin_accounts",
            column: "currency_code");

        migrationBuilder.CreateIndex(
            name: "ix_fin_accounts_group_id",
            schema: "public",
            table: "fin_accounts",
            column: "group_id");

        migrationBuilder.CreateIndex(
            name: "ix_fin_accounts_is_active",
            schema: "public",
            table: "fin_accounts",
            column: "is_active");

        migrationBuilder.CreateIndex(
            name: "ix_fin_accounts_is_bank_account",
            schema: "public",
            table: "fin_accounts",
            column: "is_bank_account");

        migrationBuilder.CreateIndex(
            name: "ix_fin_accounts_is_deleted",
            schema: "public",
            table: "fin_accounts",
            column: "is_deleted");

        migrationBuilder.CreateIndex(
            name: "ix_fin_accounts_parent_account_id",
            schema: "public",
            table: "fin_accounts",
            column: "parent_account_id");

        migrationBuilder.CreateIndex(
            name: "ix_fin_accounts_type",
            schema: "public",
            table: "fin_accounts",
            column: "type");

        migrationBuilder.CreateIndex(
            name: "ix_fin_cost_centers_budget_account_id",
            schema: "public",
            table: "fin_cost_centers",
            column: "budget_account_id");

        migrationBuilder.CreateIndex(
            name: "ix_fin_cost_centers_code",
            schema: "public",
            table: "fin_cost_centers",
            column: "code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_fin_cost_centers_created_at",
            schema: "public",
            table: "fin_cost_centers",
            column: "created_at");

        migrationBuilder.CreateIndex(
            name: "ix_fin_cost_centers_department_id",
            schema: "public",
            table: "fin_cost_centers",
            column: "department_id");

        migrationBuilder.CreateIndex(
            name: "ix_fin_cost_centers_is_active",
            schema: "public",
            table: "fin_cost_centers",
            column: "is_active");

        migrationBuilder.CreateIndex(
            name: "ix_fin_cost_centers_is_deleted",
            schema: "public",
            table: "fin_cost_centers",
            column: "is_deleted");

        migrationBuilder.CreateIndex(
            name: "ix_fin_cost_centers_manager_id",
            schema: "public",
            table: "fin_cost_centers",
            column: "manager_id");


        migrationBuilder.CreateIndex(
            name: "ix_fin_currencies_created_at",
            schema: "public",
            table: "fin_currencies",
            column: "created_at");

        migrationBuilder.CreateIndex(
            name: "ix_fin_currencies_is_active",
            schema: "public",
            table: "fin_currencies",
            column: "is_active");

        migrationBuilder.CreateIndex(
            name: "ix_fin_currencies_is_base_currency",
            schema: "public",
            table: "fin_currencies",
            column: "is_base_currency");

        migrationBuilder.CreateIndex(
            name: "ix_fin_currencies_is_deleted",
            schema: "public",
            table: "fin_currencies",
            column: "is_deleted");

        migrationBuilder.CreateIndex(
            name: "ix_fin_exchange_rates_from_currency_code_effective_date",
            schema: "public",
            table: "fin_exchange_rates",
            columns: new[] { "from_currency_code", "effective_date" });

        migrationBuilder.CreateIndex(
            name: "ix_fin_exchange_rates_from_currency_code_to_currency_code_effective_date",
            schema: "public",
            table: "fin_exchange_rates",
            columns: new[] { "from_currency_code", "to_currency_code", "effective_date" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_fin_exchange_rates_to_currency_code_effective_date",
            schema: "public",
            table: "fin_exchange_rates",
            columns: new[] { "to_currency_code", "effective_date" });

        migrationBuilder.CreateIndex(
            name: "ix_fin_fiscal_years_created_at",
            schema: "public",
            table: "fin_fiscal_years",
            column: "created_at");

        migrationBuilder.CreateIndex(
            name: "ix_fin_fiscal_years_end_date",
            schema: "public",
            table: "fin_fiscal_years",
            column: "end_date");

        migrationBuilder.CreateIndex(
            name: "ix_fin_fiscal_years_is_deleted",
            schema: "public",
            table: "fin_fiscal_years",
            column: "is_deleted");

        migrationBuilder.CreateIndex(
            name: "ix_fin_fiscal_years_name",
            schema: "public",
            table: "fin_fiscal_years",
            column: "name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_fin_fiscal_years_start_date",
            schema: "public",
            table: "fin_fiscal_years",
            column: "start_date");

        migrationBuilder.CreateIndex(
            name: "ix_fin_fiscal_years_status",
            schema: "public",
            table: "fin_fiscal_years",
            column: "status");

        migrationBuilder.CreateIndex(
            name: "ix_fin_periods_created_at",
            schema: "public",
            table: "fin_periods",
            column: "created_at");

        migrationBuilder.CreateIndex(
            name: "ix_fin_periods_fiscal_year_id",
            schema: "public",
            table: "fin_periods",
            column: "fiscal_year_id");

        migrationBuilder.CreateIndex(
            name: "ix_fin_periods_fiscal_year_id_period_number",
            schema: "public",
            table: "fin_periods",
            columns: new[] { "fiscal_year_id", "period_number" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_fin_periods_is_deleted",
            schema: "public",
            table: "fin_periods",
            column: "is_deleted");

        migrationBuilder.CreateIndex(
            name: "ix_fin_periods_status",
            schema: "public",
            table: "fin_periods",
            column: "status");

        migrationBuilder.CreateIndex(
            name: "ix_fin_tax_codes_account_id",
            schema: "public",
            table: "fin_tax_codes",
            column: "account_id");

        migrationBuilder.CreateIndex(
            name: "ix_fin_tax_codes_code",
            schema: "public",
            table: "fin_tax_codes",
            column: "code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_fin_tax_codes_created_at",
            schema: "public",
            table: "fin_tax_codes",
            column: "created_at");

        migrationBuilder.CreateIndex(
            name: "ix_fin_tax_codes_is_active",
            schema: "public",
            table: "fin_tax_codes",
            column: "is_active");

        migrationBuilder.CreateIndex(
            name: "ix_fin_tax_codes_is_deleted",
            schema: "public",
            table: "fin_tax_codes",
            column: "is_deleted");

        migrationBuilder.CreateIndex(
            name: "ix_fin_tax_codes_type",
            schema: "public",
            table: "fin_tax_codes",
            column: "type");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "fin_cost_centers",
            schema: "public");

        migrationBuilder.DropTable(
            name: "fin_exchange_rates",
            schema: "public");

        migrationBuilder.DropTable(
            name: "fin_periods",
            schema: "public");

        migrationBuilder.DropTable(
            name: "fin_tax_codes",
            schema: "public");

        migrationBuilder.DropTable(
            name: "fin_fiscal_years",
            schema: "public");

        migrationBuilder.DropTable(
            name: "fin_accounts",
            schema: "public");

        migrationBuilder.DropTable(
            name: "fin_account_groups",
            schema: "public");

        migrationBuilder.DropTable(
            name: "fin_currencies",
            schema: "public");
    }
}




