using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;

namespace ERP.Web.Services;

public sealed class MenuPermissionService(IConfigApiClient configApiClient, IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache) : IMenuPermissionService
{
    private const string PermissionClaimType = "permissions";
    private const string ParsedClaimCacheKey = "menu-permissions:claim-map";
    private const string RequestResultCachePrefix = "menu-permissions:request-result:";
    private const string CrossRequestCachePrefix = "menu-permissions:user:";
    private const string RoleIdsCachePrefix = "menu-permissions:role-ids:";
    private const string MenuUrlMapCacheKey = "menu-permissions:menu-url-map";

    private static readonly TimeSpan PermissionCacheDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan RoleIdsCacheDuration = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan MenuUrlMapCacheDuration = TimeSpan.FromMinutes(30);

    public async Task<MenuPermissionFlags> GetMenuPermissionAsync(
        ClaimsPrincipal user,
        string accessToken,
        string menuUrl,
        string? menuKey = null,
        CancellationToken ct = default)
    {
        var result = await GetMenuPermissionResultAsync(user, accessToken, menuUrl, menuKey, ct);
        return result.Permission;
    }

    public async Task<MenuPermissionResult> GetMenuPermissionResultAsync(
        ClaimsPrincipal user,
        string accessToken,
        string menuUrl,
        string? menuKey = null,
        CancellationToken ct = default)
    {
        if (user.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(accessToken))
        {
            return MenuPermissionResult.NoMatch;
        }

        var normalizedRequestMenuUrl = NormalizeMenuUrl(menuUrl);
        if (string.IsNullOrWhiteSpace(normalizedRequestMenuUrl))
        {
            return MenuPermissionResult.NoMatch;
        }

        var requestCacheKey = BuildRequestCacheKey(user, normalizedRequestMenuUrl, menuKey);
        if (TryGetRequestResultCache(requestCacheKey, out var requestCachedResult))
        {
            return requestCachedResult;
        }

        var claimMap = GetParsedClaimMap(user);

        if (!string.IsNullOrWhiteSpace(menuKey)
            && TryGetClaimPermission(claimMap, menuKey.Trim(), out var permissionByKey))
        {
            return CacheRequestResult(requestCacheKey, new MenuPermissionResult
            {
                IsMenuMatched = true,
                MenuUrl = normalizedRequestMenuUrl,
                Permission = permissionByKey
            });
        }

        var menuMatch = await ResolveMenuMatchAsync(accessToken, normalizedRequestMenuUrl, ct);
        if (!menuMatch.IsMatched)
        {
            return CacheRequestResult(requestCacheKey, MenuPermissionResult.NoMatch);
        }

        var menuIdKey = menuMatch.MenuId.ToString();
        if (TryGetClaimPermission(claimMap, menuIdKey, out var permissionByMenuId)
            || TryGetClaimPermission(claimMap, menuMatch.MenuUrl, out permissionByMenuId)
            || TryGetClaimPermission(claimMap, menuMatch.MenuUrl.TrimStart('/'), out permissionByMenuId))
        {
            return CacheRequestResult(requestCacheKey, new MenuPermissionResult
            {
                IsMenuMatched = true,
                MenuId = menuMatch.MenuId,
                MenuUrl = menuMatch.MenuUrl,
                Permission = permissionByMenuId
            });
        }

        var permissionCacheKey = BuildCrossRequestPermissionCacheKey(user, menuMatch.MenuUrl);
        if (memoryCache.TryGetValue(permissionCacheKey, out MenuPermissionFlags? cachedPermission) && cachedPermission is not null)
        {
            return CacheRequestResult(requestCacheKey, new MenuPermissionResult
            {
                IsMenuMatched = true,
                MenuId = menuMatch.MenuId,
                MenuUrl = menuMatch.MenuUrl,
                Permission = cachedPermission
            });
        }

        var resolvedPermission = await ResolvePermissionByMenuIdAsync(user, accessToken, menuMatch.MenuId, ct);
        memoryCache.Set(permissionCacheKey, resolvedPermission, PermissionCacheDuration);

        return CacheRequestResult(requestCacheKey, new MenuPermissionResult
        {
            IsMenuMatched = true,
            MenuId = menuMatch.MenuId,
            MenuUrl = menuMatch.MenuUrl,
            Permission = resolvedPermission
        });
    }

    private async Task<MenuPermissionFlags> ResolvePermissionByMenuIdAsync(
        ClaimsPrincipal user,
        string accessToken,
        int menuId,
        CancellationToken ct)
    {
        var roleIds = await ResolveRoleIdsAsync(user, accessToken, ct);
        if (roleIds.Count == 0)
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

            var permission = matrix.Permissions.FirstOrDefault(x => x.MenuId == menuId);
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

    private async Task<IReadOnlyList<int>> ResolveRoleIdsAsync(ClaimsPrincipal user, string accessToken, CancellationToken ct)
    {
        var roleNames = user.FindAll(ClaimTypes.Role)
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (roleNames.Count == 0)
        {
            return [];
        }

        var roleCacheKey = BuildRoleIdsCacheKey(user);
        if (memoryCache.TryGetValue(roleCacheKey, out List<int>? cachedRoleIds) && cachedRoleIds is not null)
        {
            return cachedRoleIds;
        }

        var roles = await configApiClient.GetRolesAsync(accessToken, ct);
        var roleIds = roles
            .Where(x => x.IsActive && roleNames.Contains(x.Name, StringComparer.OrdinalIgnoreCase))
            .Select(x => x.Id)
            .Distinct()
            .ToList();

        memoryCache.Set(roleCacheKey, roleIds, RoleIdsCacheDuration);
        return roleIds;
    }

    private async Task<MenuMatch> ResolveMenuMatchAsync(string accessToken, string normalizedRequestMenuUrl, CancellationToken ct)
    {
        var menuMap = await GetMenuUrlMapAsync(accessToken, ct);
        var candidates = BuildMenuUrlCandidates(normalizedRequestMenuUrl);

        foreach (var candidate in candidates)
        {
            if (menuMap.TryGetValue(candidate, out var menuId))
            {
                return new MenuMatch(true, menuId, candidate);
            }
        }

        return MenuMatch.NotMatched;
    }

    private async Task<IReadOnlyDictionary<string, int>> GetMenuUrlMapAsync(string accessToken, CancellationToken ct)
    {
        if (memoryCache.TryGetValue(MenuUrlMapCacheKey, out Dictionary<string, int>? cachedMap) && cachedMap is not null)
        {
            return cachedMap;
        }

        var modules = await configApiClient.GetModulesAsync(accessToken, ct);
        if (modules.Count == 0)
        {
            var empty = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            memoryCache.Set(MenuUrlMapCacheKey, empty, MenuUrlMapCacheDuration);
            return empty;
        }

        var menuTasks = modules
            .Where(x => x.IsActive)
            .Select(x => configApiClient.GetMenusByModuleAsync(accessToken, x.Id, ct))
            .ToList();

        await Task.WhenAll(menuTasks);

        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var menu in menuTasks.SelectMany(x => x.Result))
        {
            var normalizedMenuUrl = NormalizeMenuUrl(menu.Url);
            if (string.IsNullOrWhiteSpace(normalizedMenuUrl))
            {
                continue;
            }

            map.TryAdd(normalizedMenuUrl, menu.Id);
        }

        memoryCache.Set(MenuUrlMapCacheKey, map, MenuUrlMapCacheDuration);
        return map;
    }

    private static IReadOnlyList<string> BuildMenuUrlCandidates(string normalizedRequestMenuUrl)
    {
        var trimmed = normalizedRequestMenuUrl.Trim('/');
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return [];
        }

        var segments = trimmed.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var candidates = new List<string>(segments.Length);

        for (var i = segments.Length; i >= 1; i--)
        {
            var candidate = $"/{string.Join('/', segments.Take(i))}";
            if (!candidates.Contains(candidate, StringComparer.OrdinalIgnoreCase))
            {
                candidates.Add(candidate);
            }
        }

        return candidates;
    }

    private bool TryGetClaimPermission(
        IReadOnlyDictionary<string, MenuPermissionFlags> map,
        string key,
        out MenuPermissionFlags permission)
    {
        permission = MenuPermissionFlags.None;
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        if (!map.TryGetValue(key, out var found) || found is null)
        {
            return false;
        }

        permission = found;
        return true;
    }

    private IReadOnlyDictionary<string, MenuPermissionFlags> GetParsedClaimMap(ClaimsPrincipal user)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is not null
            && httpContext.Items.TryGetValue(ParsedClaimCacheKey, out var cached)
            && cached is Dictionary<string, MenuPermissionFlags> cachedMap)
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

        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var absoluteUri))
        {
            trimmed = absoluteUri.PathAndQuery;
        }

        var queryOrHashIndex = trimmed.IndexOfAny(['?', '#']);
        if (queryOrHashIndex >= 0)
        {
            trimmed = trimmed[..queryOrHashIndex];
        }

        if (!trimmed.StartsWith("/", StringComparison.Ordinal))
        {
            trimmed = $"/{trimmed}";
        }

        return trimmed.TrimEnd('/').ToLowerInvariant();
    }

    private string BuildRequestCacheKey(ClaimsPrincipal user, string normalizedRequestMenuUrl, string? menuKey)
    {
        var userKey = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        var normalizedMenuKey = string.IsNullOrWhiteSpace(menuKey) ? "-" : menuKey.Trim();
        return $"{RequestResultCachePrefix}{userKey}:{normalizedRequestMenuUrl}:{normalizedMenuKey}";
    }

    private static string BuildCrossRequestPermissionCacheKey(ClaimsPrincipal user, string normalizedMenuUrl)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        var roleSignature = BuildRoleSignature(user);
        return $"{CrossRequestCachePrefix}{userId}:{roleSignature}:{normalizedMenuUrl}";
    }

    private static string BuildRoleIdsCacheKey(ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        var roleSignature = BuildRoleSignature(user);
        return $"{RoleIdsCachePrefix}{userId}:{roleSignature}";
    }

    private static string BuildRoleSignature(ClaimsPrincipal user) =>
        string.Join(",", user.FindAll(ClaimTypes.Role)
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase));

    private bool TryGetRequestResultCache(string key, out MenuPermissionResult result)
    {
        result = MenuPermissionResult.NoMatch;
        var items = httpContextAccessor.HttpContext?.Items;
        if (items is null)
        {
            return false;
        }

        if (!items.TryGetValue(key, out var cached) || cached is not MenuPermissionResult cachedResult)
        {
            return false;
        }

        result = cachedResult;
        return true;
    }

    private MenuPermissionResult CacheRequestResult(string key, MenuPermissionResult result)
    {
        var items = httpContextAccessor.HttpContext?.Items;
        if (items is not null)
        {
            items[key] = result;
        }

        return result;
    }

    private readonly record struct MenuMatch(bool IsMatched, int MenuId, string MenuUrl)
    {
        public static readonly MenuMatch NotMatched = new(false, 0, string.Empty);
    }
}
