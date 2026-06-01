using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260601183000_AddPurchasingPhase23And24")]
public partial class AddPurchasingPhase23And24 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
CREATE TABLE IF NOT EXISTS public.pur_vendor_categories (
    id integer GENERATED ALWAYS AS IDENTITY,
    code character varying(20) NOT NULL,
    name character varying(100) NOT NULL,
    description text,
    is_active boolean NOT NULL DEFAULT TRUE,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_pur_vendor_categories PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS public.pur_approval_configs (
    id integer GENERATED ALWAYS AS IDENTITY,
    document_type integer NOT NULL,
    level integer NOT NULL,
    min_amount numeric(18,4) NOT NULL DEFAULT 0.0,
    max_amount numeric(18,4),
    approver_employee_id integer NOT NULL,
    is_active boolean NOT NULL DEFAULT TRUE,
    notes text,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_pur_approval_configs PRIMARY KEY (id),
    CONSTRAINT ck_pur_approval_configs_level_positive CHECK (level > 0),
    CONSTRAINT ck_pur_approval_configs_min_amount_non_negative CHECK (min_amount >= 0),
    CONSTRAINT ck_pur_approval_configs_max_amount_non_negative CHECK (max_amount IS NULL OR max_amount >= 0),
    CONSTRAINT ck_pur_approval_configs_amount_range CHECK (max_amount IS NULL OR max_amount >= min_amount),
    CONSTRAINT fk_pur_approval_configs_hr_employees_approver_employee_id FOREIGN KEY (approver_employee_id) REFERENCES public.hr_employees (id) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS public.pur_buyer_groups (
    id integer GENERATED ALWAYS AS IDENTITY,
    code character varying(20) NOT NULL,
    name character varying(100) NOT NULL,
    buyer_employee_id integer NOT NULL,
    description text,
    is_active boolean NOT NULL DEFAULT TRUE,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_pur_buyer_groups PRIMARY KEY (id),
    CONSTRAINT fk_pur_buyer_groups_hr_employees_buyer_employee_id FOREIGN KEY (buyer_employee_id) REFERENCES public.hr_employees (id) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS public.pur_buyer_group_categories (
    id integer GENERATED ALWAYS AS IDENTITY,
    buyer_group_id integer NOT NULL,
    item_category_id integer NOT NULL,
    created_by character varying(100) NOT NULL,
    created_at timestamptz NOT NULL,
    updated_by character varying(100),
    updated_at timestamptz,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamptz,
    CONSTRAINT pk_pur_buyer_group_categories PRIMARY KEY (id),
    CONSTRAINT fk_pur_buyer_group_categories_pur_buyer_groups_buyer_group_id FOREIGN KEY (buyer_group_id) REFERENCES public.pur_buyer_groups (id) ON DELETE CASCADE,
    CONSTRAINT fk_pur_buyer_group_categories_inv_item_categories_item_category_id FOREIGN KEY (item_category_id) REFERENCES public.inv_item_categories (id) ON DELETE RESTRICT
);

ALTER TABLE public.fin_vendors ADD COLUMN IF NOT EXISTS vendor_category_id integer;
ALTER TABLE public.fin_vendors ADD COLUMN IF NOT EXISTS buyer_group_id integer;
ALTER TABLE public.fin_vendors ADD COLUMN IF NOT EXISTS is_approved_vendor boolean NOT NULL DEFAULT FALSE;
ALTER TABLE public.fin_vendors ADD COLUMN IF NOT EXISTS approved_date date;
ALTER TABLE public.fin_vendors ADD COLUMN IF NOT EXISTS lead_time_days integer;
ALTER TABLE public.fin_vendors ADD COLUMN IF NOT EXISTS performance_score numeric(5,2);

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'ck_fin_vendors_lead_time_days_non_negative'
    ) THEN
        ALTER TABLE public.fin_vendors
            ADD CONSTRAINT ck_fin_vendors_lead_time_days_non_negative
            CHECK (lead_time_days IS NULL OR lead_time_days >= 0);
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'ck_fin_vendors_performance_score_range'
    ) THEN
        ALTER TABLE public.fin_vendors
            ADD CONSTRAINT ck_fin_vendors_performance_score_range
            CHECK (performance_score IS NULL OR (performance_score >= 0 AND performance_score <= 100));
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'fk_fin_vendors_pur_vendor_categories_vendor_category_id'
    ) THEN
        ALTER TABLE public.fin_vendors
            ADD CONSTRAINT fk_fin_vendors_pur_vendor_categories_vendor_category_id
            FOREIGN KEY (vendor_category_id) REFERENCES public.pur_vendor_categories (id)
            ON DELETE SET NULL;
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'fk_fin_vendors_pur_buyer_groups_buyer_group_id'
    ) THEN
        ALTER TABLE public.fin_vendors
            ADD CONSTRAINT fk_fin_vendors_pur_buyer_groups_buyer_group_id
            FOREIGN KEY (buyer_group_id) REFERENCES public.pur_buyer_groups (id)
            ON DELETE SET NULL;
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS ix_pur_vendor_categories_created_at ON public.pur_vendor_categories (created_at);
CREATE INDEX IF NOT EXISTS ix_pur_vendor_categories_is_deleted ON public.pur_vendor_categories (is_deleted);
CREATE UNIQUE INDEX IF NOT EXISTS ix_pur_vendor_categories_code ON public.pur_vendor_categories (code);
CREATE INDEX IF NOT EXISTS ix_pur_vendor_categories_name ON public.pur_vendor_categories (name);
CREATE INDEX IF NOT EXISTS ix_pur_vendor_categories_is_active ON public.pur_vendor_categories (is_active);

CREATE INDEX IF NOT EXISTS ix_pur_approval_configs_created_at ON public.pur_approval_configs (created_at);
CREATE INDEX IF NOT EXISTS ix_pur_approval_configs_is_deleted ON public.pur_approval_configs (is_deleted);
CREATE INDEX IF NOT EXISTS ix_pur_approval_configs_document_type ON public.pur_approval_configs (document_type);
CREATE INDEX IF NOT EXISTS ix_pur_approval_configs_level ON public.pur_approval_configs (level);
CREATE INDEX IF NOT EXISTS ix_pur_approval_configs_approver_employee_id ON public.pur_approval_configs (approver_employee_id);
CREATE INDEX IF NOT EXISTS ix_pur_approval_configs_is_active ON public.pur_approval_configs (is_active);
CREATE UNIQUE INDEX IF NOT EXISTS ix_pur_approval_configs_document_type_level_min_amount_max_amount ON public.pur_approval_configs (document_type, level, min_amount, max_amount);

CREATE INDEX IF NOT EXISTS ix_pur_buyer_groups_created_at ON public.pur_buyer_groups (created_at);
CREATE INDEX IF NOT EXISTS ix_pur_buyer_groups_is_deleted ON public.pur_buyer_groups (is_deleted);
CREATE UNIQUE INDEX IF NOT EXISTS ix_pur_buyer_groups_code ON public.pur_buyer_groups (code);
CREATE INDEX IF NOT EXISTS ix_pur_buyer_groups_name ON public.pur_buyer_groups (name);
CREATE INDEX IF NOT EXISTS ix_pur_buyer_groups_buyer_employee_id ON public.pur_buyer_groups (buyer_employee_id);
CREATE INDEX IF NOT EXISTS ix_pur_buyer_groups_is_active ON public.pur_buyer_groups (is_active);

CREATE INDEX IF NOT EXISTS ix_pur_buyer_group_categories_created_at ON public.pur_buyer_group_categories (created_at);
CREATE INDEX IF NOT EXISTS ix_pur_buyer_group_categories_is_deleted ON public.pur_buyer_group_categories (is_deleted);
CREATE INDEX IF NOT EXISTS ix_pur_buyer_group_categories_buyer_group_id ON public.pur_buyer_group_categories (buyer_group_id);
CREATE INDEX IF NOT EXISTS ix_pur_buyer_group_categories_item_category_id ON public.pur_buyer_group_categories (item_category_id);
CREATE UNIQUE INDEX IF NOT EXISTS ix_pur_buyer_group_categories_buyer_group_id_item_category_id ON public.pur_buyer_group_categories (buyer_group_id, item_category_id);

CREATE INDEX IF NOT EXISTS ix_fin_vendors_vendor_category_id ON public.fin_vendors (vendor_category_id);
CREATE INDEX IF NOT EXISTS ix_fin_vendors_buyer_group_id ON public.fin_vendors (buyer_group_id);
CREATE INDEX IF NOT EXISTS ix_fin_vendors_is_approved_vendor ON public.fin_vendors (is_approved_vendor);
CREATE INDEX IF NOT EXISTS ix_fin_vendors_performance_score ON public.fin_vendors (performance_score);
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
ALTER TABLE public.fin_vendors DROP CONSTRAINT IF EXISTS fk_fin_vendors_pur_vendor_categories_vendor_category_id;
ALTER TABLE public.fin_vendors DROP CONSTRAINT IF EXISTS fk_fin_vendors_pur_buyer_groups_buyer_group_id;
ALTER TABLE public.fin_vendors DROP CONSTRAINT IF EXISTS ck_fin_vendors_lead_time_days_non_negative;
ALTER TABLE public.fin_vendors DROP CONSTRAINT IF EXISTS ck_fin_vendors_performance_score_range;

DROP INDEX IF EXISTS public.ix_fin_vendors_vendor_category_id;
DROP INDEX IF EXISTS public.ix_fin_vendors_buyer_group_id;
DROP INDEX IF EXISTS public.ix_fin_vendors_is_approved_vendor;
DROP INDEX IF EXISTS public.ix_fin_vendors_performance_score;

ALTER TABLE public.fin_vendors DROP COLUMN IF EXISTS vendor_category_id;
ALTER TABLE public.fin_vendors DROP COLUMN IF EXISTS buyer_group_id;
ALTER TABLE public.fin_vendors DROP COLUMN IF EXISTS is_approved_vendor;
ALTER TABLE public.fin_vendors DROP COLUMN IF EXISTS approved_date;
ALTER TABLE public.fin_vendors DROP COLUMN IF EXISTS lead_time_days;
ALTER TABLE public.fin_vendors DROP COLUMN IF EXISTS performance_score;

DROP TABLE IF EXISTS public.pur_buyer_group_categories;
DROP TABLE IF EXISTS public.pur_approval_configs;
DROP TABLE IF EXISTS public.pur_buyer_groups;
DROP TABLE IF EXISTS public.pur_vendor_categories;
""");
    }
}
