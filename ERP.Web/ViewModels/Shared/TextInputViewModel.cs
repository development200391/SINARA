namespace ERP.Web.ViewModels.Shared;

public sealed class TextInputViewModel
{
    public string Name { get; set; } = string.Empty;
    public string? Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string? Placeholder { get; set; }
    public bool IsRequired { get; set; }
    public int? MaxLength { get; set; }
    public bool IsReadOnly { get; set; }
    public string? HelpText { get; set; }
}
