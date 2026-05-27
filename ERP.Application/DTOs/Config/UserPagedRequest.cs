using ERP.Application.DTOs.Common;

namespace ERP.Application.DTOs.Config;

public sealed class UserPagedRequest : PagedRequest
{
    public string? Username { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
}
