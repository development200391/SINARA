using System;
using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260530190000_AddFinancePhase10AccountsPayable")]
public partial class AddFinancePhase10AccountsPayable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "fin_vendors",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                tax_id = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                address = table.Column<string>(type: "text", nullable: true),
                phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                contact_person = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                payment_terms_days = table.Column<int>(type: "integer", nullable: false, defaultValue: 30),
                default_account_id = table.Column<int>(type: "integer", nullable: true),
                default_tax_code_id = table.Column<int>(type: "integer", nullable: true),
                bank_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                bank_account_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                table.PrimaryKey("pk_fin_vendors", x => x.id);
                table.ForeignKey(
                    name: "fk_fin_vendors_fin_accounts_default_account_id",
                    column: x => x.default_account_id,
                    principalSchema: "public",
                    principalTable: "fin_accounts",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "fk_fin_vendors_fin_tax_codes_default_tax_code_id",
                    column: x => x.default_tax_code_id,
                    principalSchema: "public",
                    principalTable: "fin_tax_codes",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "fin_ap_invoices",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                invoice_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                vendor_invoice_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                vendor_id = table.Column<int>(type: "integer", nullable: false),
                period_id = table.Column<int>(type: "integer", nullable: false),
                invoice_date = table.Column<DateOnly>(type: "date", nullable: false),
                due_date = table.Column<DateOnly>(type: "date", nullable: false),
                description = table.Column<string>(type: "text", nullable: true),
                subtotal = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                tax_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                total_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                paid_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                outstanding_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                currency_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "IDR"),
                exchange_rate = table.Column<decimal>(type: "numeric(18,6)", nullable: false, defaultValue: 1m),
                status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                approved_by = table.Column<int>(type: "integer", nullable: true),
                approved_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                journal_entry_id = table.Column<int>(type: "integer", nullable: true),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_fin_ap_invoices", x => x.id);
                table.CheckConstraint("ck_fin_ap_invoices_non_negative", "subtotal >= 0 AND tax_amount >= 0 AND total_amount >= 0 AND paid_amount >= 0 AND outstanding_amount >= 0");
                table.ForeignKey(
                    name: "fk_fin_ap_invoices_fin_currencies_currency_code",
                    column: x => x.currency_code,
                    principalSchema: "public",
                    principalTable: "fin_currencies",
                    principalColumn: "code",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_fin_ap_invoices_fin_journal_entries_journal_entry_id",
                    column: x => x.journal_entry_id,
                    principalSchema: "public",
                    principalTable: "fin_journal_entries",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "fk_fin_ap_invoices_fin_periods_period_id",
                    column: x => x.period_id,
                    principalSchema: "public",
                    principalTable: "fin_periods",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_fin_ap_invoices_fin_vendors_vendor_id",
                    column: x => x.vendor_id,
                    principalSchema: "public",
                    principalTable: "fin_vendors",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_fin_ap_invoices_sys_users_approved_by",
                    column: x => x.approved_by,
                    principalSchema: "public",
                    principalTable: "sys_users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "fin_ap_payments",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                payment_no = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                vendor_id = table.Column<int>(type: "integer", nullable: false),
                payment_date = table.Column<DateOnly>(type: "date", nullable: false),
                amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                payment_method = table.Column<int>(type: "integer", nullable: false),
                bank_account_id = table.Column<int>(type: "integer", nullable: false),
                reference_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                notes = table.Column<string>(type: "text", nullable: true),
                journal_entry_id = table.Column<int>(type: "integer", nullable: true),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_fin_ap_payments", x => x.id);
                table.CheckConstraint("ck_fin_ap_payments_positive_amount", "amount > 0");
                table.ForeignKey(
                    name: "fk_fin_ap_payments_fin_accounts_bank_account_id",
                    column: x => x.bank_account_id,
                    principalSchema: "public",
                    principalTable: "fin_accounts",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_fin_ap_payments_fin_journal_entries_journal_entry_id",
                    column: x => x.journal_entry_id,
                    principalSchema: "public",
                    principalTable: "fin_journal_entries",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "fk_fin_ap_payments_fin_vendors_vendor_id",
                    column: x => x.vendor_id,
                    principalSchema: "public",
                    principalTable: "fin_vendors",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "fin_ap_invoice_lines",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                invoice_id = table.Column<int>(type: "integer", nullable: false),
                line_no = table.Column<int>(type: "integer", nullable: false),
                description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 1m),
                unit_price = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                tax_code_id = table.Column<int>(type: "integer", nullable: true),
                tax_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                account_id = table.Column<int>(type: "integer", nullable: false),
                cost_center_id = table.Column<int>(type: "integer", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_fin_ap_invoice_lines", x => x.id);
                table.CheckConstraint("ck_fin_ap_invoice_lines_non_negative", "unit_price >= 0 AND amount >= 0 AND tax_amount >= 0");
                table.CheckConstraint("ck_fin_ap_invoice_lines_positive_qty", "quantity > 0");
                table.ForeignKey(
                    name: "fk_fin_ap_invoice_lines_fin_accounts_account_id",
                    column: x => x.account_id,
                    principalSchema: "public",
                    principalTable: "fin_accounts",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_fin_ap_invoice_lines_fin_ap_invoices_invoice_id",
                    column: x => x.invoice_id,
                    principalSchema: "public",
                    principalTable: "fin_ap_invoices",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_fin_ap_invoice_lines_fin_cost_centers_cost_center_id",
                    column: x => x.cost_center_id,
                    principalSchema: "public",
                    principalTable: "fin_cost_centers",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "fk_fin_ap_invoice_lines_fin_tax_codes_tax_code_id",
                    column: x => x.tax_code_id,
                    principalSchema: "public",
                    principalTable: "fin_tax_codes",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "fin_ap_payment_applications",
            schema: "public",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                payment_id = table.Column<int>(type: "integer", nullable: false),
                invoice_id = table.Column<int>(type: "integer", nullable: false),
                applied_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_fin_ap_payment_applications", x => x.id);
                table.CheckConstraint("ck_fin_ap_payment_apps_positive", "applied_amount > 0");
                table.ForeignKey(
                    name: "fk_fin_ap_payment_applications_fin_ap_invoices_invoice_id",
                    column: x => x.invoice_id,
                    principalSchema: "public",
                    principalTable: "fin_ap_invoices",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_fin_ap_payment_applications_fin_ap_payments_payment_id",
                    column: x => x.payment_id,
                    principalSchema: "public",
                    principalTable: "fin_ap_payments",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_fin_vendors_created_at",
            schema: "public",
            table: "fin_vendors",
            column: "created_at");

        migrationBuilder.CreateIndex(
            name: "ix_fin_vendors_is_deleted",
            schema: "public",
            table: "fin_vendors",
            column: "is_deleted");

        migrationBuilder.CreateIndex(
            name: "ix_fin_vendors_code",
            schema: "public",
            table: "fin_vendors",
            column: "code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_fin_vendors_name",
            schema: "public",
            table: "fin_vendors",
            column: "name");

        migrationBuilder.CreateIndex(
            name: "ix_fin_vendors_is_active",
            schema: "public",
            table: "fin_vendors",
            column: "is_active");

        migrationBuilder.CreateIndex(
            name: "ix_fin_vendors_default_account_id",
            schema: "public",
            table: "fin_vendors",
            column: "default_account_id");

        migrationBuilder.CreateIndex(
            name: "ix_fin_vendors_default_tax_code_id",
            schema: "public",
            table: "fin_vendors",
            column: "default_tax_code_id");

        migrationBuilder.CreateIndex(
            name: "ix_fin_ap_invoices_created_at",
            schema: "public",
            table: "fin_ap_invoices",
            column: "created_at");

        migrationBuilder.CreateIndex(
            name: "ix_fin_ap_invoices_is_deleted",
            schema: "public",
            table: "fin_ap_invoices",
            column: "is_deleted");

        migrationBuilder.CreateIndex(
            name: "ix_fin_ap_invoices_invoice_no",
            schema: "public",
            table: "fin_ap_invoices",
            column: "invoice_no",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_fin_ap_invoices_vendor_invoice_no",
            schema: "public",
            table: "fin_ap_invoices",
            column: "vendor_invoice_no");

        migrationBuilder.CreateIndex(
            name: "ix_fin_ap_invoices_vendor_id",
            schema: "public",
            table: "fin_ap_invoices",
            column: "vendor_id");

        migrationBuilder.CreateIndex(
            name: "ix_fin_ap_invoices_period_id",
            schema: "public",
            table: "fin_ap_invoices",
            column: "period_id");

        migrationBuilder.CreateIndex(
            name: "ix_fin_ap_invoices_invoice_date",
            schema: "public",
            table: "fin_ap_invoices",
            column: "invoice_date");

        migrationBuilder.CreateIndex(
            name: "ix_fin_ap_invoices_due_date",
            schema: "public",
            table: "fin_ap_invoices",
            column: "due_date");

        migrationBuilder.CreateIndex(
            name: "ix_fin_ap_invoices_status",
            schema: "public",
            table: "fin_ap_invoices",
            column: "status");

        migrationBuilder.CreateIndex(
            name: "ix_fin_ap_invoices_outstanding_amount",
            schema: "public",
            table: "fin_ap_invoices",
            column: "outstanding_amount");

        migrationBuilder.CreateIndex(
            name: "ix_fin_ap_invoices_currency_code",
            schema: "public",
            table: "fin_ap_invoices",
            column: "currency_code");

        migrationBuilder.CreateIndex(
            name: "ix_fin_ap_invoices_approved_by",
            schema: "public",
            table: "fin_ap_invoices",
            column: "approved_by");

        migrationBuilder.CreateIndex(
            name: "ix_fin_ap_invoices_journal_entry_id",
            schema: "public",
            table: "fin_ap_invoices",
            column: "journal_entry_id");

        migrationBuilder.CreateIndex(
            name: "ix_fin_ap_payments_created_at",
            schema: "public",
            table: "fin_ap_payments",
            column: "created_at");

        migrationBuilder.CreateIndex(
            name: "ix_fin_ap_payments_is_deleted",
            schema: "public",
            table: "fin_ap_payments",
            column: "is_deleted");

        migrationBuilder.CreateIndex(
            name: "ix_fin_ap_payments_payment_no",
            schema: "public",
            table: "fin_ap_payments",
            column: "payment_no",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_fin_ap_payments_vendor_id",
            schema: "public",
            table: "fin_ap_payments",
            column: "vendor_id");

        migrationBuilder.CreateIndex(
            name: "ix_fin_ap_payments_payment_date",
            schema: "public",
            table: "fin_ap_payments",
            column: "payment_date");

        migrationBuilder.CreateIndex(
            name: "ix_fin_ap_payments_payment_method",
            schema: "public",
            table: "fin_ap_payments",
            column: "payment_method");

        migrationBuilder.CreateIndex(
            name: "ix_fin_ap_payments_bank_account_id",
            schema: "public",
            table: "fin_ap_payments",
            column: "bank_account_id");

        migrationBuilder.CreateIndex(
            name: "ix_fin_ap_payments_journal_entry_id",
            schema: "public",
            table: "fin_ap_payments",
            column: "journal_entry_id");

        migrationBuilder.CreateIndex(
            name: "ix_fin_ap_invoice_lines_invoice_id",
            schema: "public",
            table: "fin_ap_invoice_lines",
            column: "invoice_id");

        migrationBuilder.CreateIndex(
            name: "ix_fin_ap_invoice_lines_invoice_id_line_no",
            schema: "public",
            table: "fin_ap_invoice_lines",
            columns: new[] { "invoice_id", "line_no" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_fin_ap_invoice_lines_account_id",
            schema: "public",
            table: "fin_ap_invoice_lines",
            column: "account_id");

        migrationBuilder.CreateIndex(
            name: "ix_fin_ap_invoice_lines_tax_code_id",
            schema: "public",
            table: "fin_ap_invoice_lines",
            column: "tax_code_id");

        migrationBuilder.CreateIndex(
            name: "ix_fin_ap_invoice_lines_cost_center_id",
            schema: "public",
            table: "fin_ap_invoice_lines",
            column: "cost_center_id");

        migrationBuilder.CreateIndex(
            name: "ix_fin_ap_payment_applications_payment_id",
            schema: "public",
            table: "fin_ap_payment_applications",
            column: "payment_id");

        migrationBuilder.CreateIndex(
            name: "ix_fin_ap_payment_applications_invoice_id",
            schema: "public",
            table: "fin_ap_payment_applications",
            column: "invoice_id");

        migrationBuilder.CreateIndex(
            name: "ix_fin_ap_payment_applications_payment_id_invoice_id",
            schema: "public",
            table: "fin_ap_payment_applications",
            columns: new[] { "payment_id", "invoice_id" },
            unique: true);

        migrationBuilder.Sql(@"
DO $$
DECLARE
    v_period_id INTEGER;
    v_expense_account_id INTEGER;
    v_bank_account_id INTEGER;
    v_vendor1_id INTEGER;
    v_vendor2_id INTEGER;
    v_vendor3_id INTEGER;
    v_invoice1_id INTEGER;
    v_invoice2_id INTEGER;
    v_invoice3_id INTEGER;
    v_payment1_id INTEGER;
BEGIN
    SELECT id INTO v_period_id
    FROM public.fin_periods
    WHERE NOT is_deleted AND status = 0
    ORDER BY start_date DESC
    LIMIT 1;

    IF v_period_id IS NULL THEN
        SELECT id INTO v_period_id
        FROM public.fin_periods
        WHERE NOT is_deleted
        ORDER BY start_date DESC
        LIMIT 1;
    END IF;

    SELECT id INTO v_expense_account_id
    FROM public.fin_accounts
    WHERE NOT is_deleted AND code = '5104'
    LIMIT 1;

    IF v_expense_account_id IS NULL THEN
        SELECT id INTO v_expense_account_id
        FROM public.fin_accounts
        WHERE NOT is_deleted AND type = 4
        ORDER BY code
        LIMIT 1;
    END IF;

    SELECT id INTO v_bank_account_id
    FROM public.fin_accounts
    WHERE NOT is_deleted AND is_bank_account
    ORDER BY code
    LIMIT 1;

    IF v_period_id IS NULL OR v_expense_account_id IS NULL THEN
        RETURN;
    END IF;

    INSERT INTO public.fin_vendors
        (code, name, tax_id, address, phone, email, contact_person, payment_terms_days, default_account_id, bank_name, bank_account_no, is_active, created_by, created_at)
    VALUES
        ('VEND-001', 'PT Nusantara Office', '01.111.222.3-444.000', 'Jakarta', '021-5551001', 'ap@nusantara-office.co.id', 'Rina', 30, NULL, 'BCA', '123-456-001', TRUE, 'system', NOW())
    ON CONFLICT (code) DO NOTHING;

    INSERT INTO public.fin_vendors
        (code, name, tax_id, address, phone, email, contact_person, payment_terms_days, default_account_id, bank_name, bank_account_no, is_active, created_by, created_at)
    VALUES
        ('VEND-002', 'PT Sinar Teknologi', '02.222.333.4-555.000', 'Bandung', '022-5551002', 'billing@sinar-teknologi.co.id', 'Dedi', 21, NULL, 'Mandiri', '123-456-002', TRUE, 'system', NOW())
    ON CONFLICT (code) DO NOTHING;

    INSERT INTO public.fin_vendors
        (code, name, tax_id, address, phone, email, contact_person, payment_terms_days, default_account_id, bank_name, bank_account_no, is_active, created_by, created_at)
    VALUES
        ('VEND-003', 'CV Maju Bersama', '03.333.444.5-666.000', 'Surabaya', '031-5551003', 'finance@majubersama.co.id', 'Santi', 14, NULL, 'BNI', '123-456-003', TRUE, 'system', NOW())
    ON CONFLICT (code) DO NOTHING;

    SELECT id INTO v_vendor1_id FROM public.fin_vendors WHERE code = 'VEND-001' AND NOT is_deleted;
    SELECT id INTO v_vendor2_id FROM public.fin_vendors WHERE code = 'VEND-002' AND NOT is_deleted;
    SELECT id INTO v_vendor3_id FROM public.fin_vendors WHERE code = 'VEND-003' AND NOT is_deleted;

    IF v_vendor1_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM public.fin_ap_invoices WHERE invoice_no = 'AP-SMP-2026-0001') THEN
        INSERT INTO public.fin_ap_invoices
            (invoice_no, vendor_invoice_no, vendor_id, period_id, invoice_date, due_date, description, subtotal, tax_amount, total_amount, paid_amount, outstanding_amount, currency_code, exchange_rate, status, created_by, created_at)
        VALUES
            ('AP-SMP-2026-0001', 'INV-NSO-001', v_vendor1_id, v_period_id, (CURRENT_DATE - INTERVAL '15 days')::date, (CURRENT_DATE + INTERVAL '15 days')::date, 'Pembelian ATK bulanan', 3500000, 0, 3500000, 0, 3500000, 'IDR', 1, 0, 'system', NOW())
        RETURNING id INTO v_invoice1_id;

        INSERT INTO public.fin_ap_invoice_lines
            (invoice_id, line_no, description, quantity, unit_price, amount, tax_code_id, tax_amount, account_id, cost_center_id)
        VALUES
            (v_invoice1_id, 1, 'ATK dan perlengkapan kantor', 1, 3500000, 3500000, NULL, 0, v_expense_account_id, NULL);
    END IF;

    IF v_vendor2_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM public.fin_ap_invoices WHERE invoice_no = 'AP-SMP-2026-0002') THEN
        INSERT INTO public.fin_ap_invoices
            (invoice_no, vendor_invoice_no, vendor_id, period_id, invoice_date, due_date, description, subtotal, tax_amount, total_amount, paid_amount, outstanding_amount, currency_code, exchange_rate, status, approved_at, created_by, created_at)
        VALUES
            ('AP-SMP-2026-0002', 'INV-SINAR-045', v_vendor2_id, v_period_id, (CURRENT_DATE - INTERVAL '20 days')::date, (CURRENT_DATE + INTERVAL '10 days')::date, 'Tagihan maintenance perangkat IT', 5000000, 0, 5000000, 0, 5000000, 'IDR', 1, 1, NOW() - INTERVAL '7 days', 'system', NOW())
        RETURNING id INTO v_invoice2_id;

        INSERT INTO public.fin_ap_invoice_lines
            (invoice_id, line_no, description, quantity, unit_price, amount, tax_code_id, tax_amount, account_id, cost_center_id)
        VALUES
            (v_invoice2_id, 1, 'Maintenance server dan jaringan', 1, 5000000, 5000000, NULL, 0, v_expense_account_id, NULL);
    END IF;

    IF v_vendor3_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM public.fin_ap_invoices WHERE invoice_no = 'AP-SMP-2026-0003') THEN
        INSERT INTO public.fin_ap_invoices
            (invoice_no, vendor_invoice_no, vendor_id, period_id, invoice_date, due_date, description, subtotal, tax_amount, total_amount, paid_amount, outstanding_amount, currency_code, exchange_rate, status, approved_at, created_by, created_at)
        VALUES
            ('AP-SMP-2026-0003', 'INV-MAJU-113', v_vendor3_id, v_period_id, (CURRENT_DATE - INTERVAL '35 days')::date, (CURRENT_DATE - INTERVAL '5 days')::date, 'Pengadaan kebutuhan operasional proyek', 4200000, 0, 4200000, 1500000, 2700000, 'IDR', 1, 2, NOW() - INTERVAL '20 days', 'system', NOW())
        RETURNING id INTO v_invoice3_id;

        INSERT INTO public.fin_ap_invoice_lines
            (invoice_id, line_no, description, quantity, unit_price, amount, tax_code_id, tax_amount, account_id, cost_center_id)
        VALUES
            (v_invoice3_id, 1, 'Material proyek tahap 1', 1, 2000000, 2000000, NULL, 0, v_expense_account_id, NULL),
            (v_invoice3_id, 2, 'Material proyek tahap 2', 1, 2200000, 2200000, NULL, 0, v_expense_account_id, NULL);
    END IF;

    SELECT id INTO v_invoice3_id FROM public.fin_ap_invoices WHERE invoice_no = 'AP-SMP-2026-0003' AND NOT is_deleted;

    IF v_vendor3_id IS NOT NULL AND v_bank_account_id IS NOT NULL AND v_invoice3_id IS NOT NULL
        AND NOT EXISTS (SELECT 1 FROM public.fin_ap_payments WHERE payment_no = 'AP-PAY-SMP-0001') THEN

        INSERT INTO public.fin_ap_payments
            (payment_no, vendor_id, payment_date, amount, payment_method, bank_account_id, reference_no, notes, created_by, created_at)
        VALUES
            ('AP-PAY-SMP-0001', v_vendor3_id, (CURRENT_DATE - INTERVAL '3 days')::date, 1500000, 0, v_bank_account_id, 'TRF-0001', 'Pembayaran sebagian invoice AP-SMP-2026-0003', 'system', NOW())
        RETURNING id INTO v_payment1_id;

        INSERT INTO public.fin_ap_payment_applications
            (payment_id, invoice_id, applied_amount)
        VALUES
            (v_payment1_id, v_invoice3_id, 1500000);
    END IF;
END $$;
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "fin_ap_payment_applications",
            schema: "public");

        migrationBuilder.DropTable(
            name: "fin_ap_invoice_lines",
            schema: "public");

        migrationBuilder.DropTable(
            name: "fin_ap_payments",
            schema: "public");

        migrationBuilder.DropTable(
            name: "fin_ap_invoices",
            schema: "public");

        migrationBuilder.DropTable(
            name: "fin_vendors",
            schema: "public");
    }
}
