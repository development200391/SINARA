using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260627120000_AddManufacturingPhase39To48")]
public partial class AddManufacturingPhase39To48 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
CREATE TABLE IF NOT EXISTS public.mfg_work_centers (
    id integer GENERATED ALWAYS AS IDENTITY,
    code character varying(30) NOT NULL,
    name character varying(100) NOT NULL,
    capacity_hours_per_day numeric(18,2) NOT NULL,
    labor_cost_per_hour numeric(18,4) NOT NULL DEFAULT 0.0,
    overhead_cost_per_hour numeric(18,4) NOT NULL DEFAULT 0.0,
    wip_account_id integer,
    is_active boolean NOT NULL DEFAULT TRUE,
    notes text,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_mfg_work_centers PRIMARY KEY (id),
    CONSTRAINT ck_mfg_work_centers_capacity_hours_positive CHECK (capacity_hours_per_day > 0),
    CONSTRAINT ck_mfg_work_centers_labor_cost_non_negative CHECK (labor_cost_per_hour >= 0),
    CONSTRAINT ck_mfg_work_centers_overhead_cost_non_negative CHECK (overhead_cost_per_hour >= 0),
    CONSTRAINT fk_mfg_work_centers_fin_accounts_wip_account_id FOREIGN KEY (wip_account_id) REFERENCES public.fin_accounts (id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS public.mfg_routings (
    id integer GENERATED ALWAYS AS IDENTITY,
    code character varying(30) NOT NULL,
    name character varying(100) NOT NULL,
    item_id integer,
    work_center_id integer,
    version integer NOT NULL DEFAULT 1,
    status integer NOT NULL DEFAULT 0,
    total_lead_time_hours numeric(18,2) NOT NULL DEFAULT 0.0,
    is_active boolean NOT NULL DEFAULT TRUE,
    notes text,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_mfg_routings PRIMARY KEY (id),
    CONSTRAINT ck_mfg_routings_version_positive CHECK (version > 0),
    CONSTRAINT ck_mfg_routings_lead_time_non_negative CHECK (total_lead_time_hours >= 0),
    CONSTRAINT fk_mfg_routings_inv_items_item_id FOREIGN KEY (item_id) REFERENCES public.inv_items (id) ON DELETE SET NULL,
    CONSTRAINT fk_mfg_routings_mfg_work_centers_work_center_id FOREIGN KEY (work_center_id) REFERENCES public.mfg_work_centers (id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS public.mfg_boms (
    id integer GENERATED ALWAYS AS IDENTITY,
    code character varying(30) NOT NULL,
    item_id integer,
    routing_id integer,
    version integer NOT NULL DEFAULT 1,
    status integer NOT NULL DEFAULT 0,
    qty_produced numeric(18,4) NOT NULL DEFAULT 1.0,
    standard_cost numeric(18,4) NOT NULL DEFAULT 0.0,
    effective_date date NOT NULL,
    is_active boolean NOT NULL DEFAULT TRUE,
    notes text,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_mfg_boms PRIMARY KEY (id),
    CONSTRAINT ck_mfg_boms_version_positive CHECK (version > 0),
    CONSTRAINT ck_mfg_boms_qty_produced_positive CHECK (qty_produced > 0),
    CONSTRAINT ck_mfg_boms_standard_cost_non_negative CHECK (standard_cost >= 0),
    CONSTRAINT fk_mfg_boms_inv_items_item_id FOREIGN KEY (item_id) REFERENCES public.inv_items (id) ON DELETE SET NULL,
    CONSTRAINT fk_mfg_boms_mfg_routings_routing_id FOREIGN KEY (routing_id) REFERENCES public.mfg_routings (id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS public.mfg_mrp_runs (
    id integer GENERATED ALWAYS AS IDENTITY,
    code character varying(30) NOT NULL,
    run_date date NOT NULL,
    status integer NOT NULL DEFAULT 0,
    horizon_days integer NOT NULL DEFAULT 30,
    total_demand_items integer NOT NULL DEFAULT 0,
    recommended_wo_count integer NOT NULL DEFAULT 0,
    recommended_pr_count integer NOT NULL DEFAULT 0,
    started_at timestamptz,
    completed_at timestamptz,
    notes text,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_mfg_mrp_runs PRIMARY KEY (id),
    CONSTRAINT ck_mfg_mrp_runs_horizon_days_positive CHECK (horizon_days > 0),
    CONSTRAINT ck_mfg_mrp_runs_total_demand_items_non_negative CHECK (total_demand_items >= 0),
    CONSTRAINT ck_mfg_mrp_runs_recommended_wo_non_negative CHECK (recommended_wo_count >= 0),
    CONSTRAINT ck_mfg_mrp_runs_recommended_pr_non_negative CHECK (recommended_pr_count >= 0),
    CONSTRAINT ck_mfg_mrp_runs_completed_after_started CHECK (completed_at IS NULL OR started_at IS NULL OR completed_at >= started_at)
);

CREATE TABLE IF NOT EXISTS public.mfg_work_orders (
    id integer GENERATED ALWAYS AS IDENTITY,
    code character varying(30) NOT NULL,
    item_id integer,
    bom_id integer,
    routing_id integer,
    work_center_id integer,
    mrp_run_id integer,
    status integer NOT NULL DEFAULT 0,
    production_type integer NOT NULL DEFAULT 0,
    qty_planned numeric(18,4) NOT NULL,
    qty_good numeric(18,4) NOT NULL DEFAULT 0.0,
    qty_scrap numeric(18,4) NOT NULL DEFAULT 0.0,
    planned_start_date date NOT NULL,
    planned_end_date date NOT NULL,
    actual_start_at timestamptz,
    actual_end_at timestamptz,
    standard_cost_total numeric(18,4) NOT NULL DEFAULT 0.0,
    actual_cost_total numeric(18,4) NOT NULL DEFAULT 0.0,
    is_active boolean NOT NULL DEFAULT TRUE,
    notes text,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_mfg_work_orders PRIMARY KEY (id),
    CONSTRAINT ck_mfg_work_orders_qty_planned_positive CHECK (qty_planned > 0),
    CONSTRAINT ck_mfg_work_orders_qty_good_non_negative CHECK (qty_good >= 0),
    CONSTRAINT ck_mfg_work_orders_qty_scrap_non_negative CHECK (qty_scrap >= 0),
    CONSTRAINT ck_mfg_work_orders_plan_date_range CHECK (planned_end_date >= planned_start_date),
    CONSTRAINT ck_mfg_work_orders_standard_cost_non_negative CHECK (standard_cost_total >= 0),
    CONSTRAINT ck_mfg_work_orders_actual_cost_non_negative CHECK (actual_cost_total >= 0),
    CONSTRAINT fk_mfg_work_orders_inv_items_item_id FOREIGN KEY (item_id) REFERENCES public.inv_items (id) ON DELETE SET NULL,
    CONSTRAINT fk_mfg_work_orders_mfg_boms_bom_id FOREIGN KEY (bom_id) REFERENCES public.mfg_boms (id) ON DELETE SET NULL,
    CONSTRAINT fk_mfg_work_orders_mfg_routings_routing_id FOREIGN KEY (routing_id) REFERENCES public.mfg_routings (id) ON DELETE SET NULL,
    CONSTRAINT fk_mfg_work_orders_mfg_work_centers_work_center_id FOREIGN KEY (work_center_id) REFERENCES public.mfg_work_centers (id) ON DELETE SET NULL,
    CONSTRAINT fk_mfg_work_orders_mfg_mrp_runs_mrp_run_id FOREIGN KEY (mrp_run_id) REFERENCES public.mfg_mrp_runs (id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS public.mfg_qc_parameters (
    id integer GENERATED ALWAYS AS IDENTITY,
    code character varying(30) NOT NULL,
    name character varying(100) NOT NULL,
    item_id integer,
    parameter_type integer NOT NULL DEFAULT 0,
    min_value numeric(18,4),
    max_value numeric(18,4),
    is_critical boolean NOT NULL DEFAULT FALSE,
    is_active boolean NOT NULL DEFAULT TRUE,
    notes text,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_mfg_qc_parameters PRIMARY KEY (id),
    CONSTRAINT ck_mfg_qc_parameters_min_max CHECK (min_value IS NULL OR max_value IS NULL OR min_value <= max_value),
    CONSTRAINT fk_mfg_qc_parameters_inv_items_item_id FOREIGN KEY (item_id) REFERENCES public.inv_items (id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS public.mfg_qc_inspections (
    id integer GENERATED ALWAYS AS IDENTITY,
    code character varying(30) NOT NULL,
    work_order_id integer,
    item_id integer,
    inspector_employee_id integer,
    inspected_at timestamptz NOT NULL,
    status integer NOT NULL DEFAULT 0,
    result integer NOT NULL DEFAULT 0,
    notes text,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_mfg_qc_inspections PRIMARY KEY (id),
    CONSTRAINT fk_mfg_qc_inspections_mfg_work_orders_work_order_id FOREIGN KEY (work_order_id) REFERENCES public.mfg_work_orders (id) ON DELETE SET NULL,
    CONSTRAINT fk_mfg_qc_inspections_inv_items_item_id FOREIGN KEY (item_id) REFERENCES public.inv_items (id) ON DELETE SET NULL,
    CONSTRAINT fk_mfg_qc_inspections_hr_employees_inspector_employee_id FOREIGN KEY (inspector_employee_id) REFERENCES public.hr_employees (id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS public.mfg_scrap_records (
    id integer GENERATED ALWAYS AS IDENTITY,
    code character varying(30) NOT NULL,
    work_order_id integer,
    item_id integer,
    work_center_id integer,
    reason integer NOT NULL DEFAULT 5,
    qty_scrap numeric(18,4) NOT NULL,
    unit_cost numeric(18,4) NOT NULL DEFAULT 0.0,
    total_scrap_cost numeric(18,4) NOT NULL DEFAULT 0.0,
    recorded_at timestamptz NOT NULL,
    notes text,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_mfg_scrap_records PRIMARY KEY (id),
    CONSTRAINT ck_mfg_scrap_records_qty_positive CHECK (qty_scrap > 0),
    CONSTRAINT ck_mfg_scrap_records_unit_cost_non_negative CHECK (unit_cost >= 0),
    CONSTRAINT ck_mfg_scrap_records_total_cost_non_negative CHECK (total_scrap_cost >= 0),
    CONSTRAINT fk_mfg_scrap_records_mfg_work_orders_work_order_id FOREIGN KEY (work_order_id) REFERENCES public.mfg_work_orders (id) ON DELETE SET NULL,
    CONSTRAINT fk_mfg_scrap_records_inv_items_item_id FOREIGN KEY (item_id) REFERENCES public.inv_items (id) ON DELETE SET NULL,
    CONSTRAINT fk_mfg_scrap_records_mfg_work_centers_work_center_id FOREIGN KEY (work_center_id) REFERENCES public.mfg_work_centers (id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS public.mfg_rework_orders (
    id integer GENERATED ALWAYS AS IDENTITY,
    code character varying(30) NOT NULL,
    source_work_order_id integer,
    work_order_id integer,
    item_id integer,
    qty_rework numeric(18,4) NOT NULL,
    status integer NOT NULL DEFAULT 0,
    opened_at timestamptz NOT NULL,
    closed_at timestamptz,
    notes text,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_mfg_rework_orders PRIMARY KEY (id),
    CONSTRAINT ck_mfg_rework_orders_qty_positive CHECK (qty_rework > 0),
    CONSTRAINT ck_mfg_rework_orders_closed_after_opened CHECK (closed_at IS NULL OR closed_at >= opened_at),
    CONSTRAINT fk_mfg_rework_orders_mfg_work_orders_source_work_order_id FOREIGN KEY (source_work_order_id) REFERENCES public.mfg_work_orders (id) ON DELETE SET NULL,
    CONSTRAINT fk_mfg_rework_orders_mfg_work_orders_work_order_id FOREIGN KEY (work_order_id) REFERENCES public.mfg_work_orders (id) ON DELETE SET NULL,
    CONSTRAINT fk_mfg_rework_orders_inv_items_item_id FOREIGN KEY (item_id) REFERENCES public.inv_items (id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS public.mfg_oee_snapshots (
    id integer GENERATED ALWAYS AS IDENTITY,
    snapshot_date date NOT NULL,
    work_center_id integer NOT NULL,
    availability_pct numeric(5,2) NOT NULL,
    performance_pct numeric(5,2) NOT NULL,
    quality_pct numeric(5,2) NOT NULL,
    oee_pct numeric(5,2) NOT NULL,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_mfg_oee_snapshots PRIMARY KEY (id),
    CONSTRAINT ck_mfg_oee_snapshots_availability_range CHECK (availability_pct >= 0 AND availability_pct <= 100),
    CONSTRAINT ck_mfg_oee_snapshots_performance_range CHECK (performance_pct >= 0 AND performance_pct <= 100),
    CONSTRAINT ck_mfg_oee_snapshots_quality_range CHECK (quality_pct >= 0 AND quality_pct <= 100),
    CONSTRAINT ck_mfg_oee_snapshots_oee_range CHECK (oee_pct >= 0 AND oee_pct <= 100),
    CONSTRAINT fk_mfg_oee_snapshots_mfg_work_centers_work_center_id FOREIGN KEY (work_center_id) REFERENCES public.mfg_work_centers (id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_mfg_work_centers_created_at ON public.mfg_work_centers (created_at);
CREATE INDEX IF NOT EXISTS ix_mfg_work_centers_is_deleted ON public.mfg_work_centers (is_deleted);
CREATE UNIQUE INDEX IF NOT EXISTS ix_mfg_work_centers_code ON public.mfg_work_centers (code);
CREATE INDEX IF NOT EXISTS ix_mfg_work_centers_name ON public.mfg_work_centers (name);
CREATE INDEX IF NOT EXISTS ix_mfg_work_centers_wip_account_id ON public.mfg_work_centers (wip_account_id);
CREATE INDEX IF NOT EXISTS ix_mfg_work_centers_is_active ON public.mfg_work_centers (is_active);

CREATE INDEX IF NOT EXISTS ix_mfg_routings_created_at ON public.mfg_routings (created_at);
CREATE INDEX IF NOT EXISTS ix_mfg_routings_is_deleted ON public.mfg_routings (is_deleted);
CREATE UNIQUE INDEX IF NOT EXISTS ix_mfg_routings_code ON public.mfg_routings (code);
CREATE INDEX IF NOT EXISTS ix_mfg_routings_name ON public.mfg_routings (name);
CREATE INDEX IF NOT EXISTS ix_mfg_routings_item_id ON public.mfg_routings (item_id);
CREATE INDEX IF NOT EXISTS ix_mfg_routings_work_center_id ON public.mfg_routings (work_center_id);
CREATE INDEX IF NOT EXISTS ix_mfg_routings_status ON public.mfg_routings (status);
CREATE INDEX IF NOT EXISTS ix_mfg_routings_is_active ON public.mfg_routings (is_active);

CREATE INDEX IF NOT EXISTS ix_mfg_boms_created_at ON public.mfg_boms (created_at);
CREATE INDEX IF NOT EXISTS ix_mfg_boms_is_deleted ON public.mfg_boms (is_deleted);
CREATE UNIQUE INDEX IF NOT EXISTS ix_mfg_boms_code ON public.mfg_boms (code);
CREATE INDEX IF NOT EXISTS ix_mfg_boms_item_id ON public.mfg_boms (item_id);
CREATE INDEX IF NOT EXISTS ix_mfg_boms_routing_id ON public.mfg_boms (routing_id);
CREATE INDEX IF NOT EXISTS ix_mfg_boms_status ON public.mfg_boms (status);
CREATE INDEX IF NOT EXISTS ix_mfg_boms_effective_date ON public.mfg_boms (effective_date);
CREATE INDEX IF NOT EXISTS ix_mfg_boms_is_active ON public.mfg_boms (is_active);

CREATE INDEX IF NOT EXISTS ix_mfg_mrp_runs_created_at ON public.mfg_mrp_runs (created_at);
CREATE INDEX IF NOT EXISTS ix_mfg_mrp_runs_is_deleted ON public.mfg_mrp_runs (is_deleted);
CREATE UNIQUE INDEX IF NOT EXISTS ix_mfg_mrp_runs_code ON public.mfg_mrp_runs (code);
CREATE INDEX IF NOT EXISTS ix_mfg_mrp_runs_run_date ON public.mfg_mrp_runs (run_date);
CREATE INDEX IF NOT EXISTS ix_mfg_mrp_runs_status ON public.mfg_mrp_runs (status);

CREATE INDEX IF NOT EXISTS ix_mfg_work_orders_created_at ON public.mfg_work_orders (created_at);
CREATE INDEX IF NOT EXISTS ix_mfg_work_orders_is_deleted ON public.mfg_work_orders (is_deleted);
CREATE UNIQUE INDEX IF NOT EXISTS ix_mfg_work_orders_code ON public.mfg_work_orders (code);
CREATE INDEX IF NOT EXISTS ix_mfg_work_orders_item_id ON public.mfg_work_orders (item_id);
CREATE INDEX IF NOT EXISTS ix_mfg_work_orders_bom_id ON public.mfg_work_orders (bom_id);
CREATE INDEX IF NOT EXISTS ix_mfg_work_orders_routing_id ON public.mfg_work_orders (routing_id);
CREATE INDEX IF NOT EXISTS ix_mfg_work_orders_work_center_id ON public.mfg_work_orders (work_center_id);
CREATE INDEX IF NOT EXISTS ix_mfg_work_orders_mrp_run_id ON public.mfg_work_orders (mrp_run_id);
CREATE INDEX IF NOT EXISTS ix_mfg_work_orders_status ON public.mfg_work_orders (status);
CREATE INDEX IF NOT EXISTS ix_mfg_work_orders_planned_start_date ON public.mfg_work_orders (planned_start_date);
CREATE INDEX IF NOT EXISTS ix_mfg_work_orders_is_active ON public.mfg_work_orders (is_active);

CREATE INDEX IF NOT EXISTS ix_mfg_qc_parameters_created_at ON public.mfg_qc_parameters (created_at);
CREATE INDEX IF NOT EXISTS ix_mfg_qc_parameters_is_deleted ON public.mfg_qc_parameters (is_deleted);
CREATE UNIQUE INDEX IF NOT EXISTS ix_mfg_qc_parameters_code ON public.mfg_qc_parameters (code);
CREATE INDEX IF NOT EXISTS ix_mfg_qc_parameters_name ON public.mfg_qc_parameters (name);
CREATE INDEX IF NOT EXISTS ix_mfg_qc_parameters_item_id ON public.mfg_qc_parameters (item_id);
CREATE INDEX IF NOT EXISTS ix_mfg_qc_parameters_parameter_type ON public.mfg_qc_parameters (parameter_type);
CREATE INDEX IF NOT EXISTS ix_mfg_qc_parameters_is_critical ON public.mfg_qc_parameters (is_critical);
CREATE INDEX IF NOT EXISTS ix_mfg_qc_parameters_is_active ON public.mfg_qc_parameters (is_active);

CREATE INDEX IF NOT EXISTS ix_mfg_qc_inspections_created_at ON public.mfg_qc_inspections (created_at);
CREATE INDEX IF NOT EXISTS ix_mfg_qc_inspections_is_deleted ON public.mfg_qc_inspections (is_deleted);
CREATE UNIQUE INDEX IF NOT EXISTS ix_mfg_qc_inspections_code ON public.mfg_qc_inspections (code);
CREATE INDEX IF NOT EXISTS ix_mfg_qc_inspections_work_order_id ON public.mfg_qc_inspections (work_order_id);
CREATE INDEX IF NOT EXISTS ix_mfg_qc_inspections_item_id ON public.mfg_qc_inspections (item_id);
CREATE INDEX IF NOT EXISTS ix_mfg_qc_inspections_inspector_employee_id ON public.mfg_qc_inspections (inspector_employee_id);
CREATE INDEX IF NOT EXISTS ix_mfg_qc_inspections_status ON public.mfg_qc_inspections (status);
CREATE INDEX IF NOT EXISTS ix_mfg_qc_inspections_result ON public.mfg_qc_inspections (result);
CREATE INDEX IF NOT EXISTS ix_mfg_qc_inspections_inspected_at ON public.mfg_qc_inspections (inspected_at);

CREATE INDEX IF NOT EXISTS ix_mfg_scrap_records_created_at ON public.mfg_scrap_records (created_at);
CREATE INDEX IF NOT EXISTS ix_mfg_scrap_records_is_deleted ON public.mfg_scrap_records (is_deleted);
CREATE UNIQUE INDEX IF NOT EXISTS ix_mfg_scrap_records_code ON public.mfg_scrap_records (code);
CREATE INDEX IF NOT EXISTS ix_mfg_scrap_records_work_order_id ON public.mfg_scrap_records (work_order_id);
CREATE INDEX IF NOT EXISTS ix_mfg_scrap_records_item_id ON public.mfg_scrap_records (item_id);
CREATE INDEX IF NOT EXISTS ix_mfg_scrap_records_work_center_id ON public.mfg_scrap_records (work_center_id);
CREATE INDEX IF NOT EXISTS ix_mfg_scrap_records_reason ON public.mfg_scrap_records (reason);
CREATE INDEX IF NOT EXISTS ix_mfg_scrap_records_recorded_at ON public.mfg_scrap_records (recorded_at);

CREATE INDEX IF NOT EXISTS ix_mfg_rework_orders_created_at ON public.mfg_rework_orders (created_at);
CREATE INDEX IF NOT EXISTS ix_mfg_rework_orders_is_deleted ON public.mfg_rework_orders (is_deleted);
CREATE UNIQUE INDEX IF NOT EXISTS ix_mfg_rework_orders_code ON public.mfg_rework_orders (code);
CREATE INDEX IF NOT EXISTS ix_mfg_rework_orders_source_work_order_id ON public.mfg_rework_orders (source_work_order_id);
CREATE INDEX IF NOT EXISTS ix_mfg_rework_orders_work_order_id ON public.mfg_rework_orders (work_order_id);
CREATE INDEX IF NOT EXISTS ix_mfg_rework_orders_item_id ON public.mfg_rework_orders (item_id);
CREATE INDEX IF NOT EXISTS ix_mfg_rework_orders_status ON public.mfg_rework_orders (status);
CREATE INDEX IF NOT EXISTS ix_mfg_rework_orders_opened_at ON public.mfg_rework_orders (opened_at);
CREATE INDEX IF NOT EXISTS ix_mfg_rework_orders_closed_at ON public.mfg_rework_orders (closed_at);

CREATE INDEX IF NOT EXISTS ix_mfg_oee_snapshots_created_at ON public.mfg_oee_snapshots (created_at);
CREATE INDEX IF NOT EXISTS ix_mfg_oee_snapshots_is_deleted ON public.mfg_oee_snapshots (is_deleted);
CREATE INDEX IF NOT EXISTS ix_mfg_oee_snapshots_work_center_id ON public.mfg_oee_snapshots (work_center_id);
CREATE INDEX IF NOT EXISTS ix_mfg_oee_snapshots_snapshot_date ON public.mfg_oee_snapshots (snapshot_date);
CREATE UNIQUE INDEX IF NOT EXISTS ix_mfg_oee_snapshots_work_center_id_snapshot_date ON public.mfg_oee_snapshots (work_center_id, snapshot_date);
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
DROP TABLE IF EXISTS public.mfg_oee_snapshots;
DROP TABLE IF EXISTS public.mfg_rework_orders;
DROP TABLE IF EXISTS public.mfg_scrap_records;
DROP TABLE IF EXISTS public.mfg_qc_inspections;
DROP TABLE IF EXISTS public.mfg_qc_parameters;
DROP TABLE IF EXISTS public.mfg_work_orders;
DROP TABLE IF EXISTS public.mfg_mrp_runs;
DROP TABLE IF EXISTS public.mfg_boms;
DROP TABLE IF EXISTS public.mfg_routings;
DROP TABLE IF EXISTS public.mfg_work_centers;
""");
    }
}
