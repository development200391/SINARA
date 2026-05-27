namespace ERP.Web.ViewModels.Shared;

public sealed class SearchableSelectViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string CssClass { get; set; } = "form-select";
    public string SearchPlaceholder { get; set; } = "Search";
    public string EmptyOptionLabel { get; set; } = "Select";
    public string? SelectedValue { get; set; }
    public bool IsRequired { get; set; }
    public bool IsDisabled { get; set; }
    public IReadOnlyList<SearchableSelectOptionViewModel> Options { get; set; } = [];
}

public sealed class SearchableSelectOptionViewModel
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool IsDisabled { get; set; }
}
