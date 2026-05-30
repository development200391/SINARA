namespace ERP.Domain.Entities.Finance;

public sealed class FinBudget : BaseEntity
{
    public string BudgetNo { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int FiscalYearId { get; set; }
    public int? PeriodId { get; set; }
    public int? CostCenterId { get; set; }
    public int? AccountId { get; set; }
    public string CurrencyCode { get; set; } = "IDR";
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public FinFiscalYear FiscalYear { get; set; } = null!;
    public FinPeriod? Period { get; set; }
    public FinCostCenter? CostCenter { get; set; }
    public FinAccount? Account { get; set; }
    public FinCurrency Currency { get; set; } = null!;
    public ICollection<FinBudgetLine> Lines { get; set; } = new List<FinBudgetLine>();
}
