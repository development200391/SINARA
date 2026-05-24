using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Config;

namespace ERP.Web.ViewModels.Config;

public sealed class ConfigAuditIndexViewModel
{
    public string? Search { get; set; }
    public PagedResult<AuditLogDto> Logs { get; set; } = PagedResult<AuditLogDto>.Create([], 0, 1, 20);
}
