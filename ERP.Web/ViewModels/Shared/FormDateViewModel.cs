namespace ERP.Web.ViewModels.Shared;

public sealed class FormDateViewModel
{
    public string Name { get; set; } = string.Empty;
    public string? Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public DateOnly? Value { get; set; }
    public bool IsRequired { get; set; }
    public string? Error { get; set; }
    public IDictionary<string, string>? HtmlAttributes { get; set; }
}
