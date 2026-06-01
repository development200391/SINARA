using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260602004500_AddFixedAssetsPhase25")]
public partial class AddFixedAssetsPhase25 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
CREATE TABLE IF NOT EXISTS public.fa_asset_categories (
    id integer GENERATED ALWAYS AS IDENTITY,
    code character varying(20) NOT NULL,
    name character varying(100) NOT NULL,
    depreciation_method integer NOT NULL,
    useful_life_months integer NOT NULL,
    depreciation_rate numeric(7,4),
    asset_account_id integer,
    accumulated_depreciation_account_id integer,
    depreciation_expense_account_id integer,
    is_active boolean NOT NULL DEFAULT TRUE,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_fa_asset_categories PRIMARY KEY (id),
    CONSTRAINT ck_fa_asset_categories_useful_life_positive CHECK (useful_life_months > 0),
    CONSTRAINT ck_fa_asset_categories_depreciation_rate_range CHECK (depreciation_rate IS NULL OR (depreciation_rate > 0 AND depreciation_rate <= 100)),
    CONSTRAINT fk_fa_asset_categories_fin_accounts_asset_account_id FOREIGN KEY (asset_account_id) REFERENCES public.fin_accounts (id) ON DELETE SET NULL,
    CONSTRAINT fk_fa_asset_categories_fin_accounts_acc_dep_account_id FOREIGN KEY (accumulated_depreciation_account_id) REFERENCES public.fin_accounts (id) ON DELETE SET NULL,
    CONSTRAINT fk_fa_asset_categories_fin_accounts_dep_exp_account_id FOREIGN KEY (depreciation_expense_account_id) REFERENCES public.fin_accounts (id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS public.fa_locations (
    id integer GENERATED ALWAYS AS IDENTITY,
    code character varying(20) NOT NULL,
    name character varying(100) NOT NULL,
    address character varying(500),
    department_id integer,
    manager_id integer,
    is_active boolean NOT NULL DEFAULT TRUE,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_fa_locations PRIMARY KEY (id),
    CONSTRAINT fk_fa_locations_hr_departments_department_id FOREIGN KEY (department_id) REFERENCES public.hr_departments (id) ON DELETE SET NULL,
    CONSTRAINT fk_fa_locations_hr_employees_manager_id FOREIGN KEY (manager_id) REFERENCES public.hr_employees (id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS public.fa_depreciation_configs (
    id integer GENERATED ALWAYS AS IDENTITY,
    name character varying(100) NOT NULL,
    fiscal_year smallint NOT NULL,
    start_date date NOT NULL,
    end_date date NOT NULL,
    run_day smallint NOT NULL DEFAULT 28,
    is_auto_post_journal boolean NOT NULL DEFAULT FALSE,
    is_active boolean NOT NULL DEFAULT TRUE,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_fa_depreciation_configs PRIMARY KEY (id),
    CONSTRAINT ck_fa_depreciation_configs_period CHECK (start_date <= end_date),
    CONSTRAINT ck_fa_depreciation_configs_run_day_range CHECK (run_day >= 1 AND run_day <= 31)
);

CREATE TABLE IF NOT EXISTS public.fa_depreciation_runs (
    id integer GENERATED ALWAYS AS IDENTITY,
    run_no character varying(30) NOT NULL,
    period_year smallint NOT NULL,
    period_month smallint NOT NULL,
    run_date date NOT NULL,
    total_asset_count integer NOT NULL,
    total_depreciation_amount numeric(18,2) NOT NULL,
    status integer NOT NULL,
    approved_by integer,
    approved_at timestamptz,
    journal_entry_id integer,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_fa_depreciation_runs PRIMARY KEY (id),
    CONSTRAINT ck_fa_depreciation_runs_period_month CHECK (period_month >= 1 AND period_month <= 12),
    CONSTRAINT ck_fa_depreciation_runs_total_asset_count CHECK (total_asset_count >= 0),
    CONSTRAINT ck_fa_depreciation_runs_total_depreciation_non_negative CHECK (total_depreciation_amount >= 0),
    CONSTRAINT fk_fa_depreciation_runs_sys_users_approved_by FOREIGN KEY (approved_by) REFERENCES public.sys_users (id) ON DELETE SET NULL,
    CONSTRAINT fk_fa_depreciation_runs_fin_journal_entries_journal_entry_id FOREIGN KEY (journal_entry_id) REFERENCES public.fin_journal_entries (id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS public.fa_assets (
    id integer GENERATED ALWAYS AS IDENTITY,
    asset_code character varying(30) NOT NULL,
    name character varying(150) NOT NULL,
    category_id integer NOT NULL,
    location_id integer NOT NULL,
    department_id integer,
    acquisition_date date NOT NULL,
    in_service_date date NOT NULL,
    acquisition_cost numeric(18,2) NOT NULL,
    salvage_value numeric(18,2) NOT NULL DEFAULT 0.0,
    useful_life_months integer NOT NULL,
    depreciation_method integer NOT NULL,
    depreciation_rate numeric(7,4),
    accumulated_depreciation numeric(18,2) NOT NULL DEFAULT 0.0,
    book_value numeric(18,2) NOT NULL,
    status integer NOT NULL,
    serial_number character varying(100),
    vendor_name character varying(200),
    description text,
    is_active boolean NOT NULL DEFAULT TRUE,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_fa_assets PRIMARY KEY (id),
    CONSTRAINT ck_fa_assets_acquisition_cost_non_negative CHECK (acquisition_cost >= 0),
    CONSTRAINT ck_fa_assets_salvage_value_non_negative CHECK (salvage_value >= 0),
    CONSTRAINT ck_fa_assets_salvage_not_exceed_cost CHECK (salvage_value <= acquisition_cost),
    CONSTRAINT ck_fa_assets_useful_life_positive CHECK (useful_life_months > 0),
    CONSTRAINT ck_fa_assets_depreciation_rate_range CHECK (depreciation_rate IS NULL OR (depreciation_rate > 0 AND depreciation_rate <= 100)),
    CONSTRAINT ck_fa_assets_accumulated_depreciation_non_negative CHECK (accumulated_depreciation >= 0),
    CONSTRAINT ck_fa_assets_book_value_non_negative CHECK (book_value >= 0),
    CONSTRAINT fk_fa_assets_fa_asset_categories_category_id FOREIGN KEY (category_id) REFERENCES public.fa_asset_categories (id) ON DELETE RESTRICT,
    CONSTRAINT fk_fa_assets_fa_locations_location_id FOREIGN KEY (location_id) REFERENCES public.fa_locations (id) ON DELETE RESTRICT,
    CONSTRAINT fk_fa_assets_hr_departments_department_id FOREIGN KEY (department_id) REFERENCES public.hr_departments (id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS public.fa_asset_documents (
    id integer GENERATED ALWAYS AS IDENTITY,
    asset_id integer NOT NULL,
    document_type character varying(50) NOT NULL,
    file_name character varying(255) NOT NULL,
    file_path character varying(500) NOT NULL,
    notes text,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_fa_asset_documents PRIMARY KEY (id),
    CONSTRAINT fk_fa_asset_documents_fa_assets_asset_id FOREIGN KEY (asset_id) REFERENCES public.fa_assets (id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS public.fa_depreciation_schedules (
    id integer GENERATED ALWAYS AS IDENTITY,
    asset_id integer NOT NULL,
    period_year smallint NOT NULL,
    period_month smallint NOT NULL,
    depreciation_date date NOT NULL,
    depreciation_amount numeric(18,2) NOT NULL,
    accumulated_depreciation numeric(18,2) NOT NULL,
    book_value numeric(18,2) NOT NULL,
    status integer NOT NULL,
    run_id integer,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_fa_depreciation_schedules PRIMARY KEY (id),
    CONSTRAINT ck_fa_depreciation_schedules_period_month CHECK (period_month >= 1 AND period_month <= 12),
    CONSTRAINT ck_fa_depreciation_schedules_amount_non_negative CHECK (depreciation_amount >= 0),
    CONSTRAINT ck_fa_depreciation_schedules_accumulated_non_negative CHECK (accumulated_depreciation >= 0),
    CONSTRAINT ck_fa_depreciation_schedules_book_value_non_negative CHECK (book_value >= 0),
    CONSTRAINT fk_fa_depreciation_schedules_fa_assets_asset_id FOREIGN KEY (asset_id) REFERENCES public.fa_assets (id) ON DELETE CASCADE,
    CONSTRAINT fk_fa_depreciation_schedules_fa_depreciation_runs_run_id FOREIGN KEY (run_id) REFERENCES public.fa_depreciation_runs (id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS public.fa_asset_transfers (
    id integer GENERATED ALWAYS AS IDENTITY,
    transfer_no character varying(30) NOT NULL,
    asset_id integer NOT NULL,
    transfer_date date NOT NULL,
    from_location_id integer NOT NULL,
    to_location_id integer NOT NULL,
    from_department_id integer,
    to_department_id integer,
    reason text,
    status integer NOT NULL,
    approved_by integer,
    approved_at timestamptz,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_fa_asset_transfers PRIMARY KEY (id),
    CONSTRAINT ck_fa_asset_transfers_locations_not_same CHECK (from_location_id <> to_location_id),
    CONSTRAINT fk_fa_asset_transfers_fa_assets_asset_id FOREIGN KEY (asset_id) REFERENCES public.fa_assets (id) ON DELETE RESTRICT,
    CONSTRAINT fk_fa_asset_transfers_fa_locations_from_location_id FOREIGN KEY (from_location_id) REFERENCES public.fa_locations (id) ON DELETE RESTRICT,
    CONSTRAINT fk_fa_asset_transfers_fa_locations_to_location_id FOREIGN KEY (to_location_id) REFERENCES public.fa_locations (id) ON DELETE RESTRICT,
    CONSTRAINT fk_fa_asset_transfers_hr_departments_from_department_id FOREIGN KEY (from_department_id) REFERENCES public.hr_departments (id) ON DELETE SET NULL,
    CONSTRAINT fk_fa_asset_transfers_hr_departments_to_department_id FOREIGN KEY (to_department_id) REFERENCES public.hr_departments (id) ON DELETE SET NULL,
    CONSTRAINT fk_fa_asset_transfers_hr_employees_approved_by FOREIGN KEY (approved_by) REFERENCES public.hr_employees (id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS public.fa_maintenance_orders (
    id integer GENERATED ALWAYS AS IDENTITY,
    work_order_no character varying(30) NOT NULL,
    asset_id integer NOT NULL,
    order_date date NOT NULL,
    maintenance_type integer NOT NULL,
    vendor_name character varying(200),
    cost numeric(18,2) NOT NULL,
    is_capitalized boolean NOT NULL,
    status integer NOT NULL,
    notes text,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_fa_maintenance_orders PRIMARY KEY (id),
    CONSTRAINT ck_fa_maintenance_orders_cost_non_negative CHECK (cost >= 0),
    CONSTRAINT fk_fa_maintenance_orders_fa_assets_asset_id FOREIGN KEY (asset_id) REFERENCES public.fa_assets (id) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS public.fa_disposals (
    id integer GENERATED ALWAYS AS IDENTITY,
    disposal_no character varying(30) NOT NULL,
    asset_id integer NOT NULL,
    disposal_date date NOT NULL,
    disposal_type integer NOT NULL,
    sale_amount numeric(18,2),
    disposal_expense numeric(18,2) NOT NULL DEFAULT 0.0,
    gain_loss_amount numeric(18,2),
    status integer NOT NULL,
    notes text,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_fa_disposals PRIMARY KEY (id),
    CONSTRAINT ck_fa_disposals_sale_amount_non_negative CHECK (sale_amount IS NULL OR sale_amount >= 0),
    CONSTRAINT ck_fa_disposals_expense_non_negative CHECK (disposal_expense >= 0),
    CONSTRAINT fk_fa_disposals_fa_assets_asset_id FOREIGN KEY (asset_id) REFERENCES public.fa_assets (id) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS public.fa_revaluations (
    id integer GENERATED ALWAYS AS IDENTITY,
    revaluation_no character varying(30) NOT NULL,
    asset_id integer NOT NULL,
    revaluation_date date NOT NULL,
    old_book_value numeric(18,2) NOT NULL,
    new_book_value numeric(18,2) NOT NULL,
    impairment_amount numeric(18,2) NOT NULL DEFAULT 0.0,
    status integer NOT NULL,
    notes text,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_fa_revaluations PRIMARY KEY (id),
    CONSTRAINT ck_fa_revaluations_values_non_negative CHECK (old_book_value >= 0 AND new_book_value >= 0 AND impairment_amount >= 0),
    CONSTRAINT fk_fa_revaluations_fa_assets_asset_id FOREIGN KEY (asset_id) REFERENCES public.fa_assets (id) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS public.fa_asset_histories (
    id integer GENERATED ALWAYS AS IDENTITY,
    asset_id integer NOT NULL,
    event_date date NOT NULL,
    event_type integer NOT NULL,
    reference_no character varying(50),
    description text,
    amount_change numeric(18,2),
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_fa_asset_histories PRIMARY KEY (id),
    CONSTRAINT fk_fa_asset_histories_fa_assets_asset_id FOREIGN KEY (asset_id) REFERENCES public.fa_assets (id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_fa_asset_categories_code ON public.fa_asset_categories (code);
CREATE INDEX IF NOT EXISTS ix_fa_asset_categories_name ON public.fa_asset_categories (name);
CREATE INDEX IF NOT EXISTS ix_fa_asset_categories_dep_method ON public.fa_asset_categories (depreciation_method);
CREATE INDEX IF NOT EXISTS ix_fa_asset_categories_asset_account_id ON public.fa_asset_categories (asset_account_id);
CREATE INDEX IF NOT EXISTS ix_fa_asset_categories_acc_dep_account_id ON public.fa_asset_categories (accumulated_depreciation_account_id);
CREATE INDEX IF NOT EXISTS ix_fa_asset_categories_dep_exp_account_id ON public.fa_asset_categories (depreciation_expense_account_id);
CREATE INDEX IF NOT EXISTS ix_fa_asset_categories_is_active ON public.fa_asset_categories (is_active);
CREATE INDEX IF NOT EXISTS ix_fa_asset_categories_is_deleted ON public.fa_asset_categories (is_deleted);
CREATE INDEX IF NOT EXISTS ix_fa_asset_categories_created_at ON public.fa_asset_categories (created_at);

CREATE UNIQUE INDEX IF NOT EXISTS ix_fa_locations_code ON public.fa_locations (code);
CREATE INDEX IF NOT EXISTS ix_fa_locations_name ON public.fa_locations (name);
CREATE INDEX IF NOT EXISTS ix_fa_locations_department_id ON public.fa_locations (department_id);
CREATE INDEX IF NOT EXISTS ix_fa_locations_manager_id ON public.fa_locations (manager_id);
CREATE INDEX IF NOT EXISTS ix_fa_locations_is_active ON public.fa_locations (is_active);
CREATE INDEX IF NOT EXISTS ix_fa_locations_is_deleted ON public.fa_locations (is_deleted);
CREATE INDEX IF NOT EXISTS ix_fa_locations_created_at ON public.fa_locations (created_at);

CREATE INDEX IF NOT EXISTS ix_fa_dep_configs_fiscal_year ON public.fa_depreciation_configs (fiscal_year);
CREATE INDEX IF NOT EXISTS ix_fa_dep_configs_is_active ON public.fa_depreciation_configs (is_active);
CREATE INDEX IF NOT EXISTS ix_fa_dep_configs_is_deleted ON public.fa_depreciation_configs (is_deleted);
CREATE INDEX IF NOT EXISTS ix_fa_dep_configs_created_at ON public.fa_depreciation_configs (created_at);

CREATE UNIQUE INDEX IF NOT EXISTS ix_fa_dep_runs_run_no ON public.fa_depreciation_runs (run_no);
CREATE INDEX IF NOT EXISTS ix_fa_dep_runs_period ON public.fa_depreciation_runs (period_year, period_month);
CREATE INDEX IF NOT EXISTS ix_fa_dep_runs_status ON public.fa_depreciation_runs (status);
CREATE INDEX IF NOT EXISTS ix_fa_dep_runs_approved_by ON public.fa_depreciation_runs (approved_by);
CREATE INDEX IF NOT EXISTS ix_fa_dep_runs_journal_entry_id ON public.fa_depreciation_runs (journal_entry_id);
CREATE INDEX IF NOT EXISTS ix_fa_dep_runs_is_deleted ON public.fa_depreciation_runs (is_deleted);
CREATE INDEX IF NOT EXISTS ix_fa_dep_runs_created_at ON public.fa_depreciation_runs (created_at);

CREATE UNIQUE INDEX IF NOT EXISTS ix_fa_assets_asset_code ON public.fa_assets (asset_code);
CREATE INDEX IF NOT EXISTS ix_fa_assets_name ON public.fa_assets (name);
CREATE INDEX IF NOT EXISTS ix_fa_assets_category_id ON public.fa_assets (category_id);
CREATE INDEX IF NOT EXISTS ix_fa_assets_location_id ON public.fa_assets (location_id);
CREATE INDEX IF NOT EXISTS ix_fa_assets_department_id ON public.fa_assets (department_id);
CREATE INDEX IF NOT EXISTS ix_fa_assets_status ON public.fa_assets (status);
CREATE INDEX IF NOT EXISTS ix_fa_assets_is_active ON public.fa_assets (is_active);
CREATE INDEX IF NOT EXISTS ix_fa_assets_acquisition_date ON public.fa_assets (acquisition_date);
CREATE INDEX IF NOT EXISTS ix_fa_assets_in_service_date ON public.fa_assets (in_service_date);
CREATE INDEX IF NOT EXISTS ix_fa_assets_is_deleted ON public.fa_assets (is_deleted);
CREATE INDEX IF NOT EXISTS ix_fa_assets_created_at ON public.fa_assets (created_at);

CREATE INDEX IF NOT EXISTS ix_fa_asset_docs_asset_id ON public.fa_asset_documents (asset_id);
CREATE INDEX IF NOT EXISTS ix_fa_asset_docs_document_type ON public.fa_asset_documents (document_type);
CREATE INDEX IF NOT EXISTS ix_fa_asset_docs_is_deleted ON public.fa_asset_documents (is_deleted);
CREATE INDEX IF NOT EXISTS ix_fa_asset_docs_created_at ON public.fa_asset_documents (created_at);

CREATE UNIQUE INDEX IF NOT EXISTS ux_fa_dep_schedules_asset_period ON public.fa_depreciation_schedules (asset_id, period_year, period_month);
CREATE INDEX IF NOT EXISTS ix_fa_dep_schedules_run_id ON public.fa_depreciation_schedules (run_id);
CREATE INDEX IF NOT EXISTS ix_fa_dep_schedules_status ON public.fa_depreciation_schedules (status);
CREATE INDEX IF NOT EXISTS ix_fa_dep_schedules_dep_date ON public.fa_depreciation_schedules (depreciation_date);
CREATE INDEX IF NOT EXISTS ix_fa_dep_schedules_is_deleted ON public.fa_depreciation_schedules (is_deleted);
CREATE INDEX IF NOT EXISTS ix_fa_dep_schedules_created_at ON public.fa_depreciation_schedules (created_at);

CREATE UNIQUE INDEX IF NOT EXISTS ix_fa_asset_transfers_transfer_no ON public.fa_asset_transfers (transfer_no);
CREATE INDEX IF NOT EXISTS ix_fa_asset_transfers_asset_id ON public.fa_asset_transfers (asset_id);
CREATE INDEX IF NOT EXISTS ix_fa_asset_transfers_from_location_id ON public.fa_asset_transfers (from_location_id);
CREATE INDEX IF NOT EXISTS ix_fa_asset_transfers_to_location_id ON public.fa_asset_transfers (to_location_id);
CREATE INDEX IF NOT EXISTS ix_fa_asset_transfers_from_department_id ON public.fa_asset_transfers (from_department_id);
CREATE INDEX IF NOT EXISTS ix_fa_asset_transfers_to_department_id ON public.fa_asset_transfers (to_department_id);
CREATE INDEX IF NOT EXISTS ix_fa_asset_transfers_transfer_date ON public.fa_asset_transfers (transfer_date);
CREATE INDEX IF NOT EXISTS ix_fa_asset_transfers_status ON public.fa_asset_transfers (status);
CREATE INDEX IF NOT EXISTS ix_fa_asset_transfers_approved_by ON public.fa_asset_transfers (approved_by);
CREATE INDEX IF NOT EXISTS ix_fa_asset_transfers_is_deleted ON public.fa_asset_transfers (is_deleted);
CREATE INDEX IF NOT EXISTS ix_fa_asset_transfers_created_at ON public.fa_asset_transfers (created_at);

CREATE UNIQUE INDEX IF NOT EXISTS ix_fa_maintenance_orders_work_order_no ON public.fa_maintenance_orders (work_order_no);
CREATE INDEX IF NOT EXISTS ix_fa_maintenance_orders_asset_id ON public.fa_maintenance_orders (asset_id);
CREATE INDEX IF NOT EXISTS ix_fa_maintenance_orders_order_date ON public.fa_maintenance_orders (order_date);
CREATE INDEX IF NOT EXISTS ix_fa_maintenance_orders_status ON public.fa_maintenance_orders (status);
CREATE INDEX IF NOT EXISTS ix_fa_maintenance_orders_is_deleted ON public.fa_maintenance_orders (is_deleted);
CREATE INDEX IF NOT EXISTS ix_fa_maintenance_orders_created_at ON public.fa_maintenance_orders (created_at);

CREATE UNIQUE INDEX IF NOT EXISTS ix_fa_disposals_disposal_no ON public.fa_disposals (disposal_no);
CREATE INDEX IF NOT EXISTS ix_fa_disposals_asset_id ON public.fa_disposals (asset_id);
CREATE INDEX IF NOT EXISTS ix_fa_disposals_disposal_date ON public.fa_disposals (disposal_date);
CREATE INDEX IF NOT EXISTS ix_fa_disposals_status ON public.fa_disposals (status);
CREATE INDEX IF NOT EXISTS ix_fa_disposals_is_deleted ON public.fa_disposals (is_deleted);
CREATE INDEX IF NOT EXISTS ix_fa_disposals_created_at ON public.fa_disposals (created_at);

CREATE UNIQUE INDEX IF NOT EXISTS ix_fa_revaluations_revaluation_no ON public.fa_revaluations (revaluation_no);
CREATE INDEX IF NOT EXISTS ix_fa_revaluations_asset_id ON public.fa_revaluations (asset_id);
CREATE INDEX IF NOT EXISTS ix_fa_revaluations_revaluation_date ON public.fa_revaluations (revaluation_date);
CREATE INDEX IF NOT EXISTS ix_fa_revaluations_status ON public.fa_revaluations (status);
CREATE INDEX IF NOT EXISTS ix_fa_revaluations_is_deleted ON public.fa_revaluations (is_deleted);
CREATE INDEX IF NOT EXISTS ix_fa_revaluations_created_at ON public.fa_revaluations (created_at);

CREATE INDEX IF NOT EXISTS ix_fa_asset_histories_asset_id ON public.fa_asset_histories (asset_id);
CREATE INDEX IF NOT EXISTS ix_fa_asset_histories_event_date ON public.fa_asset_histories (event_date);
CREATE INDEX IF NOT EXISTS ix_fa_asset_histories_event_type ON public.fa_asset_histories (event_type);
CREATE INDEX IF NOT EXISTS ix_fa_asset_histories_is_deleted ON public.fa_asset_histories (is_deleted);
CREATE INDEX IF NOT EXISTS ix_fa_asset_histories_created_at ON public.fa_asset_histories (created_at);
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
DROP TABLE IF EXISTS public.fa_asset_histories;
DROP TABLE IF EXISTS public.fa_revaluations;
DROP TABLE IF EXISTS public.fa_disposals;
DROP TABLE IF EXISTS public.fa_maintenance_orders;
DROP TABLE IF EXISTS public.fa_asset_transfers;
DROP TABLE IF EXISTS public.fa_depreciation_schedules;
DROP TABLE IF EXISTS public.fa_asset_documents;
DROP TABLE IF EXISTS public.fa_assets;
DROP TABLE IF EXISTS public.fa_depreciation_runs;
DROP TABLE IF EXISTS public.fa_depreciation_configs;
DROP TABLE IF EXISTS public.fa_locations;
DROP TABLE IF EXISTS public.fa_asset_categories;
""");
    }
}
