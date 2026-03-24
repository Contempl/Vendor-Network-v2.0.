using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Product.Application.ServiceInterfaces;
using Product.Domain.Settings;

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
        
        return JsonSerializer.Deserialize<T>(value, CacheJsonSerializerSettings.Options);
    }

    public async Task SetAsync<T>(string key, T? value, DistributedCacheEntryOptions? options = null)
    {
        var serializedValue = JsonSerializer.Serialize<T>(value, CacheJsonSerializerSettings.Options);
        await SetInCache(key, serializedValue, options);
    }

    public Task RemoveAsync(string key)
    {
        return _distributedCache.RemoveAsync(key);
    }
    
    private Task SetInCache(string key, string serializedValue, DistributedCacheEntryOptions? options = null)
    {
         return _distributedCache.SetStringAsync(key, 
             serializedValue,
             options ?? new DistributedCacheEntryOptions 
             { 
                 AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) 
             });
    }
}

