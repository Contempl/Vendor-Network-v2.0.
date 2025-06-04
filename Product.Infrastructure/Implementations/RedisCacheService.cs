using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Product.Application.ServiceInterfaces;

namespace Product.Infrastructure.Implementations;

public class RedisCacheService : IRedisCacheService
{
    private readonly IDistributedCache _distributedCache;

    public RedisCacheService(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _distributedCache.GetStringAsync(key);
        if (value is null)
            return default;

        return JsonSerializer.Deserialize<T>(value);
    }

    public async Task SetAsync<T>(string key, T? value, DistributedCacheEntryOptions? options = null)
    {
        var serializedValue = JsonSerializer.Serialize(value);
        await _distributedCache.SetStringAsync(key, serializedValue, options ?? new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        });
    }

    public async Task RemoveAsync(string key)
    {
        await _distributedCache.RemoveAsync(key);
    }
}