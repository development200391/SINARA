using System.Text.Json;
using ERP.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace ERP.Infrastructure.Cache;

public sealed class RedisCacheService(
    IDistributedCache cache,
    IConnectionMultiplexer connectionMultiplexer) : ICacheService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var payload = await cache.GetAsync(key, ct);
        if (payload is null)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(payload, SerializerOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(value, SerializerOptions);
        var options = new DistributedCacheEntryOptions();

        if (expiry.HasValue)
        {
            options.SetAbsoluteExpiration(expiry.Value);
        }

        await cache.SetAsync(key, payload, options, ct);
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        return cache.RemoveAsync(key, ct);
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        var database = connectionMultiplexer.GetDatabase();

        foreach (var endpoint in connectionMultiplexer.GetEndPoints())
        {
            var server = connectionMultiplexer.GetServer(endpoint);
            if (!server.IsConnected)
            {
                continue;
            }

            var keys = server.Keys(pattern: $"{prefix}*").ToArray();
            if (keys.Length == 0)
            {
                continue;
            }

            foreach (var key in keys)
            {
                ct.ThrowIfCancellationRequested();
                await database.KeyDeleteAsync(key);
            }
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var payload = await cache.GetAsync(key, ct);
        if (payload is not null)
        {
            var cachedValue = JsonSerializer.Deserialize<T>(payload, SerializerOptions);
            if (cachedValue is not null)
            {
                return cachedValue;
            }
        }

        var value = await factory();
        await SetAsync(key, value, expiry, ct);
        return value;
    }
}
