namespace ERP.Web.ViewModels.Shared;

public sealed class TextNumberViewModel
{
    public string Text { get; init; } = string.Empty;
    public string CssClass { get; init; } = string.Empty;
    public bool HasValue { get; init; }
}
