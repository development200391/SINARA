using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260530213000_AddFinancePhase11AccountsReceivable")]
public partial class AddFinancePhase11AccountsReceivable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
CREATE TABLE IF NOT EXISTS public.fin_customers (
    id                  INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    code                VARCHAR(20) NOT NULL,
    name                VARCHAR(200) NOT NULL,
    tax_id              VARCHAR(30),
    address             TEXT,
    phone               VARCHAR(30),
    email               VARCHAR(200),
    contact_person      VARCHAR(100),
    credit_limit        NUMERIC(18,4) NOT NULL DEFAULT 0,
    payment_terms_days  INTEGER NOT NULL DEFAULT 30,
    default_account_id  INTEGER,
    default_tax_code_id INTEGER,
    is_active           BOOLEAN NOT NULL DEFAULT TRUE,
    created_by          VARCHAR(100) NOT NULL,
    created_at          TIMESTAMPTZ NOT NULL,
    updated_by          VARCHAR(100),
    updated_at          TIMESTAMPTZ,
    is_deleted          BOOLEAN NOT NULL DEFAULT FALSE,
    deleted_at          TIMESTAMPTZ,
    CONSTRAINT ck_fin_customers_non_negative_credit_limit CHECK (credit_limit >= 0),
    CONSTRAINT ck_fin_customers_non_negative_terms CHECK (payment_terms_days >= 0),
    CONSTRAINT fk_fin_customers_fin_accounts_default_account_id
        FOREIGN KEY (default_account_id) REFERENCES public.fin_accounts (id) ON DELETE SET NULL,
    CONSTRAINT fk_fin_customers_fin_tax_codes_default_tax_code_id
        FOREIGN KEY (default_tax_code_id) REFERENCES public.fin_tax_codes (id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS public.fin_ar_invoices (
    id                  INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    invoice_no          VARCHAR(50) NOT NULL,
    customer_id         INTEGER NOT NULL,
    period_id           INTEGER NOT NULL,
    invoice_date        DATE NOT NULL,
    due_date            DATE NOT NULL,
    description         TEXT,
    subtotal            NUMERIC(18,4) NOT NULL DEFAULT 0,
    tax_amount          NUMERIC(18,4) NOT NULL DEFAULT 0,
    total_amount        NUMERIC(18,4) NOT NULL DEFAULT 0,
    received_amount     NUMERIC(18,4) NOT NULL DEFAULT 0,
    outstanding_amount  NUMERIC(18,4) NOT NULL DEFAULT 0,
    currency_code       VARCHAR(10) NOT NULL DEFAULT 'IDR',
    exchange_rate       NUMERIC(18,6) NOT NULL DEFAULT 1,
    status              INTEGER NOT NULL DEFAULT 0,
    sent_by             INTEGER,
    sent_at             TIMESTAMPTZ,
    journal_entry_id    INTEGER,
    created_by          VARCHAR(100) NOT NULL,
    created_at          TIMESTAMPTZ NOT NULL,
    updated_by          VARCHAR(100),
    updated_at          TIMESTAMPTZ,
    is_deleted          BOOLEAN NOT NULL DEFAULT FALSE,
    deleted_at          TIMESTAMPTZ,
    CONSTRAINT ck_fin_ar_invoices_non_negative CHECK (subtotal >= 0 AND tax_amount >= 0 AND total_amount >= 0 AND received_amount >= 0 AND outstanding_amount >= 0),
    CONSTRAINT fk_fin_ar_invoices_fin_customers_customer_id
        FOREIGN KEY (customer_id) REFERENCES public.fin_customers (id) ON DELETE RESTRICT,
    CONSTRAINT fk_fin_ar_invoices_fin_periods_period_id
        FOREIGN KEY (period_id) REFERENCES public.fin_periods (id) ON DELETE RESTRICT,
    CONSTRAINT fk_fin_ar_invoices_fin_currencies_currency_code
        FOREIGN KEY (currency_code) REFERENCES public.fin_currencies (code) ON DELETE RESTRICT,
    CONSTRAINT fk_fin_ar_invoices_sys_users_sent_by
        FOREIGN KEY (sent_by) REFERENCES public.sys_users (id) ON DELETE SET NULL,
    CONSTRAINT fk_fin_ar_invoices_fin_journal_entries_journal_entry_id
        FOREIGN KEY (journal_entry_id) REFERENCES public.fin_journal_entries (id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS public.fin_ar_receipts (
    id                  INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    receipt_no          VARCHAR(30) NOT NULL,
    customer_id         INTEGER NOT NULL,
    receipt_date        DATE NOT NULL,
    amount              NUMERIC(18,4) NOT NULL,
    payment_method      INTEGER NOT NULL,
    bank_account_id     INTEGER NOT NULL,
    reference_no        VARCHAR(100),
    notes               TEXT,
    journal_entry_id    INTEGER,
    created_by          VARCHAR(100) NOT NULL,
    created_at          TIMESTAMPTZ NOT NULL,
    updated_by          VARCHAR(100),
    updated_at          TIMESTAMPTZ,
    is_deleted          BOOLEAN NOT NULL DEFAULT FALSE,
    deleted_at          TIMESTAMPTZ,
    CONSTRAINT ck_fin_ar_receipts_positive_amount CHECK (amount > 0),
    CONSTRAINT fk_fin_ar_receipts_fin_customers_customer_id
        FOREIGN KEY (customer_id) REFERENCES public.fin_customers (id) ON DELETE RESTRICT,
    CONSTRAINT fk_fin_ar_receipts_fin_accounts_bank_account_id
        FOREIGN KEY (bank_account_id) REFERENCES public.fin_accounts (id) ON DELETE RESTRICT,
    CONSTRAINT fk_fin_ar_receipts_fin_journal_entries_journal_entry_id
        FOREIGN KEY (journal_entry_id) REFERENCES public.fin_journal_entries (id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS public.fin_ar_invoice_lines (
    id              INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    invoice_id      INTEGER NOT NULL,
    line_no         INTEGER NOT NULL,
    description     VARCHAR(200) NOT NULL,
    quantity        NUMERIC(18,4) NOT NULL DEFAULT 1,
    unit_price      NUMERIC(18,4) NOT NULL DEFAULT 0,
    amount          NUMERIC(18,4) NOT NULL DEFAULT 0,
    tax_code_id     INTEGER,
    tax_amount      NUMERIC(18,4) NOT NULL DEFAULT 0,
    account_id      INTEGER NOT NULL,
    cost_center_id  INTEGER,
    CONSTRAINT ck_fin_ar_invoice_lines_positive_qty CHECK (quantity > 0),
    CONSTRAINT ck_fin_ar_invoice_lines_non_negative CHECK (unit_price >= 0 AND amount >= 0 AND tax_amount >= 0),
    CONSTRAINT fk_fin_ar_invoice_lines_fin_ar_invoices_invoice_id
        FOREIGN KEY (invoice_id) REFERENCES public.fin_ar_invoices (id) ON DELETE CASCADE,
    CONSTRAINT fk_fin_ar_invoice_lines_fin_tax_codes_tax_code_id
        FOREIGN KEY (tax_code_id) REFERENCES public.fin_tax_codes (id) ON DELETE SET NULL,
    CONSTRAINT fk_fin_ar_invoice_lines_fin_accounts_account_id
        FOREIGN KEY (account_id) REFERENCES public.fin_accounts (id) ON DELETE RESTRICT,
    CONSTRAINT fk_fin_ar_invoice_lines_fin_cost_centers_cost_center_id
        FOREIGN KEY (cost_center_id) REFERENCES public.fin_cost_centers (id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS public.fin_ar_receipt_applications (
    id              INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    receipt_id      INTEGER NOT NULL,
    invoice_id      INTEGER NOT NULL,
    applied_amount  NUMERIC(18,4) NOT NULL,
    CONSTRAINT ck_fin_ar_receipt_apps_positive CHECK (applied_amount > 0),
    CONSTRAINT fk_fin_ar_receipt_applications_fin_ar_receipts_receipt_id
        FOREIGN KEY (receipt_id) REFERENCES public.fin_ar_receipts (id) ON DELETE CASCADE,
    CONSTRAINT fk_fin_ar_receipt_applications_fin_ar_invoices_invoice_id
        FOREIGN KEY (invoice_id) REFERENCES public.fin_ar_invoices (id) ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_fin_customers_code ON public.fin_customers (code);
CREATE INDEX IF NOT EXISTS ix_fin_customers_name ON public.fin_customers (name);
CREATE INDEX IF NOT EXISTS ix_fin_customers_is_active ON public.fin_customers (is_active);
CREATE INDEX IF NOT EXISTS ix_fin_customers_default_account_id ON public.fin_customers (default_account_id);
CREATE INDEX IF NOT EXISTS ix_fin_customers_default_tax_code_id ON public.fin_customers (default_tax_code_id);
CREATE INDEX IF NOT EXISTS ix_fin_customers_created_at ON public.fin_customers (created_at);
CREATE INDEX IF NOT EXISTS ix_fin_customers_is_deleted ON public.fin_customers (is_deleted);

CREATE UNIQUE INDEX IF NOT EXISTS ix_fin_ar_invoices_invoice_no ON public.fin_ar_invoices (invoice_no);
CREATE INDEX IF NOT EXISTS ix_fin_ar_invoices_customer_id ON public.fin_ar_invoices (customer_id);
CREATE INDEX IF NOT EXISTS ix_fin_ar_invoices_period_id ON public.fin_ar_invoices (period_id);
CREATE INDEX IF NOT EXISTS ix_fin_ar_invoices_invoice_date ON public.fin_ar_invoices (invoice_date);
CREATE INDEX IF NOT EXISTS ix_fin_ar_invoices_due_date ON public.fin_ar_invoices (due_date);
CREATE INDEX IF NOT EXISTS ix_fin_ar_invoices_status ON public.fin_ar_invoices (status);
CREATE INDEX IF NOT EXISTS ix_fin_ar_invoices_outstanding_amount ON public.fin_ar_invoices (outstanding_amount);
CREATE INDEX IF NOT EXISTS ix_fin_ar_invoices_currency_code ON public.fin_ar_invoices (currency_code);
CREATE INDEX IF NOT EXISTS ix_fin_ar_invoices_sent_by ON public.fin_ar_invoices (sent_by);
CREATE INDEX IF NOT EXISTS ix_fin_ar_invoices_journal_entry_id ON public.fin_ar_invoices (journal_entry_id);
CREATE INDEX IF NOT EXISTS ix_fin_ar_invoices_created_at ON public.fin_ar_invoices (created_at);
CREATE INDEX IF NOT EXISTS ix_fin_ar_invoices_is_deleted ON public.fin_ar_invoices (is_deleted);

CREATE UNIQUE INDEX IF NOT EXISTS ix_fin_ar_receipts_receipt_no ON public.fin_ar_receipts (receipt_no);
CREATE INDEX IF NOT EXISTS ix_fin_ar_receipts_customer_id ON public.fin_ar_receipts (customer_id);
CREATE INDEX IF NOT EXISTS ix_fin_ar_receipts_receipt_date ON public.fin_ar_receipts (receipt_date);
CREATE INDEX IF NOT EXISTS ix_fin_ar_receipts_payment_method ON public.fin_ar_receipts (payment_method);
CREATE INDEX IF NOT EXISTS ix_fin_ar_receipts_bank_account_id ON public.fin_ar_receipts (bank_account_id);
CREATE INDEX IF NOT EXISTS ix_fin_ar_receipts_journal_entry_id ON public.fin_ar_receipts (journal_entry_id);
CREATE INDEX IF NOT EXISTS ix_fin_ar_receipts_created_at ON public.fin_ar_receipts (created_at);
CREATE INDEX IF NOT EXISTS ix_fin_ar_receipts_is_deleted ON public.fin_ar_receipts (is_deleted);

CREATE INDEX IF NOT EXISTS ix_fin_ar_invoice_lines_invoice_id ON public.fin_ar_invoice_lines (invoice_id);
CREATE UNIQUE INDEX IF NOT EXISTS ix_fin_ar_invoice_lines_invoice_id_line_no ON public.fin_ar_invoice_lines (invoice_id, line_no);
CREATE INDEX IF NOT EXISTS ix_fin_ar_invoice_lines_account_id ON public.fin_ar_invoice_lines (account_id);
CREATE INDEX IF NOT EXISTS ix_fin_ar_invoice_lines_tax_code_id ON public.fin_ar_invoice_lines (tax_code_id);
CREATE INDEX IF NOT EXISTS ix_fin_ar_invoice_lines_cost_center_id ON public.fin_ar_invoice_lines (cost_center_id);

CREATE INDEX IF NOT EXISTS ix_fin_ar_receipt_applications_receipt_id ON public.fin_ar_receipt_applications (receipt_id);
CREATE INDEX IF NOT EXISTS ix_fin_ar_receipt_applications_invoice_id ON public.fin_ar_receipt_applications (invoice_id);
CREATE UNIQUE INDEX IF NOT EXISTS ix_fin_ar_receipt_applications_receipt_id_invoice_id ON public.fin_ar_receipt_applications (receipt_id, invoice_id);

DO $$
DECLARE
    v_period_id INTEGER;
    v_ar_account_id INTEGER;
    v_revenue_account_id INTEGER;
    v_bank_account_id INTEGER;
    v_currency_code VARCHAR(10);
    v_customer1_id INTEGER;
    v_customer2_id INTEGER;
    v_customer3_id INTEGER;
    v_invoice3_id INTEGER;
    v_receipt1_id INTEGER;
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

    SELECT code INTO v_currency_code
    FROM public.fin_currencies
    WHERE NOT is_deleted AND code = 'IDR'
    LIMIT 1;

    IF v_currency_code IS NULL THEN
        SELECT code INTO v_currency_code
        FROM public.fin_currencies
        WHERE NOT is_deleted
        ORDER BY code
        LIMIT 1;
    END IF;

    SELECT id INTO v_ar_account_id
    FROM public.fin_accounts
    WHERE NOT is_deleted AND code = '1110'
    LIMIT 1;

    SELECT id INTO v_revenue_account_id
    FROM public.fin_accounts
    WHERE NOT is_deleted AND code = '4101'
    LIMIT 1;

    IF v_revenue_account_id IS NULL THEN
        SELECT id INTO v_revenue_account_id
        FROM public.fin_accounts
        WHERE NOT is_deleted AND type = 3
        ORDER BY code
        LIMIT 1;
    END IF;

    SELECT id INTO v_bank_account_id
    FROM public.fin_accounts
    WHERE NOT is_deleted AND is_bank_account
    ORDER BY code
    LIMIT 1;

    IF v_period_id IS NULL OR v_revenue_account_id IS NULL OR v_currency_code IS NULL THEN
        RETURN;
    END IF;

    INSERT INTO public.fin_customers
        (code, name, tax_id, address, phone, email, contact_person, credit_limit, payment_terms_days, default_account_id, default_tax_code_id, is_active, created_by, created_at)
    VALUES
        ('CUST-001', 'PT Nusantara Retail', '01.444.555.6-777.000', 'Jakarta', '021-8010001', 'accounting@nusantararetail.co.id', 'Rudi', 15000000, 30, v_ar_account_id, NULL, TRUE, 'system', NOW())
    ON CONFLICT (code) DO NOTHING;

    INSERT INTO public.fin_customers
        (code, name, tax_id, address, phone, email, contact_person, credit_limit, payment_terms_days, default_account_id, default_tax_code_id, is_active, created_by, created_at)
    VALUES
        ('CUST-002', 'PT Bumi Distribusi', '02.555.666.7-888.000', 'Bandung', '022-8010002', 'finance@bumidistribusi.co.id', 'Maya', 20000000, 21, v_ar_account_id, NULL, TRUE, 'system', NOW())
    ON CONFLICT (code) DO NOTHING;

    INSERT INTO public.fin_customers
        (code, name, tax_id, address, phone, email, contact_person, credit_limit, payment_terms_days, default_account_id, default_tax_code_id, is_active, created_by, created_at)
    VALUES
        ('CUST-003', 'CV Sinar Logistik', '03.666.777.8-999.000', 'Surabaya', '031-8010003', 'billing@sinarlogistik.co.id', 'Andi', 12000000, 14, v_ar_account_id, NULL, TRUE, 'system', NOW())
    ON CONFLICT (code) DO NOTHING;

    SELECT id INTO v_customer1_id FROM public.fin_customers WHERE code = 'CUST-001' AND NOT is_deleted;
    SELECT id INTO v_customer2_id FROM public.fin_customers WHERE code = 'CUST-002' AND NOT is_deleted;
    SELECT id INTO v_customer3_id FROM public.fin_customers WHERE code = 'CUST-003' AND NOT is_deleted;

    IF v_customer1_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM public.fin_ar_invoices WHERE invoice_no = 'AR-SMP-2026-0001') THEN
        INSERT INTO public.fin_ar_invoices
            (invoice_no, customer_id, period_id, invoice_date, due_date, description, subtotal, tax_amount, total_amount, received_amount, outstanding_amount, currency_code, exchange_rate, status, created_by, created_at)
        VALUES
            ('AR-SMP-2026-0001', v_customer1_id, v_period_id, (CURRENT_DATE - INTERVAL '5 days')::date, (CURRENT_DATE + INTERVAL '25 days')::date, 'Tagihan penjualan produk retail bulanan', 4500000, 0, 4500000, 0, 4500000, v_currency_code, 1, 0, 'system', NOW());

        INSERT INTO public.fin_ar_invoice_lines
            (invoice_id, line_no, description, quantity, unit_price, amount, tax_code_id, tax_amount, account_id, cost_center_id)
        SELECT id, 1, 'Penjualan produk retail', 1, 4500000, 4500000, NULL, 0, v_revenue_account_id, NULL
        FROM public.fin_ar_invoices
        WHERE invoice_no = 'AR-SMP-2026-0001' AND NOT is_deleted;
    END IF;

    IF v_customer2_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM public.fin_ar_invoices WHERE invoice_no = 'AR-SMP-2026-0002') THEN
        INSERT INTO public.fin_ar_invoices
            (invoice_no, customer_id, period_id, invoice_date, due_date, description, subtotal, tax_amount, total_amount, received_amount, outstanding_amount, currency_code, exchange_rate, status, sent_at, created_by, created_at)
        VALUES
            ('AR-SMP-2026-0002', v_customer2_id, v_period_id, (CURRENT_DATE - INTERVAL '30 days')::date, (CURRENT_DATE - INTERVAL '5 days')::date, 'Tagihan jasa distribusi area barat', 7000000, 0, 7000000, 0, 7000000, v_currency_code, 1, 1, NOW() - INTERVAL '10 days', 'system', NOW());

        INSERT INTO public.fin_ar_invoice_lines
            (invoice_id, line_no, description, quantity, unit_price, amount, tax_code_id, tax_amount, account_id, cost_center_id)
        SELECT id, 1, 'Jasa distribusi mingguan', 1, 7000000, 7000000, NULL, 0, v_revenue_account_id, NULL
        FROM public.fin_ar_invoices
        WHERE invoice_no = 'AR-SMP-2026-0002' AND NOT is_deleted;
    END IF;

    IF v_customer3_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM public.fin_ar_invoices WHERE invoice_no = 'AR-SMP-2026-0003') THEN
        INSERT INTO public.fin_ar_invoices
            (invoice_no, customer_id, period_id, invoice_date, due_date, description, subtotal, tax_amount, total_amount, received_amount, outstanding_amount, currency_code, exchange_rate, status, sent_at, created_by, created_at)
        VALUES
            ('AR-SMP-2026-0003', v_customer3_id, v_period_id, (CURRENT_DATE - INTERVAL '20 days')::date, (CURRENT_DATE + INTERVAL '10 days')::date, 'Tagihan layanan logistik proyek', 4000000, 0, 4000000, 1200000, 2800000, v_currency_code, 1, 2, NOW() - INTERVAL '7 days', 'system', NOW());

        INSERT INTO public.fin_ar_invoice_lines
            (invoice_id, line_no, description, quantity, unit_price, amount, tax_code_id, tax_amount, account_id, cost_center_id)
        SELECT id, 1, 'Layanan logistik tahap 1', 1, 2500000, 2500000, NULL, 0, v_revenue_account_id, NULL
        FROM public.fin_ar_invoices
        WHERE invoice_no = 'AR-SMP-2026-0003' AND NOT is_deleted;

        INSERT INTO public.fin_ar_invoice_lines
            (invoice_id, line_no, description, quantity, unit_price, amount, tax_code_id, tax_amount, account_id, cost_center_id)
        SELECT id, 2, 'Layanan logistik tahap 2', 1, 1500000, 1500000, NULL, 0, v_revenue_account_id, NULL
        FROM public.fin_ar_invoices
        WHERE invoice_no = 'AR-SMP-2026-0003' AND NOT is_deleted;
    END IF;

    SELECT id INTO v_invoice3_id
    FROM public.fin_ar_invoices
    WHERE invoice_no = 'AR-SMP-2026-0003' AND NOT is_deleted;

    IF v_customer3_id IS NOT NULL
       AND v_invoice3_id IS NOT NULL
       AND v_bank_account_id IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM public.fin_ar_receipts WHERE receipt_no = 'AR-RCV-SMP-0001') THEN

        INSERT INTO public.fin_ar_receipts
            (receipt_no, customer_id, receipt_date, amount, payment_method, bank_account_id, reference_no, notes, created_by, created_at)
        VALUES
            ('AR-RCV-SMP-0001', v_customer3_id, (CURRENT_DATE - INTERVAL '2 days')::date, 1200000, 0, v_bank_account_id, 'TRX-AR-0001', 'Penerimaan sebagian untuk AR-SMP-2026-0003', 'system', NOW())
        RETURNING id INTO v_receipt1_id;

        INSERT INTO public.fin_ar_receipt_applications
            (receipt_id, invoice_id, applied_amount)
        VALUES
            (v_receipt1_id, v_invoice3_id, 1200000);
    END IF;
END $$;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
DROP TABLE IF EXISTS public.fin_ar_receipt_applications;
DROP TABLE IF EXISTS public.fin_ar_invoice_lines;
DROP TABLE IF EXISTS public.fin_ar_receipts;
DROP TABLE IF EXISTS public.fin_ar_invoices;
DROP TABLE IF EXISTS public.fin_customers;
""");
    }
}

