using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260626100000_AddSalesPhase31And32")]
public partial class AddSalesPhase31And32 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
CREATE TABLE IF NOT EXISTS public.sal_price_lists (
    id integer GENERATED ALWAYS AS IDENTITY,
    code character varying(30) NOT NULL,
    name character varying(100) NOT NULL,
    type integer NOT NULL DEFAULT 0,
    currency_code character varying(10) NOT NULL DEFAULT 'IDR',
    valid_from date NOT NULL,
    valid_to date,
    is_active boolean NOT NULL DEFAULT TRUE,
    notes text,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_sal_price_lists PRIMARY KEY (id),
    CONSTRAINT ck_sal_price_lists_valid_range CHECK (valid_to IS NULL OR valid_to >= valid_from),
    CONSTRAINT fk_sal_price_lists_fin_currencies_currency_code FOREIGN KEY (currency_code) REFERENCES public.fin_currencies (code) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS public.sal_customer_categories (
    id integer GENERATED ALWAYS AS IDENTITY,
    code character varying(20) NOT NULL,
    name character varying(100) NOT NULL,
    default_price_list_id integer,
    default_payment_terms integer NOT NULL DEFAULT 2,
    default_credit_limit numeric(18,4) NOT NULL DEFAULT 0.0,
    description text,
    is_active boolean NOT NULL DEFAULT TRUE,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_sal_customer_categories PRIMARY KEY (id),
    CONSTRAINT ck_sal_customer_categories_default_payment_terms_non_negative CHECK (default_payment_terms >= 0),
    CONSTRAINT ck_sal_customer_categories_default_credit_limit_non_negative CHECK (default_credit_limit >= 0),
    CONSTRAINT fk_sal_customer_categories_sal_price_lists_default_price_list_id FOREIGN KEY (default_price_list_id) REFERENCES public.sal_price_lists (id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS public.sal_price_list_items (
    id integer GENERATED ALWAYS AS IDENTITY,
    price_list_id integer NOT NULL,
    item_id integer NOT NULL,
    uom_id integer NOT NULL,
    min_qty numeric(18,4) NOT NULL DEFAULT 1.0,
    unit_price numeric(18,4) NOT NULL DEFAULT 0.0,
    discount_pct numeric(5,2) NOT NULL DEFAULT 0.0,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_sal_price_list_items PRIMARY KEY (id),
    CONSTRAINT ck_sal_price_list_items_min_qty_positive CHECK (min_qty > 0),
    CONSTRAINT ck_sal_price_list_items_unit_price_non_negative CHECK (unit_price >= 0),
    CONSTRAINT ck_sal_price_list_items_discount_range CHECK (discount_pct >= 0 AND discount_pct <= 100),
    CONSTRAINT fk_sal_price_list_items_sal_price_lists_price_list_id FOREIGN KEY (price_list_id) REFERENCES public.sal_price_lists (id) ON DELETE CASCADE,
    CONSTRAINT fk_sal_price_list_items_inv_items_item_id FOREIGN KEY (item_id) REFERENCES public.inv_items (id) ON DELETE RESTRICT,
    CONSTRAINT fk_sal_price_list_items_inv_units_of_measure_uom_id FOREIGN KEY (uom_id) REFERENCES public.inv_units_of_measure (id) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS public.sal_approval_configs (
    id integer GENERATED ALWAYS AS IDENTITY,
    document_type integer NOT NULL,
    level integer NOT NULL,
    min_amount numeric(18,4) NOT NULL DEFAULT 0.0,
    max_amount numeric(18,4),
    max_discount_pct numeric(5,2),
    approver_role_id integer,
    approver_employee_id integer,
    timeout_hours integer NOT NULL DEFAULT 48,
    auto_approve_if_timeout boolean NOT NULL DEFAULT FALSE,
    is_active boolean NOT NULL DEFAULT TRUE,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_sal_approval_configs PRIMARY KEY (id),
    CONSTRAINT ck_sal_approval_configs_level_positive CHECK (level > 0),
    CONSTRAINT ck_sal_approval_configs_min_amount_non_negative CHECK (min_amount >= 0),
    CONSTRAINT ck_sal_approval_configs_max_amount_non_negative CHECK (max_amount IS NULL OR max_amount >= 0),
    CONSTRAINT ck_sal_approval_configs_amount_range CHECK (max_amount IS NULL OR max_amount >= min_amount),
    CONSTRAINT ck_sal_approval_configs_max_discount_pct_range CHECK (max_discount_pct IS NULL OR (max_discount_pct >= 0 AND max_discount_pct <= 100)),
    CONSTRAINT ck_sal_approval_configs_timeout_hours_positive CHECK (timeout_hours > 0),
    CONSTRAINT ck_sal_approval_configs_has_approver CHECK (approver_role_id IS NOT NULL OR approver_employee_id IS NOT NULL),
    CONSTRAINT fk_sal_approval_configs_cfg_roles_approver_role_id FOREIGN KEY (approver_role_id) REFERENCES public.cfg_roles (id) ON DELETE SET NULL,
    CONSTRAINT fk_sal_approval_configs_hr_employees_approver_employee_id FOREIGN KEY (approver_employee_id) REFERENCES public.hr_employees (id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS public.sal_sales_teams (
    id integer GENERATED ALWAYS AS IDENTITY,
    code character varying(20) NOT NULL,
    name character varying(100) NOT NULL,
    team_leader_id integer NOT NULL,
    is_active boolean NOT NULL DEFAULT TRUE,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_sal_sales_teams PRIMARY KEY (id),
    CONSTRAINT fk_sal_sales_teams_hr_employees_team_leader_id FOREIGN KEY (team_leader_id) REFERENCES public.hr_employees (id) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS public.sal_sales_team_members (
    id integer GENERATED ALWAYS AS IDENTITY,
    sales_team_id integer NOT NULL,
    employee_id integer NOT NULL,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_sal_sales_team_members PRIMARY KEY (id),
    CONSTRAINT fk_sal_sales_team_members_sal_sales_teams_sales_team_id FOREIGN KEY (sales_team_id) REFERENCES public.sal_sales_teams (id) ON DELETE CASCADE,
    CONSTRAINT fk_sal_sales_team_members_hr_employees_employee_id FOREIGN KEY (employee_id) REFERENCES public.hr_employees (id) ON DELETE RESTRICT
);

ALTER TABLE public.fin_customers ADD COLUMN IF NOT EXISTS customer_category_id integer;
ALTER TABLE public.fin_customers ADD COLUMN IF NOT EXISTS price_list_id integer;
ALTER TABLE public.fin_customers ADD COLUMN IF NOT EXISTS sales_employee_id integer;
ALTER TABLE public.fin_customers ADD COLUMN IF NOT EXISTS sales_team_id integer;
ALTER TABLE public.fin_customers ADD COLUMN IF NOT EXISTS credit_used numeric(18,4) NOT NULL DEFAULT 0.0;
ALTER TABLE public.fin_customers ADD COLUMN IF NOT EXISTS last_order_date date;
ALTER TABLE public.fin_customers ADD COLUMN IF NOT EXISTS total_ytd_sales numeric(18,4) NOT NULL DEFAULT 0.0;
ALTER TABLE public.fin_customers ADD COLUMN IF NOT EXISTS notes text;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'ck_fin_customers_non_negative_credit_used'
    ) THEN
        ALTER TABLE public.fin_customers
            ADD CONSTRAINT ck_fin_customers_non_negative_credit_used
            CHECK (credit_used >= 0);
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'ck_fin_customers_non_negative_total_ytd_sales'
    ) THEN
        ALTER TABLE public.fin_customers
            ADD CONSTRAINT ck_fin_customers_non_negative_total_ytd_sales
            CHECK (total_ytd_sales >= 0);
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'fk_fin_customers_sal_customer_categories_customer_category_id'
    ) THEN
        ALTER TABLE public.fin_customers
            ADD CONSTRAINT fk_fin_customers_sal_customer_categories_customer_category_id
            FOREIGN KEY (customer_category_id) REFERENCES public.sal_customer_categories (id)
            ON DELETE SET NULL;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'fk_fin_customers_sal_price_lists_price_list_id'
    ) THEN
        ALTER TABLE public.fin_customers
            ADD CONSTRAINT fk_fin_customers_sal_price_lists_price_list_id
            FOREIGN KEY (price_list_id) REFERENCES public.sal_price_lists (id)
            ON DELETE SET NULL;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'fk_fin_customers_hr_employees_sales_employee_id'
    ) THEN
        ALTER TABLE public.fin_customers
            ADD CONSTRAINT fk_fin_customers_hr_employees_sales_employee_id
            FOREIGN KEY (sales_employee_id) REFERENCES public.hr_employees (id)
            ON DELETE SET NULL;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'fk_fin_customers_sal_sales_teams_sales_team_id'
    ) THEN
        ALTER TABLE public.fin_customers
            ADD CONSTRAINT fk_fin_customers_sal_sales_teams_sales_team_id
            FOREIGN KEY (sales_team_id) REFERENCES public.sal_sales_teams (id)
            ON DELETE SET NULL;
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS ix_sal_price_lists_created_at ON public.sal_price_lists (created_at);
CREATE INDEX IF NOT EXISTS ix_sal_price_lists_is_deleted ON public.sal_price_lists (is_deleted);
CREATE UNIQUE INDEX IF NOT EXISTS ix_sal_price_lists_code ON public.sal_price_lists (code);
CREATE INDEX IF NOT EXISTS ix_sal_price_lists_name ON public.sal_price_lists (name);
CREATE INDEX IF NOT EXISTS ix_sal_price_lists_type ON public.sal_price_lists (type);
CREATE INDEX IF NOT EXISTS ix_sal_price_lists_currency_code ON public.sal_price_lists (currency_code);
CREATE INDEX IF NOT EXISTS ix_sal_price_lists_valid_from ON public.sal_price_lists (valid_from);
CREATE INDEX IF NOT EXISTS ix_sal_price_lists_valid_to ON public.sal_price_lists (valid_to);
CREATE INDEX IF NOT EXISTS ix_sal_price_lists_is_active ON public.sal_price_lists (is_active);

CREATE INDEX IF NOT EXISTS ix_sal_customer_categories_created_at ON public.sal_customer_categories (created_at);
CREATE INDEX IF NOT EXISTS ix_sal_customer_categories_is_deleted ON public.sal_customer_categories (is_deleted);
CREATE UNIQUE INDEX IF NOT EXISTS ix_sal_customer_categories_code ON public.sal_customer_categories (code);
CREATE INDEX IF NOT EXISTS ix_sal_customer_categories_name ON public.sal_customer_categories (name);
CREATE INDEX IF NOT EXISTS ix_sal_customer_categories_default_price_list_id ON public.sal_customer_categories (default_price_list_id);
CREATE INDEX IF NOT EXISTS ix_sal_customer_categories_is_active ON public.sal_customer_categories (is_active);

CREATE INDEX IF NOT EXISTS ix_sal_price_list_items_created_at ON public.sal_price_list_items (created_at);
CREATE INDEX IF NOT EXISTS ix_sal_price_list_items_is_deleted ON public.sal_price_list_items (is_deleted);
CREATE INDEX IF NOT EXISTS ix_sal_price_list_items_price_list_id ON public.sal_price_list_items (price_list_id);
CREATE INDEX IF NOT EXISTS ix_sal_price_list_items_item_id ON public.sal_price_list_items (item_id);
CREATE INDEX IF NOT EXISTS ix_sal_price_list_items_uom_id ON public.sal_price_list_items (uom_id);
CREATE UNIQUE INDEX IF NOT EXISTS ix_sal_price_list_items_price_list_id_item_id_uom_id_min_qty ON public.sal_price_list_items (price_list_id, item_id, uom_id, min_qty);

CREATE INDEX IF NOT EXISTS ix_sal_approval_configs_created_at ON public.sal_approval_configs (created_at);
CREATE INDEX IF NOT EXISTS ix_sal_approval_configs_is_deleted ON public.sal_approval_configs (is_deleted);
CREATE INDEX IF NOT EXISTS ix_sal_approval_configs_document_type ON public.sal_approval_configs (document_type);
CREATE INDEX IF NOT EXISTS ix_sal_approval_configs_level ON public.sal_approval_configs (level);
CREATE INDEX IF NOT EXISTS ix_sal_approval_configs_approver_role_id ON public.sal_approval_configs (approver_role_id);
CREATE INDEX IF NOT EXISTS ix_sal_approval_configs_approver_employee_id ON public.sal_approval_configs (approver_employee_id);
CREATE INDEX IF NOT EXISTS ix_sal_approval_configs_is_active ON public.sal_approval_configs (is_active);
CREATE UNIQUE INDEX IF NOT EXISTS ix_sal_approval_configs_document_type_level ON public.sal_approval_configs (document_type, level);

CREATE INDEX IF NOT EXISTS ix_sal_sales_teams_created_at ON public.sal_sales_teams (created_at);
CREATE INDEX IF NOT EXISTS ix_sal_sales_teams_is_deleted ON public.sal_sales_teams (is_deleted);
CREATE UNIQUE INDEX IF NOT EXISTS ix_sal_sales_teams_code ON public.sal_sales_teams (code);
CREATE INDEX IF NOT EXISTS ix_sal_sales_teams_name ON public.sal_sales_teams (name);
CREATE INDEX IF NOT EXISTS ix_sal_sales_teams_team_leader_id ON public.sal_sales_teams (team_leader_id);
CREATE INDEX IF NOT EXISTS ix_sal_sales_teams_is_active ON public.sal_sales_teams (is_active);

CREATE INDEX IF NOT EXISTS ix_sal_sales_team_members_created_at ON public.sal_sales_team_members (created_at);
CREATE INDEX IF NOT EXISTS ix_sal_sales_team_members_is_deleted ON public.sal_sales_team_members (is_deleted);
CREATE INDEX IF NOT EXISTS ix_sal_sales_team_members_sales_team_id ON public.sal_sales_team_members (sales_team_id);
CREATE INDEX IF NOT EXISTS ix_sal_sales_team_members_employee_id ON public.sal_sales_team_members (employee_id);
CREATE UNIQUE INDEX IF NOT EXISTS ix_sal_sales_team_members_sales_team_id_employee_id ON public.sal_sales_team_members (sales_team_id, employee_id);

CREATE INDEX IF NOT EXISTS ix_fin_customers_customer_category_id ON public.fin_customers (customer_category_id);
CREATE INDEX IF NOT EXISTS ix_fin_customers_price_list_id ON public.fin_customers (price_list_id);
CREATE INDEX IF NOT EXISTS ix_fin_customers_sales_employee_id ON public.fin_customers (sales_employee_id);
CREATE INDEX IF NOT EXISTS ix_fin_customers_sales_team_id ON public.fin_customers (sales_team_id);
CREATE INDEX IF NOT EXISTS ix_fin_customers_credit_used ON public.fin_customers (credit_used);
CREATE INDEX IF NOT EXISTS ix_fin_customers_last_order_date ON public.fin_customers (last_order_date);
CREATE INDEX IF NOT EXISTS ix_fin_customers_total_ytd_sales ON public.fin_customers (total_ytd_sales);
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
ALTER TABLE public.fin_customers DROP CONSTRAINT IF EXISTS fk_fin_customers_sal_customer_categories_customer_category_id;
ALTER TABLE public.fin_customers DROP CONSTRAINT IF EXISTS fk_fin_customers_sal_price_lists_price_list_id;
ALTER TABLE public.fin_customers DROP CONSTRAINT IF EXISTS fk_fin_customers_hr_employees_sales_employee_id;
ALTER TABLE public.fin_customers DROP CONSTRAINT IF EXISTS fk_fin_customers_sal_sales_teams_sales_team_id;
ALTER TABLE public.fin_customers DROP CONSTRAINT IF EXISTS ck_fin_customers_non_negative_credit_used;
ALTER TABLE public.fin_customers DROP CONSTRAINT IF EXISTS ck_fin_customers_non_negative_total_ytd_sales;

DROP INDEX IF EXISTS public.ix_fin_customers_customer_category_id;
DROP INDEX IF EXISTS public.ix_fin_customers_price_list_id;
DROP INDEX IF EXISTS public.ix_fin_customers_sales_employee_id;
DROP INDEX IF EXISTS public.ix_fin_customers_sales_team_id;
DROP INDEX IF EXISTS public.ix_fin_customers_credit_used;
DROP INDEX IF EXISTS public.ix_fin_customers_last_order_date;
DROP INDEX IF EXISTS public.ix_fin_customers_total_ytd_sales;

ALTER TABLE public.fin_customers DROP COLUMN IF EXISTS customer_category_id;
ALTER TABLE public.fin_customers DROP COLUMN IF EXISTS price_list_id;
ALTER TABLE public.fin_customers DROP COLUMN IF EXISTS sales_employee_id;
ALTER TABLE public.fin_customers DROP COLUMN IF EXISTS sales_team_id;
ALTER TABLE public.fin_customers DROP COLUMN IF EXISTS credit_used;
ALTER TABLE public.fin_customers DROP COLUMN IF EXISTS last_order_date;
ALTER TABLE public.fin_customers DROP COLUMN IF EXISTS total_ytd_sales;
ALTER TABLE public.fin_customers DROP COLUMN IF EXISTS notes;

DROP TABLE IF EXISTS public.sal_sales_team_members;
DROP TABLE IF EXISTS public.sal_approval_configs;
DROP TABLE IF EXISTS public.sal_price_list_items;
DROP TABLE IF EXISTS public.sal_customer_categories;
DROP TABLE IF EXISTS public.sal_sales_teams;
DROP TABLE IF EXISTS public.sal_price_lists;
""");
    }
}
