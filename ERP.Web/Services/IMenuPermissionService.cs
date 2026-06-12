using System.Security.Claims;

namespace ERP.Web.Services;

public interface IMenuPermissionService
{
    Task<MenuPermissionFlags> GetMenuPermissionAsync(
        ClaimsPrincipal user,
        string accessToken,
        string menuUrl,
        string? menuKey = null,
        CancellationToken ct = default);
}
