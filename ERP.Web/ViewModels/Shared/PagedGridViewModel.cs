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
    public IReadOnlyDictionary<string, string?> RouteValues { get; init; } = new Dictionary<string, string?>();
    public string PageQueryKey { get; init; } = "page";
    public string PageSizeQueryKey { get; init; } = "pageSize";
    public string SortByQueryKey { get; init; } = "sortBy";
    public string SortDirectionQueryKey { get; init; } = "sortDirection";
}

public sealed class PagedGridColumnViewModel
{
    public string Key { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public bool Sortable { get; init; }
    public string HeaderClass { get; init; } = string.Empty;
    public string CellClass { get; init; } = string.Empty;
}

public sealed class PagedGridRowViewModel
{
    public IReadOnlyList<PagedGridCellViewModel> Cells { get; init; } = [];
}

public sealed class PagedGridCellViewModel
{
    public string? Text { get; init; }
    public IHtmlContent? Html { get; init; }
    public string CellClass { get; init; } = string.Empty;
}
