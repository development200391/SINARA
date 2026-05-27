using ERP.Application.DTOs.Common;

namespace ERP.Application.DTOs.Config;

public sealed class AuditLogPagedRequest : PagedRequest
{
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public string? Status { get; set; }
    public bool HasIpOnly { get; set; }
    public string? EntityNames { get; set; }
}
