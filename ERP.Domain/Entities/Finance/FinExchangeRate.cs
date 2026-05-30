namespace ERP.Domain.Entities.Finance;

public sealed class FinExchangeRate
{
    public int Id { get; set; }
    public string FromCurrencyCode { get; set; } = string.Empty;
    public string ToCurrencyCode { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public string? Source { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public FinCurrency FromCurrency { get; set; } = null!;
    public FinCurrency ToCurrency { get; set; } = null!;
}
