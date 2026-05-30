using ERP.Domain.Enums;

namespace ERP.Domain.Entities.Finance;

public sealed class FinCurrency : BaseEntity
{
    public string Code { get; set; } = "IDR";
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public bool IsBaseCurrency { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<FinAccount> Accounts { get; set; } = new List<FinAccount>();
    public ICollection<FinExchangeRate> ExchangeRatesFrom { get; set; } = new List<FinExchangeRate>();
    public ICollection<FinExchangeRate> ExchangeRatesTo { get; set; } = new List<FinExchangeRate>();
    public ICollection<FinJournalEntry> JournalEntries { get; set; } = new List<FinJournalEntry>();
}
