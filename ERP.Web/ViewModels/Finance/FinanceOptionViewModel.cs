namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceIdOptionViewModel
{
    public int Id { get; init; }
    public string Label { get; init; } = string.Empty;
}

public sealed class FinanceCodeOptionViewModel
{
    public string Code { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
}
