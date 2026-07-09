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

    /// <summary>Id of another element whose value narrows down which options are shown (options are matched via <see cref="SearchableSelectOptionViewModel.GroupValue"/>).</summary>
    public string? DependsOnElementId { get; set; }
    public IReadOnlyList<SearchableSelectOptionViewModel> Options { get; set; } = [];
}

public sealed class SearchableSelectOptionViewModel
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool IsDisabled { get; set; }

    /// <summary>Value matched against the current value of <see cref="SearchableSelectViewModel.DependsOnElementId"/> to decide visibility.</summary>
    public string? GroupValue { get; set; }
}
