namespace ERP.Application.DTOs.Common;

public sealed class PagedRequest
{
    private const int MaxPageSize = 200;

    public int Page { get; set; } = 1;

    private int _pageSize = 20;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            <= 0 => 20,
            > MaxPageSize => MaxPageSize,
            _ => value
        };
    }

    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "desc";
}
