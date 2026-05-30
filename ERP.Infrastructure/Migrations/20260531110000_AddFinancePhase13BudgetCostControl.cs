using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260531110000_AddFinancePhase13BudgetCostControl")]
public partial class AddFinancePhase13BudgetCostControl : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
CREATE TABLE IF NOT EXISTS public.fin_budgets (
    id              INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    budget_no       VARCHAR(30) NOT NULL,
    name            VARCHAR(200) NOT NULL,
    fiscal_year_id  INTEGER NOT NULL,
    period_id       INTEGER,
    cost_center_id  INTEGER,
    account_id      INTEGER,
    currency_code   VARCHAR(10) NOT NULL DEFAULT 'IDR',
    total_amount    NUMERIC(18,4) NOT NULL DEFAULT 0,
    notes           TEXT,
    is_active       BOOLEAN NOT NULL DEFAULT TRUE,
    created_by      VARCHAR(100) NOT NULL,
    created_at      TIMESTAMPTZ NOT NULL,
    updated_by      VARCHAR(100),
    updated_at      TIMESTAMPTZ,
    is_deleted      BOOLEAN NOT NULL DEFAULT FALSE,
    deleted_at      TIMESTAMPTZ,
    CONSTRAINT ck_fin_budgets_non_negative_total CHECK (total_amount >= 0),
    CONSTRAINT fk_fin_budgets_fin_fiscal_years_fiscal_year_id
        FOREIGN KEY (fiscal_year_id) REFERENCES public.fin_fiscal_years (id) ON DELETE RESTRICT,
    CONSTRAINT fk_fin_budgets_fin_periods_period_id
        FOREIGN KEY (period_id) REFERENCES public.fin_periods (id) ON DELETE SET NULL,
    CONSTRAINT fk_fin_budgets_fin_cost_centers_cost_center_id
        FOREIGN KEY (cost_center_id) REFERENCES public.fin_cost_centers (id) ON DELETE SET NULL,
    CONSTRAINT fk_fin_budgets_fin_accounts_account_id
        FOREIGN KEY (account_id) REFERENCES public.fin_accounts (id) ON DELETE SET NULL,
    CONSTRAINT fk_fin_budgets_fin_currencies_currency_code
        FOREIGN KEY (currency_code) REFERENCES public.fin_currencies (code) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS public.fin_budget_lines (
    id              INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    budget_id       INTEGER NOT NULL,
    line_no         INTEGER NOT NULL,
    period_id       INTEGER NOT NULL,
    account_id      INTEGER NOT NULL,
    cost_center_id  INTEGER,
    description     VARCHAR(200),
    amount          NUMERIC(18,4) NOT NULL,
    CONSTRAINT ck_fin_budget_lines_non_negative_amount CHECK (amount >= 0),
    CONSTRAINT fk_fin_budget_lines_fin_budgets_budget_id
        FOREIGN KEY (budget_id) REFERENCES public.fin_budgets (id) ON DELETE CASCADE,
    CONSTRAINT fk_fin_budget_lines_fin_periods_period_id
        FOREIGN KEY (period_id) REFERENCES public.fin_periods (id) ON DELETE RESTRICT,
    CONSTRAINT fk_fin_budget_lines_fin_accounts_account_id
        FOREIGN KEY (account_id) REFERENCES public.fin_accounts (id) ON DELETE RESTRICT,
    CONSTRAINT fk_fin_budget_lines_fin_cost_centers_cost_center_id
        FOREIGN KEY (cost_center_id) REFERENCES public.fin_cost_centers (id) ON DELETE SET NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_fin_budgets_budget_no ON public.fin_budgets (budget_no);
CREATE INDEX IF NOT EXISTS ix_fin_budgets_name ON public.fin_budgets (name);
CREATE INDEX IF NOT EXISTS ix_fin_budgets_fiscal_year_id ON public.fin_budgets (fiscal_year_id);
CREATE INDEX IF NOT EXISTS ix_fin_budgets_period_id ON public.fin_budgets (period_id);
CREATE INDEX IF NOT EXISTS ix_fin_budgets_cost_center_id ON public.fin_budgets (cost_center_id);
CREATE INDEX IF NOT EXISTS ix_fin_budgets_account_id ON public.fin_budgets (account_id);
CREATE INDEX IF NOT EXISTS ix_fin_budgets_currency_code ON public.fin_budgets (currency_code);
CREATE INDEX IF NOT EXISTS ix_fin_budgets_is_active ON public.fin_budgets (is_active);
CREATE INDEX IF NOT EXISTS ix_fin_budgets_total_amount ON public.fin_budgets (total_amount);
CREATE INDEX IF NOT EXISTS ix_fin_budgets_created_at ON public.fin_budgets (created_at);
CREATE INDEX IF NOT EXISTS ix_fin_budgets_is_deleted ON public.fin_budgets (is_deleted);

CREATE INDEX IF NOT EXISTS ix_fin_budget_lines_budget_id ON public.fin_budget_lines (budget_id);
CREATE UNIQUE INDEX IF NOT EXISTS ix_fin_budget_lines_budget_id_line_no ON public.fin_budget_lines (budget_id, line_no);
CREATE INDEX IF NOT EXISTS ix_fin_budget_lines_period_id ON public.fin_budget_lines (period_id);
CREATE INDEX IF NOT EXISTS ix_fin_budget_lines_account_id ON public.fin_budget_lines (account_id);
CREATE INDEX IF NOT EXISTS ix_fin_budget_lines_cost_center_id ON public.fin_budget_lines (cost_center_id);
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
DROP TABLE IF EXISTS public.fin_budget_lines;
DROP TABLE IF EXISTS public.fin_budgets;
""");
    }
}
