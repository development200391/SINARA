namespace ERP.Web.ViewModels.Shared;

public sealed class TabsViewModel
{
    public string Id { get; init; } = string.Empty;
    public string AriaLabel { get; init; } = "Tabs";
    public string ContainerClass { get; init; } = string.Empty;
    public bool Fill { get; init; }
    public IReadOnlyList<TabItemViewModel> Items { get; init; } = [];
}

public sealed class TabItemViewModel
{
    public string Key { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Url { get; init; }
    public string? IconClass { get; init; }
    public string? BadgeText { get; init; }
    public bool IsActive { get; init; }
    public bool Disabled { get; init; }
    public string? TabPaneId { get; init; }
    public string CssClass { get; init; } = string.Empty;
}
