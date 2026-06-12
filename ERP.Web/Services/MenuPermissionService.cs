using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;

namespace ERP.Web.Services;

public sealed class MenuPermissionService(IConfigApiClient configApiClient, IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache) : IMenuPermissionService
{
    private const string PermissionClaimType = "permissions";
    private const string ParsedClaimCacheKey = "menu-permissions:claim-map";
    private const string RequestCachePrefix = "menu-permissions:request:";
    private const string CrossRequestCachePrefix = "menu-permissions:user:";
    private const string MenuIdCachePrefix = "menu-permissions:menu-id:";
    private static readonly TimeSpan PermissionCacheDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan MenuIdCacheDuration = TimeSpan.FromMinutes(30);

    public async Task<MenuPermissionFlags> GetMenuPermissionAsync(
        ClaimsPrincipal user,
        string accessToken,
        string menuUrl,
        string? menuKey = null,
        CancellationToken ct = default)
    {
        if (user.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(accessToken))
        {
            return MenuPermissionFlags.None;
        }

        var normalizedMenuUrl = NormalizeMenuUrl(menuUrl);
        if (string.IsNullOrWhiteSpace(normalizedMenuUrl))
        {
            return MenuPermissionFlags.None;
        }

        var requestKey = BuildRequestKey(user, normalizedMenuUrl, menuKey);
        if (TryGetRequestCache(requestKey, out var requestCachedPermission))
        {
            return requestCachedPermission;
        }

        if (TryGetFromClaim(user, normalizedMenuUrl, menuKey, out var claimPermission))
        {
            return CacheRequest(requestKey, claimPermission);
        }

        var crossRequestKey = BuildCrossRequestKey(user, normalizedMenuUrl);
        if (memoryCache.TryGetValue(crossRequestKey, out MenuPermissionFlags? cachedPermission) && cachedPermission is not null)
        {
            return CacheRequest(requestKey, cachedPermission);
        }

        var resolved = await ResolveFromApiAsync(user, accessToken, normalizedMenuUrl, ct);
        memoryCache.Set(crossRequestKey, resolved, PermissionCacheDuration);

        return CacheRequest(requestKey, resolved);
    }

    private async Task<MenuPermissionFlags> ResolveFromApiAsync(
        ClaimsPrincipal user,
        string accessToken,
        string normalizedMenuUrl,
        CancellationToken ct)
    {
        var roleNames = user.FindAll(ClaimTypes.Role)
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (roleNames.Count == 0)
        {
            return MenuPermissionFlags.None;
        }

        var roles = await configApiClient.GetRolesAsync(accessToken, ct);
        var roleIds = roles
            .Where(x => x.IsActive && roleNames.Contains(x.Name, StringComparer.OrdinalIgnoreCase))
            .Select(x => x.Id)
            .Distinct()
            .ToList();

        if (roleIds.Count == 0)
        {
            return MenuPermissionFlags.None;
        }

        var menuId = await ResolveMenuIdAsync(accessToken, normalizedMenuUrl, ct);
        if (!menuId.HasValue)
        {
            return MenuPermissionFlags.None;
        }

        var matrixTasks = roleIds
            .Select(roleId => configApiClient.GetRolePermissionsAsync(accessToken, roleId, ct))
            .ToList();

        await Task.WhenAll(matrixTasks);

        var result = new MenuPermissionFlags();
        foreach (var matrixTask in matrixTasks)
        {
            var matrix = matrixTask.Result;
            if (matrix is null)
            {
                continue;
            }

            var permission = matrix.Permissions.FirstOrDefault(x => x.MenuId == menuId.Value);
            if (permission is null)
            {
                continue;
            }

            result = new MenuPermissionFlags
            {
                CanView = result.CanView || permission.CanView,
                CanCreate = result.CanCreate || permission.CanCreate,
                CanEdit = result.CanEdit || permission.CanEdit,
                CanDelete = result.CanDelete || permission.CanDelete
            };
        }

        return result;
    }

    private async Task<int?> ResolveMenuIdAsync(string accessToken, string normalizedMenuUrl, CancellationToken ct)
    {
        var cacheKey = $"{MenuIdCachePrefix}{normalizedMenuUrl}";
        if (memoryCache.TryGetValue(cacheKey, out int cachedMenuId))
        {
            return cachedMenuId > 0 ? cachedMenuId : null;
        }

        var modules = await configApiClient.GetModulesAsync(accessToken, ct);
        if (modules.Count == 0)
        {
            memoryCache.Set(cacheKey, 0, MenuIdCacheDuration);
            return null;
        }

        var menuTasks = modules
            .Where(x => x.IsActive)
            .Select(x => configApiClient.GetMenusByModuleAsync(accessToken, x.Id, ct))
            .ToList();

        await Task.WhenAll(menuTasks);

        var menu = menuTasks
            .SelectMany(x => x.Result)
            .FirstOrDefault(x => string.Equals(NormalizeMenuUrl(x.Url), normalizedMenuUrl, StringComparison.OrdinalIgnoreCase));

        var menuId = menu?.Id ?? 0;
        memoryCache.Set(cacheKey, menuId, MenuIdCacheDuration);

        return menuId > 0 ? menuId : null;
    }

    private bool TryGetFromClaim(
        ClaimsPrincipal user,
        string normalizedMenuUrl,
        string? menuKey,
        out MenuPermissionFlags permission)
    {
        permission = MenuPermissionFlags.None;

        var map = GetParsedClaimMap(user);
        if (map.Count == 0)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(menuKey)
            && map.TryGetValue(menuKey.Trim(), out var byMenuKey)
            && byMenuKey is not null)
        {
            permission = byMenuKey;
            return true;
        }

        if (map.TryGetValue(normalizedMenuUrl, out var byUrl) && byUrl is not null)
        {
            permission = byUrl;
            return true;
        }

        var urlWithoutLeadingSlash = normalizedMenuUrl.TrimStart('/');
        if (map.TryGetValue(urlWithoutLeadingSlash, out var byRelativeUrl) && byRelativeUrl is not null)
        {
            permission = byRelativeUrl;
            return true;
        }

        return false;
    }

    private IReadOnlyDictionary<string, MenuPermissionFlags> GetParsedClaimMap(ClaimsPrincipal user)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is not null && httpContext.Items.TryGetValue(ParsedClaimCacheKey, out var cached) && cached is Dictionary<string, MenuPermissionFlags> cachedMap)
        {
            return cachedMap;
        }

        var map = new Dictionary<string, MenuPermissionFlags>(StringComparer.OrdinalIgnoreCase);
        var rawClaim = user.FindFirstValue(PermissionClaimType);

        if (!string.IsNullOrWhiteSpace(rawClaim))
        {
            var entries = rawClaim.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var entry in entries)
            {
                var parts = entry.Split(':', 2, StringSplitOptions.TrimEntries);
                if (parts.Length != 2)
                {
                    continue;
                }

                var key = parts[0];
                var value = parts[1];
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                if (TryParsePermissionValue(value, out var parsed))
                {
                    map[key] = parsed;
                }
            }
        }

        if (httpContext is not null)
        {
            httpContext.Items[ParsedClaimCacheKey] = map;
        }

        return map;
    }

    private static bool TryParsePermissionValue(string value, out MenuPermissionFlags permission)
    {
        permission = MenuPermissionFlags.None;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (value.Contains(',', StringComparison.Ordinal))
        {
            var flags = value.Split(',', StringSplitOptions.TrimEntries);
            if (flags.Length < 4)
            {
                return false;
            }

            permission = new MenuPermissionFlags
            {
                CanView = IsAllowed(flags[0]),
                CanCreate = IsAllowed(flags[1]),
                CanEdit = IsAllowed(flags[2]),
                CanDelete = IsAllowed(flags[3])
            };
            return true;
        }

        if (!int.TryParse(value, out var mask))
        {
            return false;
        }

        permission = new MenuPermissionFlags
        {
            CanView = (mask & 1) == 1,
            CanCreate = (mask & 2) == 2,
            CanEdit = (mask & 4) == 4,
            CanDelete = (mask & 8) == 8
        };
        return true;
    }

    private static bool IsAllowed(string value) =>
        string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)
        || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeMenuUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return string.Empty;
        }

        var trimmed = url.Trim();
        if (!trimmed.StartsWith("/", StringComparison.Ordinal))
        {
            trimmed = $"/{trimmed}";
        }

        return trimmed.TrimEnd('/').ToLowerInvariant();
    }

    private string BuildRequestKey(ClaimsPrincipal user, string normalizedMenuUrl, string? menuKey)
    {
        var userKey = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        var normalizedMenuKey = string.IsNullOrWhiteSpace(menuKey) ? "-" : menuKey.Trim();
        return $"{RequestCachePrefix}{userKey}:{normalizedMenuUrl}:{normalizedMenuKey}";
    }

    private static string BuildCrossRequestKey(ClaimsPrincipal user, string normalizedMenuUrl)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        var roleSignature = string.Join(",", user.FindAll(ClaimTypes.Role)
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase));

        return $"{CrossRequestCachePrefix}{userId}:{roleSignature}:{normalizedMenuUrl}";
    }

    private bool TryGetRequestCache(string key, out MenuPermissionFlags permission)
    {
        permission = MenuPermissionFlags.None;
        var items = httpContextAccessor.HttpContext?.Items;
        if (items is null)
        {
            return false;
        }

        if (!items.TryGetValue(key, out var cached) || cached is not MenuPermissionFlags cachedPermission)
        {
            return false;
        }

        permission = cachedPermission;
        return true;
    }

    private MenuPermissionFlags CacheRequest(string key, MenuPermissionFlags permission)
    {
        var items = httpContextAccessor.HttpContext?.Items;
        if (items is not null)
        {
            items[key] = permission;
        }

        return permission;
    }
}
