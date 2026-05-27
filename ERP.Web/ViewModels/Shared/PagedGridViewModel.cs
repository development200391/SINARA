using Microsoft.AspNetCore.Html;

namespace ERP.Web.ViewModels.Shared;

public sealed class PagedGridViewModel
{
    public IReadOnlyList<PagedGridColumnViewModel> Columns { get; init; } = [];
    public IReadOnlyList<PagedGridRowViewModel> Rows { get; init; } = [];
    public string EmptyMessage { get; init; } = "No data";
    public int Page { get; init; } = 1;
    public int TotalPages { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public IReadOnlyList<int> PageSizeOptions { get; init; } = [20, 50, 100];
    public string? SortBy { get; init; }
    public string SortDirection { get; init; } = "desc";

    public bool EnableSearch { get; init; }
    public string SearchQueryKey { get; init; } = "search";
    public string? SearchValue { get; init; }
    public string SearchPlaceholder { get; init; } = "Search";
    public string SearchButtonText { get; init; } = "Search";

    public string? ExportUrl { get; init; }
    public string ExportButtonText { get; init; } = "Export Excel";

    public PagedGridActionConfigViewModel Actions { get; init; } = new();

    public IReadOnlyDictionary<string, string?> RouteValues { get; init; } = new Dictionary<string, string?>();
    public string PageQueryKey { get; init; } = "page";
    public string PageSizeQueryKey { get; init; } = "pageSize";
    public string SortByQueryKey { get; init; } = "sortBy";
    public string SortDirectionQueryKey { get; init; } = "sortDirection";
}

public sealed class PagedGridActionConfigViewModel
{
    public string ActionColumnTitle { get; init; } = "Action";
    public string? AddUrl { get; init; }
    public string AddButtonText { get; init; } = "Tambah";
    public string DetailButtonText { get; init; } = "Detail";
    public string EditButtonText { get; init; } = "Ubah";
    public string DeleteButtonText { get; init; } = "Hapus";
    public string DeleteConfirmMessage { get; init; } = "Delete this data?";
}

public sealed class PagedGridColumnViewModel
{
    public string Key { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public bool Sortable { get; init; }
    public string HeaderClass { get; init; } = string.Empty;
    public string CellClass { get; init; } = string.Empty;
    public PagedGridFilterViewModel? Filter { get; init; }
}

public enum PagedGridFilterType
{
    Text,
    DatePicker,
    DateRange,
    NumberRange,
    Select,
    Checkbox,
    MultiSelect
}

public sealed class PagedGridFilterViewModel
{
    public PagedGridFilterType Type { get; init; }
    public string QueryKey { get; init; } = string.Empty;
    public string? QueryKeyTo { get; init; }
    public string? Value { get; init; }
    public string? ValueTo { get; init; }
    public bool IsChecked { get; init; }
    public string CheckedValue { get; init; } = "true";
    public string? Placeholder { get; init; }
    public string? PlaceholderTo { get; init; }
    public string EmptyOptionLabel { get; init; } = "All";
    public string Label { get; init; } = "Yes";
    public int Size { get; init; } = 4;
    public IReadOnlyList<string> Values { get; init; } = [];
    public IReadOnlyList<PagedGridFilterOptionViewModel> Options { get; init; } = [];
}

public sealed class PagedGridFilterOptionViewModel
{
    public string Value { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
}

public sealed class PagedGridRowViewModel
{
    public IReadOnlyList<PagedGridCellViewModel> Cells { get; init; } = [];
    public PagedGridRowActionViewModel? Actions { get; init; }
}

public sealed class PagedGridRowActionViewModel
{
    public string? DetailUrl { get; init; }
    public string? EditUrl { get; init; }
    public string? DeleteUrl { get; init; }
    public string? DeleteConfirmMessage { get; init; }
}

public sealed class PagedGridCellViewModel
{
    public string? Text { get; init; }
    public IHtmlContent? Html { get; init; }
    public string CellClass { get; init; } = string.Empty;
}
