namespace ERP.Web.ViewModels.Shared;

public class PagedGridStateViewModel
{
    public string? Search { get; set; }
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public string SortDirection { get; set; } = "desc";
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public string? Status { get; set; }
    public bool HasIpOnly { get; set; }
    public IReadOnlyList<string> SelectedEntityNames { get; set; } = [];
    public IReadOnlyList<string> StatusOptions { get; set; } = [];
    public IReadOnlyList<string> EntityNameOptions { get; set; } = [];

}
