using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Config;

namespace ERP.Web.ViewModels.Config;

public sealed class ConfigAuditIndexViewModel
{
    public string? Search { get; set; }
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "createdAt";
    public string SortDirection { get; set; } = "desc";

    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public string? Status { get; set; }
    public bool HasIpOnly { get; set; }
    public IReadOnlyList<string> SelectedEntityNames { get; set; } = [];
    public IReadOnlyList<string> StatusOptions { get; set; } = [];
    public IReadOnlyList<string> EntityNameOptions { get; set; } = [];

    public PagedResult<AuditLogDto> Logs { get; set; } = PagedResult<AuditLogDto>.Create([], 0, 1, 20);
}
