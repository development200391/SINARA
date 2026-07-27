namespace ERP.Web.ViewModels.Shared;

public sealed class FormCheckboxViewModel
{
    public string Name { get; set; } = string.Empty;
    public string? Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = "true";
    public bool Checked { get; set; }
    public bool IsRequired { get; set; }
    public bool EmitHiddenFallback { get; set; } = true;
    public string? Error { get; set; }
    public IDictionary<string, string>? HtmlAttributes { get; set; }
}
