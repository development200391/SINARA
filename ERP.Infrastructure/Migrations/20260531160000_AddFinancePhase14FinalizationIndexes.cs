using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260531160000_AddFinancePhase14FinalizationIndexes")]
public partial class AddFinancePhase14FinalizationIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
CREATE INDEX IF NOT EXISTS ix_fin_accounts_type_is_active
    ON public.fin_accounts (type, is_active);

CREATE INDEX IF NOT EXISTS ix_fin_journal_entries_date_period_status
    ON public.fin_journal_entries (date, period_id, status);

CREATE INDEX IF NOT EXISTS ix_fin_journal_entries_source_source_ref_id
    ON public.fin_journal_entries (source, source_ref_id);

CREATE INDEX IF NOT EXISTS ix_fin_ap_invoices_vendor_status_due_date
    ON public.fin_ap_invoices (vendor_id, status, due_date);

CREATE INDEX IF NOT EXISTS ix_fin_ar_invoices_customer_status_due_date
    ON public.fin_ar_invoices (customer_id, status, due_date);

CREATE INDEX IF NOT EXISTS ix_fin_budget_lines_budget_account_period
    ON public.fin_budget_lines (budget_id, account_id, period_id);
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
DROP INDEX IF EXISTS public.ix_fin_budget_lines_budget_account_period;
DROP INDEX IF EXISTS public.ix_fin_ar_invoices_customer_status_due_date;
DROP INDEX IF EXISTS public.ix_fin_ap_invoices_vendor_status_due_date;
DROP INDEX IF EXISTS public.ix_fin_journal_entries_source_source_ref_id;
DROP INDEX IF EXISTS public.ix_fin_journal_entries_date_period_status;
DROP INDEX IF EXISTS public.ix_fin_accounts_type_is_active;
""");
    }
}