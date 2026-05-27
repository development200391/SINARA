using ERP.Application.DTOs.Common;
using ERP.Application.DTOs.Config;
using ERP.Web.ViewModels.Shared;

namespace ERP.Web.ViewModels.Config;

public sealed class ConfigAuditIndexViewModel : PagedGridStateViewModel
{
    public ConfigAuditIndexViewModel()
    {
        SortBy = "createdAt";
    }

    public PagedResult<AuditLogDto> Logs { get; set; } = PagedResult<AuditLogDto>.Create([], 0, 1, 20);
}
