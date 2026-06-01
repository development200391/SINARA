using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260601101000_AddInventoryPhase15To22")]
public partial class AddInventoryPhase15To22 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
CREATE TABLE IF NOT EXISTS public.inv_brands (
    id integer GENERATED ALWAYS AS IDENTITY,
    name character varying(100) NOT NULL,
    description text,
    is_active boolean NOT NULL DEFAULT TRUE,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_inv_brands PRIMARY KEY (id)
);
CREATE TABLE IF NOT EXISTS public.inv_item_categories (
    id integer GENERATED ALWAYS AS IDENTITY,
    code character varying(30) NOT NULL,
    name character varying(100) NOT NULL,
    parent_category_id integer,
    description text,
    is_active boolean NOT NULL DEFAULT TRUE,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_inv_item_categories PRIMARY KEY (id),
    CONSTRAINT fk_inv_item_categories_inv_item_categories_parent_category_id FOREIGN KEY (parent_category_id) REFERENCES public.inv_item_categories (id) ON DELETE RESTRICT
);
CREATE TABLE IF NOT EXISTS public.inv_units_of_measure (
    id integer GENERATED ALWAYS AS IDENTITY,
    code character varying(20) NOT NULL,
    name character varying(50) NOT NULL,
    description text,
    is_active boolean NOT NULL DEFAULT TRUE,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_inv_units_of_measure PRIMARY KEY (id)
);
CREATE TABLE IF NOT EXISTS public.inv_items (
    id integer GENERATED ALWAYS AS IDENTITY,
    item_code character varying(30) NOT NULL,
    sku character varying(100),
    name character varying(200) NOT NULL,
    description text,
    category_id integer NOT NULL,
    brand_id integer,
    type integer NOT NULL DEFAULT 0,
    base_uom_id integer NOT NULL,
    purchase_uom_id integer,
    status integer NOT NULL DEFAULT 0,
    valuation_method integer NOT NULL DEFAULT 0,
    last_purchase_price numeric(18,4),
    avg_cost numeric(18,4) NOT NULL DEFAULT 0.0,
    min_stock numeric(18,4) NOT NULL DEFAULT 0.0,
    max_stock numeric(18,4) NOT NULL DEFAULT 0.0,
    reorder_point numeric(18,4) NOT NULL DEFAULT 0.0,
    lead_time_days integer NOT NULL DEFAULT 0,
    account_inventory_id integer,
    account_cogs_id integer,
    account_adjustment_id integer,
    notes text,
    is_active boolean NOT NULL DEFAULT TRUE,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_inv_items PRIMARY KEY (id),
    CONSTRAINT ck_inv_items_avg_cost_non_negative CHECK (avg_cost >= 0),
    CONSTRAINT ck_inv_items_last_purchase_price_non_negative CHECK (last_purchase_price IS NULL OR last_purchase_price >= 0),
    CONSTRAINT ck_inv_items_lead_time_days_non_negative CHECK (lead_time_days >= 0),
    CONSTRAINT ck_inv_items_max_stock_non_negative CHECK (max_stock >= 0),
    CONSTRAINT ck_inv_items_min_stock_non_negative CHECK (min_stock >= 0),
    CONSTRAINT ck_inv_items_reorder_point_non_negative CHECK (reorder_point >= 0),
    CONSTRAINT fk_inv_items_fin_accounts_adjustment_account_id FOREIGN KEY (account_adjustment_id) REFERENCES public.fin_accounts (id) ON DELETE SET NULL,
    CONSTRAINT fk_inv_items_fin_accounts_cogs_account_id FOREIGN KEY (account_cogs_id) REFERENCES public.fin_accounts (id) ON DELETE SET NULL,
    CONSTRAINT fk_inv_items_fin_accounts_inventory_account_id FOREIGN KEY (account_inventory_id) REFERENCES public.fin_accounts (id) ON DELETE SET NULL,
    CONSTRAINT fk_inv_items_inv_brands_brand_id FOREIGN KEY (brand_id) REFERENCES public.inv_brands (id) ON DELETE SET NULL,
    CONSTRAINT fk_inv_items_inv_item_categories_category_id FOREIGN KEY (category_id) REFERENCES public.inv_item_categories (id) ON DELETE RESTRICT,
    CONSTRAINT fk_inv_items_inv_units_of_measure_base_uom_id FOREIGN KEY (base_uom_id) REFERENCES public.inv_units_of_measure (id) ON DELETE RESTRICT,
    CONSTRAINT fk_inv_items_inv_units_of_measure_purchase_uom_id FOREIGN KEY (purchase_uom_id) REFERENCES public.inv_units_of_measure (id) ON DELETE SET NULL
);
CREATE TABLE IF NOT EXISTS public.inv_item_unit_conversions (
    id integer GENERATED ALWAYS AS IDENTITY,
    item_id integer NOT NULL,
    from_uom_id integer NOT NULL,
    to_uom_id integer NOT NULL,
    conversion_factor numeric(18,6) NOT NULL,
    is_active boolean NOT NULL DEFAULT TRUE,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_inv_item_unit_conversions PRIMARY KEY (id),
    CONSTRAINT ck_inv_item_unit_conversions_factor_positive CHECK (conversion_factor > 0),
    CONSTRAINT fk_inv_item_unit_conversions_inv_items_item_id FOREIGN KEY (item_id) REFERENCES public.inv_items (id) ON DELETE CASCADE,
    CONSTRAINT fk_inv_item_unit_conversions_inv_units_of_measure_from_uom_id FOREIGN KEY (from_uom_id) REFERENCES public.inv_units_of_measure (id) ON DELETE RESTRICT,
    CONSTRAINT fk_inv_item_unit_conversions_inv_units_of_measure_to_uom_id FOREIGN KEY (to_uom_id) REFERENCES public.inv_units_of_measure (id) ON DELETE RESTRICT
);
CREATE TABLE IF NOT EXISTS public.inv_warehouses (
    id integer GENERATED ALWAYS AS IDENTITY,
    code character varying(20) NOT NULL,
    name character varying(100) NOT NULL,
    description text,
    address text,
    phone character varying(30),
    manager_id integer,
    cost_center_id integer,
    is_transit boolean NOT NULL DEFAULT FALSE,
    is_active boolean NOT NULL DEFAULT TRUE,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_inv_warehouses PRIMARY KEY (id),
    CONSTRAINT fk_inv_warehouses_fin_cost_centers_cost_center_id FOREIGN KEY (cost_center_id) REFERENCES public.fin_cost_centers (id) ON DELETE SET NULL,
    CONSTRAINT fk_inv_warehouses_hr_employees_manager_id FOREIGN KEY (manager_id) REFERENCES public.hr_employees (id) ON DELETE SET NULL
);
CREATE TABLE IF NOT EXISTS public.inv_warehouse_locations (
    id integer GENERATED ALWAYS AS IDENTITY,
    warehouse_id integer NOT NULL,
    code character varying(30) NOT NULL,
    name character varying(100) NOT NULL,
    description text,
    is_default boolean NOT NULL DEFAULT FALSE,
    is_active boolean NOT NULL DEFAULT TRUE,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_inv_warehouse_locations PRIMARY KEY (id),
    CONSTRAINT fk_inv_warehouse_locations_inv_warehouses_warehouse_id FOREIGN KEY (warehouse_id) REFERENCES public.inv_warehouses (id) ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS public.inv_goods_issues (
    id integer GENERATED ALWAYS AS IDENTITY,
    issue_no character varying(30) NOT NULL,
    issue_date date NOT NULL,
    issue_type integer NOT NULL,
    warehouse_id integer NOT NULL,
    location_id integer,
    department_id integer,
    cost_center_id integer,
    reference_no character varying(100),
    description text,
    status integer NOT NULL,
    requested_by integer,
    issued_by integer,
    confirmed_by integer,
    confirmed_at timestamptz,
    journal_entry_id integer,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_inv_goods_issues PRIMARY KEY (id),
    CONSTRAINT fk_inv_goods_issues_fin_cost_centers_cost_center_id FOREIGN KEY (cost_center_id) REFERENCES public.fin_cost_centers (id) ON DELETE SET NULL,
    CONSTRAINT fk_inv_goods_issues_fin_journal_entries_journal_entry_id FOREIGN KEY (journal_entry_id) REFERENCES public.fin_journal_entries (id) ON DELETE SET NULL,
    CONSTRAINT fk_inv_goods_issues_hr_departments_department_id FOREIGN KEY (department_id) REFERENCES public.hr_departments (id) ON DELETE SET NULL,
    CONSTRAINT fk_inv_goods_issues_inv_warehouse_locations_location_id FOREIGN KEY (location_id) REFERENCES public.inv_warehouse_locations (id) ON DELETE SET NULL,
    CONSTRAINT fk_inv_goods_issues_inv_warehouses_warehouse_id FOREIGN KEY (warehouse_id) REFERENCES public.inv_warehouses (id) ON DELETE RESTRICT,
    CONSTRAINT fk_inv_goods_issues_sys_users_confirmed_by FOREIGN KEY (confirmed_by) REFERENCES public.sys_users (id) ON DELETE SET NULL,
    CONSTRAINT fk_inv_goods_issues_sys_users_issued_by FOREIGN KEY (issued_by) REFERENCES public.sys_users (id) ON DELETE SET NULL,
    CONSTRAINT fk_inv_goods_issues_sys_users_requested_by FOREIGN KEY (requested_by) REFERENCES public.sys_users (id) ON DELETE SET NULL
);
CREATE TABLE IF NOT EXISTS public.inv_goods_receipts (
    id integer GENERATED ALWAYS AS IDENTITY,
    receipt_no character varying(30) NOT NULL,
    receipt_date date NOT NULL,
    receipt_type integer NOT NULL,
    warehouse_id integer NOT NULL,
    location_id integer,
    supplier_name character varying(200),
    reference_no character varying(100),
    description text,
    status integer NOT NULL,
    received_by integer,
    confirmed_by integer,
    confirmed_at timestamptz,
    journal_entry_id integer,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_inv_goods_receipts PRIMARY KEY (id),
    CONSTRAINT fk_inv_goods_receipts_fin_journal_entries_journal_entry_id FOREIGN KEY (journal_entry_id) REFERENCES public.fin_journal_entries (id) ON DELETE SET NULL,
    CONSTRAINT fk_inv_goods_receipts_inv_warehouse_locations_location_id FOREIGN KEY (location_id) REFERENCES public.inv_warehouse_locations (id) ON DELETE SET NULL,
    CONSTRAINT fk_inv_goods_receipts_inv_warehouses_warehouse_id FOREIGN KEY (warehouse_id) REFERENCES public.inv_warehouses (id) ON DELETE RESTRICT,
    CONSTRAINT fk_inv_goods_receipts_sys_users_confirmed_by FOREIGN KEY (confirmed_by) REFERENCES public.sys_users (id) ON DELETE SET NULL,
    CONSTRAINT fk_inv_goods_receipts_sys_users_received_by FOREIGN KEY (received_by) REFERENCES public.sys_users (id) ON DELETE SET NULL
);
CREATE TABLE IF NOT EXISTS public.inv_stock_adjustments (
    id integer GENERATED ALWAYS AS IDENTITY,
    adjustment_no character varying(30) NOT NULL,
    adjustment_date date NOT NULL,
    warehouse_id integer NOT NULL,
    location_id integer,
    reason integer NOT NULL,
    reference_no character varying(100),
    description text,
    status integer NOT NULL,
    requested_by integer,
    approved_by integer,
    approved_at timestamptz,
    confirmed_by integer,
    confirmed_at timestamptz,
    journal_entry_id integer,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_inv_stock_adjustments PRIMARY KEY (id),
    CONSTRAINT fk_inv_stock_adjustments_fin_journal_entries_journal_entry_id FOREIGN KEY (journal_entry_id) REFERENCES public.fin_journal_entries (id) ON DELETE SET NULL,
    CONSTRAINT fk_inv_stock_adjustments_inv_warehouse_locations_location_id FOREIGN KEY (location_id) REFERENCES public.inv_warehouse_locations (id) ON DELETE SET NULL,
    CONSTRAINT fk_inv_stock_adjustments_inv_warehouses_warehouse_id FOREIGN KEY (warehouse_id) REFERENCES public.inv_warehouses (id) ON DELETE RESTRICT,
    CONSTRAINT fk_inv_stock_adjustments_sys_users_approved_by FOREIGN KEY (approved_by) REFERENCES public.sys_users (id) ON DELETE SET NULL,
    CONSTRAINT fk_inv_stock_adjustments_sys_users_confirmed_by FOREIGN KEY (confirmed_by) REFERENCES public.sys_users (id) ON DELETE SET NULL,
    CONSTRAINT fk_inv_stock_adjustments_sys_users_requested_by FOREIGN KEY (requested_by) REFERENCES public.sys_users (id) ON DELETE SET NULL
);
CREATE TABLE IF NOT EXISTS public.inv_stock_balances (
    id integer GENERATED ALWAYS AS IDENTITY,
    item_id integer NOT NULL,
    warehouse_id integer NOT NULL,
    location_id integer,
    qty_on_hand numeric(18,4) NOT NULL DEFAULT 0.0,
    qty_reserved numeric(18,4) NOT NULL DEFAULT 0.0,
    qty_available numeric(18,4) GENERATED ALWAYS AS (qty_on_hand - qty_reserved) STORED NOT NULL,
    avg_cost numeric(18,4) NOT NULL DEFAULT 0.0,
    total_value numeric(18,4) GENERATED ALWAYS AS (qty_on_hand * avg_cost) STORED NOT NULL,
    last_movement_at timestamptz,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_inv_stock_balances PRIMARY KEY (id),
    CONSTRAINT ck_inv_stock_balances_avg_cost_non_negative CHECK (avg_cost >= 0),
    CONSTRAINT ck_inv_stock_balances_qty_available_non_negative CHECK (qty_on_hand - qty_reserved >= 0),
    CONSTRAINT ck_inv_stock_balances_qty_on_hand_non_negative CHECK (qty_on_hand >= 0),
    CONSTRAINT ck_inv_stock_balances_qty_reserved_non_negative CHECK (qty_reserved >= 0),
    CONSTRAINT fk_inv_stock_balances_inv_items_item_id FOREIGN KEY (item_id) REFERENCES public.inv_items (id) ON DELETE CASCADE,
    CONSTRAINT fk_inv_stock_balances_inv_warehouse_locations_location_id FOREIGN KEY (location_id) REFERENCES public.inv_warehouse_locations (id) ON DELETE SET NULL,
    CONSTRAINT fk_inv_stock_balances_inv_warehouses_warehouse_id FOREIGN KEY (warehouse_id) REFERENCES public.inv_warehouses (id) ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS public.inv_stock_movements (
    id integer GENERATED ALWAYS AS IDENTITY,
    movement_date date NOT NULL,
    item_id integer NOT NULL,
    warehouse_id integer NOT NULL,
    location_id integer,
    movement_type integer NOT NULL,
    qty_in numeric(18,4) NOT NULL,
    qty_out numeric(18,4) NOT NULL,
    qty_balance numeric(18,4) NOT NULL,
    unit_cost numeric(18,4) NOT NULL,
    total_cost numeric(18,4) NOT NULL,
    source_table character varying(50) NOT NULL,
    source_id integer NOT NULL,
    source_line_id integer,
    notes text,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    CONSTRAINT pk_inv_stock_movements PRIMARY KEY (id),
    CONSTRAINT ck_inv_stock_movements_qty_non_negative CHECK (qty_in >= 0 AND qty_out >= 0),
    CONSTRAINT ck_inv_stock_movements_unit_cost_non_negative CHECK (unit_cost >= 0),
    CONSTRAINT fk_inv_stock_movements_inv_items_item_id FOREIGN KEY (item_id) REFERENCES public.inv_items (id) ON DELETE RESTRICT,
    CONSTRAINT fk_inv_stock_movements_inv_warehouse_locations_location_id FOREIGN KEY (location_id) REFERENCES public.inv_warehouse_locations (id) ON DELETE SET NULL,
    CONSTRAINT fk_inv_stock_movements_inv_warehouses_warehouse_id FOREIGN KEY (warehouse_id) REFERENCES public.inv_warehouses (id) ON DELETE RESTRICT
);
CREATE TABLE IF NOT EXISTS public.inv_stock_transfers (
    id integer GENERATED ALWAYS AS IDENTITY,
    transfer_no character varying(30) NOT NULL,
    transfer_date date NOT NULL,
    from_warehouse_id integer NOT NULL,
    from_location_id integer,
    to_warehouse_id integer NOT NULL,
    to_location_id integer,
    reference_no character varying(100),
    description text,
    status integer NOT NULL,
    transferred_by integer,
    confirmed_by integer,
    confirmed_at timestamptz,
    journal_entry_id integer,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_inv_stock_transfers PRIMARY KEY (id),
    CONSTRAINT ck_inv_stock_transfers_warehouse_not_same CHECK (from_warehouse_id <> to_warehouse_id),
    CONSTRAINT fk_inv_stock_transfers_fin_journal_entries_journal_entry_id FOREIGN KEY (journal_entry_id) REFERENCES public.fin_journal_entries (id) ON DELETE SET NULL,
    CONSTRAINT fk_inv_stock_transfers_inv_warehouse_locations_from_location_id FOREIGN KEY (from_location_id) REFERENCES public.inv_warehouse_locations (id) ON DELETE SET NULL,
    CONSTRAINT fk_inv_stock_transfers_inv_warehouse_locations_to_location_id FOREIGN KEY (to_location_id) REFERENCES public.inv_warehouse_locations (id) ON DELETE SET NULL,
    CONSTRAINT fk_inv_stock_transfers_inv_warehouses_from_warehouse_id FOREIGN KEY (from_warehouse_id) REFERENCES public.inv_warehouses (id) ON DELETE RESTRICT,
    CONSTRAINT fk_inv_stock_transfers_inv_warehouses_to_warehouse_id FOREIGN KEY (to_warehouse_id) REFERENCES public.inv_warehouses (id) ON DELETE RESTRICT,
    CONSTRAINT fk_inv_stock_transfers_sys_users_confirmed_by FOREIGN KEY (confirmed_by) REFERENCES public.sys_users (id) ON DELETE SET NULL,
    CONSTRAINT fk_inv_stock_transfers_sys_users_transferred_by FOREIGN KEY (transferred_by) REFERENCES public.sys_users (id) ON DELETE SET NULL
);
CREATE TABLE IF NOT EXISTS public.inv_goods_issue_lines (
    id integer GENERATED ALWAYS AS IDENTITY,
    goods_issue_id integer NOT NULL,
    line_no integer NOT NULL,
    item_id integer NOT NULL,
    uom_id integer,
    qty_requested numeric(18,4) NOT NULL,
    qty_issued numeric(18,4) NOT NULL,
    qty_base numeric(18,4) NOT NULL,
    unit_cost numeric(18,4) NOT NULL,
    total_cost numeric(18,4) GENERATED ALWAYS AS (qty_base * unit_cost) STORED NOT NULL,
    notes text,
    CONSTRAINT pk_inv_goods_issue_lines PRIMARY KEY (id),
    CONSTRAINT ck_inv_goods_issue_lines_qty_base_positive CHECK (qty_base > 0),
    CONSTRAINT ck_inv_goods_issue_lines_qty_issued_positive CHECK (qty_issued > 0),
    CONSTRAINT ck_inv_goods_issue_lines_qty_not_exceed_requested CHECK (qty_issued <= qty_requested),
    CONSTRAINT ck_inv_goods_issue_lines_qty_requested_positive CHECK (qty_requested > 0),
    CONSTRAINT ck_inv_goods_issue_lines_unit_cost_non_negative CHECK (unit_cost >= 0),
    CONSTRAINT fk_inv_goods_issue_lines_inv_goods_issues_goods_issue_id FOREIGN KEY (goods_issue_id) REFERENCES public.inv_goods_issues (id) ON DELETE CASCADE,
    CONSTRAINT fk_inv_goods_issue_lines_inv_items_item_id FOREIGN KEY (item_id) REFERENCES public.inv_items (id) ON DELETE RESTRICT,
    CONSTRAINT fk_inv_goods_issue_lines_inv_units_of_measure_uom_id FOREIGN KEY (uom_id) REFERENCES public.inv_units_of_measure (id) ON DELETE SET NULL
);
CREATE TABLE IF NOT EXISTS public.inv_goods_receipt_lines (
    id integer GENERATED ALWAYS AS IDENTITY,
    goods_receipt_id integer NOT NULL,
    line_no integer NOT NULL,
    item_id integer NOT NULL,
    uom_id integer,
    qty_received numeric(18,4) NOT NULL,
    qty_base numeric(18,4) NOT NULL,
    unit_cost numeric(18,4) NOT NULL,
    total_cost numeric(18,4) GENERATED ALWAYS AS (qty_base * unit_cost) STORED NOT NULL,
    notes text,
    CONSTRAINT pk_inv_goods_receipt_lines PRIMARY KEY (id),
    CONSTRAINT ck_inv_goods_receipt_lines_qty_base_positive CHECK (qty_base > 0),
    CONSTRAINT ck_inv_goods_receipt_lines_qty_received_positive CHECK (qty_received > 0),
    CONSTRAINT ck_inv_goods_receipt_lines_unit_cost_non_negative CHECK (unit_cost >= 0),
    CONSTRAINT fk_inv_goods_receipt_lines_inv_goods_receipts_goods_receipt_id FOREIGN KEY (goods_receipt_id) REFERENCES public.inv_goods_receipts (id) ON DELETE CASCADE,
    CONSTRAINT fk_inv_goods_receipt_lines_inv_items_item_id FOREIGN KEY (item_id) REFERENCES public.inv_items (id) ON DELETE RESTRICT,
    CONSTRAINT fk_inv_goods_receipt_lines_inv_units_of_measure_uom_id FOREIGN KEY (uom_id) REFERENCES public.inv_units_of_measure (id) ON DELETE SET NULL
);
CREATE TABLE IF NOT EXISTS public.inv_stock_adjustment_lines (
    id integer GENERATED ALWAYS AS IDENTITY,
    stock_adjustment_id integer NOT NULL,
    line_no integer NOT NULL,
    item_id integer NOT NULL,
    uom_id integer,
    qty_adjustment numeric(18,4) NOT NULL,
    unit_cost numeric(18,4) NOT NULL,
    total_cost numeric(18,4) GENERATED ALWAYS AS (qty_adjustment * unit_cost) STORED NOT NULL,
    notes text,
    CONSTRAINT pk_inv_stock_adjustment_lines PRIMARY KEY (id),
    CONSTRAINT ck_inv_stock_adjustment_lines_qty_adjustment_not_zero CHECK (qty_adjustment <> 0),
    CONSTRAINT ck_inv_stock_adjustment_lines_unit_cost_non_negative CHECK (unit_cost >= 0),
    CONSTRAINT fk_inv_stock_adjustment_lines_inv_items_item_id FOREIGN KEY (item_id) REFERENCES public.inv_items (id) ON DELETE RESTRICT,
    CONSTRAINT fk_inv_stock_adjustment_lines_inv_stock_adjustments_stock_adju FOREIGN KEY (stock_adjustment_id) REFERENCES public.inv_stock_adjustments (id) ON DELETE CASCADE,
    CONSTRAINT fk_inv_stock_adjustment_lines_inv_units_of_measure_uom_id FOREIGN KEY (uom_id) REFERENCES public.inv_units_of_measure (id) ON DELETE SET NULL
);
CREATE TABLE IF NOT EXISTS public.inv_stock_opnames (
    id integer GENERATED ALWAYS AS IDENTITY,
    opname_no character varying(30) NOT NULL,
    opname_date date NOT NULL,
    warehouse_id integer NOT NULL,
    location_id integer,
    description text,
    status integer NOT NULL,
    counted_by integer,
    approved_by integer,
    approved_at timestamptz,
    adjustment_id integer,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_inv_stock_opnames PRIMARY KEY (id),
    CONSTRAINT fk_inv_stock_opnames_inv_stock_adjustments_adjustment_id FOREIGN KEY (adjustment_id) REFERENCES public.inv_stock_adjustments (id) ON DELETE SET NULL,
    CONSTRAINT fk_inv_stock_opnames_inv_warehouse_locations_location_id FOREIGN KEY (location_id) REFERENCES public.inv_warehouse_locations (id) ON DELETE SET NULL,
    CONSTRAINT fk_inv_stock_opnames_inv_warehouses_warehouse_id FOREIGN KEY (warehouse_id) REFERENCES public.inv_warehouses (id) ON DELETE RESTRICT,
    CONSTRAINT fk_inv_stock_opnames_sys_users_approved_by FOREIGN KEY (approved_by) REFERENCES public.sys_users (id) ON DELETE SET NULL,
    CONSTRAINT fk_inv_stock_opnames_sys_users_counted_by FOREIGN KEY (counted_by) REFERENCES public.sys_users (id) ON DELETE SET NULL
);
CREATE TABLE IF NOT EXISTS public.inv_stock_transfer_lines (
    id integer GENERATED ALWAYS AS IDENTITY,
    stock_transfer_id integer NOT NULL,
    line_no integer NOT NULL,
    item_id integer NOT NULL,
    uom_id integer,
    qty_transfer numeric(18,4) NOT NULL,
    qty_base numeric(18,4) NOT NULL,
    unit_cost numeric(18,4) NOT NULL,
    total_cost numeric(18,4) GENERATED ALWAYS AS (qty_base * unit_cost) STORED NOT NULL,
    notes text,
    CONSTRAINT pk_inv_stock_transfer_lines PRIMARY KEY (id),
    CONSTRAINT ck_inv_stock_transfer_lines_qty_base_positive CHECK (qty_base > 0),
    CONSTRAINT ck_inv_stock_transfer_lines_qty_transfer_positive CHECK (qty_transfer > 0),
    CONSTRAINT ck_inv_stock_transfer_lines_unit_cost_non_negative CHECK (unit_cost >= 0),
    CONSTRAINT fk_inv_stock_transfer_lines_inv_items_item_id FOREIGN KEY (item_id) REFERENCES public.inv_items (id) ON DELETE RESTRICT,
    CONSTRAINT fk_inv_stock_transfer_lines_inv_stock_transfers_stock_transfer FOREIGN KEY (stock_transfer_id) REFERENCES public.inv_stock_transfers (id) ON DELETE CASCADE,
    CONSTRAINT fk_inv_stock_transfer_lines_inv_units_of_measure_uom_id FOREIGN KEY (uom_id) REFERENCES public.inv_units_of_measure (id) ON DELETE SET NULL
);
CREATE TABLE IF NOT EXISTS public.inv_opname_lines (
    id integer GENERATED ALWAYS AS IDENTITY,
    stock_opname_id integer NOT NULL,
    line_no integer NOT NULL,
    item_id integer NOT NULL,
    location_id integer,
    qty_system numeric(18,4) NOT NULL,
    qty_counted numeric(18,4) NOT NULL,
    qty_variance numeric(18,4) GENERATED ALWAYS AS (qty_counted - qty_system) STORED NOT NULL,
    unit_cost numeric(18,4) NOT NULL,
    total_variance_value numeric(18,4) GENERATED ALWAYS AS ((qty_counted - qty_system) * unit_cost) STORED NOT NULL,
    notes text,
    CONSTRAINT pk_inv_opname_lines PRIMARY KEY (id),
    CONSTRAINT ck_inv_opname_lines_qty_counted_non_negative CHECK (qty_counted >= 0),
    CONSTRAINT ck_inv_opname_lines_qty_system_non_negative CHECK (qty_system >= 0),
    CONSTRAINT ck_inv_opname_lines_unit_cost_non_negative CHECK (unit_cost >= 0),
    CONSTRAINT fk_inv_opname_lines_inv_items_item_id FOREIGN KEY (item_id) REFERENCES public.inv_items (id) ON DELETE RESTRICT,
    CONSTRAINT fk_inv_opname_lines_inv_stock_opnames_stock_opname_id FOREIGN KEY (stock_opname_id) REFERENCES public.inv_stock_opnames (id) ON DELETE CASCADE,
    CONSTRAINT fk_inv_opname_lines_inv_warehouse_locations_location_id FOREIGN KEY (location_id) REFERENCES public.inv_warehouse_locations (id) ON DELETE SET NULL
);
CREATE INDEX IF NOT EXISTS ix_inv_brands_created_at ON public.inv_brands (created_at);
CREATE INDEX IF NOT EXISTS ix_inv_brands_is_active ON public.inv_brands (is_active);
CREATE INDEX IF NOT EXISTS ix_inv_brands_is_deleted ON public.inv_brands (is_deleted);
CREATE UNIQUE INDEX IF NOT EXISTS ix_inv_brands_name ON public.inv_brands (name);
CREATE INDEX IF NOT EXISTS ix_inv_goods_issue_lines_goods_issue_id ON public.inv_goods_issue_lines (goods_issue_id);
CREATE UNIQUE INDEX IF NOT EXISTS ix_inv_goods_issue_lines_goods_issue_id_line_no ON public.inv_goods_issue_lines (goods_issue_id, line_no);
CREATE INDEX IF NOT EXISTS ix_inv_goods_issue_lines_item_id ON public.inv_goods_issue_lines (item_id);
CREATE INDEX IF NOT EXISTS ix_inv_goods_issue_lines_uom_id ON public.inv_goods_issue_lines (uom_id);
CREATE INDEX IF NOT EXISTS ix_inv_goods_issues_confirmed_by ON public.inv_goods_issues (confirmed_by);
CREATE INDEX IF NOT EXISTS ix_inv_goods_issues_cost_center_id ON public.inv_goods_issues (cost_center_id);
CREATE INDEX IF NOT EXISTS ix_inv_goods_issues_created_at ON public.inv_goods_issues (created_at);
CREATE INDEX IF NOT EXISTS ix_inv_goods_issues_department_id ON public.inv_goods_issues (department_id);
CREATE INDEX IF NOT EXISTS ix_inv_goods_issues_is_deleted ON public.inv_goods_issues (is_deleted);
CREATE INDEX IF NOT EXISTS ix_inv_goods_issues_issue_date ON public.inv_goods_issues (issue_date);
CREATE UNIQUE INDEX IF NOT EXISTS ix_inv_goods_issues_issue_no ON public.inv_goods_issues (issue_no);
CREATE INDEX IF NOT EXISTS ix_inv_goods_issues_issue_type ON public.inv_goods_issues (issue_type);
CREATE INDEX IF NOT EXISTS ix_inv_goods_issues_issued_by ON public.inv_goods_issues (issued_by);
CREATE INDEX IF NOT EXISTS ix_inv_goods_issues_journal_entry_id ON public.inv_goods_issues (journal_entry_id);
CREATE INDEX IF NOT EXISTS ix_inv_goods_issues_location_id ON public.inv_goods_issues (location_id);
CREATE INDEX IF NOT EXISTS ix_inv_goods_issues_requested_by ON public.inv_goods_issues (requested_by);
CREATE INDEX IF NOT EXISTS ix_inv_goods_issues_status ON public.inv_goods_issues (status);
CREATE INDEX IF NOT EXISTS ix_inv_goods_issues_warehouse_id ON public.inv_goods_issues (warehouse_id);
CREATE INDEX IF NOT EXISTS ix_inv_goods_receipt_lines_goods_receipt_id ON public.inv_goods_receipt_lines (goods_receipt_id);
CREATE UNIQUE INDEX IF NOT EXISTS ix_inv_goods_receipt_lines_goods_receipt_id_line_no ON public.inv_goods_receipt_lines (goods_receipt_id, line_no);
CREATE INDEX IF NOT EXISTS ix_inv_goods_receipt_lines_item_id ON public.inv_goods_receipt_lines (item_id);
CREATE INDEX IF NOT EXISTS ix_inv_goods_receipt_lines_uom_id ON public.inv_goods_receipt_lines (uom_id);
CREATE INDEX IF NOT EXISTS ix_inv_goods_receipts_confirmed_by ON public.inv_goods_receipts (confirmed_by);
CREATE INDEX IF NOT EXISTS ix_inv_goods_receipts_created_at ON public.inv_goods_receipts (created_at);
CREATE INDEX IF NOT EXISTS ix_inv_goods_receipts_is_deleted ON public.inv_goods_receipts (is_deleted);
CREATE INDEX IF NOT EXISTS ix_inv_goods_receipts_journal_entry_id ON public.inv_goods_receipts (journal_entry_id);
CREATE INDEX IF NOT EXISTS ix_inv_goods_receipts_location_id ON public.inv_goods_receipts (location_id);
CREATE INDEX IF NOT EXISTS ix_inv_goods_receipts_receipt_date ON public.inv_goods_receipts (receipt_date);
CREATE UNIQUE INDEX IF NOT EXISTS ix_inv_goods_receipts_receipt_no ON public.inv_goods_receipts (receipt_no);
CREATE INDEX IF NOT EXISTS ix_inv_goods_receipts_receipt_type ON public.inv_goods_receipts (receipt_type);
CREATE INDEX IF NOT EXISTS ix_inv_goods_receipts_received_by ON public.inv_goods_receipts (received_by);
CREATE INDEX IF NOT EXISTS ix_inv_goods_receipts_status ON public.inv_goods_receipts (status);
CREATE INDEX IF NOT EXISTS ix_inv_goods_receipts_warehouse_id ON public.inv_goods_receipts (warehouse_id);
CREATE UNIQUE INDEX IF NOT EXISTS ix_inv_item_categories_code ON public.inv_item_categories (code);
CREATE INDEX IF NOT EXISTS ix_inv_item_categories_created_at ON public.inv_item_categories (created_at);
CREATE INDEX IF NOT EXISTS ix_inv_item_categories_is_active ON public.inv_item_categories (is_active);
CREATE INDEX IF NOT EXISTS ix_inv_item_categories_is_deleted ON public.inv_item_categories (is_deleted);
CREATE INDEX IF NOT EXISTS ix_inv_item_categories_name ON public.inv_item_categories (name);
CREATE INDEX IF NOT EXISTS ix_inv_item_categories_parent_category_id ON public.inv_item_categories (parent_category_id);
CREATE INDEX IF NOT EXISTS ix_inv_item_unit_conversions_created_at ON public.inv_item_unit_conversions (created_at);
CREATE INDEX IF NOT EXISTS ix_inv_item_unit_conversions_from_uom_id ON public.inv_item_unit_conversions (from_uom_id);
CREATE INDEX IF NOT EXISTS ix_inv_item_unit_conversions_is_active ON public.inv_item_unit_conversions (is_active);
CREATE INDEX IF NOT EXISTS ix_inv_item_unit_conversions_is_deleted ON public.inv_item_unit_conversions (is_deleted);
CREATE INDEX IF NOT EXISTS ix_inv_item_unit_conversions_item_id ON public.inv_item_unit_conversions (item_id);
CREATE UNIQUE INDEX IF NOT EXISTS ix_inv_item_unit_conversions_item_id_from_uom_id_to_uom_id ON public.inv_item_unit_conversions (item_id, from_uom_id, to_uom_id);
CREATE INDEX IF NOT EXISTS ix_inv_item_unit_conversions_to_uom_id ON public.inv_item_unit_conversions (to_uom_id);
CREATE INDEX IF NOT EXISTS ix_inv_items_adjustment_account_id ON public.inv_items (account_adjustment_id);
CREATE INDEX IF NOT EXISTS ix_inv_items_base_uom_id ON public.inv_items (base_uom_id);
CREATE INDEX IF NOT EXISTS ix_inv_items_brand_id ON public.inv_items (brand_id);
CREATE INDEX IF NOT EXISTS ix_inv_items_category_id ON public.inv_items (category_id);
CREATE INDEX IF NOT EXISTS ix_inv_items_cogs_account_id ON public.inv_items (account_cogs_id);
CREATE INDEX IF NOT EXISTS ix_inv_items_created_at ON public.inv_items (created_at);
CREATE INDEX IF NOT EXISTS ix_inv_items_inventory_account_id ON public.inv_items (account_inventory_id);
CREATE INDEX IF NOT EXISTS ix_inv_items_is_active ON public.inv_items (is_active);
CREATE INDEX IF NOT EXISTS ix_inv_items_is_deleted ON public.inv_items (is_deleted);
CREATE UNIQUE INDEX IF NOT EXISTS ix_inv_items_item_code ON public.inv_items (item_code);
CREATE INDEX IF NOT EXISTS ix_inv_items_min_stock ON public.inv_items (min_stock);
CREATE INDEX IF NOT EXISTS ix_inv_items_name ON public.inv_items (name);
CREATE INDEX IF NOT EXISTS ix_inv_items_purchase_uom_id ON public.inv_items (purchase_uom_id);
CREATE INDEX IF NOT EXISTS ix_inv_items_reorder_point ON public.inv_items (reorder_point);
CREATE UNIQUE INDEX IF NOT EXISTS ix_inv_items_sku ON public.inv_items (sku);
CREATE INDEX IF NOT EXISTS ix_inv_items_status ON public.inv_items (status);
CREATE INDEX IF NOT EXISTS ix_inv_items_type ON public.inv_items (type);
CREATE INDEX IF NOT EXISTS ix_inv_opname_lines_item_id ON public.inv_opname_lines (item_id);
CREATE INDEX IF NOT EXISTS ix_inv_opname_lines_location_id ON public.inv_opname_lines (location_id);
CREATE INDEX IF NOT EXISTS ix_inv_opname_lines_stock_opname_id ON public.inv_opname_lines (stock_opname_id);
CREATE UNIQUE INDEX IF NOT EXISTS ix_inv_opname_lines_stock_opname_id_item_id_location_id ON public.inv_opname_lines (stock_opname_id, item_id, location_id);
CREATE UNIQUE INDEX IF NOT EXISTS ix_inv_opname_lines_stock_opname_id_line_no ON public.inv_opname_lines (stock_opname_id, line_no);
CREATE INDEX IF NOT EXISTS ix_inv_stock_adjustment_lines_item_id ON public.inv_stock_adjustment_lines (item_id);
CREATE INDEX IF NOT EXISTS ix_inv_stock_adjustment_lines_stock_adjustment_id ON public.inv_stock_adjustment_lines (stock_adjustment_id);
CREATE UNIQUE INDEX IF NOT EXISTS ix_inv_stock_adjustment_lines_stock_adjustment_id_line_no ON public.inv_stock_adjustment_lines (stock_adjustment_id, line_no);
CREATE INDEX IF NOT EXISTS ix_inv_stock_adjustment_lines_uom_id ON public.inv_stock_adjustment_lines (uom_id);
CREATE INDEX IF NOT EXISTS ix_inv_stock_adjustments_adjustment_date ON public.inv_stock_adjustments (adjustment_date);
CREATE UNIQUE INDEX IF NOT EXISTS ix_inv_stock_adjustments_adjustment_no ON public.inv_stock_adjustments (adjustment_no);
CREATE INDEX IF NOT EXISTS ix_inv_stock_adjustments_approved_by ON public.inv_stock_adjustments (approved_by);
CREATE INDEX IF NOT EXISTS ix_inv_stock_adjustments_confirmed_by ON public.inv_stock_adjustments (confirmed_by);
CREATE INDEX IF NOT EXISTS ix_inv_stock_adjustments_created_at ON public.inv_stock_adjustments (created_at);
CREATE INDEX IF NOT EXISTS ix_inv_stock_adjustments_is_deleted ON public.inv_stock_adjustments (is_deleted);
CREATE INDEX IF NOT EXISTS ix_inv_stock_adjustments_journal_entry_id ON public.inv_stock_adjustments (journal_entry_id);
CREATE INDEX IF NOT EXISTS ix_inv_stock_adjustments_location_id ON public.inv_stock_adjustments (location_id);
CREATE INDEX IF NOT EXISTS ix_inv_stock_adjustments_reason ON public.inv_stock_adjustments (reason);
CREATE INDEX IF NOT EXISTS ix_inv_stock_adjustments_requested_by ON public.inv_stock_adjustments (requested_by);
CREATE INDEX IF NOT EXISTS ix_inv_stock_adjustments_status ON public.inv_stock_adjustments (status);
CREATE INDEX IF NOT EXISTS ix_inv_stock_adjustments_warehouse_id ON public.inv_stock_adjustments (warehouse_id);
CREATE INDEX IF NOT EXISTS ix_inv_stock_balances_created_at ON public.inv_stock_balances (created_at);
CREATE INDEX IF NOT EXISTS ix_inv_stock_balances_is_deleted ON public.inv_stock_balances (is_deleted);
CREATE INDEX IF NOT EXISTS ix_inv_stock_balances_item_id ON public.inv_stock_balances (item_id);
CREATE UNIQUE INDEX IF NOT EXISTS ix_inv_stock_balances_item_id_warehouse_id ON public.inv_stock_balances (item_id, warehouse_id) WHERE "location_id" IS NULL;
CREATE UNIQUE INDEX IF NOT EXISTS ix_inv_stock_balances_item_id_warehouse_id_location_id ON public.inv_stock_balances (item_id, warehouse_id, location_id);
CREATE INDEX IF NOT EXISTS ix_inv_stock_balances_location_id ON public.inv_stock_balances (location_id);
CREATE INDEX IF NOT EXISTS ix_inv_stock_balances_qty_available ON public.inv_stock_balances (qty_available);
CREATE INDEX IF NOT EXISTS ix_inv_stock_balances_total_value ON public.inv_stock_balances (total_value);
CREATE INDEX IF NOT EXISTS ix_inv_stock_balances_warehouse_id ON public.inv_stock_balances (warehouse_id);
CREATE INDEX IF NOT EXISTS ix_inv_stock_movements_item_id ON public.inv_stock_movements (item_id);
CREATE INDEX IF NOT EXISTS ix_inv_stock_movements_item_id_warehouse_id_location_id_moveme ON public.inv_stock_movements (item_id, warehouse_id, location_id, movement_date);
CREATE INDEX IF NOT EXISTS ix_inv_stock_movements_location_id ON public.inv_stock_movements (location_id);
CREATE INDEX IF NOT EXISTS ix_inv_stock_movements_movement_date ON public.inv_stock_movements (movement_date);
CREATE INDEX IF NOT EXISTS ix_inv_stock_movements_movement_type ON public.inv_stock_movements (movement_type);
CREATE INDEX IF NOT EXISTS ix_inv_stock_movements_source_table_source_id ON public.inv_stock_movements (source_table, source_id);
CREATE INDEX IF NOT EXISTS ix_inv_stock_movements_warehouse_id ON public.inv_stock_movements (warehouse_id);
CREATE INDEX IF NOT EXISTS ix_inv_stock_opnames_adjustment_id ON public.inv_stock_opnames (adjustment_id);
CREATE INDEX IF NOT EXISTS ix_inv_stock_opnames_approved_by ON public.inv_stock_opnames (approved_by);
CREATE INDEX IF NOT EXISTS ix_inv_stock_opnames_counted_by ON public.inv_stock_opnames (counted_by);
CREATE INDEX IF NOT EXISTS ix_inv_stock_opnames_created_at ON public.inv_stock_opnames (created_at);
CREATE INDEX IF NOT EXISTS ix_inv_stock_opnames_is_deleted ON public.inv_stock_opnames (is_deleted);
CREATE INDEX IF NOT EXISTS ix_inv_stock_opnames_location_id ON public.inv_stock_opnames (location_id);
CREATE INDEX IF NOT EXISTS ix_inv_stock_opnames_opname_date ON public.inv_stock_opnames (opname_date);
CREATE UNIQUE INDEX IF NOT EXISTS ix_inv_stock_opnames_opname_no ON public.inv_stock_opnames (opname_no);
CREATE INDEX IF NOT EXISTS ix_inv_stock_opnames_status ON public.inv_stock_opnames (status);
CREATE INDEX IF NOT EXISTS ix_inv_stock_opnames_warehouse_id ON public.inv_stock_opnames (warehouse_id);
CREATE INDEX IF NOT EXISTS ix_inv_stock_transfer_lines_item_id ON public.inv_stock_transfer_lines (item_id);
CREATE INDEX IF NOT EXISTS ix_inv_stock_transfer_lines_stock_transfer_id ON public.inv_stock_transfer_lines (stock_transfer_id);
CREATE UNIQUE INDEX IF NOT EXISTS ix_inv_stock_transfer_lines_stock_transfer_id_line_no ON public.inv_stock_transfer_lines (stock_transfer_id, line_no);
CREATE INDEX IF NOT EXISTS ix_inv_stock_transfer_lines_uom_id ON public.inv_stock_transfer_lines (uom_id);
CREATE INDEX IF NOT EXISTS ix_inv_stock_transfers_confirmed_by ON public.inv_stock_transfers (confirmed_by);
CREATE INDEX IF NOT EXISTS ix_inv_stock_transfers_created_at ON public.inv_stock_transfers (created_at);
CREATE INDEX IF NOT EXISTS ix_inv_stock_transfers_from_location_id ON public.inv_stock_transfers (from_location_id);
CREATE INDEX IF NOT EXISTS ix_inv_stock_transfers_from_warehouse_id ON public.inv_stock_transfers (from_warehouse_id);
CREATE INDEX IF NOT EXISTS ix_inv_stock_transfers_is_deleted ON public.inv_stock_transfers (is_deleted);
CREATE INDEX IF NOT EXISTS ix_inv_stock_transfers_journal_entry_id ON public.inv_stock_transfers (journal_entry_id);
CREATE INDEX IF NOT EXISTS ix_inv_stock_transfers_status ON public.inv_stock_transfers (status);
CREATE INDEX IF NOT EXISTS ix_inv_stock_transfers_to_location_id ON public.inv_stock_transfers (to_location_id);
CREATE INDEX IF NOT EXISTS ix_inv_stock_transfers_to_warehouse_id ON public.inv_stock_transfers (to_warehouse_id);
CREATE INDEX IF NOT EXISTS ix_inv_stock_transfers_transfer_date ON public.inv_stock_transfers (transfer_date);
CREATE UNIQUE INDEX IF NOT EXISTS ix_inv_stock_transfers_transfer_no ON public.inv_stock_transfers (transfer_no);
CREATE INDEX IF NOT EXISTS ix_inv_stock_transfers_transferred_by ON public.inv_stock_transfers (transferred_by);
CREATE UNIQUE INDEX IF NOT EXISTS ix_inv_units_of_measure_code ON public.inv_units_of_measure (code);
CREATE INDEX IF NOT EXISTS ix_inv_units_of_measure_created_at ON public.inv_units_of_measure (created_at);
CREATE INDEX IF NOT EXISTS ix_inv_units_of_measure_is_active ON public.inv_units_of_measure (is_active);
CREATE INDEX IF NOT EXISTS ix_inv_units_of_measure_is_deleted ON public.inv_units_of_measure (is_deleted);
CREATE INDEX IF NOT EXISTS ix_inv_units_of_measure_name ON public.inv_units_of_measure (name);
CREATE INDEX IF NOT EXISTS ix_inv_warehouse_locations_code ON public.inv_warehouse_locations (code);
CREATE INDEX IF NOT EXISTS ix_inv_warehouse_locations_created_at ON public.inv_warehouse_locations (created_at);
CREATE INDEX IF NOT EXISTS ix_inv_warehouse_locations_is_active ON public.inv_warehouse_locations (is_active);
CREATE INDEX IF NOT EXISTS ix_inv_warehouse_locations_is_default ON public.inv_warehouse_locations (is_default);
CREATE INDEX IF NOT EXISTS ix_inv_warehouse_locations_is_deleted ON public.inv_warehouse_locations (is_deleted);
CREATE INDEX IF NOT EXISTS ix_inv_warehouse_locations_name ON public.inv_warehouse_locations (name);
CREATE INDEX IF NOT EXISTS ix_inv_warehouse_locations_warehouse_id ON public.inv_warehouse_locations (warehouse_id);
CREATE UNIQUE INDEX IF NOT EXISTS ix_inv_warehouse_locations_warehouse_id_code ON public.inv_warehouse_locations (warehouse_id, code);
CREATE UNIQUE INDEX IF NOT EXISTS ix_inv_warehouses_code ON public.inv_warehouses (code);
CREATE INDEX IF NOT EXISTS ix_inv_warehouses_cost_center_id ON public.inv_warehouses (cost_center_id);
CREATE INDEX IF NOT EXISTS ix_inv_warehouses_created_at ON public.inv_warehouses (created_at);
CREATE INDEX IF NOT EXISTS ix_inv_warehouses_is_active ON public.inv_warehouses (is_active);
CREATE INDEX IF NOT EXISTS ix_inv_warehouses_is_deleted ON public.inv_warehouses (is_deleted);
CREATE INDEX IF NOT EXISTS ix_inv_warehouses_is_transit ON public.inv_warehouses (is_transit);
CREATE INDEX IF NOT EXISTS ix_inv_warehouses_manager_id ON public.inv_warehouses (manager_id);
CREATE INDEX IF NOT EXISTS ix_inv_warehouses_name ON public.inv_warehouses (name);

CREATE INDEX IF NOT EXISTS idx_inv_items_category_type_status
    ON public.inv_items (category_id, type, status);

CREATE INDEX IF NOT EXISTS idx_inv_items_status_active
    ON public.inv_items (status)
    WHERE status = 0;

CREATE INDEX IF NOT EXISTS idx_inv_stock_balances_warehouse_item
    ON public.inv_stock_balances (warehouse_id, item_id);

CREATE INDEX IF NOT EXISTS idx_inv_stock_balances_qty_depleted
    ON public.inv_stock_balances (qty_available)
    WHERE qty_available <= 0;

CREATE INDEX IF NOT EXISTS idx_inv_stock_movements_item_warehouse_date
    ON public.inv_stock_movements (item_id, warehouse_id, movement_date);

CREATE INDEX IF NOT EXISTS idx_inv_goods_receipts_warehouse_date_status
    ON public.inv_goods_receipts (warehouse_id, receipt_date, status);

CREATE INDEX IF NOT EXISTS idx_inv_goods_receipts_supplier_name
    ON public.inv_goods_receipts (supplier_name);

CREATE INDEX IF NOT EXISTS idx_inv_goods_issues_warehouse_date_status
    ON public.inv_goods_issues (warehouse_id, issue_date, status);

CREATE INDEX IF NOT EXISTS idx_inv_goods_issues_department
    ON public.inv_goods_issues (department_id);

CREATE INDEX IF NOT EXISTS idx_inv_stock_transfers_from_to_status
    ON public.inv_stock_transfers (from_warehouse_id, to_warehouse_id, status);

CREATE INDEX IF NOT EXISTS idx_inv_stock_adjustments_warehouse_status_reason
    ON public.inv_stock_adjustments (warehouse_id, status, reason);
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
DROP TABLE IF EXISTS public.inv_opname_lines;
DROP TABLE IF EXISTS public.inv_stock_transfer_lines;
DROP TABLE IF EXISTS public.inv_stock_opnames;
DROP TABLE IF EXISTS public.inv_stock_adjustment_lines;
DROP TABLE IF EXISTS public.inv_goods_receipt_lines;
DROP TABLE IF EXISTS public.inv_goods_issue_lines;
DROP TABLE IF EXISTS public.inv_stock_transfers;
DROP TABLE IF EXISTS public.inv_stock_movements;
DROP TABLE IF EXISTS public.inv_stock_balances;
DROP TABLE IF EXISTS public.inv_stock_adjustments;
DROP TABLE IF EXISTS public.inv_goods_receipts;
DROP TABLE IF EXISTS public.inv_goods_issues;
DROP TABLE IF EXISTS public.inv_warehouse_locations;
DROP TABLE IF EXISTS public.inv_warehouses;
DROP TABLE IF EXISTS public.inv_item_unit_conversions;
DROP TABLE IF EXISTS public.inv_items;
DROP TABLE IF EXISTS public.inv_brands;
DROP TABLE IF EXISTS public.inv_units_of_measure;
DROP TABLE IF EXISTS public.inv_item_categories;
""");
    }
}
