namespace ERP.Domain.Entities.Finance;

public sealed class FinBudgetLine
{
    public int Id { get; set; }
    public int BudgetId { get; set; }
    public int LineNo { get; set; }
    public int PeriodId { get; set; }
    public int AccountId { get; set; }
    public int? CostCenterId { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }

    public FinBudget Budget { get; set; } = null!;
    public FinPeriod Period { get; set; } = null!;
    public FinAccount Account { get; set; } = null!;
    public FinCostCenter? CostCenter { get; set; }
}
