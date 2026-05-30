using System;
using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260530173000_AddFinancePhase9JournalLedger")]
public partial class AddFinancePhase9JournalLedger : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "fin_journal_entries",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                journal_no = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                period_id = table.Column<int>(type: "integer", nullable: false),
                date = table.Column<DateOnly>(type: "date", nullable: false),
                description = table.Column<string>(type: "text", nullable: false),
                source = table.Column<int>(type: "integer", nullable: false),
                source_ref_id = table.Column<int>(type: "integer", nullable: true),
                source_ref_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                posted_by = table.Column<int>(type: "integer", nullable: true),
                posted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                reversed_journal_id = table.Column<int>(type: "integer", nullable: true),
                currency_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "IDR"),
                exchange_rate = table.Column<decimal>(type: "numeric(18,6)", nullable: false, defaultValue: 1m),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_fin_journal_entries", x => x.id);
                table.ForeignKey(
                    name: "fk_fin_journal_entries_fin_currencies_currency_code",
                    column: x => x.currency_code,
                    principalSchema: "public",
                    principalTable: "fin_currencies",
                    principalColumn: "code",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_fin_journal_entries_fin_journal_entries_reversed_journal_id",
                    column: x => x.reversed_journal_id,
                    principalSchema: "public",
                    principalTable: "fin_journal_entries",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_fin_journal_entries_fin_periods_period_id",
                    column: x => x.period_id,
                    principalSchema: "public",
                    principalTable: "fin_periods",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_fin_journal_entries_sys_users_posted_by",
                    column: x => x.posted_by,
                    principalSchema: "public",
                    principalTable: "sys_users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "fin_journal_entry_lines",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                journal_entry_id = table.Column<int>(type: "integer", nullable: false),
                line_no = table.Column<int>(type: "integer", nullable: false),
                account_id = table.Column<int>(type: "integer", nullable: false),
                cost_center_id = table.Column<int>(type: "integer", nullable: true),
                description = table.Column<string>(type: "text", nullable: true),
                debit = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                credit = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                debit_base = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                credit_base = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_fin_journal_entry_lines", x => x.id);
                table.CheckConstraint("ck_fin_journal_entry_lines_non_negative", "debit >= 0 AND credit >= 0");
                table.CheckConstraint("ck_fin_journal_entry_lines_single_side", "NOT (debit > 0 AND credit > 0)");
                table.ForeignKey(
                    name: "fk_fin_journal_entry_lines_fin_accounts_account_id",
                    column: x => x.account_id,
                    principalSchema: "public",
                    principalTable: "fin_accounts",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_fin_journal_entry_lines_fin_cost_centers_cost_center_id",
                    column: x => x.cost_center_id,
                    principalSchema: "public",
                    principalTable: "fin_cost_centers",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "fk_fin_journal_entry_lines_fin_journal_entries_journal_entry_id",
                    column: x => x.journal_entry_id,
                    principalSchema: "public",
                    principalTable: "fin_journal_entries",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_fin_journal_entries_created_at",
            schema: "public",
            table: "fin_journal_entries",
            column: "created_at");

        migrationBuilder.CreateIndex(
            name: "ix_fin_journal_entries_currency_code",
            schema: "public",
            table: "fin_journal_entries",
            column: "currency_code");

        migrationBuilder.CreateIndex(
            name: "ix_fin_journal_entries_date",
            schema: "public",
            table: "fin_journal_entries",
            column: "date");

        migrationBuilder.CreateIndex(
            name: "ix_fin_journal_entries_is_deleted",
            schema: "public",
            table: "fin_journal_entries",
            column: "is_deleted");

        migrationBuilder.CreateIndex(
            name: "ix_fin_journal_entries_journal_no",
            schema: "public",
            table: "fin_journal_entries",
            column: "journal_no",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_fin_journal_entries_period_id",
            schema: "public",
            table: "fin_journal_entries",
            column: "period_id");

        migrationBuilder.CreateIndex(
            name: "ix_fin_journal_entries_posted_by",
            schema: "public",
            table: "fin_journal_entries",
            column: "posted_by");

        migrationBuilder.CreateIndex(
            name: "ix_fin_journal_entries_reversed_journal_id",
            schema: "public",
            table: "fin_journal_entries",
            column: "reversed_journal_id");

        migrationBuilder.CreateIndex(
            name: "ix_fin_journal_entries_source",
            schema: "public",
            table: "fin_journal_entries",
            column: "source");

        migrationBuilder.CreateIndex(
            name: "ix_fin_journal_entries_source_source_ref_id_source_ref_type",
            schema: "public",
            table: "fin_journal_entries",
            columns: new[] { "source", "source_ref_id", "source_ref_type" });

        migrationBuilder.CreateIndex(
            name: "ix_fin_journal_entries_status",
            schema: "public",
            table: "fin_journal_entries",
            column: "status");

        migrationBuilder.CreateIndex(
            name: "ix_fin_journal_entry_lines_account_id",
            schema: "public",
            table: "fin_journal_entry_lines",
            column: "account_id");

        migrationBuilder.CreateIndex(
            name: "ix_fin_journal_entry_lines_cost_center_id",
            schema: "public",
            table: "fin_journal_entry_lines",
            column: "cost_center_id");

        migrationBuilder.CreateIndex(
            name: "ix_fin_journal_entry_lines_journal_entry_id",
            schema: "public",
            table: "fin_journal_entry_lines",
            column: "journal_entry_id");

        migrationBuilder.CreateIndex(
            name: "ix_fin_journal_entry_lines_journal_entry_id_line_no",
            schema: "public",
            table: "fin_journal_entry_lines",
            columns: new[] { "journal_entry_id", "line_no" },
            unique: true);

        migrationBuilder.Sql(@"
CREATE OR REPLACE VIEW public.fin_general_ledger AS
SELECT
    l.id AS line_id,
    l.journal_entry_id,
    l.account_id,
    a.code AS account_code,
    a.name AS account_name,
    j.date,
    j.journal_no,
    COALESCE(NULLIF(BTRIM(l.description), ''), j.description) AS description,
    l.debit_base AS debit,
    l.credit_base AS credit,
    SUM(l.debit_base - l.credit_base) OVER (
        PARTITION BY l.account_id
        ORDER BY j.date, j.journal_no, l.line_no, l.id
        ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
    ) AS balance,
    j.period_id,
    l.cost_center_id,
    j.source,
    j.status
FROM public.fin_journal_entry_lines l
INNER JOIN public.fin_journal_entries j ON j.id = l.journal_entry_id
INNER JOIN public.fin_accounts a ON a.id = l.account_id
WHERE NOT j.is_deleted
  AND j.status <> 0;
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP VIEW IF EXISTS public.fin_general_ledger;");

        migrationBuilder.DropTable(
            name: "fin_journal_entry_lines",
            schema: "public");

        migrationBuilder.DropTable(
            name: "fin_journal_entries",
            schema: "public");
    }
}
