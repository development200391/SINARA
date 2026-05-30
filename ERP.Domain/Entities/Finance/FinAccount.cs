using ERP.Domain.Enums;

namespace ERP.Domain.Entities.Finance;

public sealed class FinAccount : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int GroupId { get; set; }
    public FinanceAccountType Type { get; set; }
    public FinanceNormalBalance NormalBalance { get; set; } = FinanceNormalBalance.Debit;
    public bool IsHeader { get; set; }
    public int? ParentAccountId { get; set; }
    public string? Description { get; set; }
    public bool IsBankAccount { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNo { get; set; }
    public string CurrencyCode { get; set; } = "IDR";
    public bool IsActive { get; set; } = true;

    public FinAccountGroup Group { get; set; } = null!;
    public FinAccount? ParentAccount { get; set; }
    public ICollection<FinAccount> ChildAccounts { get; set; } = new List<FinAccount>();
    public FinCurrency Currency { get; set; } = null!;
    public ICollection<FinCostCenter> BudgetCostCenters { get; set; } = new List<FinCostCenter>();
    public ICollection<FinTaxCode> TaxCodes { get; set; } = new List<FinTaxCode>();
    public ICollection<FinJournalEntryLine> JournalLines { get; set; } = new List<FinJournalEntryLine>();
    public ICollection<FinVendor> VendorDefaultAccounts { get; set; } = new List<FinVendor>();
    public ICollection<FinCustomer> CustomerDefaultAccounts { get; set; } = new List<FinCustomer>();
    public ICollection<FinApInvoiceLine> ApInvoiceLines { get; set; } = new List<FinApInvoiceLine>();
    public ICollection<FinArInvoiceLine> ArInvoiceLines { get; set; } = new List<FinArInvoiceLine>();
    public ICollection<FinApPayment> ApPayments { get; set; } = new List<FinApPayment>();
    public ICollection<FinArReceipt> ArReceipts { get; set; } = new List<FinArReceipt>();
    public ICollection<FinBudget> Budgets { get; set; } = new List<FinBudget>();
    public ICollection<FinBudgetLine> BudgetLines { get; set; } = new List<FinBudgetLine>();
}
